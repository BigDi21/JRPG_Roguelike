using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Алгоритм генерации лабиринта методом рекурсивного бэктрекинга (DFS).
/// Создаёт идеальный лабиринт без петель. Через параметр LoopChance
/// можно добавить случайные петли.
/// </summary>
public class RecursiveBacktrackingAlgorithm : ILevelAlgorithm
{
    private static readonly Vector2Int[] _directionOffsets =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    /// <inheritdoc />
    public LevelGrid Generate(int width, int height, int seed, AlgorithmParams parameters)
    {
        var rng = new System.Random(seed);

        var grid = new LevelGrid
        {
            Width = width,
            Height = height,
            Cells = new Cell[width, height],
            Exits = new List<Vector2Int>(),
            SpawnPoint = new Vector2Int(1, 1)
        };

        CreateEmptyCells(grid);
        GenerateSpanningTree(grid, rng);

        if (parameters.LoopChance > 0)
        {
            AddLoops(grid, parameters.LoopChance, rng);
        }

        EnsureNoIsolatedCells(grid);

        return grid;
    }

    // ======== СОЗДАНИЕ КЛЕТОК ========

    private void CreateEmptyCells(LevelGrid grid)
    {
        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                grid.Cells[x, y] = new Cell(new Vector2Int(x, y));
            }
        }
    }

    // ======== DFS-ЛАБИРИНТ ========

    private void GenerateSpanningTree(LevelGrid grid, System.Random rng)
    {
        var visited = new bool[grid.Width, grid.Height];
        var stack = new Stack<Vector2Int>();

        var start = new Vector2Int(0, 0);
        visited[start.x, start.y] = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(grid, current, visited);

            if (neighbors.Count > 0)
            {
                Vector2Int next = neighbors[rng.Next(neighbors.Count)];
                ConnectCells(grid, current, next);
                visited[next.x, next.y] = true;
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    private List<Vector2Int> GetUnvisitedNeighbors(LevelGrid grid, Vector2Int pos, bool[,] visited)
    {
        var result = new List<Vector2Int>();

        foreach (Vector2Int dir in _directionOffsets)
        {
            Vector2Int neighbor = pos + dir;

            if (!IsInBounds(grid, neighbor))
            {
                continue;
            }

            if (visited[neighbor.x, neighbor.y])
            {
                continue;
            }

            result.Add(neighbor);
        }

        return result;
    }

    // ======== СОЕДИНЕНИЕ КЛЕТОК ========

    private void ConnectCells(LevelGrid grid, Vector2Int a, Vector2Int b)
    {
        Cell cellA = grid.Cells[a.x, a.y];
        Cell cellB = grid.Cells[b.x, b.y];
        Vector2Int diff = b - a;

        if (diff == Vector2Int.up)
        {
            cellA.AddConnection(Directions.North);
            cellB.AddConnection(Directions.South);
        }
        else if (diff == Vector2Int.down)
        {
            cellA.AddConnection(Directions.South);
            cellB.AddConnection(Directions.North);
        }
        else if (diff == Vector2Int.right)
        {
            cellA.AddConnection(Directions.East);
            cellB.AddConnection(Directions.West);
        }
        else if (diff == Vector2Int.left)
        {
            cellA.AddConnection(Directions.West);
            cellB.AddConnection(Directions.East);
        }
    }

    // ======== ДОБАВЛЕНИЕ ПЕТЕЛЬ ========

    private void AddLoops(LevelGrid grid, int loopChance, System.Random rng)
    {
        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                if (rng.Next(100) >= loopChance)
                {
                    continue;
                }

                var current = new Vector2Int(x, y);
                List<Vector2Int> candidates = GetUnconnectedNeighbors(grid, current);

                if (candidates.Count == 0)
                {
                    continue;
                }

                Vector2Int target = candidates[rng.Next(candidates.Count)];
                ConnectCells(grid, current, target);
            }
        }
    }

    private List<Vector2Int> GetUnconnectedNeighbors(LevelGrid grid, Vector2Int pos)
    {
        var result = new List<Vector2Int>();
        Cell cell = grid.Cells[pos.x, pos.y];

        foreach (Vector2Int dir in _directionOffsets)
        {
            Vector2Int neighbor = pos + dir;

            if (!IsInBounds(grid, neighbor))
            {
                continue;
            }

            if (cell.CanMove(GetDirectionFromOffset(dir)))
            {
                continue;
            }

            result.Add(neighbor);
        }

        return result;
    }

    private Directions GetDirectionFromOffset(Vector2Int offset)
    {
        if (offset == Vector2Int.up)
        {
            return Directions.North;
        }

        if (offset == Vector2Int.down)
        {
            return Directions.South;
        }

        if (offset == Vector2Int.right)
        {
            return Directions.East;
        }

        if (offset == Vector2Int.left)
        {
            return Directions.West;
        }

        return Directions.None;
    }

    // ======== СТРАХОВКА ОТ ИЗОЛЯЦИИ ========

    private void EnsureNoIsolatedCells(LevelGrid grid)
    {
        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                Cell cell = grid.Cells[x, y];

                if (cell.Connections != Directions.None)
                {
                    continue;
                }

                foreach (Vector2Int dir in _directionOffsets)
                {
                    Vector2Int neighbor = new Vector2Int(x, y) + dir;

                    if (!IsInBounds(grid, neighbor))
                    {
                        continue;
                    }

                    ConnectCells(grid, new Vector2Int(x, y), neighbor);
                    break;
                }
            }
        }
    }

    // ======== УТИЛИТЫ ========

    private bool IsInBounds(LevelGrid grid, Vector2Int pos) => pos.x >= 0 && pos.x < grid.Width && pos.y >= 0 && pos.y < grid.Height;
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Алгоритм генерации уровня методом клеточного автомата.
/// Создаёт органические пещеры/поляны, соединённые коридорами.
/// Подходит для Пепельного леса.
/// </summary>
public class CellularAutomataAlgorithm : ILevelAlgorithm
{
    private const int _wallThreshold = 5;
    private const int _floorThreshold = 3;
    private const int _borderThickness = 1;

    private static readonly Vector2Int[] _neighborOffsets =
    {
        new(-1, -1), new(0, -1), new(1, -1),
        new(-1,  0), new(1,  0),
        new(-1,  1), new(0,  1), new(1,  1)
    };

    private bool[,] _map;

    private Vector2Int FindNearestFloor(Vector2Int from, int width, int height)
    {
        for (var radius = 1; radius < Mathf.Max(width, height); radius++)
        {
            for (var dx = -radius; dx <= radius; dx++)
            {
                for (var dy = -radius; dy <= radius; dy++)
                {
                    var x = from.x + dx;
                    var y = from.y + dy;

                    if (x < 0 || x >= width)
                    {
                        continue;
                    }

                    if (y < 0 || y >= height)
                    {
                        continue;
                    }

                    if (_map[x, y])
                    {
                        continue;
                    }

                    return new Vector2Int(x, y);
                }
            }
        }

        return from;
    }

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
            Rooms = new List<RectInt>()
        };

        // 1. Пустые клетки
        CreateEmptyCells(grid);

        // 2. Случайный шум
        FillRandomNoise(width, height, parameters.FillChance, rng);

        // 3. Итерации сглаживания
        for (var i = 0; i < parameters.Iterations; i++)
        {
            ApplyAutomataRule(width, height);
        }

        // 4. Гарантируем рамку из стен
        EnsureBorders(width, height);

        // 5. Разбиваем слишком большие области
        SplitLargeRegions(width, height, rng);

        // 6. Находим связные области пола
        List<List<Vector2Int>> regions = FindRegions(width, height);

        // 7. Соединяем все области
        ConnectRegions(regions, rng);

        // 8. Строим соединения между клетками
        BuildConnections(grid);

        // 9. Спавн — центр первой области
        grid.SpawnPoint = regions.Count > 0
     ? GetRegionCenter(regions[0])
     : new Vector2Int(width / 2, height / 2);

        // Защита: если спавн не на полу — ищем любую проходимую клетку
        if (_map[grid.SpawnPoint.x, grid.SpawnPoint.y])
        {
            Debug.LogWarning("[CA] Спавн попал в стену, ищу ближайшую проходимую клетку");
            grid.SpawnPoint = FindNearestFloor(grid.SpawnPoint, width, height);
        }

        // 10. Выходы по краям карты
        AddDefaultExits(grid);

        Debug.Log($"[CA] Областей: {regions.Count}, " +
                  $"пол: {CountFloor(width, height) * 100 / (width * height)}%");

        return grid;
    }

    // ======== ПОДГОТОВКА ========

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

    private void FillRandomNoise(int width, int height, int fillChance, System.Random rng)
    {
        _map = new bool[width, height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                // true = стена, false = пол
                _map[x, y] = rng.Next(100) < fillChance;
            }
        }
    }

    private void ApplyAutomataRule(int width, int height)
    {
        var newMap = new bool[width, height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var wallNeighbors = CountWallNeighbors(x, y, width, height);

                if (wallNeighbors >= _wallThreshold)
                {
                    newMap[x, y] = true;
                }
                else if (wallNeighbors <= _floorThreshold)
                {
                    newMap[x, y] = false;
                }
                else
                {
                    newMap[x, y] = _map[x, y];
                }
            }
        }

        _map = newMap;
    }

    private int CountWallNeighbors(int x, int y, int width, int height)
    {
        var count = 0;

        foreach (Vector2Int offset in _neighborOffsets)
        {
            var nx = x + offset.x;
            var ny = y + offset.y;

            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
            {
                count++;
                continue;
            }

            if (_map[nx, ny])
            {
                count++;
            }
        }

        return count;
    }

    private void EnsureBorders(int width, int height)
    {
        for (var t = 0; t < _borderThickness; t++)
        {
            for (var x = 0; x < width; x++)
            {
                _map[x, t] = true;
                _map[x, height - 1 - t] = true;
            }

            for (var y = 0; y < height; y++)
            {
                _map[t, y] = true;
                _map[width - 1 - t, y] = true;
            }
        }
    }

    // ======== РАЗБИЕНИЕ БОЛЬШИХ ОБЛАСТЕЙ ========

    private void SplitLargeRegions(int width, int height, System.Random rng)
    {
        List<List<Vector2Int>> regions = FindRegions(width, height);

        foreach (List<Vector2Int> region in regions)
        {
            if (region.Count < 150)
            {
                continue;
            }

            // Оставляем несколько случайных "пятен" стен внутри области
            var splitters = Mathf.Max(2, region.Count / 100);

            for (var i = 0; i < splitters; i++)
            {
                Vector2Int seedCell = region[rng.Next(region.Count)];
                ScatterWallBlob(seedCell, rng, 2);
            }
        }
    }

    private void ScatterWallBlob(Vector2Int center, System.Random rng, int radius)
    {
        for (var dx = -radius; dx <= radius; dx++)
        {
            for (var dy = -radius; dy <= radius; dy++)
            {
                if (rng.Next(100) < 60)
                {
                    continue;
                }

                var x = center.x + dx;
                var y = center.y + dy;

                if (x <= 0 || x >= _map.GetLength(0) - 1)
                {
                    continue;
                }

                if (y <= 0 || y >= _map.GetLength(1) - 1)
                {
                    continue;
                }

                _map[x, y] = true;
            }
        }
    }

    // ======== ПОИСК ОБЛАСТЕЙ ========

    private List<List<Vector2Int>> FindRegions(int width, int height)
    {
        var visited = new bool[width, height];
        var regions = new List<List<Vector2Int>>();

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (_map[x, y])
                {
                    continue;
                }

                if (visited[x, y])
                {
                    continue;
                }

                List<Vector2Int> region = FloodFill(x, y, width, height, visited);
                regions.Add(region);
            }
        }

        // Сортируем по размеру — большие области вперёд
        regions.Sort((a, b) => b.Count.CompareTo(a.Count));

        return regions;
    }

    private List<Vector2Int> FloodFill(int startX, int startY, int width, int height, bool[,] visited)
    {
        var region = new List<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            region.Add(current);

            foreach (Vector2Int dir in dirs)
            {
                var nx = current.x + dir.x;
                var ny = current.y + dir.y;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                {
                    continue;
                }

                if (visited[nx, ny])
                {
                    continue;
                }

                if (_map[nx, ny])
                {
                    continue;
                }

                visited[nx, ny] = true;
                queue.Enqueue(new Vector2Int(nx, ny));
            }
        }

        return region;
    }

    // ======== СОЕДИНЕНИЕ ОБЛАСТЕЙ ========

    private void ConnectRegions(List<List<Vector2Int>> regions, System.Random rng)
    {
        if (regions.Count < 2)
        {
            return;
        }

        // Все области соединяем с самой большой — это гарантирует связность
        List<Vector2Int> mainRegion = regions[0];

        for (var i = 1; i < regions.Count; i++)
        {
            Vector2Int from = regions[i][regions[i].Count / 2];
            Vector2Int to = FindClosestInRegion(mainRegion, from);

            CarveTunnel(from, to, rng);
        }
    }

    private Vector2Int FindClosestInRegion(List<Vector2Int> region, Vector2Int from)
    {
        Vector2Int best = region[0];
        var bestDist = float.MaxValue;

        foreach (Vector2Int cell in region)
        {
            var d = Vector2Int.Distance(cell, from);
            if (d < bestDist)
            {
                bestDist = d;
                best = cell;
            }
        }

        return best;
    }

    private void CarveTunnel(Vector2Int from, Vector2Int to, System.Random rng)
    {
        if (rng is null)
        {
            throw new System.ArgumentNullException(nameof(rng));
        }

        Vector2Int current = from;

        while (current != to)
        {
            _map[current.x, current.y] = false;

            var dx = to.x - current.x;
            var dy = to.y - current.y;

            if (Mathf.Abs(dx) > Mathf.Abs(dy))
            {
                current.x += dx > 0 ? 1 : -1;
            }
            else
            {
                current.y += dy > 0 ? 1 : -1;
            }
        }
    }

    // ======== ПОСТРОЕНИЕ СОЕДИНЕНИЙ ========

    private void BuildConnections(LevelGrid grid)
    {
        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                if (_map[x, y])
                {
                    continue;
                }

                Cell cell = grid.Cells[x, y];

                if (IsFloor(x, y + 1))
                {
                    cell.AddConnection(Directions.North);
                }

                if (IsFloor(x + 1, y))
                {
                    cell.AddConnection(Directions.East);
                }

                if (IsFloor(x, y - 1))
                {
                    cell.AddConnection(Directions.South);
                }

                if (IsFloor(x - 1, y))
                {
                    cell.AddConnection(Directions.West);
                }
            }
        }
    }

    private bool IsFloor(int x, int y)
    {
        if (x < 0 || x >= _map.GetLength(0))
        {
            return false;
        }

        if (y < 0 || y >= _map.GetLength(1))
        {
            return false;
        }

        return !_map[x, y];
    }

    // ======== УТИЛИТЫ ========

    /// <summary>
    /// Возвращает клетку в области, ближайшую к её геометрическому центру.
    /// Гарантирует, что клетка — часть области (проходимая).
    /// </summary>
    private Vector2Int GetRegionCenter(List<Vector2Int> region)
    {
        if (region == null || region.Count == 0)
        {
            return Vector2Int.zero;
        }

        // Геометрический центр
        Vector2Int sum = Vector2Int.zero;
        foreach (Vector2Int cell in region)
        {
            sum += cell;
        }

        Vector2Int geometricCenter = sum / region.Count;

        // Ищем ближайшую клетку области к этому центру
        Vector2Int best = region[0];
        var bestDist = float.MaxValue;

        foreach (Vector2Int cell in region)
        {
            var d = Vector2Int.Distance(cell, geometricCenter);
            if (d < bestDist)
            {
                bestDist = d;
                best = cell;
            }
        }

        return best;
    }

    private int CountFloor(int width, int height)
    {
        var count = 0;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (!_map[x, y])
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void AddDefaultExits(LevelGrid grid)
    {
        grid.Exits.Add(new Vector2Int(0, grid.Height / 2));
        grid.Exits.Add(new Vector2Int(0, grid.Height / 2 + 1));
        grid.Exits.Add(new Vector2Int(grid.Width / 2, grid.Height - 1));
        grid.Exits.Add(new Vector2Int(grid.Width / 2 + 1, grid.Height - 1));
    }
}

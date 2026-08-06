using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Размеры сетки")]
    public int Width = 20;
    public int Height = 20;

    [Header("Префабы для визуализации (опционально)")]
    public GameObject cellPrefab; // для создания визуальных объектов ячеек
    public GameObject startMarker;
    public GameObject finishMarker;

    public Cell[,] Grid { get; private set; }
    public Vector2Int StartPosition { get; private set; }
    public Vector2Int FinishPosition { get; private set; }

    private Dictionary<Vector2Int, Cell> _cellMap;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GenerateGrid(Width, Height);
        PlaceStartAndFinish();
        CreateVisuals();
    }

    // ======== ГЕНЕРАЦИЯ ЛАБИРИНТА (DFS) ========
    public void GenerateGrid(int width, int height)
    {
        Width = width;
        Height = height;
        Grid = new Cell[Width, Height];
        _cellMap = new Dictionary<Vector2Int, Cell>();

        // Инициализация ячеек
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var pos = new Vector2Int(x, y);
                var cell = new Cell(pos);
                Grid[x, y] = cell;
                _cellMap[pos] = cell;
            }
        }

        // Запуск алгоритма DFS с (0,0)
        var visited = new bool[Width, Height];
        var stack = new Stack<Vector2Int>();
        Vector2Int start = new Vector2Int(0, 0);
        visited[start.x, start.y] = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            var neighbors = GetUnvisitedNeighbors(current, visited);

            if (neighbors.Count > 0)
            {
                Vector2Int next = neighbors[Random.Range(0, neighbors.Count)];
                RemoveWall(current, next);
                visited[next.x, next.y] = true;
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }

        // Убедимся, что нет изолированных ячеек (хотя алгоритм гарантирует это)
        // Дополнительно можно проверить и принудительно соединить
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (Grid[x, y].Connections == Directions.None && (x != 0 || y != 0))
                {
                    // Принудительно соединяем с соседом (чтобы избежать None)
                    ForceConnect(Grid[x, y]);
                }
            }
        }
    }

    // ======== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ========
    private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int pos, bool[,] visited)
    {
        var result = new List<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        foreach (var dir in dirs)
        {
            Vector2Int neighbor = pos + dir;
            if (IsInBounds(neighbor) && !visited[neighbor.x, neighbor.y])
                result.Add(neighbor);
        }
        return result;
    }

    private void RemoveWall(Vector2Int a, Vector2Int b)
    {
        var cellA = Grid[a.x, a.y];
        var cellB = Grid[b.x, b.y];

        Vector2Int diff = b - a;
        if (diff == Vector2Int.up)      // b находится севернее
        {
            cellA.AddConnection(Directions.North);
            cellB.AddConnection(Directions.South);
        }
        else if (diff == Vector2Int.right) // b восточнее
        {
            cellA.AddConnection(Directions.East);
            cellB.AddConnection(Directions.West);
        }
        else if (diff == Vector2Int.down)   // b южнее
        {
            cellA.AddConnection(Directions.South);
            cellB.AddConnection(Directions.North);
        }
        else if (diff == Vector2Int.left)   // b западнее
        {
            cellA.AddConnection(Directions.West);
            cellB.AddConnection(Directions.East);
        }
    }

    private void ForceConnect(Cell cell)
    {
        var pos = cell.Position;
        var neighbors = new List<Vector2Int>
        {
            pos + Vector2Int.up,
            pos + Vector2Int.right,
            pos + Vector2Int.down,
            pos + Vector2Int.left
        };

        foreach (var nPos in neighbors)
        {
            if (IsInBounds(nPos))
            {
                RemoveWall(pos, nPos);
                return;
            }
        }
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;
    }

    // ======== СТАРТ И ФИНИШ ========
    private void PlaceStartAndFinish()
    {
        StartPosition = new Vector2Int(0, 0);
        FinishPosition = new Vector2Int(Width - 1, Height - 1);

        // Можно пометить визуально через маркеры (см. CreateVisuals)
    }

    // ======== ВИЗУАЛИЗАЦИЯ (опционально) ========
    private void CreateVisuals()
    {
        // Если есть префаб ячейки, создаём визуальные объекты
        if (cellPrefab != null)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pos = new Vector3(x, y, 0);
                    var go = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                    var visual = go.GetComponent<CellVisual>();
                    if (visual != null)
                        visual.Initialize(Grid[x, y]);
                }
            }
        }

        // Маркеры старта и финиша
        if (startMarker != null)
            Instantiate(startMarker, new Vector3(StartPosition.x, StartPosition.y, 0), Quaternion.identity);

        if (finishMarker != null)
            Instantiate(finishMarker, new Vector3(FinishPosition.x, FinishPosition.y, 0), Quaternion.identity);
    }

    // ======== ДОСТУП К ЯЧЕЙКАМ ========
    public Cell GetCell(Vector2Int pos)
    {
        if (_cellMap.TryGetValue(pos, out var cell))
            return cell;
        return null;
    }

    public Cell GetCell(int x, int y) => GetCell(new Vector2Int(x, y));
}
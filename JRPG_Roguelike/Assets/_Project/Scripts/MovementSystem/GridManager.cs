using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет сеткой уровня: генерацией лабиринта, хранением клеток, визуализацией и туманом войны.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Размеры сетки")]
    [SerializeField] private int _width = 20;
    [SerializeField] private int _height = 20;

    [Header("Размер ячейки")]
    [SerializeField] private float _cellSize = 10f;

    [Header("Префабы")]
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private GameObject _startMarker;
    [SerializeField] private GameObject _finishMarker;

    private Cell[,] _grid;
    private Dictionary<Vector2Int, Cell> _cellMap;

    /// <summary>Ширина сетки в клетках.</summary>
    public int Width => _width;

    /// <summary>Высота сетки в клетках.</summary>
    public int Height => _height;

    /// <summary>Размер одной клетки в мировых единицах.</summary>
    public float CellSize => _cellSize;

    /// <summary>Двумерный массив клеток.</summary>
    public Cell[,] Grid => _grid;

    /// <summary>Стартовая позиция игрока на сетке.</summary>
    public Vector2Int StartPosition { get; private set; }

    /// <summary>Позиция выхода (финиша) на сетке.</summary>
    public Vector2Int FinishPosition { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        GenerateGrid(_width, _height);
        PlaceStartAndFinish();
    }

    private void Start()
    {
        CreateVisuals();
    }

    // ======== ГЕНЕРАЦИЯ ========

    /// <summary>
    /// Генерирует новую сетку указанного размера с помощью DFS-лабиринта.
    /// </summary>
    /// <param name="width">Ширина сетки.</param>
    /// <param name="height">Высота сетки.</param>
    public void GenerateGrid(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new Cell[_width, _height];
        _cellMap = new Dictionary<Vector2Int, Cell>();

        CreateEmptyCells();
        GenerateMaze();
        EnsureNoIsolatedCells();
    }

    private void CreateEmptyCells()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var pos = new Vector2Int(x, y);
                var cell = new Cell(pos);
                _grid[x, y] = cell;
                _cellMap[pos] = cell;
            }
        }
    }

    private void GenerateMaze()
    {
        var visited = new bool[_width, _height];
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
    }

    private void EnsureNoIsolatedCells()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y].Connections == Directions.None && (x != 0 || y != 0))
                {
                    ForceConnect(_grid[x, y]);
                }
            }
        }
    }

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
        var cellA = _grid[a.x, a.y];
        var cellB = _grid[b.x, b.y];
        Vector2Int diff = b - a;

        if (diff == Vector2Int.up)
        {
            cellA.AddConnection(Directions.North);
            cellB.AddConnection(Directions.South);
        }
        else if (diff == Vector2Int.right)
        {
            cellA.AddConnection(Directions.East);
            cellB.AddConnection(Directions.West);
        }
        else if (diff == Vector2Int.down)
        {
            cellA.AddConnection(Directions.South);
            cellB.AddConnection(Directions.North);
        }
        else if (diff == Vector2Int.left)
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

        foreach (var neighborPos in neighbors)
        {
            if (IsInBounds(neighborPos))
            {
                RemoveWall(pos, neighborPos);
                return;
            }
        }
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
    }

    private void PlaceStartAndFinish()
    {
        StartPosition = new Vector2Int(_width - 1, 0);
        FinishPosition = new Vector2Int(0, _height - 1);
    }

    // ======== ВИЗУАЛИЗАЦИЯ ========

    private void CreateVisuals()
    {
        if (_cellPrefab == null) return;

        float offsetX = (_width - 1) * _cellSize * 0.5f;
        float offsetZ = (_height - 1) * _cellSize * 0.5f;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector3 pos = new Vector3(x * _cellSize - offsetX, 0, y * _cellSize - offsetZ);
                var go = Instantiate(_cellPrefab, pos, Quaternion.identity, transform);
                var visual = go.GetComponent<CellVisual>();

                if (visual != null)
                    visual.Initialize(_grid[x, y]);
            }
        }

        SpawnMarker(_startMarker, StartPosition, offsetX, offsetZ);
        SpawnMarker(_finishMarker, FinishPosition, offsetX, offsetZ);
    }

    private void SpawnMarker(GameObject marker, Vector2Int gridPos, float offsetX, float offsetZ)
    {
        if (marker == null) return;

        Vector3 pos = new Vector3(gridPos.x * _cellSize - offsetX, 0, gridPos.y * _cellSize - offsetZ);
        Instantiate(marker, pos, Quaternion.identity);
    }

    // ======== ПУБЛИЧНЫЙ API ========

    /// <summary>
    /// Возвращает клетку по указанной позиции. null, если позиция вне сетки.
    /// </summary>
    public Cell GetCell(Vector2Int pos)
    {
        return _cellMap.TryGetValue(pos, out var cell) ? cell : null;
    }

    /// <summary>
    /// Возвращает клетку по координатам X и Y.
    /// </summary>
    public Cell GetCell(int x, int y)
    {
        return GetCell(new Vector2Int(x, y));
    }

    /// <summary>
    /// Возвращает мировую позицию для указанной клетки.
    /// </summary>
    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        float offsetX = (_width - 1) * _cellSize * 0.5f;
        float offsetZ = (_height - 1) * _cellSize * 0.5f;
        return new Vector3(gridPos.x * _cellSize - offsetX, 0, gridPos.y * _cellSize - offsetZ);
    }

    /// <summary>
    /// Открывает область вокруг стартовой позиции в указанном направлении.
    /// </summary>
    /// <param name="start">Стартовая позиция.</param>
    /// <param name="direction">Направление обзора.</param>
    /// <param name="range">Радиус обзора в клетках.</param>
    public void RevealArea(Vector2Int start, Directions direction, int range = 3)
    {
        Vector2Int current = start;
        GetCell(current)?.Discover();

        for (int i = 0; i < range; i++)
        {
            Vector2Int next = current + GetOffset(direction);
            if (!IsInBounds(next)) break;

            Cell nextCell = GetCell(next);
            if (nextCell == null) break;

            if (!GetCell(current).CanMove(direction)) break;

            nextCell.Discover();
            current = next;
        }
    }

    /// <summary>
    /// Возвращает все клетки сетки.
    /// </summary>
    public IEnumerable<Cell> GetAllCells()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                yield return _grid[x, y];
            }
        }
    }

    // ======== ВСПОМОГАТЕЛЬНЫЕ ========

    private Vector2Int GetOffset(Directions dir)
    {
        switch (dir)
        {
            case Directions.North: return Vector2Int.up;
            case Directions.South: return Vector2Int.down;
            case Directions.East: return Vector2Int.right;
            case Directions.West: return Vector2Int.left;
            default: return Vector2Int.zero;
        }
    }
}
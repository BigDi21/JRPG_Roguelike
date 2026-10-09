using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет сеткой уровня: генерацией через LevelGenerator,
/// хранением клеток, визуализацией и туманом войны.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Размеры сетки")]
    [SerializeField] private int _width = 20;
    [SerializeField] private int _height = 20;

    [Header("Размер ячейки")]
    [SerializeField] private float _cellSize = 10f;

    [Header("Генерация")]
    [SerializeField] private FlameDragonLevelGenerator _levelGenerator;

    [Header("Параметры лабиринта")]
    [Range(0, 100)]
    [Tooltip("Шанс появления петли (0–100%). 0 — идеальный лабиринт без петель, 100 — максимум петель.")]
    [SerializeField] private int _loopChance = 0;

    [Header("Seed")]
    [Tooltip("Если включено — используется фиксированный seed (для тестов и отладки).")]
    [SerializeField] private bool _useFixedSeed = false;
    [Tooltip("Значение seed. Работает только если Use Fixed Seed включён.")]
    [SerializeField] private int _fixedSeed = 12345;

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

    /// <summary>Текущий шанс появления петли (0–100%).</summary>
    public int LoopChance => _loopChance;

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

        if (_levelGenerator == null)
        {
            Debug.LogError("[GridManager] LevelGenerator не назначен в инспекторе!");
            return;
        }

        GenerateGrid(_width, _height);
    }

    private void Start() => CreateVisuals();

    // ======== ГЕНЕРАЦИЯ ========

    /// <summary>
    /// Генерирует новую сетку указанного размера с текущими параметрами.
    /// </summary>
    public void GenerateGrid(int width, int height)
    {
        var seed = _useFixedSeed ? _fixedSeed : Random.Range(int.MinValue, int.MaxValue);

        AlgorithmParams parameters = BuildAlgorithmParams();

        LevelGrid levelGrid = _levelGenerator.Generate(
            width,
            height,
            seed,
            GenerationAlgorithm.RecursiveBacktracking,
            parameters);

        if (levelGrid.Cells == null)
        {
            Debug.LogError("[GridManager] Генерация вернула пустой результат!");
            return;
        }

        _width = levelGrid.Width;
        _height = levelGrid.Height;
        _grid = levelGrid.Cells;
        StartPosition = levelGrid.SpawnPoint;
        FinishPosition = new Vector2Int(_width - 1, _height - 1);

        RebuildCellMap();

        Debug.Log($"[GridManager] Сетка {_width}×{_height} (seed={seed}, loop={_loopChance}%)");
    }

    /// <summary>
    /// Собирает параметры алгоритма из полей инспектора.
    /// </summary>
    private AlgorithmParams BuildAlgorithmParams()
    {
        return new AlgorithmParams
        {
            LoopChance = _loopChance,
            MinRoomSize = 0,
            MaxRoomSize = 0,
            MinRooms = 0,
            MaxRooms = 0
        };
    }

    private void RebuildCellMap()
    {
        _cellMap = new Dictionary<Vector2Int, Cell>();

        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                _cellMap[new Vector2Int(x, y)] = _grid[x, y];
            }
        }
    }

    // ======== ПУБЛИЧНЫЕ СЕТТЕРЫ (для смены локации) ========

    /// <summary>
    /// Устанавливает шанс появления петли. Значение 0–100.
    /// Не забудь вызвать GenerateGrid() после смены.
    /// </summary>
    public void SetLoopChance(int value) => _loopChance = Mathf.Clamp(value, 0, 100);

    /// <summary>
    /// Устанавливает фиксированный seed для воспроизводимой генерации.
    /// </summary>
    public void SetFixedSeed(int seed)
    {
        _useFixedSeed = true;
        _fixedSeed = seed;
    }

    /// <summary>
    /// Отключает фиксированный seed — генерация будет случайной.
    /// </summary>
    public void UseRandomSeed() => _useFixedSeed = false;

    // ======== ВИЗУАЛИЗАЦИЯ ========

    private void CreateVisuals()
    {
        if (_cellPrefab == null || _grid == null)
        {
            return;
        }

        var offsetX = (_width - 1) * _cellSize * 0.5f;
        var offsetZ = (_height - 1) * _cellSize * 0.5f;

        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                var pos = new Vector3(x * _cellSize - offsetX, 0, y * _cellSize - offsetZ);
                GameObject go = Instantiate(_cellPrefab, pos, Quaternion.identity, transform);
                
                if (go.TryGetComponent<CellVisual>(out CellVisual visual))
                {
                    visual.Initialize(_grid[x, y]);
                }
            }
        }

        SpawnMarker(_startMarker, StartPosition, offsetX, offsetZ);
        SpawnMarker(_finishMarker, FinishPosition, offsetX, offsetZ);
    }

    private void SpawnMarker(GameObject marker, Vector2Int gridPos, float offsetX, float offsetZ)
    {
        if (marker == null)
        {
            return;
        }

        var pos = new Vector3(gridPos.x * _cellSize - offsetX, 0, gridPos.y * _cellSize - offsetZ);
        Instantiate(marker, pos, Quaternion.identity);
    }

    // ======== ПУБЛИЧНЫЙ API ========

    /// <summary>
    /// Возвращает клетку по указанной позиции. null, если позиция вне сетки.
    /// </summary>
    public Cell GetCell(Vector2Int pos)
    {
        if (_cellMap == null)
        {
            return null;
        }

        return _cellMap.TryGetValue(pos, out Cell cell) ? cell : null;
    }

    /// <summary>
    /// Возвращает клетку по координатам X и Y.
    /// </summary>
    public Cell GetCell(int x, int y) => GetCell(new Vector2Int(x, y));

    /// <summary>
    /// Возвращает мировую позицию для указанной клетки.
    /// </summary>
    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        var offsetX = (_width - 1) * _cellSize * 0.5f;
        var offsetZ = (_height - 1) * _cellSize * 0.5f;
        return new Vector3(gridPos.x * _cellSize - offsetX, 0, gridPos.y * _cellSize - offsetZ);
    }

    /// <summary>
    /// Открывает область вокруг стартовой позиции в указанном направлении.
    /// </summary>
    public void RevealArea(Vector2Int start, Directions direction, int range = 3)
    {
        Vector2Int current = start;
        GetCell(current)?.Discover();

        for (var i = 0; i < range; i++)
        {
            Vector2Int next = current + GetOffset(direction);
            if (!IsInBounds(next))
            {
                break;
            }

            Cell nextCell = GetCell(next);
            if (nextCell == null)
            {
                break;
            }

            if (!GetCell(current).CanMove(direction))
            {
                break;
            }

            nextCell.Discover();
            current = next;
        }
    }

    /// <summary>
    /// Возвращает все клетки сетки.
    /// </summary>
    public IEnumerable<Cell> GetAllCells()
    {
        if (_grid == null)
        {
            yield break;
        }

        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                yield return _grid[x, y];
            }
        }
    }

    // ======== ВСПОМОГАТЕЛЬНЫЕ ========

    private Vector2Int GetOffset(Directions dir)
    {
        return dir switch
        {
            Directions.North => Vector2Int.up,
            Directions.South => Vector2Int.down,
            Directions.East => Vector2Int.right,
            Directions.West => Vector2Int.left,
            _ => Vector2Int.zero,
        };
    }

    private bool IsInBounds(Vector2Int pos) => pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
}

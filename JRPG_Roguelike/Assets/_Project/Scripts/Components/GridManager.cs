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
    [SerializeField] private int _width = 60;
    [SerializeField] private int _height = 60;

    [Header("Размер ячейки")]
    [SerializeField] private float _cellSize = 10f;

    [Header("Генерация")]
    [SerializeField] private FlameDragonLevelGenerator _levelGenerator;
    [Tooltip("Какой алгоритм использовать для генерации.")]
    [SerializeField] private GenerationAlgorithm _algorithm = GenerationAlgorithm.RecursiveBacktracking;


    [Header("Параметры лабиринта")]
    [Range(0, 100)]
    [Tooltip("Шанс появления петли (0–100%). 0 — идеальный лабиринт без петель, 100 — максимум петель.")]
    [SerializeField] private int _loopChance = 0;

    [Header("BSP — параметры Кладбища")]
    [Tooltip("Минимальный размер комнаты в клетках.")]
    [SerializeField] private int _bspMinRoomSize = 4;
    [Tooltip("Максимальный размер комнаты в клетках.")]
    [SerializeField] private int _bspMaxRoomSize = 6;
    [Tooltip("Минимальное количество комнат.")]
    [SerializeField] private int _bspMinRooms = 12;
    [Tooltip("Максимальное количество комнат.")]
    [SerializeField] private int _bspMaxRooms = 18;

    [Header("CA — параметры Пепельного леса")]
    [Range(30, 60)]
    [Tooltip("Процент начального заполнения стенами (0–100).")]
    [SerializeField] private int _caFillChance = 46;

    [Range(1, 10)]
    [Tooltip("Количество итераций сглаживания.")]
    [SerializeField] private int _caIterations = 4;

    [Range(5, 100)]
    [Tooltip("Минимальный размер связной области пола.")]
    [SerializeField] private int _caMinRegionSize = 20;


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
    private List<RectInt> _rooms = new();

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

    /// <summary>Seed, использованный для текущей генерации.</summary>
    public int CurrentSeed { get; private set; }

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

    /// <summary>
    /// Ищет ближайшую проходимую клетку к указанной позиции.
    /// Сканирует по расширяющемуся радиусу.
    /// </summary>
    private Vector2Int FindNearestWalkable(Vector2Int target)
    {
        if (GetCell(target) != null && GetCell(target).Connections != Directions.None)
        {
            return target;
        }

        for (var radius = 1; radius < Mathf.Max(_width, _height); radius++)
        {
            for (var dx = -radius; dx <= radius; dx++)
            {
                for (var dy = -radius; dy <= radius; dy++)
                {
                    var candidate = new Vector2Int(target.x + dx, target.y + dy);

                    if (candidate.x < 0 || candidate.x >= _width)
                    {
                        continue;
                    }

                    if (candidate.y < 0 || candidate.y >= _height)
                    {
                        continue;
                    }

                    Cell cell = GetCell(candidate);
                    if (cell != null && cell.Connections != Directions.None)
                    {
                        return candidate;
                    }
                }
            }
        }

        Debug.LogWarning("[GridManager] Не удалось найти проходимую клетку для финиша!");
        return target;
    }

    // ======== ГЕНЕРАЦИЯ ========

    /// <summary>
    /// Генерирует новую сетку указанного размера с текущими параметрами.
    /// </summary>
    public void GenerateGrid(int width, int height)
    {
        var seed = _useFixedSeed ? _fixedSeed : Random.Range(int.MinValue, int.MaxValue);
        CurrentSeed = seed;

        Random.InitState(seed);

        AlgorithmParams parameters = BuildAlgorithmParams();

        LevelGrid levelGrid = _levelGenerator.Generate(
            width,
            height,
            seed,
            _algorithm,
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
        FinishPosition = FindNearestWalkable(new Vector2Int(_width - 1, _height - 1));

        _rooms = levelGrid.Rooms ?? new List<RectInt>();

        RebuildCellMap();

        Debug.Log($"[GridManager] Комнат: {_rooms.Count}");

        Debug.Log($"[GridManager] {_algorithm} {_width}×{_height} (seed={seed}, loop={_loopChance}%)");
    }


    /// <summary>
    /// Собирает параметры алгоритма из полей инспектора.
    /// </summary>
    private AlgorithmParams BuildAlgorithmParams()
    {
        return new AlgorithmParams
        {
            LoopChance = _loopChance,
            MinRoomSize = _bspMinRoomSize,
            MaxRoomSize = _bspMaxRoomSize,
            MinRooms = _bspMinRooms,
            MaxRooms = _bspMaxRooms,
            FillChance = _caFillChance,
            Iterations = _caIterations,
            MinRegionSize = _caMinRegionSize
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

        var spawnedCount = 0;

        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                Cell cell = _grid[x, y];

                // Пропускаем «мёртвые» клетки — те, в которые нельзя попасть
                if (cell.Connections == Directions.None)
                {
                    continue;
                }

                var pos = new Vector3(x * _cellSize - offsetX, 0, y * _cellSize - offsetZ);
                GameObject go = Instantiate(_cellPrefab, pos, Quaternion.identity, transform);
                
                if (go.TryGetComponent<CellVisual>(out CellVisual visual))
                {
                    visual.Initialize(cell);
                }

                spawnedCount++;
            }
        }

        Debug.Log($"[GridManager] Заспавнено клеток: {spawnedCount}");

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
    public void RevealArea(Vector2Int start, Directions direction)
    {
        // 1. Открываем стартовую клетку
        GetCell(start)?.Discover();

        // 2. Если игрок в комнате — открываем всю комнату
        RevealCurrentRoom(start);

        // 3. Открываем область видимости (кайт) в направлении взгляда
        RevealVisionShape(start, direction);
    }

    /// <summary>
    /// Открывает клетки в форме «кайт» в направлении взгляда.
    /// Форма: 4 клетки вперёд, 2 назад, 3 влево/вправо на самом широком ряду.
    /// </summary>
    private void RevealVisionShape(Vector2Int center, Directions facing)
    {
        Vector2Int forwardDir = GetOffset(facing);
        Vector2Int rightDir = GetRightVector(facing);

        // halfWidth для каждого forward offset.
        // Индекс = forward + 2 (диапазон forward: -2 … +4).
        // forward = -2 → 1 (3 клетки)
        // forward = -1 → 2 (5 клеток)
        // forward =  0 → 3 (7 клеток) — ряд игрока
        // forward = +1 → 3 (7 клеток)
        // forward = +2 → 2 (5 клеток)
        // forward = +3 → 1 (3 клетки)
        // forward = +4 → 0 (1 клетка) — острие
        int[] halfWidths = { 1, 2, 3, 3, 2, 1, 0 };
        const int minForward = -2;
        const int maxForward = 4;

        for (var fwd = minForward; fwd <= maxForward; fwd++)
        {
            var halfWidth = halfWidths[fwd - minForward];

            for (var side = -halfWidth; side <= halfWidth; side++)
            {
                Vector2Int pos = center + forwardDir * fwd + rightDir * side;
                if (!IsInBounds(pos))
                {
                    continue;
                }

                Cell cell = GetCell(pos);
                if (cell == null)
                {
                    continue;
                }

                // Пропускаем клетки-стены (актуально для BSP и CA)
                if (cell.Connections == Directions.None)
                {
                    continue;
                }

                if (!HasLineOfSight(center, pos))
                {
                    continue;
                }

                cell.Discover();
            }
        }
    }

    /// <summary>
    /// Проверяет, есть ли прямая линия видимости между двумя клетками.
    /// Использует алгоритм Брезенхэма. Стены (клетки без соединений) блокируют обзор.
    /// </summary>
    /// <param name="from">Начальная клетка (позиция игрока).</param>
    /// <param name="to">Целевая клетка.</param>
    /// <returns>True, если между клетками нет стен.</returns>
    /// <summary>
    /// Проверяет, есть ли прямая линия видимости между двумя клетками.
    /// Работает через соединения клеток, а не через тип клетки.
    /// Подходит для всех алгоритмов: DFS, BSP, CA.
    /// </summary>
    private bool HasLineOfSight(Vector2Int from, Vector2Int to)
    {
        if (from == to)
        {
            return true;
        }

        int x0 = from.x, y0 = from.y;
        int x1 = to.x, y1 = to.y;

        var dx = Mathf.Abs(x1 - x0);
        var dy = Mathf.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx - dy;

        var prevX = x0;
        var prevY = y0;

        while (true)
        {
            var e2 = 2 * err;
            if (e2 > -dy)
            { err -= dy; x0 += sx; }
            if (e2 < dx)
            { err += dx; y0 += sy; }

            // Проверяем переход между предыдущей и текущей клеткой
            if (!CanWalkBetween(prevX, prevY, x0, y0))
            {
                return false;
            }

            if (x0 == x1 && y0 == y1)
            {
                return true;
            }

            prevX = x0;
            prevY = y0;
        }
    }

    /// <summary>
    /// Проверяет, можно ли пройти из клетки (fromX, fromY) в (toX, toY).
    /// Учитывает 4-направленные соединения и диагональные шаги.
    /// </summary>
    private bool CanWalkBetween(int fromX, int fromY, int toX, int toY)
    {
        Cell from = GetCell(new Vector2Int(fromX, fromY));
        if (from == null)
        {
            return false;
        }

        var dx = toX - fromX;
        var dy = toY - fromY;

        // Прямые шаги
        if (dx == 0 && dy == 1)
        {
            return from.CanMove(Directions.North);
        }

        if (dx == 0 && dy == -1)
        {
            return from.CanMove(Directions.South);
        }

        if (dx == 1 && dy == 0)
        {
            return from.CanMove(Directions.East);
        }

        if (dx == -1 && dy == 0)
        {
            return from.CanMove(Directions.West);
        }

        // Диагональный шаг — проверяем два возможных пути
        Directions hDir = dx > 0 ? Directions.East : Directions.West;
        Directions vDir = dy > 0 ? Directions.North : Directions.South;

        Cell horizontal = GetCell(new Vector2Int(toX, fromY));
        Cell vertical = GetCell(new Vector2Int(fromX, toY));

        // Путь 1: сначала вбок, потом вверх/вниз
        if (horizontal != null && from.CanMove(hDir) && horizontal.CanMove(vDir))
        {
            return true;
        }

        // Путь 2: сначала вверх/вниз, потом вбок
        if (vertical != null && from.CanMove(vDir) && vertical.CanMove(hDir))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Возвращает вектор «вправо» относительно направления взгляда.
    /// Используется для поворота формы обзора.
    /// </summary>
    private Vector2Int GetRightVector(Directions facing)
    {
        return facing switch
        {
            Directions.North => Vector2Int.right,
            Directions.East => Vector2Int.down,
            Directions.South => Vector2Int.left,
            Directions.West => Vector2Int.up,
            _ => Vector2Int.right,
        };
    }

    /// <summary>
    /// Открывает всю комнату, в которой находится указанная позиция.
    /// Если позиция не в комнате — ничего не делает.
    /// </summary>
    private void RevealCurrentRoom(Vector2Int position)
    {
        if (_rooms == null || _rooms.Count == 0)
        {
            return;
        }

        for (var i = 0; i < _rooms.Count; i++)
        {
            if (ContainsCell(_rooms[i], position))
            {
                RevealRoom(_rooms[i]);
                return;
            }
        }
    }

    /// <summary>
    /// Открывает все клетки внутри прямоугольника комнаты.
    /// </summary>
    private void RevealRoom(RectInt room)
    {
        for (var x = room.x; x < room.x + room.width; x++)
        {
            for (var y = room.y; y < room.y + room.height; y++)
            {
                GetCell(new Vector2Int(x, y))?.Discover();
            }
        }
    }

    /// <summary>
    /// Проверяет, находится ли клетка внутри прямоугольника.
    /// </summary>
    private bool ContainsCell(RectInt rect, Vector2Int pos)
    {
        return pos.x >= rect.x && pos.x < rect.x + rect.width
            && pos.y >= rect.y && pos.y < rect.y + rect.height;
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

    public void RegenerateWithSameSeed()
    {
        ClearVisuals();
        SetFixedSeed(CurrentSeed);
        GenerateGrid(_width, _height);
        CreateVisuals();
    }

    private void ClearVisuals()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private bool IsInBounds(Vector2Int pos) => pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
}

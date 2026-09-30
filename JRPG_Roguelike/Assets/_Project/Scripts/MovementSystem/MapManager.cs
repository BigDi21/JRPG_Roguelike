using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет миникартой, большой картой и туманом войны.
/// </summary>
public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    private const float MouseSensitivityScale = 0.01f;
    private const float TileMatchThreshold = 0.01f;
    private const float MarkerRotationNorth = 180f;
    private const float MarkerRotationEast = 90f;
    private const float MarkerRotationSouth = 0f;
    private const float MarkerRotationWest = 270f;

    [Header("UI References")]
    [SerializeField] private Transform _bigMapContainer;
    [SerializeField] private Transform _bigMapFullContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject _mapTilePrefab;
    [SerializeField] private GameObject _playerMarkerPrefab;

    [Header("Settings")]
    [SerializeField] private int _tileSize = 20;
    [SerializeField] private bool _fogOfWarEnabled = true;

    [Header("Big Map Controls")]
    [SerializeField] private float _panSpeed = 200f;
    [SerializeField] private float _mouseSensitivity = 1f;

    [Header("Big Map UI")]
    [SerializeField] private GameObject _bigMapPanel;

    private GridManager _gridManager;
    private readonly Dictionary<Vector2Int, MapTile> _tiles = new();
    private readonly List<GameObject> _bigMapTiles = new();
    private Vector2Int _playerPosition;
    private GameObject _playerMarkerInstance;
    private GameObject _bigMapPlayerMarker;

    private bool _bigMapOpen;
    private Vector2 _bigMapSize;
    private Vector2 _bigMapOffset;
    private Vector2 _bigMapCenterOffset;

    private bool _isDragging;
    private Vector3 _dragStartMousePos;
    private Vector2 _dragStartContainerPos;

    private RectTransform _containerRect;
    private RectTransform _panelRect;

    /// <summary>
    /// Ленивая инициализация GridManager. Возвращает null, если его нет на сцене.
    /// </summary>
    private GridManager Grid => _gridManager != null ? _gridManager : (_gridManager = GridManager.Instance);

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

        _gridManager = GridManager.Instance;
    }

    private void Start()
    {
        InitializeBigMapContainers();
        UpdateBigMapSize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            ToggleBigMap();

        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleFog();

        if (_bigMapOpen && _containerRect != null)
            HandleBigMapInput();
    }

    // ======== ИНИЦИАЛИЗАЦИЯ ========

    private void InitializeBigMapContainers()
    {
        if (_bigMapFullContainer != null)
        {
            _containerRect = _bigMapFullContainer.GetComponent<RectTransform>();
            _containerRect.pivot = new Vector2(0.5f, 0.5f);
            _containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            _containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            _containerRect.anchoredPosition = Vector2.zero;
        }

        if (_bigMapPanel != null)
            _panelRect = _bigMapPanel.GetComponent<RectTransform>();
    }

    // ======== УПРАВЛЕНИЕ БОЛЬШОЙ КАРТОЙ ========

    private void HandleBigMapInput()
    {
        HandleArrowKeys();
        HandleMouseDragging();
    }

    private void HandleArrowKeys()
    {
        Vector2 move = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow)) move.y += _panSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) move.y -= _panSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow)) move.x -= _panSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) move.x += _panSpeed * Time.deltaTime;

        if (move == Vector2.zero) return;

        _bigMapOffset += move;
        ClampOffset();
        _containerRect.anchoredPosition = _bigMapOffset;
    }

    private void HandleMouseDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _dragStartMousePos = Input.mousePosition;
            _dragStartContainerPos = _bigMapOffset;
        }

        if (Input.GetMouseButton(0) && _isDragging)
        {
            Vector3 delta = Input.mousePosition - _dragStartMousePos;
            Vector2 worldDelta = new Vector2(delta.x, delta.y) * _mouseSensitivity * MouseSensitivityScale;
            _bigMapOffset = _dragStartContainerPos + worldDelta;
            ClampOffset();
            _containerRect.anchoredPosition = _bigMapOffset;
        }

        if (Input.GetMouseButtonUp(0))
            _isDragging = false;
    }

    private void UpdateBigMapSize()
    {
        if (Grid == null) return;

        _bigMapSize = new Vector2(Grid.Width * _tileSize, Grid.Height * _tileSize);

        if (_containerRect != null)
        {
            _containerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _bigMapSize.x);
            _containerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _bigMapSize.y);
        }

        _bigMapCenterOffset = new Vector2(
            -_bigMapSize.x / 2f + _tileSize / 2f,
            -_bigMapSize.y / 2f + _tileSize / 2f);
    }

    private void ClampOffset()
    {
        if (_panelRect == null || _containerRect == null) return;

        float diffX = _containerRect.rect.width - _panelRect.rect.width;
        float diffY = _containerRect.rect.height - _panelRect.rect.height;

        float maxX = Mathf.Abs(diffX) / 2f;
        float maxY = Mathf.Abs(diffY) / 2f;

        _bigMapOffset.x = Mathf.Clamp(_bigMapOffset.x, -maxX, maxX);
        _bigMapOffset.y = Mathf.Clamp(_bigMapOffset.y, -maxY, maxY);
    }

    // ======== ПУБЛИЧНЫЙ API ========

    /// <summary>
    /// Создаёт тайл миникарты для указанной клетки, если его ещё нет.
    /// </summary>
    /// <param name="cell">Клетка для отображения.</param>
    public void ShowTile(Cell cell)
    {
        if (cell == null || _tiles.ContainsKey(cell.Position)) return;

        GameObject go = Instantiate(_mapTilePrefab, _bigMapContainer);
        MapTile tile = go.GetComponent<MapTile>();
        tile.Initialize(cell, _fogOfWarEnabled);
        go.transform.localPosition = new Vector3(
            cell.Position.x * _tileSize,
            cell.Position.y * _tileSize,
            0);

        _tiles[cell.Position] = tile;
    }

    /// <summary>
    /// Обновляет миникарту и большую карту под текущую позицию игрока.
    /// </summary>
    /// <param name="playerPos">Позиция игрока на сетке.</param>
    /// <param name="facingDir">Направление взгляда игрока.</param>
    /// <param name="range">Радиус обзора для тумана войны.</param>
    public void UpdateMap(Vector2Int playerPos, Directions facingDir, int range = 3)
    {
        if (Grid == null)
        {
            Debug.LogError("[MapManager] GridManager не найден!");
            return;
        }

        _playerPosition = playerPos;
        Grid.RevealArea(playerPos, facingDir, range);

        foreach (var cell in Grid.GetAllCells())
        {
            if (cell.IsDiscovered && !_tiles.ContainsKey(cell.Position))
                ShowTile(cell);
        }

        Vector3 targetPos = new Vector3(-playerPos.x * _tileSize, -playerPos.y * _tileSize, 0);
        _bigMapContainer.localPosition = targetPos;

        UpdatePlayerMarker(playerPos, facingDir);

        if (_bigMapOpen)
        {
            UpdateBigMapSize();
            UpdateBigMap();
        }
    }

    /// <summary>
    /// Обновляет позицию и поворот маркера игрока на миникарте.
    /// </summary>
    public void UpdatePlayerMarker(Vector2Int playerPos, Directions facingDir = Directions.North)
    {
        if (_playerMarkerInstance == null)
        {
            if (_playerMarkerPrefab == null || _bigMapContainer == null) return;
            _playerMarkerInstance = Instantiate(_playerMarkerPrefab, _bigMapContainer);
        }

        Vector3 pos = new Vector3(playerPos.x * _tileSize, playerPos.y * _tileSize, 0);
        _playerMarkerInstance.transform.localPosition = pos;
        _playerMarkerInstance.transform.SetAsLastSibling();
        _playerMarkerInstance.SetActive(true);

        float angle = facingDir switch
        {
            Directions.North => MarkerRotationNorth,
            Directions.East => MarkerRotationEast,
            Directions.South => MarkerRotationSouth,
            Directions.West => MarkerRotationWest,
            _ => MarkerRotationSouth
        };

        _playerMarkerInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Открывает или закрывает большую карту.
    /// </summary>
    public void ToggleBigMap()
    {
        _bigMapOpen = !_bigMapOpen;
        _bigMapPanel.SetActive(_bigMapOpen);

        if (_bigMapOpen)
        {
            UpdateBigMapSize();
            BuildBigMap();
            _bigMapOffset = new Vector2(-_playerPosition.x * _tileSize, -_playerPosition.y * _tileSize);
            ClampOffset();
            _containerRect.anchoredPosition = _bigMapOffset;
        }
        else
        {
            ClearBigMap();
        }
    }

    /// <summary>
    /// Включает или отключает туман войны.
    /// </summary>
    public void ToggleFog()
    {
        _fogOfWarEnabled = !_fogOfWarEnabled;
        RefreshAllTiles();

        if (_bigMapOpen)
            BuildBigMap();
    }

    /// <summary>
    /// Показывает всю карту (отключает туман войны).
    /// </summary>
    public void ShowFullMap()
    {
        _fogOfWarEnabled = false;
        RefreshAllTiles();

        if (_bigMapOpen)
            BuildBigMap();
    }

    // ======== ПОСТРОЕНИЕ БОЛЬШОЙ КАРТЫ ========

    private void BuildBigMap()
    {
        ClearBigMap();

        foreach (Transform child in _bigMapContainer)
        {
            if (child.gameObject == _playerMarkerInstance) continue;

            GameObject copy = Instantiate(child.gameObject, _bigMapFullContainer);
            copy.transform.localPosition = new Vector3(
                child.localPosition.x + _bigMapCenterOffset.x,
                child.localPosition.y + _bigMapCenterOffset.y,
                0);
            copy.transform.localScale = child.localScale;
            copy.transform.localRotation = child.localRotation;

            _bigMapTiles.Add(copy);
        }

        BuildBigMapPlayerMarker();
        UpdateBigMapSize();
    }

    private void BuildBigMapPlayerMarker()
    {
        if (_playerMarkerInstance == null) return;

        _bigMapPlayerMarker = Instantiate(_playerMarkerInstance, _bigMapFullContainer);
        _bigMapPlayerMarker.transform.localPosition = new Vector3(
            _playerPosition.x * _tileSize + _bigMapCenterOffset.x,
            _playerPosition.y * _tileSize + _bigMapCenterOffset.y,
            0);
        _bigMapPlayerMarker.transform.localScale = _playerMarkerInstance.transform.localScale;
        _bigMapPlayerMarker.transform.localRotation = _playerMarkerInstance.transform.localRotation;
        _bigMapPlayerMarker.transform.SetAsLastSibling();
    }

    private void UpdateBigMap()
    {
        if (!_bigMapOpen) return;

        foreach (Transform child in _bigMapContainer)
        {
            if (child.gameObject == _playerMarkerInstance) continue;

            Vector3 targetPos = new Vector3(
                child.localPosition.x + _bigMapCenterOffset.x,
                child.localPosition.y + _bigMapCenterOffset.y,
                0);

            if (TileExistsAt(targetPos)) continue;

            GameObject copy = Instantiate(child.gameObject, _bigMapFullContainer);
            copy.transform.localPosition = targetPos;
            copy.transform.localScale = child.localScale;
            copy.transform.localRotation = child.localRotation;

            _bigMapTiles.Add(copy);
        }

        UpdateBigMapPlayerMarkerPosition();
        UpdateBigMapSize();
    }

    private bool TileExistsAt(Vector3 position)
    {
        foreach (var tile in _bigMapTiles)
        {
            if (Vector3.Distance(tile.transform.localPosition, position) < TileMatchThreshold)
                return true;
        }

        return false;
    }

    private void UpdateBigMapPlayerMarkerPosition()
    {
        if (_bigMapPlayerMarker == null || _playerMarkerInstance == null) return;

        _bigMapPlayerMarker.transform.localPosition = new Vector3(
            _playerPosition.x * _tileSize + _bigMapCenterOffset.x,
            _playerPosition.y * _tileSize + _bigMapCenterOffset.y,
            0);
        _bigMapPlayerMarker.transform.localRotation = _playerMarkerInstance.transform.localRotation;
        _bigMapPlayerMarker.transform.SetAsLastSibling();
    }

    private void ClearBigMap()
    {
        foreach (var tile in _bigMapTiles)
            Destroy(tile);

        _bigMapTiles.Clear();

        if (_bigMapPlayerMarker != null)
        {
            Destroy(_bigMapPlayerMarker);
            _bigMapPlayerMarker = null;
        }
    }

    private void RefreshAllTiles()
    {
        if (Grid == null) return;

        foreach (var kvp in _tiles)
        {
            Cell cell = Grid.GetCell(kvp.Key);
            if (cell != null)
                kvp.Value.Initialize(cell, _fogOfWarEnabled);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
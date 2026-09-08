using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("UI References")]
    public Transform bigMapContainer;      // контейнер для миникарты
    public Transform bigMapFullContainer;  // контейнер для большой карты (дочерний BigMapPanel)

    [Header("Prefabs")]
    public GameObject mapTilePrefab;
    public GameObject playerMarkerPrefab;

    [Header("Settings")]
    public int tileSize = 20;
    public bool fogOfWarEnabled = true;

    [Header("Big Map Controls")]
    public float panSpeed = 200f;
    public float mouseSensitivity = 1f;

    [Header("Big Map UI")]
    public GameObject bigMapPanel;         // панель, которая открывается по M

    private GridManager _gridManager;
    private Dictionary<Vector2Int, MapTile> _tiles = new Dictionary<Vector2Int, MapTile>();
    private Vector2Int _playerPosition;
    private GameObject _playerMarkerInstance;
    private GameObject _bigMapPlayerMarker;
    private List<GameObject> _bigMapTiles = new List<GameObject>();

    private bool _bigMapOpen = false;
    private Vector2 _bigMapSize;            // размер контейнера в пикселях
    private Vector2 _bigMapOffset = Vector2.zero;   // смещение контейнера (для перемещения)
    private Vector2 _bigMapCenterOffset;    // смещение для центрирования тайлов

    private bool _isDragging = false;
    private Vector3 _dragStartMousePos;
    private Vector2 _dragStartContainerPos;

    private RectTransform _containerRect;
    private RectTransform _panelRect;       // NEW: ссылка на панель для ограничений

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _gridManager = GridManager.Instance;
        if (_gridManager == null)
            Debug.LogError("GridManager не найден!");

        if (bigMapFullContainer != null)
        {
            _containerRect = bigMapFullContainer.GetComponent<RectTransform>();
            _containerRect.pivot = new Vector2(0.5f, 0.5f);
            _containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            _containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            _containerRect.anchoredPosition = Vector2.zero;
        }

        if (bigMapPanel != null)
            _panelRect = bigMapPanel.GetComponent<RectTransform>(); // NEW

        UpdateBigMapSize();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            ToggleBigMap();

        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleFog();

        if (_bigMapOpen && _containerRect != null)
        {
            // 1. Перемещение стрелками
            Vector2 move = Vector2.zero;
            if (Input.GetKey(KeyCode.UpArrow)) move.y += panSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.DownArrow)) move.y -= panSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftArrow)) move.x -= panSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.RightArrow)) move.x += panSpeed * Time.deltaTime;

            if (move != Vector2.zero)
            {
                _bigMapOffset += move;
                ClampOffset();                  // NEW
                _containerRect.anchoredPosition = _bigMapOffset;
            }

            // 2. Перетаскивание мышью
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _dragStartMousePos = Input.mousePosition;
                _dragStartContainerPos = _bigMapOffset;
            }

            if (Input.GetMouseButton(0) && _isDragging)
            {
                Vector3 delta = Input.mousePosition - _dragStartMousePos;
                Vector2 worldDelta = new Vector2(delta.x, delta.y) * mouseSensitivity * 0.01f;
                _bigMapOffset = _dragStartContainerPos + worldDelta;
                ClampOffset();                  // NEW
                _containerRect.anchoredPosition = _bigMapOffset;
            }

            if (Input.GetMouseButtonUp(0))
                _isDragging = false;
        }
    }

    // ====== РАЗМЕР КОНТЕЙНЕРА ======

    private void UpdateBigMapSize()
    {
        if (_gridManager == null) return;
        _bigMapSize = new Vector2(
            _gridManager.Width * tileSize,
            _gridManager.Height * tileSize
        );
        if (_containerRect != null)
        {
            _containerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _bigMapSize.x);
            _containerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _bigMapSize.y);
        }
        // Вычисляем смещение для центрирования тайлов
        _bigMapCenterOffset = new Vector2(-_bigMapSize.x / 2f + tileSize / 2f, -_bigMapSize.y / 2f + tileSize / 2f);
    }

    // ====== ОГРАНИЧЕНИЕ ДВИЖЕНИЯ ======  // NEW
    private void ClampOffset()
    {
        if (_panelRect == null || _containerRect == null) return;

        // Получаем текущие размеры панели и контейнера
        float panelWidth = _panelRect.rect.width;
        float panelHeight = _panelRect.rect.height;
        float containerWidth = _containerRect.rect.width;   // или _bigMapSize.x
        float containerHeight = _containerRect.rect.height; // или _bigMapSize.y

        // Вычисляем допустимые границы по X и Y
        float diffX = containerWidth - panelWidth;
        float diffY = containerHeight - panelHeight;
        float minX = -Mathf.Abs(diffX) / 2f;
        float maxX = Mathf.Abs(diffX) / 2f;
        float minY = -Mathf.Abs(diffY) / 2f;
        float maxY = Mathf.Abs(diffY) / 2f;

        // Зажимаем смещение
        _bigMapOffset.x = Mathf.Clamp(_bigMapOffset.x, minX, maxX);
        _bigMapOffset.y = Mathf.Clamp(_bigMapOffset.y, minY, maxY);
    }

    // ====== ОСНОВНАЯ КАРТА (МИНИ) ======

    public void ShowTile(Cell cell)
    {
        if (cell == null || _tiles.ContainsKey(cell.Position)) return;

        GameObject go = Instantiate(mapTilePrefab, bigMapContainer);
        MapTile tile = go.GetComponent<MapTile>();
        tile.Initialize(cell, fogOfWarEnabled);
        go.transform.localPosition = new Vector3(cell.Position.x * tileSize, cell.Position.y * tileSize, 0);
        _tiles[cell.Position] = tile;
    }

    public void UpdateMap(Vector2Int playerPos, Directions facingDir, int range = 3)
    {
        _playerPosition = playerPos;
        _gridManager.RevealArea(playerPos, facingDir, range);

        foreach (var cell in _gridManager.GetAllCells())
        {
            if (cell.IsDiscovered && !_tiles.ContainsKey(cell.Position))
                ShowTile(cell);
        }

        Vector3 targetPos = new Vector3(-playerPos.x * tileSize, -playerPos.y * tileSize, 0);
        bigMapContainer.localPosition = targetPos;

        UpdatePlayerMarker(playerPos, facingDir);

        if (_bigMapOpen)
        {
            UpdateBigMapSize();
            UpdateBigMap();
        }
    }

    // ====== МАРКЕР ИГРОКА (на миникарте) ======

    public void UpdatePlayerMarker(Vector2Int playerPos, Directions facingDir = Directions.North)
    {
        if (_playerMarkerInstance == null)
        {
            if (playerMarkerPrefab == null || bigMapContainer == null) return;
            _playerMarkerInstance = Instantiate(playerMarkerPrefab, bigMapContainer);
        }

        Vector3 pos = new Vector3(playerPos.x * tileSize, playerPos.y * tileSize, 0);
        _playerMarkerInstance.transform.localPosition = pos;
        _playerMarkerInstance.transform.SetAsLastSibling();
        _playerMarkerInstance.SetActive(true);

        float angle = 0f;
        switch (facingDir)
        {
            case Directions.North: angle = 180f; break;
            case Directions.East: angle = 90f; break;
            case Directions.South: angle = 0f; break;
            case Directions.West: angle = 270f; break;
        }
        _playerMarkerInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // ====== БОЛЬШАЯ КАРТА ======

    public void ToggleBigMap()
    {
        _bigMapOpen = !_bigMapOpen;
        bigMapPanel.SetActive(_bigMapOpen);

        if (_bigMapOpen)
        {
            UpdateBigMapSize();
            BuildBigMap();
            // Центрируем на игроке и зажимаем
            _bigMapOffset = new Vector2(-_playerPosition.x * tileSize, -_playerPosition.y * tileSize);
            ClampOffset();                      // NEW
            _containerRect.anchoredPosition = _bigMapOffset;
        }
        else
        {
            ClearBigMap();
        }
    }

    private void BuildBigMap()
    {
        ClearBigMap();

        // Копируем тайлы с центрированием
        foreach (Transform child in bigMapContainer)
        {
            if (child.gameObject == _playerMarkerInstance) continue;
            GameObject copy = Instantiate(child.gameObject, bigMapFullContainer);
            copy.transform.localPosition = new Vector3(
                child.localPosition.x + _bigMapCenterOffset.x,
                child.localPosition.y + _bigMapCenterOffset.y,
                0
            );
            copy.transform.localScale = child.localScale;
            copy.transform.localRotation = child.localRotation;
            _bigMapTiles.Add(copy);
        }

        // Копируем маркер игрока
        if (_playerMarkerInstance != null)
        {
            _bigMapPlayerMarker = Instantiate(_playerMarkerInstance, bigMapFullContainer);
            _bigMapPlayerMarker.transform.localPosition = new Vector3(
                _playerPosition.x * tileSize + _bigMapCenterOffset.x,
                _playerPosition.y * tileSize + _bigMapCenterOffset.y,
                0
            );
            _bigMapPlayerMarker.transform.localScale = _playerMarkerInstance.transform.localScale;
            _bigMapPlayerMarker.transform.localRotation = _playerMarkerInstance.transform.localRotation;
            _bigMapPlayerMarker.transform.SetAsLastSibling();
        }

        UpdateBigMapSize();
    }

    private void UpdateBigMap()
    {
        if (!_bigMapOpen) return;

        // Добавляем новые тайлы
        foreach (Transform child in bigMapContainer)
        {
            if (child.gameObject == _playerMarkerInstance) continue;

            bool exists = false;
            Vector3 targetPos = new Vector3(child.localPosition.x + _bigMapCenterOffset.x, child.localPosition.y + _bigMapCenterOffset.y, 0);
            foreach (var tile in _bigMapTiles)
            {
                if (Vector3.Distance(tile.transform.localPosition, targetPos) < 0.01f)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                GameObject copy = Instantiate(child.gameObject, bigMapFullContainer);
                copy.transform.localPosition = targetPos;
                copy.transform.localScale = child.localScale;
                copy.transform.localRotation = child.localRotation;
                _bigMapTiles.Add(copy);
            }
        }

        // Обновляем маркер игрока на большой карте
        if (_bigMapPlayerMarker != null && _playerMarkerInstance != null)
        {
            _bigMapPlayerMarker.transform.localPosition = new Vector3(
                _playerPosition.x * tileSize + _bigMapCenterOffset.x,
                _playerPosition.y * tileSize + _bigMapCenterOffset.y,
                0
            );
            _bigMapPlayerMarker.transform.localRotation = _playerMarkerInstance.transform.localRotation;
            _bigMapPlayerMarker.transform.SetAsLastSibling();
        }

        UpdateBigMapSize();
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

    // ====== ТУМАН ВОЙНЫ ======

    public void ToggleFog()
    {
        fogOfWarEnabled = !fogOfWarEnabled;
        RefreshAllTiles();
        if (_bigMapOpen)
        {
            BuildBigMap();
        }
    }

    public void ShowFullMap()
    {
        fogOfWarEnabled = false;
        RefreshAllTiles();
        if (_bigMapOpen)
        {
            BuildBigMap();
        }
    }

    private void RefreshAllTiles()
    {
        foreach (var kvp in _tiles)
        {
            Cell cell = _gridManager.GetCell(kvp.Key);
            if (cell != null)
                kvp.Value.Initialize(cell, fogOfWarEnabled);
        }
    }
}
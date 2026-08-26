using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("UI References")]
    public Transform bigMapContainer;     // родитель для тайлов большой карты

    [Header("Prefabs")]
    public GameObject mapTilePrefab;

    [Header("Settings")]
    public int tileSize = 20;             // размер тайла в пикселях
    public bool fogOfWarEnabled = true;   // туман войны включён по умолчанию

    private GridManager _gridManager;
    private Dictionary<Vector2Int, MapTile> _tiles = new();
    private Vector2Int _playerPosition;

    public GameObject playerMarkerPrefab; // префаб маркера (переименуйте в инспекторе)
    private GameObject _playerMarkerInstance; // экземпляр на сцене
    private GameObject _bigMapPlayerMarker;

    [Header("Big Map")]
    public GameObject bigMapPanel;           // панель с большой картой
    public Transform bigMapFullContainer;    // контейнер внутри панели

    private bool _bigMapOpen = false;
    private List<GameObject> _bigMapTiles = new List<GameObject>(); // для хранения копий

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _gridManager = GridManager.Instance;
        if (_gridManager == null)
        {
            Debug.LogError("GridManager не найден!");
            return;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleBigMap();
        }
    }

    // Создать тайл для ячейки (если ещё не создан)
    public void ShowTile(Cell cell)
    {
        if (cell == null) return;
        if (_tiles.ContainsKey(cell.Position)) return;

        GameObject go = Instantiate(mapTilePrefab, bigMapContainer);
        MapTile tile = go.GetComponent<MapTile>();
        tile.Initialize(cell, fogOfWarEnabled);
        go.transform.localPosition = new Vector3(cell.Position.x * tileSize, cell.Position.y * tileSize, 0);
        _tiles[cell.Position] = tile;
    }

    // Обновить карту после перемещения игрока
    public void UpdateMap(Vector2Int playerPos, Directions facingDir, int range = 3)
    {
        _playerPosition = playerPos;
        _gridManager.RevealArea(playerPos, facingDir, range);

        // Создаём тайлы для всех открытых ячеек (если их ещё нет)
        foreach (var cell in _gridManager.GetAllCells())
        {
            if (cell.IsDiscovered && !_tiles.ContainsKey(cell.Position))
            {
                ShowTile(cell);
            }
        }

        // Центрируем карту на игроке (для большой карты и миникарты)
        Vector3 targetPos = new Vector3(-playerPos.x * tileSize, -playerPos.y * tileSize, 0);
        bigMapContainer.localPosition = targetPos;
        // Обновляем маркер игрока
        UpdatePlayerMarker(playerPos);

        float angle = 0f;
        float shift = 180f;
        switch (facingDir)
        {
            case Directions.North: angle = 0f + shift; break;
            case Directions.East: angle = -90f + shift; break;
            case Directions.South: angle = 180f + shift; break;
            case Directions.West: angle = 90f + shift; break;
        }
        _playerMarkerInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Переключение тумана войны
    public void ToggleFog()
    {
        fogOfWarEnabled = !fogOfWarEnabled;
        // Обновляем все существующие тайлы
        foreach (var tile in _tiles.Values)
        {
            // Здесь нужно обновить видимость тайла в зависимости от fogOfWarEnabled.
            // Но tile.Initialize уже принимает этот флаг, поэтому проще пересоздать все тайлы или обновить их.
            // Для простоты — пересоздадим все тайлы.
            RefreshAllTiles();
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

    public void UpdatePlayerMarker(Vector2Int playerPos)
    {
        
         // Если экземпляр ещё не создан — создаём
         if (_playerMarkerInstance == null)
         {
             if (playerMarkerPrefab == null)
             {
                 Debug.LogWarning("playerMarkerPrefab не назначен!");
                 return;
             }
             if (bigMapContainer == null)
             {
                 Debug.LogError("bigMapContainer не назначен!");
                 return;
             }
             _playerMarkerInstance = Instantiate(playerMarkerPrefab, bigMapContainer);
             // Можно задать размер маркера, если нужно
             // _playerMarkerInstance.transform.localScale = Vector3.one * 0.5f;
         }

         // Обновляем позицию маркера
         _playerMarkerInstance.transform.localPosition = new Vector3(
             playerPos.x * tileSize,
             playerPos.y * tileSize,
             0
         );

         _playerMarkerInstance.transform.SetAsLastSibling();

         // Убедимся, что маркер активен
         if (!_playerMarkerInstance.activeSelf)
             _playerMarkerInstance.SetActive(true);
        
        // Обновляем маркер миникарты
        if (_playerMarkerInstance != null)
        {
            _playerMarkerInstance.transform.localPosition = new Vector3(
                playerPos.x * tileSize,
                playerPos.y * tileSize,
                0
            );
            _playerMarkerInstance.transform.SetAsLastSibling();
        }

        // Обновляем маркер большой карты (если он существует)
        if (_bigMapPlayerMarker != null)
        {
            _bigMapPlayerMarker.transform.localPosition = new Vector3(
                playerPos.x * tileSize,
                playerPos.y * tileSize,
                0
            );
            _bigMapPlayerMarker.transform.SetAsLastSibling();
        }
    }

    public void ToggleBigMap()
    {
        _bigMapOpen = !_bigMapOpen;
        bigMapPanel.SetActive(_bigMapOpen);

        if (_bigMapOpen)
            BuildBigMap();
        else
            ClearBigMap();
    }

    private void BuildBigMap()
    {
        // Очищаем старые тайлы (если были)
        ClearBigMap();

        // Создаём копии всех тайлов из основного контейнера
        foreach (Transform child in bigMapContainer)
        {
            // Получаем компонент MapTile из исходного тайла
            MapTile originalTile = child.GetComponent<MapTile>();
            if (originalTile == null) continue;

            // Создаём копию префаба (или дублируем существующий GameObject)
            GameObject copy = Instantiate(child.gameObject, bigMapFullContainer);
            // Сохраняем локальную позицию
            copy.transform.localPosition = child.localPosition;
            copy.transform.localScale = child.localScale;
            copy.transform.localRotation = child.localRotation;

            // Копируем состояние (можно переиспользовать Initialize, если ячейка ещё доступна)
            // Для простоты мы просто копируем визуальное состояние, так как MapTile уже обновлён.
            // Но если нужно, можно вызвать Initialize с ячейкой (если у вас есть ссылка)
            // Для этого нужно хранить соответствие тайл -> ячейка.
            // Упрощённо: копируем как есть.

            _bigMapTiles.Add(copy);
        }

        // Копируем маркер игрока
        if (_playerMarkerInstance != null)
        {
            _bigMapPlayerMarker = Instantiate(_playerMarkerInstance, bigMapFullContainer);
            _bigMapPlayerMarker.transform.localPosition = _playerMarkerInstance.transform.localPosition;
            _bigMapPlayerMarker.transform.localScale = _playerMarkerInstance.transform.localScale;
            _bigMapPlayerMarker.transform.localRotation = _playerMarkerInstance.transform.localRotation;
            _bigMapPlayerMarker.transform.SetAsLastSibling();
        }

        // Убедимся, что масштаб и позиция контейнера соответствуют
        bigMapFullContainer.localPosition = bigMapContainer.localPosition; // если нужно
        // Можно также центрировать относительно игрока
        // Если вы центрируете bigMapContainer в UpdateMap, то и здесь будет то же смещение.
    }

    private void ClearBigMap()
    {
        foreach (var tile in _bigMapTiles)
        {
            Destroy(tile);
        }
        _bigMapTiles.Clear();

        if (_bigMapPlayerMarker != null)
        {
            Destroy(_bigMapPlayerMarker);
            _bigMapPlayerMarker = null;
        }
    }
}

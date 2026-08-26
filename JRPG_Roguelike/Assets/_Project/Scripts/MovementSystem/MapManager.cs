using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("UI References")]
    public Transform bigMapContainer;     // родитель дл€ тайлов большой карты
    public Transform miniMapContainer;    // родитель дл€ тайлов миникарты (или маска)
    public RectTransform miniMapMask;     // маска дл€ миникарты (опционально)

    [Header("Prefabs")]
    public GameObject mapTilePrefab;

    [Header("Settings")]
    public int tileSize = 20;             // размер тайла в пиксел€х
    public bool fogOfWarEnabled = true;   // туман войны включЄн по умолчанию

    private GridManager _gridManager;
    private Dictionary<Vector2Int, MapTile> _tiles = new();
    private Vector2Int _playerPosition;

    public GameObject playerMarkerPrefab; // префаб маркера (переименуйте в инспекторе)
    private GameObject _playerMarkerInstance; // экземпл€р на сцене

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

    // —оздать тайл дл€ €чейки (если ещЄ не создан)
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

    // ќбновить карту после перемещени€ игрока
    public void UpdateMap(Vector2Int playerPos, Directions facingDir, int range = 3)
    {
        _playerPosition = playerPos;
        _gridManager.RevealArea(playerPos, facingDir, range);

        // —оздаЄм тайлы дл€ всех открытых €чеек (если их ещЄ нет)
        foreach (var cell in _gridManager.GetAllCells())
        {
            if (cell.IsDiscovered && !_tiles.ContainsKey(cell.Position))
            {
                ShowTile(cell);
            }
        }

        // ÷ентрируем карту на игроке (дл€ большой карты и миникарты)
        Vector3 targetPos = new Vector3(-playerPos.x * tileSize, -playerPos.y * tileSize, 0);
        bigMapContainer.localPosition = targetPos;
        // ќбновл€ем маркер игрока
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

    // ѕереключение тумана войны
    public void ToggleFog()
    {
        fogOfWarEnabled = !fogOfWarEnabled;
        // ќбновл€ем все существующие тайлы
        foreach (var tile in _tiles.Values)
        {
            // «десь нужно обновить видимость тайла в зависимости от fogOfWarEnabled.
            // Ќо tile.Initialize уже принимает этот флаг, поэтому проще пересоздать все тайлы или обновить их.
            // ƒл€ простоты Ч пересоздадим все тайлы.
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
        // ≈сли экземпл€р ещЄ не создан Ч создаЄм
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
            // ћожно задать размер маркера, если нужно
            // _playerMarkerInstance.transform.localScale = Vector3.one * 0.5f;
        }

        // ќбновл€ем позицию маркера
        _playerMarkerInstance.transform.localPosition = new Vector3(
            playerPos.x * tileSize,
            playerPos.y * tileSize,
            0
        );

        _playerMarkerInstance.transform.SetAsLastSibling();

        // ”бедимс€, что маркер активен
        if (!_playerMarkerInstance.activeSelf)
            _playerMarkerInstance.SetActive(true);
    }
}

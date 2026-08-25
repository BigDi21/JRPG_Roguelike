using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("UI References")]
    public Transform bigMapContainer;     // родитель для тайлов большой карты
    public Transform miniMapContainer;    // родитель для тайлов миникарты (или маска)
    public RectTransform miniMapMask;     // маска для миникарты (опционально)

    [Header("Prefabs")]
    public GameObject mapTilePrefab;

    [Header("Settings")]
    public int tileSize = 20;             // размер тайла в пикселях
    public bool fogOfWarEnabled = true;   // туман войны включён по умолчанию

    private GridManager _gridManager;
    private Dictionary<Vector2Int, MapTile> _tiles = new();
    private Vector2Int _playerPosition;

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
        if (miniMapMask != null)
        {
            // Если миникарта использует ту же карту, но с маской, обновляем позицию контента
            // Т.к. bigMapContainer — это контент, а miniMapMask — маска, то bigMapContainer должен быть дочерним miniMapMask
            // и мы уже обновили его позицию. Миникарта покажет фрагмент.
        }
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
}

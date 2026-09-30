using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Визуальное представление одного тайла миникарты.
/// Отображает пол и стены в зависимости от соединений клетки.
/// </summary>
public class MapTile : MonoBehaviour
{
    [Header("Пол")]
    [SerializeField] private Image _floor;

    [Header("Стены")]
    [SerializeField] private Image _northWall;
    [SerializeField] private Image _eastWall;
    [SerializeField] private Image _southWall;
    [SerializeField] private Image _westWall;

    /// <summary>
    /// Инициализирует тайл данными клетки.
    /// </summary>
    /// <param name="cellData">Данные клетки.</param>
    /// <param name="showFog">Учитывать ли туман войны. True — скрывать неоткрытые клетки.</param>
    public void Initialize(Cell cellData, bool showFog = true)
    {
        if (cellData == null)
        {
            Debug.LogError("[MapTile] Передан null вместо данных клетки!");
            return;
        }

        if (showFog && !cellData.IsDiscovered)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        UpdateFloor();
        UpdateWalls(cellData);
    }

    /// <summary>
    /// Устанавливает стандартный цвет пола.
    /// </summary>
    private void UpdateFloor()
    {
        if (_floor == null) return;

        _floor.color = Color.white;
    }

    /// <summary>
    /// Включает стены там, где нет прохода.
    /// </summary>
    /// <param name="cellData">Данные клетки.</param>
    private void UpdateWalls(Cell cellData)
    {
        if (_northWall != null)
            _northWall.gameObject.SetActive(!cellData.CanMove(Directions.North));

        if (_eastWall != null)
            _eastWall.gameObject.SetActive(!cellData.CanMove(Directions.East));

        if (_southWall != null)
            _southWall.gameObject.SetActive(!cellData.CanMove(Directions.South));

        if (_westWall != null)
            _westWall.gameObject.SetActive(!cellData.CanMove(Directions.West));
    }
}
using UnityEngine;

/// <summary>
/// Визуальное представление клетки. Отображает пол, стены и иконку объекта.
/// </summary>
public class CellVisual : MonoBehaviour
{
    [Header("Пол и стены")]
    [SerializeField] private Renderer _floorRenderer;
    [SerializeField] private GameObject _northWall;
    [SerializeField] private GameObject _eastWall;
    [SerializeField] private GameObject _southWall;
    [SerializeField] private GameObject _westWall;

    [Header("Иконка объекта")]
    [SerializeField] private SpriteRenderer _occupantIcon;

    [Header("Цвета пола")]
    [SerializeField] private Color _defaultFloorColor = Color.white;
    [SerializeField] private Color _occupiedFloorColor = Color.red;

    /// <summary>Данные клетки, к которой привязан визуал.</summary>
    public Cell Data { get; private set; }

    /// <summary>
    /// Инициализирует визуал данными клетки и подписывается на изменения.
    /// </summary>
    /// <param name="cellData">Данные клетки.</param>
    public void Initialize(Cell cellData)
    {
        if (cellData == null)
        {
            Debug.LogError("[CellVisual] Передан null вместо данных клетки!");
            return;
        }

        Data = cellData;
        Data.OnDataChanged += OnCellDataChanged;
        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (Data != null)
            Data.OnDataChanged -= OnCellDataChanged;
    }

    private void OnCellDataChanged(Cell cell)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (Data == null) return;

        UpdateFloor();
        UpdateWalls();
        UpdateOccupantIcon();
    }

    /// <summary>
    /// Обновляет цвет пола в зависимости от занятости клетки.
    /// </summary>
    private void UpdateFloor()
    {
        if (_floorRenderer == null) return;

        _floorRenderer.material.color = Data.IsOccupied
            ? _occupiedFloorColor
            : _defaultFloorColor;
    }

    /// <summary>
    /// Включает и отключает стены в зависимости от соединений клетки.
    /// </summary>
    private void UpdateWalls()
    {
        if (_northWall != null)
            _northWall.SetActive(!Data.CanMove(Directions.North));

        if (_eastWall != null)
            _eastWall.SetActive(!Data.CanMove(Directions.East));

        if (_southWall != null)
            _southWall.SetActive(!Data.CanMove(Directions.South));

        if (_westWall != null)
            _westWall.SetActive(!Data.CanMove(Directions.West));
    }

    /// <summary>
    /// Обновляет иконку объекта, стоящего в клетке.
    /// </summary>
    private void UpdateOccupantIcon()
    {
        if (_occupantIcon == null) return;

        if (Data.IsOccupied && Data.Occupant != null)
        {
            var spriteRenderer = Data.Occupant.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                _occupantIcon.sprite = spriteRenderer.sprite;
                _occupantIcon.color = spriteRenderer.color;
            }

            _occupantIcon.gameObject.SetActive(true);
        }
        else
        {
            _occupantIcon.gameObject.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (Data == null) return;

        Debug.Log($"[CellVisual] Клик по клетке {Data.Position}");
    }
}
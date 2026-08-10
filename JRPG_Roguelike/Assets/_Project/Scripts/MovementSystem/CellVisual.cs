using UnityEngine;

public class CellVisual : MonoBehaviour
{
    [Header("Пол и стены")]
    public Renderer floorRenderer; // теперь Renderer, а не SpriteRenderer
    public GameObject northWall;
    public GameObject eastWall;
    public GameObject southWall;
    public GameObject westWall;

    [Header("Иконка объекта (опционально)")]
    public SpriteRenderer occupantIcon;

    public Cell Data { get; private set; }

    public Color defaultFloorColor = Color.white;
    public Color occupiedFloorColor = Color.red;

    public void Initialize(Cell cellData)
    {
        Data = cellData;
        Data.OnDataChanged += OnCellDataChanged;
        UpdateVisual();
    }

    private void OnCellDataChanged(Cell cell)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (Data == null) return;

        // 1. Обновляем пол (цвет, если занято)
        if (Data.IsOccupied)
            floorRenderer.material.color = occupiedFloorColor;
        else
            floorRenderer.material.color = defaultFloorColor;

        // 2. Обновляем стены: отключаем ту, где есть соединение
        northWall.SetActive(!Data.CanMove(Directions.North));
        eastWall.SetActive(!Data.CanMove(Directions.East));
        southWall.SetActive(!Data.CanMove(Directions.South));
        westWall.SetActive(!Data.CanMove(Directions.West));

        // 3. Иконка объекта (если есть)
        if (occupantIcon != null)
        {
            if (Data.IsOccupied && Data.Occupant != null)
            {
                var spr = Data.Occupant.GetComponent<SpriteRenderer>();
                if (spr != null)
                {
                    occupantIcon.sprite = spr.sprite;
                    occupantIcon.color = spr.color;
                }
                occupantIcon.gameObject.SetActive(true);
            }
            else
            {
                occupantIcon.gameObject.SetActive(false);
            }
        }
    }

    // Опционально: клик по ячейке
    private void OnMouseDown()
    {
        Debug.Log($"Clicked on cell {Data.Position}");
    }
}
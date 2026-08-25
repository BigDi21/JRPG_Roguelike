using UnityEngine;
using UnityEngine.UI;

public class MapTile : MonoBehaviour
{
    public Image floor;
    public Image northWall;
    public Image eastWall;
    public Image southWall;
    public Image westWall;

    public void Initialize(Cell cellData, bool showFog = true)
    {
        // Если туман войны включен, показываем только открытые ячейки
        if (showFog && !cellData.IsDiscovered)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        floor.color = Color.white;

        // Стены: отключаем те, где есть проход
        northWall.gameObject.SetActive(!cellData.CanMove(Directions.North));
        eastWall.gameObject.SetActive(!cellData.CanMove(Directions.East));
        southWall.gameObject.SetActive(!cellData.CanMove(Directions.South));
        westWall.gameObject.SetActive(!cellData.CanMove(Directions.West));
    }
}

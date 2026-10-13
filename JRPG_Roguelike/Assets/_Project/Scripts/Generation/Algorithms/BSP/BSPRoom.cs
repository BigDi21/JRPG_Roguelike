using UnityEngine;

/// <summary>
/// Комната в BSP-алгоритме. Представляет собой прямоугольную область на сетке.
/// </summary>
public struct BSPRoom
{
    /// <summary>Границы комнаты (x, y, ширина, высота).</summary>
    public RectInt Bounds;

    /// <summary>Центральная клетка комнаты.</summary>
    public readonly Vector2Int Center => new(
        Bounds.x + Bounds.width / 2,
        Bounds.y + Bounds.height / 2);

    /// <summary>
    /// Создаёт комнату с заданными границами.
    /// </summary>
    public BSPRoom(RectInt bounds)
    {
        Bounds = bounds;
    }

    /// <summary>
    /// Проверяет, находится ли клетка внутри комнаты.
    /// </summary>
    public readonly bool Contains(Vector2Int pos)
    {
        return pos.x >= Bounds.x && pos.x < Bounds.x + Bounds.width
            && pos.y >= Bounds.y && pos.y < Bounds.y + Bounds.height;
    }
}

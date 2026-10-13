using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Результат работы алгоритма генерации. Содержит сетку, точки спавна, выходы и комнаты.
/// </summary>
public struct LevelGrid
{
    /// <summary>Ширина сетки в клетках.</summary>
    public int Width;

    /// <summary>Высота сетки в клетках.</summary>
    public int Height;

    /// <summary>Двумерный массив клеток.</summary>
    public Cell[,] Cells;

    /// <summary>Стартовая позиция игрока.</summary>
    public Vector2Int SpawnPoint;

    /// <summary>Список выходов из локации.</summary>
    public List<Vector2Int> Exits;

    /// <summary>
    /// Список комнат (границы прямоугольниками).
    /// Используется для механики «открыть комнату при входе».
    /// Может быть пустым для алгоритмов, которые не создают комнаты (DFS).
    /// </summary>
    public List<RectInt> Rooms;
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Результат работы алгоритма генерации. Содержит сетку, точки спавна и выходы.
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
}

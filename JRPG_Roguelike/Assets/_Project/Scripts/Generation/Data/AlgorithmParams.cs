using System;

/// <summary>
/// Параметры генерации уровня. Передаются алгоритму.
/// </summary>
[Serializable]
public struct AlgorithmParams
{
    /// <summary>Шанс появления петли в лабиринте (0–100%).</summary>
    public int LoopChance;

    /// <summary>Минимальный размер комнаты (для BSP).</summary>
    public int MinRoomSize;

    /// <summary>Максимальный размер комнаты (для BSP).</summary>
    public int MaxRoomSize;

    /// <summary>Минимальное количество комнат (для BSP).</summary>
    public int MinRooms;

    /// <summary>Максимальное количество комнат (для BSP).</summary>
    public int MaxRooms;

    /// <summary>
    /// Стандартные параметры для простого лабиринта без петель.
    /// </summary>
    public static AlgorithmParams DefaultMaze => new()
    {
        LoopChance = 0,
        MinRoomSize = 0,
        MaxRoomSize = 0,
        MinRooms = 0,
        MaxRooms = 0
    };
}

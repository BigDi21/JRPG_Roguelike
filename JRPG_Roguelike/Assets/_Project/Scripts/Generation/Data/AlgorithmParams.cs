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

    // ======== ПАРАМЕТРЫ КЛЕТОЧНОГО АВТОМАТА ========

    /// <summary>Процент начального заполнения стенами (0–100).</summary>
    public int FillChance;

    /// <summary>Количество итераций сглаживания.</summary>
    public int Iterations;

    /// <summary>Минимальный размер связной области пола (в клетках).</summary>
    public int MinRegionSize;

    /// <summary>
    /// Стандартные параметры для простого лабиринта без петель.
    /// </summary>
    public static AlgorithmParams DefaultMaze => new()
    {
        LoopChance = 0,
        MinRoomSize = 0,
        MaxRoomSize = 0,
        MinRooms = 0,
        MaxRooms = 0,
        FillChance = 46,
        Iterations = 4,
        MinRegionSize = 20
    };

    /// <summary>
    /// Стандартные параметры для клеточного автомата (Пепельный лес).
    /// </summary>
    public static AlgorithmParams DefaultCellularAutomata => new()
    {
        LoopChance = 0,
        MinRoomSize = 0,
        MaxRoomSize = 0,
        MinRooms = 0,
        MaxRooms = 0,
        FillChance = 46,
        Iterations = 4,
        MinRegionSize = 20
    };
}

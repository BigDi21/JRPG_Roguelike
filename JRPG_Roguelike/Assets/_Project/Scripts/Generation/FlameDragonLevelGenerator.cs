using UnityEngine;

/// <summary>
/// Конкретный генератор уровней для проекта «Пламя Дракона».
/// Знает обо всех реализованных алгоритмах.
/// </summary>
public class FlameDragonLevelGenerator : LevelGenerator
{
    protected override ILevelAlgorithm CreateAlgorithm(GenerationAlgorithm type)
    {
        return type switch
        {
            GenerationAlgorithm.RecursiveBacktracking => new RecursiveBacktrackingAlgorithm(),
            // GenerationAlgorithm.BSP => new BSPAlgorithm(),
            // GenerationAlgorithm.CellularAutomata => new CellularAutomataAlgorithm(),
            _ => null
        };
    }
}

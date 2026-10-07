using UnityEngine;

/// <summary>
/// Типы алгоритмов генерации.
/// </summary>
public enum GenerationAlgorithm
{
    RecursiveBacktracking,
    BSP,
    CellularAutomata
}

/// <summary>
/// Базовый класс для управления генерацией уровней.
/// Выбирает алгоритм на основе типа локации и вызывает его.
/// </summary>
[DefaultExecutionOrder(-50)]
public abstract class LevelGenerator : MonoBehaviour
{
    /// <summary>
    /// Генерирует уровень указанного размера с заданным seed.
    /// </summary>
    /// <param name="width">Ширина сетки.</param>
    /// <param name="height">Высота сетки.</param>
    /// <param name="seed">Seed для воспроизводимости.</param>
    /// <param name="algorithmType">Тип алгоритма.</param>
    /// <param name="parameters">Параметры алгоритма.</param>
    /// <returns>Готовая сетка уровня.</returns>
    public LevelGrid Generate(
        int width,
        int height,
        int seed,
        GenerationAlgorithm algorithmType,
        AlgorithmParams parameters)
    {
        ILevelAlgorithm algorithm = CreateAlgorithm(algorithmType);

        if (algorithm == null)
        {
            Debug.LogError($"[LevelGenerator] Алгоритм {algorithmType} не поддерживается!");
            return default;
        }

        Debug.Log($"[LevelGenerator] Генерация {algorithmType} ({width}×{height}, seed={seed})");

        return algorithm.Generate(width, height, seed, parameters);
    }

    /// <summary>
    /// Создаёт экземпляр алгоритма по типу. Переопределяется в наследниках.
    /// </summary>
    protected abstract ILevelAlgorithm CreateAlgorithm(GenerationAlgorithm type);
}

/// <summary>
/// Интерфейс алгоритма генерации уровня.
/// Все алгоритмы (DFS, BSP, клеточный автомат) реализуют его.
/// </summary>
public interface ILevelAlgorithm
{
    /// <summary>
    /// Генерирует уровень и возвращает результат.
    /// </summary>
    /// <param name="width">Ширина сетки.</param>
    /// <param name="height">Высота сетки.</param>
    /// <param name="seed">Seed для воспроизводимости.</param>
    /// <param name="parameters">Параметры алгоритма.</param>
    /// <returns>Готовая сетка уровня.</returns>
    LevelGrid Generate(int width, int height, int seed, AlgorithmParams parameters);
}

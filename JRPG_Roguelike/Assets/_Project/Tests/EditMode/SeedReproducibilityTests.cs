using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Тесты для проверки воспроизводимости генерации по seed.
/// </summary>
public class SeedReproducibilityTests
{
    [Test]
    public void SameSeed_ProducesSameGridLayout()
    {
        // Arrange
        var algo = new RecursiveBacktrackingAlgorithm();

        // Act
        LevelGrid grid1 = algo.Generate(15, 15, 42, AlgorithmParams.DefaultMaze);
        LevelGrid grid2 = algo.Generate(15, 15, 42, AlgorithmParams.DefaultMaze);

        // Assert
        for (var x = 0; x < 15; x++)
        {
            for (var y = 0; y < 15; y++)
            {
                Assert.AreEqual(
                    grid1.Cells[x, y].Connections,
                    grid2.Cells[x, y].Connections,
                    $"Различие в клетке ({x},{y}) — seed не воспроизводится!");
            }
        }
    }

    [Test]
    public void UnityRandom_SameSeed_ProducesSameSequence()
    {
        // Arrange
        Random.InitState(12345);
        var first = Random.Range(0, 1000);
        var second = Random.Range(0, 1000);

        // Act
        Random.InitState(12345);
        var firstRepeat = Random.Range(0, 1000);
        var secondRepeat = Random.Range(0, 1000);

        // Assert
        Assert.AreEqual(first, firstRepeat);
        Assert.AreEqual(second, secondRepeat);
    }
}

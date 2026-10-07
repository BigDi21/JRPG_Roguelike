using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Тесты для структуры данных LevelGrid.
/// </summary>
public class LevelGridTests
{
    [Test]
    public void LevelGrid_CanStoreData()
    {
        // Arrange
        var grid = new LevelGrid
        {
            Width = 10,
            Height = 10,
            Cells = new Cell[10, 10],
            SpawnPoint = new Vector2Int(0, 0)
        };

        // Assert
        Assert.AreEqual(10, grid.Width);
        Assert.AreEqual(10, grid.Height);
        Assert.AreEqual(0, grid.SpawnPoint.x);
    }

    [Test]
    public void AlgorithmParams_DefaultMaze_HasZeroLoopChance()
    {
        // Act
        AlgorithmParams p = AlgorithmParams.DefaultMaze;

        // Assert
        Assert.AreEqual(0, p.LoopChance);
    }
}

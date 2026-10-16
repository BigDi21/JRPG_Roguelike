using NUnit.Framework;

/// <summary>
/// Тесты для алгоритма клеточного автомата.
/// </summary>
public class CellularAutomataTests
{
    private AlgorithmParams BuildParams() => AlgorithmParams.DefaultCellularAutomata;

    [Test]
    public void Generate_ReturnsCorrectSize()
    {
        var algo = new CellularAutomataAlgorithm();
        LevelGrid result = algo.Generate(60, 60, 42, BuildParams());

        Assert.AreEqual(60, result.Width);
        Assert.AreEqual(60, result.Height);
        Assert.IsNotNull(result.Cells);
    }

    [Test]
    public void Generate_SameSeed_ProducesSameLayout()
    {
        var algo = new CellularAutomataAlgorithm();

        LevelGrid grid1 = algo.Generate(60, 60, 999, BuildParams());
        LevelGrid grid2 = algo.Generate(60, 60, 999, BuildParams());

        for (var x = 0; x < 60; x++)
        {
            for (var y = 0; y < 60; y++)
            {
                Assert.AreEqual(
                    grid1.Cells[x, y].Connections,
                    grid2.Cells[x, y].Connections,
                    $"Различие в клетке ({x},{y})");
            }
        }
    }

    [Test]
    public void Generate_HasEnoughFloor()
    {
        var algo = new CellularAutomataAlgorithm();
        LevelGrid result = algo.Generate(60, 60, 42, BuildParams());

        var floorCount = 0;

        for (var x = 0; x < 60; x++)
        {
            for (var y = 0; y < 60; y++)
            {
                if (result.Cells[x, y].Connections != Directions.None)
                {
                    floorCount++;
                }
            }
        }

        var total = 60 * 60;
        var floorPercent = (float)floorCount / total;

        Assert.Greater(floorPercent, 0.20f, "Слишком мало пола");
        Assert.Less(floorPercent, 0.85f, "Слишком много пола");
    }

    [Test]
    public void Generate_SpawnPointIsWalkable()
    {
        var algo = new CellularAutomataAlgorithm();
        LevelGrid result = algo.Generate(60, 60, 555, BuildParams());

        Cell spawnCell = result.Cells[result.SpawnPoint.x, result.SpawnPoint.y];

        Assert.AreNotEqual(Directions.None, spawnCell.Connections,
            "Спавн-точка должна быть проходимой");
    }

    [Test]
    public void Generate_BordersAreWalls()
    {
        var algo = new CellularAutomataAlgorithm();
        LevelGrid result = algo.Generate(60, 60, 777, BuildParams());

        // Проверяем все клетки по периметру
        for (var x = 0; x < 60; x++)
        {
            Assert.AreEqual(Directions.None, result.Cells[x, 0].Connections,
                $"Верхняя граница содержит пол в ({x}, 0)");
            Assert.AreEqual(Directions.None, result.Cells[x, 59].Connections,
                $"Нижняя граница содержит пол в ({x}, 59)");
        }

        for (var y = 0; y < 60; y++)
        {
            Assert.AreEqual(Directions.None, result.Cells[0, y].Connections,
                $"Левая граница содержит пол в (0, {y})");
            Assert.AreEqual(Directions.None, result.Cells[59, y].Connections,
                $"Правая граница содержит пол в (59, {y})");
        }
    }
}

using NUnit.Framework;

/// <summary>
/// Тесты для алгоритма рекурсивного бэктрекинга.
/// </summary>
public class RecursiveBacktrackingTests
{
    [Test]
    public void Generate_ReturnsGridWithCorrectSize()
    {
        var algo = new RecursiveBacktrackingAlgorithm();
        LevelGrid result = algo.Generate(20, 20, 12345, AlgorithmParams.DefaultMaze);

        Assert.AreEqual(20, result.Width);
        Assert.AreEqual(20, result.Height);
        Assert.IsNotNull(result.Cells);
    }

    [Test]
    public void Generate_SameSeed_ProducesSameMaze()
    {
        var algo = new RecursiveBacktrackingAlgorithm();

        LevelGrid grid1 = algo.Generate(15, 15, 999, AlgorithmParams.DefaultMaze);
        LevelGrid grid2 = algo.Generate(15, 15, 999, AlgorithmParams.DefaultMaze);

        for (var x = 0; x < 15; x++)
        {
            for (var y = 0; y < 15; y++)
            {
                Assert.AreEqual(
                    grid1.Cells[x, y].Connections,
                    grid2.Cells[x, y].Connections,
                    $"Различие в клетке ({x},{y})");
            }
        }
    }

    [Test]
    public void Generate_DifferentSeeds_ProduceDifferentMazes()
    {
        var algo = new RecursiveBacktrackingAlgorithm();

        LevelGrid grid1 = algo.Generate(15, 15, 1, AlgorithmParams.DefaultMaze);
        LevelGrid grid2 = algo.Generate(15, 15, 2, AlgorithmParams.DefaultMaze);

        var differences = 0;
        for (var x = 0; x < 15; x++)
        {
            for (var y = 0; y < 15; y++)
            {
                if (grid1.Cells[x, y].Connections != grid2.Cells[x, y].Connections)
                {
                    differences++;
                }
            }
        }

        Assert.Greater(differences, 0, "Разные seed должны давать разные лабиринты");
    }

    [Test]
    public void Generate_NoLoopChance_ProducesPerfectMaze()
    {
        var algo = new RecursiveBacktrackingAlgorithm();
        LevelGrid result = algo.Generate(10, 10, 42, AlgorithmParams.DefaultMaze);

        var connectionBits = 0;

        for (var x = 0; x < 10; x++)
        {
            for (var y = 0; y < 10; y++)
            {
                connectionBits += CountConnections(result.Cells[x, y].Connections);
            }
        }

        // В идеальном лабиринте без петель: (N - 1) рёбер × 2 (по два конца)
        var expected = ((10 * 10) - 1) * 2;
        Assert.AreEqual(expected, connectionBits,
            "Лабиринт без петель должен содержать ровно N-1 рёбер");
    }

    [Test]
    public void Generate_AllCellsHaveAtLeastOneConnection()
    {
        var algo = new RecursiveBacktrackingAlgorithm();
        LevelGrid result = algo.Generate(12, 12, 777, AlgorithmParams.DefaultMaze);

        for (var x = 0; x < 12; x++)
        {
            for (var y = 0; y < 12; y++)
            {
                Assert.AreNotEqual(
                    Directions.None,
                    result.Cells[x, y].Connections,
                    $"Клетка ({x},{y}) изолирована");
            }
        }
    }

    private int CountConnections(Directions dir)
    {
        var count = 0;
        if ((dir & Directions.North) != 0)
        {
            count++;
        }

        if ((dir & Directions.East) != 0)
        {
            count++;
        }

        if ((dir & Directions.South) != 0)
        {
            count++;
        }

        if ((dir & Directions.West) != 0)
        {
            count++;
        }

        return count;
    }
}

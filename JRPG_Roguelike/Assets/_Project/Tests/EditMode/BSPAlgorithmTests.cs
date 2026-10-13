using NUnit.Framework;

/// <summary>
/// Тесты для BSP-алгоритма генерации уровня.
/// </summary>
public class BSPAlgorithmTests
{
    private AlgorithmParams BuildBSPParams()
    {
        return new AlgorithmParams
        {
            LoopChance = 0,
            MinRoomSize = 4,
            MaxRoomSize = 6,
            MinRooms = 10,
            MaxRooms = 15
        };
    }

    [Test]
    public void Generate_ReturnsGridWithCorrectSize()
    {
        var algo = new BSPAlgorithm();
        LevelGrid result = algo.Generate(50, 50, 12345, BuildBSPParams());

        Assert.AreEqual(50, result.Width);
        Assert.AreEqual(50, result.Height);
        Assert.IsNotNull(result.Cells);
    }

    [Test]
    public void Generate_SameSeed_ProducesSameLayout()
    {
        var algo = new BSPAlgorithm();

        LevelGrid grid1 = algo.Generate(50, 50, 999, BuildBSPParams());
        LevelGrid grid2 = algo.Generate(50, 50, 999, BuildBSPParams());

        for (var x = 0; x < 50; x++)
        {
            for (var y = 0; y < 50; y++)
            {
                Assert.AreEqual(
                    grid1.Cells[x, y].Connections,
                    grid2.Cells[x, y].Connections,
                    $"Различие в клетке ({x},{y})");
            }
        }
    }

    [Test]
    public void Generate_DifferentSeeds_ProduceDifferentLayouts()
    {
        var algo = new BSPAlgorithm();

        LevelGrid grid1 = algo.Generate(50, 50, 1, BuildBSPParams());
        LevelGrid grid2 = algo.Generate(50, 50, 2, BuildBSPParams());

        var differences = 0;

        for (var x = 0; x < 50; x++)
        {
            for (var y = 0; y < 50; y++)
            {
                if (grid1.Cells[x, y].Connections != grid2.Cells[x, y].Connections)
                {
                    differences++;
                }
            }
        }

        Assert.Greater(differences, 0, "Разные seed должны давать разные уровни");
    }

    [Test]
    public void Generate_ProducesAtLeastOneConnection()
    {
        var algo = new BSPAlgorithm();
        LevelGrid result = algo.Generate(50, 50, 42, BuildBSPParams());

        var connectionsTotal = 0;

        for (var x = 0; x < 50; x++)
        {
            for (var y = 0; y < 50; y++)
            {
                connectionsTotal += CountConnections(result.Cells[x, y].Connections);
            }
        }

        Assert.Greater(connectionsTotal, 0, "Должны быть соединения между клетками");
    }

    [Test]
    public void Generate_SpawnPointIsInsideGrid()
    {
        var algo = new BSPAlgorithm();
        LevelGrid result = algo.Generate(50, 50, 555, BuildBSPParams());

        Assert.GreaterOrEqual(result.SpawnPoint.x, 0);
        Assert.GreaterOrEqual(result.SpawnPoint.y, 0);
        Assert.Less(result.SpawnPoint.x, 50);
        Assert.Less(result.SpawnPoint.y, 50);
    }

    [Test]
    public void Generate_FillsRoomsList()
    {
        var algo = new BSPAlgorithm();
        LevelGrid result = algo.Generate(50, 50, 42, BuildBSPParams());

        Assert.IsNotNull(result.Rooms);
        Assert.Greater(result.Rooms.Count, 0, "BSP должен заполнить список комнат");
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

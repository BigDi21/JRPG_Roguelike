using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Тесты для проверки соединений клетки.
/// </summary>
public class CellTests
{
    [Test]
    public void CanMove_ReturnsTrue_WhenConnectionExists()
    {
        var cell = new Cell(Vector2Int.zero);
        cell.AddConnection(Directions.North);

        Assert.IsTrue(cell.CanMove(Directions.North));
    }

    [Test]
    public void CanMove_ReturnsFalse_WhenNoConnection()
    {
        var cell = new Cell(Vector2Int.zero);
        cell.AddConnection(Directions.North);

        Assert.IsFalse(cell.CanMove(Directions.South));
    }

    [Test]
    public void RemoveConnection_DisablesMovement()
    {
        var cell = new Cell(Vector2Int.zero);
        cell.AddConnection(Directions.East);
        cell.RemoveConnection(Directions.East);

        Assert.IsFalse(cell.CanMove(Directions.East));
    }
}

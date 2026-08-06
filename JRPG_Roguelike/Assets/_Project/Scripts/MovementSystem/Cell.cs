using UnityEngine;
using System;

[Flags]
public enum Directions
{
    None = 0,
    North = 1 << 0,
    East = 1 << 1,
    South = 1 << 2,
    West = 1 << 3
}

public class Cell
{
    public Vector2Int Position { get; private set; }
    public Directions Connections { get; set; }
    public GameObject Occupant { get; private set; }

    public event Action<Cell> OnDataChanged;

    public Cell(Vector2Int position)
    {
        Position = position;
        Connections = Directions.None;
        Occupant = null;
    }

    public bool CanMove(Directions direction) => (Connections & direction) != 0;
    public void AddConnection(Directions direction) => Connections |= direction;
    public void RemoveConnection(Directions direction) => Connections &= ~direction;

    public void SetOccupant(GameObject obj)
    {
        if (Occupant == obj) return;
        Occupant = obj;
        NotifyChanged();
    }

    public void ClearOccupant()
    {
        if (Occupant == null) return;
        Occupant = null;
        NotifyChanged();
    }

    public bool IsOccupied => Occupant != null;
    private void NotifyChanged() => OnDataChanged?.Invoke(this);
}
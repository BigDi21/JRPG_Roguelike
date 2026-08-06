using UnityEngine;
using System;

// Перечисление направлений для битовой маски
[Flags]
public enum Directions
{
    None = 0,
    North = 1 << 0, // 1
    East = 1 << 1, // 2
    South = 1 << 2, // 4
    West = 1 << 3  // 8
}

public class Cell
{
    // ====== ДАННЫЕ ======
    public Vector2Int Position { get; private set; }
    public Directions Connections { get; set; }
    public GameObject Occupant { get; private set; }

    // Событие, которое вызывается при любом изменении данных ячейки
    public event Action<Cell> OnDataChanged;

    // ====== КОНСТРУКТОР ======
    public Cell(Vector2Int position, Directions initialConnections = Directions.None)
    {
        Position = position;
        Connections = initialConnections;
        Occupant = null;
    }

    // ====== РАБОТА С ПРОХОДИМОСТЬЮ ======
    public bool CanMove(Directions direction)
    {
        return (Connections & direction) != 0;
    }

    public void SetDirection(Directions direction, bool canMove)
    {
        if (canMove)
            Connections |= direction;
        else
            Connections &= ~direction;
        NotifyChanged();
    }

    public void AddConnections(Directions directions)
    {
        Connections |= directions;
        NotifyChanged();
    }

    public void RemoveConnections(Directions directions)
    {
        Connections &= ~directions;
        NotifyChanged();
    }

    public Directions GetAvailableDirections() => Connections;

    // ====== РАБОТА С ОБЪЕКТОМ НА ЯЧЕЙКЕ ======
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

    // ====== УВЕДОМЛЕНИЕ ОБ ИЗМЕНЕНИИ ======
    private void NotifyChanged()
    {
        OnDataChanged?.Invoke(this);
    }

    // ====== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ======
    public override string ToString()
    {
        return $"Cell ({Position.x},{Position.y}): Conn={Connections}, Occupant={(Occupant != null ? Occupant.name : "null")}";
    }
}
using System;
using UnityEngine;

/// <summary>
/// Направления перемещения по сетке. Битовая маска, используется для хранения соединений между клетками.
/// </summary>
[Flags]
public enum Directions
{
    /// <summary>Отсутствие направления.</summary>
    None = 0,

    /// <summary>Север (вверх).</summary>
    North = 1 << 0,

    /// <summary>Восток (вправо).</summary>
    East = 1 << 1,

    /// <summary>Юг (вниз).</summary>
    South = 1 << 2,

    /// <summary>Запад (влево).</summary>
    West = 1 << 3
}

/// <summary>
/// Клетка игрового поля. Хранит позицию, соединения с соседями, содержимое и состояние исследования.
/// </summary>
public class Cell
{
    /// <summary>Позиция клетки на сетке.</summary>
    public Vector2Int Position { get; private set; }

    /// <summary>Битовая маска соединений с соседними клетками.</summary>
    public Directions Connections { get; set; }

    /// <summary>Объект, находящийся в клетке (игрок, враг, предмет).</summary>
    public GameObject Occupant { get; private set; }

    /// <summary>Исследована ли клетка (для тумана войны).</summary>
    public bool IsDiscovered { get; private set; }

    /// <summary>Занята ли клетка кем-либо или чем-либо.</summary>
    public bool IsOccupied => Occupant != null;

    /// <summary>Вызывается при любом изменении данных клетки.</summary>
    public event Action<Cell> OnDataChanged;

    /// <summary>
    /// Создаёт клетку с указанной позицией.
    /// </summary>
    /// <param name="position">Позиция клетки на сетке.</param>
    public Cell(Vector2Int position)
    {
        Position = position;
        Connections = Directions.None;
        Occupant = null;
        IsDiscovered = false;
    }

    /// <summary>
    /// Проверяет, можно ли двигаться в указанном направлении.
    /// </summary>
    /// <param name="direction">Направление для проверки.</param>
    /// <returns>True, если проход свободен.</returns>
    public bool CanMove(Directions direction)
    {
        return (Connections & direction) != 0;
    }

    /// <summary>
    /// Добавляет соединение в указанном направлении.
    /// </summary>
    /// <param name="direction">Направление соединения.</param>
    public void AddConnection(Directions direction)
    {
        Connections |= direction;
    }

    /// <summary>
    /// Удаляет соединение в указанном направлении.
    /// </summary>
    /// <param name="direction">Направление соединения.</param>
    public void RemoveConnection(Directions direction)
    {
        Connections &= ~direction;
    }

    /// <summary>
    /// Устанавливает объект-заполнитель в клетку. Уведомляет подписчиков об изменении.
    /// </summary>
    /// <param name="obj">Объект для размещения.</param>
    public void SetOccupant(GameObject obj)
    {
        if (Occupant == obj) return;

        Occupant = obj;
        NotifyChanged();
    }

    /// <summary>
    /// Очищает клетку от объекта. Уведомляет подписчиков об изменении.
    /// </summary>
    public void ClearOccupant()
    {
        if (Occupant == null) return;

        Occupant = null;
        NotifyChanged();
    }

    /// <summary>
    /// Отмечает клетку как исследованную. Уведомляет подписчиков об изменении.
    /// </summary>
    public void Discover()
    {
        if (IsDiscovered) return;

        IsDiscovered = true;
        OnDataChanged?.Invoke(this);
    }

    /// <summary>
    /// Уведомляет подписчиков об изменении данных клетки.
    /// </summary>
    private void NotifyChanged()
    {
        OnDataChanged?.Invoke(this);
    }
}
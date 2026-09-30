using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Компонент инвентаря. Хранит список предметов.
/// </summary>
public class InventoryComponent : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private List<ItemData> _items = new();

    /// <summary>Список предметов в инвентаре.</summary>
    public List<ItemData> Items => _items;

    /// <summary>
    /// Добавляет предмет в инвентарь.
    /// </summary>
    /// <param name="item">Добавляемый предмет.</param>
    public void AddItem(ItemData item) => _items.Add(item);

    /// <summary>
    /// Удаляет предмет из инвентаря.
    /// </summary>
    /// <param name="item">Удаляемый предмет.</param>
    public void RemoveItem(ItemData item) => _items.Remove(item);
}
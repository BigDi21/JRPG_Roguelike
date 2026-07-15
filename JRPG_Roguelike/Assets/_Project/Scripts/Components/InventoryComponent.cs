using System.Collections.Generic;
using UnityEngine;

public class InventoryComponent : MonoBehaviour
{
    public List<ItemData> Items = new List<ItemData>();

    public void AddItem(ItemData item) => Items.Add(item);
    public void RemoveItem(ItemData item) => Items.Remove(item);
}


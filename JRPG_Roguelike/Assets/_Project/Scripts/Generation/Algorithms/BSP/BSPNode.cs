using UnityEngine;

/// <summary>
/// Узел BSP-дерева. Может быть листом (с комнатой) или внутренним узлом (с двумя детьми).
/// </summary>
public class BSPNode
{
    /// <summary>Область узла на сетке.</summary>
    public RectInt Area;

    /// <summary>Левый/нижний ребёнок.</summary>
    public BSPNode Left;

    /// <summary>Правый/верхний ребёнок.</summary>
    public BSPNode Right;

    /// <summary>Комната в этом узле (только у листьев).</summary>
    public BSPRoom? Room;

    /// <summary>Является ли узел листом (нет детей).</summary>
    public bool IsLeaf => Left == null && Right == null;

    /// <summary>Есть ли у узла комната.</summary>
    public bool HasRoom => Room.HasValue;

    /// <summary>
    /// Создаёт узел с указанной областью.
    /// </summary>
    public BSPNode(RectInt area)
    {
        Area = area;
    }
}

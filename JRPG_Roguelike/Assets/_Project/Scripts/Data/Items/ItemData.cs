using UnityEngine;

/// <summary>
/// Данные предмета (расходника). Хранит ID, имя, иконку, описание и эффект.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "JRPG/Item")]
public class ItemData : ScriptableObject
{
    [Header("Идентификация")]
    [Tooltip("Уникальный ID в нижнем регистре без пробелов. Например: sword, potion_hp")]
    [SerializeField] private string _id;

    [Header("Отображение")]
    [SerializeField] private string _itemName;
    [SerializeField] private Sprite _icon;

    [TextArea]
    [SerializeField] private string _description;

    [Header("Эффект")]
    [SerializeReference] private Effect _effect;

    /// <summary>Уникальный ID предмета.</summary>
    public string Id => _id;

    /// <summary>Отображаемое имя.</summary>
    public string ItemName => _itemName;

    /// <summary>Иконка предмета.</summary>
    public Sprite Icon => _icon;

    /// <summary>Описание.</summary>
    public string Description => _description;

    /// <summary>Эффект применения.</summary>
    public Effect Effect => _effect;
}

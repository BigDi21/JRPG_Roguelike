using UnityEngine;

/// <summary>
/// Данные предмета (расходника). Хранит имя, иконку, описание и эффект.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "JRPG/Item")]
public class ItemData : ScriptableObject
{
    [Header("Настройки предмета")]
    [SerializeField] private string _itemName;
    [SerializeField] private Sprite _icon;

    [TextArea]
    [SerializeField] private string _description;

    [SerializeReference] private Effect _effect;

    /// <summary>Отображаемое имя предмета.</summary>
    public string ItemName => _itemName;

    /// <summary>Иконка предмета.</summary>
    public Sprite Icon => _icon;

    /// <summary>Описание предмета.</summary>
    public string Description => _description;

    /// <summary>Эффект, применяемый при использовании предмета.</summary>
    public Effect Effect => _effect;
}
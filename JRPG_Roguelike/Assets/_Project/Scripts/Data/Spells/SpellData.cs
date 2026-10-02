using UnityEngine;

/// <summary>
/// Данные заклинания. Хранит ID, имя, иконку, описание, стоимость маны и эффект.
/// </summary>
[CreateAssetMenu(fileName = "NewSpell", menuName = "JRPG/Spell")]
public class SpellData : ScriptableObject
{
    [Header("Идентификация")]
    [Tooltip("Уникальный ID в нижнем регистре без пробелов. Например: fireball, heal")]
    [SerializeField] private string _id;

    [Header("Отображение")]
    [SerializeField] private string _spellName;
    [SerializeField] private Sprite _icon;

    [TextArea]
    [SerializeField] private string _description;

    [Header("Стоимость")]
    [Min(0)]
    [SerializeField] private int _manaCost;

    [Header("Эффект")]
    [SerializeReference] private Effect _effect;

    /// <summary>Уникальный ID заклинания.</summary>
    public string Id => _id;

    /// <summary>Отображаемое имя.</summary>
    public string SpellName => _spellName;

    /// <summary>Иконка.</summary>
    public Sprite Icon => _icon;

    /// <summary>Описание.</summary>
    public string Description => _description;

    /// <summary>Стоимость применения в мане.</summary>
    public int ManaCost => _manaCost;

    /// <summary>Эффект заклинания.</summary>
    public Effect Effect => _effect;
}

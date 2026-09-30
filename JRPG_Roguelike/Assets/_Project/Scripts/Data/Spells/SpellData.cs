using UnityEngine;

/// <summary>
/// Данные заклинания. Хранит имя, иконку, описание, стоимость маны и эффект.
/// </summary>
[CreateAssetMenu(fileName = "NewSpell", menuName = "JRPG/Spell")]
public class SpellData : ScriptableObject
{
    [Header("Настройки заклинания")]
    [SerializeField] private string _spellName;
    [SerializeField] private Sprite _icon;

    [TextArea]
    [SerializeField] private string _description;

    [Min(0)]
    [SerializeField] private int _manaCost;

    [SerializeReference] private Effect _effect;

    /// <summary>Отображаемое имя заклинания.</summary>
    public string SpellName => _spellName;

    /// <summary>Иконка заклинания.</summary>
    public Sprite Icon => _icon;

    /// <summary>Описание заклинания.</summary>
    public string Description => _description;

    /// <summary>Стоимость применения в мане.</summary>
    public int ManaCost => _manaCost;

    /// <summary>Эффект, применяемый при использовании заклинания.</summary>
    public Effect Effect => _effect;
}
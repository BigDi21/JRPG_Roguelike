using SimpleJRPG;
using UnityEngine;

/// <summary>
/// Эффект временного усиления характеристики кастера.
/// </summary>
[CreateAssetMenu(fileName = "NewBuffSelfEffect", menuName = "JRPG/Effects/BuffSelf")]
public class BuffSelfEffect : Effect
{
    [Header("Настройки эффекта")]
    [SerializeField] private string _statName = "Strength";
    [SerializeField] private int _bonus = 5;
    [SerializeField] private int _duration = 3;

    /// <summary>Название характеристики для усиления: Strength, Defense, Magic.</summary>
    public string StatName => _statName;

    /// <summary>Величина бонуса.</summary>
    public int Bonus => _bonus;

    /// <summary>Длительность эффекта в ходах.</summary>
    public int Duration => _duration;

    /// <inheritdoc />
    public override void Apply(ICombatant caster, ICombatant target)
    {
        // TODO: реализовать систему временных баффов в StatsComponent
        Debug.Log($"{caster.Name} усиливает себя: +{_bonus} к {_statName} на {_duration} ходов.");
    }
}
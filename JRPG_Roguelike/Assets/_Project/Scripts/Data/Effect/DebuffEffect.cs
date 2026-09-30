using SimpleJRPG;
using UnityEngine;

/// <summary>
/// Эффект временного ослабления характеристики цели.
/// </summary>
[CreateAssetMenu(fileName = "NewDebuffEffect", menuName = "JRPG/Effects/DebuffEnemy")]
public class DebuffEnemyEffect : Effect
{
    [Header("Настройки эффекта")]
    [SerializeField] private string _statName = "Defense";
    [SerializeField] private int _penalty = 5;
    [SerializeField] private int _duration = 3;

    /// <summary>Название характеристики для ослабления: Strength, Defense, Magic.</summary>
    public string StatName => _statName;

    /// <summary>Величина штрафа.</summary>
    public int Penalty => _penalty;

    /// <summary>Длительность эффекта в ходах.</summary>
    public int Duration => _duration;

    /// <inheritdoc />
    public override void Apply(ICombatant caster, ICombatant target)
    {
        // TODO: реализовать систему временных дебаффов в StatsComponent
        Debug.Log($"{target.Name} ослаблен: -{_penalty} к {_statName} на {_duration} ходов.");
    }
}
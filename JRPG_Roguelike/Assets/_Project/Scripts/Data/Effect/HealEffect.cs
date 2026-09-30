using SimpleJRPG;
using UnityEngine;

/// <summary>
/// Эффект восстановления здоровья цели.
/// </summary>
[CreateAssetMenu(fileName = "NewHealEffect", menuName = "JRPG/Effects/Heal")]
public class HealEffect : Effect
{
    [Header("Настройки лечения")]
    [SerializeField] private int _healAmount = 30;

    /// <summary>Количество восстанавливаемого HP.</summary>
    public int HealAmount => _healAmount;

    /// <inheritdoc />
    public override void Apply(ICombatant user, ICombatant target)
    {
        if (target == null) return;

        target.Heal(_healAmount);
        Debug.Log($"{target.Name} вылечен на {_healAmount} HP.");
    }
}
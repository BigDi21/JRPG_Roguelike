using SimpleJRPG;
using UnityEngine;

/// <summary>
/// Эффект нанесения урона цели.
/// </summary>
[CreateAssetMenu(fileName = "NewDamageEffect", menuName = "JRPG/Effects/Damage")]
public class DamageEffect : Effect
{
    [Header("Настройки урона")]
    [SerializeField] private int _damageAmount = 10;
    [SerializeField] private DamageType _damageType = DamageType.Magical;

    /// <summary>Количество наносимого урона.</summary>
    public int DamageAmount => _damageAmount;

    /// <summary>Тип урона (физический, магический и т.д.).</summary>
    public DamageType DamageType => _damageType;

    /// <inheritdoc />
    public override void Apply(ICombatant user, ICombatant target)
    {
        target.TakeDamage(_damageAmount);
        Debug.Log($"{target.Name} получает {_damageAmount} урона.");
    }
}
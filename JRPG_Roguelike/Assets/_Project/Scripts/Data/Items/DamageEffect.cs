using SimpleJRPG;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDamageEffect", menuName = "JRPG/Effects/Damage")]
public class DamageEffect : ItemEffect
{
    public int DamageAmount;

    public override void Apply(ICombatant user, ICombatant target)
    {
        target.TakeDamage(DamageAmount);
        Debug.Log($"{target.Name} получает {DamageAmount} урона.");
    }
}
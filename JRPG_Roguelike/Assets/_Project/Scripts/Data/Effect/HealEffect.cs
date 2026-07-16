using SimpleJRPG;
using UnityEngine;

[CreateAssetMenu(fileName = "NewHealEffect", menuName = "JRPG/Effects/Heal")]
public class HealEffect : Effect
{
    public int HealAmount;

    public override void Apply(ICombatant user, ICombatant target)
    {
        target.Heal(HealAmount);
        Debug.Log($"{target.Name} вылечен на {HealAmount} HP.");
    }
}
using SimpleJRPG;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuffSelfEffect", menuName = "JRPG/Effects/BuffSelf")]
public class BuffSelfEffect : Effect
{
    public string statName; // "Strength", "Defense", "Magic"
    public int bonus;
    public int duration; // в ходах

    public override void Apply(ICombatant caster, ICombatant target)
    {
        // Временно усиливаем стат caster'а на bonus ходов
        // (нужно добавить логику временных баффов в StatsComponent)
        Debug.Log($"{caster.Name} усиливает себя: +{bonus} к {statName} на {duration} ходов.");
        // Здесь вызывается метод для добавления баффа
        if (caster.Team == 0)
        {
            // player.ApplyBuff(statName, bonus, duration);
        }
    }
}
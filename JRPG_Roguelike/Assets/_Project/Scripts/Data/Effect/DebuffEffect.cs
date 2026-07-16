using SimpleJRPG;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDebuffEffect", menuName = "JRPG/Effects/DebuffEnemy")]
public class DebuffEnemyEffect : Effect
{
    public string statName; // "Strength", "Defense", "Magic"
    public int penalty;
    public int duration; // в ходах

    public override void Apply(ICombatant caster, ICombatant target)
    {
        // Временно ослабляем stat у target'а на penalty ходов
        Debug.Log($"{target.Name} ослаблен: -{penalty} к {statName} на {duration} ходов.");
        // Здесь вызывается метод для добавления дебаффа
        if (caster.Team == 0)
        {
            // enemy.ApplyDebuff(statName, penalty, duration);
        }
    }
}
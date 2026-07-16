using UnityEngine;

public class EnemyCombatant : BaseCombatant
{
    public EnemyCombatant(GameObject enemyGO, string name, int team = 1)
    {
        Name = name;
        Team = team;
        HealthComponent = enemyGO.GetComponent<HealthComponent>();
        StatsComponent = enemyGO.GetComponent<StatsComponent>();
    }

    public override void TakeDamage(int amount)
    {
        HealthComponent.TakeDamage(amount);
    }

    public override void Heal(int amount)
    {
        HealthComponent.Heal(amount);
    }
}

using SimpleJRPG;
using UnityEngine;

public abstract class BaseCombatant : ICombatant
{
    public string Name { get; protected set; }
    public bool IsAlive => HealthComponent != null && HealthComponent.CurrentHealth > 0;
    public int Team { get; protected set; }
    public float Speed => StatsComponent != null ? StatsComponent.Speed : 1f;

    public HealthComponent HealthComponent { get; protected set; }
    public StatsComponent StatsComponent { get; protected set; }
    public InventoryComponent InventoryComponent { get; protected set; }
    public SpellManagerComponent SpellManagerComponent { get; protected set; }

    public abstract void TakeDamage(int amount);
    public abstract void Heal(int amount);
}

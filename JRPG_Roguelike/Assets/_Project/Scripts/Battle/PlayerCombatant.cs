using UnityEngine;

public class PlayerCombatant : BaseCombatant
{
    public PlayerCombatant(GameObject playerGO, string name, int team = 0)
    {
        Name = name;
        Team = team;
        HealthComponent = playerGO.GetComponent<HealthComponent>();
        StatsComponent = playerGO.GetComponent<StatsComponent>();
        InventoryComponent = playerGO.GetComponent<InventoryComponent>();
        SpellManagerComponent = playerGO.GetComponent<SpellManagerComponent>();
    }

    public int Mana => StatsComponent != null ? StatsComponent.Mana : 0;

    public void UseMana(int amount) => StatsComponent?.UseMana(amount);

    public override void TakeDamage(int amount)
    {
        int finalDamage = Mathf.Max(1, amount - StatsComponent.Defense / 2);
        HealthComponent.TakeDamage(finalDamage);
    }

    public override void Heal(int amount)
    {
        HealthComponent.Heal(amount);
    }
}

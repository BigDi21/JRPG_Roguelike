using UnityEngine;

/// <summary>
/// Боец-игрок. Хранит ссылки на компоненты персонажа и реализует контракт <see cref="BaseCombatant"/>.
/// </summary>
public class PlayerCombatant : BaseCombatant
{
    /// <summary>
    /// Создаёт бойца-игрока.
    /// </summary>
    /// <param name="playerGO">GameObject игрока с компонентами Health, Stats, Inventory и SpellManager.</param>
    /// <param name="name">Отображаемое имя игрока.</param>
    /// <param name="team">Номер команды. По умолчанию 0.</param>
    public PlayerCombatant(GameObject playerGO, string name, int team = 0)
    {
        Name = name;
        Team = team;
        HealthComponent = playerGO.GetComponent<HealthComponent>();
        StatsComponent = playerGO.GetComponent<StatsComponent>();
        InventoryComponent = playerGO.GetComponent<InventoryComponent>();
        SpellManagerComponent = playerGO.GetComponent<SpellManagerComponent>();
    }

    /// <summary>Текущий запас маны. 0, если компонент статов отсутствует.</summary>
    public int Mana => StatsComponent != null ? StatsComponent.Mana : 0;

    /// <summary>Тратит указанное количество маны.</summary>
    /// <param name="amount">Количество маны для списания.</param>
    public void UseMana(int amount) => StatsComponent?.UseMana(amount);

    /// <inheritdoc />
    public override void TakeDamage(int amount)
    {
        int finalDamage = Mathf.Max(1, amount - StatsComponent.Defense / 2);
        HealthComponent.TakeDamage(finalDamage);
    }

    /// <inheritdoc />
    public override void Heal(int amount)
    {
        HealthComponent.Heal(amount);
    }
}
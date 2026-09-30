using UnityEngine;

/// <summary>
/// Боец-противник. Хранит ссылки на компоненты врага и реализует контракт <see cref="BaseCombatant"/>.
/// </summary>
public class EnemyCombatant : BaseCombatant
{
    /// <summary>
    /// Создаёт бойца-противника.
    /// </summary>
    /// <param name="enemyGO">GameObject врага с компонентами Health и Stats.</param>
    /// <param name="name">Отображаемое имя врага.</param>
    /// <param name="team">Номер команды. По умолчанию 1.</param>
    public EnemyCombatant(GameObject enemyGO, string name, int team = 1)
    {
        Name = name;
        Team = team;
        HealthComponent = enemyGO.GetComponent<HealthComponent>();
        StatsComponent = enemyGO.GetComponent<StatsComponent>();
    }

    /// <inheritdoc />
    public override void TakeDamage(int amount)
    {
        HealthComponent.TakeDamage(amount);
    }

    /// <inheritdoc />
    public override void Heal(int amount)
    {
        HealthComponent.Heal(amount);
    }
}
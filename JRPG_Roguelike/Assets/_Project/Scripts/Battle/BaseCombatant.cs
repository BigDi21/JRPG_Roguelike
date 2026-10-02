using SimpleJRPG;
using UnityEngine;

/// <summary>
/// Базовый класс для всех бойцов (игрок, враг).
/// Реализует общий контракт <see cref="ICombatant"/> и хранит ссылки на компоненты.
/// </summary>
public abstract class BaseCombatant : ICombatant
{
    /// <summary>Отображаемое имя бойца.</summary>
    public string Name { get; protected set; }

    /// <summary>Номер команды. 0 — игрок, 1 — враги.</summary>
    public int Team { get; protected set; }

    /// <summary>Жив ли боец (HP > 0).</summary>
    public bool IsAlive => HealthComponent != null && HealthComponent.CurrentHealth > 0;

    /// <summary>Скорость (инициатива) бойца. По умолчанию 1, если статы не заданы.</summary>
    public float Speed => StatsComponent != null ? StatsComponent.Speed : 1f;

    /// <summary>Компонент здоровья.</summary>
    public HealthComponent HealthComponent { get; protected set; }

    /// <summary>Компонент характеристик.</summary>
    public StatsComponent StatsComponent { get; protected set; }

    /// <summary>Компонент инвентаря.</summary>
    public InventoryComponent InventoryComponent { get; protected set; }

    /// <summary>Компонент управления заклинаниями.</summary>
    public SpellManagerComponent SpellManagerComponent { get; protected set; }

    /// <summary>
    /// Наносит бойцу урон.
    /// </summary>
    /// <param name="amount">Количество урона.</param>
    public abstract void TakeDamage(int amount);

    /// <summary>
    /// Восстанавливает бойцу здоровье.
    /// </summary>
    /// <param name="amount">Количество восстанавливаемого HP.</param>
    public abstract void Heal(int amount);

    private readonly int _counter = 0;
}

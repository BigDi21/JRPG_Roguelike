using System.Collections.Generic;
using System.Linq;
using SimpleJRPG;
using UnityEngine;

/// <summary>
/// Управляет ходом боя: инициализацией, очерёдностью, действиями игрока и врага.
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Ссылки на бойцов")]
    [SerializeField] private GameObject _playerGO;
    [SerializeField] private GameObject _enemyGO;

    private Battle _battle;
    private ClassicTurnSystem _turnSystem;
    private readonly List<ICombatant> _allies = new();
    private readonly List<ICombatant> _enemies = new();

    private PlayerCombatant _player;
    private EnemyCombatant _enemy;

    public bool IsBattleActive { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (_playerGO == null || _enemyGO == null)
        {
            return;
        }

        StartBattle(_playerGO, _enemyGO);
    }

    /// <summary>
    /// Начинает бой между игроком и врагом.
    /// </summary>
    /// <param name="playerGO">GameObject игрока.</param>
    /// <param name="enemyGO">GameObject врага.</param>
    public void StartBattle(GameObject playerGO, GameObject enemyGO)
    {
        Debug.Log("[BattleManager] StartBattle вызван");
        if (IsBattleActive)
        {
            return;
        }

        _player = new PlayerCombatant(playerGO, "Герой", 0);
        _enemy = new EnemyCombatant(enemyGO, "Гоблин", 1);

        UIManager.Instance.Initialize(
            _player.HealthComponent,
            _enemy.HealthComponent,
            _player.StatsComponent,
            _player.InventoryComponent,
            _player.SpellManagerComponent);

        _allies.Add(_player);
        _enemies.Add(_enemy);

        _turnSystem = new ClassicTurnSystem();
        _battle = new Battle();
        _battle.Start(_allies.Concat(_enemies).ToList(), _turnSystem);

        _battle.OnTurnStart += HandleTurnStart;
        _battle.OnDamageDealt += HandleDamageDealt;
        _battle.OnKO += HandleKO;
        _battle.OnBattleEnd += HandleBattleEnd;

        IsBattleActive = true;
        _battle.BeginNextTurn();
    }

    // ======== ОБРАБОТЧИКИ СОБЫТИЙ ========

    private void HandleTurnStart(TurnEvent e)
    {
        ICombatant actor = e.Actor;

        if (actor == _player)
        {
            EventBus.Raise(new GameEvents.ShowActionPanelEvent());
            return;
        }

        if (actor is EnemyCombatant enemy && _player.IsAlive)
        {
            var damage = enemy.StatsComponent.Strength + Random.Range(0, 5);
            _battle.DealDamage(enemy, _player, damage);
            _battle.EndTurn();
        }
    }

    private void HandleDamageDealt(DamageEvent e)
    {
        // Поднимаем событие для других систем (например, будущих VFX)
        EventBus.Raise(new GameEvents.DamageEvent
        {
            SourceName = e.Source.Name,
            TargetName = e.Target.Name,
            Amount = e.Amount
        });
    }

    private void HandleKO(KOEvent e)
    {
        EventBus.Raise(new GameEvents.KOEvent
        {
            TargetName = e.Target.Name,
            IsPlayer = e.Target == _player
        });

        if (!_player.IsAlive)
        {
            _battle.EndBattle(BattleState.Defeat);
        }
        else if (!_enemy.IsAlive)
        {
            _battle.EndBattle(BattleState.Victory);
        }
    }

    private void HandleBattleEnd(Battle battle, BattleState state)
    {
        IsBattleActive = false;

        EventBus.Raise(new GameEvents.BattleEndEvent
        {
            State = state
        });

        _battle.OnTurnStart -= HandleTurnStart;
        _battle.OnDamageDealt -= HandleDamageDealt;
        _battle.OnKO -= HandleKO;
        _battle.OnBattleEnd -= HandleBattleEnd;
    }

    // ======== ДЕЙСТВИЯ ИГРОКА ========

    /// <summary>Игрок выполняет базовую атаку.</summary>
    public void PlayerAttack()
    {
        if (!IsBattleActive)
        {
            return;
        }

        var damage = _player.StatsComponent.Strength + Random.Range(0, 5);
        _battle.DealDamage(_player, _enemy, damage);
        _battle.EndTurn();

        TryStartNextTurn("атака");
    }

    /// <summary>Игрок защищается (пропускает ход).</summary>
    public void PlayerDefend()
    {
        if (!IsBattleActive)
        {
            return;
        }

        EventBus.Raise(new GameEvents.ShowMessageEvent
        {
            Text = "Герой защищается!"
        });

        _battle.EndTurn();

        TryStartNextTurn("защита");

    }

    /// <summary>Игрок использует предмет из инвентаря.</summary>
    /// <param name="item">Используемый предмет.</param>
    public void PlayerUseItem(ItemData item)
    {
        if (!IsBattleActive)
        {
            return;
        }

        if (item == null)
        {
            return;
        }

        if (!_player.InventoryComponent.Items.Contains(item))
        {
            return;
        }

        if (!TryApplyEffect(item.Effect, _player))
        {
            return;
        }

        _player.InventoryComponent.RemoveItem(item);
        _battle.EndTurn();

        TryStartNextTurn("предмет");
    }

    /// <summary>Игрок применяет заклинание.</summary>
    /// <param name="spell">Применяемое заклинание.</param>
    public void PlayerCastSpell(SpellData spell)
    {
        if (!IsBattleActive)
        {
            return;
        }

        if (spell == null)
        {
            return;
        }

        if (!_player.SpellManagerComponent.Spells.Contains(spell))
        {
            return;
        }

        if (_player.Mana < spell.ManaCost)
        {
            EventBus.Raise(new GameEvents.ShowMessageEvent
            {
                Text = "Недостаточно маны!"
            });
            return;
        }

        if (!TryApplyEffect(spell.Effect, _player))
        {
            return;
        }

        _player.UseMana(spell.ManaCost);
        _battle.EndTurn();

        TryStartNextTurn("заклинание");
    }

    // ======== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ========

    /// <summary>
    /// Определяет цель для эффекта. Возвращает null для массовых эффектов.
    /// </summary>
    private ICombatant GetTarget(Effect effect, ICombatant caster)
    {
        return effect.TargetType switch
        {
            TargetType.Self => caster,
            TargetType.Enemy => _enemies.FirstOrDefault(e => e.IsAlive),
            TargetType.Ally => _allies.FirstOrDefault(a => a.IsAlive && a != caster),
            TargetType.All => null,
            TargetType.AllEnemies => null,
            _ => null
        };
    }

    /// <summary>
    /// Применяет эффект к цели (или ко всем, если эффект массовый).
    /// Возвращает false, если не удалось подобрать цель.
    /// </summary>
    private bool TryApplyEffect(Effect effect, ICombatant caster)
    {
        ICombatant target = GetTarget(effect, caster);

        if (target != null)
        {
            effect.Apply(caster, target);
            return true;
        }

        switch (effect.TargetType)
        {
            case TargetType.All:
                foreach (ICombatant ally in _allies)
                {
                    effect.Apply(caster, ally);
                }

                return true;

            case TargetType.AllEnemies:
                foreach (ICombatant enemy in _enemies)
                {
                    effect.Apply(caster, enemy);
                }

                return true;

            default:
                Debug.LogWarning("[BattleManager] Не удалось выбрать цель для эффекта!");
                return false;
        }
    }

    /// <summary>
    /// Запускает следующий ход, если бой ждёт команды игрока.
    /// </summary>
    /// <param name="actionName">Название действия (для лога).</param>
    private void TryStartNextTurn(string actionName)
    {
        if (_battle.State != BattleState.WaitingForCommands)
        {
            return;
        }

        Debug.Log($"[BattleManager] Запускаем следующий ход ({actionName}).");
        _battle.BeginNextTurn();
    }
}

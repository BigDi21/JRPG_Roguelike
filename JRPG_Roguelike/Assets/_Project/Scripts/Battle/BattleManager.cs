using SimpleJRPG;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    public GameObject playerGO;
    public GameObject enemyGO;

    private Battle _battle;
    //private ATBTurnSystem _turnSystem;
    private ClassicTurnSystem _turnSystem;
    private List<ICombatant> _allies = new();
    private List<ICombatant> _enemies = new();

    private PlayerCombatant _player;
    private EnemyCombatant _enemy;

    public bool IsBattleActive { get; private set; }

    void Awake()
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

    void Start()
    {
        // Находим игрока и врага на сцене (по имени или по тегу)
        //GameObject playerGO = GameObject.Find("Player");
        //GameObject enemyGO = GameObject.Find("Enemy");

        if (playerGO == null || enemyGO == null)
        {
            Debug.LogError("Не найдены объекты Player или Enemy на сцене!");
            return;
        }

        StartBattle(playerGO, enemyGO);
    }

    public void StartBattle(GameObject playerGO, GameObject enemyGO)
    {
        _player = new PlayerCombatant(playerGO, "Герой", 0);
        _enemy = new EnemyCombatant(enemyGO, "Гоблин", 1);

        UIManager.Instance.Initialize(_player.HealthComponent,
                                      _enemy.HealthComponent,
                                      _player.StatsComponent,
                                      _player.InventoryComponent,
                                      _player.SpellManagerComponent);

        _allies.Add(_player);
        _enemies.Add(_enemy);

        //_turnSystem = new ATBTurnSystem();
        _turnSystem = new ClassicTurnSystem();
        _battle = new Battle();
        _battle.Start(_allies.Concat(_enemies).ToList(), _turnSystem);

        // Подписка с правильными сигнатурами
        _battle.OnTurnStart += OnTurnStart;
        _battle.OnDamageDealt += OnDamageDealt;
        _battle.OnKO += OnKO;
        _battle.OnBattleEnd += OnBattleEnd; // теперь соответствует делегату

        IsBattleActive = true;
        _battle.BeginNextTurn();
    }

    // ======== ОБРАБОТЧИКИ СОБЫТИЙ ========

    void OnTurnStart(TurnEvent e)
    {
        var actor = e.Actor;
        if (actor == _player)
        {
            if (UIManager.Instance == null) Debug.LogError("UIManager.Instance == null!");
            UIManager.Instance.ShowActionPanel();
        }
        else
        {
            var enemy = actor as EnemyCombatant;
            if (enemy != null && _player.IsAlive)
            {
                int damage = enemy.StatsComponent.Strength + Random.Range(0, 5);
                _battle.DealDamage(enemy, _player, damage);
                _battle.EndTurn();
            }
        }
    }

    void OnDamageDealt(DamageEvent e)
    {
        UIManager.Instance.ShowMessage($"{e.Source.Name} нанёс {e.Amount} урона {e.Target.Name}!");
    }

    void OnKO(KOEvent e)
    {
        UIManager.Instance.ShowMessage($"{e.Target.Name} повержен!");
        if (!_player.IsAlive) _battle.EndBattle(BattleState.Defeat);
        else if (!_enemy.IsAlive) _battle.EndBattle(BattleState.Victory);
    }

    void OnBattleEnd(Battle battle, BattleState state) // два параметра!
    {
        IsBattleActive = false;
        UIManager.Instance.ShowBattleResult(state);

        _battle.OnTurnStart -= OnTurnStart;
        _battle.OnDamageDealt -= OnDamageDealt;
        _battle.OnKO -= OnKO;
        _battle.OnBattleEnd -= OnBattleEnd;
    }

    // ======== ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ ВЫБОРА ЦЕЛИ ========

    private ICombatant GetTarget(Effect effect, ICombatant caster)
    {
        switch (effect.targetType)
        {
            case TargetType.Self:
                return caster;

            case TargetType.Enemy:
                return _enemies.FirstOrDefault(e => e.IsAlive);

            case TargetType.Ally:
                return _allies.FirstOrDefault(a => a.IsAlive && a != caster);

            // Массовые эффекты обрабатываются отдельно (возвращаем null)
            case TargetType.All:
            case TargetType.AllEnemies:
                return null;

            default:
                return null;
        }
    }

    // ======== ДЕЙСТВИЯ ИГРОКА ========

    public void PlayerAttack()
    {
        if (!IsBattleActive) return;
        int damage = _player.StatsComponent.Strength + Random.Range(0, 5);
        _battle.DealDamage(_player, _enemy, damage);
        _battle.EndTurn();

        if (_battle.State == BattleState.WaitingForCommands)
        {
            Debug.Log("Запускаем следующий ход (атака)");
            _battle.BeginNextTurn();
        }
    }

    public void PlayerDefend()
    {
        UIManager.Instance.ShowMessage("Герой защищается!");
        _battle.EndTurn();

        if (_battle.State == BattleState.WaitingForCommands)
        {
            Debug.Log("Запускаем следующий ход (защита)");
            _battle.BeginNextTurn();
        }
    }

    // ======== ИСПОЛЬЗОВАНИЕ ПРЕДМЕТА ========

    public void PlayerUseItem(ItemData item)
    {
        if (!IsBattleActive) return;
        if (!_player.InventoryComponent.Items.Contains(item)) return;

        var target = GetTarget(item.effect, _player);

        if (target != null)
        {
            item.effect.Apply(_player, target);
            _player.InventoryComponent.RemoveItem(item);
        }
        else
        {
            // Массовые эффекты
            if (item.effect.targetType == TargetType.All)
            {
                foreach (var ally in _allies)
                    item.effect.Apply(_player, ally);
            }
            else if (item.effect.targetType == TargetType.AllEnemies)
            {
                foreach (var enemy in _enemies)
                    item.effect.Apply(_player, enemy);
            }
            else
            {
                Debug.LogWarning("Не удалось выбрать цель для предмета!");
                return;
            }
            _player.InventoryComponent.RemoveItem(item);
        }

        _battle.EndTurn();
        if (_battle.State == BattleState.WaitingForCommands)
        {
            Debug.Log("Запускаем следующий ход (предмет)");
            _battle.BeginNextTurn();
        }
    }

    // ======== ИСПОЛЬЗОВАНИЕ ЗАКЛИНАНИЯ ========

    public void PlayerCastSpell(SpellData spell)
    {
        if (!IsBattleActive) return;
        if (!_player.SpellManagerComponent.Spells.Contains(spell)) return;

        if (_player.Mana < spell.manaCost)
        {
            UIManager.Instance.ShowMessage("Недостаточно маны!");
            return;
        }

        var target = GetTarget(spell.effect, _player);

        if (target != null)
        {
            spell.effect.Apply(_player, target);
        }
        else
        {
            if (spell.effect.targetType == TargetType.All)
            {
                foreach (var ally in _allies)
                    spell.effect.Apply(_player, ally);
            }
            else if (spell.effect.targetType == TargetType.AllEnemies)
            {
                foreach (var enemy in _enemies)
                    spell.effect.Apply(_player, enemy);
            }
            else
            {
                Debug.LogWarning("Не удалось выбрать цель для заклинания!");
                return;
            }
        }

        _player.UseMana(spell.manaCost);
        _battle.EndTurn();
        if (_battle.State == BattleState.WaitingForCommands)
        {
            Debug.Log("Запускаем следующий ход (заклинание)");
            _battle.BeginNextTurn();
        }
    }
}
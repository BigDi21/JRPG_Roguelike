using System.Collections.Generic;
using SimpleJRPG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управляет боевым UI: панелями действий, инвентарём, заклинаниями,
/// полосами HP/маны и сообщениями. Подписан на события EventBus.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Панели")]
    [SerializeField] private GameObject _actionPanel;
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private GameObject _spellPanel;
    [SerializeField] private TMP_Text _messageText;

    [Header("Полосы здоровья")]
    [SerializeField] private Slider _playerHealthSlider;
    [SerializeField] private Slider _enemyHealthSlider;
    [SerializeField] private TMP_Text _playerHealthText;
    [SerializeField] private TMP_Text _enemyHealthText;

    [Header("Полоса маны")]
    [SerializeField] private Slider _playerManaSlider;
    [SerializeField] private TMP_Text _playerManaText;

    [Header("Инвентарь и заклинания")]
    [SerializeField] private Transform _inventoryContent;
    [SerializeField] private Transform _spellContent;
    [SerializeField] private GameObject _itemButtonPrefab;
    [SerializeField] private GameObject _spellButtonPrefab;

    private HealthComponent _playerHealth;
    private HealthComponent _enemyHealth;
    private StatsComponent _playerStats;
    private InventoryComponent _playerInventory;
    private SpellManagerComponent _playerSpells;

    // ======== ЖИЗНЕННЫЙ ЦИКЛ ========

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

    private void OnEnable()
    {
        EventBus.Subscribe<GameEvents.DamageEvent>(OnDamageEvent);
        EventBus.Subscribe<GameEvents.KOEvent>(OnKOEvent);
        EventBus.Subscribe<GameEvents.BattleEndEvent>(OnBattleEndEvent);
        EventBus.Subscribe<GameEvents.ShowMessageEvent>(OnShowMessage);
        EventBus.Subscribe<GameEvents.ShowActionPanelEvent>(OnShowActionPanel);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameEvents.DamageEvent>(OnDamageEvent);
        EventBus.Unsubscribe<GameEvents.KOEvent>(OnKOEvent);
        EventBus.Unsubscribe<GameEvents.BattleEndEvent>(OnBattleEndEvent);
        EventBus.Unsubscribe<GameEvents.ShowMessageEvent>(OnShowMessage);
        EventBus.Unsubscribe<GameEvents.ShowActionPanelEvent>(OnShowActionPanel);
    }

    private void Start()
    {
        SetPanelsActive(false, false, false);

        if (_messageText != null)
        {
            _messageText.text = string.Empty;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromHealthEvents();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    // ======== ИНИЦИАЛИЗАЦИЯ ========

    /// <summary>
    /// Привязывает UI к компонентам игрока и врага.
    /// </summary>
    public void Initialize(
        HealthComponent playerHealth,
        HealthComponent enemyHealth,
        StatsComponent playerStats,
        InventoryComponent inventory,
        SpellManagerComponent spells)
    {
        _playerHealth = playerHealth;
        _enemyHealth = enemyHealth;
        _playerStats = playerStats;
        _playerInventory = inventory;
        _playerSpells = spells;

        UpdateHealthUI();
        UpdateManaUI();
        PopulateInventoryUI();
        PopulateSpellUI();

        SubscribeToHealthEvents();
    }

    private void SubscribeToHealthEvents()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged += OnHealthChanged;
        }

        if (_enemyHealth != null)
        {
            _enemyHealth.OnHealthChanged += OnHealthChanged;
        }
    }

    private void UnsubscribeFromHealthEvents()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged -= OnHealthChanged;
        }

        if (_enemyHealth != null)
        {
            _enemyHealth.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(int current, int max) => UpdateHealthUI();

    // ======== ОБРАБОТЧИКИ СОБЫТИЙ EVENTBUS ========

    private void OnDamageEvent(GameEvents.DamageEvent e) => ShowMessage($"{e.SourceName} нанёс {e.Amount} урона {e.TargetName}!");

    private void OnKOEvent(GameEvents.KOEvent e) => ShowMessage($"{e.TargetName} повержен!");

    private void OnBattleEndEvent(GameEvents.BattleEndEvent e) => ShowBattleResult(e.State);

    private void OnShowMessage(GameEvents.ShowMessageEvent e) => ShowMessage(e.Text);

    private void OnShowActionPanel(GameEvents.ShowActionPanelEvent e) => ShowActionPanel();

    // ======== ОБНОВЛЕНИЕ UI ========

    /// <summary>
    /// Обновляет полосы и тексты здоровья игрока и врага.
    /// </summary>
    public void UpdateHealthUI()
    {
        UpdatePlayerHealthUI();
        UpdateEnemyHealthUI();
    }

    private void UpdatePlayerHealthUI()
    {
        if (_playerHealth == null)
        {
            return;
        }

        if (_playerHealthSlider != null && _playerHealth.MaxHealth > 0)
        {
            _playerHealthSlider.value = (float)_playerHealth.CurrentHealth / _playerHealth.MaxHealth;
        }

        if (_playerHealthText != null)
        {
            _playerHealthText.text = $"{_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}";
        }
    }

    private void UpdateEnemyHealthUI()
    {
        if (_enemyHealth == null)
        {
            return;
        }

        if (_enemyHealthSlider != null && _enemyHealth.MaxHealth > 0)
        {
            _enemyHealthSlider.value = (float)_enemyHealth.CurrentHealth / _enemyHealth.MaxHealth;
        }

        if (_enemyHealthText != null)
        {
            _enemyHealthText.text = $"{_enemyHealth.CurrentHealth}/{_enemyHealth.MaxHealth}";
        }
    }

    /// <summary>
    /// Обновляет полосу и текст маны игрока.
    /// </summary>
    public void UpdateManaUI()
    {
        if (_playerStats == null)
        {
            return;
        }

        if (_playerManaSlider != null && _playerStats.MaxMana > 0)
        {
            _playerManaSlider.value = (float)_playerStats.Mana / _playerStats.MaxMana;
        }

        if (_playerManaText != null)
        {
            _playerManaText.text = $"{_playerStats.Mana}/{_playerStats.MaxMana}";
        }
    }

    // ======== НАПОЛНЕНИЕ СПИСКОВ ========

    private void PopulateInventoryUI()
    {
        ClearChildren(_inventoryContent);

        if (_playerInventory == null || _itemButtonPrefab == null || _inventoryContent == null)
        {
            return;
        }

        foreach (ItemData item in _playerInventory.Items)
        {
            Button button = CreateButton(_itemButtonPrefab, _inventoryContent);
            if (button == null)
            {
                continue;
            }

            SetButtonText(button, item.ItemName);
            button.onClick.AddListener(() => BattleManager.Instance.PlayerUseItem(item));
        }
    }

    private void PopulateSpellUI()
    {
        ClearChildren(_spellContent);

        if (_playerSpells == null || _spellButtonPrefab == null || _spellContent == null)
        {
            return;
        }

        foreach (SpellData spell in _playerSpells.Spells)
        {
            Button button = CreateButton(_spellButtonPrefab, _spellContent);
            if (button == null)
            {
                continue;
            }

            SetButtonText(button, $"{spell.SpellName} (MP: {spell.ManaCost})");
            button.onClick.AddListener(() => BattleManager.Instance.PlayerCastSpell(spell));
        }
    }

    private static Button CreateButton(GameObject prefab, Transform parent)
    {
        GameObject instance = Instantiate(prefab, parent);
        return instance.GetComponent<Button>();
    }

    private static void SetButtonText(Button button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = text;
        }
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    // ======== ПАНЕЛИ ========

    /// <summary>
    /// Показывает панель действий игрока.
    /// </summary>
    public void ShowActionPanel()
    {
        if (_actionPanel != null)
        {
            _actionPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Скрывает панель действий игрока.
    /// </summary>
    public void HideActionPanel()
    {
        if (_actionPanel != null)
        {
            _actionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Переключает видимость панели инвентаря.
    /// </summary>
    public void ShowInventory()
    {
        if (_inventoryPanel == null)
        {
            return;
        }

        _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);

        if (_inventoryPanel.activeSelf)
        {
            PopulateInventoryUI();
        }
    }

    /// <summary>
    /// Переключает видимость панели заклинаний.
    /// </summary>
    public void ShowSpells()
    {
        if (_spellPanel == null)
        {
            return;
        }

        _spellPanel.SetActive(!_spellPanel.activeSelf);

        if (_spellPanel.activeSelf)
        {
            PopulateSpellUI();
        }
    }

    // ======== СООБЩЕНИЯ ========

    /// <summary>
    /// Показывает текстовое сообщение в UI.
    /// </summary>
    public void ShowMessage(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }

    /// <summary>
    /// Показывает результат боя и скрывает все панели действий.
    /// </summary>
    public void ShowBattleResult(BattleState state)
    {
        var result = state switch
        {
            BattleState.Victory => "Победа!",
            BattleState.Defeat => "Поражение...",
            _ => string.Empty
        };

        if (_messageText != null)
        {
            _messageText.text = result;
        }

        SetPanelsActive(false, false, false);
    }

    /// <summary>
    /// Обновляет все элементы UI (HP, мана, инвентарь, заклинания).
    /// </summary>
    public void RefreshUI()
    {
        UpdateHealthUI();
        UpdateManaUI();
        PopulateInventoryUI();
        PopulateSpellUI();
    }

    // ======== ВСПОМОГАТЕЛЬНЫЕ ========

    private void SetPanelsActive(bool action, bool inventory, bool spells)
    {
        if (_actionPanel != null)
        {
            _actionPanel.SetActive(action);
        }

        if (_inventoryPanel != null)
        {
            _inventoryPanel.SetActive(inventory);
        }

        if (_spellPanel != null)
        {
            _spellPanel.SetActive(spells);
        }
    }
}

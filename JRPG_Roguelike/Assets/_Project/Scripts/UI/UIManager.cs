using SimpleJRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Панели")]
    public GameObject ActionPanel;
    public GameObject InventoryPanel;
    public GameObject SpellPanel;
    public Text MessageText;

    [Header("Полосы здоровья и маны")]
    public Slider PlayerHealthSlider;
    public Slider EnemyHealthSlider;
    public Text PlayerHealthText;
    public Text EnemyHealthText;

    public Slider PlayerManaSlider;
    public Text PlayerManaText;

    [Header("Инвентарь и заклинания")]
    public Transform InventoryContent;
    public Transform SpellContent;
    public GameObject ItemButtonPrefab;
    public GameObject SpellButtonPrefab;

    // Ссылки на компоненты персонажей
    private HealthComponent _playerHealth;
    private HealthComponent _enemyHealth;
    private StatsComponent _playerStats; // для маны
    private InventoryComponent _playerInventory;
    private SpellManagerComponent _playerSpells;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ActionPanel.SetActive(false);
        InventoryPanel.SetActive(false);
        SpellPanel.SetActive(false);
        MessageText.text = "";
    }

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

        // Подписка на события здоровья
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged += (cur, max) => UpdateHealthUI();
        if (_enemyHealth != null)
            _enemyHealth.OnHealthChanged += (cur, max) => UpdateHealthUI();

        // Подписка на изменение маны (если есть событие в StatsComponent)
        // Если нет — обновляем вручную при каждом использовании.
    }

    void UpdateHealthUI()
    {
        if (_playerHealth != null)
        {
            PlayerHealthSlider.value = (float)_playerHealth.CurrentHealth / _playerHealth.MaxHealth;
            PlayerHealthText.text = $"{_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}";
        }
        if (_enemyHealth != null)
        {
            EnemyHealthSlider.value = (float)_enemyHealth.CurrentHealth / _enemyHealth.MaxHealth;
            EnemyHealthText.text = $"{_enemyHealth.CurrentHealth}/{_enemyHealth.MaxHealth}";
        }
    }

    public void UpdateManaUI()
    {
        if (_playerStats != null)
        {
            PlayerManaSlider.value = (float)_playerStats.Mana / _playerStats.MaxMana;
            PlayerManaText.text = $"{_playerStats.Mana}/{_playerStats.MaxMana}";
        }
    }

    void PopulateInventoryUI()
    {
        // Очищаем старые кнопки
        foreach (Transform child in InventoryContent) Destroy(child.gameObject);
        if (_playerInventory == null) return;

        foreach (var item in _playerInventory.Items)
        {
            var btn = Instantiate(ItemButtonPrefab, InventoryContent).GetComponent<Button>();
            var btnText = btn.GetComponentInChildren<Text>();
            btnText.text = $"{item.itemName}";

            // Если есть иконка, можно её добавить
            // var icon = btn.GetComponentInChildren<Image>();
            // if (icon != null && item.icon != null) icon.sprite = item.icon;

            btn.onClick.AddListener(() => BattleManager.Instance.PlayerUseItem(item));
        }
    }

    void PopulateSpellUI()
    {
        foreach (Transform child in SpellContent) Destroy(child.gameObject);
        if (_playerSpells == null) return;

        foreach (var spell in _playerSpells.Spells)
        {
            var btn = Instantiate(SpellButtonPrefab, SpellContent).GetComponent<Button>();
            var btnText = btn.GetComponentInChildren<Text>();
            btnText.text = $"{spell.spellName} (MP: {spell.manaCost})";

            btn.onClick.AddListener(() => BattleManager.Instance.PlayerCastSpell(spell));
        }
    }

    public void ShowActionPanel() => ActionPanel.SetActive(true);
    public void HideActionPanel() => ActionPanel.SetActive(false);

    public void ShowInventory()
    {
        InventoryPanel.SetActive(!InventoryPanel.activeSelf);
        if (InventoryPanel.activeSelf) PopulateInventoryUI();
    }

    public void ShowSpells()
    {
        SpellPanel.SetActive(!SpellPanel.activeSelf);
        if (SpellPanel.activeSelf) PopulateSpellUI();
    }

    public void ShowMessage(string msg) => MessageText.text = msg;

    public void ShowBattleResult(BattleState state)
    {
        string result = state == BattleState.Victory ? "Победа!" : "Поражение...";
        MessageText.text = result;
        ActionPanel.SetActive(false);
        InventoryPanel.SetActive(false);
        SpellPanel.SetActive(false);
    }

    // Метод для принудительного обновления всего UI (можно вызывать после хода)
    public void RefreshUI()
    {
        UpdateHealthUI();
        UpdateManaUI();
        PopulateInventoryUI();
        PopulateSpellUI();
    }
}
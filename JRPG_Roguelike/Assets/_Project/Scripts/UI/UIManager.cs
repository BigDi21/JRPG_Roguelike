using SimpleJRPG;
using System.Collections.Generic;
using TMPro; // <-- важно!
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Панели")]
    public GameObject ActionPanel;
    public GameObject InventoryPanel;
    public GameObject SpellPanel;
    public TMP_Text MessageText; // заменено на TMP_Text

    [Header("Полосы здоровья и маны")]
    public Slider PlayerHealthSlider;
    public Slider EnemyHealthSlider;
    public TMP_Text PlayerHealthText; // TMP_Text
    public TMP_Text EnemyHealthText;  // TMP_Text

    public Slider PlayerManaSlider;
    public TMP_Text PlayerManaText;   // TMP_Text

    [Header("Инвентарь и заклинания")]
    public Transform InventoryContent;
    public Transform SpellContent;
    public GameObject ItemButtonPrefab;
    public GameObject SpellButtonPrefab;

    // Ссылки на компоненты персонажей
    private HealthComponent _playerHealth;
    private HealthComponent _enemyHealth;
    private StatsComponent _playerStats;
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
        if (MessageText != null) MessageText.text = "";
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

        if (_playerHealth != null)
            _playerHealth.OnHealthChanged += (cur, max) => UpdateHealthUI();
        if (_enemyHealth != null)
            _enemyHealth.OnHealthChanged += (cur, max) => UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (_playerHealth != null)
        {
            PlayerHealthSlider.value = (float)_playerHealth.CurrentHealth / _playerHealth.MaxHealth;
            if (PlayerHealthText != null)
                PlayerHealthText.text = $"{_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}";
        }
        if (_enemyHealth != null)
        {
            EnemyHealthSlider.value = (float)_enemyHealth.CurrentHealth / _enemyHealth.MaxHealth;
            if (EnemyHealthText != null)
                EnemyHealthText.text = $"{_enemyHealth.CurrentHealth}/{_enemyHealth.MaxHealth}";
        }
    }

    public void UpdateManaUI()
    {
        if (_playerStats != null && PlayerManaText != null)
        {
            PlayerManaSlider.value = (float)_playerStats.Mana / _playerStats.MaxMana;
            PlayerManaText.text = $"{_playerStats.Mana}/{_playerStats.MaxMana}";
        }
    }

    void PopulateInventoryUI()
    {
        foreach (Transform child in InventoryContent) Destroy(child.gameObject);
        if (_playerInventory == null) return;

        foreach (var item in _playerInventory.Items)
        {
            var btn = Instantiate(ItemButtonPrefab, InventoryContent).GetComponent<Button>();
            // Получаем TMP_Text на кнопке (или в дочернем объекте)
            var btnText = btn.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = item.itemName;

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
            var btnText = btn.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
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

    public void ShowMessage(string msg)
    {
        if (MessageText != null) MessageText.text = msg;
    }

    public void ShowBattleResult(BattleState state)
    {
        string result = state == BattleState.Victory ? "Победа!" : "Поражение...";
        if (MessageText != null) MessageText.text = result;
        ActionPanel.SetActive(false);
        InventoryPanel.SetActive(false);
        SpellPanel.SetActive(false);
    }

    public void RefreshUI()
    {
        UpdateHealthUI();
        UpdateManaUI();
        PopulateInventoryUI();
        PopulateSpellUI();
    }
}
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Центральный реестр всех ScriptableObject-ассетов игры.
/// Загружается один раз при старте и предоставляет доступ по строковому ID.
/// </summary>
[CreateAssetMenu(fileName = "GameDatabase", menuName = "Game/Database")]
public class GameDatabase : ScriptableObject
{
    [Header("Items")]
    [SerializeField] private List<ItemData> _items = new();

    [Header("Spells")]
    [SerializeField] private List<SpellData> _spells = new();

    private Dictionary<string, ItemData> _itemMap;
    private Dictionary<string, SpellData> _spellMap;

    private bool _isInitialized;

    /// <summary>Все предметы в базе.</summary>
    public IEnumerable<ItemData> AllItems => _items;

    /// <summary>Все заклинания в базе.</summary>
    public IEnumerable<SpellData> AllSpells => _spells;

    /// <summary>
    /// Инициализирует словари и проверяет целостность данных.
    /// Вызывается один раз из GameManager.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("[GameDatabase] Уже инициализирована!");
            return;
        }

        _itemMap = new Dictionary<string, ItemData>();
        _spellMap = new Dictionary<string, SpellData>();

        LoadItems();
        LoadSpells();

        _isInitialized = true;

        Debug.Log($"[GameDatabase] Загружено: {_itemMap.Count} предметов, {_spellMap.Count} заклинаний");
    }

    private void LoadItems()
    {
        foreach (ItemData item in _items)
        {
            if (item == null)
            {
                Debug.LogWarning("[GameDatabase] Обнаружен null в списке предметов!");
                continue;
            }

            if (string.IsNullOrEmpty(item.Id))
            {
                Debug.LogError($"[GameDatabase] У предмета '{item.name}' не заполнен Id!");
                continue;
            }

            if (!_itemMap.TryAdd(item.Id, item))
            {
                Debug.LogError($"[GameDatabase] Дубликат Item Id: '{item.Id}'");
            }
        }
    }

    private void LoadSpells()
    {
        foreach (SpellData spell in _spells)
        {
            if (spell == null)
            {
                Debug.LogWarning("[GameDatabase] Обнаружен null в списке заклинаний!");
                continue;
            }

            if (string.IsNullOrEmpty(spell.Id))
            {
                Debug.LogError($"[GameDatabase] У заклинания '{spell.name}' не заполнен Id!");
                continue;
            }

            if (!_spellMap.TryAdd(spell.Id, spell))
            {
                Debug.LogError($"[GameDatabase] Дубликат Spell Id: '{spell.Id}'");
            }
        }
    }

    /// <summary>
    /// Возвращает предмет по ID. null, если не найден.
    /// </summary>
    public ItemData GetItem(string id)
    {
        if (!_isInitialized)
        {
            Debug.LogError("[GameDatabase] Попытка получить предмет до Initialize()!");
            return null;
        }

        return _itemMap.TryGetValue(id, out ItemData item) ? item : null;
    }

    /// <summary>
    /// Возвращает заклинание по ID. null, если не найдено.
    /// </summary>
    public SpellData GetSpell(string id)
    {
        if (!_isInitialized)
        {
            Debug.LogError("[GameDatabase] Попытка получить заклинание до Initialize()!");
            return null;
        }

        return _spellMap.TryGetValue(id, out SpellData spell) ? spell : null;
    }
}

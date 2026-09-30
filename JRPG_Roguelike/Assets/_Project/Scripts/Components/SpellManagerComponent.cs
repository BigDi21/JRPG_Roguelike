using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Компонент управления заклинаниями. Хранит список доступных заклинаний.
/// </summary>
public class SpellManagerComponent : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private List<SpellData> _spells = new();

    /// <summary>Список доступных заклинаний.</summary>
    public List<SpellData> Spells => _spells;

    /// <summary>
    /// Добавляет заклинание в список доступных.
    /// </summary>
    /// <param name="spell">Добавляемое заклинание.</param>
    public void AddSpell(SpellData spell) => _spells.Add(spell);

    /// <summary>
    /// Удаляет заклинание из списка доступных.
    /// </summary>
    /// <param name="spell">Удаляемое заклинание.</param>
    public void RemoveSpell(SpellData spell) => _spells.Remove(spell);
}
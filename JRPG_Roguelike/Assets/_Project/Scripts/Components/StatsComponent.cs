using UnityEngine;

/// <summary>
/// Компонент характеристик бойца. Хранит атрибуты и ресурсы (мана).
/// </summary>
public class StatsComponent : MonoBehaviour
{
    [Header("Атрибуты")]
    [SerializeField] private int _strength = 10;
    [SerializeField] private int _defense = 5;
    [SerializeField] private int _magic = 8;
    [SerializeField] private float _speed = 1.0f;

    [Header("Ресурсы")]
    [SerializeField] private int _maxMana = 50;
    [SerializeField] private int _mana = 50;

    /// <summary>Сила. Влияет на физический урон.</summary>
    public int Strength => _strength;

    /// <summary>Защита. Снижает получаемый урон.</summary>
    public int Defense => _defense;

    /// <summary>Магия. Влияет на магический урон.</summary>
    public int Magic => _magic;

    /// <summary>Скорость (инициатива).</summary>
    public float Speed => _speed;

    /// <summary>Максимальный запас маны.</summary>
    public int MaxMana => _maxMana;

    /// <summary>Текущий запас маны.</summary>
    public int Mana => _mana;

    /// <summary>
    /// Тратит указанное количество маны. Мана не опускается ниже 0.
    /// </summary>
    /// <param name="amount">Количество маны для списания.</param>
    public void UseMana(int amount)
    {
        _mana = Mathf.Max(0, _mana - amount);
    }

    /// <summary>
    /// Восстанавливает ману. Не превышает <see cref="MaxMana"/>.
    /// </summary>
    /// <param name="amount">Количество восстанавливаемой маны.</param>
    public void RestoreMana(int amount)
    {
        _mana = Mathf.Min(_maxMana, _mana + amount);
    }
}
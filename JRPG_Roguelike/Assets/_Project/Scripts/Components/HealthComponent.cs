using System;
using UnityEngine;

/// <summary>
/// Компонент здоровья. Хранит текущее и максимальное HP,
/// уведомляет подписчиков об изменениях и смерти.
/// </summary>
public class HealthComponent : MonoBehaviour
{
    /// <summary>Вызывается при изменении HP. Параметры: (текущее, максимальное).</summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>Вызывается при достижении HP = 0.</summary>
    public event Action OnDeath;

    [Header("Настройки")]
    [SerializeField] private int _maxHealth = 100;

    private int _currentHealth;

    /// <summary>Текущее здоровье.</summary>
    public int CurrentHealth => _currentHealth;

    /// <summary>Максимальное здоровье.</summary>
    public int MaxHealth => _maxHealth;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    /// <summary>
    /// Наносит урон. HP не опускается ниже 0.
    /// Если HP достигает 0 — вызывается <see cref="OnDeath"/>.
    /// </summary>
    /// <param name="amount">Количество урона.</param>
    public void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth == 0)
            OnDeath?.Invoke();
    }

    /// <summary>
    /// Восстанавливает здоровье. HP не поднимается выше <see cref="MaxHealth"/>.
    /// </summary>
    /// <param name="amount">Количество восстанавливаемого HP.</param>
    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    /// <summary>
    /// Сбрасывает HP к максимальному значению.
    /// </summary>
    public void ResetHealth()
    {
        _currentHealth = _maxHealth;
    }
}
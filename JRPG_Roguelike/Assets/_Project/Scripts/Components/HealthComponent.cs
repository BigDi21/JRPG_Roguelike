using UnityEngine;
using System;

public class HealthComponent : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    [SerializeField] 
    private int _maxHealth = 100;
    private int _currentHealth;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;

    void Awake() => _currentHealth = _maxHealth;

    public void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        if (_currentHealth == 0) OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void ResetHealth() => _currentHealth = _maxHealth;
}
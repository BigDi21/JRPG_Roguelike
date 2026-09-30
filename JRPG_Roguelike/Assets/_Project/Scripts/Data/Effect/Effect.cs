using SimpleJRPG;
using UnityEngine;

/// <summary>
/// Тип цели для эффекта.
/// </summary>
public enum TargetType
{
    /// <summary>На себя.</summary>
    Self,

    /// <summary>На одного врага.</summary>
    Enemy,

    /// <summary>На одного союзника.</summary>
    Ally,

    /// <summary>На всех союзников.</summary>
    All,

    /// <summary>На всех врагов.</summary>
    AllEnemies
}

/// <summary>
/// Базовый класс для всех эффектов (заклинаний, предметов).
/// </summary>
public abstract class Effect : ScriptableObject
{
    [Header("Настройки эффекта")]
    [SerializeField] private TargetType _targetType;

    /// <summary>Тип цели эффекта.</summary>
    public TargetType TargetType => _targetType;

    /// <summary>
    /// Применяет эффект к цели.
    /// </summary>
    /// <param name="user">Тот, кто применяет эффект.</param>
    /// <param name="target">Цель эффекта.</param>
    public abstract void Apply(ICombatant user, ICombatant target);
}
using SimpleJRPG;
using UnityEngine;

public enum TargetType
{
    Self,      // на себя
    Enemy,     // на врага (одного)
    Ally,      // на союзника (одного)
    All,       // на всех (союзников)
    AllEnemies // на всех врагов
}

public abstract class Effect : ScriptableObject
{
    public TargetType targetType;
    public abstract void Apply(ICombatant user, ICombatant target);
}
/// <summary>
/// Все события игры. Структуры, передаваемые через EventBus.
/// </summary>
public static class GameEvents
{
    // ======== БОЙ ========

    /// <summary>Урон нанесён.</summary>
    public struct DamageEvent
    {
        public string SourceName;
        public string TargetName;
        public int Amount;
        public bool IsCrit;
        public bool IsBlocked;
        public bool IsDodged;
    }

    /// <summary>Здоровье восстановлено.</summary>
    public struct HealEvent
    {
        public string TargetName;
        public int Amount;
    }

    /// <summary>Боец повержен.</summary>
    public struct KOEvent
    {
        public string TargetName;
        public bool IsPlayer;
    }

    /// <summary>Начался ход бойца.</summary>
    public struct TurnStartEvent
    {
        public string ActorName;
        public bool IsPlayer;
    }

    /// <summary>Бой завершён.</summary>
    public struct BattleEndEvent
    {
        public bool IsVictory;
    }

    // ======== ПРОГРЕССИЯ ========

    /// <summary>Предмет подобран.</summary>
    public struct ItemPickedUpEvent
    {
        public string ItemId;
        public int Amount;
    }

    /// <summary>Навык открыт.</summary>
    public struct SkillUnlockedEvent
    {
        public string SkillId;
    }

    // ======== ИГРОК ========

    /// <summary>Игрок переместился.</summary>
    public struct PlayerMovedEvent
    {
        public int GridX;
        public int GridY;
    }

    /// <summary>Игрок погиб.</summary>
    public struct PlayerDiedEvent
    {
        public string ZoneId;
    }

    // ======== КАРТА ========

    /// <summary>Клетка открыта (туман войны).</summary>
    public struct CellDiscoveredEvent
    {
        public int GridX;
        public int GridY;
    }
}

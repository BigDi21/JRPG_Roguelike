/// <summary>
/// Тип действия, которое может выполнить боец в свой ход.
/// </summary>
public enum ActionType
{
    /// <summary>Базовая атака оружием.</summary>
    Attack,

    /// <summary>Защита (пропуск хода, повышение защиты).</summary>
    Defend,

    /// <summary>Использование предмета из инвентаря.</summary>
    UseItem,

    /// <summary>Применение заклинания.</summary>
    CastSpell
}

/// <summary>
/// Тип урона. Определяет, как рассчитывается урон и какие резисты применяются.
/// </summary>
public enum DamageType
{
    /// <summary>Физический урон. Снижается защитой, блокируется уклонением.</summary>
    Physical,

    /// <summary>Магический урон. Снижается сопротивлением магии, не блокируется уклонением.</summary>
    Magical,

    /// <summary>Истинный урон. Игнорирует защиту и резисты.</summary>
    True,

    /// <summary>Кислотный урон. Накладывает статус «Яд».</summary>
    Acid
}
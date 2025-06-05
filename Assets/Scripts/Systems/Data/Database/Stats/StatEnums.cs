using UnityEngine;

namespace Lineage.Ancestral.Legacies.Database
{
    #region Stats Module Enums

    /// <summary>
    /// Unique identifiers for predefined stat types.
    /// </summary>
    public enum StatID
    {
        Health = 0,
        Mana = 1,
        Stamina = 2,
        Strength = 3,
        Agility = 4,
        Intelligence = 5,
        Defense = 6,
        Speed = 7,
        CriticalHitChance = 8,
        CriticalHitDamage = 9,
        Attack = 10,
        MagicPower = 11,
        MagicDefense = 12,
        Experience = 13,
        Level = 14,
        Luck = 15,
        Charisma = 16,
        // Needs System Stats
        Hunger = 17,
        Thirst = 18,
        Energy = 19,
        Rest = 20
    }

    /// <summary>
    /// Defines the category of a stat (e.g., primary, secondary).
    /// </summary>
    public enum StatType
    {
        Primary,   // Core stats like Health, Mana, etc.
        Secondary, // Derived stats like Attack, Defense, etc.
        Tertiary   // Miscellaneous stats like Experience, Level, etc.
    }

    #endregion
}

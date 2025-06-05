using UnityEngine;

namespace Lineage.Ancestral.Legacies.Database
{
    #region Global Enums

    /// <summary>
    /// Defines the rarity levels for various game elements.
    /// </summary>
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Specifies the type of element a rarity level can apply to.
    /// </summary>
    public enum RarityType
    {
        Entity, // For entities like monsters, NPCs, etc.
        Item, // For items like weapons, armor, etc.
        Skill, // For skills that can be learned or used
        Buff, // For buffs that can be applied to entities
        Quest // For quests that can be undertaken
    }

    /// <summary>
    /// Defines the importance levels for lore entries.
    /// </summary>
    public enum LoreImportance
    {
        Minor,
        Moderate,
        Important,
        Critical,
        Legendary
    }

    /// <summary>
    /// Defines the importance levels for timeline events.
    /// </summary>
    public enum EventImportance
    {
        Minor,
        Moderate,
        Important,
        Critical,
        WorldChanging
    }

    /// <summary>
    /// Defines creature types for classification.
    /// </summary>
    public enum CreatureType
    {
        Humanoid,
        Beast,
        Undead,
        Elemental,
        Dragon,
        Fiend,
        Celestial,
        Construct,
        Plant,
        Ooze,
        Aberration
    }

    /// <summary>
    /// Defines gender options for entities.
    /// </summary>
    public enum Gender
    {
        Male,
        Female,
        NonBinary,
        Other
    }

    /// <summary>
    /// Defines skill categories and types.
    /// </summary>
    public enum SkillType
    {
        Combat,
        Crafting,
        Magic,
        Survival,
        Social,
        Stealth,
        Athletics,
        Medicine,
        Engineering,
        Art,
        Music,
        Leadership,
        Trade,
        Exploration,
        Hunting,
        Fishing,
        Cooking,
        Alchemy,
        Enchanting,
        Blacksmithing
    }

    /// <summary>
    /// Defines buff types and categories.
    /// </summary>
    public enum BuffType
    {
        Temporary,
        Permanent,
        Conditional,
        Stackable,
        Unique
    }

    /// <summary>
    /// Defines quest status values.
    /// </summary>
    public enum QuestStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed,
        Abandoned
    }    /// <summary>
    /// Defines item types for classification.
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Material,
        Tool,
        QuestItem,
        Currency,
        Accessory,
        Container,
        Book
    }

    /// <summary>
    /// Defines item rarity levels for classification.
    /// </summary>
    public enum ItemRarity
    {
        Broken,
        Poor,
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Artifact
    }

    /// <summary>
    /// Defines item quality levels for degradation system.
    /// </summary>
    public enum ItemQuality
    {
        Destroyed,
        Broken,
        Damaged,
        Worn,
        Good,
        Fine,
        Excellent,
        Perfect
    }

    /// <summary>
    /// Defines entity archetype categories.
    /// </summary>
    public enum EntityArchetype
    {
        Player,
        NPC,
        Merchant,
        Guard,
        Civilian,
        Noble,
        Warrior,
        Mage,
        Rogue,
        Healer,
        Crafter,
        Scholar
    }

    /// <summary>
    /// Defines NPC behavior types.
    /// </summary>
    public enum NPCBehaviorType
    {
        Passive,
        Aggressive,
        Defensive,
        Neutral,
        Friendly,
        Hostile,
        Territorial,
        Cowardly
    }    /// <summary>
    /// Defines location types for world mapping.
    /// </summary>
    public enum LocationType
    {
        Settlement,
        Dungeon,
        Wilderness,
        Structure,
        Landmark,
        Resource,
        Hazard,
        Portal
    }

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

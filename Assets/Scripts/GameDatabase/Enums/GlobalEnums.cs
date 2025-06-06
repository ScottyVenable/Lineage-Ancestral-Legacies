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
    }    /// <summary>
    /// Defines item types in the game.
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Tool,
        Material,
        Accessory,
        Quest,
        QuestItem, // Alias for Quest
        Currency,
        Misc
    }

    /// <summary>
    /// Defines item rarity levels.
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Artifact
    }    /// <summary>
    /// Defines item quality levels.
    /// </summary>
    public enum ItemQuality
    {
        Poor,
        Fair, // Added for compatibility
        Normal,
        Good,
        Superior,
        Excellent, // Added for compatibility
        Exceptional,
        Masterwork
    }

    /// <summary>
    /// Defines buff/debuff types.
    /// </summary>
    public enum BuffType
    {
        Beneficial,
        Detrimental,
        Neutral,
        Temporary,
        Permanent,
        Stackable
    }

    /// <summary>
    /// Defines different types of statistics.
    /// </summary>
    public enum StatType
    {
        Core,
        Combat,
        Social,
        Survival,
        Crafting,
        Magic,
        Physical
    }

    /// <summary>
    /// Defines skill types and categories.
    /// </summary>
    public enum SkillType
    {
        Combat,
        Crafting,
        Social,
        Survival,
        Magic,
        Physical,
        Mental,
        Artistic
    }

    public enum EquipSlot
    {
        Head,
        Chest,
        Legs,
        Feet,
        Hands,
        Ring,
        Back,
        Weapon,
        Shield,
        Accessory
    }

    /// <summary>
    /// Defines genetic trait types.
    /// </summary>
    public enum GeneType
    {
        Physical,
        Mental,
        Social,
        Magical,
        Survival,
        Combat
    }

    /// <summary>
    /// Defines NPC archetypes for behavior patterns.
    /// </summary>
    public enum Archetype
    {
        Warrior,
        Mage,
        Rogue,
        Healer,
        Merchant,
        Noble,
        Commoner,
        Scholar,
        Artisan,
        Leader
    }

    /// <summary>
    /// Defines journal entry types.
    /// </summary>
    public enum EntryType
    {
        Discovery,
        Event,
        Character,
        Location,
        Quest,
        Lore,
        Personal
    }

    /// <summary>
    /// Defines quest status.
    /// </summary>
    public enum Status
    {
        NotStarted,
        InProgress,
        Completed,
        Failed,
        Abandoned
    }

    /// <summary>
    /// Defines difficulty levels.
    /// </summary>
    public enum Difficulty
    {
        Trivial,
        Easy,
        Normal,
        Hard,
        Extreme,
        Legendary
    }

    /// <summary>
    /// Defines legacy categories for lore classification.
    /// </summary>
    public enum LegacyCategory
    {
        History,
        Culture,
        Technology,
        Magic,
        Religion,
        Politics,
        Geography,
        Biology,
        Personal,
        Mystery
    }

    #endregion
}

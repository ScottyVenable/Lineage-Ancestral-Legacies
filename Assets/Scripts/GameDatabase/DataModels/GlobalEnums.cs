using UnityEngine;
using System.Collections.Generic;
using System;

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

    #endregion
}

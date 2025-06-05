using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    #region Stat System

    /// <summary>
    /// Represents stat modifiers for temporary or permanent effects, including flat and percentage bonuses.
    /// </summary>
    public struct StatModifiers
    {
        public Stat stat; // Optional reference to the stat this modifier applies to, if needed

        /// <summary>
        /// The flat value to be added to a base stat.
        /// </summary>
        public readonly float flatModifier;
        /// <summary>
        /// The percentage value to modify a base stat (e.g., 0.1 for a 10% increase).
        /// </summary>
        public readonly float percentageModifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatModifiers"/> struct.
        /// </summary>
        /// <param name="flat">The flat modifier value.</param>
        /// <param name="percentage">The percentage modifier value (e.g., 0.1 for 10%).</param>
        public StatModifiers(float flat = 0f, float percentage = 0f)
        {
            this.stat = default;
            this.flatModifier = flat;
            this.percentageModifier = percentage;
        }

        /// <summary>
        /// Applies the modifiers to a base stat value.
        /// </summary>
        /// <param name="baseValue">The base stat value to modify.</param>
        /// <returns>The modified stat value.</returns>
        public float ApplyTo(float baseValue)
        {
            return (baseValue + flatModifier) * (1f + percentageModifier);
        }

        /// <summary>
        /// Combines two StatModifiers instances.
        /// </summary>
        /// <param name="a">The first StatModifiers.</param>
        /// <param name="b">The second StatModifiers.</param>
        /// <returns>A new StatModifiers instance with combined flat and percentage modifiers.</returns>
        public static StatModifiers operator +(StatModifiers a, StatModifiers b)
        {
            return new StatModifiers(
                a.flatModifier + b.flatModifier,
                a.percentageModifier + b.percentageModifier
            );
        }
    }

    /// <summary>
    /// Represents a specific statistic for an entity, such as Health, Mana, or Attack
    /// </summary>
    public struct Stat
    {
        public enum ID
        {
            Health,
            Mana,
            Attack,
            Defense,
            Speed,
            Stamina,
            Strength,
            Agility,
            Intelligence,
            MagicPower,
            MagicDefense,
            CriticalHitChance,
            CriticalHitDamage,
            Luck,
            Charisma,
            Hunger,
            Thirst,
            Energy,
            Rest,
            Experience,
            Level
        }

        public ID statID;
        public string statName;
        public string statDescription;
        public StatType statType; //Example: Primary, Secondary, Tertiary
        public float baseValue;
        public float currentValue;
        public float minValue;
        public float maxValue;
    }

    #endregion

    #region Buff System

    /// <summary>
    /// Represents a buff or debuff effect that can be applied to entities.
    /// </summary>
    public struct Buff
    {
        public int buffID;
        public string buffName;
        public string buffDescription;
        public BuffType buffType;
        public float duration;
        public float strength;
        public List<string> tags = new List<string>();
        public StatModifiers statModifiers;
    }

    #endregion

    #region Skill System

    /// <summary>
    /// Represents a skill in the game. Some can be influenced by the Entities stats, items, and buffs.
    /// Skills can be used to perform actions, craft items, or gain experience.
    /// </summary>
    public struct Skill
    {
        public enum ID
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

        public ID skillID;
        public SkillType skillName;
        public SkillType skillType;
        public string skillDescription;
        public float experience;
        public int level;
        public List<string> tags;
    }

    #endregion

    #region Trait System

    public struct Trait
    {
        public enum ID
        {
            Brave,
            Cowardly,
            Strong,
            Weak,
            Intelligent,
            Foolish,
            Charismatic,
            Antisocial,
            Lucky,
            Unlucky,
            Healthy,
            Sickly,
            Fast,
            Slow,
            Magical,
            Mundane
        }

        public readonly string traitName;
        public readonly string description;
        public readonly string category;
        public ID traitID;
        public StatModifiers modifiers;
        public List<string> tags;
        public List<Trait> requiredTraits;
        public Skill.ID requiredSkill;
        public List<Item.ID> requiredItems;
        public Stat requiredStat;
    }

    #endregion

    #region Leveling System

    /// <summary>
    /// A leveling system that uses experience to determine the level of a trait.
    /// </summary>
    public struct LevelingSystem
    {
        public int currentLevel;
        public int experienceToNextLevel;
        public int maxLevel;
        public int experiencePerLevel;
        public StatModifiers modifiers;
        public List<Trait> traitRewards;
        public List<Skill> skillRewards;
        public List<Item> itemRewards;
    }

    #endregion
}

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Lineage.Ancestral.Legacies.GameData
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

    #endregion

    #region Utility Structs

    /// <summary>
    /// Represents health data with utility methods for managing current and maximum health.
    /// </summary>
    public struct Health
    {
        /// <summary>
        /// The current health value.
        /// </summary>
        public readonly float current;
        /// <summary>
        /// The maximum possible health value.
        /// </summary>
        public readonly float max;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Health"/> struct.
        /// </summary>
        /// <param name="max">The maximum health.</param>
        /// <param name="current">The current health. If less than 0, defaults to max health.</param>
        public Health(float max, float current = -1f)
        {
            this.max = max;
            this.current = current < 0 ? max : current;
        }
        
        /// <summary>
        /// Gets the health percentage (current / max).
        /// </summary>
        public float Percentage => max > 0 ? current / max : 0f;
        /// <summary>
        /// Gets a value indicating whether the entity is alive (current health > 0).
        /// </summary>
        public bool IsAlive => current > 0f;
        /// <summary>
        /// Gets a value indicating whether the health is above 50%.
        /// </summary>
        public bool IsHealthy => Percentage > 0.5f;
        /// <summary>
        /// Gets a value indicating whether the health is below 25% (critical).
        /// </summary>
        public bool IsCritical => Percentage < 0.25f;
        
        /// <summary>
        /// Reduces health by the specified damage amount.
        /// </summary>
        /// <param name="damage">The amount of damage to take.</param>
        /// <returns>A new Health struct with the updated health.</returns>
        public Health TakeDamage(float damage)
        {
            return new Health(max, Mathf.Max(0f, current - damage));
        }
        
        /// <summary>
        /// Increases health by the specified amount, capped at maximum health.
        /// </summary>
        /// <param name="amount">The amount of health to restore.</param>
        /// <returns>A new Health struct with the updated health.</returns>
        public Health Heal(float amount)
        {
            return new Health(max, Mathf.Min(max, current + amount));
        }
    }    public struct Age
    {
        /// <summary>
        /// The current age in years.
        /// </summary>
        public readonly int currentAge;
        /// <summary>
        /// The maximum age this entity can reach.
        /// </summary>
        public readonly int maxAge;
        public StatModifiers modifiers; // Optional modifiers for age-related effects

        /// <summary>
        /// Initializes a new instance of the <see cref="Age"/> struct.
        /// </summary>
        /// <param name="max">The maximum age.</param>
        /// <param name="current">The current age. If less than 0, defaults to 0.</param>
        /// <param name="ageModifiers">Optional modifiers for age-related effects.</param>
        public Age(int max, int current = -1, StatModifiers ageModifiers = default)
        {
            maxAge = max;
            currentAge = current < 0 ? 0 : current;
            modifiers = ageModifiers;
        }
    }
    /// <summary>
    /// Represents entity size with predefined values for different size categories.
    /// </summary>
    public struct EntitySize
    {
        /// <summary>
        /// Defines standard entity size categories.
        /// </summary>
        public enum Size
        {
            Small,
            Medium,
            Large,
            Huge,
            Gargantuan
        }

        /// <summary>
        /// The size category.
        /// </summary>
        public readonly Size size;
        /// <summary>
        /// The radius of the entity.
        /// </summary>
        public readonly float radius;
        /// <summary>
        /// The height of the entity.
        /// </summary>
        public readonly float height;
        /// <summary>
        /// The weight of the entity.
        /// </summary>
        public readonly float weight;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySize"/> struct.
        /// </summary>
        /// <param name="size">The size category.</param>
        /// <param name="radius">The entity's radius.</param>
        /// <param name="height">The entity's height.</param>
        /// <param name="weight">The entity's weight.</param>
        public EntitySize(Size size, float radius, float height, float weight)
        {
            this.size = size;
            this.radius = radius;
            this.height = height;
            this.weight = weight;
        }

        /// <summary>
        /// Gets a predefined Small entity size.
        /// </summary>
        public static EntitySize Small => new EntitySize(Size.Small, 0.5f, 1f, 50f);
        /// <summary>
        /// Gets a predefined Medium entity size.
        /// </summary>
        public static EntitySize Medium => new EntitySize(Size.Medium, 1f, 2f, 150f);
        /// <summary>
        /// Gets a predefined Large entity size.
        /// </summary>
        public static EntitySize Large => new EntitySize(Size.Large, 1.5f, 3f, 400f);
        /// <summary>
        /// Gets a predefined Huge entity size.
        /// </summary>
        public static EntitySize Huge => new EntitySize(Size.Huge, 2f, 4f, 800f);
        /// <summary>
        /// Gets a predefined Gargantuan entity size.
        /// </summary>
        public static EntitySize Gargantuan => new EntitySize(Size.Gargantuan, 3f, 6f, 1600f);
    }

    /// <summary>
    /// Represents stat modifiers for temporary or permanent effects, including flat and percentage bonuses.
    /// </summary>
    public struct StatModifiers
    {
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
            flatModifier = flat;
            percentageModifier = percentage;
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

    #endregion

    #region Game Data Structures

    /// <summary>
    /// Represents data for a living entity in the game.
    /// </summary>
    public struct Entity
    {
        /// <summary>
        /// Unique identifiers for predefined entity types.
        /// </summary>
        public enum ID
        {
            Pop = 0,
            Wolf = 1,
            Bear = 2,
            Goblin = 3,
            Orc = 4,
            Dragon = 5,
            Boar = 6,
            Troll = 7,
            Zombie = 8,
            Skeleton = 9,
            Vampire = 10,
            Werewolf = 11,
            Sabertooth = 12,
            Mammoth = 13,
            Phoenix = 14,
            Hydra = 15,
            Kraken = 16,
            Minotaur = 17,
            Golem = 18,
            Chimera = 19,
            Basilisk = 20,
            Centaur = 21,
            Harpy = 22,
            Siren = 23,
            Yeti = 24,
        }

        /// <summary>
        /// Defines the behavioral categories for entities.
        /// </summary>
        public enum EntityType
        {
            Boss,
            Minion,
            NPC, // Non-Playable Character
            PlayerControlled,
            Animal,
            Monster,
        }

        /// <summary>
        /// Defines the aggression levels for entities.
        /// </summary>
        public enum AggressionType
        {
            Passive,
            Neutral,
            Aggressive
        }

        // Basic Identity
        /// <summary>The display name of the entity.</summary>
        public string entityName;
        /// <summary>The unique identifier for this entity, often corresponds to <see cref="ID"/>.</summary>
        public int entityID;
        /// <summary>The faction this entity belongs to.</summary>
        public string entityFaction;
        /// <summary>A short description of the entity.</summary>
        public string entityDescription;
        /// <summary>The icon representing this entity in UI elements.</summary>
        public Sprite entityIcon;

        // Entity Properties
        /// <summary>A list of types this entity belongs to (e.g., Boss, Animal).</summary>
        public List<EntityType> entityType;
        /// <summary>The rarity level of this entity.</summary>
        public Rarity rarity;
        /// <summary>The chance of this entity spawning, as a percentage (0-100).</summary>
        public float spawnChance;
        /// <summary>The current level of this entity.</summary>
        public int level;
        /// <summary>The physical size category of this entity.</summary>
        public EntitySize entitySize;
        /// <summary>The aggression behavior of this entity.</summary>
        public AggressionType aggressionType;

        // Health and Status
        /// <summary>The current health status of this entity.</summary>
        public Health health;
        /// <summary>Indicates whether this entity uses mana or a similar resource.</summary>
        public bool usesMana;
        /// <summary>Indicates whether this entity is currently alive.</summary>
        public bool isAlive;

        // Stats
        /// <summary>The mana (or equivalent resource) stat for this entity.</summary>
        public Stat mana;
        /// <summary>The attack power stat for this entity.</summary>
        public Stat attack;
        /// <summary>The defense stat for this entity.</summary>
        public Stat defense;
        /// <summary>The movement speed stat for this entity.</summary>
        public Stat speed;

        //todo: finish adding stats, and add methods for modifying stats, applying buffs, etc.

        // Game Balance
        /// <summary>A modifier affecting the value (e.g., loot drop value) of this entity.</summary>
        public int valueModifier;

        /// <summary>A modifier affecting the experience points granted by this entity.</summary>
        public int experienceModifier;

        // Utility
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class with a specific name and ID.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="id">The <see cref="ID"/> of the entity.</param>
        public Entity(string name, ID id, string faction = "", string description = "", Sprite icon = null, Rarity rarity = Rarity.Common, float spawnChance = 0f, int level = 1, EntitySize? size = null, AggressionType aggressionType = AggressionType.Neutral, Health? healthValue = null, bool usesMana = false, bool isAlive = true, int valueModifier = 0, int experienceModifier = 0)
        {
            entityName = name;
            entityID = (int)id;
            entityFaction = faction;
            entityDescription = description;
            entityIcon = icon;
            this.rarity = rarity;
            this.spawnChance = spawnChance;
            this.level = level;
            entitySize = size ?? EntitySize.Medium;
            this.aggressionType = aggressionType;
            health = healthValue ?? new Health(100f);
            this.usesMana = usesMana;
            this.isAlive = isAlive;
            mana = new Stat(Stat.ID.Mana, "Mana", 0f);
            attack = new Stat(Stat.ID.Attack, "Attack", 0f);
            defense = new Stat(Stat.ID.Defense, "Defense", 0f);
            speed = new Stat(Stat.ID.Speed, "Speed", 0f);
            this.valueModifier = valueModifier;
            this.experienceModifier = experienceModifier;
            entityType = new List<EntityType>();
            tags = new List<string>();
        }
        
        //Todo: Add methods for managing health, stats, and other entity behaviors. Implement a state machine for entity behaviors (e.g., idle, attack, flee), and assign which states can exist based on tags or other properties.
    }    /// <summary>
    /// Represents an item in the game, including its type, rarity, quality, and other properties.
    /// </summary>
    public struct Item
    {
        /// <summary>
        /// Unique identifiers for predefined item types.
        /// </summary>
        public enum ID
        {
            IronSword = 0,
            SteelAxe = 1,
            EnchantedStaff = 2,
            LeatherArmor = 10,
            ChainMail = 11,
            DragonScaleArmor = 12,
            HealthPotion = 20,
            ManaPotion = 21,
            Bread = 22,
            AncientKey = 30,
            GoldCoin = 40
        }

        /// <summary>
        /// Defines the categories of items.
        /// </summary>
        public enum ItemType
        {
            Weapon,
            Armor,
            Consumable,
            QuestItem,
            Miscellaneous
        }
        /// <summary>
        /// Defines the rarity levels specifically for items.
        /// </summary>
        public enum ItemRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }
        /// <summary>
        /// Defines the quality levels for items.
        /// </summary>
        public enum ItemQuality
        {
            Poor,
            Fair,
            Good,
            Excellent,
            Masterwork
        }
        /// <summary>
        /// Defines the equipment slots items can occupy.
        /// </summary>
        public enum ItemSlot
        {
            Head,
            Chest,
            Legs,
            Feet,
            Hands,
            Neck,
            Ring,
            Weapon,
            Offhand,
            Trinket
        }

        /// <summary>The display name of the item.</summary>
        public string itemName;
        /// <summary>The unique identifier for this item.</summary>
        public int itemID;
        /// <summary>The type or category of this item.</summary>
        public ItemType itemType;
        /// <summary>The weight of the item.</summary>
        public float weight;
        /// <summary>The quantity of this item (if stackable).</summary>
        public int quantity;
        /// <summary>The monetary value of the item.</summary>
        public int value;
        /// <summary>The rarity level of this item.</summary>
        public ItemRarity itemRarity;
        /// <summary>The quality level of this item.</summary>
        public ItemQuality itemQuality;
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> struct.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <param name="id">The unique ID of the item.</param>
        /// <param name="type">The type of the item.</param>
        /// <param name="weight">The weight of the item.</param>
        /// <param name="quantity">The quantity of the item.</param>
        /// <param name="value">The value of the item.</param>
        /// <param name="rarity">The rarity of the item.</param>
        /// <param name="quality">The quality of the item.</param>
        public Item(string name, ID id, ItemType type, float weight = 1f, int quantity = 1, int value = 10, ItemRarity rarity = ItemRarity.Common, ItemQuality quality = ItemQuality.Fair)
        {
            itemName = name;
            itemID = (int)id;
            itemType = type;
            this.weight = weight;
            this.quantity = quantity;
            this.value = value;
            itemRarity = rarity;
            itemQuality = quality;
            tags = new List<string>();
        }
    }

    public struct State
    {
        /// <summary>
        /// Unique identifiers for predefined state types.
        /// </summary>
        public enum ID
        {
            Idle = 0,
            Attacking = 1,
            Defending = 2,
            Fleeing = 3,
            Searching = 4, // Represents a state where the entity is searching for something
            Resting = 5, // Represents a state where the entity is resting or recovering
            //todo: Decide if we want to change resting to sleeping, or if we want to keep resting as a state where the entity is not actively doing anything but not necessarily sleeping.
            Patrolling = 6, // Represents a state where the entity is patrolling an area
            Interacting = 7, // Represents a state where the entity is interacting with an object or another entity
            Hauling = 8, // Represents a state where the entity is hauling or transporting items
            Gathering = 9, // Represents a state where the entity is gathering resources or items
            Hiding = 10, // Represents a state where the entity is hiding or camouflaging itself
            Socializing = 11,   // Represents a state where the entity is interacting with other entities socially
            Crafting = 12, // Represents a state where the entity is crafting items or equipment
            Healing = 13, // Represents a state where the entity is healing itself or others
            Exploring = 14, // Represents a state where the entity is exploring the environment
            Hunting = 15, // Represents a state where the entity is actively hunting for resources or prey
            Playing = 16, // Represents a state where the entity is engaged in play activities
            Fishing = 17, // Represents a state where the entity is engaged in fishing activities
            Farming = 18, // Represents a state where the entity is engaged in farming activities
            // Special states
            Dead = 19, // Represents a state where the entity is dead
            Unconscious = 20, // Represents a state where the entity is incapacitated but not dead
        }

        /// <summary>
        /// The unique identifier for this state, often corresponds to <see cref="ID"/>.
        /// </summary>
        public int stateID;
        /// <summary>The display name of the state.</summary>
        public string stateName;
        /// <summary>A short description of the state.</summary>
        public string stateDescription;

        /// <summary>
        /// Determines the logic of the state and what occurs when the state is active.
        /// </summary>


        // Todo: update below to add parameters we missed.
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the state.</param>
        /// <param name="name">The name of the state.</param>
        /// <param name="description">The description of the state.</param>
        public State(ID id, string name, string description)
        {
            stateID = (int)id;
            stateName = name;
            stateDescription = description;
        }

        //Todo: Transition the state logic from other scripts to pull from this data.
    }
    /// <summary>
    /// Represents a buff or debuff effect that can be applied to entities.
    /// </summary>
    public struct Buff
    {
        /// <summary>
        /// Unique identifiers for predefined buff types.
        /// </summary>
        public enum ID
        {
            HealthRegen = 0,
            ManaRegen = 1,
            SpeedBoost = 2,
            StrengthBoost = 3,
            DefenseBoost = 4,
            CriticalHitChanceBoost = 5,
            CriticalHitDamageBoost = 6,
            ExperienceBoost = 7,
            LuckBoost = 8
        }
        /// <summary>
        /// Defines the nature of the buff (e.g., temporary, permanent, debuff).
        /// </summary>
        public enum BuffType
        {
            Temporary,
            Permanent,
            Debuff
        }

        /// <summary>The unique identifier for this buff, often corresponds to <see cref="ID"/>.</summary>
        public int buffID;
        /// <summary>The display name of the buff.</summary>
        public string buffName;
        /// <summary>A short description of the buff's effects.</summary>
        public string buffDescription;
        /// <summary>The type of the buff (e.g., temporary, permanent).</summary>
        public BuffType buffType;
        /// <summary>The duration of the buff in seconds. A value of 0 typically indicates a permanent buff.</summary>
        public float duration; // 0 for permanent buffs
        /// <summary>The magnitude or strength of the buff's effect.</summary>
        public float strength;
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Buff"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the buff.</param>
        /// <param name="name">The name of the buff.</param>
        /// <param name="description">The description of the buff.</param>
        /// <param name="type">The type of the buff.</param>
        /// <param name="strength">The strength/magnitude of the buff.</param>
        /// <param name="duration">The duration of the buff in seconds (0 for permanent).</param>
        public Buff(ID id, string name, string description, BuffType type, float strength, float duration = 0f)
        {
            buffID = (int)id;
            buffName = name;
            buffDescription = description;
            buffType = type;
            this.strength = strength;
            this.duration = duration;
            tags = new List<string>();
        }
    }
    
    /// <summary>
    /// Represents a specific statistic for an entity, such as Health, Mana, or Attack
    /// </summary>
    public struct Stat
    {
        /// <summary>
        /// Unique identifiers for predefined stat types.
        /// </summary>
        public enum ID
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
            Charisma = 16
        }
        
        /// <summary>
        /// Defines the category of a stat (e.g., primary, secondary).
        /// </summary>
        public enum StatType
        {
            Primary, // Core stats like Health, Mana, etc.
            Secondary, // Derived stats like Attack, Defense, etc.
            Tertiary // Miscellaneous stats like Experience, Level, etc.
        }

        /// <summary>The unique identifier for this stat, often corresponds to <see cref="ID"/>.</summary>
        public ID statID;
        /// <summary>The display name of the stat.</summary>
        public string statName;
        /// <summary>A short description of the stat.</summary>
        public string statDescription;
        /// <summary>The type or category of this stat.</summary>
        public StatType statType;
        /// <summary>The base value of the stat before any modifiers.</summary>
        public float baseValue;
        /// <summary>The current value of the stat after modifiers.</summary>
        public float currentValue;
        /// <summary>The minimum possible value for this stat.</summary>
        public float minValue;
        /// <summary>The maximum possible value for this stat.</summary>
        public float maxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stat"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the stat.</param>
        /// <param name="name">The name of the stat.</param>
        /// <param name="baseVal">The base value of the stat.</param>
        /// <param name="minVal">The minimum value of the stat.</param>
        /// <param name="maxVal">The maximum value of the stat.</param>
        /// <param name="type">The type of the stat.</param>
        /// <param name="description">The description of the stat.</param>
        public Stat(ID id, string name, float baseVal = 0f, float minVal = 0f, float maxVal = 100f, StatType type = StatType.Primary, string description = "")
        {
            statID = id;
            statName = name;
            statDescription = description;
            statType = type;
            baseValue = baseVal;
            currentValue = baseVal;
            minValue = minVal;
            maxValue = maxVal;
        }

        /// <summary>
        /// Gets the current value of the stat as a percentage of its maximum value.
        /// </summary>
        /// <returns>The current value as a percentage (0.0 to 1.0), or 0 if max value is 0.</returns>
        public float GetPercentage()
        {
            return maxValue > 0 ? currentValue / maxValue : 0f;
        }

        /// <summary>
        /// Modifies the current value of the stat by a given amount, clamped by min/max values.
        /// </summary>
        /// <param name="amount">The amount to add to the current value (can be negative).</param>
        public void ModifyStat(float amount)
        {
            currentValue = Mathf.Clamp(currentValue + amount, minValue, maxValue);
        }

        /// <summary>
        /// Sets the current value of the stat, clamped by min/max values.
        /// </summary>
        /// <param name="value">The new value for the stat.</param>
        public void SetCurrentValue(float value)
        {
            currentValue = Mathf.Clamp(value, minValue, maxValue);
        }
    }

    /// <summary>
    /// Represents a skill in the game. Some can be influenced by the Entities stats, items, and buffs.
    /// Skills can be used to perform actions, craft items, or gain experience.
    /// </summary>
    public struct Skill
    {
        /// <summary>
        /// Unique identifiers for predefined skill types.
        /// </summary>
        public enum ID
        {
            Combat = 0,
            Crafting = 1,
            Gathering = 2,
            Social = 3,
            Magic = 4,
            Exploration = 5,
            Survival = 6,
            Stealth = 7,
            Engineering = 8,
            Alchemy = 9
        }        /// <summary>
        /// Defines the categories of skills.
        /// </summary>
        public enum SkillType
        {
            Combat,
            Crafting,
            Gathering,
            Social,
            Magic,
            Exploration,
            Survival,
            Stealth,
            Engineering,
            Alchemy
        }
        
        /// <summary>The unique identifier for this skill, often corresponds to <see cref="ID"/>.</summary>
        public ID skillID;
        /// <summary>The name of the skill, often corresponding to its <see cref="SkillType"/>.</summary>
        public SkillType skillName;
        /// <summary>The type or category of this skill.</summary>
        public SkillType skillType;
        /// <summary>The current experience points for this skill.</summary>
        public float experience;
        /// <summary>The current level of this skill.</summary>
        public int level;
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;        /// <summary>
        /// Initializes a new instance of the <see cref="Skill"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the skill.</param>
        /// <param name="type">The <see cref="SkillType"/> of the skill.</param>
        /// <param name="initialExperience">The initial experience points for the skill.</param>
        /// <param name="initialLevel">The initial level of the skill.</param>
        public Skill(ID id, SkillType type, float initialExperience = 0f, int initialLevel = 1)
        {
            skillID = id;
            skillName = type;
            skillType = type;
            experience = initialExperience;
            level = initialLevel;
            tags = new List<string>();
        }
        
        /// <summary>
        /// Adds experience to this skill and calculates level progression.
        /// </summary>
        /// <param name="expPoints">The amount of experience to add.</param>
        /// <returns>True if the skill leveled up, false otherwise.</returns>
        public bool AddExperience(float expPoints)
        {
            int oldLevel = level;
            experience += expPoints;
            // Simple leveling formula: every 100 exp = 1 level
            int newLevel = Mathf.FloorToInt(experience / 100f) + 1;
            level = Mathf.Max(1, newLevel);
            return level > oldLevel;
        }
    }

    /// <summary>
    /// A leveling system that uses experience to determine the level of a trait.
    /// </summary>

    public struct LevelingSystem
    {
        /// <summary>
        /// The current level of the trait.
        /// </summary>
        public int currentLevel;
        /// <summary>
        /// The experience points required to reach the next level.
        /// </summary>
        public int experienceToNextLevel;
        public int maxLevel; // Optional: Maximum level this system can reach
        public int experiencePerLevel; // Optional: Experience required per level, can be adjusted for balance
        public StatModifiers modifiers; // Optional modifiers for leveling effects
        public List<Trait> traitRewards; // Optional list of traits that can be rewarded at certain levels
        public List<Skill> skillRewards; // Optional list of skills that can be rewarded at certain levels
        public List<Item> itemRewards; // Optional list of items that can be rewarded at certain levels

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelingSystem"/> struct.
        /// </summary>
        /// <param name="level">The initial level.</param>
        /// <param name="experience">The initial experience points required for the next level.</param>
        public LevelingSystem(int level, int experience)
        {
            currentLevel = level;
            experienceToNextLevel = experience;
            maxLevel = 100; // Default max level
            experiencePerLevel = 100; // Default experience per level
            modifiers = new StatModifiers(); // Default empty modifiers
            traitRewards = new List<Trait>(); // Empty list
            skillRewards = new List<Skill>(); // Empty list
            itemRewards = new List<Item>(); // Empty list
        }
        
        //Todo: Find a unique name. Add methods for leveling up, gaining experience, and checking if the next level is reached for each skill.
    }
    public struct Trait
    {
        /// <summary>
        /// The unique identifier for this trait.
        /// </summary>
        public enum ID
        {
            Brave = 0,
            Cunning = 1,
            Strong = 2,
            Agile = 3,
            Wise = 4,
            Charismatic = 5,
            Resilient = 6,
            Stealthy = 7,
            Lucky = 8,
            Fearless = 9,
            Compassionate = 10,
            Honorable = 11,
            Curious = 12,
            Patient = 13,
            Resourceful = 14,
            Determined = 15,
            Creative = 16,
            Loyal = 17,
            Optimistic = 18,
            Pessimistic = 19,
            Skeptical = 20,
            Adventurous = 21,
            Sneaky = 22,
            QuickWitted = 23,
            Empathetic = 24,
            Clumbsy = 25,
            Perceptive = 26,
            Intuitive = 27,
            Analytical = 28,
            Quick = 29,
            Daring = 30,
            Fearful = 31,
            Impulsive = 32,
            Naive = 33,
            Arrogant = 34,
            QuickCrafter = 35,
            Masterful = 36,
            LuckyCrafter = 37,
            Efficient = 38,
            ResourcefulCrafter = 39,
            Strategic = 40,
            Diplomatic = 41,

        }

        /// <summary>
        /// The name of the trait.
        /// </summary>
        public readonly string traitName;
        /// <summary>
        /// A brief description of the trait.
        /// </summary>
        public readonly string description;
        /// <summary>
        /// The category of the trait (e.g., Combat, Social, etc.).
        /// </summary>
        public readonly string category;

        public ID traitID; // The unique identifier for this trait, often corresponds to <see cref="ID"/>
        public StatModifiers modifiers; // Optional modifiers for trait effects
        public List<string> tags; // Optional tags for categorization or special mechanics
        public List<Trait> requiredTraits; // Optional reference to a required trait for this trait to be applicable
        public Skill.ID requiredSkill; // Optional reference to a required skill for this trait to be applicable
        public List<Item.ID> requiredItems; // Optional reference to a list of required items for this trait to be applicable
        public Stat requiredStat; // Optional reference to a required stat for this trait to be applicable        /// <summary>
        /// Initializes a new instance of the <see cref="Trait"/> struct.
        /// </summary>
        /// <param name="id">The unique identifier for the trait.</param>
        /// <param name="name">The name of the trait.</param>
        /// <param name="description">A brief description of the trait.</param>
        /// <param name="category">The category of the trait.</param>
        public Trait(ID id, string name, string description, string category)
        {
            traitID = id;
            traitName = name;
            this.description = description;
            this.category = category;
            modifiers = new StatModifiers();
            tags = new List<string>();
            requiredTraits = new List<Trait>();
            requiredSkill = Skill.ID.Combat; // Default, can be changed
            requiredItems = new List<Item.ID>();
            requiredStat = new Stat(Stat.ID.Strength, "Default"); // Default, can be changed
        }


    }

    // Todo: Add more game data structures as needed, such as Quests, Dialogues, Genetics, NPCs, Lore, a Journal system (which might be a separate script that pulls from this) etc.

    #endregion // Game Data Structures

    /// <summary>
    /// Contains all game data structures, enums, and database management.
    /// </summary>
    public static class GameData
    {
        #region Databases
        public static List<Entity> entityDatabase = new List<Entity>();
        public static List<Item> itemDatabase = new List<Item>(); // Added item database
        public static List<Buff> buffDatabase = new List<Buff>(); // Added buff database
        public static List<Skill> skillDatabase = new List<Skill>(); // Added skill database

        #endregion // Databases

        #region Data Retrieval Methods

        /// <summary>
        /// Retrieves an entity from the database by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>The found <see cref="Entity"/>, or a default "Unknown" entity if not found.</returns>
        public static Entity GetEntityByID(int id)
        {
            Entity entity = entityDatabase.Find(e => e.entityID == id);
            if (entity != null)
            {
                return entity;
            }
            // Return a default entity if not found
            return new Entity
            {
                entityName = "Entity_" + id,
                entityID = id,
                health = new Health(100f), // Corrected to use Health struct
                mana = new Stat(Stat.ID.Mana, "Mana", 100f), // Corrected to use Stat struct
                tags = new List<string> { "Unknown" }
            };
        }

        /// <summary>
        /// Retrieves an entity from the database by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>The found <see cref="Entity"/>, or a default "Unknown" entity if not found.</returns>
        public static Entity GetEntityByID(Entity.ID id)
        {
            int entityId = (int)id;
            return GetEntityByID(entityId);
        }

        /// <summary>
        /// Retrieves an item from the database by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <returns>The found <see cref="Item"/>, or a default "Unknown" item if not found.</returns>
        public static Item GetItemByID(Item.ID id)
        {
            int itemId = (int)id;
            Item item = itemDatabase.Find(i => i.itemID == itemId);
            // Check if item is not default (structs can't be null)
            if (item.itemID == itemId && !string.IsNullOrEmpty(item.itemName))
            {
                return item;
            }
            // Return a default item if not found
            return new Item("Item_" + itemId, itemId, Item.ItemType.Miscellaneous);
        }        /// <summary>
                 /// Retrieves a buff from the database by its unique ID.
                 /// </summary>
                 /// <param name="id">The ID of the buff to retrieve.</param>
                 /// <returns>The found <see cref="Buff"/>, or a default "Unknown" buff if not found.</returns>
        public static Buff GetBuffByID(Buff.ID id)
        {
            int buffId = (int)id;
            Buff buff = buffDatabase.Find(b => b.buffID == buffId);
            // Check if buff is not default (structs can't be null)
            if (buff.buffID == buffId && !string.IsNullOrEmpty(buff.buffName))
            {
                return buff;
            }
            // Return a default buff if not found
            return new Buff(id, "Buff_" + buffId, "Unknown buff effect", Buff.BuffType.Temporary, 1f, 10f);
        }        /// <summary>
                 /// Retrieves a skill from the database by its unique ID.
                 /// </summary>
                 /// <param name="id">The ID of the skill to retrieve.</param>
                 /// <returns>The found <see cref="Skill"/>, or a default "Unknown" skill if not found.</returns>
        public static Skill GetSkillByID(Skill.ID id)
        {
            Skill skill = skillDatabase.Find(s => s.skillID == id);
            if (skill != null)
            {
                return skill;
            }
            // Return a default skill if not found
            return new Skill(id, Skill.SkillType.Combat)
            {
                tags = new List<string> { "Unknown" }
            };
        }

        /// <summary>
        /// Retrieves a stat definition by its unique ID. Returns a default stat template.
        /// </summary>
        /// <param name="id">The ID of the stat to retrieve.</param>
        /// <returns>A default <see cref="Stat"/> template with the specified ID.</returns>
        public static Stat GetStatByID(Stat.ID id)
        {
            // Since stats are typically created per-entity, this returns a template
            string statName = id.ToString();
            string description = $"The {statName.ToLower()} statistic";

            // Set appropriate defaults based on stat type
            float defaultMax = 100f;
            Stat.StatType statType = Stat.StatType.Primary;

            switch (id)
            {
                case Stat.ID.Health:
                case Stat.ID.Mana:
                case Stat.ID.Stamina:
                    defaultMax = 100f;
                    statType = Stat.StatType.Primary;
                    break;
                case Stat.ID.Attack:
                case Stat.ID.Defense:
                case Stat.ID.Speed:
                case Stat.ID.MagicPower:
                case Stat.ID.MagicDefense:
                    defaultMax = float.MaxValue;
                    statType = Stat.StatType.Secondary;
                    break;
                case Stat.ID.Experience:
                case Stat.ID.Level:
                case Stat.ID.Luck:
                    defaultMax = float.MaxValue;
                    statType = Stat.StatType.Tertiary;
                    break;
                case Stat.ID.CriticalHitChance:
                    defaultMax = 100f;
                    statType = Stat.StatType.Secondary;
                    description = "The chance to deal critical damage (percentage)";
                    break;
                case Stat.ID.CriticalHitDamage:
                    defaultMax = 500f;
                    statType = Stat.StatType.Secondary;
                    description = "The multiplier for critical hit damage (percentage)";
                    break;
                default:
                    defaultMax = 100f;
                    statType = Stat.StatType.Primary;
                    break;
            }

            return new Stat(id, statName, 0f, 0f, defaultMax, statType, description);
        }

        /// <summary>
        /// Retrieves a rarity enum value by its numeric index.
        /// </summary>
        /// <param name="rarityIndex">The numeric index of the rarity (0=Common, 1=Uncommon, etc.).</param>
        /// <returns>The corresponding <see cref="Rarity"/> enum value, or Common if out of range.</returns>
        public static Rarity GetRarityByIndex(int rarityIndex)
        {
            if (rarityIndex < 0 || rarityIndex >= System.Enum.GetValues(typeof(Rarity)).Length)
            {
                return Rarity.Common;
            }
            return (Rarity)rarityIndex;
        }

        /// <summary>
        /// Retrieves an entity size template by its size category.
        /// </summary>
        /// <param name="size">The size category to retrieve.</param>
        /// <returns>The corresponding <see cref="EntitySize"/> with predefined values.</returns>
        public static EntitySize GetEntitySizeByCategory(EntitySize.Size size)
        {
            switch (size)
            {
                case EntitySize.Size.Small:
                    return EntitySize.Small;
                case EntitySize.Size.Medium:
                    return EntitySize.Medium;
                case EntitySize.Size.Large:
                    return EntitySize.Large;
                case EntitySize.Size.Huge:
                    return EntitySize.Huge;
                case EntitySize.Size.Gargantuan:
                    return EntitySize.Gargantuan;
                default:
                    return EntitySize.Medium;
            }
        }

        /// <summary>
        /// Retrieves all entities of a specific type.
        /// </summary>
        /// <param name="entityType">The type of entities to retrieve.</param>
        /// <returns>A list of entities matching the specified type.</returns>
        public static List<Entity> GetEntitiesByType(Entity.EntityType entityType)
        {
            return entityDatabase.FindAll(e => e.entityType.Contains(entityType));
        }

        /// <summary>
        /// Retrieves all items of a specific type.
        /// </summary>
        /// <param name="itemType">The type of items to retrieve.</param>
        /// <returns>A list of items matching the specified type.</returns>
        public static List<Item> GetItemsByType(Item.ItemType itemType)
        {
            return itemDatabase.FindAll(i => i.itemType == itemType);
        }

        /// <summary>
        /// Retrieves all buffs of a specific type.
        /// </summary>
        /// <param name="buffType">The type of buffs to retrieve.</param>
        /// <returns>A list of buffs matching the specified type.</returns>
        public static List<Buff> GetBuffsByType(Buff.BuffType buffType)
        {
            return buffDatabase.FindAll(b => b.buffType == buffType);
        }

        /// <summary>
        /// Retrieves all skills of a specific type.
        /// </summary>
        /// <param name="skillType">The type of skills to retrieve.</param>
        /// <returns>A list of skills matching the specified type.</returns>
        public static List<Skill> GetSkillsByType(Skill.SkillType skillType)
        {
            return skillDatabase.FindAll(s => s.skillType == skillType);
        }

        /// <summary>
        /// Retrieves all entities with a specific rarity level.
        /// </summary>
        /// <param name="rarity">The rarity level to search for.</param>
        /// <returns>A list of entities with the specified rarity.</returns>
        public static List<Entity> GetEntitiesByRarity(Rarity rarity)
        {
            return entityDatabase.FindAll(e => e.rarity == rarity);
        }

        /// <summary>
        /// Retrieves all items with a specific rarity level.
        /// </summary>
        /// <param name="rarity">The rarity level to search for.</param>
        /// <returns>A list of items with the specified rarity.</returns>
        public static List<Item> GetItemsByRarity(Item.ItemRarity rarity)
        {
            return itemDatabase.FindAll(i => i.itemRarity == rarity);
        }

        /// <summary>
        /// Retrieves entities by name (case-insensitive search).
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <param name="exactMatch">If true, requires exact match; if false, allows partial matches.</param>
        /// <returns>A list of entities matching the search criteria.</returns>
        public static List<Entity> GetEntitiesByName(string name, bool exactMatch = true)
        {
            if (exactMatch)
            {
                return entityDatabase.FindAll(e => string.Equals(e.entityName, name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return entityDatabase.FindAll(e => e.entityName.ToLower().Contains(name.ToLower()));
            }
        }

        /// <summary>
        /// Retrieves items by name (case-insensitive search).
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <param name="exactMatch">If true, requires exact match; if false, allows partial matches.</param>
        /// <returns>A list of items matching the search criteria.</returns>
        public static List<Item> GetItemsByName(string name, bool exactMatch = true)
        {
            if (exactMatch)
            {
                return itemDatabase.FindAll(i => string.Equals(i.itemName, name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return itemDatabase.FindAll(i => i.itemName.ToLower().Contains(name.ToLower()));
            }
        }

        /// <summary>
        /// Retrieves entities that contain a specific tag.
        /// </summary>
        /// <param name="tag">The tag to search for.</param>
        /// <returns>A list of entities that have the specified tag.</returns>
        public static List<Entity> GetEntitiesByTag(string tag)
        {
            return entityDatabase.FindAll(e => e.tags != null && e.tags.Contains(tag));
        }

        /// <summary>
        /// Retrieves items that contain a specific tag.
        /// </summary>
        /// <param name="tag">The tag to search for.</param>
        /// <returns>A list of items that have the specified tag.</returns>
        public static List<Item> GetItemsByTag(string tag)
        {
            return itemDatabase.FindAll(i => i.tags != null && i.tags.Contains(tag));
        }

        /// <summary>
        /// Retrieves buffs that contain a specific tag.
        /// </summary>
        /// <param name="tag">The tag to search for.</param>
        /// <returns>A list of buffs that have the specified tag.</returns>
        public static List<Buff> GetBuffsByTag(string tag)
        {
            return buffDatabase.FindAll(b => b.tags != null && b.tags.Contains(tag));
        }

        /// <summary>
        /// Retrieves skills that contain a specific tag.
        /// </summary>
        /// <param name="tag">The tag to search for.</param>
        /// <returns>A list of skills that have the specified tag.</returns>
        public static List<Skill> GetSkillsByTag(string tag)
        {
            return skillDatabase.FindAll(s => s.tags != null && s.tags.Contains(tag));
        }

        /// <summary>
        /// Gets the total count of all entries in all databases.
        /// </summary>
        /// <returns>A dictionary with database names and their entry counts.</returns>
        public static Dictionary<string, int> GetDatabaseCounts()
        {
            return new Dictionary<string, int>
            {
                { "Entities", entityDatabase.Count },
                { "Items", itemDatabase.Count },
                { "Buffs", buffDatabase.Count },
                { "Skills", skillDatabase.Count }
            };
        }


        #endregion // Data Retrieval Methods

        #region Database Initialization

        /// <summary>
        /// Initializes the entity database with predefined entities. Clears any existing data.
        /// </summary>
        public static void InitializeEntityDatabase()
        {
            entityDatabase.Clear(); // Clear existing data

            // Example initialization of the entity database
            entityDatabase.Add(new Entity("Pop", Entity.ID.Pop)
            {
                entityType = new List<Entity.EntityType> { Entity.EntityType.PlayerControlled },
                health = new Health(100f),
                mana = new Stat(Stat.ID.Mana, "Mana", 50f, 0f, 50f),
                attack = new Stat(Stat.ID.Attack, "Attack", 5f, 0f, float.MaxValue),
                defense = new Stat(Stat.ID.Defense, "Defense", 3f, 0f, float.MaxValue),
                speed = new Stat(Stat.ID.Speed, "Speed", 8f, 0f, float.MaxValue),
                entitySize = EntitySize.Medium,
                aggressionType = Entity.AggressionType.Neutral,
                tags = new List<string> { "Humanoid", "Playable" }
            });

            entityDatabase.Add(new Entity("Wolf", Entity.ID.Wolf)
            {
                entityType = new List<Entity.EntityType> { Entity.EntityType.Animal },
                health = new Health(60f),
                attack = new Stat(Stat.ID.Attack, "Attack", 12f),
                defense = new Stat(Stat.ID.Defense, "Defense", 4f),
                speed = new Stat(Stat.ID.Speed, "Speed", 15f),
                entitySize = EntitySize.Medium,
                aggressionType = Entity.AggressionType.Aggressive,
                tags = new List<string> { "Animal", "Predator" }
            });
            entityDatabase.Add(new Entity("Dragon", Entity.ID.Dragon)
            {
                entityType = new List<Entity.EntityType> { Entity.EntityType.Boss, Entity.EntityType.Monster },
                health = new Health(500f),
                mana = new Stat(Stat.ID.Mana, "Mana", 200f),
                attack = new Stat(Stat.ID.Attack, "Attack", 50f),
                defense = new Stat(Stat.ID.Defense, "Defense", 30f),
                speed = new Stat(Stat.ID.Speed, "Speed", 20f),
                entitySize = EntitySize.Gargantuan,
                aggressionType = Entity.AggressionType.Aggressive,
                rarity = Rarity.Legendary, // Note: Rarity enum is in the namespace
                level = 50,
                tags = new List<string> { "Boss", "Flying", "Fire" }
            });
        }
        /// <summary>
        /// Initializes the item database with predefined items. Clears any existing data.
        /// </summary>        
        public static void InitializeItemDatabase()
        {
            itemDatabase.Clear();

            // Weapons
            itemDatabase.Add(new Item("Iron Sword", Item.ID.IronSword, Item.ItemType.Weapon, 3f, 1, 50, Item.ItemRarity.Common, Item.ItemQuality.Good)
            {
                tags = new List<string> { "Weapon", "Melee", "Sword" }
            });

            itemDatabase.Add(new Item("Steel Axe", Item.ID.SteelAxe, Item.ItemType.Weapon, 4f, 1, 75, Item.ItemRarity.Common, Item.ItemQuality.Good)
            {
                tags = new List<string> { "Weapon", "Melee", "Axe" }
            });

            itemDatabase.Add(new Item("Enchanted Staff", Item.ID.EnchantedStaff, Item.ItemType.Weapon, 2f, 1, 150, Item.ItemRarity.Rare, Item.ItemQuality.Excellent)
            {
                tags = new List<string> { "Weapon", "Magic", "Staff" }
            });

            // Armor
            itemDatabase.Add(new Item("Leather Armor", Item.ID.LeatherArmor, Item.ItemType.Armor, 5f, 1, 30, Item.ItemRarity.Common, Item.ItemQuality.Fair)
            {
                tags = new List<string> { "Armor", "Light", "Chest" }
            });

            itemDatabase.Add(new Item("Chain Mail", Item.ID.ChainMail, Item.ItemType.Armor, 8f, 1, 80, Item.ItemRarity.Uncommon, Item.ItemQuality.Good)
            {
                tags = new List<string> { "Armor", "Medium", "Chest" }
            });

            itemDatabase.Add(new Item("Dragon Scale Armor", Item.ID.DragonScaleArmor, Item.ItemType.Armor, 15f, 1, 500, Item.ItemRarity.Legendary, Item.ItemQuality.Masterwork)
            {
                tags = new List<string> { "Armor", "Heavy", "Chest", "Dragon" }
            });

            // Consumables
            itemDatabase.Add(new Item("Health Potion", Item.ID.HealthPotion, Item.ItemType.Consumable, 0.5f, 5, 25, Item.ItemRarity.Common, Item.ItemQuality.Good)
            {
                tags = new List<string> { "Consumable", "Potion", "Healing" }
            });

            itemDatabase.Add(new Item("Mana Potion", Item.ID.ManaPotion, Item.ItemType.Consumable, 0.5f, 3, 35, Item.ItemRarity.Common, Item.ItemQuality.Good)
            {
                tags = new List<string> { "Consumable", "Potion", "Mana" }
            });

            itemDatabase.Add(new Item("Bread", Item.ID.Bread, Item.ItemType.Consumable, 0.2f, 10, 5, Item.ItemRarity.Common, Item.ItemQuality.Fair)
            {
                tags = new List<string> { "Consumable", "Food" }
            });

            // Quest Items
            itemDatabase.Add(new Item("Ancient Key", Item.ID.AncientKey, Item.ItemType.QuestItem, 0.1f, 1, 0, Item.ItemRarity.Rare, Item.ItemQuality.Excellent)
            {
                tags = new List<string> { "Quest", "Key", "Ancient" }
            });

            // Miscellaneous
            itemDatabase.Add(new Item("Gold Coin", Item.ID.GoldCoin, Item.ItemType.Miscellaneous, 0.01f, 100, 1, Item.ItemRarity.Common, Item.ItemQuality.Fair)
            {
                tags = new List<string> { "Currency", "Gold" }
            });
        }        /// <summary>
                 /// Initializes the buff database with predefined buffs. Clears any existing data.
                 /// </summary>
        public static void InitializeBuffDatabase()
        {
            buffDatabase.Clear();

            // Positive Buffs
            buffDatabase.Add(new Buff(Buff.ID.HealthRegen, "Health Regeneration", "Slowly restores health over time", Buff.BuffType.Temporary, 5f, 30f)
            {
                tags = new List<string> { "Healing", "Regeneration", "Beneficial" }
            });

            buffDatabase.Add(new Buff(Buff.ID.ManaRegen, "Mana Regeneration", "Slowly restores mana over time", Buff.BuffType.Temporary, 3f, 45f)
            {
                tags = new List<string> { "Mana", "Regeneration", "Beneficial" }
            });

            buffDatabase.Add(new Buff(Buff.ID.SpeedBoost, "Speed Boost", "Increases movement speed significantly", Buff.BuffType.Temporary, 25f, 20f)
            {
                tags = new List<string> { "Speed", "Movement", "Beneficial" }
            });

            buffDatabase.Add(new Buff(Buff.ID.StrengthBoost, "Strength Boost", "Increases attack power and physical damage", Buff.BuffType.Temporary, 15f, 60f)
            {
                tags = new List<string> { "Strength", "Attack", "Beneficial" }
            });

            buffDatabase.Add(new Buff(Buff.ID.DefenseBoost, "Defense Boost", "Increases damage resistance and armor", Buff.BuffType.Temporary, 20f, 90f)
            {
                tags = new List<string> { "Defense", "Protection", "Beneficial" }
            });

            buffDatabase.Add(new Buff(Buff.ID.CriticalHitChanceBoost, "Critical Hit Chance", "Increases the chance of landing critical hits", Buff.BuffType.Temporary, 10f, 40f)
            {
                tags = new List<string> { "Critical", "Luck", "Beneficial" }
            });

            buffDatabase.Add(new Buff(Buff.ID.CriticalHitDamageBoost, "Critical Hit Damage", "Increases damage dealt by critical hits", Buff.BuffType.Temporary, 50f, 35f)
            {
                tags = new List<string> { "Critical", "Damage", "Beneficial" }
            });

            buffDatabase.Add(new Buff(Buff.ID.ExperienceBoost, "Experience Boost", "Increases experience points gained from actions", Buff.BuffType.Temporary, 100f, 300f)
            {
                tags = new List<string> { "Experience", "Learning", "Beneficial" }
            });

            buffDatabase.Add(new Buff(Buff.ID.LuckBoost, "Luck Boost", "Increases chance of rare drops and positive outcomes", Buff.BuffType.Temporary, 25f, 120f)
            {
                tags = new List<string> { "Luck", "Fortune", "Beneficial" }
            });

            // Debuffs (using negative IDs for distinction)
            buffDatabase.Add(new Buff((Buff.ID)(-1), "Poison", "Slowly drains health over time", Buff.BuffType.Debuff, -3f, 15f)
            {
                tags = new List<string> { "Poison", "Damage", "Debuff" }
            });

            buffDatabase.Add(new Buff((Buff.ID)(-2), "Weakness", "Reduces attack power and physical damage", Buff.BuffType.Debuff, -10f, 45f)
            {
                tags = new List<string> { "Weakness", "Attack", "Debuff" }
            });

            buffDatabase.Add(new Buff((Buff.ID)(-3), "Slowness", "Reduces movement speed", Buff.BuffType.Debuff, -15f, 25f)
            {
                tags = new List<string> { "Slow", "Movement", "Debuff" }
            });
        }

        /// <summary>
        /// Initializes the skill database with predefined skills. Clears any existing data.
        /// </summary>
        public static void InitializeSkillDatabase()
        {
            skillDatabase.Clear();

            // Combat Skills
            skillDatabase.Add(new Skill(Skill.ID.Combat, Skill.SkillType.Combat, 0f, 1f)
            {
                tags = new List<string> { "Combat", "Fighting", "Weapons" }
            });

            // Crafting Skills
            skillDatabase.Add(new Skill(Skill.ID.Crafting, Skill.SkillType.Crafting, 0f, 1f)
            {
                tags = new List<string> { "Crafting", "Creation", "Items", "CanCraft" }
            });

            // Gathering Skills
            skillDatabase.Add(new Skill(Skill.ID.Gathering, Skill.SkillType.Gathering, 0f, 1f)
            {
                tags = new List<string> { "Gathering", "Resources", "Collection", "CanGather" }
            });

            // Social Skills
            skillDatabase.Add(new Skill(Skill.ID.Social, Skill.SkillType.Social, 0f, 1f)
            {
                tags = new List<string> { "Social", "Communication", "Persuasion", "Speaking", "CanCommunicate" }
            });

            // Magic Skills
            skillDatabase.Add(new Skill(Skill.ID.Magic, Skill.SkillType.Magic, 0f, 1f)
            {
                tags = new List<string> { "Magic", "Spells", "Arcane", "CanCast" }
            });

            // Exploration Skills
            skillDatabase.Add(new Skill(Skill.ID.Exploration, Skill.SkillType.Exploration, 0f, 1f)
            {
                tags = new List<string> { "Exploration", "Discovery", "Navigation" }
            });

            // Survival Skills
            skillDatabase.Add(new Skill(Skill.ID.Survival, Skill.SkillType.Survival, 0f, 1f)
            {
                tags = new List<string> { "Survival", "Wilderness", "Endurance" }
            });

            // Stealth Skills
            skillDatabase.Add(new Skill(Skill.ID.Stealth, Skill.SkillType.Stealth, 0f, 1f)
            {
                tags = new List<string> { "Stealth", "Sneaking", "Hiding" }
            });

            // Engineering Skills
            skillDatabase.Add(new Skill(Skill.ID.Engineering, Skill.SkillType.Engineering, 0f, 1f)
            {
                tags = new List<string> { "Engineering", "Mechanics", "Technology", "CanEngineer" }
            });

            // Alchemy Skills
            skillDatabase.Add(new Skill(Skill.ID.Alchemy, Skill.SkillType.Alchemy, 0f, 1f)
            {
                tags = new List<string> { "Alchemy", "Potions", "Chemistry" }
            });
        }

        /// <summary>
        /// Initializes all game databases by calling their respective initialization methods.
        /// </summary>
        public static void InitializeAllDatabases()
        {
            InitializeEntityDatabase();
            InitializeItemDatabase();
            InitializeBuffDatabase();
            InitializeSkillDatabase();
        }
        #endregion // Database Initialization
    }
}

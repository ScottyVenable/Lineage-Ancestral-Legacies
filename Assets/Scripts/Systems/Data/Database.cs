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
        public float current;
        /// <summary>
        /// The maximum possible health value.
        /// </summary>
        public float max;

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
    }
    public struct Age
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
            Kaari = 1,
            Wolf = 2,
            Bear = 3,
            Goblin = 4,
            Orc = 5,
            Dragon = 6,
            Boar = 7,
            Troll = 8,
            Zombie = 9,
            Skeleton = 10,
            Vampire = 11,
            Werewolf = 12,
            Sabertooth = 13,
            Mammoth = 14,
            Phoenix = 15,
            Hydra = 16,
            Kraken = 17,
            Minotaur = 18,
            Golem = 19,
            Chimera = 20,
            Basilisk = 21,
            Centaur = 22,
            Harpy = 23,
            Siren = 24,
            Yeti = 25,
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

        //Todo: Additional stat management methods can be added here, such as:
        // - GetStatByID(Stat.ID id) for direct stat access
        // - ApplyStatModifier(Stat.ID id, float modifier, float duration)
        // - GetCombatPower() for calculating overall combat effectiveness
        // - RefreshAllStats() for recalculating all stat values

        /// <summary>Additional stats for comprehensive entity management.</summary>
        public Stat stamina;
        public Stat strength;
        public Stat agility;
        public Stat intelligence;
        public Stat magicPower;
        public Stat magicDefense;
        public Stat criticalHitChance;
        public Stat criticalHitDamage;
        public Stat luck;
        public Stat charisma;        /// <summary>
        /// Needs system stats - manage survival requirements.
        /// </summary>
        public Stat hunger;
        public Stat thirst;
        public Stat energy;
        public Stat rest;

        /// <summary>
        /// Experience and leveling stats.
        /// </summary>
        public Stat experience;
        public Stat levelStat;

        /// <summary>List of currently active buffs on this entity.</summary>
        public List<Buff> activeBuffs;        /// <summary>Current state of the entity for state machine behavior.</summary>
        public State currentState;
        public bool isInCombat => currentState.stateID == (int)State.ID.Attacking || currentState.stateID == (int)State.ID.Defending;
        public bool canCraft => tags.Contains("CanCraft");

        /// <summary>List of available states this entity can transition to.</summary>
        public List<State> availableStates;

        /// <summary>
        /// Applies a buff to this entity.
        /// </summary>
        /// <param name="buff">The buff to apply.</param>
        public void ApplyBuff(Buff buff)
        {
            if (activeBuffs == null) activeBuffs = new List<Buff>();
            activeBuffs.Add(buff);
            // Apply buff effects based on buff type
            switch (buff.buffID)
            {
                case (int)Buff.ID.HealthRegen:
                    health = health.Heal(buff.strength);
                    break;
                case (int)Buff.ID.ManaRegen:
                    mana.ModifyStat(buff.strength);
                    break;
                case (int)Buff.ID.SpeedBoost:
                    speed.ModifyStat(buff.strength);
                    break;
                case (int)Buff.ID.StrengthBoost:
                    attack.ModifyStat(buff.strength);
                    break;
                case (int)Buff.ID.DefenseBoost:
                    defense.ModifyStat(buff.strength);
                    break;
            }
        }

        /// <summary>
        /// Removes a buff from this entity.
        /// </summary>
        /// <param name="buffID">The ID of the buff to remove.</param>
        public void RemoveBuff(int buffID)
        {
            if (activeBuffs == null) return;

            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                if (activeBuffs[i].buffID == buffID)
                {
                    // Remove buff effects
                    Buff buff = activeBuffs[i];
                    switch (buff.buffID)
                    {
                        case (int)Buff.ID.SpeedBoost:
                            speed.ModifyStat(-buff.strength);
                            break;
                        case (int)Buff.ID.StrengthBoost:
                            attack.ModifyStat(-buff.strength);
                            break;
                        case (int)Buff.ID.DefenseBoost:
                            defense.ModifyStat(-buff.strength);
                            break;
                    }
                    activeBuffs.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Updates entity stats with modifiers from buffs and other sources.
        /// </summary>
        public void UpdateStats()
        {
            // Reset current values to base values
            mana.currentValue = mana.baseValue;
            attack.currentValue = attack.baseValue;
            defense.currentValue = defense.baseValue;
            speed.currentValue = speed.baseValue;

            // Apply active buff modifiers
            if (activeBuffs != null)
            {
                foreach (var buff in activeBuffs)
                {
                    switch (buff.buffID)
                    {
                        case (int)Buff.ID.SpeedBoost:
                            speed.currentValue += buff.strength;
                            break;
                        case (int)Buff.ID.StrengthBoost:
                            attack.currentValue += buff.strength;
                            break;
                        case (int)Buff.ID.DefenseBoost:
                            defense.currentValue += buff.strength;
                            break;
                    }
                }
            }
        }        /// <summary>
                 /// Heals the entity by a specified amount.
                 /// </summary>
                 /// <param name="amount">Amount of health to restore.</param>
        public void Heal(float amount)
        {
            health = health.Heal(amount);
        }        /// <summary>
                 /// Damages the entity by a specified amount.
                 /// </summary>
                 /// <param name="damage">Amount of damage to apply.</param>
        public void TakeDamage(float damage)
        {
            float actualDamage = Mathf.Max(0, damage - defense.currentValue);
            health = health.TakeDamage(actualDamage);

            if (health.current <= 0)
            {
                isAlive = false;
                ChangeState(State.ID.Dead);
            }
        }        /// <summary>
                 /// Changes the entity's current state.
                 /// </summary>
                 /// <param name="newStateID">The ID of the new state.</param>
        public void ChangeState(State.ID newStateID)
        {
            if (availableStates != null)
            {
                var newState = availableStates.Find(s => s.stateID == (int)newStateID);
                if (newState.stateID == (int)newStateID) // Valid state found
                {
                    currentState = newState;
                }
            }
        }        /// <summary>
                 /// Initializes entity states based on tags and entity type.
                 /// </summary>
        public void InitializeStates()
        {
            if (availableStates == null) availableStates = new List<State>();

            // Add basic states all entities can have
            availableStates.Add(new State(State.ID.Idle, "Idle", "Entity is idle and not performing any specific action"));

            // Add states based on entity type
            if (entityType.Contains(EntityType.PlayerControlled))
            {
                availableStates.Add(new State(State.ID.Exploring, "Exploring", "Entity is exploring the environment"));
                availableStates.Add(new State(State.ID.Crafting, "Crafting", "Entity is crafting items or equipment"));
                availableStates.Add(new State(State.ID.Resting, "Resting", "Entity is resting or recovering"));
            }

            if (entityType.Contains(EntityType.Animal) || entityType.Contains(EntityType.Monster))
            {
                availableStates.Add(new State(State.ID.Patrolling, "Patrolling", "Entity is patrolling an area"));
                availableStates.Add(new State(State.ID.Hunting, "Hunting", "Entity is actively hunting for resources or prey"));
                availableStates.Add(new State(State.ID.Fleeing, "Fleeing", "Entity is fleeing from danger"));
            }

            if (aggressionType == AggressionType.Aggressive)
            {
                availableStates.Add(new State(State.ID.Attacking, "Attacking", "Entity is attacking a target"));
            }

            // Set initial state
            currentState = availableStates[0]; // Start with first available state (usually Idle)
        }

        // Game Balance
        /// <summary>A modifier affecting the value (e.g., loot drop value) of this entity.</summary>
        public int valueModifier;

        /// <summary>A modifier affecting the experience points granted by this entity.</summary>
        public int experienceModifier;

        // Utility
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;        /// <summary>
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
            this.valueModifier = valueModifier;
            this.experienceModifier = experienceModifier;
            
            // Initialize entity type
            entityType = new List<EntityType>();
            
            // Initialize collections
            activeBuffs = new List<Buff>();
            availableStates = new List<State>();
            tags = new List<string>();
            
            // Initialize default state
            currentState = new State(State.ID.Idle, "Idle", "Entity is idle and not performing any specific action");
            
            mana = new Stat(Stat.ID.Mana, "Mana", 0f);
            attack = new Stat(Stat.ID.Attack, "Attack", 0f);
            defense = new Stat(Stat.ID.Defense, "Defense", 0f);
            speed = new Stat(Stat.ID.Speed, "Speed", 0f);
            stamina = new Stat(Stat.ID.Stamina, "Stamina", 100f);
            strength = new Stat(Stat.ID.Strength, "Strength", 10f);
            agility = new Stat(Stat.ID.Agility, "Agility", 10f);
            intelligence = new Stat(Stat.ID.Intelligence, "Intelligence", 10f);
            magicPower = new Stat(Stat.ID.MagicPower, "Magic Power", 0f);
            magicDefense = new Stat(Stat.ID.MagicDefense, "Magic Defense", 0f);
            criticalHitChance = new Stat(Stat.ID.CriticalHitChance, "Critical Hit Chance", 0.05f);
            criticalHitDamage = new Stat(Stat.ID.CriticalHitDamage, "Critical Hit Damage", 1.5f);
            luck = new Stat(Stat.ID.Luck, "Luck", 10f);
            charisma = new Stat(Stat.ID.Charisma, "Charisma", 10f);            // Initialize needs stats with appropriate defaults
            hunger = new Stat(Stat.ID.Hunger, "Hunger", 100f, 0f, 100f);
            thirst = new Stat(Stat.ID.Thirst, "Thirst", 100f, 0f, 100f);
            energy = new Stat(Stat.ID.Energy, "Energy", 100f, 0f, 100f);
            rest = new Stat(Stat.ID.Rest, "Rest", 100f, 0f, 100f);

            // Initialize experience and level stats
            experience = new Stat(Stat.ID.Experience, "Experience", 0f, 0f, float.MaxValue);
            levelStat = new Stat(Stat.ID.Level, "Level", level, 1f, float.MaxValue);
        }

        //Todo: Implement advanced entity behavior systems such as:
        // - State machine transitions with conditions and timers
        // - AI decision trees based on entity stats and environmental factors  
        // - Social interaction systems for entities to communicate
        // - Memory system for entities to remember past events and entities
        // - Goal-oriented action planning (GOAP) for complex behaviors

        //TODO: Make skills influence base values like speed, attack, defense, etc.

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
            Resting = 5, // Represents a state where the entity is resting or recovering (includes sleeping, meditation, or idle recovery)
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
            Sleeping = 21, // Represents a state where the entity is specifically sleeping
            // Special states
            Dead = 19, // Represents a state where the entity is dead
            Unconscious = 20, // Represents a state where the entity is incapacitated but not dead
        }

        /// <summary>
        /// The unique identifier for this state, often corresponds to <see cref="ID"/>.
        /// </summary>
        public int stateID;
        /// <summary>The display name of the state.</summary>
        public string stateName;        /// <summary>A short description of the state.</summary>
        public string stateDescription;

        /// <summary>Duration this state has been active (in seconds).</summary>
        public float stateDuration;

        /// <summary>Priority level for state transitions (higher values = higher priority).</summary>
        public int priority;

        /// <summary>Whether this state can be interrupted by other states.</summary>
        public bool canBeInterrupted;

        /// <summary>Energy cost per second while in this state.</summary>
        public float energyCostPerSecond;

        /// <summary>
        /// Determines the logic of the state and what occurs when the state is active.
        /// This method should be called regularly to update state behavior.
        /// </summary>
        public void UpdateState(Entity entity)
        {
            stateDuration += Time.deltaTime;
            
            // Apply energy cost
            if (energyCostPerSecond > 0 && entity.stamina.currentValue > 0)
            {
                entity.stamina.ModifyStat(-energyCostPerSecond * Time.deltaTime);
            }

            // State-specific logic can be added here or handled by external state machine
        }

        /// <summary>
        /// Called when entering this state.
        /// </summary>
        public void OnEnterState(Entity entity)
        {
            stateDuration = 0f;
            // State-specific entry logic can be added here
        }

        /// <summary>
        /// Called when exiting this state.
        /// </summary>
        public void OnExitState(Entity entity)
        {
            // State-specific exit logic can be added here
        }        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the state.</param>
        /// <param name="name">The name of the state.</param>
        /// <param name="description">The description of the state.</param>
        /// <param name="priority">Priority level for state transitions (default: 1).</param>
        /// <param name="canBeInterrupted">Whether this state can be interrupted (default: true).</param>
        /// <param name="energyCostPerSecond">Energy cost per second while in this state (default: 0).</param>
        public State(ID id, string name, string description, int priority = 1, bool canBeInterrupted = true, float energyCostPerSecond = 0f)
        {
            stateID = (int)id;
            stateName = name;
            stateDescription = description;
            stateDuration = 0f;
            this.priority = priority;
            this.canBeInterrupted = canBeInterrupted;
            this.energyCostPerSecond = energyCostPerSecond;
        }

        /// <summary>
        /// Checks if this state can transition to another state based on priority and interruption rules.
        /// </summary>
        /// <param name="newState">The state to transition to.</param>
        /// <returns>True if transition is allowed, false otherwise.</returns>
        public bool CanTransitionTo(State newState)
        {
            // Cannot transition to same state
            if (stateID == newState.stateID) return false;
            
            // Can always transition if current state can be interrupted
            if (canBeInterrupted) return true;
            
            // Can transition if new state has higher priority
            return newState.priority > priority;
        }

        //Todo: Transition the state logic from other scripts to pull from this data.
        // This state system provides a foundation for AI behavior management.
        // External scripts should use Entity.ChangeState() method to change states
        // and can query Entity.currentState for current behavior information.
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
        }

        /// <summary>
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
        {            currentLevel = level;
            experienceToNextLevel = experience;
            maxLevel = 100; // Default max level
            experiencePerLevel = 100; // Default experience per level
            modifiers = new StatModifiers(); // Default empty modifiers
            traitRewards = new List<Trait>(); // Empty list
            skillRewards = new List<Skill>(); // Empty list
            itemRewards = new List<Item>(); // Empty list
        }

        /// <summary>
        /// Adds experience and levels up if threshold is reached.
        /// </summary>
        /// <param name="expPoints">Experience points to add.</param>
        /// <returns>True if leveled up, false otherwise.</returns>
        public bool AddExperience(int expPoints)
        {
            int oldLevel = currentLevel;
            experienceToNextLevel -= expPoints;
            
            while (experienceToNextLevel <= 0 && currentLevel < maxLevel)
            {
                currentLevel++;
                experienceToNextLevel += experiencePerLevel * currentLevel; // Scaling difficulty
            }
            
            return currentLevel > oldLevel;
        }

        /// <summary>
        /// Checks if enough experience exists to reach the next level.
        /// </summary>
        /// <returns>True if ready to level up, false otherwise.</returns>
        public bool CanLevelUp()
        {
            return experienceToNextLevel <= 0 && currentLevel < maxLevel;
        }

        /// <summary>
        /// Gets the total experience invested in this leveling system.
        /// </summary>
        /// <returns>Total experience spent.</returns>
        public int GetTotalExperience()
        {
            int totalExp = 0;
            for (int i = 1; i < currentLevel; i++)
            {
                totalExp += experiencePerLevel * i;
            }
            return totalExp + (experiencePerLevel * currentLevel - experienceToNextLevel);
        }

        /// <summary>
        /// Gets the percentage progress toward the next level.
        /// </summary>
        /// <returns>Progress percentage (0.0 to 1.0).</returns>
        public float GetProgressToNextLevel()
        {
            int currentLevelExp = experiencePerLevel * currentLevel;
            int earnedExp = currentLevelExp - experienceToNextLevel;
            return currentLevelExp > 0 ? (float)earnedExp / currentLevelExp : 1f;
        }

        //Todo: Consider renaming to ExperienceSystem or ProgressionSystem for clarity.
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

    /// <summary>
    /// Represents a quest or mission that can be assigned to entities.
    /// </summary>
    public struct Quest
    {
        /// <summary>
        /// The unique identifiers for predefined or hardcoded quests. ID is usually the name of the quest.
        /// This can be used to reference specific quests in the game.
        /// </summary>
        public enum ID
        {
            GatherQuest_IGotBerries = 0, //IDEA: One of the first quests, where the pops are tasked with gathering berries for the lineage
            DefendQuest_UnderAttack = 1, //IDEA: A quest to defend the village from an incoming attack, possibly from goblins or bandits.
            HuntingQuest_FromHowlToYelp = 2, //IDEA: A quest to hunt a pack of wolves nearby
            KnowledgeQuest_TheMarkOfTheBeast = 3, //Possibly a quest when a pop is inflicted with a werewolf curse.


        }
        public enum Type
        {
            GatherResources = 0, // Represents quests that involve gathering resources or items
            DefendTerritory = 1, // Represents quests that involve defending a location or entity from threats
            ExploreArea = 2, // Represents quests that involve exploring new areas or dungeons
            CraftItems = 3, // Represents quests that involve crafting items or equipment
            SocialInteraction = 4, // Represents quests that involve interacting with NPCs or other entities
            KillTarget = 5, // Represents quests that involve hunting creatures or animals
            Knowledge = 6, // Represents quests that involve learning knowledge or discovering lore.
            EscortNPC = 7, // Represents quests that involve escorting an NPC to a destination
            RescueMission = 8, // Represents quests that involve rescuing an NPC or entity from danger
            Diplomatic = 9, // Represents quests that involve diplomatic negotiations or interactions
            Investigation = 10, // Represents quests that involve investigating a mystery or event
            Genetics = 11, // Represents quests that involve genetic research or experimentation
        }

        public enum Status
        {
            NotStarted = 0,
            InProgress = 1,
            Completed = 2,
            Failed = 3
        }

        public ID questID;
        public string questName;
        public string description;
        public Status status;
        public List<Objective> objectives;
        public List<Item> rewards;
        public int experienceReward;
        public int questCompletionPercentage; // Optional: Percentage of quest completion, useful for tracking progress
        public Type questType; // Optional: Type of the quest, useful for categorization or filtering

        public Quest(ID id, string name, string description)
        {
            questID = id;
            questName = name;
            this.description = description;
            status = Status.NotStarted;
            objectives = new List<Objective>(); ///todo: make a struct called Objectives and create objectives that can be added to quests that holds the name, description, and completion status.
            rewards = new List<Item>(); ///todo: add a tag to this item (if not currency) called "objectiveReward" to indicate that this item is a reward for completing the quest and show what quest it was rewarded for.
            experienceReward = 0;
            questCompletionPercentage = 0;
            questType = Type.GatherResources; // Default quest type
        }
    }

    public struct Objective
    {
        public enum ID
        {
            Collect = 0,
            Defeat = 1,
            Explore = 2,
            Craft = 3,
            Interact = 4,
            TalkToNPC = 5
        }

        public ID objectiveID;
        public string objectiveName;
        public string description;
        public bool isCompleted;
        public List<Item> objectiveReward; // Optional reward item for completing this objective
        public Quest quest; // Optional reference to the quest this objective belongs to
        public List<string> tags; // Optional tags for categorization or special mechanics
        public List<NPC> relatedNPCs; // Optional list of NPCs related to this objective
        public enum Difficulty{
            Easy = 0,
            Medium = 1,
            Hard = 2,
            Expert = 3
        }
        public Stat experienceRewardLineage; // Optional experience reward for completing this objective
            //TODO: Add a class for the Lineage, and the experience will be added to the lineage of the entity that completes this objective.
        public Stat experienceRewardPersonal; // Optional personal experience reward for if an individual pop completes this objective

        public Difficulty difficultyLevel; // Optional difficulty level associated with this objective
        public Objective(ID id, string name, string desc)
        {
            objectiveID = id;
            objectiveName = name;
            description = desc;
            isCompleted = false;
            objectiveReward = new List<Item>();
            quest = default(Quest);
            tags = new List<string>();
            relatedNPCs = new List<NPC>();
            experienceRewardLineage = new Stat(Stat.ID.Experience, "Lineage Experience", 0f);
            experienceRewardPersonal = new Stat(Stat.ID.Experience, "Personal Experience", 0f);
            difficultyLevel = Difficulty.Easy;
        }
    }
    /// <summary>
    /// Represents genetic information for hereditary traits.
    /// </summary>
    public struct Genetics
    {
        public enum GeneType
        {
            Stat_Modifier = 0, // Adds a bonus to a specific stat
            Acquire_Trait = 1, // Represents a specific trait
            Skill_Multiplier = 2, // Adds a boost to increasing a specific skill
            Appearance = 3, // Represents physical appearance traits
            Behavior = 4, // Represents behavioral traits
            Mutation = 5, // Represents a mutation that can affect stats or abilities.
            /// todo: Add more gene types as needed for future expansion.
        }

        public GeneType geneType;
        public float dominantValue;
        public float recessiveValue;
        public bool isDominant;

        public Genetics(GeneType type, float dominant, float recessive, bool isDominantExpressed = true)
        {
            geneType = type;
            dominantValue = dominant;
            recessiveValue = recessive;
            isDominant = isDominantExpressed;
        }

        public float GetExpressedValue()
        {
            return isDominant ? dominantValue : recessiveValue;
        }

        /// TODO: Research and implement basic genetic inheritance patterns.
        /// This could include Mendelian inheritance, polygenic traits, and mutations.

        /// TODO: Design a system for genetic mutations that can occur during gameplay.
        /// This could involve random mutations that affect stats, traits, or abilities.
    }

    /// <summary>
    /// Represents an NPC with behavior patterns and relationships.
    /// </summary>
    public struct NPC
    {
        public enum Archetype
        {
            Trader = 0,
            Warrior = 1,
            Scholar = 2,
            Healer = 3,
            Guide = 4,
            Hermit = 5
        }

        public string npcName;
        public Archetype archetype;
        public Entity entityData;
        public List<string> dialogueKeys;
        public Dictionary<string, int> relationships; // NPC name -> relationship value
        public List<Quest> availableQuests;

        public NPC(string name, Archetype type, Entity data)
        {
            npcName = name;
            archetype = type;
            entityData = data;
            dialogueKeys = new List<string>();
            relationships = new Dictionary<string, int>();
            availableQuests = new List<Quest>();
        }
    }

    /// <summary>
    /// Represents lore entries for world-building and immersion.
    /// </summary>
    public struct LoreEntry
    {
        public enum Category
        {
            History = 0,
            Legend = 1,
            Technology = 2,
            Culture = 3,
            Geography = 4,
            Bestiary = 5
        }

        public string title;
        public Category category;
        public string content;
        public bool isDiscovered;
        public List<string> relatedEntries;

        public LoreEntry(string title, Category category, string content)
        {
            this.title = title;
            this.category = category;
            this.content = content;
            isDiscovered = false;
            relatedEntries = new List<string>();
        }
    }

    /// <summary>
    /// Represents a journal entry for tracking events and discoveries.
    /// </summary>
    public struct JournalEntry
    {
        public enum EntryType
        {
            Discovery = 0,
            Event = 1,
            Quest = 2,
            Encounter = 3,
            Achievement = 4
        }

        public string title;
        public EntryType type;
        public string content;
        public DateTime timestamp;
        public bool isImportant;

        public JournalEntry(string title, EntryType type, string content, bool important = false)
        {
            this.title = title;
            this.type = type;
            this.content = content;
            timestamp = DateTime.Now;
            isImportant = important;
        }
    }

    // Todo: Consider adding Dialogue system, Weather system, Economy system, and Territory management structures.

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
        public static List<Quest> questDatabase = new List<Quest>(); // Added quest database
        public static List<Objective> objectiveDatabase = new List<Objective>(); // Added objective database
        public static List<Stat> statDatabase = new List<Stat>(); // Added stat database
        public static List<Trait> traitDatabase = new List<Trait>(); // Added trait database
        public static List<NPC> npcDatabase = new List<NPC>(); // Added NPC database
        public static List<LoreEntry> loreDatabase = new List<LoreEntry>(); // Added lore database
        public static List<JournalEntry> journalDatabase = new List<JournalEntry>(); // Added journal database
        public static List<Genetics> geneticsDatabase = new List<Genetics>(); // Added genetics database
        public static List<LevelingSystem> levelingSystemDatabase = new List<LevelingSystem>(); // Added leveling system database
        public static List<Skill> skillDatabase = new List<Skill>(); // Added skill database


            /// <summary>
            /// Retrieves a quest from the database by its unique ID.
            /// </summary>
            /// <param name="id">The ID of the quest to retrieve.</param>
            /// <returns>The found <see cref="Quest"/>, or a default quest if not found.</returns>
            public static Quest GetQuestByID(Quest.ID id)
            {
                Quest quest = questDatabase.Find(q => q.questID == id);
                if (quest.questID == id)
                {
                return quest;
                }
                return new Quest(id, "Unknown Quest", "Quest not found");
            }

            /// <summary>
            /// Retrieves an objective from the database by its unique ID.
            /// </summary>
            /// <param name="id">The ID of the objective to retrieve.</param>
            /// <returns>The found <see cref="Objective"/>, or a default objective if not found.</returns>
            public static Objective GetObjectiveByID(Objective.ID id)
            {
                Objective objective = objectiveDatabase.Find(o => o.objectiveID == id);
                if (objective.objectiveID == id)
                {
                return objective;
                }
                return new Objective(id, "Unknown Objective", "Objective not found");
            }

            /// <summary>
            /// Retrieves a trait from the database by its unique ID.
            /// </summary>
            /// <param name="id">The ID of the trait to retrieve.</param>
            /// <returns>The found <see cref="Trait"/>, or a default trait if not found.</returns>
            public static Trait GetTraitByID(Trait.ID id)
            {
                Trait trait = traitDatabase.Find(t => t.traitID == id);
                if (trait.traitID == id)
                {
                return trait;
                }
                return new Trait(id, "Unknown Trait", "Trait not found", "Unknown");
            }

            /// <summary>
            /// Retrieves an NPC from the database by name.
            /// </summary>
            /// <param name="name">The name of the NPC to retrieve.</param>
            /// <returns>The found <see cref="NPC"/>, or a default NPC if not found.</returns>
            public static NPC GetNPCByName(string name)
            {
                NPC npc = npcDatabase.Find(n => string.Equals(n.npcName, name, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(npc.npcName))
                {
                return npc;
                }
                return new NPC(name, NPC.Archetype.Trader, new Entity("Unknown NPC", Entity.ID.Pop));
            }

            /// <summary>
            /// Retrieves a lore entry from the database by title.
            /// </summary>
            /// <param name="title">The title of the lore entry to retrieve.</param>
            /// <returns>The found <see cref="LoreEntry"/>, or a default entry if not found.</returns>
            public static LoreEntry GetLoreEntryByTitle(string title)
            {
                LoreEntry entry = loreDatabase.Find(l => string.Equals(l.title, title, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(entry.title))
                {
                return entry;
                }
                return new LoreEntry(title, LoreEntry.Category.History, "Lore entry not found");
            }

            /// <summary>
            /// Retrieves all quests of a specific type.
            /// </summary>
            /// <param name="questType">The type of quests to retrieve.</param>
            /// <returns>A list of quests matching the specified type.</returns>
            public static List<Quest> GetQuestsByType(Quest.Type questType)
            {
                return questDatabase.FindAll(q => q.questType == questType);
            }

            /// <summary>
            /// Retrieves all quests with a specific status.
            /// </summary>
            /// <param name="status">The status of quests to retrieve.</param>
            /// <returns>A list of quests matching the specified status.</returns>
            public static List<Quest> GetQuestsByStatus(Quest.Status status)
            {
                return questDatabase.FindAll(q => q.status == status);
            }

            /// <summary>
            /// Retrieves all objectives that are completed.
            /// </summary>
            /// <returns>A list of completed objectives.</returns>
            public static List<Objective> GetCompletedObjectives()
            {
                return objectiveDatabase.FindAll(o => o.isCompleted);
            }

            /// <summary>
            /// Retrieves all traits by category.
            /// </summary>
            /// <param name="category">The category of traits to retrieve.</param>
            /// <returns>A list of traits in the specified category.</returns>
            public static List<Trait> GetTraitsByCategory(string category)
            {
                return traitDatabase.FindAll(t => string.Equals(t.category, category, StringComparison.OrdinalIgnoreCase));
            }

            /// <summary>
            /// Retrieves all NPCs of a specific archetype.
            /// </summary>
            /// <param name="archetype">The archetype of NPCs to retrieve.</param>
            /// <returns>A list of NPCs matching the specified archetype.</returns>
            public static List<NPC> GetNPCsByArchetype(NPC.Archetype archetype)
            {
                return npcDatabase.FindAll(n => n.archetype == archetype);
            }

            /// <summary>
            /// Retrieves all lore entries of a specific category.
            /// </summary>
            /// <param name="category">The category of lore entries to retrieve.</param>
            /// <returns>A list of lore entries in the specified category.</returns>
            public static List<LoreEntry> GetLoreEntriesByCategory(LoreEntry.Category category)
            {
                return loreDatabase.FindAll(l => l.category == category);
            }

            /// <summary>
            /// Retrieves all discovered lore entries.
            /// </summary>
            /// <returns>A list of discovered lore entries.</returns>
            public static List<LoreEntry> GetDiscoveredLoreEntries()
            {
                return loreDatabase.FindAll(l => l.isDiscovered);
            }

            /// <summary>
            /// Retrieves all journal entries of a specific type.
            /// </summary>
            /// <param name="entryType">The type of journal entries to retrieve.</param>
            /// <returns>A list of journal entries matching the specified type.</returns>
            public static List<JournalEntry> GetJournalEntriesByType(JournalEntry.EntryType entryType)
            {
                return journalDatabase.FindAll(j => j.type == entryType);
            }

            /// <summary>
            /// Retrieves all important journal entries.
            /// </summary>
            /// <returns>A list of important journal entries.</returns>
            public static List<JournalEntry> GetImportantJournalEntries()
            {
                return journalDatabase.FindAll(j => j.isImportant);
            }

            /// <summary>
            /// Retrieves all genetics of a specific type.
            /// </summary>
            /// <param name="geneType">The type of genetics to retrieve.</param>
            /// <returns>A list of genetics matching the specified type.</returns>
            public static List<Genetics> GetGeneticsByType(Genetics.GeneType geneType)
            {
                return geneticsDatabase.FindAll(g => g.geneType == geneType);
            }

            /// <summary>
            /// Gets updated database counts including all databases.
            /// </summary>
            /// <returns>A dictionary with database names and their entry counts.</returns>
            public static Dictionary<string, int> GetDatabaseCounts()
            {
                return new Dictionary<string, int>
                {
                { "Entities", entityDatabase.Count },
                { "Items", itemDatabase.Count },
                { "Buffs", buffDatabase.Count },
                { "Skills", skillDatabase.Count },
                { "Quests", questDatabase.Count },
                { "Objectives", objectiveDatabase.Count },
                { "Stats", statDatabase.Count },
                { "Traits", traitDatabase.Count },
                { "NPCs", npcDatabase.Count },
                { "Lore Entries", loreDatabase.Count },
                { "Journal Entries", journalDatabase.Count },
                { "Genetics", geneticsDatabase.Count },
                { "Leveling Systems", levelingSystemDatabase.Count }
                };
            }


        #endregion // Databases

        #region Data Retrieval Methods        /// <summary>
        /// Retrieves an entity from the database by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>The found <see cref="Entity"/>, or a default "Unknown" entity if not found.</returns>
        public static Entity GetEntityByID(int id)
        {
            Entity entity = entityDatabase.Find(e => e.entityID == id);
            if (entity.entityID != 0)
            {
                return entity;
            }
            // Return a default entity if not found
            return new Entity("Entity_" + id, (Entity.ID)id);
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
            }            // Return a default item if not found
            return new Item("Item_" + itemId, (Item.ID)itemId, Item.ItemType.Miscellaneous);
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
            
            // Check if skill is not default (structs can't be null, but we can check if it matches the ID)
            if (skill.skillID == id)
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
                case Stat.ID.Hunger:
                    defaultMax = 100f;
                    statType = Stat.StatType.Primary;
                    description = "Current hunger level - decreases over time, affects health when low";
                    break;
                case Stat.ID.Thirst:
                    defaultMax = 100f;
                    statType = Stat.StatType.Primary;
                    description = "Current thirst level - decreases over time, affects health when low";
                    break;
                case Stat.ID.Energy:
                    defaultMax = 100f;
                    statType = Stat.StatType.Primary;
                    description = "Current energy level - decreases with activity, affects movement and actions";
                    break;
                case Stat.ID.Rest:
                    defaultMax = 100f;
                    statType = Stat.StatType.Primary;
                    description = "Current rest level - decreases over time, affects energy recovery";
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
                tags = new List<string> { "Humanoid", "Playable", "CanCraft"},
                thirst = new Stat(Stat.ID.Thirst, "Thirst", 100f, 0f, 100f),
                hunger = new Stat(Stat.ID.Hunger, "Hunger", 100f, 0f, 100f),
                energy = new Stat(Stat.ID.Energy, "Energy", 100f, 0f, 100f),

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
        }        /// <summary>
                 /// Initializes the skill database with predefined skills. Clears any existing data.
                 /// </summary>
        public static void InitializeSkillDatabase()
        {
            skillDatabase.Clear();

            // Combat Skills
            skillDatabase.Add(new Skill(Skill.ID.Combat, Skill.SkillType.Combat, 0f, 1)
            {
                tags = new List<string> { "Combat", "Fighting", "Weapons" }
            });

            // Crafting Skills
            skillDatabase.Add(new Skill(Skill.ID.Crafting, Skill.SkillType.Crafting, 0f, 1)
            {
                tags = new List<string> { "Crafting", "Creation", "Items", "CanCraft" }
            });

            // Gathering Skills
            skillDatabase.Add(new Skill(Skill.ID.Gathering, Skill.SkillType.Gathering, 0f, 1)
            {
                tags = new List<string> { "Gathering", "Resources", "Collection", "CanGather" }
            });

            // Social Skills
            skillDatabase.Add(new Skill(Skill.ID.Social, Skill.SkillType.Social, 0f, 1)
            {
                tags = new List<string> { "Social", "Communication", "Persuasion", "Speaking", "CanCommunicate" }
            });

            // Magic Skills
            skillDatabase.Add(new Skill(Skill.ID.Magic, Skill.SkillType.Magic, 0f, 1)
            {
                tags = new List<string> { "Magic", "Spells", "Arcane", "CanCast" }
            });

            // Exploration Skills
            skillDatabase.Add(new Skill(Skill.ID.Exploration, Skill.SkillType.Exploration, 0f, 1)
            {
                tags = new List<string> { "Exploration", "Discovery", "Navigation" }
            });

            // Survival Skills
            skillDatabase.Add(new Skill(Skill.ID.Survival, Skill.SkillType.Survival, 0f, 1)
            {
                tags = new List<string> { "Survival", "Wilderness", "Endurance" }
            });

            // Stealth Skills
            skillDatabase.Add(new Skill(Skill.ID.Stealth, Skill.SkillType.Stealth, 0f, 1)
            {
                tags = new List<string> { "Stealth", "Sneaking", "Hiding" }
            });

            // Engineering Skills
            skillDatabase.Add(new Skill(Skill.ID.Engineering, Skill.SkillType.Engineering, 0f, 1)
            {
                tags = new List<string> { "Engineering", "Mechanics", "Technology", "CanEngineer" }
            });

            // Alchemy Skills
            skillDatabase.Add(new Skill(Skill.ID.Alchemy, Skill.SkillType.Alchemy, 0f, 1)
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

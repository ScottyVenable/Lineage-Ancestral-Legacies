using UnityEngine;
using System.Collections.Generic;
using System;

namespace Lineage.Ancestral.Legacies.Database
{
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
        public readonly float Percentage => max > 0 ? current / max : 0f;
        /// <summary>
        /// Gets a value indicating whether the entity is alive (current health > 0).
        /// </summary>
        public readonly bool IsAlive => current > 0f;
        /// <summary>
        /// Gets a value indicating whether the health is above 50%.
        /// </summary>
        public readonly bool IsHealthy => Percentage > 0.5f;
        /// <summary>
        /// Gets a value indicating whether the health is below 25% (critical).
        /// </summary>
        public readonly bool IsCritical => Percentage < 0.25f;

        /// <summary>
        /// Reduces health by the specified damage amount.
        /// </summary>
        /// <param name="damage">The amount of damage to take.</param>
        /// <returns>A new Health struct with the updated health.</returns>
        public readonly Health TakeDamage(float damage)
        {
            return new Health(max, Mathf.Max(0f, current - damage));
        }

        /// <summary>
        /// Increases health by the specified amount, capped at maximum health.
        /// </summary>
        /// <param name="amount">The amount of health to restore.</param>
        /// <returns>A new Health struct with the updated health.</returns>
        public readonly Health Heal(float amount)
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
            this.maxAge = max;
            this.currentAge = current < 0 ? 0 : current;
            this.modifiers = ageModifiers;
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

    #endregion

    /// <summary>
    /// Defines how many items an entity can equip in a given slot.
    /// </summary>
    public struct EquipSlotCapacity
    {
        public EquipSlot slot;
        public int capacity;

        public EquipSlotCapacity(EquipSlot slot, int capacity)
        {
            this.slot = slot;
            this.capacity = capacity;
        }
    }

    #region Entity Data Structures

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
            NPC,
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
        public List<Buff> activeBuffs; // List of currently active buffs on this entity

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
        public Stat charisma;
        
        /// <summary>
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

        /// <summary>Current state of the entity for state machine behavior.</summary>
        public State currentState;
        public bool isInCombat => currentState.stateID == (int)State.ID.Attacking || currentState.stateID == (int)State.ID.Defending;
        public bool canCraft => tags.Contains("CanCraft");

        /// <summary>List of available states this entity can transition to.</summary>
        public List<State> availableStates;

        /// <summary>
        /// Equipment slot configuration for this entity.
        /// </summary>
        public List<EquipSlotCapacity> equipmentSlots;

        /// <summary>
        /// Applies a buff to this entity.
        /// </summary>
        /// <param name="buff">The buff to apply.</param>
        public int valueModifier;
        public int experienceModifier;
        public List<string> tags;
    }

    public struct Settlement
    {
        public enum ID
        {
            Village,
            Town,
            City,
            Capital,
            Outpost,
            Fort,
            Castle,
            Ruins
        }

        public int settlementID;
        public string settlementName;
        public string settlementDescription;
        public Sprite settlementIcon;
        public bool isPlayerControlled;
        public ID settlementType;
        public string settlementSize;
        public string controllingFaction;
        public int population;
        public int maxPopulation;
        public Dictionary<string, int> resources;
        public List<string> tags;
    }

    /// <summary>
    /// Represents an NPC with behavior patterns and relationships.
    /// </summary>
    public struct NPC
    {
        public string npcName;
        public Archetype archetype;
        public Entity entityData;
        public List<string> dialogueKeys;
        public Dictionary<string, int> relationships;
        public List<Quest> availableQuests;
    }

    public class Population
    {
        public enum ID
        {
            Human,
            Elf,
            Dwarf,
            Orc,
            Goblin,
            Animal,
            Monster,
            Mixed
        }

        public enum Type
        {
            Civilian,
            Military,
            Merchant,
            Noble,
            Religious,
            Criminal,
            Tribal,
            Nomadic
        }

        public ID populationID;
        public string populationName;
        public Location populationLocation;
        private readonly List<Entity> _members = new List<Entity>();
        public IReadOnlyList<Entity> Members => _members;

        private Type _populationType;
        public Type populationType
        {
            get => _populationType;
            set
            {
                _populationType = value;
                populationTypeString = value.ToString();
            }
        }
        public string populationTypeString { get; private set; }
        public int PopulationSize => _members.Count;
        public Settlement Settlement { get; }
    }

    #endregion
}

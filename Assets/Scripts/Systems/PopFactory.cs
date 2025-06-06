using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Database;
using Lineage.Entities;
using Lineage.Systems.TraitSystem;
using Lineage.Systems.Inventory;
using static Lineage.Database.Entity;

namespace Lineage.Systems
{
    /// <summary>
    /// Factory class for creating Pops using the GameData system instead of hardcoded PopData ScriptableObjects.
    /// This allows for procedural generation with inheritance, traits, and dynamic stats.
    /// </summary>
    public static class PopFactory
    {
        /// <summary>
        /// Creates a new Pop using GameData Entity system with optional parent genetics.
        /// </summary>
        /// <param name="parentA">Optional first parent for genetic inheritance</param>
        /// <param name="parentB">Optional second parent for genetic inheritance</param>
        /// <param name="popPrefab">The Pop prefab to instantiate</param>
        /// <param name="spawnPosition">Where to spawn the Pop</param>
        /// <returns>The created Pop GameObject</returns>
        public static GameObject CreatePop(Database.Entity? parentA = null, Database.Entity? parentB = null, GameObject popPrefab = null, Vector3 spawnPosition = default)
        {
            // Create base entity data from GameData
            Database.Entity popEntity = GeneratePopEntity(parentA, parentB); //todo: have a default parentA and parentB for first gen
            
            // Instantiate the Pop GameObject
            GameObject popObject = Object.Instantiate(popPrefab, spawnPosition, Quaternion.identity);
            Pop popComponent = popObject.GetComponent<Pop>();
            
            if (popComponent != null)
            {
                // Apply GameData to Pop
                ApplyEntityDataToPop(popEntity, popComponent);
            }
            
            return popObject;
        }        /// <summary>
        /// Generates a Pop Entity using GameData system with genetic inheritance and random traits.
        /// </summary>
        private static Entities.Entity GeneratePopEntity(Entities.Entity? parentA = null, Entity? parentB = null)
        {            // Start with a base human entity from GameData
            Entity baseEntity = GameData.GetEntityByID(Entity.ID.Kaari); // Using Kaari entity type for human-like beings
              // Create a new entity based on the base
            Entity popEntity = new Entity(
                name: GenerateRandomName(),
                id: Entity.ID.Kaari, // Using Kaari entity type for human-like beings
                faction: "Village",
                description: "A member of the Kaari population",
                rarity: Rarity.Common,
                level: 1,
                aggressionType: AggressionType.Neutral,
                healthValue: new Health(100f),
                usesMana: false,
                isAlive: true
            );

            // Apply genetic inheritance if parents exist
            if (parentA.HasValue && parentB.HasValue)
            {
                ApplyGeneticInheritance(ref popEntity, parentA.Value, parentB.Value);
            }
            else
            {
                // Generate random base stats for first generation
                GenerateRandomStats(ref popEntity);
            }

            // Add random traits
            AddRandomTraits(ref popEntity);
            
            // Initialize appropriate states for a civilian pop
            InitializePopStates(ref popEntity);
            
            return popEntity;
        }

        /// <summary>
        /// Applies genetic inheritance from two parent entities.
        /// </summary>
        private static void ApplyGeneticInheritance(ref Entity child, Entity parentA, Entity parentB)
        {
            // Inherit stats with some randomization
            child.strength = InheritStat(parentA.strength, parentB.strength);
            child.agility = InheritStat(parentA.agility, parentB.agility);
            child.intelligence = InheritStat(parentA.intelligence, parentB.intelligence);
            child.charisma = InheritStat(parentA.charisma, parentB.charisma);
            child.luck = InheritStat(parentA.luck, parentB.luck);

            // Inherit health with some variation
            float inheritedMaxHealth = (parentA.health.max + parentB.health.max) / 2f;
            inheritedMaxHealth += Random.Range(-10f, 10f); // Add some variation
            child.health = new Health(Mathf.Max(50f, inheritedMaxHealth));
        }

        /// <summary>
        /// Inherits a stat from two parents with genetic variation.
        /// </summary>
        private static Stat InheritStat(Stat parentAStat, Stat parentBStat)
        {
            float baseValue = (parentAStat.baseValue + parentBStat.baseValue) / 2f;
            baseValue += Random.Range(-5f, 5f); // Genetic variation
            baseValue = Mathf.Max(1f, baseValue); // Minimum stat value
            
            return new Stat(parentAStat.statID, parentAStat.statName, baseValue);
        }

        /// <summary>
        /// Generates random stats for first-generation Pops.
        /// </summary>
        private static void GenerateRandomStats(ref Entity popEntity)
        {
            popEntity.strength = new Stat(Stat.ID.Strength, "Strength", Random.Range(8f, 15f));
            popEntity.agility = new Stat(Stat.ID.Agility, "Agility", Random.Range(8f, 15f));
            popEntity.intelligence = new Stat(Stat.ID.Intelligence, "Intelligence", Random.Range(8f, 15f));
            popEntity.charisma = new Stat(Stat.ID.Charisma, "Charisma", Random.Range(8f, 15f));
            popEntity.luck = new Stat(Stat.ID.Luck, "Luck", Random.Range(8f, 15f));
            
            // Health variation
            float maxHealth = Random.Range(80f, 120f);
            popEntity.health = new Health(maxHealth);
        }

        /// <summary>
        /// Adds random traits from the GameData Trait system.
        /// </summary>
        private static void AddRandomTraits(ref Entity popEntity)
        {
            // Add 1-3 random traits
            int traitCount = Random.Range(1, 4);
            List<Trait.ID> availableTraits = System.Enum.GetValues(typeof(Trait.ID)).Cast<Trait.ID>().ToList();
            
            for (int i = 0; i < traitCount; i++)
            {
                if (availableTraits.Count > 0)
                {
                    Trait.ID randomTraitID = availableTraits[Random.Range(0, availableTraits.Count)];
                    availableTraits.Remove(randomTraitID); // Prevent duplicates
                      // Create trait and apply its effects to the entity
                    Trait trait = new Trait(randomTraitID, randomTraitID.ToString(), "Auto-generated trait", "Random");
                    ApplyTraitToEntity(ref popEntity, trait);
                }
            }
        }        /// <summary>
        /// Applies a trait's effects to an entity's stats.
        /// </summary>
        private static void ApplyTraitToEntity(ref Entity entity, Trait trait)
        {
            // Apply trait modifiers based on trait type
            switch (trait.traitID)
            {
                case Trait.ID.Strong:
                    entity.strength.ModifyStat(5f);
                    break;
                case Trait.ID.Agile:
                    entity.agility.ModifyStat(5f);
                    entity.speed.ModifyStat(2f);
                    break;
                case Trait.ID.Wise:
                    entity.intelligence.ModifyStat(5f);
                    break;
                case Trait.ID.Charismatic:
                    entity.charisma.ModifyStat(5f);
                    break;
                case Trait.ID.Lucky:
                    entity.luck.ModifyStat(5f);
                    break;
                case Trait.ID.Brave:
                    entity.attack.ModifyStat(2f);
                    entity.defense.ModifyStat(2f);
                    break;
                case Trait.ID.QuickWitted:
                    entity.intelligence.ModifyStat(3f);
                    entity.agility.ModifyStat(2f);
                    break;
                // Add more trait effects as needed
            }
        }

        /// <summary>
        /// Initializes appropriate states for a civilian Pop.
        /// </summary>
        private static void InitializePopStates(ref Entity popEntity)
        {
            popEntity.entityType = new List<EntityType> { EntityType.PlayerControlled };
            popEntity.InitializeStates();
            
            // Set initial state to Idle
            if (popEntity.availableStates != null && popEntity.availableStates.Count > 0)
            {
                popEntity.currentState = popEntity.availableStates[0]; // Idle state
            }
        }

        /// <summary>
        /// Applies GameData Entity information to a Pop component.
        /// </summary>
        private static void ApplyEntityDataToPop(Entity entityData, Pop popComponent)
        {
            // Basic information
            popComponent.popName = entityData.entityName;

            // Create PopData from Entity if needed for backward compatibility
            if (popComponent.popData == null)
            {
                popComponent.popData = ScriptableObject.CreateInstance<PopData>();
            }
            
            popComponent.popData.maxHealth = entityData.health.max;
            // Map other stats as needed for your existing systems

            // Store the full entity data for advanced features
            // You might want to add an EntityData component to Pop for this
            StoreEntityDataInPop(popComponent, entityData);
        }

        /// <summary>
        /// Stores complete Entity data in the Pop for future use.
        /// You might want to create an EntityDataComponent for this.
        /// </summary>
        private static void StoreEntityDataInPop(Pop popComponent, Entity entityData)
        {
            // Option 1: Add EntityData as a component
            // EntityDataComponent entityComp = popComponent.GetComponent<EntityDataComponent>();
            // if (entityComp == null) entityComp = popComponent.gameObject.AddComponent<EntityDataComponent>();
            // entityComp.entityData = entityData;
            
            // Option 2: Store as serialized data in Pop (simpler approach)
            // For now, we'll store key data in Pop's existing fields
            popComponent.popName = entityData.entityName;
            // Add more mappings as needed
        }

        /// <summary>
        /// Generates a random name for a Pop.
        /// </summary>
        private static string GenerateRandomName()
        {
            string[] firstNames = { "Aelyn", "Baris", "Cira", "Daven", "Elyn", "Fynn", "Gara", "Hael", "Ira", "Jax" };
            string[] lastNames = { "Brightforge", "Swiftwind", "Ironheart", "Goldleaf", "Stormborn", "Earthshaper", "Moonwhisper", "Sunward", "Nightbane", "Starweaver" };
            
            return firstNames[Random.Range(0, firstNames.Length)] + " " + lastNames[Random.Range(0, lastNames.Length)];
        }
    }
}

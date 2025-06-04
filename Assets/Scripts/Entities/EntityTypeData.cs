using UnityEngine;
using Lineage.Ancestral.Legacies.Database;
using Lineage.Ancestral.Legacies.Components;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Entities
{
    /// <summary>
    /// Universal EntityTypeData that handles all entity types through configuration
    /// instead of requiring separate inherited classes.
    /// </summary>
    [CreateAssetMenu(fileName = "EntityTypeData", menuName = "Lineage/Entity Type Data")]
    public class EntityTypeData : ScriptableObject
    {
        [Header("Entity Type Configuration")]
        public EntityType entityType;
        public string entityTypeName;
        
        [Header("Behavior Configuration")]
        public bool hasNeedsDecay = true;
        public bool hasAging = true;
        public bool canCraft = false;
        public bool canSocialize = false;
        public bool canGather = true;
        public bool hasTerritory = false;
        public bool canHunt = false;
        public bool canFlee = true;
        
        [Header("Needs Decay Rates")]
        public float hungerDecayRate = 1f;
        public float thirstDecayRate = 1.5f;
        public float energyDecayRate = 0.8f;
        public float restDecayRate = 0.5f;
        
        [Header("Aging Configuration")]
        public float agingRate = 1f; // How fast they age
        public int maxAge = 100;
        
        [Header("Combat Configuration")]
        public float fleeHealthThreshold = 30f; // Flee when health below this %
        public float aggressionLevel = 50f; // How likely to engage in combat
        
        [Header("Movement Configuration")]
        public float wanderRadius = 10f;
        public float movementSpeed = 3.5f;
        public float territoryRadius = 15f;
        
        [Header("Social Configuration")]
        public float socialRadius = 5f;
        public int maxSocialGroup = 6;
        
        [Header("Resource Gathering")]
        public List<ResourceType> preferredResources = new List<ResourceType>();
        public float gatheringEfficiency = 1f;
        
        [Header("Crafting Configuration")]
        public List<string> knownRecipes = new List<string>();
        
        /// <summary>
        /// Initialize entity with type-specific configuration
        /// </summary>
        public virtual void OnInitialize(Entity entity)
        {
            var entityData = entity.GetComponent<EntityDataComponent>();
            if (entityData == null) return;
            
            // Configure based on entity type
            switch (entityType)
            {
                case EntityType.Pop:
                    InitializePop(entity, entityData);
                    break;
                case EntityType.Animal:
                    InitializeAnimal(entity, entityData);
                    break;
                case EntityType.Monster:
                    InitializeMonster(entity, entityData);
                    break;
                case EntityType.NPC:
                    InitializeNPC(entity, entityData);
                    break;
            }
            
            UnityEngine.Debug.Log($"Initialized {entityTypeName} entity with type: {entityType}");
        }
        
        /// <summary>
        /// Update entity behavior based on type configuration
        /// </summary>
        public virtual void OnUpdate(Entities.Entity entity)
        {
            if (!entity || !entity.isActiveAndEnabled) return;
            
            var entityData = entity.GetComponent<EntityDataComponent>();
            if (entityData == null) return;
            
            // Apply needs decay if enabled
            if (hasNeedsDecay)
            {
                ApplyNeedsDecay(entityData);
            }
            
            // Apply aging if enabled
            if (hasAging)
            {
                ApplyAging(entity, entityData);
            }
            
            // Check flee conditions
            if (canFlee && ShouldFlee(entityData))
            {
                TriggerFleeState(entity);
            }
        }
        
        /// <summary>
        /// Handle stat changes based on entity type
        /// </summary>
        public virtual void OnStatChanged(Entities.Entity entity, Stat.ID statID, float newValue)
        {
            switch (statID)
            {
                case Stat.ID.Health when newValue <= 0:
                    HandleDeath(entity);
                    break;
                case Stat.ID.Hunger when newValue <= 10f && canGather:
                    TriggerGatheringBehavior(entity);
                    break;
                case Stat.ID.Thirst when newValue <= 10f && canGather:
                    TriggerWaterSeeking(entity);
                    break;
                case Stat.ID.Energy when newValue <= 20f:
                    TriggerRestBehavior(entity);
                    break;
            }
        }
        
        /// <summary>
        /// Handle state changes based on entity type
        /// </summary>
        public virtual void OnStateChanged(Entities.Entity entity, State.ID newState)
        {
            switch (newState)
            {
                case State.ID.Aggressive when hasTerritory:
                    DefendTerritory(entity);
                    break;
                case State.ID.Social when canSocialize:
                    EngageInSocialBehavior(entity);
                    break;
                case State.ID.Crafting when canCraft:
                    StartCrafting(entity);
                    break;
            }
        }

        #region Type-Specific Initialization
        
        
        // Changed this to be for general entities instead of specific types.
        private void InitializeEntity(Entities.Entity entity, EntityDataComponent entityData)
        {
            // General entity initialization
            entity.name = entityData.Entity.entityName;

            // Set general capabilities
            hasNeedsDecay = true;
            hasAging = true;
            canCraft = false; //todo: set this to false for Entities that do not have the ability to craft
            canSocialize = false; // default to false, set true for specific types
            canGather = true;
        }

        #endregion

        #region Behavior Implementation

        private void ApplyNeedsDecay(EntityDataComponent entityData)
        {
            //todo: make this not apply to any entity that is not controlled by the player. No need to have need decay for AI entities.
            if (Time.time % 1f < Time.deltaTime) // Every second
            {
                entityData.ModifyStat(Stat.ID.Hunger, -hungerDecayRate * Time.deltaTime);
                entityData.ModifyStat(Stat.ID.Thirst, -thirstDecayRate * Time.deltaTime);
                entityData.ModifyStat(Stat.ID.Energy, -energyDecayRate * Time.deltaTime);
            }
        }
        
        private void ApplyAging(Entity entity, EntityDataComponent entityData)
        {
            //todo: Maybe make this not apply to any entity that is not controlled by the player. No need to have need decay for AI entities.
            // Age slowly over time
            if (Time.time % 60f < Time.deltaTime) // Every minute
            {
                int currentAge = entity.Age;
                entity.Age = Mathf.Min(currentAge + 1, maxAge);

                if (entity.Age >= maxAge)
                {
                    HandleDeath(entity);
                }
            }
        }
        
        private bool ShouldFlee(EntityDataComponent entityData)
        {
            float healthPercent = (entityData.GetStat(Stat.ID.Health).currentValue / 
                                  entityData.GetStat(Stat.ID.Health).maxValue) * 100f;
            return healthPercent <= fleeHealthThreshold;
        }
        
        private void TriggerFleeState(Entities.Entity entity)
        {
            // Set entity to fleeing state
            UnityEngine.Debug.Log($"{entity.name} is fleeing due to low health!");
            // Implementation would trigger behavior tree state change
        }
        
        private void TriggerGatheringBehavior(Entities.Entity entity)
        {
            UnityEngine.Debug.Log($"{entity.name} is hungry and looking for food!");
            // Implementation would trigger gathering behavior
        }
        
        private void TriggerWaterSeeking(Entities.Entity entity)
        {
            UnityEngine.Debug.Log($"{entity.name} is thirsty and seeking water!");
            // Implementation would trigger water seeking behavior
        }
        
        private void TriggerRestBehavior(Entities.Entity entity)
        {
            UnityEngine.Debug.Log($"{entity.name} is tired and needs rest!");
            // Implementation would trigger rest behavior
        }
        
        private void HandleDeath(Entities.Entity entity)
        {
            UnityEngine.Debug.Log($"{entity.name} has died!");
            // Implementation would handle death logic
        }
        
        private void DefendTerritory(Entities.Entity entity)
        {
            UnityEngine.Debug.Log($"{entity.name} is defending territory!");
            // Implementation would handle territorial defense
        }
        
        private void EngageInSocialBehavior(Entities.Entity entity)
        {
            UnityEngine.Debug.Log($"{entity.name} is socializing!");
            // Implementation would handle social interactions
        }
        
        private void StartCrafting(Entities.Entity entity)
        {
            UnityEngine.Debug.Log($"{entity.name} is crafting!");
            // Implementation would handle crafting behavior
        }
        
        #endregion
    }
    
    /// <summary>
    /// Enum defining the main entity types in the game
    /// </summary>
    public enum EntityType
    {
        Pop,
        Animal,
        Monster,
        NPC,
        Object,
        Structure
    }
    
    /// <summary>
    /// Enum for resource types that entities can prefer
    /// </summary>
    public enum ResourceType
    {
        Food,
        Water,
        Wood,
        Stone,
        Metal,
        Herbs,
        Berries
    }
}
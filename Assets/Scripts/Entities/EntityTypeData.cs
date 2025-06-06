using UnityEngine;
using Lineage.Database;
using System.Collections.Generic;

namespace Lineage.Entities
{
    /// <summary>
    /// Blackboard variable for behavior trees with support for different data types
    /// </summary>
    [System.Serializable]
    public class BlackboardVariable
    {
        [Header("Variable Configuration")]
        public string key = "";
        
        [Header("Variable Type")]
        public BlackboardVariableType type = BlackboardVariableType.Float;
        
        [Header("Values")]
        public string stringValue = "";
        public float floatValue = 0f;
        public int intValue = 0;
        public bool boolValue = false;
        
        /// <summary>
        /// Gets the value as the appropriate type
        /// </summary>
        public object GetValue()
        {
            switch (type)
            {
                case BlackboardVariableType.String: return stringValue;
                case BlackboardVariableType.Float: return floatValue;
                case BlackboardVariableType.Int: return intValue;
                case BlackboardVariableType.Bool: return boolValue;
                default: return stringValue;
            }
        }
        
        /// <summary>
        /// Sets the value from an object
        /// </summary>
        public void SetValue(object value)
        {
            switch (type)
            {
                case BlackboardVariableType.String:
                    stringValue = value?.ToString() ?? "";
                    break;
                case BlackboardVariableType.Float:
                    if (float.TryParse(value?.ToString(), out float f)) floatValue = f;
                    break;
                case BlackboardVariableType.Int:
                    if (int.TryParse(value?.ToString(), out int i)) intValue = i;
                    break;
                case BlackboardVariableType.Bool:
                    if (bool.TryParse(value?.ToString(), out bool b)) boolValue = b;
                    break;
            }
        }
    }
    
    /// <summary>
    /// Types of blackboard variables supported
    /// </summary>
    public enum BlackboardVariableType
    {
        String,
        Float,
        Int,
        Bool
    }

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
        
        [Header("Behavior Subtypes")]
        [Tooltip("Add behavior subtags like 'carnivore', 'herbivore', 'wolf', 'bandit', etc.")]
        public List<string> behaviorSubtags = new List<string>();
        
        [Header("Enhanced Resource Configuration")]
        [Tooltip("Resource tags for behavior trees: 'Food', 'Water', 'Water.Clean', 'Water.Dirty', 'Gatherable', 'Crafting'")]
        public List<string> entityResourceTags = new List<string>();
        
        [Header("Blackboard Configuration")]
        [Tooltip("Custom blackboard variables for behavior trees")]
        public List<BlackboardVariable> customBlackboardVariables = new List<BlackboardVariable>();
        
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
        public List<string> knownRecipes = new List<string>();        /// <summary>
        /// Initialize entity with type-specific configuration
        /// </summary>
        public virtual void OnInitialize(Entity entity)
        {
            if (entity == null) return;
            
            // Apply entity resource tags
            foreach (string tag in entityResourceTags)
            {
                entity.AddResourceTag(tag);
            }
            
            // Apply behavior subtags
            foreach (string subtag in behaviorSubtags)
            {
                entity.AddBehaviorSubtag(subtag);
            }
            
            // Apply custom blackboard variables
            foreach (var variable in customBlackboardVariables)
            {
                entity.SetBlackboardVariable(variable.key, variable.GetValue());
            }
              // Configure based on entity type
            switch (entityType)
            {
                case EntityType.Pop:
                    InitializePop(entity);
                    break;
                case EntityType.Animal:
                    InitializeAnimal(entity);
                    break;
                case EntityType.Monster:
                    InitializeMonster(entity);
                    break;
                case EntityType.NPC:
                    InitializeNPC(entity);
                    break;
            }
            
            UnityEngine.Debug.Log($"Initialized {entityTypeName} entity with type: {entityType}");
        }
          /// <summary>
        /// Initialize Pop entity
        /// </summary>
        private void InitializePop(Entity entity)
        {
            // Pop-specific initialization
            entity.AddBehaviorSubtag("social");
            entity.AddBehaviorSubtag("crafting");
            
            // Pops can gather all basic resources
            entity.AddResourceTag("Food");
            entity.AddResourceTag("Water");
            entity.AddResourceTag("Gatherable");
            entity.AddResourceTag("Crafting");
        }
        
        /// <summary>
        /// Initialize Animal entity
        /// </summary>
        private void InitializeAnimal(Entity entity)
        {
            // Animal-specific initialization based on subtags
            if (entity.HasBehaviorSubtag("carnivore"))
            {
                entity.AddBehaviorSubtag("hunter");
                entity.AddResourceTag("Meat");
            }
            else if (entity.HasBehaviorSubtag("herbivore"))
            {
                entity.AddBehaviorSubtag("forager");
                entity.AddResourceTag("Plants");
                entity.AddResourceTag("Berries");
            }
            
            // All animals need water
            entity.AddResourceTag("Water");
            
            // Wolf-specific behavior
            if (entity.HasBehaviorSubtag("wolf"))
            {
                entity.AddBehaviorSubtag("pack");
                entity.AddBehaviorSubtag("territorial");
            }
        }
        
        /// <summary>
        /// Initialize Monster entity
        /// </summary>
        private void InitializeMonster(Entity entity)
        {
            // Monster-specific initialization
            entity.AddBehaviorSubtag("aggressive");
            
            // Bandit-specific behavior
            if (entity.HasBehaviorSubtag("bandit"))
            {
                entity.AddBehaviorSubtag("raider");
                entity.AddBehaviorSubtag("opportunistic");
                entity.AddResourceTag("Loot");
            }
            
            // Basic monster needs
            entity.AddResourceTag("Food");
            entity.AddResourceTag("Water");
        }
        
        /// <summary>
        /// Initialize NPC entity
        /// </summary>
        private void InitializeNPC(Entity entity)
        {
            // NPC-specific initialization
            entity.AddBehaviorSubtag("social");
            entity.AddBehaviorSubtag("trader");
            
            // NPCs typically have specialized resources
            entity.AddResourceTag("Trade");
        }
          /// <summary>
        /// Update entity behavior based on type configuration
        /// </summary>
        public virtual void OnUpdate(Entities.Entity entity)
        {
            if (!entity || !entity.isActiveAndEnabled) return;
            
            // Apply needs decay if enabled
            if (hasNeedsDecay)
            {
                ApplyNeedsDecay(entity);
            }
            
            // Apply aging if enabled
            if (hasAging)
            {
                ApplyAging(entity);
            }
            
            // Check flee conditions
            if (canFlee && ShouldFlee(entity))
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
                case State.ID.Defending when hasTerritory:
                    DefendTerritory(entity);
                    break;
                case State.ID.Socializing when canSocialize:
                    EngageInSocialBehavior(entity);
                    break;
                case State.ID.Crafting when canCraft:
                    StartCrafting(entity);
                    break;
            }
        }        #region Type-Specific Initialization
        
        
        // Changed this to be for general entities instead of specific types.
        private void InitializeEntity(Entities.Entity entity)
        {
            // General entity initialization
            entity.name = entity.EntityName;

            // Set general capabilities
            hasNeedsDecay = true;
            hasAging = true;
            canCraft = false; //todo: set this to false for Entities that do not have the ability to craft
            canSocialize = false; // default to false, set true for specific types
            canGather = true;
        }

        #endregion

        #region Behavior Implementation

        private void ApplyNeedsDecay(Entity entity)
        {
            //todo: make this not apply to any entity that is not controlled by the player. No need to have need decay for AI entities.
            if (Time.time % 1f < Time.deltaTime) // Every second
            {
                entity.ModifyStat(Stat.ID.Hunger, -hungerDecayRate * Time.deltaTime);
                entity.ModifyStat(Stat.ID.Thirst, -thirstDecayRate * Time.deltaTime);
                entity.ModifyStat(Stat.ID.Energy, -energyDecayRate * Time.deltaTime);
            }
        }
        
        private void ApplyAging(Entity entity)
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
        
        private bool ShouldFlee(Entity entity)
        {
            float healthPercent = (entity.Health / entity.MaxHealth) * 100f;
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
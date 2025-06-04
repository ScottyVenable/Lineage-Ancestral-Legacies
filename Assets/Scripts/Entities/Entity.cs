using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;
using Lineage.Ancestral.Legacies.Database;
using Lineage.Ancestral.Legacies.Systems.Inventory;

namespace Lineage.Ancestral.Legacies.Entities
{
    /// <summary>
    /// Universal Entity component that handles all entity types and their data management.
    /// Combines entity behavior, stat management, and type-specific configuration.
    /// </summary>
    public class Entity : MonoBehaviour
    {
        [Header("Entity Configuration")]
        [SerializeField] private int entityID = 0;
        [SerializeField] private EntityTypeData entityTypeData;
        
        [Header("Entity Data")]
        [SerializeField] private Database.Entity _entityData;
        
        [Header("Identity")]
        [SerializeField] private string _entityName = "Unknown Entity";
        [SerializeField] private int _entityAge = 0;
        
        [Header("Base Stats")]
        [SerializeField] private float _entityHunger = 50f;
        [SerializeField] private float _entityThirst = 50f;
        [SerializeField] private float _entityEnergy = 50f;
        [SerializeField] private float _entitySpeed = 5f;
        [SerializeField] private float _entityHealth = 100f;
        [SerializeField] private float _entityMana = 50f;
        
        [Header("Max Stat Values")]
        [SerializeField] private float _entityMaxHunger = 100f;
        [SerializeField] private float _entityMaxThirst = 100f;
        [SerializeField] private float _entityMaxEnergy = 100f;
        [SerializeField] private float _entityMaxSpeed = 10f;
        [SerializeField] private float _entityMaxHealth = 100f;
        [SerializeField] private float _entityMaxMana = 100f;
        
        [Header("Ability Stats")]
        [SerializeField] private float _entityStrength = 10f;
        [SerializeField] private float _entityAgility = 10f;
        [SerializeField] private float _entityIntelligence = 10f;
        [SerializeField] private float _entityDefense = 10f;
        [SerializeField] private float _entityLuck = 5f;
        [SerializeField] private float _entityCharisma = 5f;
        
        [Header("Combat Stats")]
        [SerializeField] private float _entityAttack = 10f;
        [SerializeField] private float _entityMagicPower = 10f;
        [SerializeField] private float _entityMagicDefense = 10f;
        [SerializeField] private float _entityCriticalHitChance = 5f;
        [SerializeField] private float _entityCriticalHitDamage = 150f;
        
        [Header("Traits")]
        [SerializeField] private List<string> _entityTraits = new List<string>();
        
        [Header("Crafting")]
        [SerializeField] private bool _entityCanCraft = false;
        [SerializeField] private List<string> _craftingRecipes = new List<string>();
        
        [Header("Needs System Configuration")]
        [SerializeField] private bool enableNeedsDecay = true;
        [SerializeField] private float hungerDecayRate = 1f;
        [SerializeField] private float thirstDecayRate = 1.5f;
        [SerializeField] private float energyDecayRate = 0.8f;
        [SerializeField] private float restDecayRate = 0.5f;
        
        [Header("Runtime State")]
        public bool isInitialized = false;
        
        // Events
        public System.Action<Database.Entity> OnEntityDataChanged;
        public System.Action<Stat.ID, float> OnStatChanged;
        public System.Action<State.ID> OnStateChanged;
        
        // Component references
        private NavMeshAgent navAgent;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private InventoryComponent inventoryComponent;
        
        // Properties
        public int EntityID 
        { 
            get => entityID; 
            set => entityID = value; 
        }
        
        public string EntityName 
        { 
            get => _entityName; 
            set => _entityName = value; 
        }
        
        public int Age 
        { 
            get => _entityAge; 
            set => _entityAge = value; 
        }
        
        public Database.Entity EntityData 
        { 
            get => _entityData; 
            set 
            {
                _entityData = value;
                OnEntityDataChanged?.Invoke(_entityData);
                isInitialized = true;
            } 
        }
        
        public EntityTypeData TypeData 
        { 
            get => entityTypeData; 
            set => entityTypeData = value; 
        }
        
        // Stat properties (read-only, use ModifyStat to change)
        public float Health => GetStat(Stat.ID.Health).currentValue;
        public float MaxHealth => GetStat(Stat.ID.Health).maxValue;
        public float Mana => GetStat(Stat.ID.Mana).currentValue;
        public float MaxMana => GetStat(Stat.ID.Mana).maxValue;
        public float Hunger => GetStat(Stat.ID.Hunger).currentValue;
        public float Thirst => GetStat(Stat.ID.Thirst).currentValue;
        public float Energy => GetStat(Stat.ID.Energy).currentValue;
        public float Stamina => GetStat(Stat.ID.Stamina).currentValue;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            InitializeEntityData();
        }
        
        private void Start()
        {
            if (entityTypeData != null)
            {
                entityTypeData.OnInitialize(this);
            }
            
            LoadFromDatabase();
            UpdateVisuals();
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            // Update type-specific behavior
            if (entityTypeData != null)
            {
                entityTypeData.OnUpdate(this);
            }
            
            // Update needs decay if enabled
            if (enableNeedsDecay)
            {
                UpdateNeedsDecay();
            }
        }
        
        private void OnDestroy()
        {
            if (entityTypeData != null)
            {
                // entityTypeData.OnEntityDestroy(this); // If needed
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            navAgent = GetComponent<NavMeshAgent>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            inventoryComponent = GetComponent<InventoryComponent>();
        }
        
        private void InitializeEntityData()
        {
            if (_entityData.entityID == 0 && entityID != 0)
            {
                LoadFromDatabase();
            }
        }
        
        private void LoadFromDatabase()
        {
            if (entityID == 0) return;
            
            var database = DatabaseManager.Instance;
            if (database != null)
            {
                var entityFromDB = database.GetEntityByID(entityID);
                if (entityFromDB.HasValue)
                {
                    EntityData = entityFromDB.Value;
                    SyncSerializedFields();
                    UnityEngine.Debug.Log($"Loaded entity data for ID {entityID}: {EntityData.entityName}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Entity with ID {entityID} not found in database");
                }
            }
        }
        
        private void SyncSerializedFields()
        {
            // Sync serialized fields with entity data
            _entityName = _entityData.entityName;
            _entityAge = _entityData.level; // Assuming level represents age
            
            // Sync stats
            _entityHealth = _entityData.health.currentValue;
            _entityMana = _entityData.mana.currentValue;
            _entityHunger = _entityData.hunger.currentValue;
            _entityThirst = _entityData.thirst.currentValue;
            _entityEnergy = _entityData.energy.currentValue;
            
            // Sync max values
            _entityMaxHealth = _entityData.health.maxValue;
            _entityMaxMana = _entityData.mana.maxValue;
            _entityMaxHunger = _entityData.hunger.maxValue;
            _entityMaxThirst = _entityData.thirst.maxValue;
            _entityMaxEnergy = _entityData.energy.maxValue;
            
            // Sync ability stats
            _entityStrength = _entityData.strength.currentValue;
            _entityAgility = _entityData.agility.currentValue;
            _entityIntelligence = _entityData.intelligence.currentValue;
            _entityDefense = _entityData.defense.currentValue;
            _entityLuck = _entityData.luck.currentValue;
            _entityCharisma = _entityData.charisma.currentValue;
            
            // Sync combat stats
            _entityAttack = _entityData.attack.currentValue;
            _entityMagicPower = _entityData.magicPower.currentValue;
            _entityMagicDefense = _entityData.magicDefense.currentValue;
            _entityCriticalHitChance = _entityData.criticalHitChance.currentValue;
            _entityCriticalHitDamage = _entityData.criticalHitDamage.currentValue;
        }
        
        #endregion
        
        #region Stat Management
        
        /// <summary>
        /// Gets a stat by ID from the entity data.
        /// </summary>
        public Stat GetStat(Stat.ID statID)
        {
            switch (statID)
            {
                case Stat.ID.Health: return _entityData.health;
                case Stat.ID.Mana: return _entityData.mana;
                case Stat.ID.Stamina: return _entityData.stamina;
                case Stat.ID.Strength: return _entityData.strength;
                case Stat.ID.Agility: return _entityData.agility;
                case Stat.ID.Intelligence: return _entityData.intelligence;
                case Stat.ID.Defense: return _entityData.defense;
                case Stat.ID.Speed: return _entityData.speed;
                case Stat.ID.Attack: return _entityData.attack;
                case Stat.ID.MagicPower: return _entityData.magicPower;
                case Stat.ID.MagicDefense: return _entityData.magicDefense;
                case Stat.ID.CriticalHitChance: return _entityData.criticalHitChance;
                case Stat.ID.CriticalHitDamage: return _entityData.criticalHitDamage;
                case Stat.ID.Luck: return _entityData.luck;
                case Stat.ID.Charisma: return _entityData.charisma;
                case Stat.ID.Hunger: return _entityData.hunger;
                case Stat.ID.Thirst: return _entityData.thirst;
                case Stat.ID.Energy: return _entityData.energy;
                case Stat.ID.Rest: return _entityData.rest;
                case Stat.ID.Experience: return _entityData.experienceStat;
                case Stat.ID.Level: return _entityData.levelStat;
                
                default:
                    UnityEngine.Debug.LogWarning($"Stat {statID} not found for entity {EntityName}");
                    return new Stat(); // Return default stat
            }
        }
        
        /// <summary>
        /// Modifies a stat by the given amount.
        /// </summary>
        public void ModifyStat(Stat.ID statID, float amount)
        {
            var currentStat = GetStat(statID);
            float oldValue = currentStat.currentValue;
            float newValue = Mathf.Clamp(oldValue + amount, 0f, currentStat.maxValue);
            
            switch (statID)
            {
                case Stat.ID.Health:
                    if (amount > 0)
                        _entityData.health.Heal(amount);
                    else
                        _entityData.health.TakeDamage(-amount);
                    break;
                    
                case Stat.ID.Mana:
                    _entityData.mana.currentValue = newValue;
                    break;
                case Stat.ID.Stamina:
                    _entityData.stamina.currentValue = newValue;
                    break;
                case Stat.ID.Strength:
                    _entityData.strength.currentValue = newValue;
                    break;
                case Stat.ID.Agility:
                    _entityData.agility.currentValue = newValue;
                    break;
                case Stat.ID.Intelligence:
                    _entityData.intelligence.currentValue = newValue;
                    break;
                case Stat.ID.Defense:
                    _entityData.defense.currentValue = newValue;
                    break;
                case Stat.ID.Speed:
                    _entityData.speed.currentValue = newValue;
                    break;
                case Stat.ID.Attack:
                    _entityData.attack.currentValue = newValue;
                    break;
                case Stat.ID.MagicPower:
                    _entityData.magicPower.currentValue = newValue;
                    break;
                case Stat.ID.MagicDefense:
                    _entityData.magicDefense.currentValue = newValue;
                    break;
                case Stat.ID.CriticalHitChance:
                    _entityData.criticalHitChance.currentValue = newValue;
                    break;
                case Stat.ID.CriticalHitDamage:
                    _entityData.criticalHitDamage.currentValue = newValue;
                    break;
                case Stat.ID.Luck:
                    _entityData.luck.currentValue = newValue;
                    break;
                case Stat.ID.Charisma:
                    _entityData.charisma.currentValue = newValue;
                    break;
                case Stat.ID.Hunger:
                    _entityData.hunger.currentValue = newValue;
                    break;
                case Stat.ID.Thirst:
                    _entityData.thirst.currentValue = newValue;
                    break;
                case Stat.ID.Energy:
                    _entityData.energy.currentValue = newValue;
                    break;
                case Stat.ID.Rest:
                    _entityData.rest.currentValue = newValue;
                    break;
                case Stat.ID.Experience:
                    _entityData.experienceStat.currentValue = newValue;
                    break;
                case Stat.ID.Level:
                    _entityData.levelStat.currentValue = newValue;
                    _entityData.level = (int)newValue; // Sync with int level field
                    break;
                    
                default:
                    UnityEngine.Debug.LogWarning($"Cannot modify stat {statID} - not implemented");
                    return;
            }
            
            // Sync serialized fields
            SyncSerializedFields();
            
            // Fire events
            OnStatChanged?.Invoke(statID, newValue);
            
            // Notify type data
            if (entityTypeData != null)
            {
                entityTypeData.OnStatChanged(this, statID, newValue);
            }
        }
        
        #endregion
        
        #region State Management
        
        public void ChangeState(State.ID newState)
        {
            // Implementation for state changes
            OnStateChanged?.Invoke(newState);
            
            if (entityTypeData != null)
            {
                entityTypeData.OnStateChanged(this, newState);
            }
        }
        
        #endregion
        
        #region Needs Management
        
        private void UpdateNeedsDecay()
        {
            if (Time.time % 1f < Time.deltaTime) // Every second
            {
                ModifyStat(Stat.ID.Hunger, -hungerDecayRate * Time.deltaTime);
                ModifyStat(Stat.ID.Thirst, -thirstDecayRate * Time.deltaTime);
                ModifyStat(Stat.ID.Energy, -energyDecayRate * Time.deltaTime);
                ModifyStat(Stat.ID.Rest, -restDecayRate * Time.deltaTime);
            }
        }
        
        #endregion
        
        #region Visual Updates
        
        private void UpdateVisuals()
        {
            if (spriteRenderer != null)
            {
                // Update sprite based on entity data
                // Implementation depends on your sprite system
            }
            
            if (animator != null)
            {
                // Update animator parameters
                // Implementation depends on your animation system
            }
        }
        
        #endregion
        
        #region Configuration Methods
        
        /// <summary>
        /// Configure this entity with the given ID and type data
        /// </summary>
        public void ConfigureAsEntity(int id, EntityTypeData typeData)
        {
            EntityID = id;
            TypeData = typeData;
            LoadFromDatabase();
        }
        
        /// <summary>
        /// Change the entity type data (for dynamic type changes)
        /// </summary>
        public void ChangeEntityType(EntityTypeData newTypeData)
        {
            if (entityTypeData != null)
            {
                // Clean up old type data if needed
            }
            
            entityTypeData = newTypeData;
            
            if (newTypeData != null)
            {
                newTypeData.OnInitialize(this);
            }
        }
        
        #endregion
        
        #region Movement (NavMesh Integration)
        
        public void MoveTo(Vector3 destination)
        {
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.SetDestination(destination);
            }
        }
        
        public void StopMovement()
        {
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.ResetPath();
            }
        }
        
        public bool IsMoving => navAgent != null && navAgent.hasPath && navAgent.remainingDistance > 0.1f;
        
        #endregion
    }
}
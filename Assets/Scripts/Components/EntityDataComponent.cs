using UnityEngine;
using System.Collections.Generic;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Ancestral.Legacies.Components
{
    /// <summary>
    /// Component that stores complete GameData Entity information for a Pop.
    /// This bridges the gap between the old Pop system and the new GameData system.
    /// </summary>
    public class EntityDataComponent : MonoBehaviour
    {
        [Header("Entity Data")]
        [SerializeField] private Entity _entityData;

        [Header("Identity")]
        [SerializeField] private string _entityName = "Unknown Entity";
        [SerializeField] private int _entityID = 0;
        [SerializeField] private int _entityAge = 0; // Placeholder for age tracking

        [Header("Base Stats")]
        [SerializeField] private float _entityHunger = 50f; // Default starting hunger
        [SerializeField] private float _entityThirst = 50f; // Default starting thirst
        [SerializeField] private float _entityEnergy = 50f; // Default starting energy
        [SerializeField] private float _entitySpeed = 5f; // Default speed
        [SerializeField] private float _entityHealth = 100f; // Default health
        [SerializeField] private float _entityMana = 50f; // Default mana

        [Header("Max Stat Values")]
        [SerializeField] private float _entityMaxHunger = 100f; // Default max hunger
        [SerializeField] private float _entityMaxThirst = 100f; // Default max thirst
        [SerializeField] private float _entityMaxEnergy = 100f; // Default max energy
        [SerializeField] private float _entityMaxSpeed = 10f; // Default max speed
        [SerializeField] private float _entityMaxHealth = 100f; // Default max health
        [SerializeField] private float _entityMaxMana = 100f; // Default max mana

        [Header("Ability Stats")]
        [SerializeField] private float _entityStrength = 10f; // Default strength
        [SerializeField] private float _entityAgility = 10f; // Default agility
        [SerializeField] private float _entityIntelligence = 10f; // Default intelligence
        [SerializeField] private float _entityDefense = 10f; // Default defense
        [SerializeField] private float _entityLuck = 5f; // Default luck
        [SerializeField] private float _entityCharisma = 5f; // Default charisma

        [Header("Combat Stats")]
        [Tooltip("These stats are used for combat calculations and can be modified by buffs or equipment. Found in EntityData.")]
        
        [SerializeField] private float _entityAttack = 10f; // Default attack
        [SerializeField] private float _entityMagicPower = 10f; // Default magic power
        [SerializeField] private float _entityMagicDefense = 10f; // Default magic defense
        [SerializeField] private float _entityCriticalHitChance = 5f; // Default critical hit chance
        [SerializeField] private float _entityCriticalHitDamage = 150f; // Default critical hit damage

        [Header("Traits")]
        [SerializeField] private List<string> _entityTraits = new List<string>(); // Traits that modify stats or abilities
        
        [Header("Crafting")]
        [SerializeField] private bool _entitycanCraft = false; // Can this entity craft items?
        [SerializeField] private List<string> _craftingRecipes = new List<string>(); // List of available crafting recipes
        [Header("Runtime State")]
        public bool isInitialized = false;
        
        // Properties for easy access
        public Entity EntityData 
        { 
            get => _entityData; 
            set 
            { 
                _entityData = value; 
                isInitialized = true;
                OnEntityDataChanged?.Invoke(_entityData);
            } 
        }
        
        // Events for when entity data changes
        public System.Action<Entity> OnEntityDataChanged;
        public System.Action<Stat.ID, float> OnStatChanged;
        public System.Action<State.ID> OnStateChanged;
          private void Start()
        {
            if (!isInitialized)
            {
                UnityEngine.Debug.LogWarning($"{gameObject.name} EntityDataComponent is not initialized!", this);
            }
        }
        
        
        
        #region Stat Management

        /// <summary>
        /// Gets a stat by ID from the entity data.
        /// </summary>
        public Stat GetStat(Stat.ID statID)
        {
            switch (statID)
            {
                // Get all the stats from EntityData and return them.

                case Stat.ID.Health: return new Stat(Stat.ID.Health, "Health", _entityData.health.current);
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
                // Needs system stats
                case Stat.ID.Hunger: return _entityData.hunger;
                case Stat.ID.Thirst: return _entityData.thirst;
                case Stat.ID.Energy: return _entityData.energy;
                case Stat.ID.Rest: return _entityData.rest;
                // Experience and Level stats
                case Stat.ID.Experience: return _entityData.experience;
                case Stat.ID.Level: return _entityData.levelStat;


                default:
                    UnityEngine.Debug.LogWarning($"Stat {statID} not found in EntityData");
                    return new Stat(statID, statID.ToString(), 0f);
            }
        }
        
        /// <summary>
        /// Modifies a stat and updates the entity data.
        /// </summary>
        public void ModifyStat(Stat.ID statID, float amount)
        {
            var entityData = _entityData;
            
            switch (statID)
            {
                case Stat.ID.Health: 
                    if (amount > 0) entityData.health = entityData.health.Heal(amount);
                    else entityData.health = entityData.health.TakeDamage(-amount);
                    break;
                case Stat.ID.Mana: entityData.mana.ModifyStat(amount); break;
                case Stat.ID.Stamina: entityData.stamina.ModifyStat(amount); break;
                case Stat.ID.Strength: entityData.strength.ModifyStat(amount); break;
                case Stat.ID.Agility: entityData.agility.ModifyStat(amount); break;
                case Stat.ID.Intelligence: entityData.intelligence.ModifyStat(amount); break;
                case Stat.ID.Defense: entityData.defense.ModifyStat(amount); break;
                case Stat.ID.Speed: entityData.speed.ModifyStat(amount); break;
                case Stat.ID.Attack: entityData.attack.ModifyStat(amount); break;
                case Stat.ID.MagicPower: entityData.magicPower.ModifyStat(amount); break;
                case Stat.ID.MagicDefense: entityData.magicDefense.ModifyStat(amount); break;
                case Stat.ID.CriticalHitChance: entityData.criticalHitChance.ModifyStat(amount); break;
                case Stat.ID.CriticalHitDamage: entityData.criticalHitDamage.ModifyStat(amount); break;
                case Stat.ID.Luck: entityData.luck.ModifyStat(amount); break;                case Stat.ID.Charisma: entityData.charisma.ModifyStat(amount); break;
                // Needs system stats
                case Stat.ID.Hunger: entityData.hunger.ModifyStat(amount); break;
                case Stat.ID.Thirst: entityData.thirst.ModifyStat(amount); break;
                case Stat.ID.Energy: entityData.energy.ModifyStat(amount); break;
                case Stat.ID.Rest: entityData.rest.ModifyStat(amount); break;
                // Experience and Level stats
                case Stat.ID.Experience: entityData.experience.ModifyStat(amount); break;
                case Stat.ID.Level: 
                    entityData.levelStat.ModifyStat(amount); 
                    // Also update the int level field to keep them in sync
                    entityData.level = Mathf.RoundToInt(entityData.levelStat.currentValue);
                    break;
                default:
                    UnityEngine.Debug.LogWarning($"Cannot modify unknown stat: {statID}");
                    break;
            }
            
            _entityData = entityData;
            OnStatChanged?.Invoke(statID, amount);
        }
        
        #endregion
        
        #region State Management
        
        /// <summary>
        /// Changes the entity's current state.
        /// </summary>
        public bool ChangeState(State.ID newStateID)
        {
            var entityData = _entityData;
            entityData.ChangeState(newStateID);
            _entityData = entityData;
            
            OnStateChanged?.Invoke(newStateID);
            return true;
        }
        
        /// <summary>
        /// Gets the current state of the entity.
        /// </summary>
        public State GetCurrentState()
        {
            return _entityData.currentState;
        }
        
        /// <summary>
        /// Gets all available states for this entity.
        /// </summary>
        public List<State> GetAvailableStates()
        {
            return _entityData.availableStates ?? new List<State>();
        }
        
        #endregion
        
        #region Buff Management
        
        /// <summary>
        /// Applies a buff to the entity.
        /// </summary>
        public void ApplyBuff(Buff buff)
        {
            var entityData = _entityData;
            entityData.ApplyBuff(buff);
            _entityData = entityData;
        }
        
        /// <summary>
        /// Removes a buff from the entity.
        /// </summary>
        public void RemoveBuff(int buffID)
        {
            var entityData = _entityData;
            entityData.RemoveBuff(buffID);
            _entityData = entityData;
        }
        
        /// <summary>
        /// Gets all active buffs on the entity.
        /// </summary>
        public List<Buff> GetActiveBuffs()
        {
            return _entityData.activeBuffs ?? new List<Buff>();
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Calculates the overall "combat power" of this entity.
        /// </summary>
        public float GetCombatPower()
        {
            return (_entityData.attack.currentValue + _entityData.defense.currentValue + 
                   _entityData.strength.currentValue + _entityData.agility.currentValue) / 4f;
        }
        
        /// <summary>
        /// Calculates the overall "social power" of this entity.
        /// </summary>
        public float GetSocialPower()
        {
            return (_entityData.charisma.currentValue + _entityData.intelligence.currentValue + 
                   _entityData.luck.currentValue) / 3f;
        }
        
        /// <summary>
        /// Gets the entity's current age (you'll need to implement age tracking).
        /// </summary>
        public int GetAge()
        {
            // This would need to be implemented based on your age system
            // For now, return a placeholder
            return _entityData.level * 5; // Rough approximation
        }
        
        /// <summary>
        /// Checks if the entity is alive and healthy.
        /// </summary>
        public bool IsHealthy()
        {
            return _entityData.isAlive && _entityData.health.current > (_entityData.health.max * 0.5f);
        }
        
        #endregion
        
        #region Needs Management
        
        [Header("Needs System Configuration")]
        [SerializeField] private bool enableNeedsDecay = true;
        [SerializeField] private float hungerDecayRate = 1f;
        [SerializeField] private float thirstDecayRate = 1.5f;
        [SerializeField] private float energyDecayRate = 0.8f;
        [SerializeField] private float restDecayRate = 0.5f;
        
        /// <summary>
        /// Updates needs decay over time. Call this from Update() or a manager.
        /// </summary>
        public void UpdateNeeds(float deltaTime)
        {
            if (!enableNeedsDecay) return;
            
            // Apply decay rates to needs
            ModifyStat(Stat.ID.Hunger, -hungerDecayRate * deltaTime);
            ModifyStat(Stat.ID.Thirst, -thirstDecayRate * deltaTime);
            ModifyStat(Stat.ID.Energy, -energyDecayRate * deltaTime);
            ModifyStat(Stat.ID.Rest, -restDecayRate * deltaTime);
        }
        
        /// <summary>
        /// Satisfies hunger by the given amount.
        /// </summary>
        public void EatFood(float amount)
        {
            ModifyStat(Stat.ID.Hunger, amount);
        }

        /// <summary>
        /// Satisfies thirst by the given amount.
        /// </summary>
        public void DrinkWater(float amount)
        {
            ModifyStat(Stat.ID.Thirst, amount);
        }

        /// <summary>
        /// Restores energy by the given amount.
        /// </summary>
        public void RestoreEnergy(float amount)
        {
            ModifyStat(Stat.ID.Energy, amount);
        }

        /// <summary>
        /// Restores rest by the given amount.
        /// </summary>
        public void Sleep(float amount)
        {
            ModifyStat(Stat.ID.Rest, amount);
        }
        
        /// <summary>
        /// Gets the current hunger level (0-100).
        /// </summary>
        public float GetHunger() => GetStat(Stat.ID.Hunger).currentValue;
        
        /// <summary>
        /// Gets the current thirst level (0-100).
        /// </summary>
        public float GetThirst() => GetStat(Stat.ID.Thirst).currentValue;
        
        /// <summary>
        /// Gets the current energy level (0-100).
        /// </summary>
        public float GetEnergy() => GetStat(Stat.ID.Energy).currentValue;
        
        /// <summary>
        /// Gets the current rest level (0-100).
        /// </summary>
        public float GetRest() => GetStat(Stat.ID.Rest).currentValue;
        
        /// <summary>
        /// Checks if any critical needs are dangerously low.
        /// </summary>
        public bool HasCriticalNeeds()
        {
            return GetHunger() <= 0f || GetThirst() <= 0f;
        }
        
        #endregion
    }
}

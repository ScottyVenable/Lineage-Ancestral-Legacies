using UnityEngine;
using Lineage.Ancestral.Legacies.Systems.Inventory;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Debug;
using Lineage.Ancestral.Legacies.Components;
using Lineage.Ancestral.Legacies.Database;
using UnityEngine.UI;
using UnityEngine.AI;

namespace Lineage.Ancestral.Legacies.Entities
{    /// <summary>
    /// Core Pop entity representing a population unit with needs, inventory, and AI.
    /// 
    /// ARCHITECTURE CHANGE (v2.0):
    /// This class has been refactored to use EntityDataComponent as the primary data store.
    /// All stat management, needs tracking, and entity data is now handled by EntityDataComponent.
    /// 
    /// Pop class responsibilities:
    /// - Unity component orchestration (NavMeshAgent, SpriteRenderer, Animator, etc.)
    /// - Visual systems (health bars, selection highlighting, animations)
    /// - Navigation and movement logic
    /// - Population management integration
    /// - Game lifecycle events (death, spawning, etc.)
    /// - PopData ScriptableObject integration
    /// 
    /// EntityDataComponent responsibilities:
    /// - All stat storage and management (health, hunger, thirst, etc.)
    /// - Needs system logic and decay
    /// - Entity data structures (Entity struct from Database)
    /// - Buff/debuff management
    /// - State management
    /// </summary>
    [RequireComponent(typeof(EntityDataComponent))]
    [RequireComponent(typeof(InventoryComponent))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Pop : MonoBehaviour
    {
        [Header("Pop Identity")]
        public string popName = "Unnamed Pop";
        public int age = 0;
        [Header("Health & Stats - Use EntityDataComponent")]
        // These properties delegate to EntityDataComponent for consistency
        // The actual stat values are stored in EntityDataComponent.EntityData
        // This provides backward compatibility while using EntityDataComponent as the source of truth
        
        // Getter properties that delegate to EntityDataComponent
        public float health => entityDataComponent.GetStat(Stat.ID.Health).currentValue;
        public float thirst => entityDataComponent.GetThirst();
        public float hunger => entityDataComponent.GetHunger();
        public float maxHealth => entityDataComponent.GetStat(Stat.ID.Health).maxValue;
        public float maxThirst => entityDataComponent.GetStat(Stat.ID.Thirst).maxValue;
        public float maxHunger => entityDataComponent.GetStat(Stat.ID.Hunger).maxValue;

        [Header("Pop Data Reference")]
        public PopData popData;
        public EntityDataComponent entityDataComponent;
        public InventoryComponent inventoryComponent;

        [Header("Navigation")]
        public NavMeshAgent agent;
        public bool isMoving => agent != null && agent.hasPath && agent.remainingDistance > agent.stoppingDistance;

        public float movementSpeed => agent != null ? agent.speed : 0f;

        [Header("Visuals")]
        public SpriteRenderer spriteRenderer;
        public Image healthBar;
        public Animator animator;
        public bool lockRotation = true; // Whether to lock rotation to movement direction

        public bool showHealthBar = true; // Toggle for health bar visibility
        // Property for PopController animation access (capital A for compatibility)
        public Animator Animator => animator;

        private Color originalSpriteColor;
        private bool _hasStoredOriginalColor = false;

        // Event for when this pop is destroyed
        public static System.Action<Pop> OnPopDestroyed;        private void Awake()
        {
            // Get component references
            entityDataComponent = GetComponent<EntityDataComponent>();
            inventoryComponent = GetComponent<InventoryComponent>();
            agent = GetComponent<NavMeshAgent>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            // Configure NavMeshAgent settings
            if (agent != null)
            {
                agent.speed = 3.5f;
                agent.acceleration = 8f;
                agent.angularSpeed = 0f;
                agent.stoppingDistance = 0.5f;
                agent.autoBraking = true;
                agent.autoRepath = true;
            }

            // Store original color for selection highlighting
            if (spriteRenderer != null && !_hasStoredOriginalColor)
            {
                originalSpriteColor = spriteRenderer.color;
                _hasStoredOriginalColor = true;
            }
        }

        private void Start()
        {
            // Initialize with pop data if available
            if (popData != null)
            {
                ApplyPopData();
            }

            // Ensure EntityDataComponent is initialized
            if (entityDataComponent != null && !entityDataComponent.isInitialized)
            {
                InitializeEntityData();
            }            // Register with PopulationManager - use existing AddToPops method or manual list management
            if (PopulationManager.Instance != null)
            {
                // PopulationManager handles this automatically when pop is spawned
                // No explicit registration needed as the manager tracks pops via spawning
            }
        }

        private void Update()
        {
            // Update needs decay through EntityDataComponent
            if (entityDataComponent != null)
            {
                entityDataComponent.UpdateNeeds(Time.deltaTime);
            }

            if (showHealthBar && healthBar != null)
            {
                healthBar.gameObject.SetActive(true);
            }
            else if (healthBar != null)
            {
                healthBar.gameObject.SetActive(false);
            }

            // Check for death conditions
            CheckDeathConditions();

            // Update health bar if available
            UpdateHealthBar();

            if (lockRotation && agent != null && agent.isOnNavMesh)
            {
                gameObject.transform.rotation = Quaternion.LookRotation(agent.velocity.normalized, Vector3.up);
            }
        }

        /// <summary>
        /// Initializes EntityDataComponent with default entity data if not already set.
        /// </summary>
        private void InitializeEntityData()
        {
            if (entityDataComponent == null) return;

            // Create default entity data if not set
            var entityData = new Database.Entity(
                name: popName,
                id: Database.Entity.ID.Pop,
                faction: "Player", //todo: change this to the Settlement name from the lore.
                description: "A member of your ancestral lineage",
                level: 1,
                healthValue: new Health(maxHealth, health),
                usesMana: false // Pops do not use mana by default
            );

            entityDataComponent.EntityData = entityData;
            
        }        /// <summary>
        /// Applies PopData configuration to this Pop instance.
        /// </summary>
        private void ApplyPopData()
        {
            if (popData == null) return;

            // Apply age
            age = Random.Range(8, 64); // Random age between 8 and 64 for diversity

            // Apply starting items to inventory
            if (inventoryComponent != null && popData.startingItems != null)
            {
                foreach (var item in popData.startingItems)
                {
                    if (item != null)
                        inventoryComponent.AddItem(item.itemId, 1);
                }
            }
        }

        /// <summary>
        /// Checks if the pop should die based on health or critical needs.
        /// </summary>
        private void CheckDeathConditions()
        {
            if (health <= 0)
            {
                Die();
                return;
            }

            // Check critical needs through EntityDataComponent
            if (entityDataComponent != null && entityDataComponent.HasCriticalNeeds())
            {
                Die();
            }
        }

        /// <summary>
        /// Kills this pop and handles cleanup.
        /// </summary>
        public void Die()
        {
            Log.Info($"Pop {popName} has died.", Log.LogCategory.Population);            // Notify managers
            OnPopDestroyed?.Invoke(this);
            
            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.OnPopDied(this);
            }

            // Destroy the game object
            Destroy(gameObject);
        }

        /// <summary>
        /// Updates the health bar UI if available.
        /// </summary>
        private void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                healthBar.fillAmount = health / maxHealth;
            }
        }

        #region Needs Management (Delegated to EntityDataComponent)

        /// <summary>
        /// Gets the current hunger level (0-100).
        /// </summary>
        public float GetHunger() => entityDataComponent?.GetHunger() ?? 50f;

        /// <summary>
        /// Gets the current thirst level (0-100).
        /// </summary>
        public float GetThirst() => entityDataComponent?.GetThirst() ?? 50f;

        /// <summary>
        /// Gets the current energy level (0-100).
        /// </summary>
        public float GetEnergy() => entityDataComponent?.GetEnergy() ?? 50f;

        /// <summary>
        /// Gets the current rest level (0-100).
        /// </summary>
        public float GetRest() => entityDataComponent?.GetRest() ?? 50f;

        /// <summary>
        /// Feeds the pop, increasing hunger satisfaction.
        /// </summary>
        public void EatFood(float amount)
        {
            entityDataComponent?.EatFood(amount);
        }

        /// <summary>
        /// Gives the pop water, increasing thirst satisfaction.
        /// </summary>
        public void DrinkWater(float amount)
        {
            entityDataComponent?.DrinkWater(amount);
        }

        /// <summary>
        /// Restores the pop's energy.
        /// </summary>
        public void RestoreEnergy(float amount)
        {
            entityDataComponent?.RestoreEnergy(amount);
        }

        /// <summary>
        /// Allows the pop to sleep, restoring rest.
        /// </summary>
        public void Sleep(float amount)
        {
            entityDataComponent?.Sleep(amount);
        }        // Convenience properties for backward compatibility - these delegate to EntityDataComponent
        public float stamina => GetEnergy(); // Map energy to stamina for backward compatibility

        public bool IsHungry => GetHunger() < 50f;
        public bool IsThirsty => GetThirst() < 50f;
        public bool IsTired => GetEnergy() < 30f;

        #endregion

        #region Selection and Interaction

        /// <summary>
        /// Handles pop selection for UI highlighting.        /// </summary>
        public void OnSelected(bool selected)
        {
            if (spriteRenderer == null) return;

            if (selected)
            {
                // Highlight the pop when selected
                spriteRenderer.color = Color.yellow;
            }
            else
            {
                // Restore original color when deselected
                if (_hasStoredOriginalColor)
                {
                    spriteRenderer.color = originalSpriteColor;
                }
            }
        }

        #endregion

        #region Navigation        /// <summary>
        /// Move the pop to a specific world position using NavMesh.
        /// </summary>
        /// <param name="targetPosition">The world position to move to</param>
        /// <returns>True if the destination was set successfully</returns>
        public bool MoveTo(Vector3 targetPosition)
        {
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            {
                AdvancedLogger.LogWarning(LogCategory.AI, $"Pop {popName}: Cannot move - NavMeshAgent not ready");
                return false;
            }

            agent.SetDestination(targetPosition);
            return true;
        }

        /// <summary>
        /// Move the pop to a specific transform location.
        /// </summary>
        /// <param name="target">The target transform to move to</param>
        /// <returns>True if the destination was set successfully</returns>
        public bool MoveTo(Transform target)
        {
            if (target == null)
            {
                AdvancedLogger.LogWarning(LogCategory.AI, $"Pop {popName}: Cannot move to null target");
                return false;
            }

            return MoveTo(target.position);
        }

        /// <summary>
        /// Stop the pop's current movement.
        /// </summary>
        public void StopMovement()
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
        }

        /// <summary>
        /// Check if the pop is currently moving.
        /// </summary>
        /// <returns>True if the pop is moving</returns>
        public bool IsMoving()
        {
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
                return false;

            return agent.hasPath && agent.remainingDistance > agent.stoppingDistance;
        }

        /// <summary>
        /// Check if the pop has reached its destination.
        /// </summary>
        /// <returns>True if the pop has reached its destination</returns>
        public bool HasReachedDestination()
        {
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
                return true; // Consider "reached" if agent is not functional

            return !agent.pathPending && agent.remainingDistance < agent.stoppingDistance;
        }

        /// <summary>
        /// Get the current movement speed of the pop.
        /// </summary>
        /// <returns>Current speed</returns>
        public float GetMovementSpeed()
        {
            if (agent == null || !agent.isActiveAndEnabled)
                return 0f;            return agent.velocity.magnitude;
        }

        #endregion

        #region Debug and Status

        /// <summary>
        /// Gets a comprehensive status string for debugging.
        /// </summary>
        public string GetStatusString()
        {
            if (entityDataComponent == null) 
                return $"{popName}: Health={health:F1}/{maxHealth:F1}, Needs=N/A (EntityDataComponent missing)";

            return $"{popName}: Health={health:F1}/{maxHealth:F1}, " +
                   $"Hunger={GetHunger():F1}, Thirst={GetThirst():F1}, " +
                   $"Energy={GetEnergy():F1}, Rest={GetRest():F1}";
        }

        #endregion

        private void OnDestroy()
        {
            // Cleanup when destroyed
            OnPopDestroyed?.Invoke(this);
        }
    }
}
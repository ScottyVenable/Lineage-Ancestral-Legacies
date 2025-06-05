using UnityEngine;
using Lineage.Ancestral.Legacies.Systems.Inventory;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Debug;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Database;
using UnityEngine.UI;
using UnityEngine.AI;

namespace Lineage.Ancestral.Legacies.Entities
{    /// <summary>
    /// Core Pop entity representing a population unit with needs, inventory, and AI.
    /// 
    /// ARCHITECTURE CHANGE (v2.0):
    /// This class now relies on the unified Entity component for all stat
    /// management and data storage.
    /// 
    /// Pop class responsibilities:
    /// - Unity component orchestration (NavMeshAgent, SpriteRenderer, Animator, etc.)
    /// - Visual systems (health bars, selection highlighting, animations)
    /// - Navigation and movement logic
    /// - Population management integration
    /// - Game lifecycle events (death, spawning, etc.)
    /// - PopData ScriptableObject integration
    /// 
    /// Entity component responsibilities:
    /// - Stat storage and management (health, hunger, thirst, etc.)
    /// - Needs system logic and decay
    /// - Entity data structures (Entity struct from Database)
    /// - Buff/debuff management
    /// - State management
    /// </summary>
    [RequireComponent(typeof(Entity))]
    [RequireComponent(typeof(InventoryComponent))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Pop : MonoBehaviour
    {
        [Header("Pop Identity")]
        public string popName = "Unnamed Pop";
        public int age = 0;
        [Header("Health & Stats - Entity System")]
        // Getter properties that proxy to the Entity component
        public float health => entity != null ? entity.Health : 0f;
        public float thirst => entity != null ? entity.Thirst : 0f;
        public float hunger => entity != null ? entity.Hunger : 0f;
        public float maxHealth => entity != null ? entity.GetStat(Stat.ID.Health).maxValue : 0f;
        public float maxThirst => entity != null ? entity.GetStat(Stat.ID.Thirst).maxValue : 0f;
        public float maxHunger => entity != null ? entity.GetStat(Stat.ID.Hunger).maxValue : 0f;

        [Header("Pop Data Reference")]
        public PopData popData;
        public Entity entity;
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
            entity = GetComponent<Entity>();
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

            // Ensure Entity component is initialized
            if (entity != null && !entity.isInitialized)
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
            // Entity component handles needs decay internally

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
        /// Initializes the Entity component with default data if not already set.
        /// </summary>
        private void InitializeEntityData()
        {
            if (entity == null) return;

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

            entity.EntityData = entityData;
            
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

            // Check critical needs via Entity component
            if (HasCriticalNeeds())
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

        #region Needs Management

        /// <summary>
        /// Gets the current hunger level (0-100).
        /// </summary>
        public float GetHunger() => entity != null ? entity.Hunger : 50f;

        /// <summary>
        /// Gets the current thirst level (0-100).
        /// </summary>
        public float GetThirst() => entity != null ? entity.Thirst : 50f;

        /// <summary>
        /// Gets the current energy level (0-100).
        /// </summary>
        public float GetEnergy() => entity != null ? entity.Energy : 50f;

        /// <summary>
        /// Gets the current rest level (0-100).
        /// </summary>
        public float GetRest() => entity != null ? entity.GetStat(Stat.ID.Rest).currentValue : 50f;

        /// <summary>
        /// Feeds the pop, increasing hunger satisfaction.
        /// </summary>
        public void EatFood(float amount)
        {
            if (entity != null)
                entity.ModifyStat(Stat.ID.Hunger, amount);
        }

        /// <summary>
        /// Gives the pop water, increasing thirst satisfaction.
        /// </summary>
        public void DrinkWater(float amount)
        {
            if (entity != null)
                entity.ModifyStat(Stat.ID.Thirst, amount);
        }

        /// <summary>
        /// Restores the pop's energy.
        /// </summary>
        public void RestoreEnergy(float amount)
        {
            if (entity != null)
                entity.ModifyStat(Stat.ID.Energy, amount);
        }

        /// <summary>
        /// Allows the pop to sleep, restoring rest.
        /// </summary>
        public void Sleep(float amount)
        {
            if (entity != null)
                entity.ModifyStat(Stat.ID.Rest, amount);
        }

        public bool HasCriticalNeeds()
        {
            if (entity == null) return false;
            return entity.Hunger < 20f ||
                   entity.Thirst < 20f ||
                   entity.Energy < 20f ||
                   entity.Health < 30f;
        }
        // Convenience properties for backward compatibility
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
            if (entity == null)
                return $"{popName}: Health=N/A (Entity component missing)";

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
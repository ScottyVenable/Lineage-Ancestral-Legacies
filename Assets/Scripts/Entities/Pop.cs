using UnityEngine;
using Lineage.Ancestral.Legacies.Systems.Needs;
using Lineage.Ancestral.Legacies.Systems.Inventory;
using Lineage.Ancestral.Legacies.AI;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Debug;
using UnityEngine.UI;
using UnityEngine.AI;

namespace Lineage.Ancestral.Legacies.Entities
{
    /// <summary>
    /// Core Pop entity representing a population unit with needs, inventory, and AI.
    /// </summary>
    [RequireComponent(typeof(NeedsComponent))]
    [RequireComponent(typeof(InventoryComponent))]
    [RequireComponent(typeof(PopStateMachine))]
    public class Pop : MonoBehaviour
    {
        [Header("Pop Identity")]
        public string popName = "Unnamed Pop";
        public int age = 0;

        [Header("Health & Stats")]
        public float health = 100f;
        public float maxHealth = 100f;

        [Header("Pop Data Reference")]
        [SerializeField] public PopData popData;

        // Component references
        public NeedsComponent needsComponent;
        public InventoryComponent inventoryComponent;
        public PopStateMachine stateMachine;

        // Properties for backward compatibility
        public new string name
        {
            get => popName;
            set => popName = value;
        }

        [Header("Navigation")]
        public NavMeshAgent agent;




        // Property for backward compatibility with AI states
        public NavMeshAgent Agent => agent;

        private void Awake()
        {


            // Get required components
            needsComponent = GetComponent<NeedsComponent>();
            inventoryComponent = GetComponent<InventoryComponent>();
            stateMachine = GetComponent<PopStateMachine>();
            agent = GetComponent<NavMeshAgent>(); // Add this line
            // Set default name if empty
            if (string.IsNullOrEmpty(popName))
            {
                popName = $"Pop_{GetInstanceID()}";
            }

            // Apply pop data if assigned
            if (popData != null)
            {
                ApplyPopData();
            }
        }

        private void Start()
        {
            // Initialize the state machine
            if (stateMachine != null)
            {
                stateMachine.Initialize(this);
            }

            // Sync legacy needs with NeedsComponent
            SyncNeedsFromComponent();
        }

        private void Update()
        {
            // Update state machine
            if (stateMachine != null)
            {
                stateMachine.Tick();
            }

            // Sync needs for backward compatibility
            SyncNeedsFromComponent();

            // Check for death conditions
            CheckDeathConditions();
        }

        private void ApplyPopData()
        {
            if (popData == null) return;

            maxHealth = popData.maxHealth;
            health = maxHealth;
            age = popData.startingAge;

            // Apply to needs component if available
            if (needsComponent != null)
            {
                needsComponent.hunger = popData.maxHunger;
                needsComponent.thirst = popData.maxThirst;
            }

            // Apply starting items to inventory
            if (inventoryComponent != null && popData.startingItems != null)
            {
                foreach (var item in popData.startingItems)
                {
                    if (item != null)
                    {
                        inventoryComponent.AddItem(item.itemId, 1);
                    }
                }
            }
        }

        private void SyncNeedsFromComponent()
        {
            if (needsComponent != null)
            {
                hunger = needsComponent.hunger;
                thirst = needsComponent.thirst;
                stamina = needsComponent.energy; // Map energy to stamina for backward compatibility
            }
        }

        private void CheckDeathConditions()
        {
            if (health <= 0 || (needsComponent != null && (needsComponent.hunger <= 0 || needsComponent.thirst <= 0)))
            {
                Die();
            }
        }

        public void Die()
        {
            Log.Pop(popName, "has died.");

            // Notify PopulationManager if available
            var popManager = FindFirstObjectByType<PopulationManager>();
            if (popManager != null)
            {
                popManager.OnPopDied(this);
            }

            // Destroy the GameObject
            Destroy(gameObject);
        }

        public void TakeDamage(float damage)
        {
            health = Mathf.Max(0, health - damage);
            Log.Pop(popName, $"took {damage} damage. Health: {health}/{maxHealth}");
        }

        public void Heal(float healAmount)
        {
            health = Mathf.Min(maxHealth, health + healAmount);
            Log.Pop(popName, $"healed {healAmount}. Health: {health}/{maxHealth}");
        }

        public void SetAge(int newAge)
        {
            age = Mathf.Max(0, newAge);
        }

        public void SetPopData(PopData data)
        {
            popData = data;
            if (data != null)
            {
                ApplyPopData();
            }
        }

        // Component accessors
        public NeedsComponent GetNeedsComponent() => needsComponent;
        public InventoryComponent GetInventoryComponent() => inventoryComponent;
        public PopStateMachine GetStateMachine() => stateMachine;

        // Utility methods for external systems
        public bool IsAlive => health > 0;
        public bool IsHealthy => health > maxHealth * 0.5f;
        public bool IsHungry => needsComponent != null ? needsComponent.hunger < 50f : hunger < 50f;
        public bool IsThirsty => needsComponent != null ? needsComponent.thirst < 50f : thirst < 50f;
        public bool IsTired => needsComponent != null ? needsComponent.energy < 30f : stamina < 30f; // Use energy instead of stamina

        private void OnDestroy()
        {
            Log.Pop(popName, "was destroyed.");
        }

        // Debug information
        public string GetStatusString()
        {
            return $"{popName} - Health: {health:F1}/{maxHealth:F1}, " +
                   $"Hunger: {hunger:F1}, Thirst: {thirst:F1}, Stamina: {stamina:F1}, " +
                   $"Age: {age}, State: {stateMachine?.currentState?.GetType().Name ?? "None"}";
        }

        // Add these methods if they don't already exist
        public void OnSelected(bool selected)
        {
            // Visual feedback for selection
            var renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                if (selected)
                {
                    // Store original color if first selection
                    if (!_hasStoredOriginalColor)
                    {
                        _originalColor = renderer.color;
                        _hasStoredOriginalColor = true;
                    }
                    // Highlight with a brighter color
                    renderer.color = new Color(
                        _originalColor.r * 1.2f,
                        _originalColor.g * 1.2f,
                        _originalColor.b * 1.2f,
                        _originalColor.a
                    );
                }
                else if (_hasStoredOriginalColor)
                {
                    // Restore original color when deselected
                    renderer.color = _originalColor;
                }
            }

            // Show/hide selection indicator
            var indicator = transform.Find("SelectionIndicator");
            if (indicator != null)
            {
                indicator.gameObject.SetActive(selected);
            }
            else if (selected)
            {
                // Create indicator if it doesn't exist
                CreateSelectionIndicator();
            }
        }

        private bool _hasStoredOriginalColor = false;
        private Color _originalColor = Color.white;
        public GameObject selectionIndicator;

        public Animator animator;

        private void CreateSelectionIndicator()
        {
            // Create a circle around the character
            selectionIndicator = new GameObject("SelectionIndicator");
            selectionIndicator.transform.SetParent(transform);
            selectionIndicator.transform.localPosition = Vector3.zero;

            // Create a line renderer for the circle
            LineRenderer lineRenderer = selectionIndicator.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 20; // Number of segments in circle
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;

            // Set a material - create a simple unlit material if needed
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material.color = Color.green;

            // Draw a circle
            float radius = 0.6f; // Adjust based on your character size
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                float angle = i * 360f / lineRenderer.positionCount;
                float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }

                public bool EnsureOnNavMesh()
        {
            if (agent == null) 
            {
                UnityEngine.Debug.LogWarning($"Pop {popName} has no NavMeshAgent component!");
                return false;
            }
            
            if (agent.isOnNavMesh) return true;
            
            // Try to place on nearest NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 10.0f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
                return true;
            }
            
            UnityEngine.Debug.LogWarning($"Cannot place {popName} on NavMesh at position {transform.position}");
            return false;
        }

    }
}

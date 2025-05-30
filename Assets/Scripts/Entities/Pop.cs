using UnityEngine;
using Lineage.Ancestral.Legacies.Systems.Needs;
using Lineage.Ancestral.Legacies.Systems.Inventory;
using Lineage.Ancestral.Legacies.AI;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Debug;

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
        
        [Header("Needs (Legacy - use NeedsComponent instead)")]
        public float hunger = 100f;
        public float thirst = 100f;
        public float stamina = 100f;
        
        [Header("Pop Data Reference")]
        [SerializeField] private PopData popData;
        
        // Component references
        private NeedsComponent needsComponent;
        private InventoryComponent inventoryComponent;
        private PopStateMachine stateMachine;
        
        // Properties for backward compatibility
        public new string name
        {
            get => popName;
            set => popName = value;
        }
        
        private void Awake()
        {
            // Get required components
            needsComponent = GetComponent<NeedsComponent>();
            inventoryComponent = GetComponent<InventoryComponent>();
            stateMachine = GetComponent<PopStateMachine>();
            
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

        // Add these methods to the Pop class
        public void OnSelected()
        {
            // Visual feedback for selection
            var selectionIndicator = transform.Find("SelectionIndicator");
            if (selectionIndicator != null)
            {
                selectionIndicator.gameObject.SetActive(true);
            }
            else
            {
                // Create a selection indicator if it doesn't exist
                CreateSelectionIndicator();
            }
            
            // You could also change the sprite color
            var renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = new Color(1f, 1f, 1f, 1f); // Full brightness
            }

            Debug.Log.Info($"Pop {name} selected");
        }

        public void OnDeselected()
        {
            // Remove visual feedback
            var selectionIndicator = transform.Find("SelectionIndicator");
            if (selectionIndicator != null)
            {
                selectionIndicator.gameObject.SetActive(false);
            }
            
            var renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Normal brightness
            }
        }

        private void CreateSelectionIndicator()
        {
            // Create a new GameObject for the selection indicator
            GameObject indicator = new GameObject("SelectionIndicator");
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = Vector3.zero;
            
            // Add a sprite renderer component
            SpriteRenderer renderer = indicator.AddComponent<SpriteRenderer>();
            
            // Create a circle selection indicator
            renderer.sprite = CreateCircleSprite();
            renderer.color = new Color(0.2f, 0.8f, 0.2f, 0.5f); // Semi-transparent green
            renderer.sortingOrder = -1; // Behind the character
            
            // Make the indicator slightly larger than the character
            indicator.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }

        private Sprite CreateCircleSprite()
        {
            // Create a simple circle texture for the selection indicator
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            
            float radius = size / 2f;
            float radiusSquared = radius * radius;
            Color transparent = new Color(0, 0, 0, 0);
            Color white = Color.white;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float distSquared = (x - radius) * (x - radius) + (y - radius) * (y - radius);
                    
                    // Create a circle outline
                    float distFromEdge = Mathf.Abs(distSquared - radiusSquared);
                    if (distFromEdge < radius * 0.2f) // Outline thickness
                    {
                        texture.SetPixel(x, y, white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, transparent);
                    }
                }
            }
            
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Entities;
using Lineage.Debug;
using Lineage.Systems;
using Lineage.Components;
using Lineage.Database;

namespace Lineage.Managers
{
    /// <summary>
    /// Enhanced PopulationManager that uses GameData system for richer Pop generation.
    /// Supports genetic inheritance, traits, and dynamic stat generation.
    /// </summary>
    public class EnhancedPopulationManager : MonoBehaviour
    {
        public static EnhancedPopulationManager Instance { get; private set; }

        [Header("Population Settings")]
        public int populationCap = 20;
        public int currentPopulation = 0;
        public GameObject popPrefab;

        [Header("Spawning")]
        public Transform spawnPoint;
        public float spawnRadius = 2f;
        
        [Header("GameData Integration")]
        public bool useGameDataSystem = true;
        public bool enableGeneticInheritance = true;
        public bool enableRandomTraits = true;
        
        [Header("Breeding Settings")]
        public float breedingChance = 0.1f; // Chance per update cycle
        public float minBreedingAge = 18f;
        public float maxBreedingAge = 45f;
        
        private List<Pop> livingPops = new List<Pop>();
        private List<EntityDataComponent> entityComponents = new List<EntityDataComponent>();

        // Events
        public System.Action<int> OnPopulationChanged;
        public System.Action<int> OnPopulationCapChanged;
        public System.Action<Pop, Pop, Pop> OnPopBorn; // Parent A, Parent B, Child

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Initialize GameData if not already done
                if (useGameDataSystem)
                {
                    GameData.InitializeAllDatabases();
                }
            }
            else
            {
                Destroy(gameObject);
            }
            
            // todo: probably dont need this because we have GameData and it will handle initialization
        }

        private void Start()
        {
            // Spawn initial population
            for (int i = 0; i < Mathf.Min(3, populationCap); i++)
            {
                SpawnPop();
            }

            OnPopulationChanged?.Invoke(currentPopulation);
            OnPopulationCapChanged?.Invoke(populationCap);
        }

        private void Update()
        {
            ProcessPopulationNeeds();
            
            if (enableGeneticInheritance && currentPopulation < populationCap)
            {
                ProcessBreeding();
            }
        }

        private void ProcessPopulationNeeds()
        {
            for (int i = livingPops.Count - 1; i >= 0; i--)
            {
                var pop = livingPops[i];
                if (pop == null)
                {
                    livingPops.RemoveAt(i);
                    if (i < entityComponents.Count)
                        entityComponents.RemoveAt(i);
                    currentPopulation--;
                    OnPopulationChanged?.Invoke(currentPopulation);
                    continue;
                }

                // Use EntityDataComponent if available for richer behavior
                EntityDataComponent entityData = i < entityComponents.Count ? entityComponents[i] : null;
                
                if (entityData != null && useGameDataSystem)
                {
                    ProcessGameDataNeeds(pop, entityData);
                }
                else
                {
                    ProcessBasicNeeds(pop);
                }
            }
        }

        private void ProcessGameDataNeeds(Pop pop, EntityDataComponent entityData)
        {
            // Check entity health from GameData
            var healthStat = entityData.GetStat(Stat.ID.Health);
            
            if (healthStat.currentValue <= 0)
            {
                Log.Warning($"Pop {pop.name} died!", Log.LogCategory.Population);
                KillPop(pop);
                return;
            }

            // Apply state-based effects
            var currentState = entityData.GetCurrentState();
            switch ((State.ID)currentState.stateID)
            {
                case State.ID.Gathering:
                    ResourceManager.Instance?.AddFood(1f * Time.deltaTime);
                    entityData.ModifyStat(Stat.ID.Stamina, -2f * Time.deltaTime);
                    break;
                    
                case State.ID.Crafting:
                    // Could generate crafted items
                    entityData.ModifyStat(Stat.ID.Stamina, -1f * Time.deltaTime);
                    break;
                    
                case State.ID.Resting:
                    entityData.ModifyStat(Stat.ID.Stamina, 5f * Time.deltaTime);
                    entityData.ModifyStat(Stat.ID.Health, 1f * Time.deltaTime);
                    break;
                    
                case State.ID.Socializing:
                    if (ResourceManager.Instance != null)
                        ResourceManager.Instance.AddFaith(ResourceManager.Instance.faithGenerationRate * Time.deltaTime);
                    break;
            }

            // Auto-transition states based on needs
            if (entityData.GetStat(Stat.ID.Stamina).currentValue < 20f)
            {
                entityData.ChangeState(State.ID.Resting);
            }
            else if (Random.Range(0f, 1f) < 0.001f) // Random state changes
            {
                ChangeToRandomState(entityData);
            }
        }

        private void ProcessBasicNeeds(Pop pop)
        {
            // Fallback to basic needs system
            // Check if pop has NeedsComponent for hunger/thirst
            var needsComponent = pop.GetComponent<Lineage.Systems.Needs.NeedsComponent>();
            
            if (needsComponent != null)
            {
                // Use the existing needs system
                if (needsComponent.hunger <= 0f)
                {
                    Log.Warning($"Pop {pop.name} died of starvation!", Log.LogCategory.Population);
                    KillPop(pop);
                    return;
                }

                // Generate resources based on basic needs
                if (needsComponent.hunger > 30f && needsComponent.thirst > 30f)
                {
                    if (ResourceManager.Instance != null)
                        ResourceManager.Instance.AddFaith(ResourceManager.Instance.faithGenerationRate * Time.deltaTime);
                }

                ResourceManager.Instance?.AddFood(0.5f * Time.deltaTime);
            }
        }

        private void ProcessBreeding()
        {
            if (Random.Range(0f, 1f) > breedingChance * Time.deltaTime) return;

            // Find suitable breeding pairs
            var breedingCandidates = GetBreedingCandidates();
            
            if (breedingCandidates.Count >= 2)
            {
                var parentA = breedingCandidates[Random.Range(0, breedingCandidates.Count)];
                var parentB = breedingCandidates[Random.Range(0, breedingCandidates.Count)];
                
                if (parentA != parentB)
                {
                    SpawnChildPop(parentA, parentB);
                }
            }
        }

        private List<Pop> GetBreedingCandidates()
        {
            var candidates = new List<Pop>();
            
            for (int i = 0; i < livingPops.Count; i++)
            {
                var pop = livingPops[i];
                if (pop == null) continue;

                // Check age (you'll need to implement age tracking)
                float age = pop.age; // Assuming this exists in Pop class
                
                if (age >= minBreedingAge && age <= maxBreedingAge)
                {
                    // Additional checks could include health, relationship status, etc.
                    if (useGameDataSystem && i < entityComponents.Count)
                    {
                        var entityData = entityComponents[i];
                        if (entityData != null && entityData.IsHealthy())
                        {
                            candidates.Add(pop);
                        }
                    }
                    else
                    {
                        candidates.Add(pop);
                    }
                }
            }
            
            return candidates;
        }

        private void ChangeToRandomState(EntityDataComponent entityData)
        {
            var availableStates = entityData.GetAvailableStates();
            if (availableStates.Count > 0)
            {
                var randomState = availableStates[Random.Range(0, availableStates.Count)];
                entityData.ChangeState((State.ID)randomState.stateID);
            }
        }

        public void SpawnPop()
        {
            if (currentPopulation >= populationCap || popPrefab == null) return;

            Vector3 spawnPos = spawnPoint.position + Random.insideUnitSphere * spawnRadius;
            spawnPos.y = spawnPoint.position.y;

            GameObject popObj;
            
            if (useGameDataSystem)
            {
                // Use PopFactory for rich generation
                popObj = PopFactory.CreatePop(popPrefab: popPrefab, spawnPosition: spawnPos);
            }
            else
            {
                // Fallback to simple instantiation
                popObj = Instantiate(popPrefab, spawnPos, Quaternion.identity);
            }

            Pop pop = popObj.GetComponent<Pop>();
            if (pop != null)
            {
                if (!useGameDataSystem)
                {
                    pop.name = GenerateRandomName();
                }
                
                pop.transform.localScale = Vector3.one * Random.Range(0.9f, 1.1f);
                
                RegisterPop(pop);
                
                Log.Info($"New pop spawned: {pop.name}", Log.LogCategory.Population);
            }
        }

        public void SpawnChildPop(Pop parentA, Pop parentB)
        {
            if (currentPopulation >= populationCap || popPrefab == null) return;

            Vector3 spawnPos = spawnPoint.position + Random.insideUnitSphere * spawnRadius;
            spawnPos.y = spawnPoint.position.y;

            GameObject popObj;
            
            if (useGameDataSystem && enableGeneticInheritance)
            {
                // Get parent entity data for inheritance
                var parentAEntity = GetEntityDataForPop(parentA);
                var parentBEntity = GetEntityDataForPop(parentB);
                
                popObj = PopFactory.CreatePop(
                    parentA: parentAEntity, 
                    parentB: parentBEntity,
                    popPrefab: popPrefab, 
                    spawnPosition: spawnPos
                );
            }
            else
            {
                popObj = Instantiate(popPrefab, spawnPos, Quaternion.identity);
            }

            Pop childPop = popObj.GetComponent<Pop>();
            if (childPop != null)
            {
                // Set child properties
                childPop.age = 0; // New born
                childPop.transform.localScale = Vector3.one * 0.5f; // Smaller child
                
                RegisterPop(childPop);
                
                OnPopBorn?.Invoke(parentA, parentB, childPop);
                Log.Info($"Child pop born: {childPop.popName} (Parents: {parentA.name} & {parentB.name})", Log.LogCategory.Population);
            }
        }

        private Database.Entity GetEntityDataForPop(Pop pop)
        {
            var entityComponent = pop.GetComponent<EntityDataComponent>();
            return entityComponent.EntityData;
        }

        public void RegisterPop(Pop pop)
        {
            livingPops.Add(pop);
            
            // Add EntityDataComponent if using GameData system
            var entityComponent = pop.GetComponent<EntityDataComponent>();
            entityComponents.Add(entityComponent);
            
            currentPopulation++;
            OnPopulationChanged?.Invoke(currentPopulation);
        }

        public void KillPop(Pop pop)
        {
            if (pop == null) return;

            int index = livingPops.IndexOf(pop);
            if (index >= 0)
            {
                livingPops.RemoveAt(index);
                if (index < entityComponents.Count)
                    entityComponents.RemoveAt(index);
                
                currentPopulation--;
                OnPopulationChanged?.Invoke(currentPopulation);
                
                Log.Info($"Pop died: {pop.name}", Log.LogCategory.Population);
                Destroy(pop.gameObject);
            }
        }

        private string GenerateRandomName()
        {
            string[] names = { "Aelyn", "Baris", "Cira", "Daven", "Elyn", "Fynn", "Gara", "Hael", "Ira", "Jax" };
            return names[Random.Range(0, names.Length)];
        }

        public List<Pop> GetAllPops() => new List<Pop>(livingPops);
        public List<EntityDataComponent> GetAllEntityData() => new List<EntityDataComponent>(entityComponents);
    }
}

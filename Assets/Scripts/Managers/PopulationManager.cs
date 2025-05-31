using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Debug;
using Lineage.Ancestral.Legacies.Systems;
using Lineage.Ancestral.Legacies.Components;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Ancestral.Legacies.Managers
{
    /// <summary>
    /// Manages the population of Kaari, spawning, death, and population cap.
    /// </summary>
    public class PopulationManager : MonoBehaviour
    {
        public static PopulationManager Instance { get; private set; }

        [Header("Population Settings")]
        public int populationCap = 5;
        public int currentPopulation = 0;
        public GameObject popPrefab; // Assign in inspector

        [Header("Spawning")]
        public Transform spawnPoint;
        public float spawnRadius = 2f;

        
        private List<Pop> livingPops = new List<Pop>();

        // Events
        public System.Action<int> OnPopulationChanged;
        public System.Action<int> OnPopulationCapChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
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
            // Process population needs and death
            ProcessPopulationNeeds();
        }

        private void ProcessPopulationNeeds()
        {
            for (int i = livingPops.Count - 1; i >= 0; i--)
            {
                var pop = livingPops[i];
                if (pop == null)
                {
                    livingPops.RemoveAt(i);
                    currentPopulation--;
                    OnPopulationChanged?.Invoke(currentPopulation);
                    continue;
                }                // Check if pop should die from starvation or critical needs
                if (pop.entityDataComponent != null && pop.entityDataComponent.HasCriticalNeeds())
                {
                    Log.Warning($"Pop {pop.name} died from critical needs!", Log.LogCategory.Population);
                    KillPop(pop);
                    continue;
                }

                // Generate faith if needs are met
                if (pop.hunger > 30f && pop.thirst > 30f)
                {
                    ResourceManager.Instance.AddFaith(ResourceManager.Instance.faithGenerationRate * Time.deltaTime);
                }

                // Generate food (basic gathering)
                ResourceManager.Instance.AddFood(0.5f * Time.deltaTime);
            }
        }        public void SpawnPop()
        {
            if (currentPopulation >= populationCap || popPrefab == null) return;

            Vector3 spawnPos = spawnPoint.position + Random.insideUnitSphere * spawnRadius;
            spawnPos.y = spawnPoint.position.y; // Keep on ground level

            GameObject popObj = Instantiate(popPrefab, spawnPos, Quaternion.identity);
            Pop pop = popObj.GetComponent<Pop>();
            
            if (pop != null)
            {
                pop.name = GenerateRandomName();
                pop.transform.localScale = Vector3.one * Random.Range(0.9f, 1.1f); // Randomize size slightly
                livingPops.Add(pop);
                currentPopulation++;
                OnPopulationChanged?.Invoke(currentPopulation);
                Log.Info($"New pop spawned: {pop.name}", Log.LogCategory.Population);
            }
        }

        public Pop SpawnPopAt(Vector3 position)
        {
            if (currentPopulation >= populationCap || popPrefab == null) return null;

            GameObject popObj = Instantiate(popPrefab, position, Quaternion.identity);
            Pop pop = popObj.GetComponent<Pop>();
            
            if (pop != null)
            {
                pop.name = GenerateRandomName();
                livingPops.Add(pop);
                currentPopulation++;
                OnPopulationChanged?.Invoke(currentPopulation);
                Log.Info($"New pop spawned at {position}: {pop.name}", Log.LogCategory.Population);
            }
            
            return pop;
        }

        public void KillPop(Pop pop)
        {
            if (livingPops.Contains(pop))
            {
                livingPops.Remove(pop);
                currentPopulation--;
                OnPopulationChanged?.Invoke(currentPopulation);
                
                if (pop.gameObject != null)
                    Destroy(pop.gameObject);
            }
        }

        public void OnPopDied(Pop pop)
        {
            // Called when a pop dies naturally (not killed by manager)
            if (livingPops.Contains(pop))
            {
                livingPops.Remove(pop);
                currentPopulation--;
                OnPopulationChanged?.Invoke(currentPopulation);
                Log.Info($"Pop {pop.name} died naturally. Population: {currentPopulation}", Log.LogCategory.Population);
            }
        }

        public bool ImproveShelter(float faithCost = 10f)
        {
            if (ResourceManager.Instance.ConsumeFaith(faithCost))
            {
                populationCap++;
                OnPopulationCapChanged?.Invoke(populationCap);
                Log.Info($"Shelter improved! Population cap increased to {populationCap}", Log.LogCategory.Population);
                return true;
            }
            return false;
        }

        private string GenerateRandomName()
        {
            string[] names = { "Kael", "Mira", "Thane", "Zara", "Bren", "Lyra", "Dak", "Nira", "Vor", "Tessa" };
            return names[Random.Range(0, names.Length)];
        }

        public List<Pop> GetLivingPops()
        {
            return new List<Pop>(livingPops);
        }
    }
}

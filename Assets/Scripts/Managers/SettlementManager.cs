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
    /// Comprehensive settlement management system that handles population, inventory, 
    /// resources, and overall settlement operations. Integrates with GameData system 
    /// for rich entity generation and supports genetic inheritance and trading systems.
    /// </summary>
    public class SettlementManager : MonoBehaviour
    {
        public static SettlementManager Instance { get; private set; }

        [Header("Settlement Configuration")]
        [SerializeField] private string _settlementName = "New Settlement";
        [SerializeField] private int _populationCap = 20;
        [SerializeField] private int _currentPopulation = 0;
        [SerializeField] private GameObject _popPrefab;

        [Header("Spawning")]
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _spawnRadius = 2f;
        
        [Header("GameData Integration")]
        [SerializeField] private bool _useGameDataSystem = true;
        [SerializeField] private bool _enableGeneticInheritance = true;
        [SerializeField] private bool _enableRandomTraits = true;
        
        [Header("Population Management")]
        [SerializeField] private float _breedingChance = 0.1f;
        [SerializeField] private float _minBreedingAge = 18f;
        [SerializeField] private float _maxBreedingAge = 45f;
        
        [Header("Settlement Resources")]
        [SerializeField] private int _maxStorageCapacity = 1000;
        [SerializeField] private float _resourceGenerationMultiplier = 1f;
        [SerializeField] private bool _enableAutoTrading = false;
        
        // Private collections
        private List<Pop> _livingPops = new List<Pop>();
        private List<EntityDataComponent> _entityComponents = new List<EntityDataComponent>();
        private Dictionary<string, int> _settlementStock = new Dictionary<string, int>();
        private Dictionary<string, float> _resourceProduction = new Dictionary<string, float>();

        // Events
        public System.Action<int> OnPopulationChanged;
        public System.Action<int> OnPopulationCapChanged;
        public System.Action<Pop, Pop, Pop> OnPopBorn; // Parent A, Parent B, Child
        public System.Action<string, int> OnResourceStockChanged;
        public System.Action<string> OnSettlementUpgraded;

        // Properties
        public string SettlementName => _settlementName;
        public int CurrentPopulation => _currentPopulation;
        public int PopulationCap => _populationCap;
        public int StorageUsed => _settlementStock.Values.Sum();
        public int StorageCapacity => _maxStorageCapacity;
        public float StorageUtilization => (float)StorageUsed / _maxStorageCapacity;

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSettlement();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SpawnInitialPopulation();
            InitializeResourceProduction();
            
            OnPopulationChanged?.Invoke(_currentPopulation);
            OnPopulationCapChanged?.Invoke(_populationCap);
        }

        private void Update()
        {
            ProcessPopulationNeeds();
            ProcessResourceGeneration();
            
            if (_enableGeneticInheritance && _currentPopulation < _populationCap)
            {
                ProcessBreeding();
            }
        }

        #endregion

        #region Settlement Initialization

        private void InitializeSettlement()
        {
            if (_useGameDataSystem)
            {
                GameData.InitializeAllDatabases();
            }
            
            InitializeStock();
            Log.Info($"Settlement '{_settlementName}' initialized", Log.LogCategory.Population);
        }

        private void InitializeStock()
        {
            _settlementStock["Food"] = 50;
            _settlementStock["Water"] = 100;
            _settlementStock["Wood"] = 25;
            _settlementStock["Stone"] = 10;
            _settlementStock["Tools"] = 5;
            _settlementStock["Clothing"] = 10;
        }

        private void InitializeResourceProduction()
        {
            _resourceProduction["Food"] = 0.5f;
            _resourceProduction["Water"] = 1.0f;
            _resourceProduction["Faith"] = 0.1f;
        }

        private void SpawnInitialPopulation()
        {
            int initialPops = Mathf.Min(3, _populationCap);
            for (int i = 0; i < initialPops; i++)
            {
                SpawnPop();
            }
        }

        #endregion

        #region Population Management

        private void ProcessPopulationNeeds()
        {
            for (int i = _livingPops.Count - 1; i >= 0; i--)
            {
                var pop = _livingPops[i];
                if (pop == null)
                {
                    RemoveNullPop(i);
                    continue;
                }

                EntityDataComponent entityData = i < _entityComponents.Count ? _entityComponents[i] : null;
                
                if (entityData != null && _useGameDataSystem)
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
            var healthStat = entityData.GetStat(Stat.ID.Health);
            
            if (healthStat.currentValue <= 0)
            {
                Log.Warning($"Pop {pop.name} died in settlement '{_settlementName}'!", Log.LogCategory.Population);
                KillPop(pop);
                return;
            }

            var currentState = entityData.GetCurrentState();
            ProcessPopStateEffects(pop, entityData, (State.ID)currentState.stateID);
            ManagePopStateTransitions(entityData);
        }

        private void ProcessPopStateEffects(Pop pop, EntityDataComponent entityData, State.ID stateID)
        {
            switch (stateID)
            {
                case State.ID.Gathering:
                    AddResource("Food", 2f * Time.deltaTime * _resourceGenerationMultiplier);
                    AddResource("Water", 1f * Time.deltaTime * _resourceGenerationMultiplier);
                    entityData.ModifyStat(Stat.ID.Stamina, -2f * Time.deltaTime);
                    break;
                    
                case State.ID.Crafting:
                    if (ConsumeResource("Wood", 0.5f * Time.deltaTime))
                    {
                        AddResource("Tools", 0.1f * Time.deltaTime * _resourceGenerationMultiplier);
                    }
                    entityData.ModifyStat(Stat.ID.Stamina, -1f * Time.deltaTime);
                    break;
                    
                case State.ID.Resting:
                    entityData.ModifyStat(Stat.ID.Stamina, 5f * Time.deltaTime);
                    entityData.ModifyStat(Stat.ID.Health, 1f * Time.deltaTime);
                    break;
                    
                case State.ID.Socializing:
                    AddResource("Faith", 0.2f * Time.deltaTime * _resourceGenerationMultiplier);
                    break;
            }
        }

        private void ProcessBasicNeeds(Pop pop)
        {
            var needsComponent = pop.GetComponent<Lineage.Ancestral.Legacies.Systems.Needs.NeedsComponent>();
            
            if (needsComponent != null)
            {
                if (needsComponent.hunger <= 0f)
                {
                    Log.Warning($"Pop {pop.name} died of starvation in settlement '{_settlementName}'!", Log.LogCategory.Population);
                    KillPop(pop);
                    return;
                }

                if (needsComponent.hunger > 30f && needsComponent.thirst > 30f)
                {
                    AddResource("Faith", 0.1f * Time.deltaTime * _resourceGenerationMultiplier);
                }

                AddResource("Food", 0.5f * Time.deltaTime * _resourceGenerationMultiplier);
            }
        }

        private void ManagePopStateTransitions(EntityDataComponent entityData)
        {
            var staminaStat = entityData.GetStat(Stat.ID.Stamina);
            
            if (staminaStat.currentValue < 20f)
            {
                entityData.ChangeState(State.ID.Resting);
            }
            else if (GetResourceStock("Food") < _currentPopulation * 5)
            {
                entityData.ChangeState(State.ID.Gathering);
            }
            else if (GetResourceStock("Tools") < _currentPopulation)
            {
                entityData.ChangeState(State.ID.Crafting);
            }
            else if (Random.Range(0f, 1f) < 0.001f)
            {
                ChangeToRandomState(entityData);
            }
        }

        private void ProcessBreeding()
        {
            if (Random.Range(0f, 1f) > _breedingChance * Time.deltaTime) return;

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
            
            for (int i = 0; i < _livingPops.Count; i++)
            {
                var pop = _livingPops[i];
                if (pop == null) continue;

                float age = pop.age;
                
                if (age >= _minBreedingAge && age <= _maxBreedingAge)
                {
                    if (_useGameDataSystem && i < _entityComponents.Count)
                    {
                        var entityData = _entityComponents[i];
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

        #endregion

        #region Resource Management

        private void ProcessResourceGeneration()
        {
            foreach (var resource in _resourceProduction)
            {
                float baseGeneration = resource.Value * Time.deltaTime;
                float populationMultiplier = Mathf.Max(1f, _currentPopulation * 0.1f);
                AddResource(resource.Key, baseGeneration * populationMultiplier);
            }
        }

        public bool AddResource(string resourceType, float amount)
        {
            if (StorageUsed + (int)amount > _maxStorageCapacity)
            {
                Log.Warning($"Settlement '{_settlementName}' storage full! Cannot add {amount} {resourceType}", Log.LogCategory.Population);
                return false;
            }

            if (!_settlementStock.ContainsKey(resourceType))
                _settlementStock[resourceType] = 0;

            _settlementStock[resourceType] += (int)amount;
            OnResourceStockChanged?.Invoke(resourceType, _settlementStock[resourceType]);
            return true;
        }

        public bool ConsumeResource(string resourceType, float amount)
        {
            if (!_settlementStock.ContainsKey(resourceType) || _settlementStock[resourceType] < amount)
            {
                return false;
            }

            _settlementStock[resourceType] -= (int)amount;
            if (_settlementStock[resourceType] < 0)
                _settlementStock[resourceType] = 0;

            OnResourceStockChanged?.Invoke(resourceType, _settlementStock[resourceType]);
            return true;
        }

        public int GetResourceStock(string resourceType)
        {
            return _settlementStock.ContainsKey(resourceType) ? _settlementStock[resourceType] : 0;
        }

        public Dictionary<string, int> GetAllResources()
        {
            return new Dictionary<string, int>(_settlementStock);
        }

        #endregion

        #region Pop Spawning and Management

        public void SpawnPop()
        {
            if (_currentPopulation >= _populationCap || _popPrefab == null) return;

            Vector3 spawnPos = _spawnPoint.position + Random.insideUnitSphere * _spawnRadius;
            spawnPos.y = _spawnPoint.position.y;

            GameObject popObj = Instantiate(_popPrefab, spawnPos, Quaternion.identity);
            Pop pop = popObj.GetComponent<Pop>();
            
            if (pop != null)
            {
                if (!_useGameDataSystem)
                {
                    pop.name = GenerateRandomName();
                }
                
                pop.transform.localScale = Vector3.one * Random.Range(0.9f, 1.1f);
                RegisterPop(pop);
                
                Log.Info($"New pop spawned in settlement '{_settlementName}': {pop.name}", Log.LogCategory.Population);
            }
        }

        public void SpawnChildPop(Pop parentA, Pop parentB)
        {
            if (_currentPopulation >= _populationCap || _popPrefab == null) return;

            Vector3 spawnPos = _spawnPoint.position + Random.insideUnitSphere * _spawnRadius;
            spawnPos.y = _spawnPoint.position.y;

            GameObject popObj = Instantiate(_popPrefab, spawnPos, Quaternion.identity);
            Pop childPop = popObj.GetComponent<Pop>();
            
            if (childPop != null)
            {
                childPop.age = 0;
                childPop.transform.localScale = Vector3.one * 0.5f;
                
                RegisterPop(childPop);
                
                OnPopBorn?.Invoke(parentA, parentB, childPop);
                Log.Info($"Child pop born in settlement '{_settlementName}': {childPop.popName} (Parents: {parentA.name} & {parentB.name})", Log.LogCategory.Population);
            }
        }

        public void RegisterPop(Pop pop)
        {
            _livingPops.Add(pop);
            
            var entityComponent = pop.GetComponent<EntityDataComponent>();
            _entityComponents.Add(entityComponent);
            
            _currentPopulation++;
            OnPopulationChanged?.Invoke(_currentPopulation);
        }

        public void KillPop(Pop pop)
        {
            if (pop == null) return;

            int index = _livingPops.IndexOf(pop);
            if (index >= 0)
            {
                _livingPops.RemoveAt(index);
                if (index < _entityComponents.Count)
                    _entityComponents.RemoveAt(index);
                
                _currentPopulation--;
                OnPopulationChanged?.Invoke(_currentPopulation);
                
                Log.Info($"Pop died in settlement '{_settlementName}': {pop.name}", Log.LogCategory.Population);
                Destroy(pop.gameObject);
            }
        }

        private void RemoveNullPop(int index)
        {
            _livingPops.RemoveAt(index);
            if (index < _entityComponents.Count)
                _entityComponents.RemoveAt(index);
            _currentPopulation--;
            OnPopulationChanged?.Invoke(_currentPopulation);
        }

        #endregion

        #region Utility Methods

        private void ChangeToRandomState(EntityDataComponent entityData)
        {
            var availableStates = entityData.GetAvailableStates();
            if (availableStates.Count > 0)
            {
                var randomState = availableStates[Random.Range(0, availableStates.Count)];
                entityData.ChangeState((State.ID)randomState.stateID);
            }
        }

        private string GenerateRandomName()
        {
            //TODO: Pull data from the database file for random name generation. Names in the database file can be expanded on from pulling .jsons from the /names/ folder in the mod folder.
            string[] names = { "Aelyn", "Baris", "Cira", "Daven", "Elyn", "Fynn", "Gara", "Hael", "Ira", "Jax" };
            return names[Random.Range(0, names.Length)];
        }

        #endregion

        #region Public API

        public List<Pop> GetAllPops() => new List<Pop>(_livingPops);
        public List<EntityDataComponent> GetAllEntityData() => new List<EntityDataComponent>(_entityComponents);
        
        public void SetSettlementName(string newName)
        {
            _settlementName = newName;
            Log.Info($"Settlement renamed to '{_settlementName}'", Log.LogCategory.Population);
        }

        public void UpgradePopulationCap(int increase)
        {
            _populationCap += increase;
            OnPopulationCapChanged?.Invoke(_populationCap);
        }

        public void UpgradeStorageCapacity(int increase)
        {
            _maxStorageCapacity += increase;
        }

        #endregion
    }
}

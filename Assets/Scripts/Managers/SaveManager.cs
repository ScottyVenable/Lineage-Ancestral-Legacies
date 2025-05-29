using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Managers
{
    /// <summary>
    /// Manages game saves, settings persistence, and player preferences.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Save Settings")]
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
        [SerializeField] private int maxSaveSlots = 5;

        private string saveDirectory;
        private float autoSaveTimer;

        // Events
        public System.Action OnGameSaved;
        public System.Action OnGameLoaded;

        [System.Serializable]
        public class GameSaveData
        {
            public float playTime;
            public int currentPopulation;
            public float currentFood;
            public float currentFaith;
            public float currentWood;
            public int populationCap;
            public bool hasEfficientGathering;
            public Vector3[] popPositions;
            public PopSaveData[] popData;
            public string saveDateTime;
            public string playerName;
            public int gameGeneration;
        }

        [System.Serializable]
        public class PopSaveData
        {
            public Vector3 position;
            public float hunger;
            public float thirst;
            public float energy;
            public string currentState;
            public string[] traits;
        }

        [System.Serializable]
        public class GameSettings
        {
            public float masterVolume = 1f;
            public float musicVolume = 0.7f;
            public float sfxVolume = 0.8f;
            public float ambientVolume = 0.5f;
            public int qualityLevel = 2;
            public bool fullscreen = true;
            public int screenWidth = 1920;
            public int screenHeight = 1080;
            public bool vsync = true;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSaveSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadSettings();
        }

        private void Update()
        {
            if (autoSave)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    AutoSave();
                    autoSaveTimer = 0f;
                }
            }
        }

        private void InitializeSaveSystem()
        {
            saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            UnityEngine.Debug.Log($"Save directory: {saveDirectory}");
        }

        public void SaveGame(int slot = 0)
        {
            try
            {
                GameSaveData saveData = CreateSaveData();
                string json = JsonUtility.ToJson(saveData, true);
                string filePath = Path.Combine(saveDirectory, $"save_slot_{slot}.json");
                
                File.WriteAllText(filePath, json);
                
                UnityEngine.Debug.Log($"Game saved to slot {slot}");
                OnGameSaved?.Invoke();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to save game: {e.Message}");
            }
        }

        public bool LoadGame(int slot = 0)
        {
            try
            {
                string filePath = Path.Combine(saveDirectory, $"save_slot_{slot}.json");
                
                if (!File.Exists(filePath))
                {
                    UnityEngine.Debug.LogWarning($"Save file not found in slot {slot}");
                    return false;
                }

                string json = File.ReadAllText(filePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                
                ApplySaveData(saveData);
                
                UnityEngine.Debug.Log($"Game loaded from slot {slot}");
                OnGameLoaded?.Invoke();
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to load game: {e.Message}");
                return false;
            }
        }

        public void DeleteSave(int slot)
        {
            try
            {
                string filePath = Path.Combine(saveDirectory, $"save_slot_{slot}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    UnityEngine.Debug.Log($"Save slot {slot} deleted");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to delete save: {e.Message}");
            }
        }

        public bool SaveExists(int slot)
        {
            string filePath = Path.Combine(saveDirectory, $"save_slot_{slot}.json");
            return File.Exists(filePath);
        }

        public List<GameSaveData> GetAllSaves()
        {
            List<GameSaveData> saves = new List<GameSaveData>();
            
            for (int i = 0; i < maxSaveSlots; i++)
            {
                string filePath = Path.Combine(saveDirectory, $"save_slot_{i}.json");
                if (File.Exists(filePath))
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                        saves.Add(saveData);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Failed to load save preview for slot {i}: {e.Message}");
                    }
                }
                else
                {
                    saves.Add(null); // Empty slot
                }
            }
            
            return saves;
        }

        public void AutoSave()
        {
            SaveGame(0); // Auto-save always uses slot 0
        }

        public void QuickSave()
        {
            SaveGame(1); // Quick save uses slot 1
        }

        public void QuickLoad()
        {
            LoadGame(1);
        }

        public void SaveSettings(GameSettings settings)
        {
            try
            {
                string json = JsonUtility.ToJson(settings, true);
                string filePath = Path.Combine(saveDirectory, "settings.json");
                File.WriteAllText(filePath, json);
                
                ApplySettings(settings);
                UnityEngine.Debug.Log("Settings saved");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to save settings: {e.Message}");
            }
        }

        public void LoadSettings()
        {
            try
            {
                string filePath = Path.Combine(saveDirectory, "settings.json");
                
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    GameSettings settings = JsonUtility.FromJson<GameSettings>(json);
                    ApplySettings(settings);
                }
                else
                {
                    // Apply default settings
                    ApplySettings(new GameSettings());
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to load settings: {e.Message}");
                ApplySettings(new GameSettings());
            }
        }

        private GameSaveData CreateSaveData()
        {
            GameSaveData saveData = new GameSaveData
            {
                playTime = Time.time,
                saveDateTime = System.DateTime.Now.ToString(),
                playerName = "Player", // TODO: Get from player input
                gameGeneration = 1 // TODO: Implement generation system
            };

            // Get data from managers
            if (ResourceManager.Instance != null)
            {
                saveData.currentFood = ResourceManager.Instance.currentFood;
                saveData.currentFaith = ResourceManager.Instance.currentFaithPoints;
                saveData.currentWood = ResourceManager.Instance.currentWood;
                saveData.hasEfficientGathering = ResourceManager.Instance.hasEfficientGathering;
            }

            if (PopulationManager.Instance != null)
            {
                saveData.currentPopulation = PopulationManager.Instance.currentPopulation;
                saveData.populationCap = PopulationManager.Instance.populationCap;                // Save pop data
                var allPops = FindObjectsByType<Entities.Pop>(FindObjectsSortMode.None);
                saveData.popPositions = new Vector3[allPops.Length];
                saveData.popData = new PopSaveData[allPops.Length];

                for (int i = 0; i < allPops.Length; i++)
                {
                    saveData.popPositions[i] = allPops[i].transform.position;
                    
                    var needsComponent = allPops[i].GetComponent<Systems.Needs.NeedsComponent>();
                    if (needsComponent != null)
                    {
                        saveData.popData[i] = new PopSaveData
                        {
                            position = allPops[i].transform.position,
                            hunger = needsComponent.hunger,
                            thirst = needsComponent.thirst,
                            energy = needsComponent.hunger, // Using hunger as energy placeholder until energy is implemented
                            currentState = allPops[i].GetComponent<AI.PopStateMachine>()?.currentState?.GetType().Name ?? "Idle",
                            traits = new string[0] // TODO: Implement traits saving
                        };
                    }
                }
            }

            return saveData;
        }

        private void ApplySaveData(GameSaveData saveData)
        {
            // Apply data to managers
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.SetFood(saveData.currentFood);
                ResourceManager.Instance.SetFaith(saveData.currentFaith);
                ResourceManager.Instance.SetWood(saveData.currentWood);
                ResourceManager.Instance.hasEfficientGathering = saveData.hasEfficientGathering;
            }

            if (PopulationManager.Instance != null)
            {                // Clear existing pops
                var existingPops = FindObjectsByType<Entities.Pop>(FindObjectsSortMode.None);
                foreach (var pop in existingPops)
                {
                    DestroyImmediate(pop.gameObject);
                }

                // Restore population
                PopulationManager.Instance.populationCap = saveData.populationCap;
                
                if (saveData.popData != null)
                {
                    for (int i = 0; i < saveData.popData.Length; i++)
                    {
                        var popData = saveData.popData[i];
                        var newPop = PopulationManager.Instance.SpawnPopAt(popData.position);
                        
                        if (newPop != null)
                        {
                            var needsComponent = newPop.GetComponent<Systems.Needs.NeedsComponent>();
                            if (needsComponent != null)
                            {
                                needsComponent.hunger = popData.hunger;
                                needsComponent.thirst = popData.thirst;
                                needsComponent.energy = popData.energy;
                            }
                        }
                    }
                }
            }
        }

        private void ApplySettings(GameSettings settings)
        {
            // Apply audio settings
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.masterVolume = settings.masterVolume;
                AudioManager.Instance.musicVolume = settings.musicVolume;
                AudioManager.Instance.sfxVolume = settings.sfxVolume;
                AudioManager.Instance.ambientVolume = settings.ambientVolume;
                AudioManager.Instance.UpdateVolumes();
            }

            // Apply graphics settings
            QualitySettings.SetQualityLevel(settings.qualityLevel);
            Screen.SetResolution(settings.screenWidth, settings.screenHeight, settings.fullscreen);
            QualitySettings.vSyncCount = settings.vsync ? 1 : 0;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && autoSave)
            {
                AutoSave();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && autoSave)
            {
                AutoSave();
            }
        }
    }
}

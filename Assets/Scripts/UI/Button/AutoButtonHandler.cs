using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Lineage.Debug;
using Lineage.Managers;

namespace Lineage.UI
{
    /// <summary>
    /// Automatically handles button functionality based on the button's text label.
    /// Attach this to any button prefab and it will execute the appropriate logic based on the button's text.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AutoButtonHandler : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool ignoreCase = true;
        [SerializeField] private bool trimWhitespace = true;
        [SerializeField] private bool enableLogging = true;

        private Button button;
        private TMP_Text buttonText;
        private string currentButtonText;

        // Dictionary to map button text to actions
        private Dictionary<string, System.Action> buttonActions;

        private void Awake()
        {
            // Get required components
            button = GetComponent<Button>();
            buttonText = GetComponentInChildren<TMP_Text>();

            if (buttonText == null)
            {
                Log.Error($"AutoButtonHandler on {gameObject.name}: No TMP_Text component found in children!", Log.LogCategory.UI);
                enabled = false;
                return;
            }

            InitializeButtonActions();
            SetupButtonListener();
        }

        private void Start()
        {
            // Update the button text reference in case it changed
            UpdateButtonText();
        }

        private void InitializeButtonActions()
        {
            buttonActions = new Dictionary<string, System.Action>();

            // Game Management Actions
            buttonActions["quit"] = QuitGame;
            buttonActions["exit"] = QuitGame;
            buttonActions["quit game"] = QuitGame;
            buttonActions["exit game"] = QuitGame;

            // Save/Load Actions
            buttonActions["save"] = SaveGame;
            buttonActions["save game"] = SaveGame;
            buttonActions["load"] = LoadGame;
            buttonActions["load game"] = LoadGame;
            buttonActions["quick save"] = QuickSave;
            buttonActions["quick load"] = QuickLoad;
            buttonActions["auto save"] = AutoSave;

            // Menu Navigation Actions
            buttonActions["main menu"] = GoToMainMenu;
            buttonActions["menu"] = GoToMainMenu;
            buttonActions["back"] = GoBack;
            buttonActions["return"] = GoBack;
            buttonActions["cancel"] = GoBack;
            buttonActions["close"] = CloseUI;

            // Game State Actions
            buttonActions["pause"] = PauseGame;
            buttonActions["resume"] = ResumeGame;
            buttonActions["unpause"] = ResumeGame;
            buttonActions["restart"] = RestartGame;
            buttonActions["new game"] = NewGame;
            buttonActions["start game"] = StartGame;

            // Settings Actions
            buttonActions["settings"] = OpenSettings;
            buttonActions["options"] = OpenSettings;
            buttonActions["apply"] = ApplySettings;
            buttonActions["reset"] = ResetSettings;
            buttonActions["defaults"] = ResetSettings;

            // Audio Actions
            buttonActions["mute"] = ToggleMute;
            buttonActions["unmute"] = ToggleMute;

            // Population Actions
            buttonActions["spawn pop"] = SpawnPop;
            buttonActions["add pop"] = SpawnPop;
            buttonActions["create pop"] = SpawnPop;

            // Resource Actions
            buttonActions["add food"] = AddFood;
            buttonActions["add wood"] = AddWood;
            buttonActions["add faith"] = AddFaith;

            // Debug Actions
            buttonActions["debug"] = ToggleDebug;
            buttonActions["console"] = ToggleDebug;
            buttonActions["debug menu"] = ToggleDebug;
        }

        private void SetupButtonListener()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleButtonClick);
            }
        }

        private void UpdateButtonText()
        {
            if (buttonText != null)
            {
                string rawText = buttonText.text;
                
                if (trimWhitespace)
                    rawText = rawText.Trim();
                
                if (ignoreCase)
                    rawText = rawText.ToLower();

                currentButtonText = rawText;

                if (enableLogging)
                {
                    Log.Info($"AutoButtonHandler on {gameObject.name}: Button text set to '{currentButtonText}'", Log.LogCategory.UI);
                }
            }
        }

        private void HandleButtonClick()
        {
            UpdateButtonText();

            if (string.IsNullOrEmpty(currentButtonText))
            {
                Log.Warning($"AutoButtonHandler on {gameObject.name}: Button text is empty or null!", Log.LogCategory.UI);
                return;
            }

            if (buttonActions.TryGetValue(currentButtonText, out System.Action action))
            {
                if (enableLogging)
                {
                    Log.Info($"AutoButtonHandler: Executing action for '{currentButtonText}'", Log.LogCategory.UI);
                }
                
                try
                {
                    action.Invoke();
                }
                catch (System.Exception e)
                {
                    Log.Error($"AutoButtonHandler: Error executing action for '{currentButtonText}': {e.Message}", Log.LogCategory.UI);
                }
            }
            else
            {
                Log.Warning($"AutoButtonHandler: No action found for button text '{currentButtonText}'", Log.LogCategory.UI);
            }
        }

        // Add a new button action at runtime
        public void AddButtonAction(string buttonText, System.Action action)
        {
            string processedText = buttonText;
            
            if (trimWhitespace)
                processedText = processedText.Trim();
            
            if (ignoreCase)
                processedText = processedText.ToLower();

            buttonActions[processedText] = action;
            
            if (enableLogging)
            {
                Log.Info($"AutoButtonHandler: Added custom action for '{processedText}'", Log.LogCategory.UI);
            }
        }

        // Remove a button action
        public void RemoveButtonAction(string buttonText)
        {
            string processedText = buttonText;
            
            if (trimWhitespace)
                processedText = processedText.Trim();
            
            if (ignoreCase)
                processedText = processedText.ToLower();

            if (buttonActions.Remove(processedText))
            {
                if (enableLogging)
                {
                    Log.Info($"AutoButtonHandler: Removed action for '{processedText}'", Log.LogCategory.UI);
                }
            }
        }

        // Check if an action exists for the given text
        public bool HasAction(string buttonText)
        {
            string processedText = buttonText;
            
            if (trimWhitespace)
                processedText = processedText.Trim();
            
            if (ignoreCase)
                processedText = processedText.ToLower();

            return buttonActions.ContainsKey(processedText);
        }

        #region Button Action Implementations

        private void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void SaveGame()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
            else
            {
                Log.Warning("SaveManager instance not found!", Log.LogCategory.UI);
            }
        }

        private void LoadGame()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.LoadGame();
            }
            else
            {
                Log.Warning("SaveManager instance not found!", Log.LogCategory.UI);
            }
        }

        private void QuickSave()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.QuickSave();
            }
            else
            {
                Log.Warning("SaveManager instance not found!", Log.LogCategory.UI);
            }
        }

        private void QuickLoad()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.QuickLoad();
            }
            else
            {
                Log.Warning("SaveManager instance not found!", Log.LogCategory.UI);
            }
        }

        private void AutoSave()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.AutoSave();
            }
            else
            {
                Log.Warning("SaveManager instance not found!", Log.LogCategory.UI);
            }
        }

        private void GoToMainMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Assumes main menu is scene 0
        }

        private void GoBack()
        {
            // Try to find a parent canvas or UI manager to handle going back
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }
            else
            {
                Log.Info("Go back action triggered but no specific back logic implemented", Log.LogCategory.UI);
            }
        }

        private void CloseUI()
        {
            // Close the current UI panel or window
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }
            else
            {
                transform.parent?.gameObject.SetActive(false);
            }
        }

        private void PauseGame()
        {
            Time.timeScale = 0f;
            Log.Info("Game paused", Log.LogCategory.UI);
        }

        private void ResumeGame()
        {
            Time.timeScale = 1f;
            Log.Info("Game resumed", Log.LogCategory.UI);
        }

        private void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void NewGame()
        {
            Time.timeScale = 1f;
            // Load the main game scene (assuming it's scene 1, adjust as needed)
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }

        private void StartGame()
        {
            NewGame();
        }

        private void OpenSettings()
        {
            Log.Info("Open settings action triggered - implement settings UI logic", Log.LogCategory.UI);
        }

        private void ApplySettings()
        {
            Log.Info("Apply settings action triggered", Log.LogCategory.UI);
        }

        private void ResetSettings()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.LoadSettings();
                Log.Info("Settings reset to defaults", Log.LogCategory.UI);
            }
        }

        private void ToggleMute()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.masterVolume = AudioManager.Instance.masterVolume > 0 ? 0 : 1;
                AudioManager.Instance.UpdateVolumes();
            }
            else
            {
                AudioListener.volume = AudioListener.volume > 0 ? 0 : 1;
            }
        }

        private void SpawnPop()
        {
            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.SpawnPop();
            }
            else
            {
                Log.Warning("PopulationManager instance not found!", Log.LogCategory.UI);
            }
        }

        private void AddFood()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddFood(10f); // Add 10 food
            }
            else
            {
                Log.Warning("ResourceManager instance not found!", Log.LogCategory.UI);
            }
        }

        private void AddWood()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddWood(10f); // Add 10 wood
            }
            else
            {
                Log.Warning("ResourceManager instance not found!", Log.LogCategory.UI);
            }
        }

        private void AddFaith()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddFaith(10f); // Add 10 faith
            }
            else
            {
                Log.Warning("ResourceManager instance not found!", Log.LogCategory.UI);
            }
        }

        private void ToggleDebug()
        {
            Log.Info("Debug toggle action triggered", Log.LogCategory.UI);
            // Implement debug menu toggle logic here
        }

        #endregion

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleButtonClick);
            }
        }

        #if UNITY_EDITOR
        [Header("Editor Tools")]
        [SerializeField] private bool showAvailableActions = false;
        
        private void OnValidate()
        {
            if (showAvailableActions && buttonActions != null)
            {
                UnityEngine.Debug.Log($"Available button actions: {string.Join(", ", buttonActions.Keys)}");
            }
        }
        #endif
    }
}

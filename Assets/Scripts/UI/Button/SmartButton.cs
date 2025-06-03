using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.UI
{
    /// <summary>
    /// Smart button that automatically determines its functionality based on its text label.
    /// Attach this to any button and it will automatically handle the appropriate logic
    /// based on what the button's text says.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SmartButton : MonoBehaviour
    {
        [Header("Smart Button Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool useCustomText = false;
        [SerializeField] private string customButtonText = "";

        private Button button;
        private TMP_Text buttonText;
        private string currentButtonText;

        // Button action mappings - you can easily add more here!
        private readonly System.Collections.Generic.Dictionary<string, System.Action> buttonActions = 
            new System.Collections.Generic.Dictionary<string, System.Action>(System.StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {
            // Get components
            button = GetComponent<Button>();
            
            // Try to find TMP_Text component on this object or children
            buttonText = GetComponentInChildren<TMP_Text>();
            
            if (buttonText == null)
            {
                // Fallback to regular Text component if TMP_Text not found
                var regularText = GetComponentInChildren<Text>();
                if (regularText != null)
                {
                    AdvancedLogger.LogWarning(LogCategory.UI, $"SmartButton on {gameObject.name}: Using regular Text instead of TMP_Text. Consider upgrading to TextMeshPro for better performance.");
                }
                else
                {
                    AdvancedLogger.LogError(LogCategory.UI, $"SmartButton on {gameObject.name}: No Text or TMP_Text component found! Button will not function properly.");
                    return;
                }
            }

            // Initialize button actions
            InitializeButtonActions();
            
            // Set up the button click listener
            SetupButtonListener();
        }

        private void InitializeButtonActions()
        {
            // Clear existing actions
            buttonActions.Clear();

            // Game Flow Actions
            buttonActions["Quit"] = QuitGame;
            buttonActions["Exit"] = QuitGame;
            buttonActions["Quit Game"] = QuitGame;
            buttonActions["Exit Game"] = QuitGame;
            
            buttonActions["Play"] = PlayGame;
            buttonActions["Start"] = PlayGame;
            buttonActions["Start Game"] = PlayGame;
            buttonActions["New Game"] = PlayGame;
            
            buttonActions["Continue"] = ContinueGame;
            buttonActions["Resume"] = ResumeGame;
            buttonActions["Resume Game"] = ResumeGame;
            
            buttonActions["Restart"] = RestartGame;
            buttonActions["Restart Game"] = RestartGame;
            
            // Menu Navigation
            buttonActions["Main Menu"] = GoToMainMenu;
            buttonActions["Menu"] = GoToMainMenu;
            buttonActions["Back"] = GoBack;
            buttonActions["Return"] = GoBack;
            
            buttonActions["Settings"] = OpenSettings;
            buttonActions["Options"] = OpenSettings;
            buttonActions["Preferences"] = OpenSettings;
            
            buttonActions["Credits"] = OpenCredits;
            
            // Save/Load Actions
            buttonActions["Save"] = SaveGame;
            buttonActions["Save Game"] = SaveGame;
            buttonActions["Quick Save"] = QuickSave;
            
            buttonActions["Load"] = LoadGame;
            buttonActions["Load Game"] = LoadGame;
            buttonActions["Quick Load"] = QuickLoad;
            
            // Game Actions
            buttonActions["Pause"] = PauseGame;
            buttonActions["Unpause"] = UnpauseGame;
            
            // Population Actions
            buttonActions["Spawn Pop"] = SpawnPop;
            buttonActions["Add Pop"] = SpawnPop;
            buttonActions["Create Pop"] = SpawnPop;
            
            // Debug Actions (only in debug builds)
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            buttonActions["Debug Console"] = OpenDebugConsole;
            buttonActions["Console"] = OpenDebugConsole;
            buttonActions["Toggle Debug"] = ToggleDebug;
            buttonActions["God Mode"] = ToggleGodMode;
            #endif

            // UI Actions
            buttonActions["Close"] = CloseUI;
            buttonActions["Cancel"] = CloseUI;
            buttonActions["OK"] = CloseUI;
            buttonActions["Confirm"] = ConfirmAction;
            buttonActions["Accept"] = ConfirmAction;
            buttonActions["Yes"] = ConfirmAction;
            buttonActions["No"] = CancelAction;
            buttonActions["Decline"] = CancelAction;

            if (debugMode)
            {
                AdvancedLogger.LogInfo(LogCategory.UI, $"SmartButton: Initialized {buttonActions.Count} button actions");
            }
        }

        private void SetupButtonListener()
        {
            if (button == null) return;

            // Remove existing listeners to avoid duplicates
            button.onClick.RemoveAllListeners();
            
            // Add our smart button listener
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            // Get the current button text
            string buttonText = GetButtonText();
            
            if (string.IsNullOrEmpty(buttonText))
            {
                AdvancedLogger.LogWarning(LogCategory.UI, $"SmartButton on {gameObject.name}: Button text is empty or null!");
                return;
            }

            if (debugMode)
            {
                AdvancedLogger.LogInfo(LogCategory.UI, $"SmartButton clicked: '{buttonText}'");
            }

            // Try to find and execute the appropriate action
            if (buttonActions.TryGetValue(buttonText, out System.Action action))
            {
                try
                {
                    action.Invoke();
                    if (debugMode)
                    {
                        AdvancedLogger.LogInfo(LogCategory.UI, $"SmartButton: Successfully executed action for '{buttonText}'");
                    }
                }
                catch (System.Exception e)
                {
                    AdvancedLogger.LogError(LogCategory.UI, $"SmartButton: Error executing action for '{buttonText}': {e.Message}");
                }
            }
            else
            {
                AdvancedLogger.LogWarning(LogCategory.UI, $"SmartButton: No action found for button text '{buttonText}'. Available actions: {string.Join(", ", buttonActions.Keys)}");
            }
        }

        private string GetButtonText()
        {
            // Use custom text if specified
            if (useCustomText && !string.IsNullOrEmpty(customButtonText))
            {
                return customButtonText.Trim();
            }

            // Try TMP_Text first
            if (buttonText != null)
            {
                return buttonText.text.Trim();
            }

            // Fallback to regular Text
            var regularText = GetComponentInChildren<Text>();
            if (regularText != null)
            {
                return regularText.text.Trim();
            }

            return "";
        }

        #region Button Action Implementations

        // Game Flow Actions
        private void QuitGame()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Quitting game");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void PlayGame()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Starting new game");
            // Load the main game scene (adjust scene name as needed)
            SceneManager.LoadScene("GameScene"); // Change to your actual game scene name
        }

        private void ContinueGame()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Continuing game");
            if (SaveManager.Instance != null)
            {
                if (SaveManager.Instance.SaveExists(0)) // Auto-save slot
                {
                    SaveManager.Instance.LoadGame(0);
                }
                else
                {
                    AdvancedLogger.LogWarning(LogCategory.UI, "No save file found to continue from");
                    PlayGame(); // Start new game if no save exists
                }
            }
        }

        private void ResumeGame()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Resuming game");
            Time.timeScale = 1f;
            // Hide pause menu if it exists
            var pauseMenu = FindFirstObjectByType<Canvas>()?.transform.Find("PauseMenu");
            if (pauseMenu != null)
            {
                pauseMenu.gameObject.SetActive(false);
            }
        }

        private void RestartGame()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Restarting game");
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Menu Navigation
        private void GoToMainMenu()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Going to main menu");
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu"); // Change to your actual main menu scene name
        }

        private void GoBack()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Going back");
            // Try to find a menu manager or just close current UI
            CloseUI();
        }

        private void OpenSettings()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Opening settings");
            // Try to find and open settings panel
            var settingsPanel = FindFirstObjectByType<Canvas>()?.transform.Find("SettingsPanel");
            if (settingsPanel != null)
            {
                settingsPanel.gameObject.SetActive(true);
            }
            else
            {
                AdvancedLogger.LogWarning(LogCategory.UI, "Settings panel not found");
            }
        }

        private void OpenCredits()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Opening credits");
            var creditsPanel = FindFirstObjectByType<Canvas>()?.transform.Find("CreditsPanel");
            if (creditsPanel != null)
            {
                creditsPanel.gameObject.SetActive(true);
            }
            else
            {
                AdvancedLogger.LogWarning(LogCategory.UI, "Credits panel not found");
            }
        }

        // Save/Load Actions
        private void SaveGame()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Saving game");
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
        }

        private void QuickSave()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Quick saving");
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.QuickSave();
            }
        }

        private void LoadGame()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Loading game");
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.LoadGame();
            }
        }

        private void QuickLoad()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Quick loading");
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.QuickLoad();
            }
        }

        // Game Actions
        private void PauseGame()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Pausing game");
            Time.timeScale = 0f;
        }

        private void UnpauseGame()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Unpausing game");
            Time.timeScale = 1f;
        }

        // Population Actions
        private void SpawnPop()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Spawning pop");
            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.SpawnPop();
            }
        }

        // Debug Actions
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OpenDebugConsole()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Opening debug console");
            if (DebugManager.Instance != null && DebugManager.Instance.consoleManager != null)
            {
                // Toggle console visibility
                var console = DebugManager.Instance.consoleManager;
                // You might need to add a public method to toggle console visibility
            }
        }

        private void ToggleDebug()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Toggling debug mode");
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.EnableDebugSystems(!DebugManager.Instance.enabled);
            }
        }

        private void ToggleGodMode()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Toggling god mode");
            // Implement god mode logic here
        }
        #endif

        // UI Actions
        private void CloseUI()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Closing UI");
            // Try to close parent panel or destroy this object
            Transform parent = transform.parent;
            if (parent != null)
            {
                parent.gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void ConfirmAction()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Confirm action");
            // Default confirm behavior - close UI
            CloseUI();
        }

        private void CancelAction()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "SmartButton: Cancel action");
            // Default cancel behavior - close UI
            CloseUI();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Add a custom button action at runtime
        /// </summary>
        public void AddCustomAction(string buttonText, System.Action action)
        {
            buttonActions[buttonText] = action;
            if (debugMode)
            {
                AdvancedLogger.LogInfo(LogCategory.UI, $"SmartButton: Added custom action for '{buttonText}'");
            }
        }

        /// <summary>
        /// Remove a button action
        /// </summary>
        public void RemoveAction(string buttonText)
        {
            if (buttonActions.ContainsKey(buttonText))
            {
                buttonActions.Remove(buttonText);
                if (debugMode)
                {
                    AdvancedLogger.LogInfo(LogCategory.UI, $"SmartButton: Removed action for '{buttonText}'");
                }
            }
        }

        /// <summary>
        /// Check if an action exists for the given text
        /// </summary>
        public bool HasAction(string buttonText)
        {
            return buttonActions.ContainsKey(buttonText);
        }

        /// <summary>
        /// Get all available button actions
        /// </summary>
        public string[] GetAvailableActions()
        {
            var keys = new string[buttonActions.Count];
            buttonActions.Keys.CopyTo(keys, 0);
            return keys;
        }

        /// <summary>
        /// Force refresh the button text and setup
        /// </summary>
        public void RefreshButton()
        {
            SetupButtonListener();
            currentButtonText = GetButtonText();
        }

        #endregion

        #region Editor Helpers

        #if UNITY_EDITOR
        [ContextMenu("List Available Actions")]
        private void ListAvailableActions()
        {
            if (buttonActions.Count == 0)
            {
                InitializeButtonActions();
            }

            UnityEngine.Debug.Log($"SmartButton Available Actions ({buttonActions.Count}):\n" + 
                      string.Join("\n", buttonActions.Keys));
        }

        [ContextMenu("Test Button Action")]
        private void TestButtonAction()
        {
            OnButtonClicked();
        }
        #endif

        #endregion
    }
}

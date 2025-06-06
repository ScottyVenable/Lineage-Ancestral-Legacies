using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Lineage.Ancestral.Legacies.Debug;
using Lineage.Ancestral.Legacies.Managers;

namespace Lineage.Ancestral.Legacies.UI
{
    /// <summary>
    /// Enhanced AutoButtonHandler with parameterized actions, regex support, and visual feedback.
    /// This extends the base AutoButtonHandler with additional features for more complex scenarios.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class EnhancedAutoButtonHandler : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool ignoreCase = true;
        [SerializeField] private bool trimWhitespace = true;
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool useRegexPatterns = false;
        [SerializeField] private bool enableVisualFeedback = true;

        [SerializeField] private GameObject panelToClose;

        [Header("Visual Feedback")]
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private float feedbackDuration = 1f;

        private Button button;
        private TMP_Text buttonText;
        private string currentButtonText;
        private Color originalTextColor;

        

        // Dictionary to map button text to actions
        private Dictionary<string, System.Action> buttonActions;
        private Dictionary<string, System.Action<string[]>> parameterizedActions;
        private List<(Regex pattern, System.Action<Match> action)> regexActions;


        private void Awake()
        {
            // Get required components
            button = GetComponent<Button>();
            buttonText = GetComponentInChildren<TMP_Text>();

            if (buttonText == null)
            {
                Log.Error($"EnhancedAutoButtonHandler on {gameObject.name}: No TMP_Text component found in children!", Log.LogCategory.UI);
                enabled = false;
                return;
            }

            originalTextColor = buttonText.color;
            InitializeActions();
            SetupButtonListener();
        }

        private void Start()
        {
            UpdateButtonText();
        }

        private void InitializeActions()
        {
            buttonActions = new Dictionary<string, System.Action>();
            parameterizedActions = new Dictionary<string, System.Action<string[]>>();
            regexActions = new List<(Regex pattern, System.Action<Match> action)>();

            InitializeBasicActions();
            InitializeParameterizedActions();
            InitializeRegexActions();
        }
        private void InitializeBasicActions()
        {
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
            buttonActions["close"] = () => CloseUI(panelToClose);
            buttonActions["resume"] = () => ResumeAndClose(panelToClose);

            // Game State Actions
            buttonActions["pause"] = PauseGame;
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
            buttonActions["debug"] = ToggleDebugConsole;
            buttonActions["console"] = ToggleDebugConsole;
            buttonActions["debug menu"] = ToggleDebugConsole;
            buttonActions["debug console"] = ToggleDebugConsole;

            // Toggle Menu Actions
            // TODO: Find a way to make this more efficient.
            buttonActions["inspector"] = () => ToggleMenu("Inspector");
            buttonActions["announcements"] = () => ToggleMenu("Announcements");
            buttonActions["commands"] = () => ToggleMenu("Commands");
        }

        private void InitializeParameterizedActions()
        {
            // Actions that can take parameters
            parameterizedActions["add"] = HandleAddResource;
            parameterizedActions["spawn"] = HandleSpawnCommand;
            parameterizedActions["set"] = HandleSetCommand;
            parameterizedActions["load scene"] = HandleLoadScene;
        }

        private void InitializeRegexActions()
        {
            if (!useRegexPatterns) return;

            // Regex patterns for complex actions
            regexActions.Add((
                new Regex(@"add (\d+) (\w+)", RegexOptions.IgnoreCase),
                match => AddResourceWithAmount(match.Groups[2].Value, int.Parse(match.Groups[1].Value))
            ));

            regexActions.Add((
                new Regex(@"spawn (\d+) (\w+)", RegexOptions.IgnoreCase),
                match => SpawnMultiple(match.Groups[2].Value, int.Parse(match.Groups[1].Value))
            ));

            regexActions.Add((
                new Regex(@"set (\w+) to (\d+)", RegexOptions.IgnoreCase),
                match => SetValue(match.Groups[1].Value, int.Parse(match.Groups[2].Value))
            ));
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
                    Log.Info($"EnhancedAutoButtonHandler on {gameObject.name}: Button text set to '{currentButtonText}'", Log.LogCategory.UI);
                }
            }
        }

        private void HandleButtonClick()
        {
            UpdateButtonText();

            if (string.IsNullOrEmpty(currentButtonText))
            {
                ShowFeedback(false);
                Log.Warning($"EnhancedAutoButtonHandler on {gameObject.name}: Button text is empty or null!", Log.LogCategory.UI);
                return;
            }

            bool actionExecuted = false;


            try
            {

                // Try basic actions
                if (buttonActions.TryGetValue(currentButtonText, out System.Action action))
                {
                    action.Invoke();
                    actionExecuted = true;
                }
                // Try parameterized actions
                else if (TryExecuteParameterizedAction())
                {
                    actionExecuted = true;
                }
                // Try regex actions
                else if (useRegexPatterns && TryExecuteRegexAction())
                {
                    actionExecuted = true;
                }

                if (actionExecuted)
                {
                    ShowFeedback(true);
                    if (enableLogging)
                    {
                        Log.Info($"EnhancedAutoButtonHandler: Successfully executed action for '{currentButtonText}'", Log.LogCategory.UI);
                    }
                }
                else
                {
                    ShowFeedback(false);
                    Log.Warning($"EnhancedAutoButtonHandler: No action found for button text '{currentButtonText}'", Log.LogCategory.UI);
                }
            
            }
            catch (System.Exception e)
            {
                ShowFeedback(false);
                Log.Error($"EnhancedAutoButtonHandler: Error executing action for '{currentButtonText}': {e.Message}", Log.LogCategory.UI);
            }
        }

        private bool TryExecuteParameterizedAction()
        {
            string[] parts = currentButtonText.Split(' ');
            if (parts.Length > 1)
            {
                string command = parts[0];
                if (parameterizedActions.TryGetValue(command, out System.Action<string[]> paramAction))
                {
                    string[] parameters = new string[parts.Length - 1];
                    System.Array.Copy(parts, 1, parameters, 0, parameters.Length);
                    paramAction.Invoke(parameters);
                    return true;
                }
            }
            return false;
        }

        private bool TryExecuteRegexAction()
        {
            foreach (var (pattern, action) in regexActions)
            {
                Match match = pattern.Match(currentButtonText);
                if (match.Success)
                {
                    action.Invoke(match);
                    return true;
                }
            }
            return false;
        }

        private void ShowFeedback(bool success)
        {
            if (!enableVisualFeedback || buttonText == null) return;

            Color feedbackColor = success ? successColor : errorColor;
              // Start coroutine to show visual feedback
            StartCoroutine(ShowFeedbackCoroutine(feedbackColor));
        }

        private System.Collections.IEnumerator ShowFeedbackCoroutine(Color feedbackColor)
        {
            // Check if buttonText is still valid before changing color
            if (buttonText != null && this != null)
            {
                buttonText.color = feedbackColor;
            }
            
            yield return new WaitForSeconds(feedbackDuration);
            
            // Check again before restoring color, as the component might have been destroyed
            if (buttonText != null && this != null)
            {
                buttonText.color = originalTextColor;
            }
        }

        #region Parameterized Action Handlers

        private void HandleAddResource(string[] parameters)
        {
            if (parameters.Length >= 2)
            {
                if (int.TryParse(parameters[0], out int amount))
                {
                    string resourceType = parameters[1];
                    AddResourceWithAmount(resourceType, amount);
                }
                else
                {
                    // Default amount
                    AddResourceWithAmount(parameters[0], 10);
                }
            }
        }

        private void HandleSpawnCommand(string[] parameters)
        {
            if (parameters.Length >= 1)
            {
                if (parameters.Length >= 2 && int.TryParse(parameters[0], out int count))
                {
                    SpawnMultiple(parameters[1], count);
                }
                else
                {
                    SpawnMultiple(parameters[0], 1);
                }
            }
        }

        private void HandleSetCommand(string[] parameters)
        {
            if (parameters.Length >= 3 && parameters[1].ToLower() == "to")
            {
                if (int.TryParse(parameters[2], out int value))
                {
                    SetValue(parameters[0], value);
                }
            }
        }

        private void HandleLoadScene(string[] parameters)
        {
            if (parameters.Length >= 1)
            {
                if (int.TryParse(parameters[0], out int sceneIndex))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(parameters[0]);
                }
            }
        }

        #endregion

        #region Enhanced Action Implementations

        private void AddResourceWithAmount(string resourceType, int amount)
        {
            if (ResourceManager.Instance == null)
            {
                Log.Warning("ResourceManager instance not found!", Log.LogCategory.UI);
                return;
            }

            switch (resourceType.ToLower())
            {
                case "food":
                    ResourceManager.Instance.AddFood(amount);
                    break;
                case "wood":
                    ResourceManager.Instance.AddWood(amount);
                    break;
                case "faith":
                    ResourceManager.Instance.AddFaith(amount);
                    break;
                default:
                    Log.Warning($"Unknown resource type: {resourceType}", Log.LogCategory.UI);
                    break;
            }
        }

        private void SpawnMultiple(string entityType, int count)
        {
            switch (entityType.ToLower())
            {
                case "pop":
                case "population":
                    if (PopulationManager.Instance != null)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            PopulationManager.Instance.SpawnPop();
                        }
                    }
                    break;
                default:
                    Log.Warning($"Unknown entity type for spawning: {entityType}", Log.LogCategory.UI);
                    break;
            }
        }

        private void SetValue(string property, int value)
        {
            switch (property.ToLower())
            {
                case "timescale":
                    Time.timeScale = value;
                    break;
                case "volume":
                    AudioListener.volume = Mathf.Clamp01(value / 100f);
                    break;
                default:
                    Log.Warning($"Unknown property for setting: {property}", Log.LogCategory.UI);
                    break;
            }
        }        // Basic actions (simplified - include all from original AutoButtonHandler)
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
            SaveManager.Instance?.SaveGame();
        }

        private void LoadGame()
        {
            SaveManager.Instance?.LoadGame();
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

        private void ResumeAndClose(GameObject panel) {
            ResumeGame();
            CloseUI(panel);
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

        private void CloseUI(GameObject panel)
        {
            Canvas canvas = null;

            if (panel == null)
            {
                Log.Error("Panel to close not set!", Log.LogCategory.UI);
            }
            else if (panel != null)
                {
                    panel.SetActive(false);
                    Log.Info($"Closed UI panel: '{panel.name}'", Log.LogCategory.UI);
                    return;
                }
            
            // Fallback to original behavior: traverse up hierarchy to find a Canvas
            Transform current = transform;
            while (current != null && canvas == null)
            {
            canvas = current.GetComponent<Canvas>();
            if (canvas == null)
            {
                current = current.parent;
                Log.Info($"Current transform is: '{current?.name}'", Log.LogCategory.UI);
            }
            }

            if (canvas != null)
            {
            canvas.gameObject.SetActive(false);
            Log.Info($"Closed UI canvas: {canvas.name}", Log.LogCategory.UI);
            }
            else
            {
            // Fallback: try to deactivate the immediate parent if no Canvas is found
            if (transform.parent != null)
            {
                transform.parent.gameObject.SetActive(false);
                Log.Info($"Closed UI parent: {transform.parent.name}", Log.LogCategory.UI);
            }
            else
            {
                Log.Warning("CloseUI: No Canvas found in hierarchy and no parent to deactivate", Log.LogCategory.UI);
            }
            }
        }

        private void PauseGame()
        {
            Time.timeScale = 0f;
        }

        private void ResumeGame()
        {
            Time.timeScale = 1f;
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
            PopulationManager.Instance?.SpawnPop();
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
        public void ToggleDebugConsole()
        {
            Log.Info("Debug toggle action triggered", Log.LogCategory.UI);

            if (DebugConsoleManager.Instance != null)
            {
                DebugConsoleManager.Instance.ToggleConsoleVisibility();
            }
            else
            {
                Log.Warning("DebugConsoleManager.Instance is null - debug console not available", Log.LogCategory.UI);
            }
        }

        private void ToggleMenu(string menuName)
        {
            Log.Info($"{menuName.ToUpper()} MENU toggle action triggered - implement {menuName} UI logic", Log.LogCategory.UI);
            // Hide the Panel object inside of the specified menu GameObject
            var menu = GameObject.Find($"{menuName}_Menu");
            if (menu != null)
            {
                //Find the Panel child
                var panel = menu.transform.Find("Panel");
                if (panel != null)
                {
                    panel.gameObject.SetActive(!panel.gameObject.activeSelf);
                }
                else
                {
                    Debug.Log.Error($"Panel child not found in menu '{menuName}_Menu'. Check the hierarchy.", Log.LogCategory.UI);
                }
            }
            else
            {
                Debug.Log.Error($"Menu '{menuName}_Menu' not found in the scene. Check the spelling matches the object name exactly.", Log.LogCategory.UI);
            }
        }

        #endregion

        #region Public API

        public void AddCustomAction(string buttonText, System.Action action)
        {
            string processedText = ProcessButtonText(buttonText);
            buttonActions[processedText] = action;
        }

        public void AddParameterizedAction(string command, System.Action<string[]> action)
        {
            string processedCommand = ProcessButtonText(command);
            parameterizedActions[processedCommand] = action;
        }

        public void AddRegexAction(string pattern, System.Action<Match> action)
        {
            regexActions.Add((new Regex(pattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None), action));
        }        private string ProcessButtonText(string text)
        {
            if (trimWhitespace) text = text.Trim();
            if (ignoreCase) text = text.ToLower();
            return text;
        }

        #endregion

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleButtonClick);
            }
            
            // Stop all coroutines to prevent access to destroyed components
            StopAllCoroutines();
        }
    }
}

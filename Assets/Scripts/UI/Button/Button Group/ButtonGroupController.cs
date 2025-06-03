using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lineage.Ancestral.Legacies.UI
{
    /// <summary>
    /// Advanced controller that manages multiple ButtonGroupManagers and provides
    /// comprehensive control over entire button group ecosystems within a UI system.
    /// This is the master controller for all button group operations.
    /// </summary>
    public class ButtonGroupController : MonoBehaviour
    {
        [Header("Controller Settings")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool autoManageGroups = true;
        [SerializeField] private bool enableGlobalHotkeys = true;
        [SerializeField] private bool enableCrossPlatformSupport = true;

        [Header("Group Management")]
        [SerializeField] private List<ButtonGroupConfiguration> groupConfigurations = new List<ButtonGroupConfiguration>();
        [SerializeField] private Transform buttonGroupContainer;
        [SerializeField] private GameObject buttonGroupManagerPrefab;
        [SerializeField] private bool createGroupsAutomatically = true;

        [Header("Global Settings")]
        [SerializeField] private GlobalButtonSettings globalSettings = new GlobalButtonSettings();
        [SerializeField] private bool overrideIndividualSettings = false;
        [SerializeField] private bool maintainConsistentStyling = true;

        [Header("Advanced Features")]
        [SerializeField] private bool enableContextualGroups = true;
        [SerializeField] private bool enableDynamicGrouping = true;
        [SerializeField] private bool enableStateBasedVisibility = true;
        [SerializeField] private bool enableGroupTransitions = true;

        [Header("Performance & Optimization")]
        [SerializeField] private bool enableObjectPooling = true;
        [SerializeField] private int maxPooledButtons = 100;
        [SerializeField] private bool enableBatchOperations = true;
        [SerializeField] private bool enableLazyLoading = false;

        [Header("Events & Callbacks")]
        [SerializeField] private bool enableGlobalEvents = true;
        [SerializeField] private bool enableAnalytics = false;
        [SerializeField] private bool enableDebugEvents = true;

        // Private fields
        private List<ButtonGroupManager> managedGroups = new List<ButtonGroupManager>();
        private Dictionary<string, ButtonGroupManager> namedGroups = new Dictionary<string, ButtonGroupManager>();
        private Dictionary<GameState, List<string>> stateBasedGroups = new Dictionary<GameState, List<string>>();
        private Queue<GameObject> buttonPool = new Queue<GameObject>();
        private GameState currentGameState = GameState.MainMenu;

        // Events
        public System.Action<ButtonGroupManager> OnGroupCreated;
        public System.Action<ButtonGroupManager> OnGroupDestroyed;
        public System.Action<string> OnGlobalButtonClicked;
        public System.Action<GameState> OnGameStateChanged;
        public System.Action OnAllGroupsInitialized;

        #region Enums and Data Structures

        public enum GameState
        {
            MainMenu, InGame, Paused, Settings, Loading, 
            GameOver, Victory, Inventory, BuildMode, CombatMode
        }

        [System.Serializable]
        public class ButtonGroupConfiguration
        {
            [Header("Basic Configuration")]
            public string groupName = "Button Group";
            public string groupId = "";
            public bool isEnabled = true;
            public Vector3 position = Vector3.zero;
            public Vector3 rotation = Vector3.zero;
            public Vector3 scale = Vector3.one;

            [Header("Group Settings")]
            public ButtonGroupManager.ButtonGroupType groupType = ButtonGroupManager.ButtonGroupType.Horizontal;
            public int buttonCount = 3;            public List<ButtonGroupManager.ButtonData> buttons = new List<ButtonGroupManager.ButtonData>();
            public bool autoGenerateButtons = false;
            public ButtonGroupManager.CommandCategory[] commandCategories = { ButtonGroupManager.CommandCategory.GameManagement };

            [Header("Visibility & State")]
            public List<GameState> visibleInStates = new List<GameState> { GameState.MainMenu };
            public bool hideInOtherStates = true;
            public bool animateTransitions = true;

            [Header("Layout Override")]
            public bool overrideGlobalLayout = false;
            public Vector2 customButtonSize = new Vector2(120, 40);
            public float customSpacing = 10f;
            public ButtonGroupManager.ButtonAlignment customAlignment = ButtonGroupManager.ButtonAlignment.MiddleCenter;

            [Header("Visual Override")]
            public bool overrideGlobalVisuals = false;
            public ButtonGroupManager.ButtonVisualSettings customVisuals = new ButtonGroupManager.ButtonVisualSettings();

            [Header("Behavior Override")]
            public bool overrideGlobalBehavior = false;
            public bool customEnableInteraction = true;
            public bool customEnableKeyboardNav = true;
            public ButtonGroupManager.ButtonBehavior customDefaultBehavior = ButtonGroupManager.ButtonBehavior.Standard;
        }

        [System.Serializable]
        public class GlobalButtonSettings
        {
            [Header("Global Layout")]
            public Vector2 defaultButtonSize = new Vector2(120, 40);
            public float defaultSpacing = 10f;
            public ButtonGroupManager.ButtonAlignment defaultAlignment = ButtonGroupManager.ButtonAlignment.MiddleCenter;

            [Header("Global Visuals")]
            public ButtonGroupManager.ButtonVisualSettings globalVisuals = new ButtonGroupManager.ButtonVisualSettings();

            [Header("Global Behavior")]
            public bool globalEnableInteraction = true;
            public bool globalEnableKeyboardNav = true;
            public bool globalEnableAnimations = true;
            public bool globalEnableSounds = true;

            [Header("Auto-Handler Settings")]
            public bool globalEnableAutoHandler = true;
            public bool globalEnableRegex = false;
            public bool globalEnableVisualFeedback = true;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeController();
        }

        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeAllGroups();
            }
        }

        private void Update()
        {
            if (enableGlobalHotkeys)
            {
                HandleGlobalHotkeys();
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying && autoManageGroups)
            {
                ValidateAndUpdateGroups();
            }
        }

        #endregion

        #region Initialization

        private void InitializeController()
        {
            if (buttonGroupContainer == null)
            {
                buttonGroupContainer = transform;
            }

            InitializeObjectPool();
            InitializeStateBasedGroups();
            
            // Subscribe to global events if needed
            if (enableGlobalEvents)
            {
                SubscribeToGlobalEvents();
            }
        }

        private void InitializeObjectPool()
        {
            if (!enableObjectPooling) return;

            for (int i = 0; i < maxPooledButtons; i++)
            {
                var pooledButton = CreatePooledButton();
                if (pooledButton != null)
                {
                    buttonPool.Enqueue(pooledButton);
                }
            }
        }

        private GameObject CreatePooledButton()
        {
            // Create a basic button for pooling
            var buttonObj = new GameObject("PooledButton");
            buttonObj.AddComponent<RectTransform>();
            buttonObj.AddComponent<Image>();
            buttonObj.AddComponent<Button>();
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            textObj.AddComponent<RectTransform>();
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = "Button";
            textComponent.alignment = TextAlignmentOptions.Center;
            
            buttonObj.SetActive(false);
            buttonObj.transform.SetParent(buttonGroupContainer);
            
            return buttonObj;
        }

        private void InitializeStateBasedGroups()
        {
            stateBasedGroups.Clear();
            
            foreach (var config in groupConfigurations)
            {
                foreach (var state in config.visibleInStates)
                {
                    if (!stateBasedGroups.ContainsKey(state))
                    {
                        stateBasedGroups[state] = new List<string>();
                    }
                    stateBasedGroups[state].Add(config.groupId);
                }
            }
        }

        #endregion

        #region Group Management

        public void InitializeAllGroups()
        {
            if (createGroupsAutomatically)
            {
                CreateAllConfiguredGroups();
            }
            
            ApplyGlobalSettings();
            UpdateGroupVisibilityForCurrentState();
            
            OnAllGroupsInitialized?.Invoke();
        }

        private void CreateAllConfiguredGroups()
        {
            foreach (var config in groupConfigurations)
            {
                if (config.isEnabled)
                {
                    CreateButtonGroup(config);
                }
            }
        }

        public ButtonGroupManager CreateButtonGroup(ButtonGroupConfiguration config)
        {
            // Create container for the group
            var groupContainer = new GameObject(config.groupName);
            groupContainer.transform.SetParent(buttonGroupContainer);
            groupContainer.transform.localPosition = config.position;
            groupContainer.transform.localRotation = Quaternion.Euler(config.rotation);
            groupContainer.transform.localScale = config.scale;

            // Add RectTransform for UI positioning
            var rectTransform = groupContainer.AddComponent<RectTransform>();
            
            // Add ButtonGroupManager component
            var groupManager = groupContainer.AddComponent<ButtonGroupManager>();
            
            // Configure the group manager
            ConfigureGroupManager(groupManager, config);
            
            // Register the group
            managedGroups.Add(groupManager);
            if (!string.IsNullOrEmpty(config.groupId))
            {
                namedGroups[config.groupId] = groupManager;
            }
            
            // Create the buttons
            groupManager.CreateButtonGroup();
            
            OnGroupCreated?.Invoke(groupManager);
            
            return groupManager;
        }

        private void ConfigureGroupManager(ButtonGroupManager groupManager, ButtonGroupConfiguration config)
        {
            // Use reflection or direct assignment to configure the group manager
            // This would require making the ButtonGroupManager fields public or adding setters
            
            // For now, we'll configure through the inspector-accessible fields
            // In a real implementation, you'd add public setters to ButtonGroupManager
            
            var serializedObject = new UnityEditor.SerializedObject(groupManager);
            
            // Configure basic settings
            serializedObject.FindProperty("buttonCount").intValue = config.buttonCount;
            serializedObject.FindProperty("groupType").enumValueIndex = (int)config.groupType;
            
            // Configure layout settings if overriding
            if (config.overrideGlobalLayout)
            {
                serializedObject.FindProperty("buttonSize").vector2Value = config.customButtonSize;
                serializedObject.FindProperty("buttonSpacing").floatValue = config.customSpacing;
                serializedObject.FindProperty("alignment").enumValueIndex = (int)config.customAlignment;
            }
            else
            {
                // Apply global settings
                serializedObject.FindProperty("buttonSize").vector2Value = globalSettings.defaultButtonSize;
                serializedObject.FindProperty("buttonSpacing").floatValue = globalSettings.defaultSpacing;
                serializedObject.FindProperty("alignment").enumValueIndex = (int)globalSettings.defaultAlignment;
            }
            
            // Configure auto-generation settings
            serializedObject.FindProperty("autoGenerateFromCommands").boolValue = config.autoGenerateButtons;
            if (config.autoGenerateButtons && config.commandCategories.Length > 0)
            {
                var categoriesProperty = serializedObject.FindProperty("includeCategories");
                categoriesProperty.arraySize = config.commandCategories.Length;
                for (int i = 0; i < config.commandCategories.Length; i++)
                {
                    categoriesProperty.GetArrayElementAtIndex(i).enumValueIndex = (int)config.commandCategories[i];
                }
            }
            
            // Configure button configs if not auto-generating
            if (!config.autoGenerateButtons && config.buttons.Count > 0)
            {
                var buttonConfigsProperty = serializedObject.FindProperty("buttonConfigs");
                buttonConfigsProperty.arraySize = config.buttons.Count;
                
                for (int i = 0; i < config.buttons.Count; i++)
                {
                    var buttonConfigProperty = buttonConfigsProperty.GetArrayElementAtIndex(i);
                    var buttonData = config.buttons[i];
                    
                    buttonConfigProperty.FindPropertyRelative("buttonText").stringValue = buttonData.buttonText;
                    buttonConfigProperty.FindPropertyRelative("command").stringValue = buttonData.command;
                    buttonConfigProperty.FindPropertyRelative("isEnabled").boolValue = buttonData.isEnabled;
                    buttonConfigProperty.FindPropertyRelative("behavior").enumValueIndex = (int)buttonData.behavior;
                }
            }
            
            // Configure visual settings
            if (config.overrideGlobalVisuals)
            {
                ConfigureVisualSettings(serializedObject, config.customVisuals);
            }
            else if (overrideIndividualSettings)
            {
                ConfigureVisualSettings(serializedObject, globalSettings.globalVisuals);
            }
            
            // Configure behavior settings
            if (config.overrideGlobalBehavior)
            {
                serializedObject.FindProperty("enableButtonInteraction").boolValue = config.customEnableInteraction;
                serializedObject.FindProperty("enableKeyboardNavigation").boolValue = config.customEnableKeyboardNav;
                serializedObject.FindProperty("defaultBehavior").enumValueIndex = (int)config.customDefaultBehavior;
            }
            else if (overrideIndividualSettings)
            {
                serializedObject.FindProperty("enableButtonInteraction").boolValue = globalSettings.globalEnableInteraction;
                serializedObject.FindProperty("enableKeyboardNavigation").boolValue = globalSettings.globalEnableKeyboardNav;
            }
            
            // Configure auto-handler settings
            serializedObject.FindProperty("addAutoButtonHandler").boolValue = globalSettings.globalEnableAutoHandler;
            serializedObject.FindProperty("enableRegexPatterns").boolValue = globalSettings.globalEnableRegex;
            serializedObject.FindProperty("enableVisualFeedback").boolValue = globalSettings.globalEnableVisualFeedback;
            
            // Configure animation and sound settings
            serializedObject.FindProperty("enableAnimations").boolValue = globalSettings.globalEnableAnimations;
            serializedObject.FindProperty("enableSoundEffects").boolValue = globalSettings.globalEnableSounds;
            
            serializedObject.ApplyModifiedProperties();
        }

        private void ConfigureVisualSettings(UnityEditor.SerializedObject serializedObject, ButtonGroupManager.ButtonVisualSettings visualSettings)
        {
            var visualSettingsProperty = serializedObject.FindProperty("visualSettings");
            
            // Configure colors
            visualSettingsProperty.FindPropertyRelative("normalColor").colorValue = visualSettings.normalColor;
            visualSettingsProperty.FindPropertyRelative("highlightedColor").colorValue = visualSettings.highlightedColor;
            visualSettingsProperty.FindPropertyRelative("pressedColor").colorValue = visualSettings.pressedColor;
            visualSettingsProperty.FindPropertyRelative("selectedColor").colorValue = visualSettings.selectedColor;
            visualSettingsProperty.FindPropertyRelative("disabledColor").colorValue = visualSettings.disabledColor;
            
            // Configure text settings
            visualSettingsProperty.FindPropertyRelative("fontSize").intValue = visualSettings.fontSize;
            visualSettingsProperty.FindPropertyRelative("textColor").colorValue = visualSettings.textColor;
            visualSettingsProperty.FindPropertyRelative("fontStyle").enumValueIndex = (int)visualSettings.fontStyle;
            visualSettingsProperty.FindPropertyRelative("textAlignment").enumValueIndex = (int)visualSettings.textAlignment;
            
            // Configure background settings
            visualSettingsProperty.FindPropertyRelative("imageType").enumValueIndex = (int)visualSettings.imageType;
            visualSettingsProperty.FindPropertyRelative("useGradient").boolValue = visualSettings.useGradient;
            
            // Configure border settings
            visualSettingsProperty.FindPropertyRelative("showBorder").boolValue = visualSettings.showBorder;
            visualSettingsProperty.FindPropertyRelative("borderColor").colorValue = visualSettings.borderColor;
            visualSettingsProperty.FindPropertyRelative("borderWidth").floatValue = visualSettings.borderWidth;
        }

        #endregion

        #region State Management

        public void SetGameState(GameState newState)
        {
            if (currentGameState == newState) return;
            
            var previousState = currentGameState;
            currentGameState = newState;
            
            if (enableStateBasedVisibility)
            {
                UpdateGroupVisibilityForCurrentState();
            }
            
            OnGameStateChanged?.Invoke(newState);
        }

        private void UpdateGroupVisibilityForCurrentState()
        {
            foreach (var group in managedGroups)
            {
                var config = GetConfigurationForGroup(group);
                if (config != null)
                {
                    bool shouldBeVisible = config.visibleInStates.Contains(currentGameState);
                    
                    if (config.hideInOtherStates)
                    {
                        SetGroupVisibility(group, shouldBeVisible, config.animateTransitions);
                    }
                }
            }
        }

        private ButtonGroupConfiguration GetConfigurationForGroup(ButtonGroupManager group)
        {
            string groupName = group.gameObject.name;
            return groupConfigurations.FirstOrDefault(c => c.groupName == groupName);
        }

        private void SetGroupVisibility(ButtonGroupManager group, bool visible, bool animate = true)
        {
            if (animate && enableGroupTransitions)
            {
                // Implement smooth transitions
                var canvasGroup = group.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = group.gameObject.AddComponent<CanvasGroup>();
                }
                
                if (visible)
                {
                    group.gameObject.SetActive(true);
                    StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, 0.3f));
                }
                else
                {
                    StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f, 0.3f, () => {
                        group.gameObject.SetActive(false);
                    }));
                }
            }
            else
            {
                group.gameObject.SetActive(visible);
            }
        }

        private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration, System.Action onComplete = null)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            
            canvasGroup.alpha = to;
            onComplete?.Invoke();
        }

        #endregion

        #region Global Operations

        public void ApplyGlobalSettings()
        {
            if (!overrideIndividualSettings) return;
            
            foreach (var group in managedGroups)
            {
                ApplyGlobalSettingsToGroup(group);
            }
        }

        private void ApplyGlobalSettingsToGroup(ButtonGroupManager group)
        {
            // This would require public setters on ButtonGroupManager
            // For now, this is a placeholder for the concept
        }

        public void RefreshAllGroups()
        {
            foreach (var group in managedGroups)
            {
                group.UpdateButtonGroup();
            }
        }

        public void DestroyAllGroups()
        {
            var groupsToDestroy = new List<ButtonGroupManager>(managedGroups);
            
            foreach (var group in groupsToDestroy)
            {
                DestroyButtonGroup(group);
            }
        }

        public void DestroyButtonGroup(ButtonGroupManager group)
        {
            if (group == null) return;
            
            managedGroups.Remove(group);
            
            // Remove from named groups
            var kvpToRemove = namedGroups.FirstOrDefault(kvp => kvp.Value == group);
            if (!kvpToRemove.Equals(default(KeyValuePair<string, ButtonGroupManager>)))
            {
                namedGroups.Remove(kvpToRemove.Key);
            }
            
            OnGroupDestroyed?.Invoke(group);
            
            if (group.gameObject != null)
            {
                DestroyImmediate(group.gameObject);
            }
        }

        #endregion

        #region Access Methods

        public ButtonGroupManager GetGroupById(string groupId)
        {
            namedGroups.TryGetValue(groupId, out var group);
            return group;
        }

        public ButtonGroupManager GetGroupByName(string groupName)
        {
            return managedGroups.FirstOrDefault(g => g.gameObject.name == groupName);
        }

        public List<ButtonGroupManager> GetGroupsForState(GameState state)
        {
            var groups = new List<ButtonGroupManager>();
            
            if (stateBasedGroups.TryGetValue(state, out var groupIds))
            {
                foreach (var groupId in groupIds)
                {
                    var group = GetGroupById(groupId);
                    if (group != null)
                    {
                        groups.Add(group);
                    }
                }
            }
            
            return groups;
        }

        public List<ButtonGroupManager> GetAllGroups()
        {
            return new List<ButtonGroupManager>(managedGroups);
        }

        #endregion

        #region Hotkeys and Input

        private void HandleGlobalHotkeys()
        {
            // Handle global hotkeys for group operations
            if (Input.GetKeyDown(KeyCode.F1))
            {
                SetGameState(GameState.MainMenu);
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                SetGameState(GameState.InGame);
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                SetGameState(GameState.Settings);
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                RefreshAllGroups();
            }
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                ToggleAllGroupsVisibility();
            }
        }

        private void ToggleAllGroupsVisibility()
        {
            bool anyVisible = managedGroups.Any(g => g.gameObject.activeInHierarchy);
            
            foreach (var group in managedGroups)
            {
                group.gameObject.SetActive(!anyVisible);
            }
        }

        #endregion

        #region Validation and Debugging

        private void ValidateAndUpdateGroups()
        {
            // Remove null references
            managedGroups.RemoveAll(g => g == null);
            
            // Update named groups dictionary
            var keysToRemove = namedGroups.Where(kvp => kvp.Value == null).Select(kvp => kvp.Key).ToList();
            foreach (var key in keysToRemove)
            {
                namedGroups.Remove(key);
            }
            
            // Ensure configurations match actual groups
            SynchronizeConfigurationsWithGroups();
        }

        private void SynchronizeConfigurationsWithGroups()
        {
            // Add configurations for groups that don't have them
            foreach (var group in managedGroups)
            {
                var groupName = group.gameObject.name;
                var config = groupConfigurations.FirstOrDefault(c => c.groupName == groupName);
                
                if (config == null)
                {
                    // Create a default configuration
                    var newConfig = new ButtonGroupConfiguration
                    {
                        groupName = groupName,
                        groupId = System.Guid.NewGuid().ToString(),
                        isEnabled = true,
                        visibleInStates = new List<GameState> { currentGameState }
                    };
                    
                    groupConfigurations.Add(newConfig);
                }
            }
        }

        private void SubscribeToGlobalEvents()
        {
            // Subscribe to events from all managed groups
            foreach (var group in managedGroups)
            {
                // This would require the ButtonGroupManager to expose these events
                // group.OnButtonClicked += (buttonText) => OnGlobalButtonClicked?.Invoke(buttonText);
            }
        }

        #endregion

        #region Context Menu Methods (Editor Only)

        #if UNITY_EDITOR
        [UnityEditor.MenuItem("CONTEXT/ButtonGroupController/Create Default Groups")]
        private static void CreateDefaultGroups(UnityEditor.MenuCommand command)
        {
            var controller = command.context as ButtonGroupController;
            if (controller != null)
            {
                controller.CreateDefaultGroupConfigurations();
                controller.InitializeAllGroups();
            }
        }

        [UnityEditor.MenuItem("CONTEXT/ButtonGroupController/Refresh All Groups")]
        private static void RefreshGroups(UnityEditor.MenuCommand command)
        {
            var controller = command.context as ButtonGroupController;
            if (controller != null)
            {
                controller.RefreshAllGroups();
            }
        }

        [UnityEditor.MenuItem("CONTEXT/ButtonGroupController/Clear All Groups")]
        private static void ClearGroups(UnityEditor.MenuCommand command)
        {
            var controller = command.context as ButtonGroupController;
            if (controller != null)
            {
                controller.DestroyAllGroups();
            }
        }
        #endif

        private void CreateDefaultGroupConfigurations()
        {
            groupConfigurations.Clear();
            
            // Main Menu Group
            groupConfigurations.Add(new ButtonGroupConfiguration
            {
                groupName = "Main Menu Buttons",
                groupId = "main_menu",
                groupType = ButtonGroupManager.ButtonGroupType.Vertical,
                buttonCount = 4,
                visibleInStates = new List<GameState> { GameState.MainMenu },
                autoGenerateButtons = true,
                commandCategories = new[] { ButtonGroupManager.CommandCategory.GameManagement, ButtonGroupManager.CommandCategory.SaveLoad }
            });
            
            // In-Game HUD Group
            groupConfigurations.Add(new ButtonGroupConfiguration
            {
                groupName = "Game HUD Buttons",
                groupId = "game_hud",
                groupType = ButtonGroupManager.ButtonGroupType.Horizontal,
                buttonCount = 6,
                visibleInStates = new List<GameState> { GameState.InGame },
                autoGenerateButtons = true,
                commandCategories = new[] { ButtonGroupManager.CommandCategory.GameState, ButtonGroupManager.CommandCategory.Resources, ButtonGroupManager.CommandCategory.Population }
            });
            
            // Settings Group
            groupConfigurations.Add(new ButtonGroupConfiguration
            {
                groupName = "Settings Buttons",
                groupId = "settings",
                groupType = ButtonGroupManager.ButtonGroupType.Grid,
                buttonCount = 8,
                visibleInStates = new List<GameState> { GameState.Settings },
                autoGenerateButtons = true,
                commandCategories = new[] { ButtonGroupManager.CommandCategory.Settings, ButtonGroupManager.CommandCategory.Audio, ButtonGroupManager.CommandCategory.Navigation }
            });
        }

        #endregion
    }
}

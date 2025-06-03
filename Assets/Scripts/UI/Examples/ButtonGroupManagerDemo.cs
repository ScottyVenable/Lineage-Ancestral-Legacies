using UnityEngine;
using UnityEngine.UI;
using Lineage.Ancestral.Legacies.UI;

namespace Lineage.Ancestral.Legacies.UI.Examples
{
    /// <summary>
    /// Comprehensive demonstration of ButtonGroupManager capabilities.
    /// Shows various configurations and use cases for button group management.
    /// </summary>
    public class ButtonGroupManagerDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool createDemoGroupsOnStart = true;
        [SerializeField] private bool enableRuntimeTesting = true;
        [SerializeField] private Transform demoParent;

        [Header("Button Group Prefabs")]
        [SerializeField] private GameObject standardButtonPrefab;
        [SerializeField] private GameObject customButtonPrefab;

        // Demo button groups
        private ButtonGroupManager gameControlGroup;
        private ButtonGroupManager saveLoadGroup;
        private ButtonGroupManager settingsGroup;
        private ButtonGroupManager radioButtonGroup;
        private ButtonGroupManager verticalMenuGroup;

        private void Start()
        {
            if (demoParent == null)
                demoParent = transform;

            if (createDemoGroupsOnStart)
            {
                CreateDemoButtonGroups();
            }
        }

        private void Update()
        {
            if (enableRuntimeTesting)
            {
                HandleDemoInputs();
            }
        }

        #region Demo Group Creation

        private void CreateDemoButtonGroups()
        {
            CreateGameControlGroup();
            CreateSaveLoadGroup();
            CreateSettingsGroup();
            CreateRadioButtonGroup();
            CreateVerticalMenuGroup();
        }

        private void CreateGameControlGroup()
        {
            GameObject groupObj = new GameObject("Game_Control_Group");
            groupObj.transform.SetParent(demoParent, false);

            // Position the group
            RectTransform rect = groupObj.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 200);

            gameControlGroup = groupObj.AddComponent<ButtonGroupManager>();
            
            // Configure basic settings
            var settings = new SerializedProperty_Helper<ButtonGroupManager>(gameControlGroup);
            settings.SetValue("buttonCount", 4);
            settings.SetValue("groupType", ButtonGroupManager.ButtonGroupType.Horizontal);
            settings.SetValue("buttonSize", new Vector2(100, 40));
            settings.SetValue("buttonSpacing", 15f);
            settings.SetValue("autoGenerateFromCommands", true);

            // Set command categories
            var categories = new ButtonGroupManager.CommandCategory[] 
            { 
                ButtonGroupManager.CommandCategory.GameManagement,
                ButtonGroupManager.CommandCategory.GameState 
            };
            settings.SetValue("includeCategories", categories);

            // Configure visual settings
            var visualSettings = new ButtonGroupManager.ButtonVisualSettings();
            visualSettings.normalColor = new Color(0.8f, 0.9f, 1f);
            visualSettings.highlightedColor = new Color(0.9f, 0.95f, 1f);
            visualSettings.pressedColor = new Color(0.7f, 0.8f, 0.9f);
            visualSettings.fontSize = 12;
            visualSettings.textColor = Color.black;
            settings.SetValue("visualSettings", visualSettings);

            // Enable animations and sound
            settings.SetValue("enableAnimations", true);
            settings.SetValue("enableSoundEffects", false); // No audio clips assigned

            UnityEngine.Debug.Log("Created Game Control Group with auto-generated commands");
        }

        private void CreateSaveLoadGroup()
        {
            GameObject groupObj = new GameObject("Save_Load_Group");
            groupObj.transform.SetParent(demoParent, false);

            // Position the group
            RectTransform rect = groupObj.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 150);

            saveLoadGroup = groupObj.AddComponent<ButtonGroupManager>();

            // Manual configuration with custom buttons
            var buttonConfigs = new System.Collections.Generic.List<ButtonGroupManager.ButtonData>
            {
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Quick Save",
                    command = "quick save",
                    isEnabled = true,
                    customColor = new Color(0.8f, 1f, 0.8f)
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Quick Load",
                    command = "quick load",
                    isEnabled = true,
                    customColor = new Color(1f, 0.9f, 0.8f)
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Save As...",
                    command = "save",
                    isEnabled = true,
                    customColor = new Color(0.9f, 0.9f, 1f)
                }
            };

            var settings = new SerializedProperty_Helper<ButtonGroupManager>(saveLoadGroup);
            settings.SetValue("buttonConfigs", buttonConfigs);
            settings.SetValue("buttonCount", 3);
            settings.SetValue("groupType", ButtonGroupManager.ButtonGroupType.Horizontal);
            settings.SetValue("autoGenerateFromCommands", false);
            settings.SetValue("addAutoButtonHandler", true);
            settings.SetValue("enableVisualFeedback", true);

            UnityEngine.Debug.Log("Created Save/Load Group with custom button configurations");
        }

        private void CreateSettingsGroup()
        {
            GameObject groupObj = new GameObject("Settings_Group");
            groupObj.transform.SetParent(demoParent, false);

            // Position the group
            RectTransform rect = groupObj.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 100);

            settingsGroup = groupObj.AddComponent<ButtonGroupManager>();

            var buttonConfigs = new System.Collections.Generic.List<ButtonGroupManager.ButtonData>
            {
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Settings",
                    command = "settings",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.Standard
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Apply",
                    command = "apply",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.Standard,
                    customColor = new Color(0.8f, 1f, 0.8f)
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Reset",
                    command = "reset",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.Standard,
                    customColor = new Color(1f, 0.8f, 0.8f)
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Cancel",
                    command = "back",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.Standard
                }
            };

            var settings = new SerializedProperty_Helper<ButtonGroupManager>(settingsGroup);
            settings.SetValue("buttonConfigs", buttonConfigs);
            settings.SetValue("buttonCount", 4);
            settings.SetValue("groupType", ButtonGroupManager.ButtonGroupType.Horizontal);
            settings.SetValue("enableKeyboardNavigation", true);
            settings.SetValue("autoSize", false);
            settings.SetValue("buttonSize", new Vector2(80, 35));

            UnityEngine.Debug.Log("Created Settings Group with navigation and custom behaviors");
        }

        private void CreateRadioButtonGroup()
        {
            GameObject groupObj = new GameObject("Radio_Button_Group");
            groupObj.transform.SetParent(demoParent, false);

            // Position the group
            RectTransform rect = groupObj.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-200, 50);

            radioButtonGroup = groupObj.AddComponent<ButtonGroupManager>();

            var buttonConfigs = new System.Collections.Generic.List<ButtonGroupManager.ButtonData>
            {
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Easy",
                    command = "difficulty easy",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.RadioButton,
                    isToggled = true // Default selection
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Normal",
                    command = "difficulty normal",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.RadioButton
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Hard",
                    command = "difficulty hard",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.RadioButton
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Expert",
                    command = "difficulty expert",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.RadioButton
                }
            };

            var settings = new SerializedProperty_Helper<ButtonGroupManager>(radioButtonGroup);
            settings.SetValue("buttonConfigs", buttonConfigs);
            settings.SetValue("buttonCount", 4);
            settings.SetValue("groupType", ButtonGroupManager.ButtonGroupType.Vertical);
            settings.SetValue("enableButtonGroups", true);
            settings.SetValue("allowMultipleSelection", false);
            settings.SetValue("buttonSpacing", 8f);

            var visualSettings = new ButtonGroupManager.ButtonVisualSettings();
            visualSettings.selectedColor = new Color(0.6f, 0.8f, 1f);
            visualSettings.normalColor = new Color(0.9f, 0.9f, 0.9f);
            settings.SetValue("visualSettings", visualSettings);

            UnityEngine.Debug.Log("Created Radio Button Group for difficulty selection");
        }

        private void CreateVerticalMenuGroup()
        {
            GameObject groupObj = new GameObject("Vertical_Menu_Group");
            groupObj.transform.SetParent(demoParent, false);

            // Position the group
            RectTransform rect = groupObj.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(200, 50);

            verticalMenuGroup = groupObj.AddComponent<ButtonGroupManager>();

            var buttonConfigs = new System.Collections.Generic.List<ButtonGroupManager.ButtonData>
            {
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Population",
                    command = "show population",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.Toggle
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Resources",
                    command = "show resources",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.Toggle
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Buildings",
                    command = "show buildings",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.Toggle
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Research",
                    command = "show research",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.Toggle
                },
                new ButtonGroupManager.ButtonData
                {
                    buttonText = "Help",
                    command = "show help",
                    isEnabled = true,
                    behavior = ButtonGroupManager.ButtonBehavior.Standard
                }
            };

            var settings = new SerializedProperty_Helper<ButtonGroupManager>(verticalMenuGroup);
            settings.SetValue("buttonConfigs", buttonConfigs);
            settings.SetValue("buttonCount", 5);
            settings.SetValue("groupType", ButtonGroupManager.ButtonGroupType.Vertical);
            settings.SetValue("allowMultipleSelection", true);
            settings.SetValue("buttonSize", new Vector2(120, 30));
            settings.SetValue("enableAnimations", true);
            
            var animSettings = new ButtonGroupManager.AnimationSettings();
            animSettings.enableHoverAnimation = true;
            animSettings.hoverScale = new Vector3(1.1f, 1.1f, 1f);
            animSettings.animationDuration = 0.15f;
            settings.SetValue("animationSettings", animSettings);

            UnityEngine.Debug.Log("Created Vertical Menu Group with toggle behaviors and animations");
        }

        #endregion

        #region Runtime Demo Controls

        private void HandleDemoInputs()
        {
            // Demo controls with keyboard
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestDynamicButtonAddition();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestButtonTextChange();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestButtonEnableDisable();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestGroupRefresh();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                TestButtonRemoval();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                RecreateAllGroups();
            }
        }

        private void TestDynamicButtonAddition()
        {
            if (gameControlGroup != null)
            {
                var newButton = new ButtonGroupManager.ButtonData
                {
                    buttonText = $"Dynamic {Random.Range(1, 100)}",
                    command = "dynamic command",
                    isEnabled = true,
                    customColor = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f))
                };

                gameControlGroup.AddButton(newButton);
                UnityEngine.Debug.Log("Added dynamic button to Game Control Group");
            }
        }

        private void TestButtonTextChange()
        {
            if (saveLoadGroup != null)
            {
                var buttons = saveLoadGroup.GetManagedButtons();
                if (buttons.Count > 0)
                {
                    int randomIndex = Random.Range(0, buttons.Count);
                    string newText = $"Changed {Random.Range(1, 100)}";
                    saveLoadGroup.SetButtonText(randomIndex, newText);
                    UnityEngine.Debug.Log($"Changed button {randomIndex} text to: {newText}");
                }
            }
        }

        private void TestButtonEnableDisable()
        {
            if (settingsGroup != null)
            {
                var buttons = settingsGroup.GetManagedButtons();
                if (buttons.Count > 1)
                {
                    int randomIndex = Random.Range(0, buttons.Count);
                    bool currentState = buttons[randomIndex].GetComponent<Button>().interactable;
                    settingsGroup.SetButtonEnabled(randomIndex, !currentState);
                    UnityEngine.Debug.Log($"Toggled button {randomIndex} enabled state to: {!currentState}");
                }
            }
        }

        private void TestGroupRefresh()
        {
            if (verticalMenuGroup != null)
            {
                verticalMenuGroup.RefreshButtonGroup();
                UnityEngine.Debug.Log("Refreshed Vertical Menu Group");
            }
        }

        private void TestButtonRemoval()
        {
            if (gameControlGroup != null)
            {
                var buttons = gameControlGroup.GetManagedButtons();
                if (buttons.Count > 2) // Keep at least 2 buttons
                {
                    int lastIndex = buttons.Count - 1;
                    gameControlGroup.RemoveButton(lastIndex);
                    UnityEngine.Debug.Log($"Removed button at index {lastIndex}");
                }
            }
        }

        private void RecreateAllGroups()
        {
            // Clear existing groups
            DestroyExistingGroups();
            
            // Recreate them
            CreateDemoButtonGroups();
            UnityEngine.Debug.Log("Recreated all demo button groups");
        }

        private void DestroyExistingGroups()
        {
            if (gameControlGroup != null) DestroyImmediate(gameControlGroup.gameObject);
            if (saveLoadGroup != null) DestroyImmediate(saveLoadGroup.gameObject);
            if (settingsGroup != null) DestroyImmediate(settingsGroup.gameObject);
            if (radioButtonGroup != null) DestroyImmediate(radioButtonGroup.gameObject);
            if (verticalMenuGroup != null) DestroyImmediate(verticalMenuGroup.gameObject);
        }

        #endregion

        #region Event Handlers

        private void OnEnable()
        {
            // Subscribe to button group events if groups exist
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (gameControlGroup != null)
            {
                gameControlGroup.OnButtonClicked += OnGameControlButtonClicked;
                gameControlGroup.OnLayoutChanged += OnLayoutChanged;
            }

            if (saveLoadGroup != null)
            {
                saveLoadGroup.OnButtonClicked += OnSaveLoadButtonClicked;
            }

            if (radioButtonGroup != null)
            {
                radioButtonGroup.OnButtonClicked += OnRadioButtonClicked;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (gameControlGroup != null)
            {
                gameControlGroup.OnButtonClicked -= OnGameControlButtonClicked;
                gameControlGroup.OnLayoutChanged -= OnLayoutChanged;
            }

            if (saveLoadGroup != null)
            {
                saveLoadGroup.OnButtonClicked -= OnSaveLoadButtonClicked;
            }

            if (radioButtonGroup != null)
            {
                radioButtonGroup.OnButtonClicked -= OnRadioButtonClicked;
            }
        }

        private void OnGameControlButtonClicked(string buttonText)
        {
            UnityEngine.Debug.Log($"Game Control Button Clicked: {buttonText}");
        }

        private void OnSaveLoadButtonClicked(string buttonText)
        {
            UnityEngine.Debug.Log($"Save/Load Button Clicked: {buttonText}");
        }

        private void OnRadioButtonClicked(string buttonText)
        {
            UnityEngine.Debug.Log($"Radio Button Selected: {buttonText}");
        }

        private void OnLayoutChanged()
        {
            UnityEngine.Debug.Log("Button group layout changed");
        }

        #endregion

        #region Utility Class for Setting SerializedProperties

        /// <summary>
        /// Helper class to set values on MonoBehaviour components during runtime.
        /// This is a simplified version - in production you'd use reflection or proper serialization.
        /// </summary>
        private class SerializedProperty_Helper<T> where T : MonoBehaviour
        {
            private T target;

            public SerializedProperty_Helper(T target)
            {
                this.target = target;
            }

            public void SetValue(string fieldName, object value)
            {
                var field = typeof(T).GetField(fieldName, 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);
                
                if (field != null)
                {
                    field.SetValue(target, value);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Field '{fieldName}' not found on {typeof(T).Name}");
                }
            }
        }

        #endregion

        private void OnGUI()
        {
            if (!enableRuntimeTesting) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("ButtonGroupManager Demo Controls:", GUI.skin.box);
            GUILayout.Label("1 - Add Dynamic Button");
            GUILayout.Label("2 - Change Button Text");
            GUILayout.Label("3 - Toggle Button Enable/Disable");
            GUILayout.Label("4 - Refresh Group");
            GUILayout.Label("5 - Remove Last Button");
            GUILayout.Label("R - Recreate All Groups");
            GUILayout.EndArea();
        }
    }
}

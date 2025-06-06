using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lineage.UI.Examples
{
    /// <summary>
    /// Comprehensive example showing all available commands for the EnhancedAutoButtonHandler.
    /// This script demonstrates how to create buttons with various auto-detected functionalities.
    /// </summary>
    public class AutoButtonCommandsDemo : MonoBehaviour
    {
        [Header("Button Creation")]
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private int buttonsPerRow = 4;
        [SerializeField] private float buttonSpacing = 10f;

        [Header("Demo Categories")]
        [SerializeField] private bool showGameManagement = true;
        [SerializeField] private bool showSaveLoad = true;
        [SerializeField] private bool showNavigation = true;
        [SerializeField] private bool showGameState = true;
        [SerializeField] private bool showSettings = true;
        [SerializeField] private bool showAudio = true;
        [SerializeField] private bool showPopulation = true;
        [SerializeField] private bool showResources = true;
        [SerializeField] private bool showDebug = true;
        [SerializeField] private bool showParameterized = true;

        private void Start()
        {
            CreateAllDemoButtons();
        }

        private void CreateAllDemoButtons()
        {
            if (buttonPrefab == null || buttonContainer == null)
            {
                UnityEngine.Debug.LogError("Button prefab or container not assigned!");
                return;
            }

            // Create header
            CreateSectionHeader("AutoButtonHandler Commands Demo");

            if (showGameManagement)
            {
                CreateSectionHeader("Game Management");
                CreateDemoButton("Quit", "Exits the application");
                CreateDemoButton("Exit", "Alternative quit command");
                CreateDemoButton("Quit Game", "Full quit command");
                CreateDemoButton("Exit Game", "Alternative full quit command");
            }

            if (showSaveLoad)
            {
                CreateSectionHeader("Save/Load System");
                CreateDemoButton("Save", "Saves current game");
                CreateDemoButton("Save Game", "Full save command");
                CreateDemoButton("Load", "Loads saved game");
                CreateDemoButton("Load Game", "Full load command");
                CreateDemoButton("Quick Save", "Quick save functionality");
                CreateDemoButton("Quick Load", "Quick load functionality");
                CreateDemoButton("Auto Save", "Trigger auto save");
            }

            if (showNavigation)
            {
                CreateSectionHeader("Menu Navigation");
                CreateDemoButton("Main Menu", "Return to main menu");
                CreateDemoButton("Menu", "Alternative menu command");
                CreateDemoButton("Back", "Go back/close UI");
                CreateDemoButton("Return", "Alternative back command");
                CreateDemoButton("Cancel", "Cancel/close UI");
                CreateDemoButton("Close", "Close current UI");
            }

            if (showGameState)
            {
                CreateSectionHeader("Game State Control");
                CreateDemoButton("Pause", "Pauses the game");
                CreateDemoButton("Resume", "Resumes the game");
                CreateDemoButton("Unpause", "Alternative resume command");
                CreateDemoButton("Restart", "Restart current scene");
                CreateDemoButton("New Game", "Start new game");
                CreateDemoButton("Start Game", "Alternative start command");
            }

            if (showSettings)
            {
                CreateSectionHeader("Settings Management");
                CreateDemoButton("Settings", "Open settings menu");
                CreateDemoButton("Options", "Alternative settings command");
                CreateDemoButton("Apply", "Apply current settings");
                CreateDemoButton("Reset", "Reset to defaults");
                CreateDemoButton("Defaults", "Alternative reset command");
            }

            if (showAudio)
            {
                CreateSectionHeader("Audio Controls");
                CreateDemoButton("Mute", "Mute/unmute audio");
                CreateDemoButton("Unmute", "Alternative mute toggle");
            }

            if (showPopulation)
            {
                CreateSectionHeader("Population Management");
                CreateDemoButton("Spawn Pop", "Spawn new population");
                CreateDemoButton("Add Pop", "Alternative spawn command");
                CreateDemoButton("Create Pop", "Another spawn variant");
            }

            if (showResources)
            {
                CreateSectionHeader("Resource Management");
                CreateDemoButton("Add Food", "Add 10 food resources");
                CreateDemoButton("Add Wood", "Add 10 wood resources");
                CreateDemoButton("Add Faith", "Add 10 faith resources");
            }

            if (showDebug)
            {
                CreateSectionHeader("Debug Functions");
                CreateDemoButton("Debug", "Toggle debug mode");
                CreateDemoButton("Console", "Alternative debug command");
                CreateDemoButton("Debug Menu", "Open debug menu");
            }

            if (showParameterized)
            {
                CreateSectionHeader("Parameterized Commands (Enhanced Handler)");
                CreateDemoButton("Add 50 Food", "Add specific amount of food");
                CreateDemoButton("Add 100 Wood", "Add specific amount of wood");
                CreateDemoButton("Add 25 Faith", "Add specific amount of faith");
                CreateDemoButton("Spawn 5 Pop", "Spawn multiple population");
                CreateDemoButton("Spawn 10 Population", "Spawn many population");
                CreateDemoButton("Set Volume to 80", "Set audio volume to 80%");
                CreateDemoButton("Set Timescale to 2", "Set game speed to 2x");
                CreateDemoButton("Load Scene 1", "Load scene by index");
                CreateDemoButton("Load Scene MainGame", "Load scene by name");
            }

            CreateSectionHeader($"Total Commands: {GetTotalCommandCount()}");
        }

        private void CreateSectionHeader(string headerText)
        {
            // Create a text-only GameObject for section headers
            GameObject headerObj = new GameObject($"Header_{headerText.Replace(" ", "_")}");
            headerObj.transform.SetParent(buttonContainer, false);

            // Add text component
            TMP_Text headerTextComp = headerObj.AddComponent<TextMeshProUGUI>();
            headerTextComp.text = headerText;
            headerTextComp.fontSize = 16f;
            headerTextComp.fontStyle = FontStyles.Bold;
            headerTextComp.color = Color.yellow;
            headerTextComp.alignment = TextAlignmentOptions.Center;

            // Add layout element to control spacing
            var layoutElement = headerObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 30f;
            layoutElement.preferredWidth = 200f;
        }

        private void CreateDemoButton(string buttonText, string description = "")
        {
            // Instantiate button
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);
            newButton.name = $"AutoButton_{buttonText.Replace(" ", "_")}";

            // Set button text
            TMP_Text textComponent = newButton.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = buttonText;
                textComponent.fontSize = 12f; // Smaller font for demo
            }

            // Add EnhancedAutoButtonHandler if not present
            if (newButton.GetComponent<EnhancedAutoButtonHandler>() == null)
            {
                var handler = newButton.AddComponent<EnhancedAutoButtonHandler>();
                // Enable regex patterns for parameterized commands
                var handlerType = typeof(EnhancedAutoButtonHandler);
                var useRegexField = handlerType.GetField("useRegexPatterns", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                useRegexField?.SetValue(handler, true);
            }

            // Add tooltip if description provided
            if (!string.IsNullOrEmpty(description))
            {
                // You can add a tooltip component here if you have one
                // For now, just log the description
                UnityEngine.Debug.Log($"Button '{buttonText}': {description}");
            }

            // Adjust button size for demo
            var rectTransform = newButton.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(150f, 30f);
            }
        }

        private int GetTotalCommandCount()
        {
            int count = 0;
            if (showGameManagement) count += 4;
            if (showSaveLoad) count += 7;
            if (showNavigation) count += 6;
            if (showGameState) count += 6;
            if (showSettings) count += 5;
            if (showAudio) count += 2;
            if (showPopulation) count += 3;
            if (showResources) count += 3;
            if (showDebug) count += 3;
            if (showParameterized) count += 9;
            return count;
        }

        [Header("Runtime Testing")]
        [SerializeField] private bool enableRuntimeTesting = false;

        private void Update()
        {
            if (!enableRuntimeTesting) return;

            // Test keyboard shortcuts for common commands
            if (Input.GetKeyDown(KeyCode.F5))
            {
                TestCommand("Quick Save");
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                TestCommand("Quick Load");
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TestCommand("Pause");
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TestCommand("Resume");
            }
        }

        private void TestCommand(string command)
        {
            UnityEngine.Debug.Log($"Testing command: {command}");
            // Find a button with this text and simulate click
            var buttons = FindObjectsByType<EnhancedAutoButtonHandler>(FindObjectsSortMode.None);
            foreach (var handler in buttons)
            {
                var textComp = handler.GetComponentInChildren<TMP_Text>();
                if (textComp != null && textComp.text.Equals(command, System.StringComparison.OrdinalIgnoreCase))
                {
                    var button = handler.GetComponent<Button>();
                    button?.onClick?.Invoke();
                    break;
                }
            }
        }

        #if UNITY_EDITOR
        [Header("Editor Tools")]
        [SerializeField] private bool recreateButtons = false;

        private void OnValidate()
        {
            if (recreateButtons && Application.isPlaying)
            {
                recreateButtons = false;
                ClearExistingButtons();
                CreateAllDemoButtons();
            }
        }

        private void ClearExistingButtons()
        {
            if (buttonContainer == null) return;

            for (int i = buttonContainer.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                {
                    Destroy(buttonContainer.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(buttonContainer.GetChild(i).gameObject);
                }
            }
        }
        #endif
    }
}

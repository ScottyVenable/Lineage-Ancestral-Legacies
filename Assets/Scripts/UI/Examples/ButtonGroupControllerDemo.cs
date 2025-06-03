using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Lineage.Ancestral.Legacies.Debug;
using System.Linq;

namespace Lineage.Ancestral.Legacies.UI
{
    /// <summary>
    /// Comprehensive demonstration of the ButtonGroupController capabilities.
    /// Shows how to manage multiple button groups with full centralized control.
    /// </summary>
    public class ButtonGroupControllerDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool runDemoOnStart = true;
        [SerializeField] private bool enableKeyboardShortcuts = true;
        [SerializeField] private bool showDemoUI = true;
        [SerializeField] private Canvas demoCanvas;

        [Header("Demo Scenarios")]
        [SerializeField] private bool demoBasicGroupCreation = true;
        [SerializeField] private bool demoStateBasedVisibility = true;
        [SerializeField] private bool demoGlobalStyling = true;
        [SerializeField] private bool demoDynamicGrouping = true;
        [SerializeField] private bool demoAdvancedFeatures = true;

        [Header("Demo UI Elements")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button nextDemoButton;
        [SerializeField] private Button prevDemoButton;
        [SerializeField] private Slider progressSlider;

        private ButtonGroupController controller;
        private int currentDemoStep = 0;
        private int totalDemoSteps = 12;
        private List<System.Action> demoSteps;

        private void Start()
        {
            SetupController();
            SetupDemoUI();
            InitializeDemoSteps();
            
            if (runDemoOnStart)
            {
                StartDemo();
            }
        }

        private void Update()
        {
            if (enableKeyboardShortcuts)
            {
                HandleKeyboardShortcuts();
            }
        }

        #region Setup

        private void SetupController()
        {
            // Create ButtonGroupController if it doesn't exist
            controller = FindFirstObjectByType<ButtonGroupController>();
            if (controller == null)
            {
                var controllerObj = new GameObject("Button Group Controller");
                controller = controllerObj.AddComponent<ButtonGroupController>();
                DontDestroyOnLoad(controllerObj);
            }

            // Subscribe to controller events
            controller.OnGroupCreated += OnGroupCreated;
            controller.OnGroupDestroyed += OnGroupDestroyed;
            controller.OnGlobalButtonClicked += OnGlobalButtonClicked;
            controller.OnGameStateChanged += OnGameStateChanged;
            controller.OnAllGroupsInitialized += OnAllGroupsInitialized;
        }

        private void SetupDemoUI()
        {
            if (!showDemoUI) return;

            // Create demo canvas if not assigned
            if (demoCanvas == null)
            {
                var canvasObj = new GameObject("Demo Canvas");
                demoCanvas = canvasObj.AddComponent<Canvas>();
                demoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                demoCanvas.sortingOrder = 100;
                
                var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            CreateDemoUI();
        }

        private void CreateDemoUI()
        {
            // Create demo control panel
            var panelObj = new GameObject("Demo Control Panel");
            panelObj.transform.SetParent(demoCanvas.transform);
            
            var rectTransform = panelObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.offsetMin = new Vector2(20, 20);
            rectTransform.offsetMax = new Vector2(-20, 120);
            
            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            // Create status text
            CreateStatusText(panelObj);
            
            // Create control buttons
            CreateControlButtons(panelObj);
            
            // Create progress slider
            CreateProgressSlider(panelObj);
        }

        private void CreateStatusText(GameObject parent)
        {
            var textObj = new GameObject("Status Text");
            textObj.transform.SetParent(parent.transform);
            
            var rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(20, 0);
            rectTransform.offsetMax = new Vector2(-20, -10);
            
            statusText = textObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "ButtonGroupController Demo Ready";
            statusText.fontSize = 18;
            statusText.color = Color.white;
            statusText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateControlButtons(GameObject parent)
        {
            // Previous button
            prevDemoButton = CreateDemoButton(parent, "Previous", new Vector2(20, 10), new Vector2(120, 40), PreviousDemo);
            
            // Next button
            nextDemoButton = CreateDemoButton(parent, "Next", new Vector2(150, 10), new Vector2(120, 40), NextDemo);
            
            // Reset button
            CreateDemoButton(parent, "Reset", new Vector2(280, 10), new Vector2(120, 40), ResetDemo);
            
            // Auto Play button
            CreateDemoButton(parent, "Auto Play", new Vector2(410, 10), new Vector2(120, 40), StartAutoDemo);
        }

        private Button CreateDemoButton(GameObject parent, string text, Vector2 position, Vector2 size, System.Action onClick)
        {
            var buttonObj = new GameObject($"Demo Button - {text}");
            buttonObj.transform.SetParent(parent.transform);
            
            var rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            
            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.4f, 0.8f);
            
            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => onClick?.Invoke());
            
            // Add text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 14;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            return button;
        }

        private void CreateProgressSlider(GameObject parent)
        {
            var sliderObj = new GameObject("Progress Slider");
            sliderObj.transform.SetParent(parent.transform);
            
            var rectTransform = sliderObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.6f, 0);
            rectTransform.anchorMax = new Vector2(1, 0.4f);
            rectTransform.offsetMin = new Vector2(10, 10);
            rectTransform.offsetMax = new Vector2(-20, -10);
            
            progressSlider = sliderObj.AddComponent<Slider>();
            progressSlider.minValue = 0;
            progressSlider.maxValue = totalDemoSteps - 1;
            progressSlider.wholeNumbers = true;
            progressSlider.value = 0;
            
            // Create slider background
            var background = new GameObject("Background");
            background.transform.SetParent(sliderObj.transform);
            var bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);
            
            // Create slider fill area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            
            // Create slider fill
            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.8f, 0.4f);
            
            progressSlider.fillRect = fillRect;
        }

        #endregion

        #region Demo Steps

        private void InitializeDemoSteps()
        {
            demoSteps = new List<System.Action>
            {
                () => DemoStep1_Introduction(),
                () => DemoStep2_BasicGroupCreation(),
                () => DemoStep3_LayoutTypes(),
                () => DemoStep4_CommandCategories(),
                () => DemoStep5_GlobalStyling(),
                () => DemoStep6_StateBasedVisibility(),
                () => DemoStep7_DynamicGroupModification(),
                () => DemoStep8_AdvancedFeatures(),
                () => DemoStep9_EventSystem(),
                () => DemoStep10_KeyboardNavigation(),
                () => DemoStep11_PerformanceFeatures(),
                () => DemoStep12_Conclusion()
            };
        }

        private void DemoStep1_Introduction()
        {
            UpdateStatus("Step 1: Introduction to ButtonGroupController\n" +
                        "The ButtonGroupController provides centralized management of multiple button groups.\n" +
                        "It offers state-based visibility, global styling, and comprehensive control.");
        }

        private void DemoStep2_BasicGroupCreation()
        {
            UpdateStatus("Step 2: Basic Group Creation\n" +
                        "Creating a simple horizontal button group with auto-generated commands.");
            
            var config = new ButtonGroupController.ButtonGroupConfiguration
            {
                groupName = "Demo Basic Group",
                groupId = "demo_basic",
                groupType = ButtonGroupManager.ButtonGroupType.Horizontal,
                buttonCount = 4,
                autoGenerateButtons = true,
                commandCategories = new[] { ButtonGroupManager.CommandCategory.GameManagement },
                visibleInStates = new List<ButtonGroupController.GameState> { ButtonGroupController.GameState.MainMenu }
            };
            
            controller.CreateButtonGroup(config);
        }

        private void DemoStep3_LayoutTypes()
        {
            UpdateStatus("Step 3: Different Layout Types\n" +
                        "Demonstrating Horizontal, Vertical, and Grid layouts.");
            
            // Vertical layout
            var verticalConfig = new ButtonGroupController.ButtonGroupConfiguration
            {
                groupName = "Demo Vertical Group",
                groupId = "demo_vertical",
                groupType = ButtonGroupManager.ButtonGroupType.Vertical,
                buttonCount = 3,
                autoGenerateButtons = true,
                commandCategories = new[] { ButtonGroupManager.CommandCategory.SaveLoad },
                position = new Vector3(-200, 0, 0),
                visibleInStates = new List<ButtonGroupController.GameState> { ButtonGroupController.GameState.MainMenu }
            };
            
            // Grid layout
            var gridConfig = new ButtonGroupController.ButtonGroupConfiguration
            {
                groupName = "Demo Grid Group",
                groupId = "demo_grid",
                groupType = ButtonGroupManager.ButtonGroupType.Grid,
                buttonCount = 6,
                autoGenerateButtons = true,
                commandCategories = new[] { ButtonGroupManager.CommandCategory.Resources },
                position = new Vector3(200, 0, 0),
                visibleInStates = new List<ButtonGroupController.GameState> { ButtonGroupController.GameState.MainMenu }
            };
            
            controller.CreateButtonGroup(verticalConfig);
            controller.CreateButtonGroup(gridConfig);
        }

        private void DemoStep4_CommandCategories()
        {
            UpdateStatus("Step 4: Command Categories\n" +
                        "Showing different command categories and auto-generation.");
            
            var categories = new[] { 
                ButtonGroupManager.CommandCategory.Population, 
                ButtonGroupManager.CommandCategory.Audio, 
                ButtonGroupManager.CommandCategory.Debug 
            };
            
            foreach (var category in categories)
            {
                var config = new ButtonGroupController.ButtonGroupConfiguration
                {
                    groupName = $"Demo {category} Group",
                    groupId = $"demo_{category.ToString().ToLower()}",
                    groupType = ButtonGroupManager.ButtonGroupType.Horizontal,
                    buttonCount = 3,
                    autoGenerateButtons = true,
                    commandCategories = new[] { category },
                    position = new Vector3(0, -100 * (int)category, 0),
                    visibleInStates = new List<ButtonGroupController.GameState> { ButtonGroupController.GameState.InGame }
                };
                
                controller.CreateButtonGroup(config);
            }
        }

        private void DemoStep5_GlobalStyling()
        {
            UpdateStatus("Step 5: Global Styling\n" +
                        "Applying consistent styling across all button groups.");
            
            // This would modify the global settings and apply them
            controller.ApplyGlobalSettings();
        }

        private void DemoStep6_StateBasedVisibility()
        {
            UpdateStatus("Step 6: State-Based Visibility\n" +
                        "Switching between game states to show different button groups.");
            
            // Demonstrate state switching
            StartCoroutine(StateSwitchingDemo());
        }

        private System.Collections.IEnumerator StateSwitchingDemo()
        {
            var states = new[] { 
                ButtonGroupController.GameState.MainMenu,
                ButtonGroupController.GameState.InGame,
                ButtonGroupController.GameState.Settings,
                ButtonGroupController.GameState.MainMenu
            };
            
            foreach (var state in states)
            {
                controller.SetGameState(state);
                UpdateStatus($"Current State: {state}\nVisible groups updated automatically.");
                yield return new WaitForSeconds(2f);
            }
        }

        private void DemoStep7_DynamicGroupModification()
        {
            UpdateStatus("Step 7: Dynamic Group Modification\n" +
                        "Adding, removing, and modifying groups at runtime.");
            
            // Create a temporary group
            var tempConfig = new ButtonGroupController.ButtonGroupConfiguration
            {
                groupName = "Temporary Group",
                groupId = "temp_group",
                buttonCount = 2,
                autoGenerateButtons = true,
                commandCategories = new[] { ButtonGroupManager.CommandCategory.Navigation },
                visibleInStates = new List<ButtonGroupController.GameState> { ButtonGroupController.GameState.MainMenu }
            };
            
            var tempGroup = controller.CreateButtonGroup(tempConfig);
            
            // Remove it after 3 seconds
            StartCoroutine(RemoveGroupAfterDelay(tempGroup, 3f));
        }

        private System.Collections.IEnumerator RemoveGroupAfterDelay(ButtonGroupManager group, float delay)
        {
            yield return new WaitForSeconds(delay);
            controller.DestroyButtonGroup(group);
            UpdateStatus("Temporary group removed successfully!");
        }

        private void DemoStep8_AdvancedFeatures()
        {
            UpdateStatus("Step 8: Advanced Features\n" +
                        "Demonstrating object pooling, batch operations, and performance optimizations.");
            
            // This would showcase advanced features
            LogAdvancedFeatures();
        }

        private void LogAdvancedFeatures()
        {
            UnityEngine.Debug.Log("Advanced Features Demo:");
            UnityEngine.Debug.Log("- Object Pooling: Enabled for better performance");
            UnityEngine.Debug.Log("- Batch Operations: Multiple groups can be modified simultaneously");
            UnityEngine.Debug.Log("- Lazy Loading: Groups are created only when needed");
            UnityEngine.Debug.Log("- Cross-Platform Support: Works on all Unity-supported platforms");
        }

        private void DemoStep9_EventSystem()
        {
            UpdateStatus("Step 9: Event System\n" +
                        "Comprehensive event system for responding to group and button actions.");
            
            // Events are already hooked up and will be demonstrated through interactions
        }

        private void DemoStep10_KeyboardNavigation()
        {
            UpdateStatus("Step 10: Keyboard Navigation\n" +
                        "Use F1-F3 to switch states, F9 to refresh groups, F10 to toggle visibility.\n" +
                        "Arrow keys and Tab for button navigation within groups.");
        }

        private void DemoStep11_PerformanceFeatures()
        {
            UpdateStatus("Step 11: Performance Features\n" +
                        "Object pooling, batch operations, and optimized rendering for large numbers of buttons.");
            
            // Create many groups to demonstrate performance
            CreatePerformanceTestGroups();
        }

        private void CreatePerformanceTestGroups()
        {
            for (int i = 0; i < 5; i++)
            {
                var config = new ButtonGroupController.ButtonGroupConfiguration
                {
                    groupName = $"Performance Test Group {i}",
                    groupId = $"perf_test_{i}",
                    buttonCount = 4,
                    autoGenerateButtons = true,
                    commandCategories = new[] { ButtonGroupManager.CommandCategory.Debug },
                    position = new Vector3(i * 150 - 300, -200, 0),
                    visibleInStates = new List<ButtonGroupController.GameState> { ButtonGroupController.GameState.MainMenu }
                };
                
                controller.CreateButtonGroup(config);
            }
        }

        private void DemoStep12_Conclusion()
        {
            UpdateStatus("Step 12: Conclusion\n" +
                        "ButtonGroupController provides comprehensive control over button groups:\n" +
                        "• Centralized management • State-based visibility • Global styling\n" +
                        "• Enhanced Auto Button integration • Performance optimization\n" +
                        "• Event-driven architecture • Keyboard navigation support");
        }

        #endregion

        #region Demo Control

        public void StartDemo()
        {
            currentDemoStep = 0;
            ExecuteCurrentStep();
        }

        public void NextDemo()
        {
            if (currentDemoStep < totalDemoSteps - 1)
            {
                currentDemoStep++;
                ExecuteCurrentStep();
            }
        }

        public void PreviousDemo()
        {
            if (currentDemoStep > 0)
            {
                currentDemoStep--;
                ExecuteCurrentStep();
            }
        }

        public void ResetDemo()
        {
            controller.DestroyAllGroups();
            currentDemoStep = 0;
            UpdateStatus("Demo reset. All groups cleared.");
            UpdateProgress();
        }

        public void StartAutoDemo()
        {
            StartCoroutine(AutoDemoCoroutine());
        }

        private System.Collections.IEnumerator AutoDemoCoroutine()
        {
            ResetDemo();
            
            for (int i = 0; i < totalDemoSteps; i++)
            {
                currentDemoStep = i;
                ExecuteCurrentStep();
                yield return new WaitForSeconds(4f);
            }
        }

        private void ExecuteCurrentStep()
        {
            if (currentDemoStep >= 0 && currentDemoStep < demoSteps.Count)
            {
                demoSteps[currentDemoStep]?.Invoke();
                UpdateProgress();
                UpdateButtonStates();
            }
        }

        private void UpdateProgress()
        {
            if (progressSlider != null)
            {
                progressSlider.value = currentDemoStep;
            }
        }

        private void UpdateButtonStates()
        {
            if (prevDemoButton != null)
            {
                prevDemoButton.interactable = currentDemoStep > 0;
            }
            
            if (nextDemoButton != null)
            {
                nextDemoButton.interactable = currentDemoStep < totalDemoSteps - 1;
            }
        }

        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            
            UnityEngine.Debug.Log($"Demo: {message}");
        }

        #endregion

        #region Input Handling

        private void HandleKeyboardShortcuts()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NextDemo();
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                PreviousDemo();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ResetDemo();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                StartAutoDemo();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        #endregion

        #region Event Handlers

        private void OnGroupCreated(ButtonGroupManager group)
        {
            UnityEngine.Debug.Log($"Group created: {group.gameObject.name}");
        }

        private void OnGroupDestroyed(ButtonGroupManager group)
        {
            UnityEngine.Debug.Log($"Group destroyed: {group.gameObject.name}");
        }

        private void OnGlobalButtonClicked(string buttonText)
        {
            UnityEngine.Debug.Log($"Global button clicked: {buttonText}");
            UpdateStatus($"Button clicked: {buttonText}");
        }

        private void OnGameStateChanged(ButtonGroupController.GameState newState)
        {
            UnityEngine.Debug.Log($"Game state changed to: {newState}");
        }

        private void OnAllGroupsInitialized()
        {
            UnityEngine.Debug.Log("All button groups have been initialized");
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.OnGroupCreated -= OnGroupCreated;
                controller.OnGroupDestroyed -= OnGroupDestroyed;
                controller.OnGlobalButtonClicked -= OnGlobalButtonClicked;
                controller.OnGameStateChanged -= OnGameStateChanged;
                controller.OnAllGroupsInitialized -= OnAllGroupsInitialized;
            }
        }

        #endregion
    }
}

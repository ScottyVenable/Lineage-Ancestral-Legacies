using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lineage.UI
{
    /// <summary>
    /// Comprehensive manager for button groups with EnhancedAutoButtonHandler integration.
    /// Provides full control over layout, styling, behavior, and content from a single component.
    /// </summary>
    public class ButtonGroupManager : MonoBehaviour
    {
        [Header("Button Group Settings")]
        [SerializeField] private int buttonCount = 2;
        [SerializeField] private int maxButtonCount = 20;
        [SerializeField] private int minButtonCount = 1;
        [SerializeField] private bool createOnStart = true;
        [SerializeField] private bool autoUpdateOnChange = true;

        [Header("Button Prefab & Creation")]
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private bool createPrefabIfMissing = true;
        [SerializeField] private bool addAutoButtonHandler = true;
        [SerializeField] private bool enableRegexPatterns = false;
        [SerializeField] private bool enableVisualFeedback = true;

        [Header("Layout Settings")]
        [SerializeField] private ButtonGroupType groupType = ButtonGroupType.Horizontal;
        [SerializeField] private ButtonAlignment alignment = ButtonAlignment.UpperCenter;
        [SerializeField] private Vector2 buttonSize = new Vector2(120, 40);
        [SerializeField] private float buttonSpacing = 10f;
        [SerializeField] private bool autoSize = true;
        [SerializeField] private bool uniformButtonSizes = true;
        [SerializeField] private Padding containerPadding = new Padding(10, 10, 10, 10);

        [Header("Button Content")]
        [SerializeField] private List<ButtonData> buttonConfigs = new List<ButtonData>();
        [SerializeField] private string defaultButtonText = "Button";
        [SerializeField] private bool autoGenerateFromCommands = false;
        [SerializeField] private CommandCategory[] includeCategories = { CommandCategory.GameManagement, CommandCategory.SaveLoad };

        [Header("Visual Styling")]
        [SerializeField] private ButtonVisualSettings visualSettings = new ButtonVisualSettings();
        [SerializeField] private bool applyThemeToAllButtons = true;
        [SerializeField] private bool inheritParentTheme = true;

        [Header("Behavior Settings")]
        [SerializeField] private bool enableButtonInteraction = true;
        [SerializeField] private bool enableKeyboardNavigation = true;
        [SerializeField] private bool enableButtonGroups = false;
        [SerializeField] private bool allowMultipleSelection = false;
        [SerializeField] private ButtonBehavior defaultBehavior = ButtonBehavior.Standard;

        [Header("Animation & Effects")]
        [SerializeField] private bool enableAnimations = true;
        [SerializeField] private AnimationSettings animationSettings = new AnimationSettings();
        [SerializeField] private bool enableSoundEffects = true;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip buttonHoverSound;

        [Header("Debug & Testing")]
        [SerializeField] private bool enableDebugMode = false;
        [SerializeField] private bool showButtonBounds = false;
        [SerializeField] private bool logButtonActions = false;

        // Private fields
        private List<GameObject> managedButtons = new List<GameObject>();
        private List<EnhancedAutoButtonHandler> buttonHandlers = new List<EnhancedAutoButtonHandler>();
        private LayoutGroup layoutGroup;
        private ContentSizeFitter sizeFitter;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        // Events
        public System.Action<GameObject> OnButtonCreated;
        public System.Action<GameObject> OnButtonDestroyed;
        public System.Action<string> OnButtonClicked;
        public System.Action OnLayoutChanged;

        public enum ButtonGroupType
        {
            Horizontal,
            Vertical,
            Grid,
            Radial,
            Custom
        }

        public enum ButtonAlignment
        {
            UpperLeft, UpperCenter, UpperRight,
            MiddleLeft, MiddleCenter, MiddleRight,
            LowerLeft, LowerCenter, LowerRight
        }

        public enum CommandCategory
        {
            GameManagement, SaveLoad, Navigation, GameState,
            Settings, Audio, Population, Resources, Debug, Custom
        }

        public enum ButtonBehavior
        {
            Standard, Toggle, RadioButton, Momentary, Disabled
        }

        [System.Serializable]
        public class ButtonData
        {
            public string buttonText = "Button";
            public string command = "";
            public bool isEnabled = true;
            public ButtonBehavior behavior = ButtonBehavior.Standard;
            public Sprite customIcon;
            public Color customColor = Color.white;
            public Vector2 customSize = Vector2.zero;
            public string tooltip = "";
            public KeyCode shortcutKey = KeyCode.None;
            public bool isToggled = false;
            public System.Action customAction;
        }

        [System.Serializable]
        public class ButtonVisualSettings
        {
            [Header("Colors")]
            public Color normalColor = Color.white;
            public Color highlightedColor = new Color(0.96f, 0.96f, 0.96f);
            public Color pressedColor = new Color(0.78f, 0.78f, 0.78f);
            public Color selectedColor = new Color(0.96f, 0.96f, 0.96f);
            public Color disabledColor = new Color(0.78f, 0.78f, 0.78f, 0.5f);

            [Header("Text")]
            public Font font;
            public int fontSize = 14;
            public Color textColor = Color.black;
            public FontStyle fontStyle = FontStyle.Normal;
            public TextAnchor textAlignment = TextAnchor.MiddleCenter;

            [Header("Background")]
            public Sprite backgroundSprite;
            public Image.Type imageType = Image.Type.Sliced;
            public bool useGradient = false;
            public Gradient backgroundGradient = new Gradient();

            [Header("Border")]
            public bool showBorder = false;
            public Color borderColor = Color.black;
            public float borderWidth = 1f;
        }

        [System.Serializable]
        public class AnimationSettings
        {
            public bool enableHoverAnimation = true;
            public bool enableClickAnimation = true;
            public bool enableFadeInOut = true;
            public float animationDuration = 0.2f;
            public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            public Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1f);
            public Vector3 clickScale = new Vector3(0.95f, 0.95f, 1f);
        }

        [System.Serializable]
        public class Padding
        {
            public float left, right, top, bottom;
            
            public Padding(float left, float right, float top, float bottom)
            {
                this.left = left;
                this.right = right;
                this.top = top;
                this.bottom = bottom;
            }
        }

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            if (createOnStart)
            {
                CreateButtonGroup();
            }
        }

        private void OnValidate()
        {
            if (autoUpdateOnChange && Application.isPlaying)
            {
                UpdateButtonGroup();
            }
        }

        #region Initialization & Setup

        private void InitializeComponents()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && enableButtonInteraction)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            SetupLayoutGroup();
            SetupContentSizeFitter();
        }

        private void SetupLayoutGroup()
        {
            // Remove existing layout group
            if (layoutGroup != null)
            {
                DestroyImmediate(layoutGroup);
            }

            switch (groupType)
            {
                case ButtonGroupType.Horizontal:
                    var hLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
                    hLayoutGroup.spacing = buttonSpacing;
                    hLayoutGroup.childAlignment = GetTextAnchor(alignment);
                    hLayoutGroup.childControlWidth = autoSize;
                    hLayoutGroup.childControlHeight = autoSize;
                    hLayoutGroup.childForceExpandWidth = autoSize;
                    hLayoutGroup.childForceExpandHeight = false;
                    SetLayoutPadding(hLayoutGroup);
                    layoutGroup = hLayoutGroup;
                    break;

                case ButtonGroupType.Vertical:
                    var vLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
                    vLayoutGroup.spacing = buttonSpacing;
                    vLayoutGroup.childAlignment = GetTextAnchor(alignment);
                    vLayoutGroup.childControlWidth = autoSize;
                    vLayoutGroup.childControlHeight = autoSize;
                    vLayoutGroup.childForceExpandWidth = false;
                    vLayoutGroup.childForceExpandHeight = autoSize;
                    SetLayoutPadding(vLayoutGroup);
                    layoutGroup = vLayoutGroup;
                    break;

                case ButtonGroupType.Grid:
                    var gridLayoutGroup = gameObject.AddComponent<GridLayoutGroup>();
                    gridLayoutGroup.spacing = new Vector2(buttonSpacing, buttonSpacing);
                    gridLayoutGroup.cellSize = buttonSize;
                    gridLayoutGroup.childAlignment = GetTextAnchor(alignment);
                    SetLayoutPadding(gridLayoutGroup);
                    layoutGroup = gridLayoutGroup;
                    break;
            }
        }

        private void SetupContentSizeFitter()
        {
            if (autoSize)
            {
                sizeFitter = GetComponent<ContentSizeFitter>();
                if (sizeFitter == null)
                {
                    sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
                }

                switch (groupType)
                {
                    case ButtonGroupType.Horizontal:
                        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        break;
                    case ButtonGroupType.Vertical:
                        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        break;
                    case ButtonGroupType.Grid:
                        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        break;
                }
            }
        }

        private void SetLayoutPadding(HorizontalOrVerticalLayoutGroup layoutGroup)
        {
            layoutGroup.padding.left = (int)containerPadding.left;
            layoutGroup.padding.right = (int)containerPadding.right;
            layoutGroup.padding.top = (int)containerPadding.top;
            layoutGroup.padding.bottom = (int)containerPadding.bottom;
        }

        private void SetLayoutPadding(GridLayoutGroup layoutGroup)
        {
            layoutGroup.padding.left = (int)containerPadding.left;
            layoutGroup.padding.right = (int)containerPadding.right;
            layoutGroup.padding.top = (int)containerPadding.top;
            layoutGroup.padding.bottom = (int)containerPadding.bottom;
        }

        private TextAnchor GetTextAnchor(ButtonAlignment alignment)
        {
            switch (alignment)
            {
                case ButtonAlignment.UpperLeft: return TextAnchor.UpperLeft;
                case ButtonAlignment.UpperCenter: return TextAnchor.UpperCenter;
                case ButtonAlignment.UpperRight: return TextAnchor.UpperRight;
                case ButtonAlignment.MiddleLeft: return TextAnchor.MiddleLeft;
                case ButtonAlignment.MiddleCenter: return TextAnchor.MiddleCenter;
                case ButtonAlignment.MiddleRight: return TextAnchor.MiddleRight;
                case ButtonAlignment.LowerLeft: return TextAnchor.LowerLeft;
                case ButtonAlignment.LowerCenter: return TextAnchor.LowerCenter;
                case ButtonAlignment.LowerRight: return TextAnchor.LowerRight;
                default: return TextAnchor.MiddleCenter;
            }
        }

        #endregion

        #region Button Group Management

        public void CreateButtonGroup()
        {
            ClearExistingButtons();
            
            if (autoGenerateFromCommands)
            {
                GenerateButtonsFromCommands();
            }
            else
            {
                CreateButtonsFromConfigs();
            }

            ApplyGlobalStyling();
            SetupKeyboardNavigation();
        }

        public void UpdateButtonGroup()
        {
            SetupLayoutGroup();
            SetupContentSizeFitter();
            
            // Update existing buttons
            for (int i = 0; i < managedButtons.Count; i++)
            {
                if (i < buttonConfigs.Count)
                {
                    UpdateButton(managedButtons[i], buttonConfigs[i], i);
                }
            }

            // Add missing buttons
            while (managedButtons.Count < buttonCount && managedButtons.Count < buttonConfigs.Count)
            {
                CreateButton(buttonConfigs[managedButtons.Count], managedButtons.Count);
            }

            // Remove excess buttons
            while (managedButtons.Count > buttonCount)
            {
                RemoveButtonAt(managedButtons.Count - 1);
            }

            ApplyGlobalStyling();
            OnLayoutChanged?.Invoke();
        }

        private void CreateButtonsFromConfigs()
        {
            // Ensure we have enough button configs
            while (buttonConfigs.Count < buttonCount)
            {
                buttonConfigs.Add(new ButtonData
                {
                    buttonText = $"{defaultButtonText} {buttonConfigs.Count + 1}",
                    command = "",
                    isEnabled = true
                });
            }

            // Create buttons
            for (int i = 0; i < buttonCount && i < buttonConfigs.Count; i++)
            {
                CreateButton(buttonConfigs[i], i);
            }
        }

        private void GenerateButtonsFromCommands()
        {
            buttonConfigs.Clear();
            var commands = GetCommandsForCategories();

            foreach (var command in commands.Take(buttonCount))
            {
                buttonConfigs.Add(new ButtonData
                {
                    buttonText = FormatCommandText(command),
                    command = command,
                    isEnabled = true
                });
            }

            CreateButtonsFromConfigs();
        }

        private List<string> GetCommandsForCategories()
        {
            var commands = new List<string>();

            foreach (var category in includeCategories)
            {
                commands.AddRange(GetCommandsForCategory(category));
            }

            return commands.Distinct().ToList();
        }

        private List<string> GetCommandsForCategory(CommandCategory category)
        {
            switch (category)
            {
                case CommandCategory.GameManagement:
                    return new List<string> { "quit", "exit", "restart", "new game" };
                case CommandCategory.SaveLoad:
                    return new List<string> { "save", "load", "quick save", "quick load" };
                case CommandCategory.Navigation:
                    return new List<string> { "main menu", "back", "close" };
                case CommandCategory.GameState:
                    return new List<string> { "pause", "resume" };
                case CommandCategory.Settings:
                    return new List<string> { "settings", "apply", "reset" };
                case CommandCategory.Audio:
                    return new List<string> { "mute", "unmute" };
                case CommandCategory.Population:
                    return new List<string> { "spawn pop", "add pop" };
                case CommandCategory.Resources:
                    return new List<string> { "add food", "add wood", "add faith" };
                case CommandCategory.Debug:
                    return new List<string> { "debug", "console" };
                default:
                    return new List<string>();
            }
        }

        private string FormatCommandText(string command)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(command);
        }

        #endregion

        #region Button Creation & Management

        private GameObject CreateButton(ButtonData config, int index)
        {
            GameObject buttonObj = CreateButtonGameObject(config, index);
            SetupButtonComponents(buttonObj, config);
            SetupAutoButtonHandler(buttonObj, config);
            ApplyButtonStyling(buttonObj, config);
            SetupButtonAnimations(buttonObj);
            SetupButtonEvents(buttonObj, config);

            managedButtons.Add(buttonObj);
            OnButtonCreated?.Invoke(buttonObj);            if (enableDebugMode)
            {
                UnityEngine.Debug.Log($"Created button: {config.buttonText} at index {index}");
            }

            return buttonObj;
        }

        private GameObject CreateButtonGameObject(ButtonData config, int index)
        {
            GameObject buttonObj;

            if (buttonPrefab != null)
            {
                buttonObj = Instantiate(buttonPrefab, transform);
            }
            else if (createPrefabIfMissing)
            {
                buttonObj = CreateDefaultButtonPrefab();
            }            else
            {
                UnityEngine.Debug.LogError("No button prefab assigned and createPrefabIfMissing is false!");
                return null;
            }

            buttonObj.name = $"Button_{index}_{config.buttonText.Replace(" ", "_")}";
            return buttonObj;
        }

        private GameObject CreateDefaultButtonPrefab()
        {
            GameObject buttonObj = new GameObject("Button");
            buttonObj.transform.SetParent(transform, false);

            // Add Image component
            Image image = buttonObj.AddComponent<Image>();
            image.sprite = visualSettings.backgroundSprite;
            image.type = visualSettings.imageType;
            image.color = visualSettings.normalColor;

            // Add Button component
            Button button = buttonObj.AddComponent<Button>();
            
            // Setup button colors
            ColorBlock colors = button.colors;
            colors.normalColor = visualSettings.normalColor;
            colors.highlightedColor = visualSettings.highlightedColor;
            colors.pressedColor = visualSettings.pressedColor;
            colors.selectedColor = visualSettings.selectedColor;
            colors.disabledColor = visualSettings.disabledColor;
            button.colors = colors;

            // Add RectTransform
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = buttonSize;

            // Add text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = defaultButtonText;
            text.fontSize = visualSettings.fontSize;
            text.color = visualSettings.textColor;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = GetFontStyle(visualSettings.fontStyle);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return buttonObj;
        }        private void SetupButtonComponents(GameObject buttonObj, ButtonData config)
        {
            // Ensure button has required components
            if (buttonObj.GetComponent<Button>() == null)
            {
                buttonObj.AddComponent<Button>();
            }

            if (buttonObj.GetComponent<Image>() == null)
            {
                buttonObj.AddComponent<Image>();
            }

            // Set button text - handle both TextMeshPro and legacy Text components
            var tmpTextComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            var legacyTextComponent = buttonObj.GetComponentInChildren<Text>();

            if (tmpTextComponent != null)
            {
                tmpTextComponent.text = config.buttonText;
            }
            else if (legacyTextComponent != null)
            {
                legacyTextComponent.text = config.buttonText;
            }
            else
            {
                if (enableDebugMode)
                {
                    UnityEngine.Debug.LogWarning($"No text component found in button {buttonObj.name}");
                }
            }
        }

        private void SetupAutoButtonHandler(GameObject buttonObj, ButtonData config)
        {
            if (!addAutoButtonHandler) return;

            var handler = buttonObj.GetComponent<EnhancedAutoButtonHandler>();
            if (handler == null)
            {
                handler = buttonObj.AddComponent<EnhancedAutoButtonHandler>();
            }

            // Configure handler settings via reflection if needed
            var handlerType = typeof(EnhancedAutoButtonHandler);
            
            var useRegexField = handlerType.GetField("useRegexPatterns", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            useRegexField?.SetValue(handler, enableRegexPatterns);

            var enableFeedbackField = handlerType.GetField("enableVisualFeedback", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enableFeedbackField?.SetValue(handler, enableVisualFeedback);

            // Add custom action if specified
            if (config.customAction != null)
            {
                handler.AddCustomAction(config.command, config.customAction);
            }

            buttonHandlers.Add(handler);
        }

        private void ApplyButtonStyling(GameObject buttonObj, ButtonData config)
        {
            var button = buttonObj.GetComponent<Button>();
            var image = buttonObj.GetComponent<Image>();
            
            if (button != null && applyThemeToAllButtons)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = config.customColor != Color.white ? config.customColor : visualSettings.normalColor;
                colors.highlightedColor = visualSettings.highlightedColor;
                colors.pressedColor = visualSettings.pressedColor;
                colors.selectedColor = visualSettings.selectedColor;
                colors.disabledColor = visualSettings.disabledColor;
                button.colors = colors;
                
                button.interactable = config.isEnabled && enableButtonInteraction;
            }

            if (image != null)
            {
                if (config.customIcon != null)
                {
                    image.sprite = config.customIcon;
                }
                else if (visualSettings.backgroundSprite != null)
                {
                    image.sprite = visualSettings.backgroundSprite;
                }
                
                image.type = visualSettings.imageType;
            }

            // Apply custom size if specified
            if (config.customSize != Vector2.zero)
            {
                var rectTransform = buttonObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = config.customSize;
            }
            else if (!autoSize)
            {
                var rectTransform = buttonObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = buttonSize;
            }
        }

        private void SetupButtonAnimations(GameObject buttonObj)
        {
            if (!enableAnimations) return;

            var animator = buttonObj.GetComponent<Animator>();
            if (animator == null && animationSettings.enableHoverAnimation)
            {
                // Add animation component and setup basic animations
                // This would require creating animation clips, which is complex
                // For now, we'll use simple scale animations
                var scaleAnimation = buttonObj.AddComponent<ButtonScaleAnimation>();
                scaleAnimation.Initialize(animationSettings);
            }
        }

        private void SetupButtonEvents(GameObject buttonObj, ButtonData config)
        {
            var button = buttonObj.GetComponent<Button>();
            if (button == null) return;

            button.onClick.AddListener(() => {
                OnButtonClicked?.Invoke(config.buttonText);
                
                if (enableSoundEffects && buttonClickSound != null)
                {
                    AudioSource.PlayClipAtPoint(buttonClickSound, transform.position);
                }                if (logButtonActions)
                {
                    UnityEngine.Debug.Log($"Button clicked: {config.buttonText} | Command: {config.command}");
                }

                HandleButtonBehavior(buttonObj, config);
            });
        }

        private void HandleButtonBehavior(GameObject buttonObj, ButtonData config)
        {
            switch (config.behavior)
            {
                case ButtonBehavior.Toggle:
                    config.isToggled = !config.isToggled;
                    UpdateButtonToggleState(buttonObj, config.isToggled);
                    break;
                    
                case ButtonBehavior.RadioButton:
                    if (enableButtonGroups)
                    {
                        // Deselect all other radio buttons in group
                        for (int i = 0; i < buttonConfigs.Count; i++)
                        {
                            if (buttonConfigs[i].behavior == ButtonBehavior.RadioButton)
                            {
                                buttonConfigs[i].isToggled = buttonConfigs[i] == config;
                                UpdateButtonToggleState(managedButtons[i], buttonConfigs[i].isToggled);
                            }
                        }
                    }
                    break;
                    
                case ButtonBehavior.Disabled:
                    // Button should not respond
                    break;
            }
        }

        private void UpdateButtonToggleState(GameObject buttonObj, bool isToggled)
        {
            var image = buttonObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = isToggled ? visualSettings.selectedColor : visualSettings.normalColor;
            }
        }

        #endregion

        #region Utility Methods

        private void UpdateButton(GameObject buttonObj, ButtonData config, int index)
        {
            SetupButtonComponents(buttonObj, config);
            ApplyButtonStyling(buttonObj, config);
            
            buttonObj.name = $"Button_{index}_{config.buttonText.Replace(" ", "_")}";
        }

        private void RemoveButtonAt(int index)
        {
            if (index >= 0 && index < managedButtons.Count)
            {
                GameObject buttonToRemove = managedButtons[index];
                managedButtons.RemoveAt(index);
                
                if (index < buttonHandlers.Count)
                {
                    buttonHandlers.RemoveAt(index);
                }

                OnButtonDestroyed?.Invoke(buttonToRemove);
                
                if (Application.isPlaying)
                {
                    Destroy(buttonToRemove);
                }
                else
                {
                    DestroyImmediate(buttonToRemove);
                }
            }
        }

        private void ClearExistingButtons()
        {
            foreach (var button in managedButtons)
            {
                if (button != null)
                {
                    OnButtonDestroyed?.Invoke(button);
                    
                    if (Application.isPlaying)
                    {
                        Destroy(button);
                    }
                    else
                    {
                        DestroyImmediate(button);
                    }
                }
            }
            
            managedButtons.Clear();
            buttonHandlers.Clear();
        }

        private void ApplyGlobalStyling()
        {
            if (!applyThemeToAllButtons) return;

            foreach (var buttonObj in managedButtons)
            {
                if (buttonObj != null)
                {
                    ApplyButtonStyling(buttonObj, buttonConfigs[managedButtons.IndexOf(buttonObj)]);
                }
            }
        }

        private void SetupKeyboardNavigation()
        {
            if (!enableKeyboardNavigation) return;

            for (int i = 0; i < managedButtons.Count; i++)
            {
                var button = managedButtons[i].GetComponent<Button>();
                if (button == null) continue;

                var navigation = button.navigation;
                navigation.mode = Navigation.Mode.Explicit;

                // Set up navigation based on group type
                switch (groupType)
                {
                    case ButtonGroupType.Horizontal:
                        if (i > 0)
                            navigation.selectOnLeft = managedButtons[i - 1].GetComponent<Button>();
                        if (i < managedButtons.Count - 1)
                            navigation.selectOnRight = managedButtons[i + 1].GetComponent<Button>();
                        break;
                        
                    case ButtonGroupType.Vertical:
                        if (i > 0)
                            navigation.selectOnUp = managedButtons[i - 1].GetComponent<Button>();
                        if (i < managedButtons.Count - 1)
                            navigation.selectOnDown = managedButtons[i + 1].GetComponent<Button>();
                        break;
                }

                button.navigation = navigation;
            }
        }

        private FontStyles GetFontStyle(FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case FontStyle.Bold: return FontStyles.Bold;
                case FontStyle.Italic: return FontStyles.Italic;
                case FontStyle.BoldAndItalic: return FontStyles.Bold | FontStyles.Italic;
                default: return FontStyles.Normal;
            }
        }

        #endregion

        #region Public API

        public void AddButton(ButtonData buttonData)
        {
            if (buttonConfigs.Count >= maxButtonCount) return;

            buttonConfigs.Add(buttonData);
            buttonCount = buttonConfigs.Count;
            
            if (Application.isPlaying)
            {
                CreateButton(buttonData, buttonConfigs.Count - 1);
                ApplyGlobalStyling();
            }
        }

        public void RemoveButton(int index)
        {
            if (index >= 0 && index < buttonConfigs.Count)
            {
                buttonConfigs.RemoveAt(index);
                buttonCount = buttonConfigs.Count;
                
                if (Application.isPlaying)
                {
                    RemoveButtonAt(index);
                }
            }
        }

        public void SetButtonText(int index, string text)
        {
            if (index >= 0 && index < buttonConfigs.Count)
            {
                buttonConfigs[index].buttonText = text;
                
                if (Application.isPlaying && index < managedButtons.Count)
                {
                    UpdateButton(managedButtons[index], buttonConfigs[index], index);
                }
            }
        }

        public void SetButtonEnabled(int index, bool enabled)
        {
            if (index >= 0 && index < buttonConfigs.Count)
            {
                buttonConfigs[index].isEnabled = enabled;
                
                if (Application.isPlaying && index < managedButtons.Count)
                {
                    var button = managedButtons[index].GetComponent<Button>();
                    if (button != null)
                    {
                        button.interactable = enabled && enableButtonInteraction;
                    }
                }
            }
        }

        public void RefreshButtonGroup()
        {
            if (Application.isPlaying)
            {
                UpdateButtonGroup();
            }
        }

        public List<GameObject> GetManagedButtons()
        {
            return new List<GameObject>(managedButtons);
        }

        public GameObject GetButton(int index)
        {
            return (index >= 0 && index < managedButtons.Count) ? managedButtons[index] : null;
        }

        #endregion

        #region Animation Component

        private class ButtonScaleAnimation : MonoBehaviour
        {
            private AnimationSettings settings;
            private Vector3 originalScale;
            private Button button;
            private bool isHovering = false;

            public void Initialize(AnimationSettings animSettings)
            {
                settings = animSettings;
                originalScale = transform.localScale;
                button = GetComponent<Button>();

                if (button != null && settings.enableHoverAnimation)
                {
                    var eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                    
                    var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                    pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                    pointerEnter.callback.AddListener((eventData) => OnPointerEnter());
                    eventTrigger.triggers.Add(pointerEnter);

                    var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                    pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                    pointerExit.callback.AddListener((eventData) => OnPointerExit());
                    eventTrigger.triggers.Add(pointerExit);

                    if (settings.enableClickAnimation)
                    {
                        var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
                        pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
                        pointerDown.callback.AddListener((eventData) => OnPointerDown());
                        eventTrigger.triggers.Add(pointerDown);

                        var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
                        pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
                        pointerUp.callback.AddListener((eventData) => OnPointerUp());
                        eventTrigger.triggers.Add(pointerUp);
                    }
                }
            }

            private void OnPointerEnter()
            {
                isHovering = true;
                StopAllCoroutines();
                StartCoroutine(ScaleTo(Vector3.Scale(originalScale, settings.hoverScale)));
            }

            private void OnPointerExit()
            {
                isHovering = false;
                StopAllCoroutines();
                StartCoroutine(ScaleTo(originalScale));
            }

            private void OnPointerDown()
            {
                StopAllCoroutines();
                StartCoroutine(ScaleTo(Vector3.Scale(originalScale, settings.clickScale)));
            }

            private void OnPointerUp()
            {
                StopAllCoroutines();
                Vector3 targetScale = isHovering ? Vector3.Scale(originalScale, settings.hoverScale) : originalScale;
                StartCoroutine(ScaleTo(targetScale));
            }

            private System.Collections.IEnumerator ScaleTo(Vector3 targetScale)
            {
                Vector3 startScale = transform.localScale;
                float elapsed = 0f;

                while (elapsed < settings.animationDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / settings.animationDuration;
                    t = settings.animationCurve.Evaluate(t);
                    
                    transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                    yield return null;
                }

                transform.localScale = targetScale;
            }
        }

        #endregion
    }
}

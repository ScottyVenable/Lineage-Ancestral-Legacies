# ButtonGroupManager - Complete Usage Guide

## Overview
The `ButtonGroupManager` is a comprehensive Unity component that provides full control over button groups with Enhanced Auto Button integration. It allows you to create, manage, and style button groups from a single central location instead of managing individual components separately.

## Key Features

### 🎯 **Core Functionality**
- **Automatic Button Creation**: Generate buttons with proper components and handlers
- **Layout Management**: Support for Horizontal, Vertical, Grid, and Radial layouts
- **Enhanced Auto Button Integration**: Automatically adds EnhancedAutoButtonHandler to buttons
- **Visual Styling**: Complete control over colors, fonts, and visual appearance
- **Behavior Management**: Support for Standard, Toggle, RadioButton, and Disabled behaviors
- **Animation System**: Built-in hover and click animations
- **Keyboard Navigation**: Automatic navigation setup between buttons
- **Event System**: Comprehensive event callbacks for button interactions

### 🎨 **Layout Types**
1. **Horizontal Layout**: Buttons arranged in a row
2. **Vertical Layout**: Buttons arranged in a column  
3. **Grid Layout**: Buttons arranged in a grid pattern
4. **Radial Layout**: Buttons arranged in a circle (planned)
5. **Custom Layout**: For specialized arrangements

### 🔘 **Button Behaviors**
- **Standard**: Normal click behavior
- **Toggle**: Button stays pressed/unpressed
- **RadioButton**: Only one button in group can be selected
- **Momentary**: Button is active only while pressed
- **Disabled**: Button cannot be interacted with

## Basic Setup

### 1. Add ButtonGroupManager to GameObject
```csharp
// In Unity Editor: Add Component -> ButtonGroupManager
// Or via script:
var buttonGroupManager = gameObject.AddComponent<ButtonGroupManager>();
```

### 2. Configure Basic Settings
```csharp
// Set number of buttons
buttonCount = 4;

// Set layout type
groupType = ButtonGroupType.Horizontal;

// Set button size
buttonSize = new Vector2(120, 40);

// Set spacing between buttons
buttonSpacing = 10f;
```

### 3. Create Buttons Automatically
```csharp
// Option 1: Auto-generate from command categories
autoGenerateFromCommands = true;
includeCategories = new CommandCategory[] { 
    CommandCategory.GameManagement, 
    CommandCategory.SaveLoad 
};

// Option 2: Manual configuration
autoGenerateFromCommands = false;
buttonConfigs = new List<ButtonData> {
    new ButtonData {
        buttonText = "Save Game",
        command = "save",
        isEnabled = true
    },
    new ButtonData {
        buttonText = "Load Game", 
        command = "load",
        isEnabled = true
    }
};
```

## Configuration Examples

### Game Control Buttons (Horizontal Layout)
```csharp
public void CreateGameControlGroup()
{
    var manager = GetComponent<ButtonGroupManager>();
    
    // Basic setup
    manager.buttonCount = 4;
    manager.groupType = ButtonGroupType.Horizontal;
    manager.buttonSpacing = 15f;
    manager.autoGenerateFromCommands = true;
    manager.includeCategories = new CommandCategory[] { 
        CommandCategory.GameManagement,
        CommandCategory.GameState 
    };
    
    // Visual styling
    manager.visualSettings.normalColor = new Color(0.8f, 0.9f, 1f);
    manager.visualSettings.fontSize = 12;
    manager.visualSettings.textColor = Color.black;
    
    // Enable features
    manager.enableAnimations = true;
    manager.enableKeyboardNavigation = true;
    manager.addAutoButtonHandler = true;
}
```

### Save/Load Menu (Custom Buttons)
```csharp
public void CreateSaveLoadMenu()
{
    var manager = GetComponent<ButtonGroupManager>();
    
    manager.buttonConfigs = new List<ButtonData> {
        new ButtonData {
            buttonText = "Quick Save",
            command = "quick save",
            isEnabled = true,
            customColor = new Color(0.8f, 1f, 0.8f),
            shortcutKey = KeyCode.F5
        },
        new ButtonData {
            buttonText = "Quick Load",
            command = "quick load", 
            isEnabled = true,
            customColor = new Color(1f, 0.9f, 0.8f),
            shortcutKey = KeyCode.F9
        },
        new ButtonData {
            buttonText = "Save As...",
            command = "save",
            isEnabled = true,
            customColor = new Color(0.9f, 0.9f, 1f)
        }
    };
    
    manager.buttonCount = manager.buttonConfigs.Count;
    manager.autoGenerateFromCommands = false;
    manager.enableVisualFeedback = true;
}
```

### Radio Button Group (Difficulty Selection)
```csharp
public void CreateDifficultySelector()
{
    var manager = GetComponent<ButtonGroupManager>();
    
    manager.buttonConfigs = new List<ButtonData> {
        new ButtonData {
            buttonText = "Easy",
            command = "difficulty easy",
            behavior = ButtonBehavior.RadioButton,
            isToggled = true // Default selection
        },
        new ButtonData {
            buttonText = "Normal",
            command = "difficulty normal",
            behavior = ButtonBehavior.RadioButton
        },
        new ButtonData {
            buttonText = "Hard", 
            command = "difficulty hard",
            behavior = ButtonBehavior.RadioButton
        }
    };
    
    manager.groupType = ButtonGroupType.Vertical;
    manager.enableButtonGroups = true;
    manager.allowMultipleSelection = false;
    manager.visualSettings.selectedColor = new Color(0.6f, 0.8f, 1f);
}
```

### Toggle Menu (Multiple Selection)
```csharp
public void CreateToggleMenu()
{
    var manager = GetComponent<ButtonGroupManager>();
    
    manager.buttonConfigs = new List<ButtonData> {
        new ButtonData {
            buttonText = "Show Population",
            command = "show population",
            behavior = ButtonBehavior.Toggle
        },
        new ButtonData {
            buttonText = "Show Resources",
            command = "show resources", 
            behavior = ButtonBehavior.Toggle
        },
        new ButtonData {
            buttonText = "Show Buildings",
            command = "show buildings",
            behavior = ButtonBehavior.Toggle
        }
    };
    
    manager.allowMultipleSelection = true;
    manager.groupType = ButtonGroupType.Vertical;
}
```

## Advanced Configuration

### Custom Visual Styling
```csharp
public void SetupCustomStyling()
{
    var visualSettings = new ButtonVisualSettings();
    
    // Colors
    visualSettings.normalColor = new Color(0.2f, 0.3f, 0.4f);
    visualSettings.highlightedColor = new Color(0.3f, 0.4f, 0.5f);
    visualSettings.pressedColor = new Color(0.1f, 0.2f, 0.3f);
    visualSettings.selectedColor = new Color(0.4f, 0.6f, 0.8f);
    visualSettings.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    
    // Text
    visualSettings.fontSize = 16;
    visualSettings.textColor = Color.white;
    visualSettings.fontStyle = FontStyle.Bold;
    visualSettings.textAlignment = TextAnchor.MiddleCenter;
    
    // Background
    visualSettings.useGradient = true;
    visualSettings.showBorder = true;
    visualSettings.borderColor = Color.black;
    visualSettings.borderWidth = 2f;
    
    manager.visualSettings = visualSettings;
}
```

### Animation Settings
```csharp
public void SetupAnimations()
{
    var animSettings = new AnimationSettings();
    
    animSettings.enableHoverAnimation = true;
    animSettings.enableClickAnimation = true;
    animSettings.animationDuration = 0.2f;
    animSettings.hoverScale = new Vector3(1.1f, 1.1f, 1f);
    animSettings.clickScale = new Vector3(0.9f, 0.9f, 1f);
    
    // Custom animation curve for smooth transitions
    animSettings.animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    manager.animationSettings = animSettings;
    manager.enableAnimations = true;
}
```

### Event Handling
```csharp
public void SetupEventHandlers()
{
    var manager = GetComponent<ButtonGroupManager>();
    
    // Subscribe to events
    manager.OnButtonCreated += OnButtonCreated;
    manager.OnButtonDestroyed += OnButtonDestroyed;
    manager.OnButtonClicked += OnButtonClicked;
    manager.OnLayoutChanged += OnLayoutChanged;
}

private void OnButtonCreated(GameObject button)
{
    Debug.Log($"Button created: {button.name}");
    
    // Add custom components or modify button
    var customComponent = button.AddComponent<CustomButtonBehavior>();
}

private void OnButtonClicked(string buttonText)
{
    Debug.Log($"Button clicked: {buttonText}");
    
    // Handle button click logic
    switch (buttonText.ToLower())
    {
        case "save game":
            SaveGame();
            break;
        case "load game":
            LoadGame();
            break;
    }
}
```

## Runtime Management

### Dynamic Button Addition
```csharp
public void AddDynamicButton()
{
    var newButton = new ButtonData {
        buttonText = "New Feature",
        command = "new feature",
        isEnabled = true,
        customColor = Color.green
    };
    
    manager.AddButton(newButton);
}
```

### Modify Existing Buttons
```csharp
public void ModifyButtons()
{
    // Change button text
    manager.SetButtonText(0, "Updated Text");
    
    // Enable/disable button
    manager.SetButtonEnabled(1, false);
    
    // Remove button
    manager.RemoveButton(2);
    
    // Refresh entire group
    manager.RefreshButtonGroup();
}
```

### Access Managed Buttons
```csharp
public void AccessButtons()
{
    // Get all managed buttons
    var buttons = manager.GetManagedButtons();
    
    // Get specific button
    var firstButton = manager.GetButton(0);
    
    // Modify button directly
    if (firstButton != null)
    {
        var buttonComponent = firstButton.GetComponent<Button>();
        buttonComponent.interactable = false;
    }
}
```

## Integration with EnhancedAutoButtonHandler

The ButtonGroupManager automatically integrates with EnhancedAutoButtonHandler when `addAutoButtonHandler` is enabled.

### Available Commands
All commands from EnhancedAutoButtonHandler are automatically available:

**Game Management**: quit, exit, restart, new game, pause, resume
**Save/Load**: save, load, quick save, quick load, auto save
**Navigation**: main menu, back, close, settings
**Resources**: add food, add wood, add faith, add [amount] [resource]
**Population**: spawn pop, add pop, spawn [amount] pop
**Audio**: mute, unmute, volume up, volume down

### Custom Commands
```csharp
public void AddCustomCommands()
{
    // Add custom action to button
    var buttonData = new ButtonData {
        buttonText = "Custom Action",
        command = "custom",
        customAction = () => {
            Debug.Log("Custom action executed!");
            // Your custom logic here
        }
    };
    
    manager.AddButton(buttonData);
}
```

## Best Practices

### 1. Organization
- Group related buttons together
- Use consistent naming conventions
- Keep button groups focused on specific functionality

### 2. Visual Design
- Maintain consistent styling across button groups
- Use color coding for different button types
- Ensure adequate spacing and sizing for touch interfaces

### 3. Accessibility
- Enable keyboard navigation for accessibility
- Use clear, descriptive button text
- Provide visual feedback for interactions

### 4. Performance
- Limit the number of buttons per group (recommended: 2-8)
- Use object pooling for frequently changing button groups
- Disable unnecessary features like animations on low-end devices

### 5. User Experience
- Group buttons logically by function
- Place frequently used buttons in easily accessible positions
- Provide consistent behavior across similar button groups

## Troubleshooting

### Common Issues

**Buttons not appearing:**
- Check if `createOnStart` is enabled
- Verify `buttonCount` is greater than 0
- Ensure parent Canvas is set up correctly

**Commands not working:**
- Verify `addAutoButtonHandler` is enabled
- Check if EnhancedAutoButtonHandler is in the project
- Ensure command text matches available commands

**Styling not applied:**
- Check if `applyThemeToAllButtons` is enabled
- Verify visual settings are configured
- Ensure button prefab supports styling

**Layout issues:**
- Check container padding settings
- Verify button sizes and spacing
- Ensure parent RectTransform is properly configured

### Debug Features
Enable debug mode for troubleshooting:
```csharp
manager.enableDebugMode = true;
manager.logButtonActions = true;
manager.showButtonBounds = true;
```

## Complete Example Scene Setup

```csharp
public class CompleteButtonGroupExample : MonoBehaviour
{
    void Start()
    {
        CreateMainMenuButtons();
        CreateGameHUD();
        CreateSettingsPanel();
    }
    
    void CreateMainMenuButtons()
    {
        GameObject menuGroup = new GameObject("MainMenu");
        var manager = menuGroup.AddComponent<ButtonGroupManager>();
        
        manager.buttonConfigs = new List<ButtonData> {
            new ButtonData { buttonText = "New Game", command = "new game" },
            new ButtonData { buttonText = "Load Game", command = "load" },
            new ButtonData { buttonText = "Settings", command = "settings" },
            new ButtonData { buttonText = "Quit", command = "quit" }
        };
        
        manager.groupType = ButtonGroupType.Vertical;
        manager.buttonSize = new Vector2(200, 50);
        manager.buttonSpacing = 20f;
        manager.enableAnimations = true;
    }
    
    void CreateGameHUD()
    {
        GameObject hudGroup = new GameObject("GameHUD");
        var manager = hudGroup.AddComponent<ButtonGroupManager>();
        
        manager.autoGenerateFromCommands = true;
        manager.includeCategories = new CommandCategory[] {
            CommandCategory.SaveLoad,
            CommandCategory.GameState
        };
        
        manager.groupType = ButtonGroupType.Horizontal;
        manager.buttonSize = new Vector2(80, 30);
    }
    
    void CreateSettingsPanel()
    {
        GameObject settingsGroup = new GameObject("Settings");
        var manager = settingsGroup.AddComponent<ButtonGroupManager>();
        
        manager.buttonConfigs = new List<ButtonData> {
            new ButtonData { 
                buttonText = "Graphics", 
                command = "graphics settings",
                behavior = ButtonBehavior.Toggle
            },
            new ButtonData { 
                buttonText = "Audio", 
                command = "audio settings",
                behavior = ButtonBehavior.Toggle  
            },
            new ButtonData { 
                buttonText = "Controls", 
                command = "control settings",
                behavior = ButtonBehavior.Toggle
            }
        };
        
        manager.allowMultipleSelection = true;
        manager.groupType = ButtonGroupType.Grid;
    }
}
```

This comprehensive ButtonGroupManager provides complete control over button group creation and management, integrating seamlessly with your Enhanced Auto Button system for a powerful, centralized button management solution.

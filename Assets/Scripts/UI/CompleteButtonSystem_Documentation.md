# Complete Button System Documentation

## Overview

The Complete Button System provides a comprehensive, hierarchical approach to button management in Unity projects. It consists of three main layers working together to provide maximum flexibility and control:

1. **ButtonGroupController** - Master controller for managing multiple button groups
2. **ButtonGroupManager** - Individual group management with layout and styling
3. **EnhancedAutoButtonHandler** - Intelligent button functionality detection and execution

## System Architecture

```
ButtonGroupController (Master Controller)
├── ButtonGroupManager (Group 1) 
│   ├── EnhancedAutoButtonHandler (Button 1)
│   ├── EnhancedAutoButtonHandler (Button 2)
│   └── EnhancedAutoButtonHandler (Button 3)
├── ButtonGroupManager (Group 2)
│   ├── EnhancedAutoButtonHandler (Button 1)
│   └── EnhancedAutoButtonHandler (Button 2)
└── ButtonGroupManager (Group 3)
    └── ...
```

## Key Features

### 🎯 Centralized Control
- Manage all button groups from a single component
- Global settings that cascade to all groups
- State-based visibility management
- Cross-platform compatibility

### 🎨 Flexible Styling
- Global visual settings with individual overrides
- Consistent theming across all button groups
- Animation system with hover/click effects
- Custom styling per group when needed

### 🔄 Dynamic Management
- Create, modify, and destroy groups at runtime
- Auto-generation from command categories
- Manual configuration with full control
- Object pooling for performance optimization

### 📱 Enhanced Auto Button Integration
- Automatic EnhancedAutoButtonHandler attachment
- 50+ built-in commands across 9 categories
- Parameterized commands ("Add 50 Food")
- Regex pattern support for complex commands
- Visual feedback on command execution

### 🎮 Game State Management
- Show/hide groups based on game state
- Smooth transitions between states
- Context-aware button functionality
- State-specific group configurations

## Quick Start Guide

### 1. Basic Setup

```csharp
// Create the controller
var controllerObj = new GameObject("Button Group Controller");
var controller = controllerObj.AddComponent<ButtonGroupController>();

// Create a simple button group
var config = new ButtonGroupController.ButtonGroupConfiguration
{
    groupName = "Main Menu",
    groupId = "main_menu",
    groupType = ButtonGroupManager.ButtonGroupType.Vertical,
    buttonCount = 4,
    autoGenerateButtons = true,
    commandCategories = new[] { CommandCategory.GameManagement, CommandCategory.SaveLoad }
};

var group = controller.CreateButtonGroup(config);
```

### 2. State-Based Visibility

```csharp
// Configure groups for different states
var mainMenuConfig = new ButtonGroupController.ButtonGroupConfiguration
{
    groupName = "Main Menu Buttons",
    visibleInStates = new List<ButtonGroupController.GameState> { GameState.MainMenu },
    hideInOtherStates = true
};

var inGameConfig = new ButtonGroupController.ButtonGroupConfiguration
{
    groupName = "In-Game HUD",
    visibleInStates = new List<ButtonGroupController.GameState> { GameState.InGame },
    hideInOtherStates = true
};

// Switch states to show different groups
controller.SetGameState(GameState.MainMenu); // Shows main menu buttons
controller.SetGameState(GameState.InGame);   // Shows in-game HUD
```

### 3. Custom Button Configuration

```csharp
var customConfig = new ButtonGroupController.ButtonGroupConfiguration
{
    groupName = "Custom Buttons",
    autoGenerateButtons = false,
    buttons = new List<ButtonData>
    {
        new ButtonData { buttonText = "Save Game", command = "save", behavior = ButtonBehavior.Standard },
        new ButtonData { buttonText = "Load Game", command = "load", behavior = ButtonBehavior.Standard },
        new ButtonData { buttonText = "Quit", command = "quit", behavior = ButtonBehavior.Standard }
    }
};
```

## Configuration Options

### ButtonGroupConfiguration

| Property | Type | Description |
|----------|------|-------------|
| `groupName` | string | Display name for the group |
| `groupId` | string | Unique identifier for retrieval |
| `groupType` | ButtonGroupType | Layout type (Horizontal, Vertical, Grid, Radial) |
| `buttonCount` | int | Number of buttons to create |
| `autoGenerateButtons` | bool | Generate buttons from command categories |
| `commandCategories` | CommandCategory[] | Categories for auto-generation |
| `visibleInStates` | List<GameState> | Game states where group is visible |
| `hideInOtherStates` | bool | Hide group in non-specified states |
| `overrideGlobalLayout` | bool | Use custom layout settings |
| `overrideGlobalVisuals` | bool | Use custom visual settings |
| `overrideGlobalBehavior` | bool | Use custom behavior settings |

### Global Settings

| Property | Type | Description |
|----------|------|-------------|
| `defaultButtonSize` | Vector2 | Default size for all buttons |
| `defaultSpacing` | float | Default spacing between buttons |
| `globalVisuals` | ButtonVisualSettings | Global visual styling |
| `globalEnableAutoHandler` | bool | Enable auto-handler on all buttons |
| `globalEnableAnimations` | bool | Enable animations globally |

## Command Categories

The system supports automatic button generation from predefined command categories:

### Available Categories
- **GameManagement**: quit, exit, restart, new game
- **SaveLoad**: save, load, quick save, quick load, auto save
- **Navigation**: main menu, back, close, return, cancel
- **GameState**: pause, resume, unpause
- **Settings**: settings, apply, reset, defaults
- **Audio**: mute, unmute, volume controls
- **Population**: spawn pop, add pop, remove pop
- **Resources**: add food, add wood, add faith, add stone
- **Debug**: debug, console, logs, stats

### Custom Commands
You can also define custom commands with parameters:
- "Add 50 Food" - Adds specific amount of food
- "Spawn 5 Pop" - Spawns specific number of population
- "Set Volume 0.8" - Sets volume to specific level

## Event System

The controller provides comprehensive events for monitoring and responding to changes:

```csharp
// Subscribe to events
controller.OnGroupCreated += (group) => Debug.Log($"Group created: {group.name}");
controller.OnGroupDestroyed += (group) => Debug.Log($"Group destroyed: {group.name}");
controller.OnGlobalButtonClicked += (text) => Debug.Log($"Button clicked: {text}");
controller.OnGameStateChanged += (state) => Debug.Log($"State changed: {state}");
controller.OnAllGroupsInitialized += () => Debug.Log("All groups ready");
```

## Advanced Features

### Object Pooling
Enable object pooling for better performance with large numbers of buttons:

```csharp
controller.enableObjectPooling = true;
controller.maxPooledButtons = 100;
```

### Batch Operations
Perform operations on multiple groups simultaneously:

```csharp
// Apply global settings to all groups
controller.ApplyGlobalSettings();

// Refresh all groups
controller.RefreshAllGroups();

// Destroy all groups
controller.DestroyAllGroups();
```

### Dynamic Group Management
Add and remove groups at runtime:

```csharp
// Create new group dynamically
var dynamicGroup = controller.CreateButtonGroup(newConfig);

// Remove specific group
controller.DestroyButtonGroup(specificGroup);

// Get group by ID
var group = controller.GetGroupById("main_menu");
```

### Keyboard Navigation
Enable keyboard navigation for accessibility:

```csharp
controller.enableGlobalHotkeys = true;
// F1-F3: Switch game states
// F9: Refresh all groups
// F10: Toggle all groups visibility
```

## Performance Considerations

### Best Practices
1. **Use Object Pooling**: Enable for projects with many dynamic buttons
2. **Batch Operations**: Apply global changes in batches rather than individually
3. **State Management**: Use state-based visibility to reduce active button count
4. **Lazy Loading**: Enable lazy loading for large projects
5. **Limit Group Count**: Keep the number of simultaneous groups reasonable

### Optimization Features
- **Object pooling** for button reuse
- **Batch operations** for multiple group updates
- **Lazy loading** for deferred group creation
- **State-based visibility** to reduce rendering overhead
- **Content size fitting** for automatic layout optimization

## Testing and Debugging

### Built-in Testing
Use the `CompleteButtonSystemTest` component for comprehensive testing:

```csharp
var tester = gameObject.AddComponent<CompleteButtonSystemTest>();
tester.runTestsOnStart = true;
tester.enableVisualTests = true;
tester.enablePerformanceTests = true;
```

### Debug Features
Enable debug mode for detailed logging:

```csharp
controller.enableDebugEvents = true;
// Provides detailed logs for all operations
```

### Demo Scenes
Use the `ButtonGroupControllerDemo` for interactive demonstrations:

```csharp
var demo = gameObject.AddComponent<ButtonGroupControllerDemo>();
demo.runDemoOnStart = true;
demo.enableKeyboardShortcuts = true;
```

## Integration with Existing Systems

### Save/Load Integration
The system automatically integrates with your SaveManager:

```csharp
// Buttons with "save" text automatically call SaveManager.SaveGame()
// Buttons with "load" text automatically call SaveManager.LoadGame()
```

### Resource Management
Integrates with ResourceManager for resource commands:

```csharp
// "Add Food" buttons call ResourceManager.AddFood()
// "Add Wood" buttons call ResourceManager.AddWood()
```

### Audio System
Integrates with AudioManager for sound controls:

```csharp
// "Mute" buttons call AudioManager.Mute()
// "Set Volume" buttons adjust AudioManager volume
```

## Troubleshooting

### Common Issues

1. **Buttons not responding**
   - Verify Canvas has GraphicRaycaster
   - Check button interactable state
   - Ensure EnhancedAutoButtonHandler is attached

2. **Layout not updating**
   - Call `UpdateButtonGroup()` after configuration changes
   - Verify layout components are properly configured
   - Check for conflicting layout settings

3. **State switching not working**
   - Verify groups have correct `visibleInStates` configuration
   - Check `hideInOtherStates` setting
   - Ensure game state is actually changing

4. **Performance issues**
   - Enable object pooling
   - Reduce number of simultaneous groups
   - Use state-based visibility
   - Enable batch operations

### Debug Commands
Access debug information through the console:

```csharp
// Get all groups
var groups = controller.GetAllGroups();

// Get groups for specific state
var stateGroups = controller.GetGroupsForState(GameState.InGame);

// Check group configuration
var config = controller.GetConfigurationForGroup(specificGroup);
```

## API Reference

### ButtonGroupController Methods

| Method | Description |
|--------|-------------|
| `CreateButtonGroup(config)` | Creates a new button group |
| `DestroyButtonGroup(group)` | Destroys a specific group |
| `GetGroupById(id)` | Retrieves group by ID |
| `GetGroupByName(name)` | Retrieves group by name |
| `GetAllGroups()` | Gets all managed groups |
| `SetGameState(state)` | Changes current game state |
| `ApplyGlobalSettings()` | Applies global settings to all groups |
| `RefreshAllGroups()` | Refreshes all group layouts |
| `DestroyAllGroups()` | Destroys all groups |

### ButtonGroupController Properties

| Property | Type | Description |
|----------|------|-------------|
| `initializeOnStart` | bool | Initialize groups on Start() |
| `autoManageGroups` | bool | Automatically manage group lifecycle |
| `enableGlobalHotkeys` | bool | Enable keyboard shortcuts |
| `enableObjectPooling` | bool | Use object pooling for performance |
| `maxPooledButtons` | int | Maximum buttons in pool |
| `globalSettings` | GlobalButtonSettings | Global configuration settings |

## Examples

### Complete Implementation Example

```csharp
using UnityEngine;
using Lineage.Ancestral.Legacies.UI;

public class GameUIManager : MonoBehaviour
{
    private ButtonGroupController buttonController;
    
    private void Start()
    {
        SetupButtonController();
        CreateMainMenuButtons();
        CreateInGameButtons();
        CreateSettingsButtons();
    }
    
    private void SetupButtonController()
    {
        var controllerObj = new GameObject("UI Button Controller");
        buttonController = controllerObj.AddComponent<ButtonGroupController>();
        
        // Configure global settings
        buttonController.globalSettings.defaultButtonSize = new Vector2(150, 50);
        buttonController.globalSettings.defaultSpacing = 15f;
        buttonController.globalSettings.globalEnableAnimations = true;
        
        // Subscribe to events
        buttonController.OnGlobalButtonClicked += OnButtonClicked;
        buttonController.OnGameStateChanged += OnGameStateChanged;
    }
    
    private void CreateMainMenuButtons()
    {
        var config = new ButtonGroupController.ButtonGroupConfiguration
        {
            groupName = "Main Menu",
            groupId = "main_menu",
            groupType = ButtonGroupManager.ButtonGroupType.Vertical,
            buttonCount = 5,
            autoGenerateButtons = true,
            commandCategories = new[] { 
                CommandCategory.GameManagement, 
                CommandCategory.SaveLoad 
            },
            visibleInStates = new List<ButtonGroupController.GameState> { 
                ButtonGroupController.GameState.MainMenu 
            },
            position = new Vector3(0, 0, 0)
        };
        
        buttonController.CreateButtonGroup(config);
    }
    
    private void CreateInGameButtons()
    {
        var config = new ButtonGroupController.ButtonGroupConfiguration
        {
            groupName = "Game HUD",
            groupId = "game_hud",
            groupType = ButtonGroupManager.ButtonGroupType.Horizontal,
            buttonCount = 6,
            autoGenerateButtons = true,
            commandCategories = new[] { 
                CommandCategory.GameState, 
                CommandCategory.Resources, 
                CommandCategory.Population 
            },
            visibleInStates = new List<ButtonGroupController.GameState> { 
                ButtonGroupController.GameState.InGame 
            },
            position = new Vector3(0, 300, 0)
        };
        
        buttonController.CreateButtonGroup(config);
    }
    
    private void CreateSettingsButtons()
    {
        var config = new ButtonGroupController.ButtonGroupConfiguration
        {
            groupName = "Settings Panel",
            groupId = "settings",
            groupType = ButtonGroupManager.ButtonGroupType.Grid,
            buttonCount = 8,
            autoGenerateButtons = false,
            buttons = new List<ButtonData>
            {
                new ButtonData { buttonText = "Graphics", command = "graphics_settings" },
                new ButtonData { buttonText = "Audio", command = "audio_settings" },
                new ButtonData { buttonText = "Controls", command = "control_settings" },
                new ButtonData { buttonText = "Gameplay", command = "gameplay_settings" },
                new ButtonData { buttonText = "Apply", command = "apply_settings" },
                new ButtonData { buttonText = "Reset", command = "reset_settings" },
                new ButtonData { buttonText = "Defaults", command = "default_settings" },
                new ButtonData { buttonText = "Back", command = "back" }
            },
            visibleInStates = new List<ButtonGroupController.GameState> { 
                ButtonGroupController.GameState.Settings 
            }
        };
        
        buttonController.CreateButtonGroup(config);
    }
    
    private void OnButtonClicked(string buttonText)
    {
        Debug.Log($"Button clicked: {buttonText}");
        
        // Handle special cases
        switch (buttonText.ToLower())
        {
            case "start game":
                buttonController.SetGameState(ButtonGroupController.GameState.InGame);
                break;
            case "settings":
                buttonController.SetGameState(ButtonGroupController.GameState.Settings);
                break;
            case "main menu":
                buttonController.SetGameState(ButtonGroupController.GameState.MainMenu);
                break;
        }
    }
    
    private void OnGameStateChanged(ButtonGroupController.GameState newState)
    {
        Debug.Log($"Game state changed to: {newState}");
        
        // Perform state-specific logic
        switch (newState)
        {
            case ButtonGroupController.GameState.InGame:
                // Start game logic
                break;
            case ButtonGroupController.GameState.MainMenu:
                // Main menu logic
                break;
            case ButtonGroupController.GameState.Settings:
                // Settings logic
                break;
        }
    }
    
    private void Update()
    {
        // Handle hotkeys
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (buttonController.currentGameState == ButtonGroupController.GameState.InGame)
            {
                buttonController.SetGameState(ButtonGroupController.GameState.MainMenu);
            }
        }
    }
}
```

## Conclusion

The Complete Button System provides a robust, scalable solution for managing UI buttons in Unity projects. With its three-layer architecture, comprehensive feature set, and focus on performance and usability, it eliminates the need to manage individual button components while providing full control over button group behavior and appearance.

Whether you're building a simple menu system or a complex UI with state-dependent button groups, this system provides the tools and flexibility needed to create professional, maintainable button interfaces.

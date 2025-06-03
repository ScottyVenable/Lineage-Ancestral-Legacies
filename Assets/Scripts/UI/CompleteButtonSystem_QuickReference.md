# Complete Button System - Quick Reference

## 🚀 Quick Setup

### 1. Create Controller
```csharp
var controller = gameObject.AddComponent<ButtonGroupController>();
```

### 2. Create Button Group
```csharp
var config = new ButtonGroupController.ButtonGroupConfiguration
{
    groupName = "My Buttons",
    groupId = "my_group",
    buttonCount = 4,
    autoGenerateButtons = true,
    commandCategories = new[] { CommandCategory.GameManagement }
};
var group = controller.CreateButtonGroup(config);
```

### 3. Handle Events
```csharp
controller.OnGlobalButtonClicked += (text) => Debug.Log($"Clicked: {text}");
```

## 📋 Command Categories

| Category | Commands |
|----------|----------|
| **GameManagement** | quit, exit, restart, new game |
| **SaveLoad** | save, load, quick save, quick load |
| **Navigation** | main menu, back, close, return |
| **GameState** | pause, resume, unpause |
| **Settings** | settings, apply, reset, defaults |
| **Audio** | mute, unmute, volume up/down |
| **Population** | spawn pop, add pop, remove pop |
| **Resources** | add food/wood/faith/stone |
| **Debug** | debug, console, logs, stats |

## 🎮 Game States

```csharp
controller.SetGameState(ButtonGroupController.GameState.MainMenu);
controller.SetGameState(ButtonGroupController.GameState.InGame);
controller.SetGameState(ButtonGroupController.GameState.Settings);
controller.SetGameState(ButtonGroupController.GameState.Paused);
```

## 🎨 Layout Types

```csharp
groupType = ButtonGroupManager.ButtonGroupType.Horizontal;  // Side by side
groupType = ButtonGroupManager.ButtonGroupType.Vertical;    // Top to bottom
groupType = ButtonGroupManager.ButtonGroupType.Grid;        // Grid layout
groupType = ButtonGroupManager.ButtonGroupType.Radial;      // Circular layout
```

## ⚡ Common Operations

### Create State-Specific Groups
```csharp
var config = new ButtonGroupController.ButtonGroupConfiguration
{
    groupName = "Main Menu",
    visibleInStates = new List<GameState> { GameState.MainMenu },
    hideInOtherStates = true
};
```

### Custom Button Configuration
```csharp
config.autoGenerateButtons = false;
config.buttons = new List<ButtonData>
{
    new ButtonData { buttonText = "Custom", command = "my_command" }
};
```

### Apply Global Styling
```csharp
controller.globalSettings.defaultButtonSize = new Vector2(150, 50);
controller.globalSettings.globalVisuals.normalColor = Color.blue;
controller.ApplyGlobalSettings();
```

### Dynamic Group Management
```csharp
// Create
var group = controller.CreateButtonGroup(config);

// Destroy
controller.DestroyButtonGroup(group);

// Get by ID
var group = controller.GetGroupById("my_group");

// Get all
var allGroups = controller.GetAllGroups();
```

## 🔧 Key Properties

### ButtonGroupConfiguration
- `groupName` - Display name
- `groupId` - Unique identifier
- `buttonCount` - Number of buttons
- `autoGenerateButtons` - Auto-create from categories
- `visibleInStates` - When to show group
- `overrideGlobalLayout/Visuals/Behavior` - Custom settings

### Global Settings
- `defaultButtonSize` - Button dimensions
- `defaultSpacing` - Space between buttons
- `globalEnableAutoHandler` - Auto-attach handlers
- `globalEnableAnimations` - Animation system

## 🎯 Events

```csharp
controller.OnGroupCreated += (group) => { };
controller.OnGroupDestroyed += (group) => { };
controller.OnGlobalButtonClicked += (text) => { };
controller.OnGameStateChanged += (state) => { };
controller.OnAllGroupsInitialized += () => { };
```

## ⌨️ Default Hotkeys

- **F1** - Main Menu state
- **F2** - In Game state  
- **F3** - Settings state
- **F9** - Refresh all groups
- **F10** - Toggle all groups visibility

## 🛠️ Performance Tips

1. **Enable Object Pooling**
   ```csharp
   controller.enableObjectPooling = true;
   controller.maxPooledButtons = 100;
   ```

2. **Use State-Based Visibility**
   ```csharp
   config.visibleInStates = new List<GameState> { GameState.InGame };
   config.hideInOtherStates = true;
   ```

3. **Batch Operations**
   ```csharp
   controller.ApplyGlobalSettings(); // Apply to all at once
   controller.RefreshAllGroups();    // Refresh all at once
   ```

## 🐛 Debug & Testing

### Enable Debug Mode
```csharp
controller.enableDebugEvents = true;
```

### Run Tests
```csharp
var tester = gameObject.AddComponent<CompleteButtonSystemTest>();
tester.runTestsOnStart = true;
```

### Demo Scene
```csharp
var demo = gameObject.AddComponent<ButtonGroupControllerDemo>();
demo.runDemoOnStart = true;
```

## 🔗 Integration Examples

### With Save System
```csharp
// Buttons with "save" text automatically call SaveManager.SaveGame()
// Buttons with "load" text automatically call SaveManager.LoadGame()
```

### With Resource System
```csharp
// "Add 50 Food" automatically calls ResourceManager.AddFood(50)
// "Add Wood" automatically calls ResourceManager.AddWood()
```

### Custom Commands
```csharp
// In EnhancedAutoButtonHandler, add custom actions:
buttonActions["my_command"] = () => { /* custom logic */ };
```

## 📱 Platform Support

- ✅ Windows
- ✅ macOS  
- ✅ Linux
- ✅ iOS
- ✅ Android
- ✅ WebGL
- ✅ Console Platforms

## 🚨 Common Issues

1. **Buttons not clickable** - Check Canvas has GraphicRaycaster
2. **Layout not updating** - Call `UpdateButtonGroup()` after changes
3. **Groups not switching** - Verify `visibleInStates` configuration
4. **Performance slow** - Enable object pooling and state-based visibility

## 📚 Full Documentation

For complete documentation, see `CompleteButtonSystem_Documentation.md`

---

*Quick Reference v1.0 - Complete Button System*

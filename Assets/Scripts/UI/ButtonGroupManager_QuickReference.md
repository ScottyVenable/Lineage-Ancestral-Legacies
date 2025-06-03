# ButtonGroupManager - Quick Reference Card

## 🚀 Quick Setup
```csharp
// 1. Add component
var manager = gameObject.AddComponent<ButtonGroupManager>();

// 2. Configure basic settings
manager.buttonCount = 4;
manager.groupType = ButtonGroupType.Horizontal;
manager.buttonSpacing = 10f;

// 3. Auto-generate or manual config
manager.autoGenerateFromCommands = true; // OR
manager.buttonConfigs = new List<ButtonData> { /* your buttons */ };
```

## 🎯 Layout Types
| Type | Description | Use Case |
|------|-------------|----------|
| `Horizontal` | Row layout | Main menu, toolbar |
| `Vertical` | Column layout | Side panels, lists |
| `Grid` | Grid pattern | Icon grids, categories |
| `Radial` | Circle layout | Radial menus |
| `Custom` | Manual control | Special layouts |

## 🔘 Button Behaviors
| Behavior | Description | Example |
|----------|-------------|---------|
| `Standard` | Normal click | Action buttons |
| `Toggle` | On/off state | Show/hide panels |
| `RadioButton` | One selection | Difficulty levels |
| `Momentary` | Active while pressed | Hold to sprint |
| `Disabled` | No interaction | Locked features |

## 📋 Command Categories
```csharp
CommandCategory.GameManagement    // quit, restart, new game
CommandCategory.SaveLoad         // save, load, quick save
CommandCategory.Navigation       // main menu, back, close
CommandCategory.GameState        // pause, resume
CommandCategory.Settings         // settings, apply, reset
CommandCategory.Audio           // mute, unmute
CommandCategory.Population      // spawn pop, add pop
CommandCategory.Resources       // add food, add wood
CommandCategory.Debug           // debug, console
```

## 🎨 Visual Styling
```csharp
manager.visualSettings.normalColor = Color.white;
manager.visualSettings.highlightedColor = Color.gray;
manager.visualSettings.pressedColor = Color.darkGray;
manager.visualSettings.fontSize = 14;
manager.visualSettings.textColor = Color.black;
```

## 🎭 Animation Settings
```csharp
manager.enableAnimations = true;
manager.animationSettings.enableHoverAnimation = true;
manager.animationSettings.hoverScale = new Vector3(1.1f, 1.1f, 1f);
manager.animationSettings.animationDuration = 0.2f;
```

## 🎯 Essential Properties
```csharp
// Core Settings
buttonCount                 // Number of buttons
groupType                  // Layout type
buttonSize                 // Individual button size
buttonSpacing              // Space between buttons
autoSize                   // Auto-size buttons

// Content
buttonConfigs              // Manual button list
autoGenerateFromCommands   // Auto-create from categories
includeCategories          // Which command categories

// Behavior
enableButtonInteraction    // Global enable/disable
enableKeyboardNavigation   // Arrow key navigation
enableButtonGroups         // Radio button grouping
allowMultipleSelection     // Multiple toggles

// Effects
enableAnimations           // Hover/click animations
enableSoundEffects         // Audio feedback
enableVisualFeedback       // Color feedback
```

## 🎮 Runtime Management
```csharp
// Add button
manager.AddButton(new ButtonData { 
    buttonText = "New", 
    command = "action" 
});

// Modify existing
manager.SetButtonText(0, "Updated");
manager.SetButtonEnabled(1, false);
manager.RemoveButton(2);

// Access buttons
var buttons = manager.GetManagedButtons();
var button = manager.GetButton(0);

// Refresh layout
manager.RefreshButtonGroup();
```

## 📡 Event Handling
```csharp
manager.OnButtonCreated += (button) => Debug.Log("Created: " + button.name);
manager.OnButtonClicked += (text) => Debug.Log("Clicked: " + text);
manager.OnButtonDestroyed += (button) => Debug.Log("Destroyed: " + button.name);
manager.OnLayoutChanged += () => Debug.Log("Layout changed");
```

## 🔧 ButtonData Structure
```csharp
new ButtonData {
    buttonText = "Display Text",      // Button label
    command = "command name",         // Auto-handler command
    isEnabled = true,                 // Interactable state
    behavior = ButtonBehavior.Standard, // Button behavior
    customIcon = sprite,              // Custom icon
    customColor = Color.red,          // Custom color
    customSize = new Vector2(100,40), // Custom size
    tooltip = "Help text",            // Tooltip text
    shortcutKey = KeyCode.F1,         // Keyboard shortcut
    isToggled = false,               // Toggle state
    customAction = () => { }         // Custom callback
};
```

## 🏗️ Common Patterns

### Main Menu
```csharp
manager.groupType = ButtonGroupType.Vertical;
manager.autoGenerateFromCommands = true;
manager.includeCategories = new[] { 
    CommandCategory.GameManagement 
};
```

### Game HUD
```csharp
manager.groupType = ButtonGroupType.Horizontal;
manager.buttonSize = new Vector2(80, 30);
manager.includeCategories = new[] { 
    CommandCategory.SaveLoad, 
    CommandCategory.GameState 
};
```

### Settings Panel
```csharp
manager.groupType = ButtonGroupType.Grid;
manager.enableButtonGroups = true;
manager.allowMultipleSelection = true;
```

### Radio Group
```csharp
manager.groupType = ButtonGroupType.Vertical;
manager.enableButtonGroups = true;
manager.allowMultipleSelection = false;
// Set all buttons to ButtonBehavior.RadioButton
```

## 🎨 Styling Shortcuts
```csharp
// Game theme
manager.visualSettings.normalColor = new Color(0.2f, 0.3f, 0.5f);
manager.visualSettings.highlightedColor = new Color(0.3f, 0.4f, 0.6f);

// Success theme  
manager.visualSettings.normalColor = new Color(0.2f, 0.6f, 0.2f);

// Warning theme
manager.visualSettings.normalColor = new Color(0.8f, 0.6f, 0.2f);

// Danger theme
manager.visualSettings.normalColor = new Color(0.8f, 0.2f, 0.2f);
```

## ⚡ Performance Tips
- Keep button count ≤ 8 per group
- Disable animations on low-end devices
- Use object pooling for dynamic groups
- Group related buttons together
- Cache button references when accessing frequently

## 🐛 Quick Debug
```csharp
manager.enableDebugMode = true;      // Log button creation
manager.logButtonActions = true;     // Log button clicks
manager.showButtonBounds = true;     // Visual bounds
```

## 🔗 Integration Commands
All EnhancedAutoButtonHandler commands work automatically:
- **save**, **load**, **quick save**, **quick load**
- **pause**, **resume**, **quit**, **restart**
- **add food**, **add wood**, **spawn pop**
- **mute**, **unmute**, **main menu**, **back**
- **settings**, **apply**, **reset**, **close**

## 🎯 One-Liner Examples
```csharp
// Horizontal save/load buttons
var saveLoad = obj.AddComponent<ButtonGroupManager>();
saveLoad.autoGenerateFromCommands = true;
saveLoad.includeCategories = new[] { CommandCategory.SaveLoad };

// Vertical toggle menu
var menu = obj.AddComponent<ButtonGroupManager>();
menu.groupType = ButtonGroupType.Vertical;
menu.allowMultipleSelection = true;

// Game control toolbar
var toolbar = obj.AddComponent<ButtonGroupManager>();
toolbar.groupType = ButtonGroupType.Horizontal;
toolbar.includeCategories = new[] { CommandCategory.GameManagement };
```

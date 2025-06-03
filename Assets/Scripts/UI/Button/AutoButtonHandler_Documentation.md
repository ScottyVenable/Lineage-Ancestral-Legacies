# AutoButtonHandler Documentation

## Overview
The `AutoButtonHandler` is a smart button component that automatically detects button functionality based on TMP_Text labels and executes appropriate logic. Simply attach this script to any button prefab, and it will automatically execute the corresponding action based on the button's text content.

## Features
- **Automatic Detection**: Reads TMP_Text labels and maps them to predefined actions
- **Case Insensitive**: Configurable case-insensitive matching
- **Flexible Configuration**: Trimming whitespace, logging options
- **Runtime Extensibility**: Add/remove actions dynamically
- **Comprehensive Actions**: Pre-built actions for common game functionality
- **Error Handling**: Robust error handling with detailed logging

## Setup Instructions

### Basic Setup
1. Create a button in your UI hierarchy
2. Ensure the button has a TMP_Text component as a child
3. Attach the `AutoButtonHandler` script to the button GameObject
4. Set the TMP_Text content to any of the supported action names

### Configuration Options
- **Ignore Case**: Whether to ignore text case when matching (default: true)
- **Trim Whitespace**: Whether to trim whitespace from button text (default: true)
- **Enable Logging**: Whether to log button actions for debugging (default: true)

## Supported Actions

### Game Management
- "quit", "exit", "quit game", "exit game" → Quit the application
- "restart" → Restart current scene
- "new game", "start game" → Start a new game

### Save/Load System
- "save", "save game" → Save the current game
- "load", "load game" → Load a saved game
- "quick save" → Quick save functionality
- "quick load" → Quick load functionality
- "auto save" → Trigger auto save

### Menu Navigation
- "main menu", "menu" → Return to main menu
- "back", "return", "cancel" → Go back/close current UI
- "close" → Close current UI panel

### Game State
- "pause" → Pause the game
- "resume", "unpause" → Resume the game

### Settings
- "settings", "options" → Open settings menu
- "apply" → Apply current settings
- "reset", "defaults" → Reset settings to defaults

### Audio
- "mute", "unmute" → Toggle audio mute

### Population Management
- "spawn pop", "add pop", "create pop" → Spawn new population

### Resource Management
- "add food" → Add food resources
- "add wood" → Add wood resources
- "add faith" → Add faith resources

### Debug
- "debug", "console", "debug menu" → Toggle debug functionality

## Usage Examples

### Example 1: Simple Save Button
```
Button GameObject
├── AutoButtonHandler (script)
└── Text (TMP_Text): "Save Game"
```

### Example 2: Resource Addition Button
```
Button GameObject
├── AutoButtonHandler (script)
└── Text (TMP_Text): "Add Food"
```

### Example 3: Navigation Button
```
Button GameObject
├── AutoButtonHandler (script)
└── Text (TMP_Text): "Main Menu"
```

## Runtime API

### Adding Custom Actions
```csharp
autoButtonHandler.AddButtonAction("custom action", () => {
    // Your custom logic here
});
```

### Removing Actions
```csharp
autoButtonHandler.RemoveButtonAction("custom action");
```

### Checking Available Actions
```csharp
bool hasAction = autoButtonHandler.HasAction("save game");
```

## Advanced Usage

### Custom Action Implementation
You can extend the functionality by adding custom actions at runtime:

```csharp
public class CustomButtonSetup : MonoBehaviour
{
    private void Start()
    {
        var autoHandler = GetComponent<AutoButtonHandler>();
        
        // Add custom action
        autoHandler.AddButtonAction("special ability", () => {
            // Custom special ability logic
            Debug.Log("Special ability activated!");
        });
        
        // Add parameterized action
        autoHandler.AddButtonAction("heal player", () => {
            PlayerHealth.Instance.Heal(50);
        });
    }
}
```

### Integration with Existing Systems
The AutoButtonHandler automatically integrates with your existing manager systems:
- `SaveManager.Instance` for save/load operations
- `PopulationManager.Instance` for population management
- `ResourceManager.Instance` for resource operations
- `AudioManager.Instance` for audio controls

## Best Practices

1. **Consistent Naming**: Use clear, descriptive text labels that match the expected actions
2. **Case Insensitive**: Don't worry about exact case matching - the system handles it automatically
3. **Logging**: Keep logging enabled during development to debug button actions
4. **Custom Actions**: Add custom actions for game-specific functionality
5. **Error Handling**: The system gracefully handles missing managers and invalid actions

## Troubleshooting

### Common Issues
1. **No TMP_Text Found**: Ensure the button has a TMP_Text component as a child
2. **Action Not Found**: Check that the button text matches one of the supported actions
3. **Manager Not Found**: Ensure the required manager instances exist in the scene

### Debug Tips
- Enable logging to see what actions are being triggered
- Use the editor validation feature to see available actions
- Check the console for detailed error messages

## Dependencies
- Unity UI System
- TextMeshPro (TMP_Text)
- Your game's manager systems (SaveManager, PopulationManager, etc.)
- Custom logging system (Log class)

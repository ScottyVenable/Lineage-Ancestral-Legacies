# EnhancedAutoButtonHandler Command Reference

## Quick Reference Card
This document lists all available commands that can be used with the EnhancedAutoButtonHandler. Simply set your button's TMP_Text to any of these commands and it will automatically work!

## Basic Commands (Case Insensitive)

### 🎮 Game Management
| Command | Alternative | Action |
|---------|-------------|--------|
| `quit` | `exit` | Exit the application |
| `quit game` | `exit game` | Exit with full command |

### 💾 Save/Load System
| Command | Action |
|---------|--------|
| `save` | Save current game |
| `save game` | Save with full command |
| `load` | Load saved game |
| `load game` | Load with full command |
| `quick save` | Quick save functionality |
| `quick load` | Quick load functionality |
| `auto save` | Trigger auto save |

### 🧭 Menu Navigation
| Command | Alternative | Action |
|---------|-------------|--------|
| `main menu` | `menu` | Return to main menu |
| `back` | `return`, `cancel` | Go back/close UI |
| `close` | | Close current UI panel |

### ⏯️ Game State Control
| Command | Alternative | Action |
|---------|-------------|--------|
| `pause` | | Pause the game |
| `resume` | `unpause` | Resume the game |
| `restart` | | Restart current scene |
| `new game` | `start game` | Start new game |

### ⚙️ Settings Management
| Command | Alternative | Action |
|---------|-------------|--------|
| `settings` | `options` | Open settings menu |
| `apply` | | Apply current settings |
| `reset` | `defaults` | Reset to defaults |

### 🔊 Audio Controls
| Command | Alternative | Action |
|---------|-------------|--------|
| `mute` | `unmute` | Toggle audio mute |

### 👥 Population Management
| Command | Alternative | Action |
|---------|-------------|--------|
| `spawn pop` | `add pop`, `create pop` | Spawn new population |

### 📦 Resource Management
| Command | Action |
|---------|--------|
| `add food` | Add 10 food resources |
| `add wood` | Add 10 wood resources |
| `add faith` | Add 10 faith resources |

### 🐛 Debug Functions
| Command | Alternative | Action |
|---------|-------------|--------|
| `debug` | `console`, `debug menu` | Toggle debug mode |

## Enhanced Commands (Parameterized)

### 📊 Resource Commands with Amounts
| Pattern | Example | Action |
|---------|---------|--------|
| `add [amount] [resource]` | `add 50 food` | Add specific amount of resource |
| `add [amount] [resource]` | `add 100 wood` | Add 100 wood |
| `add [amount] [resource]` | `add 25 faith` | Add 25 faith |

### 👥 Population Commands with Counts
| Pattern | Example | Action |
|---------|---------|--------|
| `spawn [count] [type]` | `spawn 5 pop` | Spawn multiple units |
| `spawn [count] [type]` | `spawn 10 population` | Spawn 10 population |

### ⚙️ System Settings Commands
| Pattern | Example | Action |
|---------|---------|--------|
| `set [property] to [value]` | `set volume to 80` | Set audio volume to 80% |
| `set [property] to [value]` | `set timescale to 2` | Set game speed to 2x |

### 🗺️ Scene Loading Commands
| Pattern | Example | Action |
|---------|---------|--------|
| `load scene [index]` | `load scene 1` | Load scene by index |
| `load scene [name]` | `load scene MainGame` | Load scene by name |

## Usage Examples

### Basic Usage
```csharp
// Create a button with TMP_Text
Button myButton;
TMP_Text buttonText = myButton.GetComponentInChildren<TMP_Text>();
buttonText.text = "Save Game"; // Will automatically save when clicked

// Add the handler
myButton.gameObject.AddComponent<EnhancedAutoButtonHandler>();
```

### Custom Actions
```csharp
var handler = GetComponent<EnhancedAutoButtonHandler>();

// Add custom basic action
handler.AddCustomAction("heal player", () => {
    PlayerHealth.Instance.Heal(50);
});

// Add parameterized action
handler.AddParameterizedAction("teleport", (parameters) => {
    if (parameters.Length >= 2) {
        float x = float.Parse(parameters[0]);
        float y = float.Parse(parameters[1]);
        Player.Instance.Teleport(x, y);
    }
});

// Add regex pattern action
handler.AddRegexAction(@"give (\w+) (\d+) (\w+)", (match) => {
    string player = match.Groups[1].Value;
    int amount = int.Parse(match.Groups[2].Value);
    string item = match.Groups[3].Value;
    GiveItemToPlayer(player, item, amount);
});
```

## Configuration Options

### Inspector Settings
- **Ignore Case**: Whether to ignore text case when matching (default: true)
- **Trim Whitespace**: Whether to trim whitespace from button text (default: true)
- **Enable Logging**: Whether to log button actions for debugging (default: true)
- **Use Regex Patterns**: Enable regex pattern matching for complex commands (default: false)
- **Enable Visual Feedback**: Show color feedback when buttons are pressed (default: true)

### Visual Feedback Settings
- **Success Color**: Color shown when action succeeds (default: green)
- **Error Color**: Color shown when action fails (default: red)
- **Feedback Duration**: How long to show feedback color (default: 1 second)

## Tips for Best Results

1. **Keep It Simple**: Use clear, descriptive text that matches the available commands
2. **Case Doesn't Matter**: "Save Game", "save game", and "SAVE GAME" all work
3. **Use Alternatives**: Many commands have multiple variations that work the same way
4. **Test Parameterized**: Enable regex patterns to use advanced parameterized commands
5. **Custom Actions**: Add your own commands for game-specific functionality
6. **Visual Feedback**: Keep feedback enabled to see when actions succeed or fail

## Troubleshooting

### Common Issues
- **No Action Found**: Check that button text exactly matches a command
- **Manager Not Found**: Ensure required managers (SaveManager, ResourceManager, etc.) exist
- **Regex Not Working**: Make sure "Use Regex Patterns" is enabled in inspector

### Debug Tips
- Enable logging to see what commands are being processed
- Check console for detailed error messages
- Use the demo scene to test all available commands
- Verify TMP_Text component exists as child of button

## Total Available Commands
- **Basic Commands**: 39 different text variations
- **Parameterized Commands**: Unlimited variations with patterns
- **Custom Commands**: Add your own at runtime
- **Regex Commands**: Complex pattern-based commands

The EnhancedAutoButtonHandler supports over 50+ built-in commands and can be extended with unlimited custom functionality!

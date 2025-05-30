## Refined Console Command Syntax Proposal

Here's a slightly adjusted syntax for executing commands in the CommandWindow, focusing on clarity and parsing:

**1. Command Structure:**
   `namespace.command [arg1, arg2, ...] {key1:value1, key2:"value with spaces", key3:[listVal1, listVal2], ...}`

**2. Namespaces:**
   * Use dots (`.`) to separate command groups (e.g., `entity`, `game`, `lineage`) and sub-commands/categories (e.g., `entity.inspect`, `entity.traits.add`, `game.time.set`). This allows for a hierarchical organization.

**3. Positional Arguments `[...]`:**
   * Enclose **required positional arguments** in a single pair of square brackets, separated by commas.
   * These are typically for specific targets (like an entity name/ID) or primary values.
   * If an argument string contains spaces, it **must** be enclosed in double quotes (e.g., `["My Pop Name", AnotherArg]`).
   * **Contextual Default:** For commands that target an entity (e.g., `entity.inspect`), if the bracketed argument is omitted AND a single entity is currently selected, the command will automatically target the selected entity.
   * Example: `entity.health [Grok, 50]` or `entity.health ["Elder Grok", full]`

**4. Data Block `{...}` (for Complex/Optional Named Parameters):**
   * Use curly braces for a set of **key-value pairs**, especially useful for spawning entities with multiple specific attributes or for commands with many optional named parameters.
   * Format: `{key1:value1, key2:"A String Value", key3:true, key4:[item1, "item two", 3]}`.
        * Keys are simple strings (no quotes needed if no spaces).
        * Values:
            * Numbers: `50`, `3.14`
            * Strings: Enclose in double quotes if they contain spaces or special characters (`"Skilled Crafter"`). Simple strings don't strictly need quotes (`male`).
            * Booleans: `true`, `false`
            * Arrays/Lists: `[value1, "value2", value3]`
   * The data block is generally optional. If a command supports it, it will parse the provided values.
   * Example: `spawn.pop [10,20,0] {name:"Steve", sex:male, traits:["Skilled Crafter", "Agile"], health:100}`

**5. Aliases:**
   * Short aliases for common commands are still a good idea (e.g., `quit` for `game.quit`).

**Applying this Refined Syntax to Your Examples:**

* **Inspect Entity:**
    * `entity.inspect ["Grok"]`
    * `entity.inspect` (If "Grok" or another single entity is selected)
* **Set Entity Health:**
    * `entity.health ["Grok", full]`
    * `entity.health ["Grok", 50]`
    * `entity.health [50]` (If an entity is selected)
* **Add Trait:**
    * `entity.traits.add ["Grok", "Skilled Crafter"]` (Trait name in quotes due to space)
    * `entity.traits.add ["Skilled Crafter"]` (If "Grok" is selected)
* **Remove Trait:**
    * `entity.traits.remove ["Grok", "Skilled Crafter"]`
* **Set Entity State:**
    * `entity.state.set ["Grok", Hauling]`
* **Show Game Data (Traits):**
    * `show.gamedata.traits` (Defaults to all)
    * `show.gamedata.traits [crafting]`
    * `show.gamedata.traits [all]`
* **Add Resources to Lineage:**
    * `lineage.resources.add [wood, 50]`
* **Add Items to Lineage Inventory:**
    * `lineage.inventory.add ["RedBerry", 4]` (Assuming "RedBerry" is the item ID)
* **Quit Game:**
    * `game.quit` (Alias: `quit`)
* **Time Manipulation:**
    * `game.time.add [50]`
    * `game.time.set [0]`
    * `game.time.set [morning]`
* **System Info:**
    * `system.info`
* **Spawn Pop with Data:**
    * `spawn.pop [10,20,0] {name:"Steve", sex:male, lineage:"DefaultTribe", traits:["Skilled Crafter", "FastRunner"], speed:50}`
        * `x,y,z` are positional.
        * The rest are in the data block.
    * `spawn.pop [mouse] {name:"Steve", sex:male}` (The keyword `mouse` can be parsed as a special location for the first positional arguments).
* **List Pops in Lineage:**
    * `lineage.pops.list` (Alias: `lineage.pops`)
* **Set Pop Cap for Lineage:**
    * `lineage.pops.set_cap [25]`
* **Inspect Entity Inventory:**
    * `entity.inventory.list ["Grok"]`
    * `entity.inventory.list` (If "Grok" is selected)
* **Add to Entity Inventory:**
    * `entity.inventory.add ["Grok", "RawMeat", 5]`
    * `entity.inventory.add ["RawMeat", 5]` (If "Grok" is selected)

This structure provides a good balance of power and readability. The comma-separated positional arguments are standard, and the `{}` block for key-value pairs is flexible for complex data.

---

## Updated Console Window Guide (Reflecting New Syntax)

**Version 1.1**

### 1. Introduction

Welcome, Ancestral Guide (and discerning Player)! The Console Window is a powerful tool integrated into *Lineage: Ancestral Legacies*. For developers, it's an indispensable debugging utility, allowing for real-time inspection and manipulation of game systems. For players, it can be a gateway to cheats, experimentation, and a deeper understanding of the game's mechanics (if enabled for release builds with cheats).

This document outlines its features, functionality, and common usage scenarios with the new command syntax.

### 2. Accessing the Console

* **Toggle Console:** Press the **F2** key to open or close the Console Window at any time during gameplay.
    * When opened, the input field will be automatically focused.

### 3. Core Features

* **Command Input & Execution:**
    * Type commands into the input field at the bottom.
    * Press **Enter** to execute.
* **Log Output:**
    * Displays command history, output, and game logs.
    * Supports markdown-like formatting (bold: `**text**`, italics: `*text*`) and colored text.
* **Command History:**
    * **Up/Down Arrows** navigate through previous commands.
* **Auto-Completion & Suggestions:**
    * **Tab** attempts auto-completion.
    * Suggestions may appear as you type.
* **Log Filtering:**
    * Use `log_filter [category]` (e.g., `AI`, `Combat`).
* **Window Management:**
    * Draggable header, resizable corner, minimize button.

### 4. Command Syntax Overview

Commands follow a `namespace.command [args...] {data...}` structure.

* **Namespaces:** Dots group related commands (e.g., `entity.traits.add`).
* **Positional Arguments `[...]`:**
    * Enclosed in `[]`, comma-separated (e.g., `[targetName, value]`).
    * Strings with spaces require double quotes: `["Elder Grok", 50]`.
    * **Contextual Target:** If a command expects an entity target as the first argument and it's omitted (e.g., `entity.inspect` instead of `entity.inspect [Grok]`), it will try to use the currently selected single entity.
* **Data Blocks `{...}`:**
    * For complex or multiple named parameters, enclosed in `{}`.
    * Format: `{key1:value, key2:"string value", key3:[arrayVal1, arrayVal2]}`.
    * Keys are typically unquoted. String values with spaces need quotes.
    * Example: `spawn.pop [10,20,0] {name:"Unga", health:80, traits:["Strong","Fast"]}`

### 5. Usage Scenarios

#### 5.1. Developer Debugging

* **Inspecting State:**
    * `system.info`: General system and performance data.
    * `entity.inspect ["PopName"]`: Detailed info for "PopName".
    * `entity.inspect`: Detailed info for the selected Pop.
    * `lineage.pops.list`: View all Pops in the current lineage.
    * `lineage.resources.show`: Display current global resources. (New name for `show_resources`)
* **Manipulating Game Systems:**
    * `game.time.set [18.5]`: Set game time to 6:30 PM.
    * `game.time.add [60]`: Advance game time by 60 units (e.g., minutes or configurable ticks).
    * `scene.load [MyTestScene]`: Load a specific scene.
    * `spawn.pop [15,25,0] {name:"TestPop", age:20}`: Spawn "TestPop" at coordinates.
    * `entity.state.set ["PopName", Idle]`: Force "PopName" to idle state.
* **Visual Debugging & Other Tools:**
    * The `DebugManager` provides toggles: F1 for help, F3 for Stats Overlay, F4 for Visual Debugger, F12 to toggle all debug systems.
    * `DebugManager` also has functions for drawing debug visuals like vision cones and paths, which can be triggered via specialized console commands if implemented (e.g., `debug.draw.vision [PopName]`).

#### 5.2. Player Cheats & Experimentation (If Enabled)

* **Resource & Item Cheats:**
    * `lineage.resources.add [food, 1000]`: Add 1000 food.
    * `entity.inventory.add ["SelectedPopName", "StoneAxe", 1]` (If "SelectedPopName" is selected, `entity.inventory.add ["StoneAxe", 1]`)
* **Pop Modification:**
    * `entity.health ["TargetPop", 999]`
    * `entity.needs.set ["TargetPop"] {hunger:100, thirst:100, energy:100}`
    * `entity.traits.add ["TargetPop", "SuperStrength"]`
* **Game Control:**
    * `game.timescale [2.0]`: Double game speed.
    * `entity.teleport ["PlayerCharacter", 100, 50, 0]` (If a direct player character exists).

### 6. Command Reference (Examples)

Type `help` in the console for a list of commands. Type `help [command.name]` for details.

* **General:**
    * `help [command.name]`: Shows help.
    * `clear`: Clears console output.
    * `game.quit` (alias: `quit`): Exits application.
    * `system.info`: Displays system info.
* **Entity Focused (`entity.`):**
    * `entity.inspect ["NameOrID"]` or `entity.inspect`
    * `entity.health ["NameOrID", value]` or `entity.health [value]`
    * `entity.traits.add ["NameOrID", "TraitID"]`
    * `entity.inventory.list ["NameOrID"]`
    * `entity.inventory.add ["NameOrID", "ItemID", quantity]`
* **Lineage Focused (`lineage.`):**
    * `lineage.resources.add [resourceType, amount]`
    * `lineage.pops.list`
    * `lineage.pops.set_cap [value]`
* **Spawning (`spawn.`):**
    * `spawn.pop [x,y,z] {data...}`
    * `spawn.pop [mouse] {data...}`
* **Game Systems (`game.`):**
    * `game.time.set [valueOrPreset]`
    * `game.timescale [value]`
* **Scene Management (`scene.`):**
    * `scene.load [sceneName]`
    * `scene.list`

*(This list would be expanded as commands are implemented.)*

### 7. Extending the Console (For Developers)

New commands can be added to `DebugConsoleManager.cs` using the `RegisterCommand(string command, string description, Action<string[]> handler)` method. The parsing logic within `ProcessCommand` will need to be updated to handle the new namespaced syntax, positional arguments `[...]`, and data blocks `{...}`.

---

## Implementation Considerations for `DebugConsoleManager.cs`

Modifying `DebugConsoleManager.cs` to support this new syntax will be a significant update.

1.  **`ProcessCommand(string line)` Overhaul:**
    * **Command Name Parsing:**
        * The `line` will first need to be split to separate the command name (e.g., `entity.traits.add`) from the arguments part (e.g., `["Grok", "Skilled Crafter"]`).
        * The command name itself (the key in your `commands` dictionary) will be the full string, e.g., `"entity.traits.add"`.
    * **Argument Parsing:** This is the complex part. After isolating the arguments string:
        * You'll need to parse out the `[...]` block and the `{...}` block. Regex can be very helpful here.
            * Example Regex for `[...]`: `@"\[(.*?)\]"` to capture content inside the first pair of square brackets.
            * Example Regex for `{...}`: `@"\{(.*?)\}"` to capture content inside the first pair of curly braces.
        * **Parsing Positional Arguments `[...]`:**
            * Once the content inside `[]` is extracted (e.g., `"Grok", "Skilled Crafter"`), it needs to be split by commas.
            * Handle quoted strings carefully to ensure commas inside quotes are not treated as delimiters. A simple split by comma might not be enough if arguments can contain commas themselves (though usually, IDs/names won't). A more robust CSV-like parsing or sequential parsing approach might be needed for this.
            * Convert these parsed strings into the `string[] args` that your command handlers expect, or adapt handlers to take more structured input.
        * **Parsing Data Blocks `{...}`:**
            * Extract content inside `{}`.
            * Split by comma for key-value pairs.
            * For each pair, split by the first colon `:` to separate key and value.
            * Keys are strings. Values need type detection (string, number, boolean, array). This could involve trying to parse as a number, checking for `true`/`false`, or if it starts with `[` and ends with `]` for an array (which then needs further parsing).
            * This parsed data block could be passed as a `Dictionary<string, object>` to command handlers.
    * **Error Handling:** Robust error messages for incorrect syntax (e.g., mismatched brackets, malformed data blocks) will be crucial.

2.  **Command Registration and Handling:**
    * `commands` dictionary: Keys will now be like `"entity.traits.add"`.
    * `commandDescriptions` dictionary: Update similarly.
    * Command handlers (`Action<string[]>`) might need to be adapted:
        * Some might still work with `string[] args` if you pre-process the positional arguments into that format.
        * For commands using data blocks, handlers might need a new signature like `Action<string[], Dictionary<string, object>>` or you could pass the raw data block string for the handler to parse. A dedicated argument parsing class/utility could be beneficial.

3.  **Contextual Argument Logic (Selected Entity):**
    * Before calling a command handler that expects an entity target, if the parsed positional arguments for the target are missing, `ProcessCommand` would query your `SelectionManager` (assuming you have one, as seen in `SelectionManager.cs`).
    * `SelectionManager.Instance.GetSelectedPops()` or a similar method would be used. If a single entity is selected, its ID or name is used as the default first argument.

4.  **Auto-Completion and Suggestions (`AutoComplete`, `UpdateSuggestions`):**
    * These methods will need to understand the namespaced command structure to provide relevant suggestions.
    * Suggesting based on `namespace.` then `namespace.command.` would be a good enhancement.
    * Suggesting parameter names or enum values after a command is typed could also be advanced features.

**Example Snippet Idea for `ProcessCommand` (Conceptual):**

```csharp
// Inside DebugConsoleManager.cs
void ProcessCommand(string line)
{
    // Basic split for command and the rest of the line
    string commandName = line.Split(' ')[0].ToLower(); // This part needs to be smarter to grab full namespaced command
    string remainingArgsString = "";
    if (line.Contains(" "))
    {
        commandName = line.Substring(0, line.IndexOf(' ')).ToLower();
        remainingArgsString = line.Substring(line.IndexOf(' ') + 1).Trim();
    }
    else
    {
        commandName = line.ToLower();
    }

    List<string> positionalArgs = new List<string>();
    Dictionary<string, object> dataBlockArgs = new Dictionary<string, object>();

    // --- Advanced Parsing Logic Would Go Here ---
    // 1. Use Regex to find and extract content of [...] and {...} from remainingArgsString
    //    Example:
    //    Match positionalMatch = Regex.Match(remainingArgsString, @"^\[(.*?)\]");
    //    if (positionalMatch.Success)
    //    {
    //        string argsContent = positionalMatch.Groups[1].Value;
    //        // TODO: Parse argsContent (split by comma, handle quotes)
    //        // positionalArgs.AddRange(parsedPositional);
    //        // Remove this part from remainingArgsString for further parsing
    //    }
    //
    //    Match dataBlockMatch = Regex.Match(remainingArgsString, @"\{(.*)\}"); // May need to be careful with greediness
    //    if (dataBlockMatch.Success)
    //    {
    //        string dataContent = dataBlockMatch.Groups[1].Value;
    //        // TODO: Parse dataContent (key:value pairs, handle types, arrays)
    //        // dataBlockArgs = parsedDataBlock;
    //    }
    // --- End of Advanced Parsing Logic ---


    if (commands.ContainsKey(commandName))
    {
        // Potentially adapt how args are passed or what the Action takes
        // This current structure passes all positional args as string[]
        // You might need to create a more complex argument object if commands need both positional and data block
        
        // For commands that need a target and it's missing:
        // if (IsEntityTargetingCommand(commandName) && positionalArgs.Count == 0 && SelectionManager.Instance.HasSingleSelection()) {
        //    positionalArgs.Insert(0, SelectionManager.Instance.GetSelectedPop().name); // Or ID
        // }

        // For now, assuming positionalArgs are processed into a string array for compatibility
        string[] argsArray = positionalArgs.ToArray(); // This is simplified
        
        try
        {
            commands[commandName].Invoke(argsArray); // Handler might need access to dataBlockArgs too
        }
        catch (Exception e)
        {
            AppendLine($"<color=red>Error executing '{commandName}':</color> {e.Message}");
            #if UNITY_EDITOR
            UnityEngine.Debug.LogError($"Console command error: {commandName} - {e}");
            #endif
        }
    }
    else
    {
        AppendLine($"<color=red>Error:</color> Unknown command '{commandName}'");
    }
}
```
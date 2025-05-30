using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Systems.Inventory;
using Lineage.Ancestral.Legacies.Debug;
using TMPro;

namespace Lineage.Ancestral.Legacies.Debug
{
    public class DebugConsoleManager : MonoBehaviour
    {
        [Header("Console Settings")]
        [SerializeField] private bool enableConsole = true;
        [SerializeField] private KeyCode legacyToggleKey = KeyCode.F2;
        [SerializeField] private KeyCode legacyToggleKeyAlt = KeyCode.BackQuote; // Tilde/backtick
        [SerializeField] private int maxHistoryCount = 50;
        [SerializeField] private int maxLogCount = 100;

        [Header("UI Settings")]
        [SerializeField] private Vector2 consoleSize = new Vector2(800, 400);
        [SerializeField] private Vector2 consolePosition = new Vector2(50, 50);
        [SerializeField] private bool isDraggable = true;
        [SerializeField] private bool isResizable = true;
        [SerializeField] private Texture2D resizeCursor;

        [SerializeField] public Font consolebodyFont;
        [SerializeField] public Font consoleheaderFont;

        // Input System support
        private InputAction toggleAction;
        private InputAction toggleActionAlt;
        private InputAction enterAction;
        private InputAction tabAction;
        private InputAction upArrowAction;
        private InputAction downArrowAction;

        // Console state
        private bool isConsoleVisible = false;
        private string currentInput = "";
        private List<string> commandHistory = new List<string>();
        public List<string> consoleLog = new List<string>();
        private int historyIndex = -1;
        private Vector2 scrollPosition = Vector2.zero;
        private Rect consoleRect;
        private bool isDragging = false;
        private bool isResizing = false;
        private Vector2 dragOffset;

        // Auto-completion
        private List<string> suggestions = new List<string>();
        private int selectedSuggestion = -1;
        private bool showSuggestions = true;

        // Command registry
        private Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();

        // Manager references
        private PopulationManager populationManager;
        private ResourceManager resourceManager;
        private SelectionManager selectionManager;

        // Console command delegate - updated to support data blocks
        public delegate string CommandDelegate(List<string> positionalArgs, Dictionary<string, object> dataBlock);

        // Console command structure
        [System.Serializable]
        public class ConsoleCommand
        {
            public string name;
            public string description;
            public CommandDelegate command;
            public string usage;
            public bool requiresEntityTarget;

            public ConsoleCommand(string name, string description, string usage, CommandDelegate command, bool requiresEntityTarget = false)
            {
                this.name = name;
                this.description = description;
                this.usage = usage;
                this.command = command;
                this.requiresEntityTarget = requiresEntityTarget;
            }
        }

        // Command parsing result
        private class ParsedCommand
        {
            public string commandName;
            public List<string> positionalArgs = new List<string>();
            public Dictionary<string, object> dataBlock = new Dictionary<string, object>();
        }

        private void Awake()
        {
            // Initialize console rect
            consoleRect = new Rect(consolePosition.x, consolePosition.y, consoleSize.x, consoleSize.y);

            // Get manager references
            populationManager = FindFirstObjectByType<PopulationManager>();
            resourceManager = FindFirstObjectByType<ResourceManager>();
            selectionManager = FindFirstObjectByType<SelectionManager>();

            // Setup input actions for new Input System
            SetupInputActions();

            // Register all commands
            RegisterCommands();

            // Log console initialization
            LogToConsole("Debug Console initialized. Type 'help' for available commands.");
        }

        private void SetupInputActions()
        {
            if (enableConsole)
            {
                // Create Input Actions for new Input System
                try
                {
                    toggleAction = new InputAction("ToggleConsole", InputActionType.Button, "<Keyboard>/f2");
                    toggleActionAlt = new InputAction("ToggleConsoleAlt", InputActionType.Button, "<Keyboard>/backquote");
                    
                    // Console-specific input actions
                    enterAction = new InputAction("ExecuteCommand", InputActionType.Button, "<Keyboard>/enter");
                    tabAction = new InputAction("AutoComplete", InputActionType.Button, "<Keyboard>/tab");
                    upArrowAction = new InputAction("NavigateUp", InputActionType.Button, "<Keyboard>/upArrow");
                    downArrowAction = new InputAction("NavigateDown", InputActionType.Button, "<Keyboard>/downArrow");

                    toggleAction.performed += _ => ToggleConsole();
                    toggleActionAlt.performed += _ => ToggleConsole();

                    toggleAction.Enable();
                    toggleActionAlt.Enable();
                    
                    // Console input actions are enabled/disabled when console visibility changes
                }
                catch (System.Exception e)
                {
                    Debug.Log.Error($"Failed to setup Input System actions: {e.Message}. Falling back to legacy input.");
                }
            }
        }

        private void Update()
        {
            // Legacy Input Manager support as fallback
            if (enableConsole && (toggleAction == null || toggleActionAlt == null))
            {
                if (Input.GetKeyDown(legacyToggleKey) || Input.GetKeyDown(legacyToggleKeyAlt))
                {
                    ToggleConsole();
                }
            }

            // Handle input when console is visible
            if (isConsoleVisible)
            {
                HandleConsoleInput();
            }
        }

        private void ToggleConsole()
        {
            isConsoleVisible = !isConsoleVisible;
            
            if (isConsoleVisible)
            {
                // Focus input when opening and enable console input actions
                GUI.FocusControl("ConsoleInput");
                EnableConsoleInputActions();
            }
            else
            {
                // Disable console input actions when closing
                DisableConsoleInputActions();
            }
        }

        private void EnableConsoleInputActions()
        {
            if (enterAction != null && tabAction != null && upArrowAction != null && downArrowAction != null)
            {
                enterAction.performed += _ => ExecuteCommand();
                tabAction.performed += _ => HandleAutoComplete();
                upArrowAction.performed += _ => HandleUpArrow();
                downArrowAction.performed += _ => HandleDownArrow();

                enterAction.Enable();
                tabAction.Enable();
                upArrowAction.Enable();
                downArrowAction.Enable();
            }
        }

        private void DisableConsoleInputActions()
        {
            if (enterAction != null && tabAction != null && upArrowAction != null && downArrowAction != null)
            {
                enterAction.performed -= _ => ExecuteCommand();
                tabAction.performed -= _ => HandleAutoComplete();
                upArrowAction.performed -= _ => HandleUpArrow();
                downArrowAction.performed -= _ => HandleDownArrow();

                enterAction.Disable();
                tabAction.Disable();
                upArrowAction.Disable();
                downArrowAction.Disable();
            }
        }

        private void HandleUpArrow()
        {
            // Handle suggestion navigation (only when suggestions are visible)
            if (showSuggestions && suggestions.Count > 0)
            {
                if (selectedSuggestion > 0)
                {
                    selectedSuggestion--;
                }
            }
            // Handle command history navigation (only when suggestions are not visible)
            else
            {
                NavigateHistory(-1);
            }
        }

        private void HandleDownArrow()
        {
            // Handle suggestion navigation (only when suggestions are visible)
            if (showSuggestions && suggestions.Count > 0)
            {
                if (selectedSuggestion < suggestions.Count - 1)
                {
                    selectedSuggestion++;
                }
            }
            // Handle command history navigation (only when suggestions are not visible)
            else
            {
                NavigateHistory(1);
            }
        }

        private void HandleConsoleInput()
        {
            // Legacy Input Manager support as fallback for console input
            if (enterAction == null || tabAction == null || upArrowAction == null || downArrowAction == null)
            {
                // Handle command execution first (highest priority)
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    ExecuteCommand();
                    return; // Exit early to prevent other input processing
                }

                // Handle auto-completion
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    HandleAutoComplete();
                    return;
                }

                // Handle suggestion navigation (only when suggestions are visible)
                if (showSuggestions && suggestions.Count > 0)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow) && selectedSuggestion > 0)
                    {
                        selectedSuggestion--;
                        return;
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow) && selectedSuggestion < suggestions.Count - 1)
                    {
                        selectedSuggestion++;
                        return;
                    }
                }

                // Handle command history navigation (only when suggestions are not visible)
                if (!showSuggestions)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        NavigateHistory(-1);
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        NavigateHistory(1);
                    }
                }
            }
            // New Input System is handling console input through actions
        }

        private void OnGUI()
        {
            if (!isConsoleVisible) return;

            // Draw console window
            GUI.skin.window.fontSize = 12;
            GUI.skin.label.font = consolebodyFont;
            GUI.skin.window.font = consoleheaderFont;
            GUI.skin.window.fontSize = 12;
            GUI.skin.textField.font = consolebodyFont;

            //make the text below bold
            GUI.skin.textField.fontStyle = FontStyle.Normal;
            GUI.skin.textField.fontSize = 12;
            GUI.skin.window.fontStyle = FontStyle.Bold;
            GUI.skin.button.fontStyle = FontStyle.Bold;
            GUI.skin.button.font = consolebodyFont;
            consoleRect = GUI.Window(0, consoleRect, DrawConsoleWindow, "Debug Console");

            // Handle dragging and resizing
            if (isDraggable || isResizable)
            {
                HandleWindowInteraction();
            }
        }

        private void DrawConsoleWindow(int windowID)
        {
            GUI.skin.textField.fontSize = 14;
            GUI.skin.textField.border = new RectOffset(4, 4, 4, 4);
            GUILayout.BeginVertical();

            // Log area
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(consoleRect.height - 80));
            
            foreach (string logEntry in consoleLog)
            {
                GUILayout.Label(logEntry, GUI.skin.textField);
            }

            GUILayout.EndScrollView();

            // Input area
            GUILayout.BeginHorizontal();
            GUILayout.Label("> ", GUILayout.Width(20));
            
            GUI.SetNextControlName("ConsoleInput");
            string newInput = GUILayout.TextField(currentInput);
            
            if (newInput != currentInput)
            {
                currentInput = newInput;
                UpdateSuggestions();
            }

            if (GUILayout.Button("Execute", GUILayout.Width(90)))
            {
                ExecuteCommand();
            }

            GUILayout.EndHorizontal();

            // Suggestions
            if (showSuggestions && suggestions.Count > 0)
            {
                DrawSuggestions();
            }

            GUILayout.EndVertical();

            // Make window draggable
            if (isDraggable)
            {
                GUI.DragWindow();
            }
        }

        private void DrawSuggestions()
        {
            int maxSuggestions = Mathf.Min(suggestions.Count, 5);
            float suggestionHeight = 20f; // Height per suggestion item
            float totalHeight = maxSuggestions * suggestionHeight + 10f; // Add some padding
            
            GUILayout.BeginVertical("box", GUILayout.Height(totalHeight));
            
            for (int i = 0; i < maxSuggestions; i++)
            {
                bool isSelected = i == selectedSuggestion;
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                
                if (GUILayout.Button(suggestions[i], GUI.skin.label, GUILayout.Height(suggestionHeight)))
                {
                    currentInput = suggestions[i];
                    showSuggestions = false;
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();
        }

        private void HandleWindowInteraction()
        {
            Event currentEvent = Event.current;
            Vector2 mousePos = currentEvent.mousePosition;

            // Define minimum sizes
            const float minWidth = 300f;
            const float minHeight = 150f;

            // Check if mouse is in resize area (bottom-right corner)
            Rect resizeArea = new Rect(consoleRect.x + consoleRect.width - 20, 
                                     consoleRect.y + consoleRect.height - 20, 20, 20);

            if (isResizable)
            {
                // Set cursor when hovering over resize area
                if (resizeArea.Contains(mousePos))
                {
                    #if UNITY_EDITOR
                    UnityEditor.EditorGUIUtility.AddCursorRect(resizeArea, UnityEditor.MouseCursor.ResizeUpRight);
                    #else
                    if (resizeCursor != null)
                    {
                        Cursor.SetCursor(resizeCursor, new Vector2(resizeCursor.width * 0.5f, resizeCursor.height * 0.5f), CursorMode.Auto);
                    }
                    #endif
                    
                    if (currentEvent.type == EventType.MouseDown)
                    {
                        isResizing = true;
                    }
                }
                else if (!isResizing)
                {
                    // Reset cursor when not over resize area and not actively resizing
                    #if !UNITY_EDITOR
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    #endif
                }
            }

            if (currentEvent.type == EventType.MouseDrag && isResizing)
            {
                consoleRect.width = mousePos.x - consoleRect.x;
                consoleRect.height = mousePos.y - consoleRect.y;
                consoleRect.width = Mathf.Max(minWidth, consoleRect.width);
                consoleRect.height = Mathf.Max(minHeight, consoleRect.height);
            }

            if (currentEvent.type == EventType.MouseUp)
            {
                isResizing = false;
                // Reset cursor when mouse is released
                #if !UNITY_EDITOR
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                #endif
            }
        }

        private void NavigateHistory(int direction)
        {
            if (commandHistory.Count == 0) return;

            historyIndex += direction;
            historyIndex = Mathf.Clamp(historyIndex, -1, commandHistory.Count - 1);

            if (historyIndex >= 0)
            {
                currentInput = commandHistory[historyIndex];
            }
            else
            {
                currentInput = "";
            }
        }

        private void UpdateSuggestions()
        {
            suggestions.Clear();
            selectedSuggestion = -1;

            if (string.IsNullOrEmpty(currentInput))
            {
                showSuggestions = false;
                return;
            }

            // Support namespace suggestions
            string input = currentInput.ToLower();
            foreach (var command in commands.Keys)
            {
                if (command.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                {
                    suggestions.Add(command);
                }
            }

            // Also suggest namespace prefixes
            var namespaces = new HashSet<string>();
            foreach (var command in commands.Keys)
            {
                var parts = command.Split('.');
                for (int i = 1; i <= parts.Length; i++)
                {
                    string prefix = string.Join(".", parts.Take(i));
                    if (prefix.StartsWith(input, StringComparison.OrdinalIgnoreCase) && prefix != command)
                    {
                        namespaces.Add(prefix);
                    }
                }
            }

            suggestions.AddRange(namespaces);
            suggestions = suggestions.OrderBy(s => s).ToList();

            showSuggestions = suggestions.Count > 0;
            if (showSuggestions)
            {
                selectedSuggestion = 0;
            }
        }

        private void HandleAutoComplete()
        {
            if (showSuggestions && suggestions.Count > 0)
            {
                currentInput = suggestions[selectedSuggestion];
                showSuggestions = false;
            }
        }

        private void ExecuteCommand()
        {
            if (string.IsNullOrEmpty(currentInput)) return;

            // Add to history
            if (commandHistory.Count == 0 || commandHistory[commandHistory.Count - 1] != currentInput)
            {
                commandHistory.Add(currentInput);
                if (commandHistory.Count > maxHistoryCount)
                {
                    commandHistory.RemoveAt(0);
                }
            }

            // Log the command
            LogToConsole($"> {currentInput}");

            // Parse command with new syntax
            ParsedCommand parsed = ParseCommandLine(currentInput);
            
            if (parsed != null && !string.IsNullOrEmpty(parsed.commandName))
            {
                if (commands.ContainsKey(parsed.commandName))
                {
                    var command = commands[parsed.commandName];
                    
                    // Handle contextual entity targeting
                    if (command.requiresEntityTarget && parsed.positionalArgs.Count == 0)
                    {
                        string selectedEntity = GetSelectedEntityTarget();
                        if (!string.IsNullOrEmpty(selectedEntity))
                        {
                            parsed.positionalArgs.Insert(0, selectedEntity);
                        }
                    }

                    try
                    {
                        string result = command.command(parsed.positionalArgs, parsed.dataBlock);
                        if (!string.IsNullOrEmpty(result))
                        {
                            LogToConsole(result);
                        }
                    }
                    catch (Exception e)
                    {
                        LogToConsole($"Error executing command: {e.Message}", true);
                    }
                }
                else
                {
                    LogToConsole($"Unknown command: {parsed.commandName}. Type 'help' for available commands.", true);
                }
            }
            else
            {
                LogToConsole("Invalid command syntax. Use: namespace.command [args...] {data...}", true);
            }

            // Clear input
            currentInput = "";
            historyIndex = -1;
            showSuggestions = false;

            // Auto-scroll to bottom
            scrollPosition.y = float.MaxValue;
        }

        private ParsedCommand ParseCommandLine(string line)
        {
            var result = new ParsedCommand();
            
            try
            {
                // Extract command name (everything before first space or bracket)
                string commandPart = line.Trim();
                int spaceIndex = commandPart.IndexOf(' ');
                int bracketIndex = commandPart.IndexOf('[');
                int braceIndex = commandPart.IndexOf('{');
                
                int firstSeparator = int.MaxValue;
                if (spaceIndex >= 0) firstSeparator = Math.Min(firstSeparator, spaceIndex);
                if (bracketIndex >= 0) firstSeparator = Math.Min(firstSeparator, bracketIndex);
                if (braceIndex >= 0) firstSeparator = Math.Min(firstSeparator, braceIndex);
                
                if (firstSeparator == int.MaxValue)
                {
                    result.commandName = commandPart.ToLower();
                    return result;
                }
                
                result.commandName = commandPart.Substring(0, firstSeparator).Trim().ToLower();
                string argumentsPart = commandPart.Substring(firstSeparator).Trim();

                // Parse positional arguments [...]
                var positionalMatch = Regex.Match(argumentsPart, @"\[(.*?)\]");
                if (positionalMatch.Success)
                {
                    string argsContent = positionalMatch.Groups[1].Value;
                    result.positionalArgs = ParsePositionalArguments(argsContent);
                }

                // Parse data block {...}
                var dataBlockMatch = Regex.Match(argumentsPart, @"\{(.*)\}");
                if (dataBlockMatch.Success)
                {
                    string dataContent = dataBlockMatch.Groups[1].Value;
                    result.dataBlock = ParseDataBlock(dataContent);
                }

                return result;
            }
            catch (Exception e)
            {
                LogToConsole($"Error parsing command: {e.Message}", true);
                return null;
            }
        }

        private List<string> ParsePositionalArguments(string argsContent)
        {
            var args = new List<string>();
            if (string.IsNullOrWhiteSpace(argsContent)) return args;

            // Simple CSV-like parsing with quote support
            var parts = new List<string>();
            bool inQuotes = false;
            string current = "";
            
            for (int i = 0; i < argsContent.Length; i++)
            {
                char c = argsContent[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    parts.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            
            if (!string.IsNullOrEmpty(current))
            {
                parts.Add(current.Trim());
            }

            // Clean up quoted strings
            foreach (string part in parts)
            {
                string cleaned = part.Trim();
                if (cleaned.StartsWith("\"") && cleaned.EndsWith("\"") && cleaned.Length > 1)
                {
                    cleaned = cleaned.Substring(1, cleaned.Length - 2);
                }
                args.Add(cleaned);
            }

            return args;
        }

        private Dictionary<string, object> ParseDataBlock(string dataContent)
        {
            var data = new Dictionary<string, object>();
            if (string.IsNullOrWhiteSpace(dataContent)) return data;

            // Split by comma, respecting quotes and brackets
            var pairs = SplitDataBlock(dataContent);
            
            foreach (string pair in pairs)
            {
                int colonIndex = pair.IndexOf(':');
                if (colonIndex > 0)
                {
                    string key = pair.Substring(0, colonIndex).Trim();
                    string valueStr = pair.Substring(colonIndex + 1).Trim();
                    
                    object value = ParseDataValue(valueStr);
                    data[key] = value;
                }
            }

            return data;
        }

        private List<string> SplitDataBlock(string content)
        {
            var parts = new List<string>();
            bool inQuotes = false;
            bool inBrackets = false;
            int bracketDepth = 0;
            string current = "";
            
            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                
                if (c == '"' && !inBrackets)
                {
                    inQuotes = !inQuotes;
                    current += c;
                }
                else if (c == '[' && !inQuotes)
                {
                    inBrackets = true;
                    bracketDepth++;
                    current += c;
                }
                else if (c == ']' && !inQuotes)
                {
                    bracketDepth--;
                    if (bracketDepth <= 0)
                    {
                        inBrackets = false;
                        bracketDepth = 0;
                    }
                    current += c;
                }
                else if (c == ',' && !inQuotes && !inBrackets)
                {
                    parts.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            
            if (!string.IsNullOrEmpty(current))
            {
                parts.Add(current.Trim());
            }

            return parts;
        }

        private object ParseDataValue(string valueStr)
        {
            valueStr = valueStr.Trim();
            
            // Handle arrays [...]
            if (valueStr.StartsWith("[") && valueStr.EndsWith("]"))
            {
                string arrayContent = valueStr.Substring(1, valueStr.Length - 2);
                var arrayItems = ParsePositionalArguments(arrayContent);
                return arrayItems.ToArray();
            }
            
            // Handle quoted strings
            if (valueStr.StartsWith("\"") && valueStr.EndsWith("\"") && valueStr.Length > 1)
            {
                return valueStr.Substring(1, valueStr.Length - 2);
            }
            
            // Handle booleans
            if (valueStr.ToLower() == "true") return true;
            if (valueStr.ToLower() == "false") return false;
            
            // Handle numbers
            if (int.TryParse(valueStr, out int intVal)) return intVal;
            if (float.TryParse(valueStr, out float floatVal)) return floatVal;
            
            // Default to string
            return valueStr;
        }

        private string GetSelectedEntityTarget()
        {
            if (selectionManager != null)
            {
                var selectedPops = selectionManager.GetSelectedPops();
                if (selectedPops.Count == 1)
                {
                    var popController = selectedPops[0].GetComponent<PopController>();
                    if (popController != null)
                    {
                        var pop = popController.GetPop();
                        return pop != null ? pop.GetInstanceID().ToString() : null;
                    }
                }
            }
            return null;
        }

        private void LogToConsole(string message, bool isError = false)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string prefix = isError ? "[ERROR]" : "[INFO]";
            string logEntry = $"[{timestamp}] {prefix} {message}";

            consoleLog.Add(logEntry);

            // Trim log if too long
            if (consoleLog.Count > maxLogCount)
            {
                consoleLog.RemoveAt(0);
            }

            // Also log to Unity console
            if (isError)
            {
                Debug.Log.Error(message);
            }
            else
            {
                Debug.Log.Error(message);
            }

            if (isError)
            {
                AdvancedLogger.LogError(LogCategory.General, message);
            }
            else
            {
                AdvancedLogger.LogInfo(LogCategory.General, message);
            }
        }

        public void RegisterCommands()
        {
            // General commands
            RegisterCommand("help", "Show all available commands", "help [command]", HelpCommand);
            RegisterCommand("clear", "Clear console log", "clear", ClearCommand);
            RegisterCommand("game.quit", "Quit application", "game.quit", QuitCommand);
            RegisterCommand("quit", "Quit application (alias)", "quit", QuitCommand);
            RegisterCommand("system.info", "Show system information", "system.info", SystemInfoCommand);

            // Entity commands
            RegisterCommand("entity.inspect", "Show detailed entity information", "entity.inspect [entity_id]", EntityInspectCommand, true);
            RegisterCommand("entity.health", "Set entity health", "entity.health [entity_id, value] or entity.health [value]", EntityHealthCommand, true);
            RegisterCommand("entity.state.set", "Set entity AI state", "entity.state.set [entity_id, state_name] or entity.state.set [state_name]", EntitySetStateCommand, true);
            RegisterCommand("entity.traits.add", "Add trait to entity", "entity.traits.add [entity_id, trait_name] or entity.traits.add [trait_name]", EntityAddTraitCommand, true);
            RegisterCommand("entity.inventory.list", "List entity inventory", "entity.inventory.list [entity_id]", EntityInventoryListCommand, true);
            RegisterCommand("entity.inventory.add", "Add item to entity inventory", "entity.inventory.add [entity_id, item_name, quantity] or entity.inventory.add [item_name, quantity]", EntityInventoryAddCommand, true);
            RegisterCommand("entity.teleport", "Teleport entity to position", "entity.teleport [entity_id, x, y, z] or entity.teleport [x, y, z]", EntityTeleportCommand, true);

            // Spawn commands
            RegisterCommand("spawn.pop", "Spawn a new pop", "spawn.pop [x, y, z] {name:\"PopName\", health:100, ...}", SpawnPopCommand);

            // Lineage commands
            RegisterCommand("lineage.resources.show", "Show current lineage resources", "lineage.resources.show", LineageResourcesShowCommand);
            RegisterCommand("lineage.resources.add", "Add resources to lineage", "lineage.resources.add [resource_type, amount]", LineageResourcesAddCommand);
            RegisterCommand("lineage.pops.list", "List all pops in lineage", "lineage.pops.list", LineagePopsListCommand);
            RegisterCommand("lineage.pops.set_cap", "Set population cap", "lineage.pops.set_cap [value]", LineagePopsSetCapCommand);

            // Game commands
            RegisterCommand("game.time.set", "Set game time", "game.time.set [value_or_preset]", GameTimeSetCommand);
            RegisterCommand("game.time.add", "Add time to game", "game.time.add [amount]", GameTimeAddCommand);
            RegisterCommand("game.timescale", "Set time scale", "game.timescale [scale]", GameTimescaleCommand);
            RegisterCommand("game.pause", "Pause/unpause simulation", "game.pause", GamePauseCommand);

            // Scene commands
            RegisterCommand("scene.load", "Load a scene", "scene.load [scene_name]", SceneLoadCommand);
            RegisterCommand("scene.list", "List available scenes", "scene.list", SceneListCommand);

            // Debug commands
            RegisterCommand("debug.draw.vision", "Toggle vision cone for entity", "debug.draw.vision [entity_id]", DebugDrawVisionCommand, true);
            RegisterCommand("debug.stats.toggle", "Toggle stats overlay", "debug.stats.toggle", DebugStatsToggleCommand);

            // Legacy command aliases for backward compatibility
            RegisterCommand("spawn_pop", "Legacy: Spawn pop", "spawn_pop <x> <y> [z]", LegacySpawnPopCommand);
            RegisterCommand("list_pops", "Legacy: List pops", "list_pops", LegacyListPopsCommand);
            RegisterCommand("resources", "Legacy: Show resources", "resources", LegacyShowResourcesCommand);

            LogToConsole($"Registered {commands.Count} debug commands.");
        }

        public void RegisterCommand(string name, string description, string usage, CommandDelegate command, bool requiresEntityTarget = false)
        {
            commands[name] = new ConsoleCommand(name, description, usage, command, requiresEntityTarget);
        }

        #region Command Implementations

        private string HelpCommand(List<string> args, Dictionary<string, object> data)
        {
            if (args.Count > 0)
            {
                string commandName = args[0].ToLower();
                if (commands.ContainsKey(commandName))
                {
                    var cmd = commands[commandName];
                    return $"{cmd.name}: {cmd.description}\nUsage: {cmd.usage}";
                }
                else
                {
                    return $"Unknown command: {commandName}";
                }
            }

            string result = "Available commands:\n";
            var groupedCommands = commands.Values.GroupBy(c => c.name.Split('.')[0]).OrderBy(g => g.Key);
            
            foreach (var group in groupedCommands)
            {
                result += $"\n{group.Key.ToUpper()}:\n";
                foreach (var cmd in group.OrderBy(c => c.name))
                {
                    result += $"  {cmd.name} - {cmd.description}\n";
                }
            }
            result += "\nType 'help [command]' for detailed usage.";
            return result;
        }

        private string ClearCommand(List<string> args, Dictionary<string, object> data)
        {
            consoleLog.Clear();
            return "";
        }

        private string QuitCommand(List<string> args, Dictionary<string, object> data)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
            return "Quitting application...";
        }

        private string SystemInfoCommand(List<string> args, Dictionary<string, object> data)
        {
            return $"System Info:\n" +
                   $"  Unity Version: {Application.unityVersion}\n" +
                   $"  Platform: {Application.platform}\n" +
                   $"  Time Scale: {Time.timeScale}\n" +
                   $"  Frame Rate: {1f / Time.unscaledDeltaTime:F1} FPS\n" +
                   $"  Memory Usage: {System.GC.GetTotalMemory(false) / 1024 / 1024} MB";
        }

        private string EntityInspectCommand(List<string> args, Dictionary<string, object> data)
        {
            if (args.Count == 0) return "No entity specified or selected.";
            
            Pop targetPop = GetPopById(args[0]);
            if (targetPop == null) return $"Entity '{args[0]}' not found.";

            var controller = targetPop.GetComponent<PopController>();
            string result = $"Entity {targetPop.GetInstanceID()} Info:\n";
            result += $"  Position: {targetPop.transform.position}\n";
            result += $"  Health: {targetPop.health:F1}/{targetPop.maxHealth}\n";
            result += $"  Hunger: {targetPop.hunger:F1}\n";
            result += $"  Thirst: {targetPop.thirst:F1}\n";
            result += $"  Energy: {targetPop.stamina:F1}\n";
            result += $"  AI State: {controller?.GetCurrentStateName() ?? "Unknown"}\n";

            return result;
        }

        private string EntityHealthCommand(List<string> args, Dictionary<string, object> data)
        {
            if (args.Count == 0) return "Usage: entity.health [entity_id, value] or entity.health [value]";
            
            string entityId = args.Count > 1 ? args[0] : null;
            string valueStr = args.Count > 1 ? args[1] : args[0];
            
            if (string.IsNullOrEmpty(entityId))
            {
                entityId = GetSelectedEntityTarget();
                if (string.IsNullOrEmpty(entityId)) return "No entity specified or selected.";
            }
            
            if (!float.TryParse(valueStr, out float value)) return "Invalid health value.";
            
            Pop targetPop = GetPopById(entityId);
            if (targetPop == null) return $"Entity '{entityId}' not found.";
            
            if (valueStr.ToLower() == "full")
            {
                targetPop.health = targetPop.maxHealth;
            }
            else
            {
                targetPop.health = Mathf.Clamp(value, 0f, targetPop.maxHealth);
            }
            
            return $"Set health of entity {entityId} to {targetPop.health:F1}";
        }

        // Legacy command implementations for backward compatibility
        private string LegacySpawnPopCommand(List<string> args, Dictionary<string, object> data)
        {
            // Convert legacy args to new format
            var newArgs = new List<string>();
            if (args.Count >= 2)
            {
                newArgs.Add(args[0]); // x
                newArgs.Add(args[1]); // y
                if (args.Count > 2) newArgs.Add(args[2]); // z
            }
            return SpawnPopCommand(newArgs, new Dictionary<string, object>());
        }

        private string SpawnPopCommand(List<string> args, Dictionary<string, object> data)
        {
            if (args.Count < 2) return "Usage: spawn.pop [x, y, z] {name:\"PopName\", ...}";

            if (!float.TryParse(args[0], out float x) || !float.TryParse(args[1], out float y))
            {
                return "Invalid coordinates.";
            }

            float z = args.Count > 2 && float.TryParse(args[2], out float zVal) ? zVal : 0f;
            Vector3 position = new Vector3(x, y, z);

            if (populationManager != null)
            {
                try
                {
                    populationManager.SpawnPop();
                    
                    // Find the most recently spawned pop (assuming it's the last one in the scene)
                    var allPops = FindObjectsByType<Pop>(FindObjectsSortMode.None);
                    if (allPops.Length > 0)
                    {
                        GameObject spawnedPop = allPops[allPops.Length - 1].gameObject;
                        
                        // Set position with Z = 0
                        spawnedPop.transform.position = new Vector3(position.x, position.y, 0f);
                        
                        // Set sorting layer to "Entities"
                        var renderer = spawnedPop.GetComponent<SpriteRenderer>();
                        if (renderer != null)
                        {
                            renderer.sortingLayerName = "Entities";
                        }
                        
                        return $"Spawned pop at {spawnedPop.transform.position} on Entities layer";
                    }
                    else
                    {
                        return "Failed to find the spawned pop";
                    }
                }
                catch (Exception e)
                {
                    return $"Error spawning pop: {e.Message}";
                }
            }
            else
            {
                return "PopulationManager not found.";
            }
        }

        // Additional command stubs - implement based on existing methods
        private string EntitySetStateCommand(List<string> args, Dictionary<string, object> data) => "Entity state command [Implementation needed]";
        private string EntityAddTraitCommand(List<string> args, Dictionary<string, object> data) => "Entity add trait command [Implementation needed]";
        private string EntityInventoryListCommand(List<string> args, Dictionary<string, object> data) => "Entity inventory list command [Implementation needed]";
        private string EntityInventoryAddCommand(List<string> args, Dictionary<string, object> data) => "Entity inventory add command [Implementation needed]";
        private string EntityTeleportCommand(List<string> args, Dictionary<string, object> data) => "Entity teleport command [Implementation needed]";
        private string LineageResourcesShowCommand(List<string> args, Dictionary<string, object> data) => ShowResourcesCommand(new string[0]);
        private string LineageResourcesAddCommand(List<string> args, Dictionary<string, object> data) => "Lineage resources add command [Implementation needed]";
        private string LineagePopsListCommand(List<string> args, Dictionary<string, object> data) => ListPopsCommand(new string[0]);
        private string LineagePopsSetCapCommand(List<string> args, Dictionary<string, object> data) => "Lineage pops set cap command [Implementation needed]";
        private string GameTimeSetCommand(List<string> args, Dictionary<string, object> data) => "Game time set command [Implementation needed]";
        private string GameTimeAddCommand(List<string> args, Dictionary<string, object> data) => "Game time add command [Implementation needed]";
        private string GameTimescaleCommand(List<string> args, Dictionary<string, object> data) => TimeScaleCommand(args.ToArray());
        private string GamePauseCommand(List<string> args, Dictionary<string, object> data) => PauseCommand(new string[0]);
        private string SceneLoadCommand(List<string> args, Dictionary<string, object> data) => "Scene load command [Implementation needed]";
        private string SceneListCommand(List<string> args, Dictionary<string, object> data) => "Scene list command [Implementation needed]";
        private string DebugDrawVisionCommand(List<string> args, Dictionary<string, object> data) => "Debug draw vision command [Implementation needed]";
        private string DebugStatsToggleCommand(List<string> args, Dictionary<string, object> data) => "Debug stats toggle command [Implementation needed]";

        private string LegacyListPopsCommand(List<string> args, Dictionary<string, object> data)
        {
            return ListPopsCommand(new string[0]);
        }

        private string LegacyShowResourcesCommand(List<string> args, Dictionary<string, object> data)
        {
            return ShowResourcesCommand(new string[0]);
        }

        #endregion

        #region Legacy Helper Methods (for backward compatibility)

        private Pop GetPopById(string idStr)
        {
            if (int.TryParse(idStr, out int id))
            {
                var allPops = FindObjectsByType<Pop>(FindObjectsSortMode.None);
                return allPops.FirstOrDefault(p => p.GetInstanceID() == id);
            }
            return null;
        }

        // Keep existing legacy methods for reference/compatibility
        private string ShowResourcesCommand(string[] args)
        {
            if (resourceManager != null)
            {
                return $"Resources:\n  Food: {resourceManager.currentFood}\n  Faith: {resourceManager.currentFaithPoints}\n  Wood: {resourceManager.currentWood}";
            }
            else
            {
                return "ResourceManager not found.";
            }
        }

        private string ListPopsCommand(string[] args)
        {
            var allPops = FindObjectsByType<Pop>(FindObjectsSortMode.None);
            
            if (allPops.Length == 0)
            {
                return "No pops found.";
            }

            string result = $"Found {allPops.Length} pops:\n";
            foreach (var pop in allPops)
            {
                var controller = pop.GetComponent<PopController>();
                string stateName = controller?.GetCurrentStateName() ?? "Unknown";
                result += $"  ID: {pop.GetInstanceID()}, Position: {pop.transform.position}, State: {stateName}, Health: {pop.health:F1}\n";
            }

            return result;
        }

        private string TimeScaleCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return $"Current time scale: {Time.timeScale}";
            }

            if (!float.TryParse(args[0], out float scale))
            {
                return "Invalid time scale value.";
            }

            Time.timeScale = Mathf.Max(0f, scale);
            return $"Set time scale to {Time.timeScale}";
        }

        private string PauseCommand(string[] args)
        {
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
                return "Unpaused simulation";
            }
            else
            {
                Time.timeScale = 0f;
                return "Paused simulation";
            }
        }

        #endregion

        private void OnDestroy()
        {
            // Reset cursor before cleanup
            #if !UNITY_EDITOR
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            #endif
            
            // Clean up input actions
            if (toggleAction != null)
            {
                toggleAction.Disable();
                toggleAction.Dispose();
            }

            if (toggleActionAlt != null)
            {
                toggleActionAlt.Disable();
                toggleActionAlt.Dispose();
            }

            if (enterAction != null)
            {
                enterAction.Disable();
                enterAction.Dispose();
            }

            if (tabAction != null)
            {
                tabAction.Disable();
                tabAction.Dispose();
            }

            if (upArrowAction != null)
            {
                upArrowAction.Disable();
                upArrowAction.Dispose();
            }

            if (downArrowAction != null)
            {
                downArrowAction.Disable();
                downArrowAction.Dispose();
            }
        }

        public bool IsMouseOverConsole()
        {
            if (!isConsoleVisible) return false;
            
            Vector2 mousePos = Input.mousePosition;
            // Convert screen space mouse position to GUI space
            mousePos.y = Screen.height - mousePos.y;
            
            return consoleRect.Contains(mousePos);
        }
    }
}

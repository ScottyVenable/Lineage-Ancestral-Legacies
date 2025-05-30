using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Systems.Inventory;
using Lineage.Ancestral.Legacies.Debug;

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

        // Input System support
        private InputAction toggleAction;
        private InputAction toggleActionAlt;

        // Console state
        private bool isConsoleVisible = false;
        private string currentInput = "";
        private List<string> commandHistory = new List<string>();
        private List<string> consoleLog = new List<string>();
        private int historyIndex = -1;
        private Vector2 scrollPosition = Vector2.zero;
        private Rect consoleRect;
        private bool isDragging = false;
        private bool isResizing = false;
        private Vector2 dragOffset;

        // Auto-completion
        private List<string> suggestions = new List<string>();
        private int selectedSuggestion = -1;
        private bool showSuggestions = false;

        // Command registry
        private Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();

        // Manager references
        private PopulationManager populationManager;
        private ResourceManager resourceManager;
        private SelectionManager selectionManager;
        private AdvancedLogger logger;

        // Console command delegate
        public delegate string CommandDelegate(string[] args);

        // Console command structure
        [System.Serializable]
        public class ConsoleCommand
        {
            public string name;
            public string description;
            public CommandDelegate command;
            public string usage;

            public ConsoleCommand(string name, string description, string usage, CommandDelegate command)
            {
                this.name = name;
                this.description = description;
                this.usage = usage;
                this.command = command;
            }
        }

        private void Awake()
        {
            // Initialize console rect
            consoleRect = new Rect(consolePosition.x, consolePosition.y, consoleSize.x, consoleSize.y);

            // Get manager references
            populationManager = FindFirstObjectByType<PopulationManager>();
            resourceManager = FindFirstObjectByType<ResourceManager>();
            selectionManager = FindFirstObjectByType<SelectionManager>();
            logger = FindFirstObjectByType<AdvancedLogger>();

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

                    toggleAction.performed += _ => ToggleConsole();
                    toggleActionAlt.performed += _ => ToggleConsole();

                    toggleAction.Enable();
                    toggleActionAlt.Enable();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to setup Input System actions: {e.Message}. Falling back to legacy input.");
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

        private void HandleConsoleInput()
        {
            // Handle command history navigation
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(1);
            }

            // Handle auto-completion
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                HandleAutoComplete();
            }

            // Handle command execution
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ExecuteCommand();
            }

            // Handle suggestion navigation
            if (showSuggestions)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) && selectedSuggestion > 0)
                {
                    selectedSuggestion--;
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) && selectedSuggestion < suggestions.Count - 1)
                {
                    selectedSuggestion++;
                }
            }
        }

        private void ToggleConsole()
        {
            isConsoleVisible = !isConsoleVisible;
            
            if (isConsoleVisible)
            {
                // Focus input when opening
                GUI.FocusControl("ConsoleInput");
            }
        }

        private void OnGUI()
        {
            if (!isConsoleVisible) return;

            // Draw console window
            GUI.skin.window.fontSize = 12;
            consoleRect = GUI.Window(0, consoleRect, DrawConsoleWindow, "Debug Console");

            // Handle dragging and resizing
            if (isDraggable || isResizable)
            {
                HandleWindowInteraction();
            }
        }

        private void DrawConsoleWindow(int windowID)
        {
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

            if (GUILayout.Button("Execute", GUILayout.Width(70)))
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
            GUILayout.BeginVertical("box");
            for (int i = 0; i < Mathf.Min(suggestions.Count, 5); i++)
            {
                bool isSelected = i == selectedSuggestion;
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                
                if (GUILayout.Button(suggestions[i], GUI.skin.label))
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

            // Check if mouse is in resize area (bottom-right corner)
            Rect resizeArea = new Rect(consoleRect.x + consoleRect.width - 20, 
                                     consoleRect.y + consoleRect.height - 20, 20, 20);

            if (isResizable && resizeArea.Contains(mousePos))
            {
                if (currentEvent.type == EventType.MouseDown)
                {
                    isResizing = true;
                }
            }

            if (currentEvent.type == EventType.MouseDrag && isResizing)
            {
                consoleRect.width = mousePos.x - consoleRect.x;
                consoleRect.height = mousePos.y - consoleRect.y;
                consoleRect.width = Mathf.Max(400, consoleRect.width);
                consoleRect.height = Mathf.Max(200, consoleRect.height);
            }

            if (currentEvent.type == EventType.MouseUp)
            {
                isResizing = false;
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
            suggestions.clear();
            selectedSuggestion = -1;

            if (string.IsNullOrEmpty(currentInput))
            {
                showSuggestions = false;
                return;
            }

            foreach (var command in commands.Keys)
            {
                if (command.StartsWith(currentInput, StringComparison.OrdinalIgnoreCase))
                {
                    suggestions.Add(command);
                }
            }

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

            // Parse and execute
            string[] parts = currentInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                string commandName = parts[0].ToLower();
                string[] args = parts.Skip(1).ToArray();

                if (commands.ContainsKey(commandName))
                {
                    try
                    {
                        string result = commands[commandName].command(args);
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
                    LogToConsole($"Unknown command: {commandName}. Type 'help' for available commands.", true);
                }
            }

            // Clear input
            currentInput = "";
            historyIndex = -1;
            showSuggestions = false;

            // Auto-scroll to bottom
            scrollPosition.y = float.MaxValue;
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
                Debug.LogError(message);
            }
            else
            {
                Debug.Log(message);
            }

            // Log to AdvancedLogger if available
            if (logger != null)
            {
                if (isError)
                {
                    logger.LogError("DebugConsole", message);
                }
                else
                {
                    logger.LogInfo("DebugConsole", message);
                }
            }
        }

        private void RegisterCommands()
        {
            // General commands
            RegisterCommand("help", "Show all available commands", "help [command]", HelpCommand);
            RegisterCommand("clear", "Clear console log", "clear", ClearCommand);
            RegisterCommand("quit", "Quit application", "quit", QuitCommand);

            // Population commands
            RegisterCommand("spawn_pop", "Spawn a new pop at position", "spawn_pop <x> <y> [z]", SpawnPopCommand);
            RegisterCommand("kill_pop", "Kill selected pop or pop by ID", "kill_pop [id]", KillPopCommand);
            RegisterCommand("list_pops", "List all pops with their IDs", "list_pops", ListPopsCommand);
            RegisterCommand("select_pop", "Select pop by ID", "select_pop <id>", SelectPopCommand);
            RegisterCommand("move_pop", "Move selected pop to position", "move_pop <x> <y> [z]", MovePopCommand);

            // Health and needs commands
            RegisterCommand("set_health", "Set pop health", "set_health <value> [pop_id]", SetHealthCommand);
            RegisterCommand("set_hunger", "Set pop hunger", "set_hunger <value> [pop_id]", SetHungerCommand);
            RegisterCommand("set_thirst", "Set pop thirst", "set_thirst <value> [pop_id]", SetThirstCommand);
            RegisterCommand("set_energy", "Set pop energy", "set_energy <value> [pop_id]", SetEnergyCommand);
            RegisterCommand("heal_pop", "Fully heal pop", "heal_pop [pop_id]", HealPopCommand);

            // Inventory commands
            RegisterCommand("give_item", "Give item to pop", "give_item <item_name> <quantity> [pop_id]", GiveItemCommand);
            RegisterCommand("remove_item", "Remove item from pop", "remove_item <item_name> <quantity> [pop_id]", RemoveItemCommand);
            RegisterCommand("clear_inventory", "Clear pop inventory", "clear_inventory [pop_id]", ClearInventoryCommand);
            RegisterCommand("list_inventory", "List pop inventory", "list_inventory [pop_id]", ListInventoryCommand);

            // Resource commands
            RegisterCommand("give_food", "Add food to resources", "give_food <amount>", GiveFoodCommand);
            RegisterCommand("give_faith", "Add faith to resources", "give_faith <amount>", GiveFaithCommand);
            RegisterCommand("give_wood", "Add wood to resources", "give_wood <amount>", GiveWoodCommand);
            RegisterCommand("set_food", "Set food amount", "set_food <amount>", SetFoodCommand);
            RegisterCommand("set_faith", "Set faith amount", "set_faith <amount>", SetFaithCommand);
            RegisterCommand("set_wood", "Set wood amount", "set_wood <amount>", SetWoodCommand);
            RegisterCommand("resources", "Show current resources", "resources", ShowResourcesCommand);

            // AI and state commands
            RegisterCommand("set_ai_state", "Set pop AI state", "set_ai_state <state_name> [pop_id]", SetAIStateCommand);
            RegisterCommand("list_ai_states", "List available AI states", "list_ai_states", ListAIStatesCommand);
            RegisterCommand("pop_info", "Show detailed pop information", "pop_info [pop_id]", PopInfoCommand);

            // Time and simulation commands
            RegisterCommand("timescale", "Set time scale", "timescale <scale>", TimeScaleCommand);
            RegisterCommand("pause", "Pause/unpause simulation", "pause", PauseCommand);

            // Debug visualization commands
            RegisterCommand("toggle_debug", "Toggle debug visualization", "toggle_debug", ToggleDebugCommand);
            RegisterCommand("toggle_stats", "Toggle stats overlay", "toggle_stats", ToggleStatsCommand);
            RegisterCommand("debug_pop_paths", "Toggle pop path visualization", "debug_pop_paths", DebugPopPathsCommand);

            LogToConsole($"Registered {commands.Count} debug commands.");
        }

        private void RegisterCommand(string name, string description, string usage, CommandDelegate command)
        {
            commands[name] = new ConsoleCommand(name, description, usage, command);
        }

        #region Command Implementations

        private string HelpCommand(string[] args)
        {
            if (args.Length > 0)
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
            foreach (var cmd in commands.Values.OrderBy(c => c.name))
            {
                result += $"  {cmd.name} - {cmd.description}\n";
            }
            result += "\nType 'help <command>' for detailed usage.";
            return result;
        }

        private string ClearCommand(string[] args)
        {
            consoleLog.Clear();
            return "";
        }

        private string QuitCommand(string[] args)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
            return "Quitting application...";
        }

        private string SpawnPopCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "Usage: spawn_pop <x> <y> [z]";
            }

            if (!float.TryParse(args[0], out float x) || !float.TryParse(args[1], out float y))
            {
                return "Invalid coordinates. Use numbers for x and y.";
            }

            float z = 0f;
            if (args.Length > 2 && !float.TryParse(args[2], out z))
            {
                return "Invalid z coordinate.";
            }

            Vector3 position = new Vector3(x, y, z);

            if (populationManager != null)
            {
                try
                {
                    var pop = populationManager.SpawnPop(position);
                    if (pop != null)
                    {
                        return $"Spawned pop at {position}. Pop ID: {pop.GetInstanceID()}";
                    }
                    else
                    {
                        return "Failed to spawn pop.";
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

        private string KillPopCommand(string[] args)
        {
            Pop targetPop = null;

            if (args.Length > 0)
            {
                // Kill by ID
                if (int.TryParse(args[0], out int popId))
                {
                    var allPops = FindObjectsByType<Pop>(FindObjectsSortMode.None);
                    targetPop = allPops.FirstOrDefault(p => p.GetInstanceID() == popId);
                }
            }
            else
            {
                // Kill selected pop
                if (selectionManager != null)
                {
                    var selectedPops = selectionManager.GetSelectedPops();
                    if (selectedPops.Count > 0)
                    {
                        var popController = selectedPops[0].GetComponent<PopController>();
                        if (popController != null)
                        {
                            targetPop = popController.GetPop();
                        }
                    }
                }
            }

            if (targetPop != null)
            {
                string popName = $"Pop {targetPop.GetInstanceID()}";
                Destroy(targetPop.gameObject);
                return $"Killed {popName}";
            }
            else
            {
                return "No pop found to kill. Select a pop or provide a valid ID.";
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

        private string SelectPopCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: select_pop <id>";
            }

            if (!int.TryParse(args[0], out int popId))
            {
                return "Invalid pop ID.";
            }

            var allPops = FindObjectsByType<Pop>(FindObjectsSortMode.None);
            var targetPop = allPops.FirstOrDefault(p => p.GetInstanceID() == popId);

            if (targetPop != null)
            {
                var controller = targetPop.GetComponent<PopController>();
                if (controller != null)
                {
                    controller.ForceSelect();
                    return $"Selected pop {popId}";
                }
                else
                {
                    return "Pop has no PopController component.";
                }
            }
            else
            {
                return $"Pop with ID {popId} not found.";
            }
        }

        private string MovePopCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "Usage: move_pop <x> <y> [z]";
            }

            if (!float.TryParse(args[0], out float x) || !float.TryParse(args[1], out float y))
            {
                return "Invalid coordinates.";
            }

            float z = 0f;
            if (args.Length > 2 && !float.TryParse(args[2], out z))
            {
                return "Invalid z coordinate.";
            }

            Vector3 targetPosition = new Vector3(x, y, z);

            if (selectionManager != null)
            {
                var selectedPops = selectionManager.GetSelectedPops();
                if (selectedPops.Count > 0)
                {
                    var popController = selectedPops[0].GetComponent<PopController>();
                    if (popController != null)
                    {
                        popController.MoveTo(targetPosition);
                        return $"Moving selected pop to {targetPosition}";
                    }
                    else
                    {
                        return "Selected object has no PopController.";
                    }
                }
                else
                {
                    return "No pop selected. Select a pop first.";
                }
            }
            else
            {
                return "SelectionManager not found.";
            }
        }

        private string SetHealthCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: set_health <value> [pop_id]";
            }

            if (!float.TryParse(args[0], out float healthValue))
            {
                return "Invalid health value.";
            }

            Pop targetPop = GetTargetPop(args.Length > 1 ? args[1] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            targetPop.health = Mathf.Clamp(healthValue, 0f, targetPop.maxHealth);
            return $"Set health of pop {targetPop.GetInstanceID()} to {targetPop.health:F1}";
        }

        private string SetHungerCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: set_hunger <value> [pop_id]";
            }

            if (!float.TryParse(args[0], out float hungerValue))
            {
                return "Invalid hunger value.";
            }

            Pop targetPop = GetTargetPop(args.Length > 1 ? args[1] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            targetPop.hunger = Mathf.Clamp(hungerValue, 0f, 100f);
            return $"Set hunger of pop {targetPop.GetInstanceID()} to {targetPop.hunger:F1}";
        }

        private string SetThirstCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: set_thirst <value> [pop_id]";
            }

            if (!float.TryParse(args[0], out float thirstValue))
            {
                return "Invalid thirst value.";
            }

            Pop targetPop = GetTargetPop(args.Length > 1 ? args[1] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            targetPop.thirst = Mathf.Clamp(thirstValue, 0f, 100f);
            return $"Set thirst of pop {targetPop.GetInstanceID()} to {targetPop.thirst:F1}";
        }

        private string SetEnergyCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: set_energy <value> [pop_id]";
            }

            if (!float.TryParse(args[0], out float energyValue))
            {
                return "Invalid energy value.";
            }

            Pop targetPop = GetTargetPop(args.Length > 1 ? args[1] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            targetPop.energy = Mathf.Clamp(energyValue, 0f, 100f);
            return $"Set energy of pop {targetPop.GetInstanceID()} to {targetPop.energy:F1}";
        }

        private string HealPopCommand(string[] args)
        {
            Pop targetPop = GetTargetPop(args.Length > 0 ? args[0] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            targetPop.health = targetPop.maxHealth;
            targetPop.hunger = 100f;
            targetPop.thirst = 100f;
            targetPop.energy = 100f;

            return $"Fully healed pop {targetPop.GetInstanceID()}";
        }        private string GiveItemCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "Usage: give_item <item_name> <quantity> [pop_id]";
            }

            string itemName = args[0];
            if (!int.TryParse(args[1], out int quantity))
            {
                return "Invalid quantity.";
            }

            Pop targetPop = GetTargetPop(args.Length > 2 ? args[2] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            var inventory = targetPop.GetComponent<InventoryComponent>();
            if (inventory == null)
            {
                return "Pop has no inventory component.";
            }

            bool success = inventory.AddItem(itemName, quantity);
            if (success)
            {
                return $"Gave {quantity} {itemName} to pop {targetPop.GetInstanceID()}";
            }
            else
            {
                return $"Failed to give {quantity} {itemName} to pop {targetPop.GetInstanceID()} (inventory may be full)";
            }
        }        private string RemoveItemCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "Usage: remove_item <item_name> <quantity> [pop_id]";
            }

            string itemName = args[0];
            if (!int.TryParse(args[1], out int quantity))
            {
                return "Invalid quantity.";
            }

            Pop targetPop = GetTargetPop(args.Length > 2 ? args[2] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            var inventory = targetPop.GetComponent<InventoryComponent>();
            if (inventory == null)
            {
                return "Pop has no inventory component.";
            }

            bool success = inventory.RemoveItem(itemName, quantity);
            if (success)
            {
                return $"Removed {quantity} {itemName} from pop {targetPop.GetInstanceID()}";
            }
            else
            {
                return $"Failed to remove {quantity} {itemName} from pop {targetPop.GetInstanceID()} (not enough items)";
            }
        }        private string ClearInventoryCommand(string[] args)
        {
            Pop targetPop = GetTargetPop(args.Length > 0 ? args[0] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            var inventory = targetPop.GetComponent<InventoryComponent>();
            if (inventory == null)
            {
                return "Pop has no inventory component.";
            }

            inventory.ClearInventory();
            return $"Cleared inventory of pop {targetPop.GetInstanceID()}";
        }        private string ListInventoryCommand(string[] args)
        {
            Pop targetPop = GetTargetPop(args.Length > 0 ? args[0] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            var inventory = targetPop.GetComponent<InventoryComponent>();
            if (inventory == null)
            {
                return "Pop has no inventory component.";
            }

            var items = inventory.GetAllItems();
            if (items.Count == 0)
            {
                return $"Pop {targetPop.GetInstanceID()} inventory is empty.";
            }

            string result = $"Pop {targetPop.GetInstanceID()} inventory ({inventory.GetTotalItemCount()}/{inventory.capacity}):\n";
            foreach (var item in items)
            {
                result += $"  {item.Key}: {item.Value}\n";
            }

            return result.TrimEnd('\n');
        }        private string GiveFoodCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: give_food <amount>";
            }

            if (!float.TryParse(args[0], out float amount))
            {
                return "Invalid amount.";
            }

            if (resourceManager != null)
            {
                resourceManager.AddFood(amount);
                return $"Added {amount} food. Current food: {resourceManager.currentFood}";
            }
            else
            {
                return "ResourceManager not found.";
            }
        }        private string GiveFaithCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: give_faith <amount>";
            }

            if (!float.TryParse(args[0], out float amount))
            {
                return "Invalid amount.";
            }

            if (resourceManager != null)
            {
                resourceManager.AddFaith(amount);
                return $"Added {amount} faith. Current faith: {resourceManager.currentFaithPoints}";
            }
            else
            {
                return "ResourceManager not found.";
            }
        }        private string GiveWoodCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: give_wood <amount>";
            }

            if (!float.TryParse(args[0], out float amount))
            {
                return "Invalid amount.";
            }

            if (resourceManager != null)
            {
                resourceManager.AddWood(amount);
                return $"Added {amount} wood. Current wood: {resourceManager.currentWood}";
            }
            else
            {
                return "ResourceManager not found.";
            }
        }        private string SetFoodCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: set_food <amount>";
            }

            if (!float.TryParse(args[0], out float amount))
            {
                return "Invalid amount.";
            }

            if (resourceManager != null)
            {
                resourceManager.SetFood(amount);
                return $"Set food to {amount}";
            }
            else
            {
                return "ResourceManager not found.";
            }
        }        private string SetFaithCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: set_faith <amount>";
            }

            if (!float.TryParse(args[0], out float amount))
            {
                return "Invalid amount.";
            }

            if (resourceManager != null)
            {
                resourceManager.SetFaith(amount);
                return $"Set faith to {amount}";
            }
            else
            {
                return "ResourceManager not found.";
            }
        }        private string SetWoodCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: set_wood <amount>";
            }

            if (!float.TryParse(args[0], out float amount))
            {
                return "Invalid amount.";
            }

            if (resourceManager != null)
            {
                resourceManager.SetWood(amount);
                return $"Set wood to {amount}";
            }
            else
            {
                return "ResourceManager not found.";
            }
        }

        private string ShowResourcesCommand(string[] args)
        {
            if (resourceManager != null)
            {
                return $"Resources:\n  Food: {resourceManager.GetFood()}\n  Faith: {resourceManager.GetFaith()}\n  Wood: {resourceManager.GetWood()}";
            }
            else
            {
                return "ResourceManager not found.";
            }
        }        private string SetAIStateCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: set_ai_state <state_name> [pop_id]";
            }

            string stateName = args[0].ToLower();
            Pop targetPop = GetTargetPop(args.Length > 1 ? args[1] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            var stateMachine = targetPop.GetComponent<AI.PopStateMachine>();
            if (stateMachine == null)
            {
                return "Pop has no state machine.";
            }

            AI.States.IState newState = null;
            switch (stateName)
            {
                case "idle":
                    newState = new AI.States.IdleState();
                    break;
                case "wander":
                    newState = new AI.States.WanderState();
                    break;
                case "forage":
                    newState = new AI.States.ForageState();
                    break;
                case "commanded":
                    newState = new AI.States.CommandedState();
                    break;
                case "wait":
                    newState = new AI.States.WaitState();
                    break;
                default:
                    return $"Unknown state '{stateName}'. Available states: idle, wander, forage, commanded, wait";
            }

            stateMachine.ForceChangeState(newState);
            return $"Set AI state of pop {targetPop.GetInstanceID()} to {stateName}";
        }        private string ListAIStatesCommand(string[] args)
        {
            return "Available AI States:\n" +
                   "  idle - Pop stands still and recovers\n" +
                   "  wander - Pop moves around randomly\n" +
                   "  forage - Pop searches for food\n" +
                   "  commanded - Pop follows player commands\n" +
                   "  wait - Pop waits in place";
        }

        private string PopInfoCommand(string[] args)
        {
            Pop targetPop = GetTargetPop(args.Length > 0 ? args[0] : null);
            if (targetPop == null)
            {
                return "No valid pop found.";
            }

            var controller = targetPop.GetComponent<PopController>();
            string result = $"Pop {targetPop.GetInstanceID()} Info:\n";
            result += $"  Position: {targetPop.transform.position}\n";
            result += $"  Health: {targetPop.health:F1}/{targetPop.maxHealth}\n";
            result += $"  Hunger: {targetPop.hunger:F1}\n";
            result += $"  Thirst: {targetPop.thirst:F1}\n";
            result += $"  Energy: {targetPop.energy:F1}\n";
            result += $"  AI State: {controller?.GetCurrentStateName() ?? "Unknown"}\n";
            result += $"  Selected: {(controller != null && selectionManager != null && selectionManager.GetSelectedPops().Contains(controller.gameObject))}\n";

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

        private string ToggleDebugCommand(string[] args)
        {
            var debugManager = FindFirstObjectByType<DebugManager>();
            if (debugManager != null)
            {
                // Toggle debug visualization - adjust based on your DebugManager implementation
                return "Debug visualization toggled [Implementation needed]";
            }
            else
            {
                return "DebugManager not found.";
            }
        }

        private string ToggleStatsCommand(string[] args)
        {
            var statsOverlay = FindFirstObjectByType<DebugStatsOverlay>();
            if (statsOverlay != null)
            {
                // Toggle stats overlay - adjust based on your implementation
                return "Stats overlay toggled [Implementation needed]";
            }
            else
            {
                return "DebugStatsOverlay not found.";
            }
        }

        private string DebugPopPathsCommand(string[] args)
        {
            // Toggle pop path visualization
            return "Pop path visualization toggled [Implementation needed]";
        }

        #endregion

        #region Helper Methods

        private Pop GetTargetPop(string popIdString)
        {
            if (!string.IsNullOrEmpty(popIdString) && int.TryParse(popIdString, out int popId))
            {
                // Get by ID
                var allPops = FindObjectsByType<Pop>(FindObjectsSortMode.None);
                return allPops.FirstOrDefault(p => p.GetInstanceID() == popId);
            }
            else
            {
                // Get selected pop
                if (selectionManager != null)
                {
                    var selectedPops = selectionManager.GetSelectedPops();
                    if (selectedPops.Count > 0)
                    {
                        var popController = selectedPops[0].GetComponent<PopController>();
                        return popController?.GetPop();
                    }
                }
            }

            return null;
        }

        #endregion

        private void OnDestroy()
        {
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
        }
    }
}

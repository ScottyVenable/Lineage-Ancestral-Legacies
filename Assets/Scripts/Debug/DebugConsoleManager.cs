using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Systems.Inventory;
using Lineage.Ancestral.Legacies.Debug;
using TMPro;

namespace Lineage.Ancestral.Legacies.Debug
{
    [System.Serializable]
    public class ParsedCommand
    {
        public string CommandName;
        public List<string> Arguments;
        public Dictionary<string, string> Options;

        public ParsedCommand()
        {
            CommandName = "";
            Arguments = new List<string>();
            Options = new Dictionary<string, string>();
        }
    }

    public class DebugConsoleManager : MonoBehaviour
    {
        [Header("Console Settings")]
        [SerializeField] private bool enableConsole = true;
        [SerializeField] private KeyCode legacyToggleKey = KeyCode.F2;
        [SerializeField] private KeyCode legacyToggleKeyAlt = KeyCode.BackQuote; // Tilde/backtick
        [SerializeField] private int maxHistoryCount = 50;
        [SerializeField] private int maxLogCount = 100;

        [Header("Log Filtering")]
        [SerializeField] public bool showInfoLogs = true;
        [SerializeField] public bool showWarningLogs = true;
        [SerializeField] public bool showErrorLogs = true;
        [SerializeField] public bool showDebugLogs = true;
        [SerializeField] public bool showCommandLogs = true;
        [SerializeField] public bool showSystemLogs = true;
        [SerializeField] public bool showTimestamps = true;

        [Header("UI Settings")]
        [SerializeField] private Vector2 consoleSize = new Vector2(800, 400);
        [SerializeField] private Vector2 consolePosition = new Vector2(50, 50);
        [SerializeField] private bool isDraggable = true;
        [SerializeField] private bool isResizable = true;
        [SerializeField] private Texture2D resizeCursor;

        [Header("Font Settings")]
        [SerializeField] public Font consolebodyFont;
        [SerializeField] public Font consoleheaderFont;
        [SerializeField] public Font consoleInputFont;
        [SerializeField] public Font consoleButtonFont;
        [SerializeField] public Font consoleSuggestionFont;
        
        [Header("Font Sizes")]
        [SerializeField] public int logTextFontSize = 12;
        [SerializeField] public int inputFieldFontSize = 14;
        [SerializeField] public int headerFontSize = 16;
        [SerializeField] public int buttonFontSize = 12;
        [SerializeField] public int suggestionFontSize = 11;

        // Input System
        private InputAction toggleConsoleAction;
        
        // Console state
        private bool isConsoleVisible = false;
        private string currentInput = "";
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 logScrollPosition = Vector2.zero;
        private bool isInitialized = false;
        
        // Command system
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;
        public List<LogEntry> logEntries = new List<LogEntry>();
        private bool isInputFocused = false;
        
        // Command suggestions
        private List<string> availableCommands = new List<string>();
        private List<string> filteredSuggestions = new List<string>();
        private bool showSuggestions = false;
        private int selectedSuggestionIndex = -1;
        
        // GUI styles
        private GUIStyle logStyle;
        private GUIStyle inputStyle;
        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;
        private GUIStyle suggestionStyle;
        private GUIStyle windowStyle;
        private bool stylesInitialized = false;
        
        // Window properties
        private Rect windowRect;
        private bool isDragging = false;
        private bool isResizing = false;
        private Vector2 dragOffset;
        private Vector2 resizeOffset;

        // Manager references
        private PopulationManager populationManager;
        private ResourceManager resourceManager;
        private SelectionManager selectionManager;

        // Log entry types
        public enum LogType
        {
            Info,
            Warning,
            Error,
            Debug,
            Command,
            System
        }

        [System.Serializable]
        public class LogEntry
        {
            public string message;
            public LogType type;
            public DateTime timestamp;
            public Color color;

            public LogEntry(string msg, LogType logType, Color logColor)
            {
                message = msg;
                type = logType;
                timestamp = DateTime.Now;
                color = logColor;
            }
        }

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

        // Command registry
        private Dictionary<string, ConsoleCommand> registeredCommands = new Dictionary<string, ConsoleCommand>();

        // Singleton pattern
        public static DebugConsoleManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeConsole();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            SetupInputActions();
            LoadAvailableCommands();
            windowRect = new Rect(consolePosition.x, consolePosition.y, consoleSize.x, consoleSize.y);
            
            // Get manager references
            populationManager = FindFirstObjectByType<PopulationManager>();
            resourceManager = FindFirstObjectByType<ResourceManager>();
            selectionManager = FindFirstObjectByType<SelectionManager>();
            
            LogToConsole("Debug Console initialized. Type 'help' for available commands.", LogType.System, Color.green);
        }

        void Update()
        {
            if (!enableConsole) return;

            if (!isConsoleVisible) return;

            UpdateSuggestions();
        }

        void OnGUI()
        {
            if (!enableConsole || !isConsoleVisible) return;
            
            InitializeStyles();
            
            windowRect = GUI.Window(0, windowRect, DrawConsoleWindow, "Debug Console", windowStyle);
            
            // Clamp window to screen
            windowRect.x = Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
            windowRect.y = Mathf.Clamp(windowRect.y, 0, Screen.height - windowRect.height);
        }

        private void InitializeConsole()
        {
            if (isInitialized) return;
            
            commandHistory = new List<string>();
            availableCommands = new List<string>();
            filteredSuggestions = new List<string>();
            
            Application.logMessageReceived += HandleUnityLog;
            
            isInitialized = true;
        }

        private void SetupInputActions()
        {
            var inputActions = new InputActionMap("DebugConsole");
            
            // Existing toggle action
            toggleConsoleAction = inputActions.AddAction("ToggleConsole", InputActionType.Button);
            toggleConsoleAction.AddBinding("<Keyboard>/f2");
            toggleConsoleAction.AddBinding("<Keyboard>/backquote");
            toggleConsoleAction.performed += _ => ToggleConsole();
            
            // Add navigation actions
            var executeAction = inputActions.AddAction("Execute", InputActionType.Button);
            executeAction.AddBinding("<Keyboard>/enter");
            executeAction.AddBinding("<Keyboard>/numpadEnter");
            executeAction.performed += _ => { if (isInputFocused) ExecuteCommand(); };
            
            var historyUpAction = inputActions.AddAction("HistoryUp", InputActionType.Button);
            historyUpAction.AddBinding("<Keyboard>/upArrow");
            historyUpAction.performed += _ => { if (isInputFocused) NavigateHistory(-1); };
            
            var historyDownAction = inputActions.AddAction("HistoryDown", InputActionType.Button);
            historyDownAction.AddBinding("<Keyboard>/downArrow");
            historyDownAction.performed += _ => { if (isInputFocused) NavigateHistory(1); };
            
            var tabAction = inputActions.AddAction("Tab", InputActionType.Button);
            tabAction.AddBinding("<Keyboard>/tab");
            tabAction.performed += _ => { 
                if (isInputFocused && showSuggestions && filteredSuggestions.Count > 0) 
                    SelectSuggestion(selectedSuggestionIndex >= 0 ? selectedSuggestionIndex : 0); 
            };
              var escapeAction = inputActions.AddAction("Escape", InputActionType.Button);
            escapeAction.AddBinding("<Keyboard>/escape");
            escapeAction.performed += _ => {
                if (isInputFocused) {
                    if (showSuggestions) {
                        showSuggestions = false;
                        selectedSuggestionIndex = -1;
                    } else {
                        ToggleConsole();
                    }
                } else if (!isConsoleVisible) {
                    // Quit the game when console is not open and escape is pressed
                    Application.Quit();
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #endif
                }
            };
            
            inputActions.Enable();
        }

        private void ToggleConsole()
        {
            isConsoleVisible = !isConsoleVisible;
            
            if (isConsoleVisible)
            {
                GUI.FocusControl("ConsoleInput");
                isInputFocused = true;
            }
            else
            {
                isInputFocused = false;
                showSuggestions = false;
                selectedSuggestionIndex = -1;
            }
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            CreateLogStyle();
            CreateInputStyle();
            CreateHeaderStyle();
            CreateButtonStyle();
            CreateSuggestionStyle();
            CreateWindowStyle();
            
            stylesInitialized = true;
        }

        private void CreateLogStyle()
        {
            logStyle = new GUIStyle(GUI.skin.label);
            logStyle.font = consolebodyFont;
            logStyle.fontSize = logTextFontSize;
            logStyle.wordWrap = true;
            logStyle.richText = true;
            logStyle.normal.textColor = Color.white;
            logStyle.padding = new RectOffset(5, 5, 2, 2);
            logStyle.margin = new RectOffset(0, 0, 0, 0);
        }        private void CreateInputStyle()
        {
            inputStyle = new GUIStyle(GUI.skin.textField);
            inputStyle.font = consoleInputFont;
            inputStyle.fontSize = inputFieldFontSize;
            inputStyle.normal.textColor = Color.white;
            inputStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.8f));
            inputStyle.focused.textColor = Color.white;
            inputStyle.focused.background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 0.8f));
            inputStyle.hover.textColor = Color.white;
            inputStyle.hover.background = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f, 0.8f));
            inputStyle.active.textColor = Color.white;
            inputStyle.active.background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 0.8f));
            inputStyle.padding = new RectOffset(5, 5, 5, 5);
        }

        private void CreateHeaderStyle()
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.font = consoleheaderFont;
            headerStyle.fontSize = headerFontSize;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;
            headerStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void CreateButtonStyle()
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.font = consoleButtonFont;
            buttonStyle.fontSize = buttonFontSize;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.normal.background = MakeTex(2, 2, new Color(0.4f, 0.4f, 0.4f, 0.8f));
            buttonStyle.hover.background = MakeTex(2, 2, new Color(0.5f, 0.5f, 0.5f, 0.8f));
            buttonStyle.active.background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 0.8f));
        }

        private void CreateSuggestionStyle()
        {
            suggestionStyle = new GUIStyle(GUI.skin.button);
            suggestionStyle.font = consoleSuggestionFont;
            suggestionStyle.fontSize = suggestionFontSize;
            suggestionStyle.alignment = TextAnchor.MiddleLeft;
            suggestionStyle.normal.textColor = Color.white;
            suggestionStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 0.9f));
            suggestionStyle.hover.background = MakeTex(2, 2, new Color(0.4f, 0.4f, 0.4f, 0.9f));
            suggestionStyle.padding = new RectOffset(5, 5, 2, 2);
        }

        private void CreateWindowStyle()
        {
            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.9f));
            windowStyle.padding = new RectOffset(10, 10, 20, 10);
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void DrawConsoleWindow(int windowID)
        {
            GUILayout.BeginVertical();
            
            // Header with filter toggles
            DrawFilterToggles();
            
            // Log display area
            DrawLogArea();
            
            // Input area
            DrawInputArea();
            
            // Suggestions
            if (showSuggestions && filteredSuggestions.Count > 0)
            {
                DrawSuggestions();
            }
            
            GUILayout.EndVertical();
            
            HandleWindowDragging();
        }

        private void DrawFilterToggles()
        {
            GUILayout.BeginHorizontal();
            
            showInfoLogs = GUILayout.Toggle(showInfoLogs, "Info", buttonStyle, GUILayout.Width(50));
            showWarningLogs = GUILayout.Toggle(showWarningLogs, "Warn", buttonStyle, GUILayout.Width(50));
            showErrorLogs = GUILayout.Toggle(showErrorLogs, "Error", buttonStyle, GUILayout.Width(50));
            showDebugLogs = GUILayout.Toggle(showDebugLogs, "Debug", buttonStyle, GUILayout.Width(50));
            showCommandLogs = GUILayout.Toggle(showCommandLogs, "Cmd", buttonStyle, GUILayout.Width(50));
            showSystemLogs = GUILayout.Toggle(showSystemLogs, "Sys", buttonStyle, GUILayout.Width(50));
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Clear", buttonStyle, GUILayout.Width(60)))
            {
                ClearLogs();
            }
            
            GUILayout.EndHorizontal();
        }

        private void DrawLogArea()
        {
            logScrollPosition = GUILayout.BeginScrollView(logScrollPosition, GUILayout.Height(consoleSize.y - 120));
            
            var filteredLogs = GetFilteredLogs();
            
            foreach (var log in filteredLogs)
            {
                GUI.color = log.color;
                
                string displayText = log.message;
                if (showTimestamps)
                {
                    displayText = $"[{log.timestamp:HH:mm:ss}] {displayText}";
                }
                
                GUILayout.Label(displayText, logStyle);
            }
            
            GUI.color = Color.white;
            GUILayout.EndScrollView();
        }        private void DrawInputArea()
        {
            GUILayout.BeginHorizontal();
            
            GUI.SetNextControlName("ConsoleInput");
            
            // Store the current GUI color to restore after
            Color originalColor = GUI.color;
            Color originalBackgroundColor = GUI.backgroundColor;
            
            // Ensure consistent styling regardless of focus state
            GUI.color = Color.white;
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            string newInput = GUILayout.TextField(currentInput, inputStyle, GUILayout.ExpandWidth(true));
            
            // Restore original GUI colors
            GUI.color = originalColor;
            GUI.backgroundColor = originalBackgroundColor;
            
            if (newInput != currentInput)
            {
                currentInput = newInput;
                UpdateSuggestions();
            }
            
            if (GUILayout.Button("Execute", buttonStyle, GUILayout.Width(80)))
            {
                ExecuteCommand();
            }
            
            GUILayout.EndHorizontal();
            
            HandleInputKeys();
        }

        private void DrawSuggestions()
        {
            GUILayout.BeginVertical();
            
            for (int i = 0; i < Mathf.Min(filteredSuggestions.Count, 5); i++)
            {
                bool isSelected = i == selectedSuggestionIndex;
                GUI.color = isSelected ? Color.yellow : Color.white;
                
                if (GUILayout.Button(filteredSuggestions[i], suggestionStyle))
                {
                    SelectSuggestion(i);
                }
            }
            
            GUI.color = Color.white;
            GUILayout.EndVertical();
        }

        private void HandleWindowDragging()
        {
            if (isDraggable)
            {
                GUI.DragWindow();
            }
        }


        private void HandleInputKeys()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == "ConsoleInput")
            {
                if (showSuggestions && filteredSuggestions.Count > 0)
                {
                    if (e.keyCode == KeyCode.UpArrow)
                    {
                        selectedSuggestionIndex = Mathf.Max(-1, selectedSuggestionIndex - 1);
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.DownArrow)
                    {
                        selectedSuggestionIndex = Mathf.Min(filteredSuggestions.Count - 1, selectedSuggestionIndex + 1);
                        e.Use();
                    }
                }
            }
        }

        private void UpdateSuggestions()
        {
            if (string.IsNullOrEmpty(currentInput))
            {
                showSuggestions = false;
                selectedSuggestionIndex = -1;
                return;
            }
            
            filteredSuggestions.Clear();
            
            foreach (string command in availableCommands)
            {
                if (command.StartsWith(currentInput, StringComparison.OrdinalIgnoreCase))
                {
                    filteredSuggestions.Add(command);
                }
            }
            
            showSuggestions = filteredSuggestions.Count > 0;
            selectedSuggestionIndex = showSuggestions ? 0 : -1;
        }

        private void SelectSuggestion(int index)
        {
            if (index >= 0 && index < filteredSuggestions.Count)
            {
                currentInput = filteredSuggestions[index];
                showSuggestions = false;
                selectedSuggestionIndex = -1;
                GUI.FocusControl("ConsoleInput");
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

        private void ExecuteCommand()
        {
            if (string.IsNullOrEmpty(currentInput.Trim())) return;
            
            string command = currentInput.Trim();
            
            // Add to history
            if (commandHistory.Count == 0 || commandHistory[commandHistory.Count - 1] != command)
            {
                commandHistory.Add(command);
                if (commandHistory.Count > maxHistoryCount)
                {
                    commandHistory.RemoveAt(0);
                }
            }
            
            historyIndex = -1;
            
            // Log the command
            LogToConsole($"> {command}", LogType.Command, Color.cyan);
            
            // Parse and execute
            ParsedCommand parsedCommand = ParseCommandLine(command);
            ProcessCommand(parsedCommand);
            
            // Clear input
            currentInput = "";
            showSuggestions = false;
            selectedSuggestionIndex = -1;
            
            // Auto-scroll to bottom
            logScrollPosition.y = float.MaxValue;
        }

        private ParsedCommand ParseCommandLine(string commandLine)
        {
            ParsedCommand parsed = new ParsedCommand();
            
            if (string.IsNullOrEmpty(commandLine)) return parsed;
            
            // Simple parsing - split by spaces, handle quoted arguments
            var matches = Regex.Matches(commandLine, @"[\""].+?[\""]|[^ ]+");
            var parts = matches.Cast<Match>().Select(m => m.Value.Trim('"')).ToList();
            
            if (parts.Count > 0)
            {
                parsed.CommandName = parts[0].ToLower();
                
                
                for (int i = 1; i < parts.Count; i++)
                {
                    string part = parts[i];
                    
                    // Check if it's an option (starts with -)
                    if (part.StartsWith("-"))
                    {
                        string optionName = part.TrimStart('-');
                        string optionValue = "";
                        
                        // Check if next part is the value
                        if (i + 1 < parts.Count && !parts[i + 1].StartsWith("-"))
                        {
                            optionValue = parts[i + 1];
                            i++; // Skip the value in next iteration
                        }
                        
                        parsed.Options[optionName] = optionValue;
                    }
                    else
                    {
                        parsed.Arguments.Add(part);
                    }
                }
            }
            
            return parsed;
        }        private void ProcessCommand(ParsedCommand command)
        {
            try
            {
                // First check if it's a registered command
                if (registeredCommands.ContainsKey(command.CommandName))
                {
                    var registeredCommand = registeredCommands[command.CommandName];
                    
                    // Handle contextual entity targeting
                    var positionalArgs = new List<string>(command.Arguments);
                    if (registeredCommand.requiresEntityTarget && positionalArgs.Count == 0)
                    {
                        var selectedEntity = GetSelectedEntityTarget();
                        if (selectedEntity != null)
                        {
                            positionalArgs.Insert(0, selectedEntity.GetInstanceID().ToString());
                        }
                    }
                    
                    // Convert options to data block
                    var dataBlock = new Dictionary<string, object>();
                    foreach (var option in command.Options)
                    {
                        dataBlock[option.Key] = option.Value;
                    }
                    
                    string result = registeredCommand.command(positionalArgs, dataBlock);
                    if (!string.IsNullOrEmpty(result))
                    {
                        LogToConsole(result, LogType.Info, Color.white);
                    }
                    return;
                }
                
                // Fall back to built-in commands
                switch (command.CommandName)
                {
                    case "help":
                        ShowHelp();
                        break;
                    case "clear":
                        ClearLogs();
                        break;
                    case "quit":
                    case "exit":
                        QuitApplication();
                        break;
                    case "scene":
                        HandleSceneCommand(command);
                        break;
                    case "spawn":
                    case "spawn_pop":
                        HandleSpawnCommand(command);
                        break;
                    case "teleport":
                    case "tp":
                        HandleTeleportCommand(command);
                        break;
                    case "give":
                        HandleGiveCommand(command);
                        break;
                    case "god":
                        HandleGodModeCommand(command);
                        break;
                    case "noclip":
                        HandleNoClipCommand(command);
                        break;
                    case "time":
                        HandleTimeCommand(command);
                        break;
                    case "fps":
                        HandleFpsCommand(command);
                        break;
                    case "debug":
                        HandleDebugCommand(command);
                        break;
                    case "resources":
                        ShowResourcesCommand();
                        break;                    case "list_pops":
                        ListPopsCommand();
                        break;
                    case "pause":
                        HandlePauseCommand();
                        break;
                    case "resume":
                        HandleResumeCommand();
                        break;
                    case "speed":
                        HandleSpeedCommand(command);
                        break;
                    case "version":
                        HandleVersionCommand();
                        break;
                    case "memory":
                        HandleMemoryCommand();
                        break;
                    case "screenshot":
                        HandleScreenshotCommand(command);
                        break;
                    case "fullscreen":
                        HandleFullscreenCommand();
                        break;
                    default:
                        LogToConsole($"Unknown command: {command.CommandName}. Type 'help' for available commands.", LogType.Error, Color.red);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogToConsole($"Error executing command: {ex.Message}", LogType.Error, Color.red);
            }
        }        private void LoadAvailableCommands()
        {
            availableCommands.Clear();
              // Add built-in commands
            availableCommands.AddRange(new string[]
            {
                "help", "clear", "quit", "exit", "scene", "spawn", "spawn_pop", "teleport", "tp", 
                "give", "god", "noclip", "time", "fps", "debug", "resources", "list_pops", 
                "pause", "resume", "speed", "version", "memory", "screenshot", "fullscreen"
            });
            
            // Add registered commands
            foreach (var command in registeredCommands.Keys)
            {
                if (!availableCommands.Contains(command))
                {
                    availableCommands.Add(command);
                }
            }
        }

        private List<LogEntry> GetFilteredLogs()
        {
            return logEntries.Where(log =>
            {
                switch (log.type)
                {
                    case LogType.Info: return showInfoLogs;
                    case LogType.Warning: return showWarningLogs;
                    case LogType.Error: return showErrorLogs;
                    case LogType.Debug: return showDebugLogs;
                    case LogType.Command: return showCommandLogs;
                    case LogType.System: return showSystemLogs;
                    default: return true;
                }
            }).TakeLast(maxLogCount).ToList();
        }

        public void LogToConsole(string message, LogType type = LogType.Info, Color? color = null)
        {
            Color logColor = color ?? GetDefaultColorForLogType(type);
            LogEntry entry = new LogEntry(message, type, logColor);
            
            logEntries.Add(entry);
            
            // Trim log entries if needed
            if (logEntries.Count > maxLogCount * 2) // Keep double to account for filtering
            {
                int removeCount = logEntries.Count - maxLogCount;
                logEntries.RemoveRange(0, removeCount);
            }
        }

        private Color GetDefaultColorForLogType(LogType type)
        {
            switch (type)
            {
                case LogType.Info: return Color.white;
                case LogType.Warning: return Color.yellow;
                case LogType.Error: return Color.red;
                case LogType.Debug: return Color.gray;
                case LogType.Command: return Color.cyan;
                case LogType.System: return Color.green;
                default: return Color.white;
            }
        }

        private void HandleUnityLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            LogType consoleLogType = LogType.Info;
            Color logColor = Color.white;
            
            switch (type)
            {
                case UnityEngine.LogType.Error:
                case UnityEngine.LogType.Exception:
                    consoleLogType = LogType.Error;
                    logColor = Color.red;
                    break;
                case UnityEngine.LogType.Warning:
                    consoleLogType = LogType.Warning;
                    logColor = Color.yellow;
                    break;
                case UnityEngine.LogType.Log:
                    consoleLogType = LogType.Info;
                    logColor = Color.white;
                    break;
            }
            
            LogToConsole($"[Unity] {logString}", consoleLogType, logColor);
        }

        private void ClearLogs()
        {
            logEntries.Clear();
            LogToConsole("Console cleared.", LogType.System, Color.green);
        }        private void ShowHelp()
        {
            LogToConsole("Available Commands:", LogType.System, Color.green);
            
            // Show built-in commands
            LogToConsole("Built-in Commands:", LogType.Info, Color.cyan);
            LogToConsole("  help - Show this help message", LogType.Info, Color.white);
            LogToConsole("  clear - Clear the console log", LogType.Info, Color.white);
            LogToConsole("  quit/exit - Quit the application", LogType.Info, Color.white);
            LogToConsole("  scene <name> - Load a scene", LogType.Info, Color.white);
            LogToConsole("  spawn <x> <y> [z] - Spawn a pop at coordinates", LogType.Info, Color.white);
            LogToConsole("  spawn_pop <x> <y> [z] - Spawn a pop at coordinates", LogType.Info, Color.white);
            LogToConsole("  teleport/tp <x> <y> <z> - Teleport player", LogType.Info, Color.white);
            LogToConsole("  give <item> [amount] - Give item to player", LogType.Info, Color.white);
            LogToConsole("  god [on/off] - Toggle god mode", LogType.Info, Color.white);
            LogToConsole("  noclip [on/off] - Toggle no-clip mode", LogType.Info, Color.white);
            LogToConsole("  time <scale> - Set time scale", LogType.Info, Color.white);
            LogToConsole("  fps [show/hide] - Toggle FPS display", LogType.Info, Color.white);
            LogToConsole("  debug <system> [on/off] - Toggle debug for system", LogType.Info, Color.white);
            LogToConsole("  resources - Show current resources", LogType.Info, Color.white);
            LogToConsole("  list_pops - List all pops", LogType.Info, Color.white);
            LogToConsole("  pause - Pause the game", LogType.Info, Color.white);
            LogToConsole("  resume - Resume the game", LogType.Info, Color.white);
            LogToConsole("  speed <value> - Set game speed (0.1-4.0)", LogType.Info, Color.white);
            LogToConsole("  version - Show version information", LogType.Info, Color.white);
            LogToConsole("  memory - Show memory usage and perform GC", LogType.Info, Color.white);
            LogToConsole("  screenshot [filename] - Take a screenshot", LogType.Info, Color.white);
            LogToConsole("  fullscreen - Toggle fullscreen mode", LogType.Info, Color.white);
            
            // Show registered commands
            if (registeredCommands.Count > 0)
            {
                LogToConsole("", LogType.Info, Color.white); // Empty line
                LogToConsole("Registered Commands:", LogType.Info, Color.cyan);
                foreach (var command in registeredCommands.Values.OrderBy(c => c.name))
                {
                    LogToConsole($"  {command.name} - {command.description}", LogType.Info, Color.white);
                }
            }
        }

        private void QuitApplication()
        {
            LogToConsole("Quitting application...", LogType.System, Color.yellow);
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void HandleSceneCommand(ParsedCommand command)
        {
            if (command.Arguments.Count == 0)
            {
                LogToConsole("Current scene: " + SceneManager.GetActiveScene().name, LogType.Info, Color.white);
                LogToConsole("Usage: scene <sceneName>", LogType.Info, Color.white);
                return;
            }
            
            string sceneName = command.Arguments[0];
            
            try
            {
                SceneManager.LoadScene(sceneName);
                LogToConsole($"Loading scene: {sceneName}", LogType.System, Color.green);
            }
            catch (Exception ex)
            {
                LogToConsole($"Failed to load scene '{sceneName}': {ex.Message}", LogType.Error, Color.red);
            }
        }

        private void HandleSpawnCommand(ParsedCommand command)
        {
            if (command.Arguments.Count < 2)
            {
                LogToConsole("Usage: spawn <x> <y> [z]", LogType.Info, Color.white);
                return;
            }
            
            if (!float.TryParse(command.Arguments[0], out float x) || !float.TryParse(command.Arguments[1], out float y))
            {
                LogToConsole("Invalid coordinates. Usage: spawn <x> <y> [z]", LogType.Error, Color.red);
                return;
            }

            float z = command.Arguments.Count > 2 && float.TryParse(command.Arguments[2], out float zVal) ? zVal : 0f;
            Vector3 position = new Vector3(x, y, z);

            if (populationManager != null)
            {
                try
                {
                    populationManager.SpawnPop();
                    
                    // Find the most recently spawned pop
                    var allPops = FindObjectsByType<Pop>(FindObjectsSortMode.None);
                    if (allPops.Length > 0)
                    {
                        GameObject spawnedPop = allPops[allPops.Length - 1].gameObject;
                        spawnedPop.transform.position = new Vector3(position.x, position.y, 0f);
                        
                        // Set sorting layer to "Entities"
                        var renderer = spawnedPop.GetComponent<SpriteRenderer>();
                        if (renderer != null)
                        {
                            renderer.sortingLayerName = "Entities";
                        }
                        
                        LogToConsole($"Spawned pop at {spawnedPop.transform.position}", LogType.System, Color.green);
                    }
                    else
                    {
                        LogToConsole("Failed to find the spawned pop", LogType.Error, Color.red);
                    }
                }
                catch (Exception e)
                {
                    LogToConsole($"Error spawning pop: {e.Message}", LogType.Error, Color.red);
                }
            }
            else
            {
                LogToConsole("PopulationManager not found.", LogType.Warning, Color.yellow);
            }
        }

        private void HandleTeleportCommand(ParsedCommand command)
        {
            if (command.Arguments.Count < 3)
            {
                LogToConsole("Usage: teleport/tp <x> <y> <z>", LogType.Info, Color.white);
                return;
            }
            
            if (float.TryParse(command.Arguments[0], out float x) &&
                float.TryParse(command.Arguments[1], out float y) &&
                float.TryParse(command.Arguments[2], out float z))
            {
                Vector3 position = new Vector3(x, y, z);
                
                var target = GetSelectedEntityTarget();
                if (target != null)
                {
                    target.transform.position = position;
                    LogToConsole($"Teleported to {position}", LogType.System, Color.green);
                }
                else
                {
                    LogToConsole("No target found to teleport", LogType.Warning, Color.yellow);
                }
            }
            else
            {
                LogToConsole("Invalid coordinates. Usage: teleport <x> <y> <z>", LogType.Error, Color.red);
            }
        }

        private void HandleGiveCommand(ParsedCommand command)
        {
            if (command.Arguments.Count == 0)
            {
                LogToConsole("Usage: give <itemName> [amount]", LogType.Info, Color.white);
                return;
            }
            
            string itemName = command.Arguments[0];
            int amount = 1;
            
            if (command.Arguments.Count > 1)
            {
                if (!int.TryParse(command.Arguments[1], out amount))
                {
                    amount = 1;
                }
            }
            
            LogToConsole($"Giving {amount}x {itemName} to player", LogType.System, Color.green);
            // TODO: Implement item giving logic based on your inventory system
        }

        private void HandleGodModeCommand(ParsedCommand command)
        {
            bool enable = true;
            
            if (command.Arguments.Count > 0)
            {
                string arg = command.Arguments[0].ToLower();
                enable = arg == "on" || arg == "true" || arg == "1";
            }
            
            LogToConsole($"God mode {(enable ? "enabled" : "disabled")}", LogType.System, Color.green);
            // TODO: Implement god mode logic
        }

        private void HandleNoClipCommand(ParsedCommand command)
        {
            bool enable = true;
            
            if (command.Arguments.Count > 0)
            {
                string arg = command.Arguments[0].ToLower();
                enable = arg == "on" || arg == "true" || arg == "1";
            }
            
            LogToConsole($"No-clip mode {(enable ? "enabled" : "disabled")}", LogType.System, Color.green);
            // TODO: Implement no-clip logic
        }

        private void HandleTimeCommand(ParsedCommand command)
        {
            if (command.Arguments.Count == 0)
            {
                LogToConsole($"Current time scale: {Time.timeScale}", LogType.Info, Color.white);
                LogToConsole("Usage: time <scale>", LogType.Info, Color.white);
                return;
            }
            
            if (float.TryParse(command.Arguments[0], out float timeScale))
            {
                Time.timeScale = Mathf.Max(0, timeScale);
                LogToConsole($"Time scale set to {Time.timeScale}", LogType.System, Color.green);
            }
            else
            {
                LogToConsole("Invalid time scale value", LogType.Error, Color.red);
            }
        }

        private void HandleFpsCommand(ParsedCommand command)
        {
            LogToConsole("FPS display toggle functionality not implemented", LogType.Warning, Color.yellow);
            // TODO: Implement FPS display toggle
        }

        private void HandleDebugCommand(ParsedCommand command)
        {
            if (command.Arguments.Count == 0)
            {
                LogToConsole("Usage: debug <system> [on/off]", LogType.Info, Color.white);
                LogToConsole("Available systems: rendering, physics, ai, networking", LogType.Info, Color.white);
                return;
            }
            
            string system = command.Arguments[0];
            bool enable = true;
            
            if (command.Arguments.Count > 1)
            {
                string arg = command.Arguments[1].ToLower();
                enable = arg == "on" || arg == "true" || arg == "1";
            }
            
            LogToConsole($"Debug for {system} {(enable ? "enabled" : "disabled")}", LogType.System, Color.green);
            // TODO: Implement system-specific debug toggles
        }

        private void ShowResourcesCommand()
        {
            if (resourceManager != null)
            {
                LogToConsole($"Resources:", LogType.Info, Color.white);
                LogToConsole($"  Food: {resourceManager.currentFood}", LogType.Info, Color.white);
                LogToConsole($"  Faith: {resourceManager.currentFaithPoints}", LogType.Info, Color.white);
                LogToConsole($"  Wood: {resourceManager.currentWood}", LogType.Info, Color.white);
            }
            else
            {
                LogToConsole("ResourceManager not found.", LogType.Warning, Color.yellow);
            }
        }

        private void ListPopsCommand()
        {
            var allPops = FindObjectsByType<Pop>(FindObjectsSortMode.None);
            
            if (allPops.Length == 0)
            {
                LogToConsole("No pops found.", LogType.Info, Color.white);
                return;
            }

            LogToConsole($"Found {allPops.Length} pops:", LogType.Info, Color.white);
            foreach (var pop in allPops)
            {
                var animator = pop.GetComponent<Animator>();
                string stateName = "Unknown";
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    stateName = stateInfo.IsName("") ? "Unknown" : animator.GetLayerName(0);
                }
                LogToConsole($"  ID: {pop.GetInstanceID()}, Position: {pop.transform.position}, State: {stateName}, Health: {pop.health:F1}", LogType.Info, Color.white);
            }        }

        private void HandlePauseCommand()
        {
            var timeManager = TimeManager.Instance;
            if (timeManager != null)
            {
                timeManager.SetPaused(true);
                LogToConsole("Game paused.", LogType.System, Color.yellow);
            }
            else
            {
                LogToConsole("TimeManager not found.", LogType.Error, Color.red);
            }
        }

        private void HandleResumeCommand()
        {
            var timeManager = TimeManager.Instance;
            if (timeManager != null)
            {
                timeManager.SetPaused(false);
                LogToConsole("Game resumed.", LogType.System, Color.yellow);
            }
            else
            {
                LogToConsole("TimeManager not found.", LogType.Error, Color.red);
            }
        }

        private void HandleSpeedCommand(ParsedCommand command)
        {
            var timeManager = TimeManager.Instance;
            if (timeManager == null)
            {
                LogToConsole("TimeManager not found.", LogType.Error, Color.red);
                return;
            }

            if (command.Arguments.Count == 0)
            {
                LogToConsole($"Current game speed: {timeManager.GameSpeed:F1}x", LogType.Info, Color.white);
                LogToConsole("Usage: speed <value> (1.0-4.0)", LogType.Info, Color.white);
                return;
            }

            if (float.TryParse(command.Arguments[0], out float speed))
            {
                timeManager.SetGameSpeed(Mathf.Clamp(speed, 0.1f, 4f));
                LogToConsole($"Game speed set to: {speed:F1}x", LogType.System, Color.yellow);
            }
            else
            {
                LogToConsole("Invalid speed value. Use a number between 0.1 and 4.0", LogType.Error, Color.red);
            }
        }

        private void HandleVersionCommand()
        {
            LogToConsole($"Unity Version: {Application.unityVersion}", LogType.Info, Color.white);
            LogToConsole($"Application Version: {Application.version}", LogType.Info, Color.white);
            LogToConsole($"Platform: {Application.platform}", LogType.Info, Color.white);
            LogToConsole($"Data Path: {Application.dataPath}", LogType.Info, Color.white);
        }

        private void HandleMemoryCommand()
        {
            long totalMemory = System.GC.GetTotalMemory(false);
            string memoryInfo = $"Allocated Memory: {totalMemory / 1024 / 1024} MB";
            LogToConsole(memoryInfo, LogType.Info, Color.white);
            
            LogToConsole($"System Memory Size: {SystemInfo.systemMemorySize} MB", LogType.Info, Color.white);
            LogToConsole($"Graphics Memory Size: {SystemInfo.graphicsMemorySize} MB", LogType.Info, Color.white);
            
            System.GC.Collect();
            LogToConsole("Garbage collection performed.", LogType.System, Color.yellow);
        }

        private void HandleScreenshotCommand(ParsedCommand command)
        {
            string filename = command.Arguments.Count > 0 ? command.Arguments[0] : $"screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            
            if (!filename.EndsWith(".png"))
                filename += ".png";
                
            string path = System.IO.Path.Combine(Application.dataPath, "..", "Screenshots");
            
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
                
            string fullPath = System.IO.Path.Combine(path, filename);
            
            ScreenCapture.CaptureScreenshot(fullPath);
            LogToConsole($"Screenshot saved to: {fullPath}", LogType.System, Color.yellow);
        }

        private void HandleFullscreenCommand()
        {
            Screen.fullScreen = !Screen.fullScreen;
            LogToConsole($"Fullscreen: {(Screen.fullScreen ? "ON" : "OFF")}", LogType.System, Color.yellow);
        }

        /// <summary>
        /// Register a new command for external systems to add custom debug commands
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="description">Command description</param>
        /// <param name="usage">Usage string</param>
        /// <param name="command">Command delegate</param>
        /// <param name="requiresEntityTarget">Whether command requires entity target</param>
        public void RegisterCommand(string name, string description, string usage, CommandDelegate command, bool requiresEntityTarget = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                LogToConsole("Cannot register command with empty name", LogType.Error, Color.red);
                return;
            }

            var consoleCommand = new ConsoleCommand(name, description, usage, command, requiresEntityTarget);
            registeredCommands[name.ToLower()] = consoleCommand;
            
            // Add to available commands for autocomplete
            if (!availableCommands.Contains(name.ToLower()))
            {
                availableCommands.Add(name.ToLower());
            }
            
            LogToConsole($"Registered command: {name}", LogType.System, Color.green);
        }

        private GameObject GetSelectedEntityTarget()
        {
            // Try to find player first
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) return player;
            
            // Fallback to main camera
            if (Camera.main != null) return Camera.main.gameObject;
              // Last resort - any camera
            Camera anyCamera = FindFirstObjectByType<Camera>();
            if (anyCamera != null) return anyCamera.gameObject;
            
            return null;
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= HandleUnityLog;
            
            if (toggleConsoleAction != null)
            {
                toggleConsoleAction.performed -= _ => ToggleConsole();
            }
        }
    }
}

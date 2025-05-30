using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Entities;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Lineage.Ancestral.Legacies.Debug
{
    /// <summary>
    /// Enhanced in-game debug console with advanced logging integration, command history, and filtering.
    /// Toggle with F2. Supports resizing, markdown-like formatting, and colored text.
    /// </summary>
    public class DebugConsoleManager : MonoBehaviour
    {
        private Canvas consoleCanvas;
        private RectTransform window;
        private RectTransform contentArea;
        private TextMeshProUGUI outputText;
        private TMP_InputField inputField;
        private ScrollRect scrollRect;
        private Button minimizeButton;
        private RectTransform resizeHandle;
        private bool isOpen = false;
        private bool isMinimized = false;
        private Vector2 dragOffset;
        private Vector2 resizeStart;
        private Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();        // Enhanced features
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;
        private List<string> suggestions = new List<string>();
        private bool showingSuggestions = false;
        private LogCategory currentLogFilter = LogCategory.All;
        private List<string> logBuffer = new List<string>();
        private int maxLogLines = 1000;
        private Dictionary<string, string> commandDescriptions = new Dictionary<string, string>();
        private GameObject suggestionPanel;
        private TextMeshProUGUI suggestionText;
        private string currentFilterText = "";
        
        // Log filtering
        public enum LogCategory
        {
            All,
            General,
            Combat,
            AI,
            Inventory,
            Quest,
            Debug,
            Warning,
            Error
        }

        [RuntimeInitializeOnLoadMethod]
        static void InitConsole()
        {
            var go = new GameObject("DebugConsoleManager");
            DontDestroyOnLoad(go);
            go.AddComponent<DebugConsoleManager>();
        }

        void Awake()
        {
            CreateUI();
            RegisterCommands();
            window.gameObject.SetActive(isOpen);
        }        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ToggleConsole();
            }
            
            if (isOpen)
            {
                HandleConsoleInput();
            }
        }
        
        private void ToggleConsole()
        {
            isOpen = !isOpen;
            window.gameObject.SetActive(isOpen);
            if (isOpen) 
            {
                inputField.ActivateInputField();
                inputField.Select();
            }
        }
        
        private void HandleConsoleInput()
        {
            // Command history navigation
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(1);
            }
            
            // Execute command
            if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(inputField.text))
            {
                ExecuteCurrentCommand();
            }
            
            // Tab for auto-completion
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                AutoComplete();
            }
            
            // Real-time suggestion updates
            if (inputField.text != currentFilterText)
            {
                currentFilterText = inputField.text;
                UpdateSuggestions();
            }
        }
        
        private void ExecuteCurrentCommand()
        {
            var line = inputField.text;
            AddToHistory(line);
            inputField.text = string.Empty;
            historyIndex = -1;
            HideSuggestions();
            AppendLine("> " + line);
            ProcessCommand(line);
        }

        void CreateUI()
        {
            // create canvas
            GameObject cgo = new GameObject("ConsoleCanvas");
            consoleCanvas = cgo.AddComponent<Canvas>();
            consoleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(cgo);

            // window
            GameObject wgo = new GameObject("ConsoleWindow");
            wgo.transform.SetParent(cgo.transform);
            window = wgo.AddComponent<RectTransform>();
            window.sizeDelta = new Vector2(600, 300);
            window.anchorMin = new Vector2(0.05f, 0.05f);
            window.anchorMax = new Vector2(0.95f, 0.5f);
            window.pivot = new Vector2(0, 1);
            var img = wgo.AddComponent<Image>(); img.color = new Color(0,0,0,0.9f);

            // header for drag + minimize
            GameObject header = new GameObject("Header");
            header.transform.SetParent(wgo.transform);
            var hrt = header.AddComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0, 1); hrt.anchorMax = new Vector2(1, 1);
            hrt.sizeDelta = new Vector2(0, 30); hrt.pivot = new Vector2(0,1);
            var himg = header.AddComponent<Image>(); himg.color = new Color(0.1f,0.1f,0.1f,1);
            // drag events
            var trig = header.AddComponent<EventTrigger>();
            AddEvent(trig, EventTriggerType.BeginDrag, e => OnBeginDrag((PointerEventData)e));
            AddEvent(trig, EventTriggerType.Drag, e => OnDrag((PointerEventData)e));
            // minimize button
            GameObject mb = new GameObject("Minimize"); mb.transform.SetParent(header.transform);
            var mbrt = mb.AddComponent<RectTransform>();
            mbrt.anchorMin = new Vector2(1,0); mbrt.anchorMax = new Vector2(1,1);
            mbrt.sizeDelta = new Vector2(30,30); mbrt.pivot = new Vector2(1,0.5f);
            minimizeButton = mb.AddComponent<Button>();
            var mbimg = mb.AddComponent<Image>(); mbimg.color = Color.gray;
            minimizeButton.onClick.AddListener(ToggleMinimize);

            // scroll area for output
            GameObject sg = new GameObject("ScrollArea"); sg.transform.SetParent(wgo.transform);
            contentArea = sg.AddComponent<RectTransform>();
            contentArea.anchorMin = new Vector2(0,0.2f); contentArea.anchorMax = new Vector2(1,1);
            contentArea.offsetMin = new Vector2(10,10); contentArea.offsetMax = new Vector2(-10,-40);
            scrollRect = sg.AddComponent<ScrollRect>(); scrollRect.horizontal = false;
            var sback = sg.AddComponent<Image>(); sback.color = Color.clear;
            // viewport
            GameObject vp = new GameObject("Viewport"); vp.transform.SetParent(sg.transform);
            var vprt = vp.AddComponent<RectTransform>(); vprt.anchorMin=Vector2.zero; vprt.anchorMax=Vector2.one; vprt.offsetMin=vprt.offsetMax=Vector2.zero;
            var mask = vp.AddComponent<Mask>(); mask.showMaskGraphic=false;
            var vpimg = vp.AddComponent<Image>(); vpimg.color = Color.clear;
            scrollRect.viewport = vprt;
            // text content
            GameObject ct = new GameObject("Content"); ct.transform.SetParent(vp.transform);
            var ctrt = ct.AddComponent<RectTransform>(); ctrt.anchorMin=Vector2.zero; ctrt.anchorMax=Vector2.one; ctrt.offsetMin=ctrt.offsetMax=Vector2.zero;
            outputText = ct.AddComponent<TextMeshProUGUI>();
            outputText.color = Color.white;
            outputText.fontSize = 14;
            // Use legacy enableWordWrapping (obsolete) with warning suppression
            #pragma warning disable 0618
            outputText.enableWordWrapping = true;
            #pragma warning restore 0618
            outputText.alignment = TextAlignmentOptions.TopLeft;
            scrollRect.content = ctrt;

            // input field
            GameObject ig = new GameObject("InputField"); ig.transform.SetParent(wgo.transform);
            var irt = ig.AddComponent<RectTransform>(); irt.anchorMin=new Vector2(0,0); irt.anchorMax=new Vector2(1,0); irt.sizeDelta=new Vector2(0,30); irt.pivot=new Vector2(0,0);
            inputField = ig.AddComponent<TMP_InputField>(); var tf = ig.AddComponent<TextMeshProUGUI>(); tf.color=Color.white; tf.fontSize=14;
            inputField.textComponent = tf;
            var ph = ig.AddComponent<TextMeshProUGUI>(); ph.text="Enter command..."; ph.color=Color.gray; inputField.placeholder=ph;

            // resize handle
            GameObject rh = new GameObject("Resize"); rh.transform.SetParent(wgo.transform);
            resizeHandle = rh.AddComponent<RectTransform>();
            resizeHandle.anchorMin=resizeHandle.anchorMax=new Vector2(1,0);
            resizeHandle.sizeDelta=new Vector2(20,20); resizeHandle.anchoredPosition=new Vector2(-10,10);
            var rhimg = rh.AddComponent<Image>(); rhimg.color=Color.gray;
            var rtrig = rh.AddComponent<EventTrigger>();
            AddEvent(rtrig, EventTriggerType.BeginDrag, e => OnBeginResize((PointerEventData)e));
            AddEvent(rtrig, EventTriggerType.Drag, e => OnDragResize((PointerEventData)e));
        }

        void AddEvent(EventTrigger trg, EventTriggerType t, Action<BaseEventData> cb) { var e=new EventTrigger.Entry{eventID=t}; e.callback.AddListener(cb.Invoke); trg.triggers.Add(e); }

        void OnBeginDrag(PointerEventData e) { RectTransformUtility.ScreenPointToLocalPointInRectangle(window,e.position,null, out dragOffset); }
        void OnDrag(PointerEventData e) { Vector2 lp; RectTransformUtility.ScreenPointToLocalPointInRectangle(consoleCanvas.transform as RectTransform,e.position,null, out lp); window.anchoredPosition = lp - dragOffset; }
        void OnBeginResize(PointerEventData e){ resizeStart=window.sizeDelta; }
        void OnDragResize(PointerEventData e){ var s=resizeStart + new Vector2(e.delta.x,-e.delta.y); window.sizeDelta=new Vector2(Mathf.Max(200,s.x),Mathf.Max(100,s.y)); }

        void ToggleMinimize(){ isMinimized=!isMinimized; contentArea.gameObject.SetActive(!isMinimized); inputField.gameObject.SetActive(!isMinimized); }

        void AppendLine(string line){ outputText.text += ParseMarkdown(line)+"\n"; Canvas.ForceUpdateCanvases(); scrollRect.verticalNormalizedPosition=0; }
        string ParseMarkdown(string s)
        {
            // Bold: **text**
            s = Regex.Replace(s, @"\*\*(.*?)\*\*", "<b>$1</b>");
            // Italic: *text*
            s = Regex.Replace(s, @"\*(.*?)\*", "<i>$1</i>");
            return s;
        }        void RegisterCommands()
        {
            commands.Clear();
            
            // Basic console commands
            commands["help"] = args => {
                if (args.Length == 0)
                {
                    AppendLine("<color=cyan>=== Debug Console Help ===</color>");
                    AppendLine("<color=yellow>Basic Commands:</color>");
                    foreach(var k in commands.Keys.OrderBy(x => x)) 
                        AppendLine($"  {k}");
                    AppendLine("\n<color=yellow>Usage:</color> help [command] for detailed info");
                    AppendLine("<color=yellow>Navigation:</color> Up/Down arrows for history, Tab for autocomplete");
                }
                else
                {
                    ShowCommandHelp(args[0]);
                }
            };
            
            commands["echo"] = args => AppendLine(string.Join(" ", args));
            commands["clear"] = args => { outputText.text = string.Empty; logBuffer.Clear(); };
            
            // Scene management
            commands["scene_load"] = args => {
                if (args.Length == 0) { AppendLine("<color=red>Usage:</color> scene_load <scene_name>"); return; }
                try { SceneManager.LoadScene(args[0]); AppendLine($"<color=green>Loading scene:</color> {args[0]}"); }
                catch (Exception e) { AppendLine($"<color=red>Error loading scene:</color> {e.Message}"); }
            };
            
            commands["scene_list"] = args => {
                AppendLine("<color=cyan>=== Available Scenes ===</color>");
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    AppendLine($"  {i}: {sceneName}");
                }
            };
            
            commands["scene_reload"] = args => {
                var currentScene = SceneManager.GetActiveScene();
                AppendLine($"<color=green>Reloading scene:</color> {currentScene.name}");
                SceneManager.LoadScene(currentScene.name);
            };
            
            // Application control
            commands["quit"] = args => {
                AppendLine("<color=yellow>Quitting application...</color>");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            };
            
            commands["timescale"] = args => {
                if (args.Length == 0) { 
                    AppendLine($"<color=cyan>Current timescale:</color> {Time.timeScale}"); 
                    return; 
                }
                if (float.TryParse(args[0], out float scale)) {
                    Time.timeScale = Mathf.Max(0, scale);
                    AppendLine($"<color=green>Timescale set to:</color> {Time.timeScale}");
                } else {
                    AppendLine("<color=red>Usage:</color> timescale <float_value>");
                }
            };
            
            // System information
            commands["sysinfo"] = args => {
                AppendLine("<color=cyan>=== System Information ===</color>");
                AppendLine($"<color=yellow>Unity Version:</color> {Application.unityVersion}");
                AppendLine($"<color=yellow>Platform:</color> {Application.platform}");
                AppendLine($"<color=yellow>FPS:</color> {(1.0f / Time.unscaledDeltaTime):F1}");
                AppendLine($"<color=yellow>Frame Count:</color> {Time.frameCount}");
                AppendLine($"<color=yellow>Time Scale:</color> {Time.timeScale}");
                AppendLine($"<color=yellow>Memory Usage:</color> {(System.GC.GetTotalMemory(false) / 1024f / 1024f):F2} MB");
                AppendLine($"<color=yellow>Graphics Memory:</color> {(UnityEngine.Profiling.Profiler.usedHeapSizeLong / 1024f / 1024f):F2} MB");
            };
            
            // Player cheats
            commands["god_mode"] = args => {
                var player = FindFirstObjectByType<MonoBehaviour>(); // This would be your actual player class
                AppendLine("<color=yellow>God mode toggle</color> (implement with actual player reference)");
            };
            
            commands["noclip"] = args => {
                AppendLine("<color=yellow>Noclip toggle</color> (implement with actual player movement)");
            };
            
            commands["teleport"] = args => {
                if (args.Length < 3) { 
                    AppendLine("<color=red>Usage:</color> teleport <x> <y> <z>"); 
                    return; 
                }
                if (float.TryParse(args[0], out float x) && float.TryParse(args[1], out float y) && float.TryParse(args[2], out float z)) {
                    var player = FindFirstObjectByType<Camera>()?.transform; // Use camera as fallback
                    if (player != null) {
                        player.position = new Vector3(x, y, z);
                        AppendLine($"<color=green>Teleported to:</color> {x}, {y}, {z}");
                    } else {
                        AppendLine("<color=red>Error:</color> No player object found");
                    }
                } else {
                    AppendLine("<color=red>Error:</color> Invalid coordinates");
                }
            };
            
            // Log filtering
            commands["log_filter"] = args => {
                if (args.Length == 0) {
                    AppendLine($"<color=cyan>Current filter:</color> {currentLogFilter}");
                    AppendLine("<color=yellow>Available filters:</color> " + string.Join(", ", Enum.GetNames(typeof(LogCategory))));
                    return;
                }
                if (Enum.TryParse<LogCategory>(args[0], true, out var category)) {
                    currentLogFilter = category;
                    AppendLine($"<color=green>Log filter set to:</color> {category}");
                } else {
                    AppendLine("<color=red>Error:</color> Invalid category");
                }
            };
              // Memory management
            commands["gc_collect"] = args => {
                System.GC.Collect();
                AppendLine("<color=green>Garbage collection forced</color>");
            };            // Stats overlay commands
            commands["stats_toggle"] = args => {
                AppendLine("<color=green>Stats overlay toggle command received</color>");
                AppendLine("<color=yellow>Note:</color> Stats overlay controls available in development builds (F3 to toggle)");
            };
            
            commands["stats_fps"] = args => {
                if (args.Length == 0) { AppendLine("<color=red>Usage:</color> stats_fps <true/false>"); return; }
                AppendLine("<color=green>FPS display toggle command received</color>");
                AppendLine("<color=yellow>Note:</color> Stats overlay controls available in development builds");
            };
            
            // Visual debugger commands
            commands["debug_draw"] = args => {
                AppendLine("<color=green>Debug visualizer command received</color>");
                AppendLine("<color=yellow>Note:</color> Visual debugging available in development builds only (F4 to toggle)");
            };
            
            commands["draw_line"] = args => {
                if (args.Length < 6) { 
                    AppendLine("<color=red>Usage:</color> draw_line <x1> <y1> <z1> <x2> <y2> <z2> [duration]"); 
                    return; 
                }
                AppendLine("<color=yellow>Debug line drawing available in development builds only</color>");
            };
            
            commands["draw_sphere"] = args => {
                if (args.Length < 4) { 
                    AppendLine("<color=red>Usage:</color> draw_sphere <x> <y> <z> <radius> [duration]"); 
                    return; 
                }
                AppendLine("<color=yellow>Debug sphere drawing available in development builds only</color>");
            };
            
            commands["clear_debug"] = args => {
                AppendLine("<color=green>Debug visualization clear command received</color>");
            };
            
            // Add integration with AdvancedLogger
            RegisterAdvancedLoggerIntegration();
            
            // Population/Entity management commands
            commands["spawn_pop"] = args => {
                try {
                    var popManager = PopulationManager.Instance;
                    if (popManager == null) {
                        AppendLine("<color=red>Error:</color> PopulationManager not found");
                        return;
                    }
                    
                    Vector3 spawnPos = Vector3.zero;
                    if (args.Length >= 3) {
                        if (float.TryParse(args[0], out float x) && float.TryParse(args[1], out float y) && float.TryParse(args[2], out float z)) {
                            spawnPos = new Vector3(x, y, z);
                        } else {
                            AppendLine("<color=red>Error:</color> Invalid coordinates");
                            return;
                        }
                    } else {
                        // Use default spawn location
                        spawnPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                    }
                    
                    var newPop = popManager.SpawnPopAt(spawnPos);
                    if (newPop != null) {
                        AppendLine($"<color=green>Spawned pop:</color> {newPop.name} at {spawnPos}");
                    } else {
                        AppendLine("<color=red>Error:</color> Failed to spawn pop (population cap reached?)");
                    }
                } catch (Exception e) {
                    AppendLine($"<color=red>Error spawning pop:</color> {e.Message}");
                }
            };
            
            commands["kill_all_pops"] = args => {
                try {
                    var popManager = PopulationManager.Instance;
                    if (popManager == null) {
                        AppendLine("<color=red>Error:</color> PopulationManager not found");
                        return;
                    }
                    
                    var livingPops = popManager.GetLivingPops();
                    int count = livingPops.Count;
                    
                    // Kill all pops (iterate backwards to avoid collection modification issues)
                    for (int i = livingPops.Count - 1; i >= 0; i--) {
                        if (livingPops[i] != null) {
                            popManager.KillPop(livingPops[i]);
                        }
                    }
                    
                    AppendLine($"<color=yellow>Killed {count} pops</color>");
                } catch (Exception e) {
                    AppendLine($"<color=red>Error killing pops:</color> {e.Message}");
                }
            };
            
            commands["list_pops"] = args => {
                try {
                    var popManager = PopulationManager.Instance;
                    if (popManager == null) {
                        AppendLine("<color=red>Error:</color> PopulationManager not found");
                        return;
                    }
                    
                    var livingPops = popManager.GetLivingPops();
                    AppendLine($"<color=cyan>=== Living Pops ({livingPops.Count}/{popManager.populationCap}) ===</color>");
                    
                    for (int i = 0; i < livingPops.Count; i++) {
                        var pop = livingPops[i];
                        if (pop != null) {
                            var pos = pop.transform.position;
                            AppendLine($"  {i}: <color=yellow>{pop.name}</color> - Health: {pop.health:F1}, Position: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
                        }
                    }
                } catch (Exception e) {
                    AppendLine($"<color=red>Error listing pops:</color> {e.Message}");
                }
            };
            
            commands["set_pop_cap"] = args => {
                if (args.Length == 0) { 
                    AppendLine("<color=red>Usage:</color> set_pop_cap <number>"); 
                    return; 
                }
                
                if (int.TryParse(args[0], out int newCap) && newCap >= 0) {
                    var popManager = PopulationManager.Instance;
                    if (popManager != null) {
                        popManager.populationCap = newCap;
                        AppendLine($"<color=green>Population cap set to:</color> {newCap}");
                    } else {
                        AppendLine("<color=red>Error:</color> PopulationManager not found");
                    }
                } else {
                    AppendLine("<color=red>Error:</color> Invalid number");
                }
            };
            
            // Inventory system commands
            commands["give_item"] = args => {
                if (args.Length < 1) {
                    AppendLine("<color=red>Usage:</color> give_item <item_id> [amount] [target_pop_name]");
                    AppendLine("<color=yellow>Available items:</color> Check ItemSO assets in project");
                    return;
                }
                
                try {
                    string itemId = args[0];
                    int amount = args.Length > 1 && int.TryParse(args[1], out int qty) ? qty : 1;
                    string targetName = args.Length > 2 ? args[2] : null;
                    
                    // Find target pop
                    var targetPop = FindTargetPop(targetName);
                    if (targetPop == null) {
                        AppendLine("<color=red>Error:</color> No target pop found. Use list_pops to see available targets.");
                        return;
                    }
                    
                    var inventory = targetPop.GetComponent<Lineage.Ancestral.Legacies.Systems.Inventory.InventoryComponent>();
                    if (inventory == null) {
                        AppendLine("<color=red>Error:</color> Target pop has no inventory component");
                        return;
                    }
                    
                    if (inventory.AddItem(itemId, amount)) {
                        AppendLine($"<color=green>Added {amount}x {itemId} to {targetPop.name}'s inventory</color>");
                    } else {
                        AppendLine("<color=red>Error:</color> Failed to add item (inventory full?)");
                    }
                } catch (Exception e) {
                    AppendLine($"<color=red>Error giving item:</color> {e.Message}");
                }
            };
            
            commands["remove_item"] = args => {
                if (args.Length < 1) {
                    AppendLine("<color=red>Usage:</color> remove_item <item_id> [amount] [target_pop_name]");
                    return;
                }
                
                try {
                    string itemId = args[0];
                    int amount = args.Length > 1 && int.TryParse(args[1], out int qty) ? qty : 1;
                    string targetName = args.Length > 2 ? args[2] : null;
                    
                    var targetPop = FindTargetPop(targetName);
                    if (targetPop == null) {
                        AppendLine("<color=red>Error:</color> No target pop found");
                        return;
                    }
                    
                    var inventory = targetPop.GetComponent<Lineage.Ancestral.Legacies.Systems.Inventory.InventoryComponent>();
                    if (inventory == null) {
                        AppendLine("<color=red>Error:</color> Target pop has no inventory component");
                        return;
                    }
                    
                    if (inventory.RemoveItem(itemId, amount)) {
                        AppendLine($"<color=green>Removed {amount}x {itemId} from {targetPop.name}'s inventory</color>");
                    } else {
                        AppendLine("<color=red>Error:</color> Failed to remove item (not enough items?)");
                    }
                } catch (Exception e) {
                    AppendLine($"<color=red>Error removing item:</color> {e.Message}");
                }
            };
            
            commands["list_inventory"] = args => {
                try {
                    string targetName = args.Length > 0 ? args[0] : null;
                    var targetPop = FindTargetPop(targetName);
                    
                    if (targetPop == null) {
                        AppendLine("<color=red>Error:</color> No target pop found");
                        return;
                    }
                    
                    var inventory = targetPop.GetComponent<Lineage.Ancestral.Legacies.Systems.Inventory.InventoryComponent>();
                    if (inventory == null) {
                        AppendLine("<color=red>Error:</color> Target pop has no inventory component");
                        return;
                    }
                    
                    AppendLine($"<color=cyan>=== {targetPop.name}'s Inventory ===</color>");
                    var items = inventory.GetAllItems();
                    if (items.Count == 0) {
                        AppendLine("  <color=gray>Empty</color>");
                    } else {
                        foreach (var item in items) {
                            AppendLine($"  <color=yellow>{item.Key}</color>: {item.Value}");
                        }
                        AppendLine($"<color=gray>Capacity: {inventory.GetTotalItemCount()}/{inventory.capacity}</color>");
                    }
                } catch (Exception e) {
                    AppendLine($"<color=red>Error listing inventory:</color> {e.Message}");
                }
            };
            
            // Pop stat modification commands
            commands["set_health"] = args => {
                if (args.Length < 1) {
                    AppendLine("<color=red>Usage:</color> set_health <value> [target_pop_name]");
                    return;
                }
                
                if (float.TryParse(args[0], out float health)) {
                    string targetName = args.Length > 1 ? args[1] : null;
                    var targetPop = FindTargetPop(targetName);
                    
                    if (targetPop == null) {
                        AppendLine("<color=red>Error:</color> No target pop found");
                        return;
                    }
                    
                    targetPop.health = health;
                    AppendLine($"<color=green>Set {targetPop.name}'s health to:</color> {health}");
                } else {
                    AppendLine("<color=red>Error:</color> Invalid health value");
                }
            };
            
            commands["set_needs"] = args => {
                if (args.Length < 4) {
                    AppendLine("<color=red>Usage:</color> set_needs <hunger> <thirst> <energy> [target_pop_name]");
                    return;
                }
                
                if (float.TryParse(args[0], out float hunger) && 
                    float.TryParse(args[1], out float thirst) && 
                    float.TryParse(args[2], out float energy)) {
                    
                    string targetName = args.Length > 3 ? args[3] : null;
                    var targetPop = FindTargetPop(targetName);
                    
                    if (targetPop == null) {
                        AppendLine("<color=red>Error:</color> No target pop found");
                        return;
                    }
                    
                    var needs = targetPop.GetComponent<Lineage.Ancestral.Legacies.Systems.Needs.NeedsComponent>();
                    if (needs != null) {
                        needs.hunger = hunger;
                        needs.thirst = thirst;
                        needs.energy = energy;
                        AppendLine($"<color=green>Set {targetPop.name}'s needs:</color> H:{hunger} T:{thirst} E:{energy}");
                    } else {
                        AppendLine("<color=red>Error:</color> Target pop has no needs component");
                    }
                } else {
                    AppendLine("<color=red>Error:</color> Invalid need values");
                }
            };
            
            // AI and state control
            commands["toggle_ai"] = args => {
                try {
                    bool enable = true;
                    if (args.Length > 0) {
                        if (!bool.TryParse(args[0], out enable)) {
                            AppendLine("<color=red>Usage:</color> toggle_ai [true/false]");
                            return;
                        }
                    }
                    
                    var pops = FindObjectsByType<Lineage.Ancestral.Legacies.Entities.Pop>(FindObjectsSortMode.None);
                    int count = 0;
                    
                    foreach (var pop in pops) {
                        var stateMachine = pop.GetComponent<Lineage.Ancestral.Legacies.AI.PopStateMachine>();
                        if (stateMachine != null) {
                            stateMachine.enabled = enable;
                            count++;
                        }
                    }
                    
                    AppendLine($"<color=green>{(enable ? "Enabled" : "Disabled")} AI for {count} pops</color>");
                } catch (Exception e) {
                    AppendLine($"<color=red>Error toggling AI:</color> {e.Message}");
                }
            };
            
            // Resource management commands
            commands["add_resources"] = args => {
                if (args.Length < 2) {
                    AppendLine("<color=red>Usage:</color> add_resources <resource_type> <amount>");
                    AppendLine("<color=yellow>Resource types:</color> food, faith, wood");
                    return;
                }
                
                try {
                    string resourceType = args[0].ToLower();
                    if (float.TryParse(args[1], out float amount)) {
                        var resourceManager = ResourceManager.Instance;
                        if (resourceManager == null) {
                            AppendLine("<color=red>Error:</color> ResourceManager not found");
                            return;
                        }
                        
                        switch (resourceType) {
                            case "food":
                                resourceManager.AddFood(amount);
                                AppendLine($"<color=green>Added {amount} food</color>");
                                break;
                            case "faith":
                                resourceManager.AddFaith(amount);
                                AppendLine($"<color=green>Added {amount} faith</color>");
                                break;
                            case "wood":
                                resourceManager.AddWood(amount);
                                AppendLine($"<color=green>Added {amount} wood</color>");
                                break;
                            default:
                                AppendLine("<color=red>Error:</color> Unknown resource type");
                                break;
                        }
                    } else {
                        AppendLine("<color=red>Error:</color> Invalid amount");
                    }
                } catch (Exception e) {
                    AppendLine($"<color=red>Error adding resources:</color> {e.Message}");
                }
            };
            
            commands["show_resources"] = args => {
                try {
                    var resourceManager = ResourceManager.Instance;
                    if (resourceManager == null) {
                        AppendLine("<color=red>Error:</color> ResourceManager not found");
                        return;
                    }
                    
                    AppendLine("<color=cyan>=== Current Resources ===</color>");
                    AppendLine($"<color=yellow>Food:</color> {resourceManager.currentFood:F1}");
                    AppendLine($"<color=yellow>Faith:</color> {resourceManager.currentFaithPoints:F1}");
                    AppendLine($"<color=yellow>Wood:</color> {resourceManager.currentWood:F1}");
                    AppendLine($"<color=yellow>Efficient Gathering:</color> {resourceManager.hasEfficientGathering}");
                } catch (Exception e) {
                    AppendLine($"<color=red>Error showing resources:</color> {e.Message}");
                }
            };

            // ...existing code...
        }
          void RegisterAdvancedLoggerIntegration()
        {
            // Subscribe to AdvancedLogger messages if available in development builds
            try {
                // This will only work in development builds where AdvancedLogger is compiled
                AppendLine("<color=cyan>Debug console initialized</color>");
                AppendLine("<color=yellow>Advanced logging integration available in development builds</color>");
            } catch {
                // AdvancedLogger might not be available in release builds
                AppendLine("<color=gray>Running in release mode - limited debug features</color>");
            }
        }
        
        /// <summary>
        /// Method for AdvancedLogger to append log messages to the console
        /// </summary>
        public void AppendLogMessage(string message)
        {
            if (isOpen && !isMinimized)
            {
                AppendLine(message);
            }
        }
        
        void ShowCommandHelp(string command)
        {
            var helpText = command.ToLower() switch {
                "help" => "Usage: help [command] - Show help for all commands or specific command",
                "echo" => "Usage: echo <message> - Echo a message to console",
                "clear" => "Usage: clear - Clear the console output",
                "scene_load" => "Usage: scene_load <scene_name> - Load a specific scene",
                "scene_list" => "Usage: scene_list - List all available scenes",
                "scene_reload" => "Usage: scene_reload - Reload the current scene",
                "quit" => "Usage: quit - Exit the application",
                "timescale" => "Usage: timescale [value] - Get or set the time scale",
                "sysinfo" => "Usage: sysinfo - Display system information",
                "teleport" => "Usage: teleport <x> <y> <z> - Teleport player to coordinates",
                "log_filter" => "Usage: log_filter [category] - Get or set log category filter",
                "gc_collect" => "Usage: gc_collect - Force garbage collection",
                _ => $"No help available for '{command}'"
            };
            AppendLine($"<color=green>{helpText}</color>");
        }void ProcessCommand(string line)
        {
            var parts=line.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            var cmd=parts[0].ToLower();
            var args=parts.Length>1? parts[1..] : new string[0];
            if(commands.ContainsKey(cmd)) commands[cmd].Invoke(args);
            else AppendLine($"<color=red>Error:</color> Unknown command '{cmd}'");
        }
        
        void NavigateHistory(int direction)
        {
            if (commandHistory.Count == 0) return;
            
            historyIndex += direction;
            historyIndex = Mathf.Clamp(historyIndex, -1, commandHistory.Count - 1);
            
            if (historyIndex == -1)
            {
                inputField.text = "";
            }
            else
            {
                inputField.text = commandHistory[commandHistory.Count - 1 - historyIndex];
            }
            
            // Move cursor to end
            inputField.stringPosition = inputField.text.Length;
        }
        
        void AddToHistory(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;
            
            // Remove duplicate if it exists
            commandHistory.Remove(command);
            // Add to end
            commandHistory.Add(command);
            
            // Limit history size
            if (commandHistory.Count > 50)
            {
                commandHistory.RemoveAt(0);
            }
        }
        
        void AutoComplete()
        {
            string input = inputField.text.ToLower();
            if (string.IsNullOrEmpty(input)) return;
            
            var matches = commands.Keys.Where(cmd => cmd.StartsWith(input)).ToList();
            
            if (matches.Count == 1)
            {
                inputField.text = matches[0];
                inputField.stringPosition = inputField.text.Length;
            }
            else if (matches.Count > 1)
            {
                AppendLine($"<color=yellow>Suggestions:</color> {string.Join(", ", matches)}");
            }
        }
          // Helper method to find a target pop by name
        private Pop FindTargetPop(string targetName)
        {
            var popManager = PopulationManager.Instance;
            if (popManager == null) return null;
            
            var livingPops = popManager.GetLivingPops();
            if (livingPops == null || livingPops.Count == 0) return null;
            
            // If no specific name provided, return the first pop
            if (string.IsNullOrEmpty(targetName))
            {
                return livingPops.FirstOrDefault();
            }
            
            // Find pop by name (case insensitive)
            return livingPops.FirstOrDefault(p => 
                string.Equals(p.name, targetName, StringComparison.OrdinalIgnoreCase));
        }
        
        // Enhanced auto-completion and suggestion methods
        private void UpdateSuggestions()
        {
            if (string.IsNullOrEmpty(inputField.text))
            {
                HideSuggestions();
                return;
            }
            
            string input = inputField.text.ToLower();
            suggestions.Clear();
            
            foreach (var command in commands.Keys)
            {
                if (command.StartsWith(input))
                {
                    suggestions.Add(command);
                }
            }
            
            if (suggestions.Count > 0)
            {
                ShowSuggestions();
            }
            else
            {
                HideSuggestions();
            }
        }
        
        private void ShowSuggestions()
        {
            if (suggestionPanel == null)
            {
                CreateSuggestionPanel();
            }
            
            suggestionPanel.SetActive(true);
            showingSuggestions = true;
            
            string suggestionList = string.Join("\n", suggestions.Take(5));
            suggestionText.text = suggestionList;
        }
        
        private void HideSuggestions()
        {
            if (suggestionPanel != null)
            {
                suggestionPanel.SetActive(false);
            }
            showingSuggestions = false;
        }
        
        private void CreateSuggestionPanel()
        {
            suggestionPanel = new GameObject("SuggestionPanel");
            suggestionPanel.transform.SetParent(window.transform, false);
            
            RectTransform suggestionRect = suggestionPanel.AddComponent<RectTransform>();
            suggestionRect.anchorMin = new Vector2(0, 0);
            suggestionRect.anchorMax = new Vector2(1, 0.2f);
            suggestionRect.offsetMin = new Vector2(10, 10);
            suggestionRect.offsetMax = new Vector2(-10, 50);
            
            Image suggestionBg = suggestionPanel.AddComponent<Image>();
            suggestionBg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            
            GameObject textObj = new GameObject("SuggestionText");
            textObj.transform.SetParent(suggestionPanel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);
            
            suggestionText = textObj.AddComponent<TextMeshProUGUI>();
            suggestionText.fontSize = 12;
            suggestionText.color = Color.yellow;
            suggestionText.alignment = TextAlignmentOptions.TopLeft;
            
            suggestionPanel.SetActive(false);
        }
        
        // Enhanced registration method with descriptions
        public void RegisterCommand(string command, string description, Action<string[]> handler)
        {
            commands[command] = handler;
            commandDescriptions[command] = description;
        }
        
        // Log filtering methods
        public void AppendLogMessage(string message, string category = "General")
        {
            LogCategory msgCategory = LogCategory.General;
            Enum.TryParse(category, true, out msgCategory);
            
            if (currentLogFilter != LogCategory.All && currentLogFilter != msgCategory)
            {
                return; // Filter out this message
            }
            
            logBuffer.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            
            // Maintain buffer size
            if (logBuffer.Count > maxLogLines)
            {
                logBuffer.RemoveAt(0);
            }
            
            // Update display
            RefreshLogDisplay();
        }
        
        private void RefreshLogDisplay()
        {
            if (outputText != null)
            {
                outputText.text = string.Join("\n", logBuffer);
                
                // Auto-scroll to bottom
                Canvas.ForceUpdateCanvases();
                if (scrollRect != null)
                {
                    scrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }
    }
}
#endif

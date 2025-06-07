using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Lineage.Debug
{
    /// <summary>
    /// Central debug manager that coordinates all debug systems and provides unified access
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
        private static DebugManager instance;
        public static DebugManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<DebugManager>() ?? CreateInstance();
                return instance;
            }
        }
        
        [Header("Debug System Settings")]
        [SerializeField] private bool enableDebugSystems = true;
        [SerializeField] private bool autoInitialize = true;
          [Header("Component References")]
        [SerializeField] public DebugConsoleManager consoleManager;
        [SerializeField] public DebugStatsOverlay statsOverlay;
        [SerializeField] public DebugVisualizer visualizer;
        
        // Input System
        private InputAction helpAction;
        private InputAction toggleAllAction;
        
        // Debug system state
        private bool systemsInitialized = false;
        
        static DebugManager CreateInstance()
        {
            var go = new GameObject("DebugManager");
            DontDestroyOnLoad(go);
            return go.AddComponent<DebugManager>();
        }
        
        [RuntimeInitializeOnLoadMethod]
        static void InitDebugManager()
        {
            CreateInstance();
        }
          void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (autoInitialize)
                {
                    InitializeDebugSystems();
                }
                
                SetupInputActions();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        void SetupInputActions()
        {
            // F1 for help
            helpAction = new InputAction("ShowDebugHelp", InputActionType.Button);
            helpAction.AddBinding("<Keyboard>/f1");
            helpAction.performed += OnHelpPerformed;
            helpAction.Enable();
            
            // F12 for toggle all
            toggleAllAction = new InputAction("ToggleAllDebugSystems", InputActionType.Button);
            toggleAllAction.AddBinding("<Keyboard>/f12");
            toggleAllAction.performed += OnToggleAllPerformed;
            toggleAllAction.Enable();
        }
        
        void OnHelpPerformed(InputAction.CallbackContext context)
        {
            ShowDebugHelp();
        }
        
        void OnToggleAllPerformed(InputAction.CallbackContext context)
        {
            ToggleAllDebugSystems();
        }
        
        void Start()
        {
            if (enableDebugSystems && !systemsInitialized)
            {
                InitializeDebugSystems();
            }
            
            // Log startup information
            AdvancedLogger.LogInfo(LogCategory.General, "Debug Manager initialized");
            AdvancedLogger.LogInfo(LogCategory.General, $"Unity Version: {Application.unityVersion}");
            AdvancedLogger.LogInfo(LogCategory.General, $"Platform: {Application.platform}");
            AdvancedLogger.LogInfo(LogCategory.General, $"Development Build: {UnityEngine.Debug.isDebugBuild}");
        }
          void Update()
        {
            // Input is now handled by InputActions
        }
        
        void InitializeDebugSystems()
        {
            if (systemsInitialized) return;
            
            AdvancedLogger.LogInfo(LogCategory.General, "Initializing debug systems...");
            
            // Ensure all debug components exist
            if (consoleManager == null)
                consoleManager = FindFirstObjectByType<DebugConsoleManager>();
            
            if (statsOverlay == null)
                statsOverlay = FindFirstObjectByType<DebugStatsOverlay>();
            
            if (visualizer == null)
                visualizer = DebugVisualizer.Instance;
            
            systemsInitialized = true;
            AdvancedLogger.LogInfo(LogCategory.General, "Debug systems initialized successfully");
        }
        
        void ShowDebugHelp()
        {
            AdvancedLogger.LogInfo(LogCategory.UI, "=== Debug System Help ===");
            AdvancedLogger.LogInfo(LogCategory.UI, "F1 - Show this help");
            AdvancedLogger.LogInfo(LogCategory.UI, "F2 - Toggle Debug Console");
            AdvancedLogger.LogInfo(LogCategory.UI, "F3 - Toggle Stats Overlay");
            AdvancedLogger.LogInfo(LogCategory.UI, "F4 - Toggle Visual Debugger");
            AdvancedLogger.LogInfo(LogCategory.UI, "F12 - Toggle All Debug Systems");
            AdvancedLogger.LogInfo(LogCategory.UI, "Type 'help' in console for commands");
        }
        
        void ToggleAllDebugSystems()
        {
            enableDebugSystems = !enableDebugSystems;
            
            if (statsOverlay != null)
                statsOverlay.SetVisibility(enableDebugSystems);
            
            AdvancedLogger.LogInfo(LogCategory.UI, $"Debug systems {(enableDebugSystems ? "enabled" : "disabled")}");
        }
        
        // Public API for controlling debug systems
        public void EnableDebugSystems(bool enable)
        {
            enableDebugSystems = enable;
            ToggleAllDebugSystems();
        }
        
        public void LogGameEvent(string eventName, params object[] parameters)
        {
            var paramString = parameters.Length > 0 ? $" - {string.Join(", ", parameters)}" : "";
            AdvancedLogger.LogInfo(LogCategory.General, $"Game Event: {eventName}{paramString}");
        }
        
        public void LogPerformanceMetric(string metricName, float value, string unit = "")
        {
            AdvancedLogger.LogInfo(LogCategory.Performance, $"Performance: {metricName} = {value:F2} {unit}");
        }
        
        public void LogAIEvent(string agentName, string action, Vector3 position)
        {
            AdvancedLogger.LogDebug(LogCategory.AI, $"AI Event: {agentName} - {action} at {position}");
        }
        
        public void LogUIEvent(string uiElement, string action)
        {
            AdvancedLogger.LogDebug(LogCategory.UI, $"UI Event: {uiElement} - {action}");
        }
        
        public void LogInventoryEvent(string action, string itemName, int quantity = 1)
        {
            AdvancedLogger.LogDebug(LogCategory.Inventory, $"Inventory: {action} - {itemName} x{quantity}");
        }
        
        public void LogCombatEvent(string attacker, string target, float damage, Vector3 position)
        {
            AdvancedLogger.LogDebug(LogCategory.Combat, $"Combat: {attacker} -> {target} ({damage} dmg) at {position}");
        }
        
        // Visual debugging helpers
        public void DrawAgentVision(Transform agent, float range, float angle, Color color, float duration = 1f)
        {
            if (visualizer != null && enableDebugSystems)
            {
                DebugVisualizer.DrawVisionCone(agent.position, agent.forward, angle, range, color, duration);
            }
        }
        
        public void DrawBoundingBox(Bounds bounds, Color color, float duration = 1f)
        {
            if (visualizer != null && enableDebugSystems)
            {
                DebugVisualizer.DrawBounds(bounds, color, duration);
            }
        }
        
        public void DrawPath(Vector3[] points, Color color, float duration = 1f)
        {
            if (visualizer != null && enableDebugSystems && points.Length > 1)
            {
                for (int i = 0; i < points.Length - 1; i++)
                {
                    DebugVisualizer.DrawLine(points[i], points[i + 1], color, duration);
                }
            }
        }
        
        public void DrawWorldText(Vector3 position, string text, Color color, float duration = 1f)
        {
            if (visualizer != null && enableDebugSystems)
            {
                DebugVisualizer.DrawText(position, text, color, duration);
            }
        }
        
        // Performance monitoring
        public void StartPerformanceTimer(string timerName)
        {
            // Implementation for performance timing
            AdvancedLogger.LogDebug(LogCategory.Performance, $"Performance timer started: {timerName}");
        }
        
        public void EndPerformanceTimer(string timerName)
        {
            // Implementation for performance timing
            AdvancedLogger.LogDebug(LogCategory.Performance, $"Performance timer ended: {timerName}");
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            AdvancedLogger.LogInfo(LogCategory.General, $"Application {(pauseStatus ? "paused" : "resumed")}");
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            AdvancedLogger.LogInfo(LogCategory.General, $"Application {(hasFocus ? "gained" : "lost")} focus");
        }
        
        void OnApplicationQuit()
        {
            AdvancedLogger.LogInfo(LogCategory.General, "Application shutting down");
        }
        
        void OnDestroy()
        {
            // Clean up input actions
            if (helpAction != null)
            {
                helpAction.performed -= OnHelpPerformed;
                helpAction.Disable();
                helpAction.Dispose();
            }
            
            if (toggleAllAction != null)
            {
                toggleAllAction.performed -= OnToggleAllPerformed;
                toggleAllAction.Disable();
                toggleAllAction.Dispose();
            }
        }
    }
}
#endif

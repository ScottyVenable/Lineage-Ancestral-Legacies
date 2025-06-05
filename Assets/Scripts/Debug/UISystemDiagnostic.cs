using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Lineage.Ancestral.Legacies.Debug
{
    /// <summary>
    /// UI System Diagnostic Tool - Diagnoses common Unity UI issues that prevent button interactions
    /// This tool helps identify problems with EventSystem, Canvas, GraphicRaycaster, and Input System setup
    /// </summary>
    public class UISystemDiagnostic : MonoBehaviour
    {
        [Header("Diagnostic Settings")]
        [SerializeField] private bool runDiagnosticOnStart = true;
        [SerializeField] private bool showDetailedLogging = true;
        [SerializeField] private KeyCode diagnosticHotkey = KeyCode.F10;
        
        [Header("UI References (Optional - Auto-detected if null)")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private EventSystem eventSystem;
        
        [Header("Runtime Info (Read-Only)")]
        [SerializeField, Tooltip("Current EventSystem status")] private string eventSystemStatus;
        [SerializeField, Tooltip("Canvas setup status")] private string canvasStatus;
        [SerializeField, Tooltip("Input System status")] private string inputSystemStatus;
        [SerializeField, Tooltip("Overall UI health")] private string overallStatus;
        
        private Canvas[] allCanvases;
        private GraphicRaycaster[] allRaycasters;
        private Button[] allButtons;
        private List<string> diagnosticResults = new List<string>();
        
        void Start()
        {
            if (runDiagnosticOnStart)
            {
                RunFullDiagnostic();
            }
        }
        
        void Update()
        {
            if (Input.GetKeyDown(diagnosticHotkey))
            {
                RunFullDiagnostic();
            }
        }
        
        /// <summary>
        /// Runs a comprehensive diagnostic of the Unity UI system
        /// </summary>
        [ContextMenu("Run Full UI Diagnostic")]
        public void RunFullDiagnostic()
        {
            diagnosticResults.Clear();
            LogInfo("=== UI SYSTEM DIAGNOSTIC START ===");
            
            // 1. Check EventSystem
            DiagnoseEventSystem();
            
            // 2. Check Canvas setup
            DiagnoseCanvasSetup();
            
            // 3. Check Input System
            DiagnoseInputSystem();
            
            // 4. Check Button configurations
            DiagnoseButtons();
            
            // 5. Check for blocking elements
            DiagnoseBlockingElements();
            
            // 6. Generate recommendations
            GenerateRecommendations();
            
            UpdateInspectorStatus();
            LogInfo("=== UI DIAGNOSTIC COMPLETE ===");
        }
        
        private void DiagnoseEventSystem()
        {
            LogInfo("--- EVENTSYSTEM DIAGNOSTIC ---");
            
            // Find EventSystem
            eventSystem = FindFirstObjectByType<EventSystem>();
            
            if (eventSystem == null)
            {
                LogError("❌ NO EVENTSYSTEM FOUND! This is likely the main issue.");
                LogError("   → Solution: Create GameObject → UI → Event System");
                eventSystemStatus = "MISSING - CRITICAL ERROR";
                diagnosticResults.Add("CRITICAL: EventSystem missing");
                return;
            }
            
            LogSuccess($"✅ EventSystem found: {eventSystem.name}");
            
            // Check if EventSystem is enabled
            if (!eventSystem.enabled)
            {
                LogError("❌ EventSystem is DISABLED!");
                LogError("   → Solution: Enable the EventSystem component");
                eventSystemStatus = "DISABLED - CRITICAL ERROR";
                diagnosticResults.Add("CRITICAL: EventSystem disabled");
                return;
            }
            
            // Check Input Module
            var inputModule = eventSystem.currentInputModule;
            if (inputModule == null)
            {
                LogError("❌ EventSystem has NO INPUT MODULE!");
                LogError("   → Solution: Add StandaloneInputModule or InputSystemUIInputModule");
                eventSystemStatus = "NO INPUT MODULE - CRITICAL ERROR";
                diagnosticResults.Add("CRITICAL: No input module on EventSystem");
                return;
            }
            
            LogSuccess($"✅ Input Module: {inputModule.GetType().Name}");
            
            // Check if Input Module is enabled
            if (!inputModule.enabled)
            {
                LogError("❌ Input Module is DISABLED!");
                LogError("   → Solution: Enable the Input Module component");
                eventSystemStatus = "INPUT MODULE DISABLED - CRITICAL ERROR";
                diagnosticResults.Add("CRITICAL: Input module disabled");
                return;
            }
            
            eventSystemStatus = "OK";
            LogSuccess("✅ EventSystem diagnostic passed");
        }
        
        private void DiagnoseCanvasSetup()
        {
            LogInfo("--- CANVAS DIAGNOSTIC ---");
            
            allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            
            if (allCanvases.Length == 0)
            {
                LogError("❌ NO CANVAS FOUND!");
                LogError("   → Solution: Create GameObject → UI → Canvas");
                canvasStatus = "NO CANVAS - CRITICAL ERROR";
                diagnosticResults.Add("CRITICAL: No Canvas found");
                return;
            }
            
            LogInfo($"Found {allCanvases.Length} Canvas(es)");
            
            bool hasWorkingCanvas = false;
            
            for (int i = 0; i < allCanvases.Length; i++)
            {
                var canvas = allCanvases[i];
                LogInfo($"  Canvas {i + 1}: {canvas.name}");
                
                // Check if Canvas is enabled
                if (!canvas.enabled || !canvas.gameObject.activeInHierarchy)
                {
                    LogWarning($"    ⚠️ Canvas '{canvas.name}' is disabled or inactive");
                    continue;
                }
                
                // Check GraphicRaycaster
                var raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    LogError($"    ❌ Canvas '{canvas.name}' has NO GraphicRaycaster!");
                    LogError("       → Solution: Add GraphicRaycaster component to Canvas");
                    continue;
                }
                
                if (!raycaster.enabled)
                {
                    LogError($"    ❌ GraphicRaycaster on '{canvas.name}' is DISABLED!");
                    LogError("       → Solution: Enable GraphicRaycaster component");
                    continue;
                }
                
                LogSuccess($"    ✅ Canvas '{canvas.name}' has working GraphicRaycaster");
                hasWorkingCanvas = true;
                
                if (mainCanvas == null) mainCanvas = canvas;
            }
            
            if (hasWorkingCanvas)
            {
                canvasStatus = "OK";
                LogSuccess("✅ Canvas diagnostic passed");
            }
            else
            {
                canvasStatus = "NO WORKING CANVAS - CRITICAL ERROR";
                diagnosticResults.Add("CRITICAL: No working Canvas with GraphicRaycaster");
            }
        }
        
        private void DiagnoseInputSystem()
        {
            LogInfo("--- INPUT SYSTEM DIAGNOSTIC ---");
            
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            LogInfo("New Input System is enabled (Legacy disabled)");
            
            if (eventSystem != null)
            {
                var inputModule = eventSystem.currentInputModule;
                if (inputModule != null && inputModule.GetType().Name.Contains("InputSystemUIInputModule"))
                {
                    LogSuccess("✅ Using InputSystemUIInputModule (correct for new Input System)");
                    inputSystemStatus = "NEW INPUT SYSTEM - OK";
                }
                else
                {
                    LogError("❌ Using wrong input module for new Input System!");
                    LogError("   → Solution: Replace StandaloneInputModule with InputSystemUIInputModule");
                    inputSystemStatus = "WRONG INPUT MODULE";
                    diagnosticResults.Add("ERROR: Wrong input module for new Input System");
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER && !ENABLE_INPUT_SYSTEM
            LogInfo("Legacy Input Manager is enabled (New Input System disabled)");
            
            if (eventSystem != null)
            {
                var inputModule = eventSystem.currentInputModule;
                if (inputModule != null && inputModule.GetType().Name.Contains("StandaloneInputModule"))
                {
                    LogSuccess("✅ Using StandaloneInputModule (correct for Legacy Input)");
                    inputSystemStatus = "LEGACY INPUT - OK";
                }
                else
                {
                    LogError("❌ Using wrong input module for Legacy Input Manager!");
                    LogError("   → Solution: Replace with StandaloneInputModule");
                    inputSystemStatus = "WRONG INPUT MODULE";
                    diagnosticResults.Add("ERROR: Wrong input module for Legacy Input");
                }
            }
#elif ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            LogWarning("⚠️ Both Input Systems are enabled (this can cause conflicts)");
            LogWarning("   → Recommendation: Disable one of them in Project Settings");
            inputSystemStatus = "BOTH ENABLED - WARNING";
#else
            LogError("❌ NO INPUT SYSTEM ENABLED!");
            LogError("   → Solution: Enable either New Input System or Legacy Input Manager in Project Settings");
            inputSystemStatus = "NO INPUT SYSTEM - CRITICAL ERROR";
            diagnosticResults.Add("CRITICAL: No input system enabled");
#endif
        }
        
        private void DiagnoseButtons()
        {
            LogInfo("--- BUTTON DIAGNOSTIC ---");
            
            allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            
            if (allButtons.Length == 0)
            {
                LogWarning("⚠️ No Unity Button components found");
                LogInfo("   Note: This is expected if using only EnhancedAutoButtonHandler");
                return;
            }
            
            LogInfo($"Found {allButtons.Length} Button(s)");
            
            int workingButtons = 0;
            
            foreach (var button in allButtons)
            {
                if (!button.enabled || !button.gameObject.activeInHierarchy)
                {
                    LogWarning($"  ⚠️ Button '{button.name}' is disabled or inactive");
                    continue;
                }
                
                if (!button.interactable)
                {
                    LogWarning($"  ⚠️ Button '{button.name}' is not interactable");
                    continue;
                }
                
                // Check if button is under a Canvas with GraphicRaycaster
                var canvas = button.GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    LogError($"  ❌ Button '{button.name}' is not under any Canvas!");
                    continue;
                }
                
                var raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null || !raycaster.enabled)
                {
                    LogError($"  ❌ Button '{button.name}' Canvas lacks working GraphicRaycaster!");
                    continue;
                }
                
                workingButtons++;
                LogSuccess($"  ✅ Button '{button.name}' appears properly configured");
            }
            
            LogInfo($"Properly configured buttons: {workingButtons}/{allButtons.Length}");
        }
        
        private void DiagnoseBlockingElements()
        {
            LogInfo("--- BLOCKING ELEMENTS DIAGNOSTIC ---");
            
            // Check for CanvasGroups that might block raycasts
            var canvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
            
            foreach (var cg in canvasGroups)
            {
                if (!cg.blocksRaycasts && cg.gameObject.activeInHierarchy)
                {
                    LogWarning($"  ⚠️ CanvasGroup '{cg.name}' has blocksRaycasts = false");
                    LogWarning("     This might prevent UI interactions if placed over buttons");
                }
                
                if (cg.alpha <= 0 && cg.gameObject.activeInHierarchy)
                {
                    LogWarning($"  ⚠️ CanvasGroup '{cg.name}' has alpha = 0");
                    LogWarning("     Invisible UI elements might still block interactions");
                }
            }
            
            // Check for overlapping UI elements
            if (allCanvases != null && allCanvases.Length > 1)
            {
                LogWarning($"  ⚠️ Multiple Canvases found ({allCanvases.Length})");
                LogWarning("     Check sorting orders to ensure proper layering");
            }
        }
        
        private void GenerateRecommendations()
        {
            LogInfo("--- RECOMMENDATIONS ---");
            
            if (diagnosticResults.Count == 0)
            {
                LogSuccess("🎉 NO ISSUES FOUND! Your UI system appears to be properly configured.");
                overallStatus = "HEALTHY";
                return;
            }
            
            LogError($"❌ Found {diagnosticResults.Count} issue(s) that need attention:");
            
            bool hasCriticalIssues = false;
            
            foreach (string issue in diagnosticResults)
            {
                LogError($"  • {issue}");
                if (issue.Contains("CRITICAL"))
                {
                    hasCriticalIssues = true;
                }
            }
            
            if (hasCriticalIssues)
            {
                overallStatus = "CRITICAL ISSUES FOUND";
                LogError("🚨 CRITICAL ISSUES DETECTED!");
                LogError("   These issues will completely prevent UI interactions.");
                LogError("   Fix these first before testing buttons again.");
            }
            else
            {
                overallStatus = "MINOR ISSUES FOUND";
                LogWarning("⚠️ Minor issues detected that might affect UI performance.");
            }
        }
        
        private void UpdateInspectorStatus()
        {
            // Update inspector fields for easy viewing
            if (eventSystemStatus == null) eventSystemStatus = "Not checked";
            if (canvasStatus == null) canvasStatus = "Not checked";
            if (inputSystemStatus == null) inputSystemStatus = "Not checked";
            if (overallStatus == null) overallStatus = "Not checked";
        }
        
        private void LogInfo(string message)
        {
            if (showDetailedLogging)
                Log.Debug($"[UI Diagnostic] {message}", Log.LogCategory.UI);
        }
        
        private void LogSuccess(string message)
        {
            if (showDetailedLogging)
                Log.Debug($"[UI Diagnostic] {message}", Log.LogCategory.UI);
        }

        private void LogWarning(string message)
        {
            Log.Warning($"[UI Diagnostic] {message}", Log.LogCategory.UI);
        }
        
        private void LogError(string message)
        {
            Log.Error($"[UI Diagnostic] {message}", Log.LogCategory.UI);
        }
        private void LogCritical(string message)
        {
            Log.Critical($"[UI Diagnostic] {message}", Log.LogCategory.UI);
        }
        [ContextMenu("Quick Fix - Create EventSystem")]
        public void QuickFixEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                LogWarning("EventSystem already exists!");
                return;
            }
            
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // Add InputSystemUIInputModule for new Input System
            var inputModule = eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            LogSuccess("Created EventSystem with InputSystemUIInputModule");
#else
            // Add StandaloneInputModule for legacy input
            var inputModule = eventSystemGO.AddComponent<StandaloneInputModule>();
            LogSuccess("Created EventSystem with StandaloneInputModule");
#endif
            
            LogSuccess("EventSystem created! Run diagnostic again to verify.");
        }
        
        [ContextMenu("Quick Fix - Add GraphicRaycaster to Main Canvas")]
        public void QuickFixGraphicRaycaster()
        {
            if (mainCanvas == null)
                mainCanvas = FindFirstObjectByType<Canvas>();
                
            if (mainCanvas == null)
            {
                LogError("No Canvas found! Create a Canvas first.");
                return;
            }
            
            var raycaster = mainCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                mainCanvas.gameObject.AddComponent<GraphicRaycaster>();
                LogSuccess($"Added GraphicRaycaster to Canvas '{mainCanvas.name}'");
            }
            else
            {
                LogWarning("GraphicRaycaster already exists on this Canvas!");
            }
        }
    }
}

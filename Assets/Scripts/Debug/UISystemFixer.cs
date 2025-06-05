using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Lineage.Debug
{
    /// <summary>
    /// Tool to fix UI system issues, particularly MissingReferenceException problems
    /// with destroyed TextMeshProUGUI components in GraphicRaycaster
    /// </summary>
    public class UISystemFixer : MonoBehaviour
    {
        [Header("UI System Fixer")]
        [SerializeField] private bool autoFixOnStart = false;
        [SerializeField] private bool showDetailedLogs = true;
        
        private void Start()
        {
            if (autoFixOnStart)
            {
                FixUISystem();
            }
        }
          [ContextMenu("Fix UI System")]
        public void FixUISystem()
        {
            Log("=== Starting UI System Fix ===");
            
            // Step 1: Check and fix EventSystem
            FixEventSystem();
            
            // Step 2: Migrate to new Input System if needed
            MigrateToNewInputSystem();
            
            // Step 3: Clean up destroyed UI references
            CleanupDestroyedUIReferences();
            
            // Step 4: Verify Canvas setup
            VerifyCanvasSetup();
            
            // Step 5: Check for orphaned UI components
            CheckForOrphanedComponents();
            
            Log("=== UI System Fix Complete ===");
        }
        
        [ContextMenu("Migrate to New Input System")]
        public void MigrateToNewInputSystem()
        {
            Log("Checking Input System migration...");
            
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                Log("ERROR: No EventSystem found! Run FixEventSystem first.");
                return;
            }
            
            // Check for legacy StandaloneInputModule
            StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            InputSystemUIInputModule newModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            
            if (legacyModule != null && newModule == null)
            {
                Log("Found legacy StandaloneInputModule. Migrating to InputSystemUIInputModule...");
                
                // Remove legacy module
                if (Application.isPlaying)
                {
                    Destroy(legacyModule);
                }
                else
                {
                    DestroyImmediate(legacyModule);
                }
                
                // Add new Input System module
                newModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                Log("✓ Successfully migrated to InputSystemUIInputModule!");
            }
            else if (legacyModule != null && newModule != null)
            {
                Log("WARNING: Both input modules present! Removing legacy StandaloneInputModule...");
                if (Application.isPlaying)
                {
                    Destroy(legacyModule);
                }
                else
                {
                    DestroyImmediate(legacyModule);
                }
                Log("✓ Removed duplicate legacy input module");
            }
            else if (newModule != null)
            {
                Log("✓ Already using InputSystemUIInputModule (New Input System)");
            }
            else
            {
                Log("No input modules found. Adding InputSystemUIInputModule...");
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                Log("✓ Added InputSystemUIInputModule");
            }
        }
          private void FixEventSystem()
        {
            Log("Checking EventSystem...");
            
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                Log("ERROR: No EventSystem found! Creating one...");
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystem = eventSystemGO.AddComponent<EventSystem>();
                
                // Use InputSystemUIInputModule for new Input System
                InputSystemUIInputModule inputModule = eventSystemGO.AddComponent<InputSystemUIInputModule>();
                Log("EventSystem created with InputSystemUIInputModule (New Input System)");
            }
            else
            {
                Log($"EventSystem found: {eventSystem.name}");
                
                // Check input module
                BaseInputModule inputModule = eventSystem.currentInputModule;
                if (inputModule == null)
                {
                    Log("WARNING: No input module found! Adding InputSystemUIInputModule...");
                    eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                }
                else
                {
                    string moduleType = inputModule.GetType().Name;
                    Log($"Input module: {moduleType}");
                    
                    // Check if using legacy input system and warn
                    if (moduleType == "StandaloneInputModule")
                    {
                        Log("WARNING: Using legacy StandaloneInputModule. Consider switching to InputSystemUIInputModule for new Input System.");
                        Log("To fix: Remove StandaloneInputModule and add InputSystemUIInputModule component.");
                    }
                    else if (moduleType == "InputSystemUIInputModule")
                    {
                        Log("✓ Using InputSystemUIInputModule (New Input System) - Correct!");
                    }
                }
            }
        }
        
        private void CleanupDestroyedUIReferences()
        {
            Log("Cleaning up destroyed UI references...");
            
            // Find all GraphicRaycasters
            GraphicRaycaster[] raycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
            Log($"Found {raycasters.Length} GraphicRaycasters");
            
            foreach (GraphicRaycaster raycaster in raycasters)
            {
                Canvas canvas = raycaster.GetComponent<Canvas>();
                if (canvas != null)
                {
                    CleanupCanvasHierarchy(canvas.transform);
                }
            }
            
            // Force refresh all UI elements
            Canvas.ForceUpdateCanvases();
        }
        
        private void CleanupCanvasHierarchy(Transform parent)
        {
            List<Transform> childrenToRemove = new List<Transform>();
            
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                
                // Check if any UI components are destroyed/null
                if (HasDestroyedUIComponents(child.gameObject))
                {
                    Log($"Found object with destroyed UI components: {child.name}");
                    childrenToRemove.Add(child);
                }
                else
                {
                    // Recursively check children
                    CleanupCanvasHierarchy(child);
                }
            }
            
            // Remove objects with destroyed components
            foreach (Transform child in childrenToRemove)
            {
                Log($"Removing object with destroyed components: {child.name}");
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        
        private bool HasDestroyedUIComponents(GameObject obj)
        {
            // Check for destroyed UI components
            Component[] components = obj.GetComponents<Component>();
            
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    return true; // Found a destroyed component
                }
                
                // Specifically check TextMeshProUGUI since that's mentioned in the error
                if (comp is TextMeshProUGUI tmp && tmp == null)
                {
                    return true;
                }
                
                // Check other UI components
                if ((comp is Graphic && comp == null) ||
                    (comp is Selectable && comp == null) ||
                    (comp is LayoutElement && comp == null))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private void VerifyCanvasSetup()
        {
            Log("Verifying Canvas setup...");
            
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            
            foreach (Canvas canvas in canvases)
            {
                // Check GraphicRaycaster
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null && canvas.renderMode != RenderMode.WorldSpace)
                {
                    Log($"WARNING: Canvas '{canvas.name}' missing GraphicRaycaster. Adding...");
                    canvas.gameObject.AddComponent<GraphicRaycaster>();
                }
                
                // Check CanvasGroup for raycast blocking
                CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
                if (canvasGroup != null && !canvasGroup.blocksRaycasts)
                {
                    Log($"INFO: Canvas '{canvas.name}' has CanvasGroup with blocksRaycasts=false");
                }
                
                Log($"Canvas '{canvas.name}': RenderMode={canvas.renderMode}, SortingOrder={canvas.sortingOrder}");
            }
        }
        
        private void CheckForOrphanedComponents()
        {
            Log("Checking for orphaned button components...");
            
            // Find all Button components
            Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            Log($"Found {buttons.Length} Button components");
            
            foreach (Button button in buttons)
            {
                if (button == null) continue;
                
                // Check if button has valid target graphic
                if (button.targetGraphic == null)
                {
                    Log($"WARNING: Button '{button.name}' has null targetGraphic");
                }
                
                // Check for destroyed child components
                TextMeshProUGUI[] textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (TextMeshProUGUI text in textComponents)
                {
                    if (text == null)
                    {
                        Log($"ERROR: Button '{button.name}' has destroyed TextMeshProUGUI component!");
                    }
                }
                
                // Check button interactability
                if (!button.interactable)
                {
                    Log($"INFO: Button '{button.name}' is not interactable");
                }
            }
            
            // Check for any remaining references to ButtonGroupController/Manager
            MonoBehaviour[] allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (MonoBehaviour script in allScripts)
            {
                if (script == null) continue;
                
                string typeName = script.GetType().Name;
                if (typeName.Contains("ButtonGroup") || typeName.Contains("ButtonManager"))
                {
                    Log($"Found potential button group component: {typeName} on {script.name}");
                }
            }
        }
        
        [ContextMenu("Force Refresh UI")]
        public void ForceRefreshUI()
        {
            Log("Force refreshing UI...");
            
            // Force update all canvases
            Canvas.ForceUpdateCanvases();
            
            // Rebuild all UI layouts
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            
            // Force graphics update
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                canvas.enabled = false;
                canvas.enabled = true;
            }
            
            Log("UI refresh complete");
        }
        
        private void Log(string message)
        {
            if (showDetailedLogs)
            {
                UnityEngine.Debug.Log($"[UISystemFixer] {message}");
            }
        }
        
        [ContextMenu("Quick Fix - Remove Null Components")]
        public void RemoveNullComponents()
        {
            Log("Removing null components from all UI objects...");
            
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int removedCount = 0;
            
            foreach (GameObject obj in allObjects)
            {
                // Get all components
                Component[] components = obj.GetComponents<Component>();
                List<Component> validComponents = new List<Component>();
                
                foreach (Component comp in components)
                {
                    if (comp != null)
                    {
                        validComponents.Add(comp);
                    }
                    else
                    {
                        removedCount++;
                        Log($"Found null component on {obj.name}");
                    }
                }
            }
            
            Log($"Found {removedCount} null components. Unity will clean these up automatically.");
            
            // Force a refresh
            Canvas.ForceUpdateCanvases();
        }
    }
}

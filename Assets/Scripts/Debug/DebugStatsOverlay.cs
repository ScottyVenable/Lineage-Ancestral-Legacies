using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Lineage.Ancestral.Legacies.Debug
{
    /// <summary>
    /// On-screen debug stats overlay showing FPS, memory usage, and system information
    /// </summary>
    public class DebugStatsOverlay : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;
        [SerializeField] private float updateInterval = 0.5f;
          [Header("Stats to Display")]
        [SerializeField] private bool showFPS = true;
        [SerializeField] private bool showMemory = true;
        [SerializeField] private bool showSystemInfo = true;
        [SerializeField] private bool showPlayerPosition = true;
        [SerializeField] private bool showTimeScale = true;
        
        // Input System
        private InputAction toggleAction;
        
        private Canvas overlayCanvas;
        private TextMeshProUGUI statsText;
        private bool isVisible;
        private float lastUpdateTime;
        
        // Performance tracking
        private float fps;
        private int frameCount;
        private float deltaSum;
        
        [RuntimeInitializeOnLoadMethod]
        static void InitStatsOverlay()
        {
            var go = new GameObject("DebugStatsOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<DebugStatsOverlay>();
        }
          void Awake()
        {
            CreateUI();
            isVisible = showOnStart;
            overlayCanvas.gameObject.SetActive(isVisible);
            SetupInputActions();
        }
        
        void SetupInputActions()
        {
            // Create input action for toggle key
            toggleAction = new InputAction("ToggleDebugStats", InputActionType.Button);
            toggleAction.AddBinding($"<Keyboard>/{toggleKey.ToString().ToLower()}");
            toggleAction.performed += OnTogglePerformed;
            toggleAction.Enable();
        }
        
        void OnTogglePerformed(InputAction.CallbackContext context)
        {
            ToggleVisibility();
        }
          void Update()
        {
            if (isVisible)
            {
                UpdateFPSCounter();
                
                if (Time.time - lastUpdateTime >= updateInterval)
                {
                    UpdateStatsDisplay();
                    lastUpdateTime = Time.time;
                }
            }
        }
        
        void CreateUI()
        {
            // Create canvas
            GameObject canvasGO = new GameObject("StatsOverlayCanvas");
            overlayCanvas = canvasGO.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 999; // Ensure it's on top
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGO);
            
            // Create background panel
            GameObject panelGO = new GameObject("StatsPanel");
            panelGO.transform.SetParent(canvasGO.transform);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(10, -10);
            panelRect.sizeDelta = new Vector2(300, 200);
            
            var panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            
            // Create text component
            GameObject textGO = new GameObject("StatsText");
            textGO.transform.SetParent(panelGO.transform);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            statsText = textGO.AddComponent<TextMeshProUGUI>();
            statsText.text = "Debug Stats";
            statsText.fontSize = 12;
            statsText.color = Color.white;
            statsText.alignment = TextAlignmentOptions.TopLeft;
        }
        
        void UpdateFPSCounter()
        {
            frameCount++;
            deltaSum += Time.unscaledDeltaTime;
            
            if (frameCount >= 30) // Update FPS every 30 frames
            {
                fps = frameCount / deltaSum;
                frameCount = 0;
                deltaSum = 0;
            }
        }
        
        void UpdateStatsDisplay()
        {
            var stats = "";
            
            if (showFPS)
            {
                var fpsColor = fps >= 60 ? "green" : fps >= 30 ? "yellow" : "red";
                stats += $"<color={fpsColor}>FPS: {fps:F1}</color>\n";
                stats += $"Frame Time: {Time.unscaledDeltaTime * 1000:F1}ms\n";
            }
            
            if (showMemory)
            {
                var totalMemory = System.GC.GetTotalMemory(false) / 1024f / 1024f;
                var gpuMemory = UnityEngine.Profiling.Profiler.usedHeapSizeLong / 1024f / 1024f;
                stats += $"\n<color=cyan>Memory:</color>\n";
                stats += $"System: {totalMemory:F1} MB\n";
                stats += $"GPU: {gpuMemory:F1} MB\n";
            }
            
            if (showSystemInfo)
            {
                stats += $"\n<color=yellow>System:</color>\n";
                stats += $"Unity: {Application.unityVersion}\n";
                stats += $"Platform: {Application.platform}\n";
                stats += $"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}\n";
            }
            
            if (showTimeScale)
            {
                var timeColor = Time.timeScale == 1.0f ? "white" : "orange";
                stats += $"\n<color={timeColor}>Time Scale: {Time.timeScale:F2}</color>\n";
            }
            
            if (showPlayerPosition)
            {
                var playerTransform = GetPlayerTransform();
                if (playerTransform != null)
                {
                    var pos = playerTransform.position;
                    stats += $"\n<color=magenta>Position:</color>\n";
                    stats += $"X: {pos.x:F2}\n";
                    stats += $"Y: {pos.y:F2}\n";
                    stats += $"Z: {pos.z:F2}\n";
                }
            }
            
            stats += $"\n<color=gray>Toggle: {toggleKey}</color>";
            
            statsText.text = stats;
        }
        
        Transform GetPlayerTransform()
        {
            // Try to find player by common tags/names
            var player = GameObject.FindWithTag("Player");
            if (player != null) return player.transform;
            
            // Fallback to main camera
            var mainCamera = Camera.main;
            if (mainCamera != null) return mainCamera.transform;
            
            return null;
        }
        
        public void ToggleVisibility()
        {
            isVisible = !isVisible;
            overlayCanvas.gameObject.SetActive(isVisible);
            
            if (isVisible)
            {
                AdvancedLogger.LogInfo(LogCategory.UI, "Debug stats overlay enabled");
            }
            else
            {
                AdvancedLogger.LogInfo(LogCategory.UI, "Debug stats overlay disabled");
            }
        }
        
        public void SetVisibility(bool visible)
        {
            isVisible = visible;
            overlayCanvas.gameObject.SetActive(isVisible);
        }
        
        // Public methods for controlling what stats to show
        public void SetShowFPS(bool show) { showFPS = show; }
        public void SetShowMemory(bool show) { showMemory = show; }
        public void SetShowSystemInfo(bool show) { showSystemInfo = show; }
        public void SetShowPlayerPosition(bool show) { showPlayerPosition = show; }
        public void SetShowTimeScale(bool show) { showTimeScale = show; }
        public void SetUpdateInterval(float interval) { updateInterval = Mathf.Max(0.1f, interval); }
        
        void OnDestroy()
        {
            // Clean up input action
            if (toggleAction != null)
            {
                toggleAction.performed -= OnTogglePerformed;
                toggleAction.Disable();
                toggleAction.Dispose();
            }
        }
    }
}
#endif

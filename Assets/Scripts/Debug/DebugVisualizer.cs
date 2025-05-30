using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Lineage.Ancestral.Legacies.Debug
{
    /// <summary>
    /// Visual debugging system for drawing shapes, lines, and overlays in the scene
    /// </summary>
    public class DebugVisualizer : MonoBehaviour
    {
        private static DebugVisualizer instance;
        public static DebugVisualizer Instance 
        { 
            get 
            { 
                if (instance == null) 
                    instance = FindFirstObjectByType<DebugVisualizer>() ?? CreateInstance();
                return instance;
            }
        }
          [Header("Visual Debug Settings")]
        [SerializeField] private bool enableDebugDraw = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F4;
        
        // Input System
        private InputAction toggleAction;
        
        // Debug draw queues
        private List<DebugLine> debugLines = new List<DebugLine>();
        private List<DebugSphere> debugSpheres = new List<DebugSphere>();
        private List<DebugBox> debugBoxes = new List<DebugBox>();
        private List<DebugText> debugTexts = new List<DebugText>();
        
        // Materials for rendering
        private Material lineMaterial;
        private Material sphereMaterial;
        private Material boxMaterial;
        
        static DebugVisualizer CreateInstance()
        {
            var go = new GameObject("DebugVisualizer");
            DontDestroyOnLoad(go);
            return go.AddComponent<DebugVisualizer>();
        }
        
        [RuntimeInitializeOnLoadMethod]
        static void InitDebugVisualizer()
        {
            CreateInstance();
        }        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                CreateMaterials();
                SetupInputActions();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        void SetupInputActions()
        {
            // Create input action for toggle key
            toggleAction = new InputAction("ToggleDebugVisualization", InputActionType.Button);
            toggleAction.AddBinding($"<Keyboard>/{toggleKey.ToString().ToLower()}");
            toggleAction.performed += OnTogglePerformed;
            toggleAction.Enable();
        }
        
        void OnTogglePerformed(InputAction.CallbackContext context)
        {
            enableDebugDraw = !enableDebugDraw;
            AdvancedLogger.LogInfo(LogCategory.UI, $"Debug visualization {(enableDebugDraw ? "enabled" : "disabled")}");
        }        void Update()
        {
            // Clear expired debug items
            ClearExpiredItems();
        }
        
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
        
        void CreateMaterials()
        {
            // Create basic unlit materials for debug rendering
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            
            lineMaterial = new Material(shader);
            lineMaterial.color = Color.white;
            
            sphereMaterial = new Material(shader);
            sphereMaterial.color = Color.red;
            
            boxMaterial = new Material(shader);
            boxMaterial.color = Color.blue;
        }
        
        void OnRenderObject()
        {
            if (!enableDebugDraw) return;
            
            // Draw debug lines using GL
            if (debugLines.Count > 0)
            {
                lineMaterial.SetPass(0);
                GL.Begin(GL.LINES);
                
                foreach (var line in debugLines)
                {
                    GL.Color(line.color);
                    GL.Vertex3(line.start.x, line.start.y, line.start.z);
                    GL.Vertex3(line.end.x, line.end.y, line.end.z);
                }
                
                GL.End();
            }
        }
        
        void OnGUI()
        {
            if (!enableDebugDraw || debugTexts.Count == 0) return;
            
            foreach (var debugText in debugTexts)
            {
                var screenPos = Camera.main.WorldToScreenPoint(debugText.position);
                if (screenPos.z > 0) // In front of camera
                {
                    var rect = new Rect(screenPos.x, Screen.height - screenPos.y, 200, 20);
                    GUI.color = debugText.color;
                    GUI.Label(rect, debugText.text);
                }
            }
            GUI.color = Color.white;
        }
        
        void ClearExpiredItems()
        {
            var currentTime = Time.time;
            
            debugLines.RemoveAll(line => currentTime > line.endTime);
            debugSpheres.RemoveAll(sphere => currentTime > sphere.endTime);
            debugBoxes.RemoveAll(box => currentTime > box.endTime);
            debugTexts.RemoveAll(text => currentTime > text.endTime);
        }
        
        // Public API for drawing debug shapes
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f)
        {
            if (Instance == null || !Instance.enableDebugDraw) return;
            
            Instance.debugLines.Add(new DebugLine
            {
                start = start,
                end = end,
                color = color,
                endTime = Time.time + duration
            });
        }
        
        public static void DrawSphere(Vector3 center, float radius, Color color, float duration = 0f)
        {
            if (Instance == null || !Instance.enableDebugDraw) return;
            
            Instance.debugSpheres.Add(new DebugSphere
            {
                center = center,
                radius = radius,
                color = color,
                endTime = Time.time + duration
            });
            
            // Draw sphere as wireframe using lines
            const int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i * 2 * Mathf.PI) / segments;
                float angle2 = ((i + 1) * 2 * Mathf.PI) / segments;
                
                // Horizontal circle
                Vector3 p1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
                Vector3 p2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);
                DrawLine(p1, p2, color, duration);
                
                // Vertical circle
                p1 = center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0);
                p2 = center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0);
                DrawLine(p1, p2, color, duration);
            }
        }
        
        public static void DrawBox(Vector3 center, Vector3 size, Color color, float duration = 0f)
        {
            if (Instance == null || !Instance.enableDebugDraw) return;
            
            Instance.debugBoxes.Add(new DebugBox
            {
                center = center,
                size = size,
                color = color,
                endTime = Time.time + duration
            });
            
            // Draw box as wireframe
            Vector3 halfSize = size * 0.5f;
            Vector3[] corners = new Vector3[8];
            
            corners[0] = center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            corners[1] = center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            corners[2] = center + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            corners[3] = center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            corners[4] = center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            corners[5] = center + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            corners[6] = center + new Vector3(halfSize.x, halfSize.y, halfSize.z);
            corners[7] = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);
            
            // Bottom face
            DrawLine(corners[0], corners[1], color, duration);
            DrawLine(corners[1], corners[2], color, duration);
            DrawLine(corners[2], corners[3], color, duration);
            DrawLine(corners[3], corners[0], color, duration);
            
            // Top face
            DrawLine(corners[4], corners[5], color, duration);
            DrawLine(corners[5], corners[6], color, duration);
            DrawLine(corners[6], corners[7], color, duration);
            DrawLine(corners[7], corners[4], color, duration);
            
            // Vertical lines
            DrawLine(corners[0], corners[4], color, duration);
            DrawLine(corners[1], corners[5], color, duration);
            DrawLine(corners[2], corners[6], color, duration);
            DrawLine(corners[3], corners[7], color, duration);
        }
        
        public static void DrawText(Vector3 position, string text, Color color, float duration = 0f)
        {
            if (Instance == null || !Instance.enableDebugDraw) return;
            
            Instance.debugTexts.Add(new DebugText
            {
                position = position,
                text = text,
                color = color,
                endTime = Time.time + duration
            });
        }
        
        public static void DrawRay(Vector3 origin, Vector3 direction, Color color, float duration = 0f)
        {
            DrawLine(origin, origin + direction, color, duration);
        }
        
        public static void DrawBounds(Bounds bounds, Color color, float duration = 0f)
        {
            DrawBox(bounds.center, bounds.size, color, duration);
        }
        
        public static void DrawWireCube(Vector3 center, Vector3 size, Color color, float duration = 0f)
        {
            DrawBox(center, size, color, duration);
        }
        
        // AI Vision cone visualization
        public static void DrawVisionCone(Vector3 origin, Vector3 direction, float angle, float range, Color color, float duration = 0f)
        {
            if (Instance == null || !Instance.enableDebugDraw) return;
            
            const int segments = 10;
            float halfAngle = angle * 0.5f;
            
            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
                Vector3 rayDirection = Quaternion.AngleAxis(currentAngle, Vector3.up) * direction.normalized;
                DrawLine(origin, origin + rayDirection * range, color, duration);
            }
            
            // Draw arc at the end
            for (int i = 0; i < segments; i++)
            {
                float angle1 = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
                float angle2 = Mathf.Lerp(-halfAngle, halfAngle, (i + 1) / (float)segments);
                
                Vector3 dir1 = Quaternion.AngleAxis(angle1, Vector3.up) * direction.normalized;
                Vector3 dir2 = Quaternion.AngleAxis(angle2, Vector3.up) * direction.normalized;
                
                DrawLine(origin + dir1 * range, origin + dir2 * range, color, duration);
            }
        }
        
        public static void Clear()
        {
            if (Instance == null) return;
            
            Instance.debugLines.Clear();
            Instance.debugSpheres.Clear();
            Instance.debugBoxes.Clear();
            Instance.debugTexts.Clear();
        }
    }
    
    // Data structures for debug items
    struct DebugLine
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public float endTime;
    }
    
    struct DebugSphere
    {
        public Vector3 center;
        public float radius;
        public Color color;
        public float endTime;
    }
    
    struct DebugBox
    {
        public Vector3 center;
        public Vector3 size;
        public Color color;
        public float endTime;
    }
    
    struct DebugText
    {
        public Vector3 position;
        public string text;
        public Color color;
        public float endTime;
    }
}
#endif

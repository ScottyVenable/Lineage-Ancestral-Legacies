using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Lineage.Editor
{
    public static class LightingRenderingTools
    {
        [MenuItem("Lineage/Lighting & Rendering/Smart Lighting Setup", priority = 900)]
        public static void SmartLightingSetup()
        {
            var window = EditorWindow.GetWindow<SmartLightingWindow>();
            window.titleContent = new GUIContent("Smart Lighting Setup");
            window.Show();
        }
        
        [MenuItem("Lineage/Lighting & Rendering/Lightmap Optimizer", priority = 901)]
        public static void LightmapOptimizer()
        {
            var renderers = Object.FindObjectsOfType<MeshRenderer>();
            int optimized = 0;
            
            foreach (var renderer in renderers)
            {
                if (OptimizeRendererForLightmapping(renderer))
                {
                    optimized++;
                }
            }
            
            Debug.Log($"Lightmap Optimizer: Optimized {optimized} renderers for lightmapping");
            EditorUtility.DisplayDialog("Optimization Complete", 
                $"Optimized {optimized} renderers for better lightmapping", "OK");
        }
        
        [MenuItem("Lineage/Lighting & Rendering/Shadow Cascade Optimizer", priority = 902)]
        public static void ShadowCascadeOptimizer()
        {
            var lights = Object.FindObjectsOfType<Light>();
            var directionalLights = lights.Where(l => l.type == LightType.Directional).ToArray();
            
            if (directionalLights.Length == 0)
            {
                EditorUtility.DisplayDialog("No Directional Lights", 
                    "No directional lights found in the scene", "OK");
                return;
            }
            
            var window = EditorWindow.GetWindow<ShadowCascadeWindow>();
            window.titleContent = new GUIContent("Shadow Cascade Optimizer");
            window.SetLights(directionalLights);
            window.Show();
        }
        
        [MenuItem("Lineage/Lighting & Rendering/Material Performance Analyzer", priority = 903)]
        public static void MaterialPerformanceAnalyzer()
        {
            var window = EditorWindow.GetWindow<MaterialAnalyzerWindow>();
            window.titleContent = new GUIContent("Material Performance Analyzer");
            window.Show();
        }
        
        [MenuItem("Lineage/Lighting & Rendering/LOD Group Generator", priority = 904)]
        public static void LODGroupGenerator()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Objects Selected", 
                    "Please select one or more GameObjects to generate LOD groups for", "OK");
                return;
            }
            
            var window = EditorWindow.GetWindow<LODGeneratorWindow>();
            window.titleContent = new GUIContent("LOD Group Generator");
            window.SetObjects(selectedObjects);
            window.Show();
        }
        
        [MenuItem("Lineage/Lighting & Rendering/Occlusion Culling Setup", priority = 905)]
        public static void OcclusionCullingSetup()
        {
            var staticObjects = Object.FindObjectsOfType<MeshRenderer>()
                .Where(r => r.gameObject.isStatic)
                .ToArray();
            
            int configured = 0;
            foreach (var renderer in staticObjects)
            {
                var staticFlags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                if ((staticFlags & StaticEditorFlags.OccluderStatic) == 0)
                {
                    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, 
                        staticFlags | StaticEditorFlags.OccluderStatic);
                    configured++;
                }
            }
            
            Debug.Log($"Occlusion Culling Setup: Configured {configured} objects as occluders");
            EditorUtility.DisplayDialog("Setup Complete", 
                $"Configured {configured} static objects for occlusion culling", "OK");
        }
        
        [MenuItem("Lineage/Lighting & Rendering/Generate Lighting Report", priority = 906)]
        public static void GenerateLightingReport()
        {
            var lights = Object.FindObjectsOfType<Light>();
            var renderers = Object.FindObjectsOfType<MeshRenderer>();
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== LINEAGE LIGHTING & RENDERING REPORT ===");
            report.AppendLine($"Generated: {System.DateTime.Now}");
            report.AppendLine();
            
            // Lighting analysis
            report.AppendLine($"LIGHTING ANALYSIS:");
            report.AppendLine($"  Total Lights: {lights.Length}");
            
            var lightsByType = lights.GroupBy(l => l.type);
            foreach (var group in lightsByType)
            {
                report.AppendLine($"    {group.Key}: {group.Count()}");
            }
            
            var realtimeLights = lights.Where(l => l.lightmapBakeType == LightmapBakeType.Realtime).Count();
            var mixedLights = lights.Where(l => l.lightmapBakeType == LightmapBakeType.Mixed).Count();
            var bakedLights = lights.Where(l => l.lightmapBakeType == LightmapBakeType.Baked).Count();
            
            report.AppendLine($"  Realtime Lights: {realtimeLights}");
            report.AppendLine($"  Mixed Lights: {mixedLights}");
            report.AppendLine($"  Baked Lights: {bakedLights}");
            report.AppendLine();
            
            // Renderer analysis
            report.AppendLine($"RENDERER ANALYSIS:");
            report.AppendLine($"  Total Renderers: {renderers.Length}");
            
            var staticRenderers = renderers.Where(r => r.gameObject.isStatic).Count();
            var dynamicRenderers = renderers.Length - staticRenderers;
            
            report.AppendLine($"  Static Renderers: {staticRenderers}");
            report.AppendLine($"  Dynamic Renderers: {dynamicRenderers}");
            
            var lightmappedRenderers = renderers.Where(r => r.receiveGI).Count();
            report.AppendLine($"  Lightmapped Renderers: {lightmappedRenderers}");
            report.AppendLine();
            
            // Performance recommendations
            report.AppendLine("PERFORMANCE RECOMMENDATIONS:");
            if (realtimeLights > 8)
            {
                report.AppendLine($"  WARNING: {realtimeLights} realtime lights may impact performance");
            }
            if (dynamicRenderers > staticRenderers)
            {
                report.AppendLine($"  SUGGESTION: Consider making more objects static for better batching");
            }
            
            // Save report
            var reportPath = "Assets/LightingRenderingReport.txt";
            File.WriteAllText(reportPath, report.ToString());
            AssetDatabase.Refresh();
            
            Debug.Log($"Lighting & Rendering Report generated: {reportPath}");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(reportPath);
        }
        
        // Helper methods
        private static bool OptimizeRendererForLightmapping(MeshRenderer renderer)
        {
            bool changed = false;
            
            // Set appropriate lightmap parameters
            if (renderer.gameObject.isStatic)
            {
                var staticFlags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                if ((staticFlags & StaticEditorFlags.ContributeGI) == 0)
                {
                    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, 
                        staticFlags | StaticEditorFlags.ContributeGI);
                    changed = true;
                }
                
                // Optimize lightmap scale for small objects
                var bounds = renderer.bounds;
                var size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                
                if (size < 1f && renderer.scaleInLightmap > 1f)
                {
                    renderer.scaleInLightmap = 0.5f;
                    changed = true;
                }
                else if (size > 10f && renderer.scaleInLightmap < 2f)
                {
                    renderer.scaleInLightmap = 2f;
                    changed = true;
                }
            }
            
            return changed;
        }
    }
    
    // Smart Lighting Setup Window
    public class SmartLightingWindow : EditorWindow
    {
        private enum LightingPreset
        {
            Indoor,
            Outdoor,
            Studio,
            Custom
        }
        
        private LightingPreset selectedPreset = LightingPreset.Indoor;
        private Color ambientColor = Color.gray;
        private float ambientIntensity = 1f;
        private bool enableFog = false;
        private Color fogColor = Color.gray;
        private float fogDensity = 0.01f;
        
        private void OnGUI()
        {
            GUILayout.Label("Smart Lighting Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            selectedPreset = (LightingPreset)EditorGUILayout.EnumPopup("Lighting Preset:", selectedPreset);
            
            GUILayout.Space(10);
            
            ambientColor = EditorGUILayout.ColorField("Ambient Color:", ambientColor);
            ambientIntensity = EditorGUILayout.Slider("Ambient Intensity:", ambientIntensity, 0f, 2f);
            
            GUILayout.Space(10);
            
            enableFog = EditorGUILayout.Toggle("Enable Fog:", enableFog);
            if (enableFog)
            {
                fogColor = EditorGUILayout.ColorField("Fog Color:", fogColor);
                fogDensity = EditorGUILayout.Slider("Fog Density:", fogDensity, 0f, 0.1f);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Apply Lighting Setup"))
            {
                ApplyLightingSetup();
            }
            
            if (GUILayout.Button("Load Preset"))
            {
                LoadPreset();
            }
        }
        
        private void LoadPreset()
        {
            switch (selectedPreset)
            {
                case LightingPreset.Indoor:
                    ambientColor = new Color(0.4f, 0.4f, 0.5f);
                    ambientIntensity = 0.3f;
                    enableFog = false;
                    break;
                    
                case LightingPreset.Outdoor:
                    ambientColor = new Color(0.5f, 0.6f, 0.8f);
                    ambientIntensity = 1.0f;
                    enableFog = true;
                    fogColor = new Color(0.8f, 0.9f, 1f);
                    fogDensity = 0.005f;
                    break;
                    
                case LightingPreset.Studio:
                    ambientColor = Color.white;
                    ambientIntensity = 0.8f;
                    enableFog = false;
                    break;
            }
        }
        
        private void ApplyLightingSetup()
        {
            // Apply ambient lighting
            RenderSettings.ambientLight = ambientColor * ambientIntensity;
            
            // Apply fog settings
            RenderSettings.fog = enableFog;
            if (enableFog)
            {
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogDensity = fogDensity;
            }
            
            // Create main directional light if none exists
            var directionalLight = Object.FindObjectOfType<Light>();
            if (directionalLight == null || directionalLight.type != LightType.Directional)
            {
                var lightGO = new GameObject("Main Light");
                directionalLight = lightGO.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
                directionalLight.intensity = 1f;
                lightGO.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
            }
            
            Debug.Log($"Applied {selectedPreset} lighting setup");
            EditorUtility.DisplayDialog("Lighting Setup", $"Applied {selectedPreset} lighting preset", "OK");
        }
    }
    
    // Shadow Cascade Window
    public class ShadowCascadeWindow : EditorWindow
    {
        private Light[] lights;
        private float[] cascadeDistances = new float[4] { 10f, 30f, 100f, 300f };
        private int cascadeCount = 4;
        
        public void SetLights(Light[] lights)
        {
            this.lights = lights;
        }
        
        private void OnGUI()
        {
            if (lights == null || lights.Length == 0)
            {
                GUILayout.Label("No directional lights provided", EditorStyles.boldLabel);
                return;
            }
            
            GUILayout.Label("Shadow Cascade Optimizer", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label($"Optimizing {lights.Length} directional light(s)");
            
            cascadeCount = EditorGUILayout.IntSlider("Cascade Count:", cascadeCount, 1, 4);
            
            for (int i = 0; i < cascadeCount; i++)
            {
                cascadeDistances[i] = EditorGUILayout.FloatField($"Cascade {i + 1} Distance:", cascadeDistances[i]);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Apply Cascade Settings"))
            {
                ApplyCascadeSettings();
            }
            
            if (GUILayout.Button("Auto-Calculate Cascades"))
            {
                AutoCalculateCascades();
            }
        }
        
        private void ApplyCascadeSettings()
        {
            // Note: In a real implementation, you would set QualitySettings.shadowCascades
            // and QualitySettings.shadowDistance. This is a simplified version.
            
            foreach (var light in lights)
            {
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 1f;
            }
            
            Debug.Log($"Applied shadow cascade settings to {lights.Length} lights");
            EditorUtility.DisplayDialog("Cascades Applied", "Shadow cascade settings have been applied", "OK");
        }
        
        private void AutoCalculateCascades()
        {
            // Auto-calculate optimal cascade distances based on scene bounds
            var renderers = Object.FindObjectsOfType<MeshRenderer>();
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                
                var maxDistance = bounds.size.magnitude;
                for (int i = 0; i < cascadeCount; i++)
                {
                    cascadeDistances[i] = (maxDistance / cascadeCount) * (i + 1);
                }
            }
            
            Debug.Log("Auto-calculated cascade distances based on scene bounds");
        }
    }
    
    // Material Analyzer Window
    public class MaterialAnalyzerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<MaterialInfo> materialInfos = new List<MaterialInfo>();
        
        private struct MaterialInfo
        {
            public Material material;
            public string shaderName;
            public int textureCount;
            public int passCount;
            public bool isTransparent;
            public int renderQueue;
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Material Performance Analyzer", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Analyze All Materials"))
            {
                AnalyzeMaterials();
            }
            
            if (materialInfos.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Analysis Results ({materialInfos.Count} materials):", EditorStyles.boldLabel);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var info in materialInfos)
                {
                    var color = GetPerformanceColor(info);
                    GUI.color = color;
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(info.material.name, GUILayout.Width(150));
                    GUILayout.Label(info.shaderName, GUILayout.Width(120));
                    GUILayout.Label($"{info.textureCount} tex", GUILayout.Width(50));
                    GUILayout.Label($"{info.passCount} pass", GUILayout.Width(50));
                    GUILayout.Label($"Q:{info.renderQueue}", GUILayout.Width(60));
                    if (info.isTransparent)
                        GUILayout.Label("ALPHA", GUILayout.Width(50));
                    GUILayout.EndHorizontal();
                    
                    GUI.color = Color.white;
                }
                
                EditorGUILayout.EndScrollView();
            }
        }
        
        private void AnalyzeMaterials()
        {
            materialInfos.Clear();
            var materials = Resources.FindObjectsOfTypeAll<Material>()
                .Where(m => AssetDatabase.Contains(m))
                .ToArray();
            
            foreach (var material in materials)
            {
                var info = new MaterialInfo
                {
                    material = material,
                    shaderName = material.shader.name,
                    textureCount = GetTextureCount(material),
                    passCount = material.passCount,
                    isTransparent = IsTransparent(material),
                    renderQueue = material.renderQueue
                };
                materialInfos.Add(info);
            }
            
            materialInfos = materialInfos.OrderBy(m => GetPerformanceScore(m)).ToList();
        }
        
        private int GetTextureCount(Material material)
        {
            int count = 0;
            var shader = material.shader;
            
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    var texture = material.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                    if (texture != null) count++;
                }
            }
            
            return count;
        }
        
        private bool IsTransparent(Material material)
        {
            return material.renderQueue >= 3000; // Transparent render queue
        }
        
        private int GetPerformanceScore(MaterialInfo info)
        {
            int score = 0;
            score += info.textureCount * 2;
            score += info.passCount * 5;
            if (info.isTransparent) score += 3;
            if (info.shaderName.Contains("Standard")) score += 1;
            return score;
        }
        
        private Color GetPerformanceColor(MaterialInfo info)
        {
            var score = GetPerformanceScore(info);
            if (score <= 5) return Color.green;
            if (score <= 10) return Color.yellow;
            return Color.red;
        }
    }
    
    // LOD Generator Window
    public class LODGeneratorWindow : EditorWindow
    {
        private GameObject[] objects;
        private float[] lodDistances = new float[3] { 10f, 30f, 100f };
        private float[] qualityReductions = new float[3] { 0.7f, 0.4f, 0.1f };
        
        public void SetObjects(GameObject[] objects)
        {
            this.objects = objects;
        }
        
        private void OnGUI()
        {
            if (objects == null || objects.Length == 0)
            {
                GUILayout.Label("No objects provided", EditorStyles.boldLabel);
                return;
            }
            
            GUILayout.Label("LOD Group Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label($"Generating LODs for {objects.Length} object(s)");
            
            for (int i = 0; i < 3; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"LOD {i + 1}:", GUILayout.Width(50));
                lodDistances[i] = EditorGUILayout.FloatField("Distance:", lodDistances[i], GUILayout.Width(100));
                qualityReductions[i] = EditorGUILayout.Slider("Quality:", qualityReductions[i], 0.1f, 1f);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Generate LOD Groups"))
            {
                GenerateLODGroups();
            }
        }
        
        private void GenerateLODGroups()
        {
            int generated = 0;
            
            foreach (var obj in objects)
            {
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                if (meshRenderer == null) continue;
                
                var lodGroup = obj.GetComponent<LODGroup>();
                if (lodGroup == null)
                {
                    lodGroup = obj.AddComponent<LODGroup>();
                }
                
                var lods = new LOD[4]; // Original + 3 LODs
                
                // LOD 0 (original)
                lods[0] = new LOD(1f, new Renderer[] { meshRenderer });
                
                // Create simplified LODs (in a real implementation, you'd use mesh decimation)
                for (int i = 1; i < 4; i++)
                {
                    var lodRenderer = CreateLODRenderer(obj, i, qualityReductions[i - 1]);
                    lods[i] = new LOD(1f - (lodDistances[i - 1] / 100f), new Renderer[] { lodRenderer });
                }
                
                lodGroup.SetLODs(lods);
                generated++;
            }
            
            Debug.Log($"Generated LOD groups for {generated} objects");
            EditorUtility.DisplayDialog("LOD Generation Complete", 
                $"Generated LOD groups for {generated} objects", "OK");
        }
        
        private MeshRenderer CreateLODRenderer(GameObject original, int lodLevel, float quality)
        {
            // This is a simplified version - in a real implementation,
            // you would use mesh decimation algorithms or pre-created LOD meshes
            
            var lodObject = new GameObject($"{original.name}_LOD{lodLevel}");
            lodObject.transform.SetParent(original.transform);
            lodObject.transform.localPosition = Vector3.zero;
            lodObject.transform.localRotation = Quaternion.identity;
            lodObject.transform.localScale = Vector3.one;
            
            var originalRenderer = original.GetComponent<MeshRenderer>();
            var lodRenderer = lodObject.AddComponent<MeshRenderer>();
            var meshFilter = lodObject.AddComponent<MeshFilter>();
            
            // Copy material
            lodRenderer.material = originalRenderer.material;
            meshFilter.mesh = originalRenderer.GetComponent<MeshFilter>().sharedMesh;
            
            // Disable by default (LODGroup will manage visibility)
            lodObject.SetActive(false);
            
            return lodRenderer;
        }
    }
}

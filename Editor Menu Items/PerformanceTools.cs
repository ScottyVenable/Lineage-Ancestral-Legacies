using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using System.Linq;
using UnityEngine.Rendering;

namespace Lineage.Editor
{
    /// <summary>
    /// Performance debugging and optimization tools
    /// </summary>
    public static class PerformanceTools
    {
        [MenuItem("Lineage/Performance/Memory Snapshot", false, 300)]
        public static void TakeMemorySnapshot()
        {
            long totalMemory = Profiler.GetTotalAllocatedMemory(Profiler.GetAreaCount());
            long reservedMemory = Profiler.GetTotalReservedMemory(Profiler.GetAreaCount());
            long unusedMemory = Profiler.GetTotalUnusedReservedMemory(Profiler.GetAreaCount());
            
            Debug.Log("=== Memory Snapshot ===");
            Debug.Log($"Total Allocated: {totalMemory / (1024 * 1024)} MB");
            Debug.Log($"Total Reserved: {reservedMemory / (1024 * 1024)} MB");
            Debug.Log($"Unused Reserved: {unusedMemory / (1024 * 1024)} MB");
            Debug.Log($"Available System Memory: {SystemInfo.systemMemorySize} MB");
        }
        
        [MenuItem("Lineage/Performance/Force Garbage Collection", false, 301)]
        public static void ForceGarbageCollection()
        {
            long beforeMemory = System.GC.GetTotalMemory(false);
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            long afterMemory = System.GC.GetTotalMemory(false);
            
            Debug.Log($"[Performance] Garbage Collection freed {(beforeMemory - afterMemory) / 1024} KB");
        }
        
        [MenuItem("Lineage/Performance/Count Render Objects", false, 320)]
        public static void CountRenderObjects()
        {
            Renderer[] renderers = Object.FindObjectsOfType<Renderer>();
            MeshFilter[] meshFilters = Object.FindObjectsOfType<MeshFilter>();
            
            Debug.Log("=== Render Object Count ===");
            Debug.Log($"Total Renderers: {renderers.Length}");
            Debug.Log($"Mesh Filters: {meshFilters.Length}");
            
            var rendererTypes = renderers
                .GroupBy(r => r.GetType().Name)
                .OrderByDescending(g => g.Count());
            
            Debug.Log("Renderer Types:");
            foreach (var group in rendererTypes)
            {
                Debug.Log($"  {group.Key}: {group.Count()}");
            }
        }
        
        [MenuItem("Lineage/Performance/Analyze Texture Memory", false, 340)]
        public static void AnalyzeTextureMemory()
        {
            Texture[] allTextures = Resources.FindObjectsOfTypeAll<Texture>();
            long totalMemory = 0;
            
            var textureInfo = allTextures
                .Where(t => t != null)
                .Select(t => new {
                    Name = t.name,
                    Size = Profiler.GetRuntimeMemorySizeLong(t),
                    Width = t.width,
                    Height = t.height
                })
                .OrderByDescending(info => info.Size)
                .Take(10);
            
            Debug.Log("=== Top 10 Largest Textures ===");
            foreach (var info in textureInfo)
            {
                totalMemory += info.Size;
                Debug.Log($"{info.Name}: {info.Size / (1024 * 1024)} MB ({info.Width}x{info.Height})");
            }
            
            Debug.Log($"Total texture memory (all): {totalMemory / (1024 * 1024)} MB");
        }
        
        [MenuItem("Lineage/Performance/Profile Current Scene", false, 360)]
        public static void ProfileCurrentScene()
        {
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            Component[] allComponents = Object.FindObjectsOfType<Component>();
            
            Debug.Log("=== Scene Performance Profile ===");
            Debug.Log($"GameObjects: {allObjects.Length}");
            Debug.Log($"Components: {allComponents.Length}");
            Debug.Log($"Average components per object: {(float)allComponents.Length / allObjects.Length:F2}");
            
            // Check for expensive components
            int particleSystems = Object.FindObjectsOfType<ParticleSystem>().Length;
            int animators = Object.FindObjectsOfType<Animator>().Length;
            int rigidbodies = Object.FindObjectsOfType<Rigidbody>().Length;
            int lights = Object.FindObjectsOfType<Light>().Length;
            
            Debug.Log("Potentially expensive components:");
            Debug.Log($"  Particle Systems: {particleSystems}");
            Debug.Log($"  Animators: {animators}");
            Debug.Log($"  Rigidbodies: {rigidbodies}");
            Debug.Log($"  Lights: {lights}");
        }
        
        [MenuItem("Lineage/Performance/Check Graphics Settings", false, 380)]
        public static void CheckGraphicsSettings()
        {
            Debug.Log("=== Graphics Settings ===");
            Debug.Log($"Graphics Device: {SystemInfo.graphicsDeviceName}");
            Debug.Log($"Graphics Memory: {SystemInfo.graphicsMemorySize} MB");
            Debug.Log($"Max Texture Size: {SystemInfo.maxTextureSize}");
            Debug.Log($"Supports Shadows: {SystemInfo.supportsShadows}");
            Debug.Log($"Shadow Cascade Count: {QualitySettings.shadowCascades}");
            Debug.Log($"Current Quality Level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        }
    }
}

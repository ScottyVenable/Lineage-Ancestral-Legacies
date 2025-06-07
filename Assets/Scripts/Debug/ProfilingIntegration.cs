using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Generic;
using System.Text;
using System;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Lineage.Debug
{
    /// <summary>
    /// Profiling integration helper for advanced performance analysis.
    /// Provides easy-to-use profiling markers, custom sampling, and performance reporting.
    /// </summary>
    public static class ProfilingIntegration
    {
        private static Dictionary<string, CustomSampler> customSamplers = new Dictionary<string, CustomSampler>();
        private static Dictionary<string, Unity.Profiling.ProfilerMarker> profilerMarkers = new Dictionary<string, Unity.Profiling.ProfilerMarker>();
        private static Dictionary<string, PerformanceMetric> performanceMetrics = new Dictionary<string, PerformanceMetric>();
        
        public struct PerformanceMetric
        {
            public string name;
            public float totalTime;
            public float averageTime;
            public int sampleCount;
            public float minTime;
            public float maxTime;
            public DateTime lastSample;
        }
        
        public struct ProfilingSession : IDisposable
        {
            private readonly string sessionName;
            private readonly float startTime;
            private readonly Unity.Profiling.ProfilerMarker marker;
            
            public ProfilingSession(string name)
            {
                sessionName = name;
                startTime = Time.realtimeSinceStartup;
                marker = GetOrCreateProfilerMarker(name);
                marker.Begin();
            }
            
            public void Dispose()
            {
                marker.End();
                float duration = Time.realtimeSinceStartup - startTime;
                RecordPerformanceMetric(sessionName, duration);
            }
        }
        
        // Initialize profiling integration
        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            RegisterProfilingCommands();
            CreateCommonProfilerMarkers();
        }
        
        private static void RegisterProfilingCommands()
        {
            var console = UnityEngine.Object.FindFirstObjectByType<DebugConsoleManager>();
            if (console != null)
            {
                console.RegisterCommand("profiler_report", "Generate performance profiling report", "profiler_report", 
                    (args, data) => { GenerateProfilingReport(args.ToArray()); return "Profiling report generated"; }, false);
                console.RegisterCommand("profiler_clear", "Clear profiling data", "profiler_clear", 
                    (args, data) => { ClearProfilingData(args.ToArray()); return "Profiling data cleared"; }, false);
                console.RegisterCommand("profiler_start", "Start a profiling session", "profiler_start <session_name>", 
                    (args, data) => { StartProfilingSession(args.ToArray()); return "Profiling session started"; }, false);
                console.RegisterCommand("profiler_stop", "Stop a profiling session", "profiler_stop <session_name>", 
                    (args, data) => { StopProfilingSession(args.ToArray()); return "Profiling session stopped"; }, false);
                console.RegisterCommand("memory_report", "Generate detailed memory usage report", "memory_report", 
                    (args, data) => { GenerateMemoryReport(args.ToArray()); return "Memory report generated"; }, false);
                console.RegisterCommand("gc_analyze", "Analyze garbage collection patterns", "gc_analyze", 
                    (args, data) => { AnalyzeGarbageCollection(args.ToArray()); return "GC analysis completed"; }, false);
            }
        }
        
        private static void CreateCommonProfilerMarkers()
        {
            // Create common profiler markers for the game
            GetOrCreateProfilerMarker("Game.Update");
            GetOrCreateProfilerMarker("Game.FixedUpdate");
            GetOrCreateProfilerMarker("Game.LateUpdate");
            GetOrCreateProfilerMarker("AI.Processing");
            GetOrCreateProfilerMarker("Inventory.Update");
            GetOrCreateProfilerMarker("Quest.Processing");
            GetOrCreateProfilerMarker("Combat.Resolution");
            GetOrCreateProfilerMarker("UI.Update");
            GetOrCreateProfilerMarker("Audio.Processing");
            GetOrCreateProfilerMarker("Physics.Simulation");
        }
        
        // Public API
        public static ProfilingSession StartSession(string sessionName)
        {
            return new ProfilingSession(sessionName);
        }
        
        public static void BeginSample(string sampleName)
        {
            GetOrCreateCustomSampler(sampleName).Begin();
        }
        
        public static void EndSample(string sampleName)
        {
            if (customSamplers.ContainsKey(sampleName))
            {
                customSamplers[sampleName].End();
            }
        }
        
        public static void RecordFrameTiming(string category, float deltaTime)
        {
            RecordPerformanceMetric($"Frame.{category}", deltaTime);
        }
        
        public static void RecordMethodTiming(string methodName, float executionTime)
        {
            RecordPerformanceMetric($"Method.{methodName}", executionTime);
        }
        
        public static Unity.Profiling.ProfilerMarker GetProfilerMarker(string name)
        {
            return GetOrCreateProfilerMarker(name);
        }
        
        // Helper methods
        private static CustomSampler GetOrCreateCustomSampler(string name)
        {
            if (!customSamplers.ContainsKey(name))
            {
                customSamplers[name] = CustomSampler.Create(name);
            }
            return customSamplers[name];
        }
        
        private static Unity.Profiling.ProfilerMarker GetOrCreateProfilerMarker(string name)
        {
            if (!profilerMarkers.ContainsKey(name))
            {
                profilerMarkers[name] = new Unity.Profiling.ProfilerMarker(name);
            }
            return profilerMarkers[name];
        }
        
        private static void RecordPerformanceMetric(string name, float time)
        {
            if (!performanceMetrics.ContainsKey(name))
            {
                performanceMetrics[name] = new PerformanceMetric
                {
                    name = name,
                    totalTime = 0,
                    averageTime = 0,
                    sampleCount = 0,
                    minTime = float.MaxValue,
                    maxTime = float.MinValue,
                    lastSample = DateTime.Now
                };
            }
            
            var metric = performanceMetrics[name];
            metric.totalTime += time;
            metric.sampleCount++;
            metric.averageTime = metric.totalTime / metric.sampleCount;
            metric.minTime = Mathf.Min(metric.minTime, time);
            metric.maxTime = Mathf.Max(metric.maxTime, time);
            metric.lastSample = DateTime.Now;
            
            performanceMetrics[name] = metric;
        }
        
        // Command handlers
        private static void GenerateProfilingReport(string[] args)
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== PROFILING REPORT ===");
            report.AppendLine($"Generated at: {DateTime.Now}");
            report.AppendLine();
            
            // System information
            report.AppendLine("SYSTEM INFORMATION:");
            report.AppendLine($"Platform: {Application.platform}");
            report.AppendLine($"Unity Version: {Application.unityVersion}");
            report.AppendLine($"Target Frame Rate: {Application.targetFrameRate}");
            report.AppendLine($"Current FPS: {1.0f / Time.unscaledDeltaTime:F1}");
            report.AppendLine();
            
            // Memory information
            report.AppendLine("MEMORY USAGE:");
            report.AppendLine($"Total Memory: {Profiler.usedHeapSizeLong / 1024f / 1024f:F2} MB");
            report.AppendLine($"Mono Memory: {(System.GC.GetTotalMemory(false) / 1024f / 1024f):F2} MB");
            report.AppendLine($"Graphics Memory: {Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024f / 1024f:F2} MB");
            report.AppendLine();
            
            // Performance metrics
            if (performanceMetrics.Count > 0)
            {
                report.AppendLine("PERFORMANCE METRICS:");
                report.AppendLine("Name".PadRight(30) + "Avg(ms)".PadRight(10) + "Min(ms)".PadRight(10) + 
                    "Max(ms)".PadRight(10) + "Samples".PadRight(10) + "Total(ms)");
                report.AppendLine(new string('-', 80));
                
                foreach (var metric in performanceMetrics.Values)
                {
                    report.AppendLine(
                        metric.name.PadRight(30) + 
                        $"{metric.averageTime * 1000:F4}".PadRight(10) + 
                        $"{metric.minTime * 1000:F4}".PadRight(10) + 
                        $"{metric.maxTime * 1000:F4}".PadRight(10) + 
                        metric.sampleCount.ToString().PadRight(10) + 
                        $"{metric.totalTime * 1000:F2}"
                    );
                }
            }
            
            report.AppendLine();
            report.AppendLine("=== END REPORT ===");
              // Log each line separately for better readability
            string[] lines = report.ToString().Split('\n');
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    AdvancedLogger.Log(LogLevel.Info, LogCategory.Performance, line);
                }
            }
        }
          private static void ClearProfilingData(string[] args)
        {
            performanceMetrics.Clear();
            AdvancedLogger.Log(LogLevel.Info, LogCategory.Performance, "Profiling data cleared.");
        }
        
        private static void StartProfilingSession(string[] args)
        {
            if (args.Length < 1)
            {
                AdvancedLogger.LogWarning(LogCategory.Performance, "Usage: profiler_start <session_name>");
                return;
            }
            
            string sessionName = args[0];
            Profiler.BeginSample(sessionName);
            AdvancedLogger.LogInfo(LogCategory.Performance, $"Started profiling session: {sessionName}");
        }
        
        private static void StopProfilingSession(string[] args)
        {
            if (args.Length < 1)
            {
                AdvancedLogger.LogWarning(LogCategory.Performance, "Usage: profiler_stop <session_name>");
                return;
            }
            
            string sessionName = args[0];
            Profiler.EndSample();
            AdvancedLogger.LogInfo(LogCategory.Performance, $"Stopped profiling session: {sessionName}");
        }
        
        private static void GenerateMemoryReport(string[] args)
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== MEMORY REPORT ===");
            report.AppendLine($"Generated at: {DateTime.Now}");
            report.AppendLine();
            
            // Detailed memory breakdown
            long totalMemory = Profiler.usedHeapSizeLong;
            long monoMemory = System.GC.GetTotalMemory(false);
            long graphicsMemory = Profiler.GetAllocatedMemoryForGraphicsDriver();
            long audioMemory = Profiler.GetAllocatedMemoryForGraphicsDriver(); // Approximation
            
            report.AppendLine("MEMORY BREAKDOWN:");
            report.AppendLine($"Total Heap: {totalMemory / 1024f / 1024f:F2} MB");
            report.AppendLine($"Mono Heap: {monoMemory / 1024f / 1024f:F2} MB");
            report.AppendLine($"Graphics: {graphicsMemory / 1024f / 1024f:F2} MB");
            report.AppendLine($"Audio: {audioMemory / 1024f / 1024f:F2} MB (approx)");
            report.AppendLine();
            
            // GC information
            report.AppendLine("GARBAGE COLLECTION:");
            for (int gen = 0; gen <= 2; gen++)
            {
                report.AppendLine($"Gen {gen} Collections: {System.GC.CollectionCount(gen)}");
            }
            report.AppendLine();
            
            // Object counts (if available)
            report.AppendLine("UNITY OBJECTS:");
            UnityEngine.Object[] allObjects = UnityEngine.Object.FindObjectsByType<UnityEngine.Object>(FindObjectsSortMode.None);
            Dictionary<System.Type, int> objectCounts = new Dictionary<System.Type, int>();
            
            foreach (var obj in allObjects)
            {
                System.Type type = obj.GetType();
                if (!objectCounts.ContainsKey(type))
                    objectCounts[type] = 0;
                objectCounts[type]++;
            }
            
            // Sort by count and show top 10
            var sortedObjects = new List<KeyValuePair<System.Type, int>>(objectCounts);
            sortedObjects.Sort((x, y) => y.Value.CompareTo(x.Value));
            
            foreach (var kvp in sortedObjects.GetRange(0, Mathf.Min(10, sortedObjects.Count)))
            {
                report.AppendLine($"{kvp.Key.Name}: {kvp.Value}");
            }
            
            report.AppendLine();
            report.AppendLine("=== END MEMORY REPORT ===");
            
            // Log the report
            string[] lines = report.ToString().Split('\n');
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    AdvancedLogger.LogInfo(LogCategory.Performance, line);
                }
            }
        }
        
        private static void AnalyzeGarbageCollection(string[] args)
        {
            AdvancedLogger.LogInfo(LogCategory.Performance, "=== GARBAGE COLLECTION ANALYSIS ===");
            
            // Current GC stats
            for (int gen = 0; gen <= 2; gen++)
            {
                int collections = System.GC.CollectionCount(gen);
                AdvancedLogger.LogInfo(LogCategory.Performance, $"Generation {gen}: {collections} collections");
            }
            
            // Memory before and after GC
            long memoryBefore = System.GC.GetTotalMemory(false);
            AdvancedLogger.LogInfo(LogCategory.Performance, $"Memory before GC: {memoryBefore / 1024f / 1024f:F2} MB");
            
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            
            long memoryAfter = System.GC.GetTotalMemory(false);
            long freedMemory = memoryBefore - memoryAfter;
              AdvancedLogger.LogInfo(LogCategory.Performance, $"Memory after GC: {memoryAfter / 1024f / 1024f:F2} MB");
            AdvancedLogger.LogInfo(LogCategory.Performance, $"Memory freed: {freedMemory / 1024f / 1024f:F2} MB");
            
            // GC efficiency
            float efficiency = memoryBefore > 0 ? (float)freedMemory / memoryBefore * 100 : 0;
            AdvancedLogger.LogInfo(LogCategory.Performance, $"GC Efficiency: {efficiency:F1}%");
            
            AdvancedLogger.LogInfo(LogCategory.Performance, "=== END GC ANALYSIS ===");
        }
        
        // Performance monitoring helpers
        public static void MonitorFrameRate()
        {
            float deltaTime = Time.unscaledDeltaTime;
            float fps = 1.0f / deltaTime;
            
            RecordPerformanceMetric("Frame.DeltaTime", deltaTime);
            RecordPerformanceMetric("Frame.FPS", fps);
            
            // Log performance warnings
            if (fps < 30)
            {
                AdvancedLogger.LogWarning(LogCategory.Performance, $"Low FPS detected: {fps:F1}");
            }
            
            if (deltaTime > 0.05f) // 50ms frame time
            {
                AdvancedLogger.LogWarning(LogCategory.Performance, $"High frame time: {deltaTime * 1000:F1}ms");
            }
        }
        
        public static void MonitorMemoryUsage()
        {
            long currentMemory = System.GC.GetTotalMemory(false);
            float memoryMB = currentMemory / 1024f / 1024f;
            
            RecordPerformanceMetric("Memory.MonoHeap", memoryMB);
            
            // Log memory warnings
            if (memoryMB > 500) // 500MB threshold
            {
                AdvancedLogger.LogWarning(LogCategory.Performance, $"High memory usage: {memoryMB:F1}MB");
            }
        }
        
        // Automatic performance monitoring
        public static void EnableAutomaticMonitoring()
        {
            var monitorObject = new GameObject("PerformanceMonitor");
            monitorObject.AddComponent<PerformanceMonitor>();
            UnityEngine.Object.DontDestroyOnLoad(monitorObject);
        }
        
        private class PerformanceMonitor : MonoBehaviour
        {
            private float lastFrameRateCheck = 0;
            private float lastMemoryCheck = 0;
            private const float CHECK_INTERVAL = 1.0f; // Check every second
            
            private void Update()
            {
                float currentTime = Time.unscaledTime;
                
                // Monitor frame rate
                if (currentTime - lastFrameRateCheck >= CHECK_INTERVAL)
                {
                    MonitorFrameRate();
                    lastFrameRateCheck = currentTime;
                }
                
                // Monitor memory usage
                if (currentTime - lastMemoryCheck >= CHECK_INTERVAL * 5) // Every 5 seconds
                {
                    MonitorMemoryUsage();
                    lastMemoryCheck = currentTime;
                }
            }
        }
    }
}
#endif

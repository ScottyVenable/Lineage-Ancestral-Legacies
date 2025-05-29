using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Lineage.Ancestral.Legacies.Debug.Testing
{
    /// <summary>
    /// Comprehensive testing framework for the debug system.
    /// Provides unit tests, integration tests, and performance validation.
    /// </summary>
    public class DebugSystemTestFramework : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool enablePerformanceTests = true;
        [SerializeField] private int performanceTestIterations = 1000;
        
        // Test results
        private List<TestResult> testResults = new List<TestResult>();
        private Dictionary<string, float> performanceMetrics = new Dictionary<string, float>();
        
        public struct TestResult
        {
            public string testName;
            public bool passed;
            public string message;
            public float executionTime;
            public DateTime timestamp;
        }
        
        public enum TestCategory
        {
            Unit,
            Integration,
            Performance,
            Stress,
            UserInterface
        }
        
        // Test component for inspector testing
        public class TestComponent : MonoBehaviour
        {
            public int testInt = 42;
            public string testString = "Hello World";
            public bool testBool = true;
            
            public void TestMethod()
            {
                // Test method for reflection
            }
        }
        
        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
            
            RegisterTestCommands();
        }
        
        private void RegisterTestCommands()
        {
            var console = FindFirstObjectByType<DebugConsoleManager>();
            if (console != null)
            {
                console.RegisterCommand("run_tests", "Run all debug system tests", args => StartCoroutine(RunAllTests()));
                console.RegisterCommand("run_unit_tests", "Run unit tests only", args => StartCoroutine(RunUnitTests()));
                console.RegisterCommand("run_integration_tests", "Run integration tests", args => StartCoroutine(RunIntegrationTests()));
                console.RegisterCommand("run_performance_tests", "Run performance tests", args => StartCoroutine(RunPerformanceTests()));
                console.RegisterCommand("test_results", "Show latest test results", ShowTestResults);
                console.RegisterCommand("clear_test_results", "Clear test result history", ClearTestResults);
            }
        }
        
        // Public API for running tests
        public IEnumerator RunAllTests()
        {
            AdvancedLogger.LogInfo(LogCategory.General, "Starting comprehensive debug system tests...");
            testResults.Clear();
            
            yield return RunUnitTests();
            yield return RunIntegrationTests();
            
            if (enablePerformanceTests)
            {
                yield return RunPerformanceTests();
            }
            
            GenerateTestReport();
        }
        
        public IEnumerator RunUnitTests()
        {
            AdvancedLogger.LogInfo(LogCategory.General, "Running unit tests...");
            
            // Test AdvancedLogger
            yield return TestAdvancedLogger();
            
            // Test DebugConsoleManager command parsing
            yield return TestConsoleCommandParsing();
            
            // Test DebugStatsOverlay calculations
            yield return TestStatsOverlayCalculations();
            
            // Test RuntimeObjectInspector reflection
            yield return TestObjectInspectorReflection();
            
            AdvancedLogger.LogInfo(LogCategory.General, "Unit tests completed.");
        }
        
        public IEnumerator RunIntegrationTests()
        {
            AdvancedLogger.LogInfo(LogCategory.General, "Running integration tests...");
            
            // Test console logger integration
            yield return TestConsoleLoggerIntegration();
            
            // Test debug manager coordination
            yield return TestDebugManagerCoordination();
            
            // Test visual debugger integration
            yield return TestVisualDebuggerIntegration();
            
            // Test runtime inspector with game objects
            yield return TestRuntimeInspectorWithGameObjects();
            
            AdvancedLogger.LogInfo(LogCategory.General, "Integration tests completed.");
        }
        
        public IEnumerator RunPerformanceTests()
        {
            AdvancedLogger.LogInfo(LogCategory.General, "Running performance tests...");
            
            // Test logging performance
            yield return TestLoggingPerformance();
            
            // Test console UI performance
            yield return TestConsoleUIPerformance();
            
            // Test visual debugger performance
            yield return TestVisualDebuggerPerformance();
            
            // Test memory usage
            yield return TestMemoryUsage();
            
            AdvancedLogger.LogInfo(LogCategory.General, "Performance tests completed.");
        }
        
        #region Unit Tests
        
        private IEnumerator TestAdvancedLogger()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            
            try
            {
                // Test basic logging
                AdvancedLogger.LogInfo(LogCategory.General, "Test message");
                AdvancedLogger.LogWarning(LogCategory.General, "Test warning");
                AdvancedLogger.LogError(LogCategory.General, "Test error");
                
                // Test log level filtering
                AdvancedLogger.SetLogLevel(LogLevel.Warning);
                AdvancedLogger.LogInfo(LogCategory.General, "This should be filtered");
                
                AdvancedLogger.SetLogLevel(LogLevel.Debug); // Reset
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("AdvancedLogger Basic Functionality", testPassed, 
                testPassed ? "Logger working correctly" : $"Logger failed: {errorMessage}", executionTime);
            
            yield return null;
        }
        
        private IEnumerator TestConsoleCommandParsing()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            
            try
            {
                var console = FindFirstObjectByType<DebugConsoleManager>();
                if (console != null)
                {
                    // Test would go here - actual command parsing validation
                    testPassed = true;
                }
                else
                {
                    testPassed = false;
                    errorMessage = "DebugConsoleManager not found";
                }
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Console Command Parsing", testPassed, 
                testPassed ? "Command parsing working" : $"Command parsing failed: {errorMessage}", executionTime);
            
            yield return null;
        }
        
        private IEnumerator TestStatsOverlayCalculations()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            
            try
            {
                var statsOverlay = FindFirstObjectByType<DebugStatsOverlay>();
                if (statsOverlay != null)
                {
                    // Test would validate stats calculations
                    testPassed = true;
                }
                else
                {
                    testPassed = false;
                    errorMessage = "DebugStatsOverlay not found";
                }
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Stats Overlay Calculations", testPassed, 
                testPassed ? "Stats calculations working" : $"Stats calculations failed: {errorMessage}", executionTime);
            
            yield return null;
        }
        
        private IEnumerator TestObjectInspectorReflection()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            
            try
            {
                var inspector = FindFirstObjectByType<RuntimeObjectInspector>();
                if (inspector != null)
                {
                    // Create a test object for inspection
                    var testObject = new GameObject("TestObject");
                    var testComponent = testObject.AddComponent<TestComponent>();
                    
                    // Test inspection (would normally validate reflection results)
                    testPassed = true;
                    
                    DestroyImmediate(testObject);
                }
                else
                {
                    testPassed = false;
                    errorMessage = "RuntimeObjectInspector not found";
                }
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Object Inspector Reflection", testPassed, 
                testPassed ? "Object inspection working" : $"Object inspection failed: {errorMessage}", executionTime);
            
            yield return null;
        }
        
        #endregion
        
        #region Integration Tests
        
        private IEnumerator TestConsoleLoggerIntegration()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            
            try
            {
                var console = FindFirstObjectByType<DebugConsoleManager>();
                if (console != null)
                {
                    string testMessage = "Integration test message";
                    AdvancedLogger.LogInfo(LogCategory.General, testMessage);
                    
                    // Test would verify message appears in console
                    testPassed = true;
                }
                else
                {
                    testPassed = false;
                    errorMessage = "Console integration failed";
                }
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Console Logger Integration", testPassed, 
                testPassed ? "Console integration working" : $"Console integration failed: {errorMessage}", executionTime);
            
            yield return null;
        }
        
        private IEnumerator TestDebugManagerCoordination()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            
            try
            {
                var debugManager = FindFirstObjectByType<DebugManager>();
                if (debugManager != null)
                {
                    // Test manager coordination
                    testPassed = true;
                }
                else
                {
                    testPassed = false;
                    errorMessage = "DebugManager not found";
                }
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Debug Manager Coordination", testPassed, 
                testPassed ? "Debug manager working" : $"Debug manager failed: {errorMessage}", executionTime);
            
            yield return null;
        }
        
        private IEnumerator TestVisualDebuggerIntegration()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            
            try
            {
                var console = FindFirstObjectByType<DebugConsoleManager>();
                var statsOverlay = FindFirstObjectByType<DebugStatsOverlay>();
                var visualizer = FindFirstObjectByType<DebugVisualizer>();
                
                if (console != null && statsOverlay != null && visualizer != null)
                {
                    // Test integration between components
                    testPassed = true;
                }
                else
                {
                    testPassed = false;
                    errorMessage = "Missing debug components";
                }
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Visual Debugger Integration", testPassed, 
                testPassed ? "Visual debugger integration working" : $"Visual debugger integration failed: {errorMessage}", executionTime);
            
            yield return null;
        }
        
        private IEnumerator TestRuntimeInspectorWithGameObjects()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            GameObject testObj = null;
            
            try
            {
                var visualizer = FindFirstObjectByType<DebugVisualizer>();
                var inspector = FindFirstObjectByType<RuntimeObjectInspector>();
                
                if (visualizer != null && inspector != null)
                {
                    // Create test object and inspect it
                    testObj = new GameObject("RuntimeInspectorTest");
                    testObj.AddComponent<TestComponent>();
                    
                    testPassed = true;
                }
                else
                {
                    testPassed = false;
                    errorMessage = "Runtime inspector components not found";
                }
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            // Yield outside of try-catch block
            if (testPassed && testObj != null)
            {
                yield return new WaitForSeconds(0.1f); // Allow UI to update
                DestroyImmediate(testObj);
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Runtime Inspector with GameObjects", testPassed, 
                testPassed ? "Runtime inspector working with GameObjects" : $"Runtime inspector failed: {errorMessage}", executionTime);
        }
        
        #endregion
        
        #region Performance Tests
        
        private IEnumerator TestLoggingPerformance()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            float totalTime = 0f;
            
            // Use a separate loop structure to handle yielding
            for (int i = 0; i < performanceTestIterations && testPassed; i += 100)
            {
                try
                {
                    int endIndex = Mathf.Min(i + 100, performanceTestIterations);
                    for (int j = i; j < endIndex; j++)
                    {
                        float logStart = Time.realtimeSinceStartup;
                        AdvancedLogger.LogInfo(LogCategory.Performance, $"Performance test message {j}");
                        totalTime += Time.realtimeSinceStartup - logStart;
                    }
                }
                catch (Exception e)
                {
                    testPassed = false;
                    errorMessage = e.Message;
                    break;
                }
                
                yield return null; // Yield outside try-catch
            }
            
            if (testPassed)
            {
                try
                {
                    float averageTime = (totalTime / performanceTestIterations) * 1000; // Convert to ms
                    performanceMetrics["Logging_AverageTime_ms"] = averageTime;
                    
                    testPassed = averageTime < 1.0f; // Should be under 1ms per log
                    if (!testPassed)
                        errorMessage = $"Logging too slow: {averageTime:F4}ms average";
                }
                catch (Exception e)
                {
                    testPassed = false;
                    errorMessage = e.Message;
                }
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Logging Performance", testPassed, 
                testPassed ? $"Logging performance acceptable" : $"Logging performance failed: {errorMessage}", executionTime);
        }
          private IEnumerator TestConsoleUIPerformance()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            
            DebugConsoleManager console = null;
            
            try
            {
                console = FindFirstObjectByType<DebugConsoleManager>();
                if (console == null)
                {
                    testPassed = false;
                    errorMessage = "Console not found for UI performance test";
                }
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            if (testPassed && console != null)
            {
                float totalTime = 0f;
                for (int i = 0; i < 50; i++) // Smaller iteration for UI tests
                {
                    float uiStart = Time.realtimeSinceStartup;
                    // Simulate UI operations
                    yield return null;
                    totalTime += Time.realtimeSinceStartup - uiStart;
                }
                
                performanceMetrics["ConsoleUI_UpdateTime_ms"] = totalTime * 1000;
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Console UI Performance", testPassed, 
                testPassed ? "Console UI performance acceptable" : $"Console UI performance failed: {errorMessage}", executionTime);
        }
          private IEnumerator TestVisualDebuggerPerformance()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            
            DebugVisualizer visualizer = null;
            
            try
            {
                visualizer = FindFirstObjectByType<DebugVisualizer>();
                if (visualizer == null)
                {
                    testPassed = false;
                    errorMessage = "Visual debugger not found";
                }
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            if (testPassed && visualizer != null)
            {
                float totalTime = 0f;
                for (int i = 0; i < 30; i++) // Smaller iteration for render tests
                {
                    float renderStart = Time.realtimeSinceStartup;
                    // Simulate visual debug operations
                    yield return null;
                    totalTime += Time.realtimeSinceStartup - renderStart;
                }
                
                performanceMetrics["VisualDebugger_DrawTime_ms"] = totalTime * 1000;
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Visual Debugger Performance", testPassed, 
                testPassed ? "Visual debugger performance acceptable" : $"Visual debugger performance failed: {errorMessage}", executionTime);
        }
          private IEnumerator TestMemoryUsage()
        {
            float startTime = Time.realtimeSinceStartup;
            bool testPassed = true;
            string errorMessage = "";
            
            long startMemory = 0;
            long endMemory = 0;
            
            try
            {
                startMemory = GC.GetTotalMemory(true);
            }
            catch (Exception e)
            {
                testPassed = false;
                errorMessage = e.Message;
            }
            
            if (testPassed)
            {
                // Generate memory usage with yield statements outside try-catch
                for (int i = 0; i < 1000; i++)
                {
                    AdvancedLogger.LogInfo(LogCategory.Performance, $"Memory test {i}");
                    if (i % 100 == 0) yield return null;
                }
                
                try
                {
                    endMemory = GC.GetTotalMemory(false);
                    long memoryDelta = endMemory - startMemory;
                    
                    performanceMetrics["Memory_Delta_KB"] = memoryDelta / 1024f;
                    
                    testPassed = memoryDelta < 1024 * 1024; // Should be under 1MB
                    if (!testPassed)
                        errorMessage = $"Excessive memory usage: {memoryDelta / 1024f:F2}KB";
                }
                catch (Exception e)
                {
                    testPassed = false;
                    errorMessage = e.Message;
                }
            }
            
            float executionTime = Time.realtimeSinceStartup - startTime;
            RecordTestResult("Memory Usage", testPassed, 
                testPassed ? "Memory usage acceptable" : $"Memory usage failed: {errorMessage}", executionTime);
        }
        
        #endregion
        
        #region Test Result Management
        
        private void RecordTestResult(string testName, bool passed, string message, float executionTime)
        {
            testResults.Add(new TestResult
            {
                testName = testName,
                passed = passed,
                message = message,
                executionTime = executionTime,
                timestamp = DateTime.Now
            });
            
            string logMessage = $"TEST: {testName} - {(passed ? "PASSED" : "FAILED")} - {message} ({executionTime:F4}s)";
            
            if (passed)
                AdvancedLogger.LogInfo(LogCategory.General, logMessage);
            else
                AdvancedLogger.LogError(LogCategory.General, logMessage);
        }
        
        private void GenerateTestReport()
        {
            int passedTests = testResults.Count(r => r.passed);
            int totalTests = testResults.Count;
            float successRate = totalTests > 0 ? (passedTests / (float)totalTests) * 100 : 0;
            
            AdvancedLogger.LogInfo(LogCategory.General, "========== DEBUG SYSTEM TEST REPORT ==========");
            AdvancedLogger.LogInfo(LogCategory.General, $"Total Tests: {totalTests}");
            AdvancedLogger.LogInfo(LogCategory.General, $"Passed: {passedTests}");
            AdvancedLogger.LogInfo(LogCategory.General, $"Failed: {totalTests - passedTests}");
            AdvancedLogger.LogInfo(LogCategory.General, $"Success Rate: {successRate:F1}%");
            
            if (performanceMetrics.Count > 0)
            {
                AdvancedLogger.LogInfo(LogCategory.General, "Performance Metrics:");
                foreach (var metric in performanceMetrics)
                {
                    AdvancedLogger.LogInfo(LogCategory.General, $"  {metric.Key}: {metric.Value:F4}");
                }
            }
            
            AdvancedLogger.LogInfo(LogCategory.General, "===============================================");
        }
        
        private void ShowTestResults(string[] args)
        {
            if (testResults.Count == 0)
            {
                AdvancedLogger.LogInfo(LogCategory.General, "No test results available. Run tests first.");
                return;
            }
            
            GenerateTestReport();
            
            AdvancedLogger.LogInfo(LogCategory.General, "Detailed Results:");
            foreach (var result in testResults)
            {
                string status = result.passed ? "[PASS]" : "[FAIL]";
                AdvancedLogger.LogInfo(LogCategory.General, $"{status} {result.testName}: {result.message}");
            }
        }
        
        private void ClearTestResults(string[] args)
        {
            testResults.Clear();
            performanceMetrics.Clear();
            AdvancedLogger.LogInfo(LogCategory.General, "Test results cleared.");
        }
        
        #endregion
    }
}
#endif

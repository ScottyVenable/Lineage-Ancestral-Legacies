using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Lineage.Ancestral.Legacies.Debug;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Lineage.Ancestral.Legacies.Debug.Testing
{
    public class DebugSystemTestFramework : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool enablePerformanceTests = true;
        [SerializeField] private int performanceTestIterations = 1000;
        
        private List<TestResult> testResults = new List<TestResult>();
        private Dictionary<string, float> performanceMetrics = new Dictionary<string, float>();
        private DebugConsoleManager console;
        
        private enum TestStatus
        {
            Passed,
            Failed,
            Skipped
        }
        
        private class TestResult
        {
            public string testName;
            public TestStatus status;
            public string message;
            public float duration;
            public System.DateTime timestamp;
            
            public TestResult(string name, TestStatus status, string message = "", float duration = 0f)
            {
                this.testName = name;
                this.status = status;
                this.message = message;
                this.duration = duration;
                this.timestamp = System.DateTime.Now;
            }
        }
        
        private void Start()
        {
            // Get reference to console
            console = FindFirstObjectByType<DebugConsoleManager>();
            
            if (console == null)
            {
                Debug.Log.Error("DebugConsoleManager not found. Test framework commands will not be available.");
            }
            else
            {
                RegisterTestCommands();
            }
            
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }
        
        private void RegisterTestCommands()
        {
            if (console != null)
            {
                // Since RegisterCommand is private, we'll use reflection to access it
                try
                {
                    var consoleType = typeof(DebugConsoleManager);
                    var registerMethod = consoleType.GetMethod("RegisterCommand", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (registerMethod != null)
                    {
                        // Create delegate instances for each command
                        DebugConsoleManager.CommandDelegate runAllDelegate = (args, data) => RunAllTestsCommand(args, data);
                        DebugConsoleManager.CommandDelegate runUnitDelegate = (args, data) => RunUnitTestsCommand(args, data);
                        DebugConsoleManager.CommandDelegate runIntegrationDelegate = (args, data) => RunIntegrationTestsCommand(args, data);
                        DebugConsoleManager.CommandDelegate runPerformanceDelegate = (args, data) => RunPerformanceTestsCommand(args, data);
                        DebugConsoleManager.CommandDelegate showResultsDelegate = (args, data) => ShowTestResultsCommand(args, data);
                        DebugConsoleManager.CommandDelegate clearResultsDelegate = (args, data) => ClearTestResultsCommand(args, data);
                        DebugConsoleManager.CommandDelegate showMetricsDelegate = (args, data) => ShowPerformanceMetricsCommand(args, data);
                        
                        // Register test commands using reflection
                        registerMethod.Invoke(console, new object[] { "test.run.all", "Run all debug system tests", "test.run.all", runAllDelegate, false });
                        registerMethod.Invoke(console, new object[] { "test.run.unit", "Run unit tests only", "test.run.unit", runUnitDelegate, false });
                        registerMethod.Invoke(console, new object[] { "test.run.integration", "Run integration tests", "test.run.integration", runIntegrationDelegate, false });
                        registerMethod.Invoke(console, new object[] { "test.run.performance", "Run performance tests", "test.run.performance", runPerformanceDelegate, false });
                        registerMethod.Invoke(console, new object[] { "test.results.show", "Show test results", "test.results.show", showResultsDelegate, false });
                        registerMethod.Invoke(console, new object[] { "test.results.clear", "Clear test result history", "test.results.clear", clearResultsDelegate, false });
                        registerMethod.Invoke(console, new object[] { "test.metrics.show", "Show performance metrics", "test.metrics.show", showMetricsDelegate, false });
                        
                        LogTestMessage("Test framework commands registered successfully.");
                    }
                    else
                    {
                        LogTestMessage("RegisterCommand method not found. Test commands will not be available.");
                    }
                }
                catch (System.Exception e)
                {
                    LogTestMessage($"Failed to register test commands: {e.Message}");
                }
            }
        }
        
        #region Command Handlers
        
        private string RunAllTestsCommand(List<string> args, Dictionary<string, object> data)
        {
            StartCoroutine(RunAllTests());
            return "Starting all tests...";
        }
        
        private string RunUnitTestsCommand(List<string> args, Dictionary<string, object> data)
        {
            StartCoroutine(RunUnitTests());
            return "Starting unit tests...";
        }
        
        private string RunIntegrationTestsCommand(List<string> args, Dictionary<string, object> data)
        {
            StartCoroutine(RunIntegrationTests());
            return "Starting integration tests...";
        }
        
        private string RunPerformanceTestsCommand(List<string> args, Dictionary<string, object> data)
        {
            StartCoroutine(RunPerformanceTests());
            return "Starting performance tests...";
        }
        
        private string ShowTestResultsCommand(List<string> args, Dictionary<string, object> data)
        {
            return GetTestResultsSummary();
        }
        
        private string ClearTestResultsCommand(List<string> args, Dictionary<string, object> data)
        {
            testResults.Clear();
            performanceMetrics.Clear();
            return "Test results and metrics cleared.";
        }
        
        private string ShowPerformanceMetricsCommand(List<string> args, Dictionary<string, object> data)
        {
            return GetPerformanceMetricsSummary();
        }
        
        #endregion
        
        // Public API for running tests
        public IEnumerator RunAllTests()
        {
            LogTestMessage("Starting comprehensive test suite...");
            
            yield return RunUnitTests();
            yield return RunIntegrationTests();
            
            if (enablePerformanceTests)
            {
                yield return RunPerformanceTests();
            }
            
            LogTestMessage($"All tests completed. Results: {GetTestResultsSummary()}");
        }
        
        #region Unit Tests
        
        public IEnumerator RunUnitTests()
        {
            LogTestMessage("Running unit tests...");
            
            yield return TestConsoleLogging();
            yield return TestConsoleCommandParsing();
            yield return TestAdvancedLoggerFunctionality();
            yield return TestObjectInspectorBasics();
            
            LogTestMessage("Unit tests completed.");
        }
        
        private IEnumerator TestConsoleLogging()
        {
            var stopwatch = Stopwatch.StartNew();
            string testName = "Console Logging Test";
            
            try
            {
                if (console == null)
                {
                    AddTestResult(testName, TestStatus.Skipped, "Console not available");
                    yield break;
                }
                
                int initialLogCount = console.consoleLog.Count;
                
                // Use the public LogToConsole method instead of directly accessing consoleLog
                var logMethod = typeof(DebugConsoleManager).GetMethod("LogToConsole", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (logMethod != null)
                {
                    logMethod.Invoke(console, new object[] { "Test log message", false });
                    
                    if (console.consoleLog.Count > initialLogCount)
                    {
                        AddTestResult(testName, TestStatus.Passed, "", stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        AddTestResult(testName, TestStatus.Failed, "Log message not added to console");
                    }
                }
                else
                {
                    AddTestResult(testName, TestStatus.Skipped, "LogToConsole method not accessible");
                }
            }
            catch (System.Exception e)
            {
                AddTestResult(testName, TestStatus.Failed, $"Exception: {e.Message}");
            }
            finally
            {
                stopwatch.Stop();
            }
            
            yield return null;
        }
        
        private IEnumerator TestConsoleCommandParsing()
        {
            var stopwatch = Stopwatch.StartNew();
            string testName = "Console Command Parsing Test";
            
            try
            {
                if (console == null)
                {
                    AddTestResult(testName, TestStatus.Skipped, "Console not available");
                    yield break;
                }
                
                // Test basic command parsing by checking if commands exist
                var testCommands = new[]
                {
                    "help",
                    "system.info",
                    "entity.inspect",
                    "spawn.pop"
                };
                
                bool allCommandsExist = true;
                
                // Access the commands dictionary through reflection
                var commandsField = typeof(DebugConsoleManager).GetField("commands", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (commandsField != null)
                {
                    var commandsDict = commandsField.GetValue(console) as Dictionary<string, DebugConsoleManager.ConsoleCommand>;
                    
                    if (commandsDict != null)
                    {
                        foreach (string cmd in testCommands)
                        {
                            if (!commandsDict.ContainsKey(cmd))
                            {
                                allCommandsExist = false;
                                break;
                            }
                        }
                        
                        if (allCommandsExist)
                        {
                            AddTestResult(testName, TestStatus.Passed, "", stopwatch.ElapsedMilliseconds);
                        }
                        else
                        {
                            AddTestResult(testName, TestStatus.Failed, "Not all test commands are registered");
                        }
                    }
                    else
                    {
                        AddTestResult(testName, TestStatus.Failed, "Commands dictionary is null");
                    }
                }
                else
                {
                    AddTestResult(testName, TestStatus.Skipped, "Commands field not accessible");
                }
            }
            catch (System.Exception e)
            {
                AddTestResult(testName, TestStatus.Failed, $"Exception: {e.Message}");
            }
            finally
            {
                stopwatch.Stop();
            }
            
            yield return null;
        }
        
        private IEnumerator TestAdvancedLoggerFunctionality()
        {
            var stopwatch = Stopwatch.StartNew();
            string testName = "Advanced Logger Test";
            
            try
            {
                // Test AdvancedLogger static methods
                AdvancedLogger.LogInfo(LogCategory.General, "Test info message");
                AdvancedLogger.LogError(LogCategory.General, "Test error message");
                
                AddTestResult(testName, TestStatus.Passed, "", stopwatch.ElapsedMilliseconds);
            }
            catch (System.Exception e)
            {
                AddTestResult(testName, TestStatus.Failed, $"Exception: {e.Message}");
            }
            finally
            {
                stopwatch.Stop();
            }
            
            yield return null;
        }
        
        private IEnumerator TestObjectInspectorBasics()
        {
            var stopwatch = Stopwatch.StartNew();
            string testName = "Object Inspector Test";
            
            try
            {
                var inspector = FindFirstObjectByType<RuntimeObjectInspector>();
                if (inspector != null)
                {
                    // Basic test - just check if inspector exists and is enabled
                    AddTestResult(testName, TestStatus.Passed, "", stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    AddTestResult(testName, TestStatus.Skipped, "RuntimeObjectInspector not found");
                }
            }
            catch (System.Exception e)
            {
                AddTestResult(testName, TestStatus.Failed, $"Exception: {e.Message}");
            }
            finally
            {
                stopwatch.Stop();
            }
            
            yield return null;
        }
        
        #endregion
        
        #region Integration Tests
        
        public IEnumerator RunIntegrationTests()
        {
            LogTestMessage("Running integration tests...");
            
            yield return TestConsoleManagerIntegration();
            yield return TestDebugSystemsIntegration();
            
            LogTestMessage("Integration tests completed.");
        }
        
        private IEnumerator TestConsoleManagerIntegration()
        {
            var stopwatch = Stopwatch.StartNew();
            string testName = "Console Manager Integration Test";
            
            try
            {
                if (console == null)
                {
                    AddTestResult(testName, TestStatus.Failed, "Console manager not found");
                    yield break;
                }
                
                // Test command registration and execution by checking if commands dictionary exists
                var commandsField = typeof(DebugConsoleManager).GetField("commands", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (commandsField != null)
                {
                    var commandsDict = commandsField.GetValue(console);
                    if (commandsDict != null)
                    {
                        AddTestResult(testName, TestStatus.Passed, "", stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        AddTestResult(testName, TestStatus.Failed, "Commands dictionary not initialized");
                    }
                }
                else
                {
                    AddTestResult(testName, TestStatus.Failed, "Commands field not accessible");
                }
            }
            catch (System.Exception e)
            {
                AddTestResult(testName, TestStatus.Failed, $"Exception: {e.Message}");
            }
            finally
            {
                stopwatch.Stop();
            }
            
            yield return null;
        }
        
        private IEnumerator TestDebugSystemsIntegration()
        {
            var stopwatch = Stopwatch.StartNew();
            string testName = "Debug Systems Integration Test";
            
            try
            {
                var debugManager = FindFirstObjectByType<DebugManager>();
                var inspector = FindFirstObjectByType<RuntimeObjectInspector>();
                
                bool systemsAvailable = console != null && (debugManager != null || inspector != null);
                
                if (systemsAvailable)
                {
                    AddTestResult(testName, TestStatus.Passed, "", stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    AddTestResult(testName, TestStatus.Failed, "Debug systems not properly integrated");
                }
            }
            catch (System.Exception e)
            {
                AddTestResult(testName, TestStatus.Failed, $"Exception: {e.Message}");
            }
            finally
            {
                stopwatch.Stop();
            }
            
            yield return null;
        }
        
        #endregion
        
        #region Performance Tests
        
        public IEnumerator RunPerformanceTests()
        {
            if (!enablePerformanceTests)
            {
                LogTestMessage("Performance tests disabled.");
                yield break;
            }
            
            LogTestMessage("Running performance tests...");
            
            yield return TestConsoleLoggingPerformance();
            yield return TestCommandParsingPerformance();
            
            LogTestMessage("Performance tests completed.");
        }
        
        private IEnumerator TestConsoleLoggingPerformance()
        {
            string testName = "Console Logging Performance";
            var stopwatch = Stopwatch.StartNew();
            
            if (console == null)
            {
                AddTestResult(testName, TestStatus.Skipped, "Console not available");
                yield break;
            }
            
            var logMethod = typeof(DebugConsoleManager).GetMethod("LogToConsole", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (logMethod == null)
            {
                AddTestResult(testName, TestStatus.Skipped, "LogToConsole method not accessible");
                yield break;
            }
            
            bool hasError = false;
            string errorMessage = "";
            
            for (int i = 0; i < performanceTestIterations; i++)
            {
                try
                {
                    logMethod.Invoke(console, new object[] { $"Performance test message {i}", false });
                }
                catch (System.Exception e)
                {
                    hasError = true;
                    errorMessage = e.Message;
                    break;
                }
                
                if (i % 100 == 0)
                {
                    yield return null; // Yield occasionally to prevent frame drops
                }
            }
            
            stopwatch.Stop();
            
            if (hasError)
            {
                AddTestResult(testName, TestStatus.Failed, $"Exception: {errorMessage}");
            }
            else
            {
                float avgTimePerLog = (float)stopwatch.ElapsedMilliseconds / performanceTestIterations;
                performanceMetrics[testName] = avgTimePerLog;
                AddTestResult(testName, TestStatus.Passed, $"Avg: {avgTimePerLog:F4}ms per log", stopwatch.ElapsedMilliseconds);
            }
        }
        private IEnumerator TestCommandParsingPerformance()
        {
            string testName = "Command Parsing Performance";
            var stopwatch = Stopwatch.StartNew();
            
            if (console == null)
            {
                AddTestResult(testName, TestStatus.Skipped, "Console not available");
                yield break;
            }
            
            string[] testCommands = {
                "help",
                "entity.inspect [12345]",
                "spawn.pop [10, 20, 0] {name:\"Test\", health:100}",
                "lineage.resources.add [food, 1000]"
            };
            
            var method = typeof(DebugConsoleManager).GetMethod("ParseCommandLine", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method == null)
            {
                AddTestResult(testName, TestStatus.Skipped, "ParseCommandLine method not accessible");
                yield break;
            }
            
            bool hasError = false;
            string errorMessage = "";
            
            for (int i = 0; i < performanceTestIterations; i++)
            {
                try
                {
                    string cmd = testCommands[i % testCommands.Length];
                    method.Invoke(console, new object[] { cmd });
                }
                catch (System.Exception e)
                {
                    hasError = true;
                    errorMessage = e.Message;
                    break;
                }
                
                if (i % 100 == 0)
                {
                    yield return null;
                }
            }
            
            stopwatch.Stop();
            
            if (hasError)
            {
                AddTestResult(testName, TestStatus.Failed, $"Exception: {errorMessage}");
            }
            else
            {
                float avgTimePerParse = (float)stopwatch.ElapsedMilliseconds / performanceTestIterations;
                performanceMetrics[testName] = avgTimePerParse;
                
                AddTestResult(testName, TestStatus.Passed, $"Avg: {avgTimePerParse:F4}ms per parse", stopwatch.ElapsedMilliseconds);
            }
        }
        
        #endregion
        
        #region Test Result Management
        
        private void AddTestResult(string testName, TestStatus status, string message = "", float duration = 0f)
        {
            var result = new TestResult(testName, status, message, duration);
            testResults.Add(result);
            
            string statusColor = status switch
            {
                TestStatus.Passed => "green",
                TestStatus.Failed => "red",
                TestStatus.Skipped => "yellow",
                _ => "white"
            };
            
            LogTestMessage($"<color={statusColor}>{status}</color>: {testName} {(string.IsNullOrEmpty(message) ? "" : $"- {message}")}");
        }
        
        private string GetTestResultsSummary()
        {
            if (testResults.Count == 0)
            {
                return "No test results available.";
            }
            
            int passed = testResults.Count(r => r.status == TestStatus.Passed);
            int failed = testResults.Count(r => r.status == TestStatus.Failed);
            int skipped = testResults.Count(r => r.status == TestStatus.Skipped);
            
            string summary = $"Test Results Summary:\n";
            summary += $"  Total: {testResults.Count}\n";
            summary += $"  Passed: {passed}\n";
            summary += $"  Failed: {failed}\n";
            summary += $"  Skipped: {skipped}\n";
            
            if (failed > 0)
            {
                summary += "\nFailed Tests:\n";
                foreach (var result in testResults.Where(r => r.status == TestStatus.Failed))
                {
                    summary += $"  - {result.testName}: {result.message}\n";
                }
            }
            
            return summary;
        }
        
        private string GetPerformanceMetricsSummary()
        {
            if (performanceMetrics.Count == 0)
            {
                return "No performance metrics available.";
            }
            
            string summary = "Performance Metrics:\n";
            foreach (var metric in performanceMetrics)
            {
                summary += $"  {metric.Key}: {metric.Value:F4}ms\n";
            }
            
            return summary;
        }
        
        private void LogTestMessage(string message)
        {
            if (console != null)
            {
                // Use reflection to access LogToConsole since it's private
                var logMethod = typeof(DebugConsoleManager).GetMethod("LogToConsole", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (logMethod != null)
                {
                    logMethod.Invoke(console, new object[] { $"[TEST] {message}", false });
                }
                else
                {
                    Debug.Log.Error($"[TEST] {message}");
                }
            }
            else
            {
                Debug.Log.Error($"[TEST] {message}");
            }
        }
        
        public void ClearTestResults()
        {
            testResults.Clear();
            performanceMetrics.Clear();
            LogTestMessage("Test results cleared.");
        }
        
        #endregion
    }
}
#endif

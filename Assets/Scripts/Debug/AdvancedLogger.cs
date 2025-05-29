using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Lineage.Ancestral.Legacies.Debug
{
    /// <summary>
    /// Log levels for the debug system
    /// </summary>
    public enum LogLevel
    {
        Verbose = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }

    /// <summary>
    /// Log categories for filtering
    /// </summary>
    public enum LogCategory
    {
        General,
        AI,
        Inventory,
        Combat,
        UI,
        Audio,
        Resource,
        Population,
        Camera,
        Save,
        Performance
    }

    /// <summary>
    /// Advanced logging system with levels, categories, and multiple output targets
    /// </summary>
    public static class AdvancedLogger
    {
        private static LogLevel currentLogLevel = LogLevel.Debug;
        private static readonly Dictionary<LogCategory, bool> categoryFilters = new Dictionary<LogCategory, bool>();
        private static StreamWriter logFileWriter;
        private static readonly List<string> logBuffer = new List<string>();
        private static readonly int maxBufferSize = 1000;

        static AdvancedLogger()
        {
            // Initialize all categories as enabled
            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                categoryFilters[category] = true;
            }

            InitializeLogFile();
        }

        private static void InitializeLogFile()
        {
            try
            {
                string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string logFileName = $"LineageDebug_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
                string logPath = Path.Combine(logDirectory, logFileName);
                
                logFileWriter = new StreamWriter(logPath, true);
                logFileWriter.WriteLine($"=== Lineage Ancestral Legacies Debug Log Started ===");
                logFileWriter.WriteLine($"Date: {DateTime.Now}");
                logFileWriter.WriteLine($"Unity Version: {Application.unityVersion}");
                logFileWriter.WriteLine($"Platform: {Application.platform}");
                logFileWriter.WriteLine($"Device: {SystemInfo.deviceName}");
                logFileWriter.WriteLine($"OS: {SystemInfo.operatingSystem}");
                logFileWriter.WriteLine($"Memory: {SystemInfo.systemMemorySize}MB");
                logFileWriter.WriteLine("=============================================");
                logFileWriter.Flush();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to initialize log file: {e.Message}");
            }
        }

        public static void SetLogLevel(LogLevel level)
        {
            currentLogLevel = level;
            Log(LogLevel.Info, LogCategory.General, $"Log level set to: {level}");
        }

        public static void SetCategoryFilter(LogCategory category, bool enabled)
        {
            categoryFilters[category] = enabled;
            Log(LogLevel.Info, LogCategory.General, $"Category '{category}' filter set to: {enabled}");
        }

        public static void Log(LogLevel level, LogCategory category, string message)
        {
            if (level < currentLogLevel || !categoryFilters[category])
                return;

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string frameCount = Time.frameCount.ToString();
            string formattedMessage = $"[{timestamp}][Frame:{frameCount}][{level}][{category}] {message}";

            // Add to buffer
            logBuffer.Add(formattedMessage);
            if (logBuffer.Count > maxBufferSize)
            {
                logBuffer.RemoveAt(0);
            }

            // Output to Unity Console
            switch (level)
            {
                case LogLevel.Verbose:
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
            }

            // Output to file
            WriteToFile(formattedMessage);

            // Send to debug console if available
            NotifyDebugConsole(level, category, formattedMessage);
        }

        private static void WriteToFile(string message)
        {
            try
            {
                logFileWriter?.WriteLine(message);
                logFileWriter?.Flush();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to write to log file: {e.Message}");
            }
        }

        private static void NotifyDebugConsole(LogLevel level, LogCategory category, string message)
        {
            var console = UnityEngine.Object.FindFirstObjectByType<DebugConsoleManager>();
            if (console != null)
            {
                string coloredMessage = GetColoredMessage(level, message);
                console.AppendLogMessage(coloredMessage);
            }
        }

        private static string GetColoredMessage(LogLevel level, string message)
        {
            string color = level switch
            {
                LogLevel.Verbose => "gray",
                LogLevel.Debug => "white",
                LogLevel.Info => "cyan",
                LogLevel.Warning => "yellow",
                LogLevel.Error => "red",
                LogLevel.Critical => "magenta",
                _ => "white"
            };

            return $"<color={color}>{message}</color>";
        }

        // Convenience methods
        public static void LogVerbose(LogCategory category, string message) => Log(LogLevel.Verbose, category, message);
        public static void LogDebug(LogCategory category, string message) => Log(LogLevel.Debug, category, message);
        public static void LogInfo(LogCategory category, string message) => Log(LogLevel.Info, category, message);
        public static void LogWarning(LogCategory category, string message) => Log(LogLevel.Warning, category, message);
        public static void LogError(LogCategory category, string message) => Log(LogLevel.Error, category, message);
        public static void LogCritical(LogCategory category, string message) => Log(LogLevel.Critical, category, message);

        public static List<string> GetLogBuffer() => new List<string>(logBuffer);

        public static void Shutdown()
        {
            try
            {
                logFileWriter?.WriteLine("=== Log session ended ===");
                logFileWriter?.Close();
                logFileWriter = null;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error shutting down logger: {e.Message}");
            }
        }
    }
}
#endif

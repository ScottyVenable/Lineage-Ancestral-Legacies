#if DEVELOPMENT_BUILD || UNITY_EDITOR
using UnityEngine;
using System;
using System.IO;

namespace Lineage.Debug
{
    /// <summary>
    /// Advanced logging system for Lineage Ancestral Legacies with levels, categories, and multiple outputs.
    /// Only compiled in development builds and editor.
    /// </summary>
    public static class Log
    {
        public enum LogLevel
        {
            Verbose = 0,
            Debug = 1,
            Info = 2,
            Warning = 3,
            Error = 4,
            Critical = 5
        }

        public enum LogCategory
        {
            General,
            AI,
            Inventory,
            Quest,
            Combat,
            Population,
            Needs,
            Systems,
            UI,
            Performance
        }

        // Settings
        public static LogLevel MinLogLevel = LogLevel.Debug;
        public static bool EnableUnityConsole = true;
        public static bool EnableInGameConsole = false;
        public static bool EnableFileLogging = false;
        public static bool IncludeTimestamp = true;
        public static bool IncludeFrameNumber = true;

        private const string LOG_PREFIX = "[LAL]";
        private static string logFilePath;
        private static bool isInitialized = false;

        static Log()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (isInitialized) return;

            // Setup log file path
            if (EnableFileLogging)
            {
                string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                logFilePath = Path.Combine(logDirectory, $"LAL_Log_{timestamp}.txt");
                
                WriteToFile($"=== Lineage Ancestral Legacies Debug Log Started ===");
                WriteToFile($"Time: {DateTime.Now}");
                WriteToFile($"Unity Version: {Application.unityVersion}");
                WriteToFile($"Platform: {Application.platform}");
                WriteToFile($"Build: {(UnityEngine.Debug.isDebugBuild ? "Debug" : "Release")}");
                WriteToFile("======================================================");
            }

            isInitialized = true;
        }

        // Core logging method
        private static void WriteLog(LogLevel level, LogCategory category, object message, UnityEngine.Object context = null)
        {
            if (level < MinLogLevel) return;

            string formattedMessage = FormatMessage(level, category, message);

            // Output to Unity Console
            if (EnableUnityConsole)
            {
                switch (level)
                {
                    case LogLevel.Warning:
                        UnityEngine.Debug.LogWarning(formattedMessage, context);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        UnityEngine.Debug.LogError(formattedMessage, context);
                        break;
                    default:
                        UnityEngine.Debug.Log(formattedMessage, context);
                        break;
                }
            }

            // Output to file
            if (EnableFileLogging)
            {
                WriteToFile(formattedMessage);
            }

            // TODO: Output to in-game console when implemented
            if (EnableInGameConsole)
            {
                // InGameConsole.AddMessage(formattedMessage, level, category);
            }
        }

        private static string FormatMessage(LogLevel level, LogCategory category, object message)
        {
            string timestamp = IncludeTimestamp ? $"[{DateTime.Now:HH:mm:ss.fff}]" : "";
            string frameNumber = IncludeFrameNumber ? $"[F:{Time.frameCount}]" : "";
            string levelStr = $"[{level.ToString().ToUpper()}]";
            string categoryStr = $"[{category}]";
            
            return $"{LOG_PREFIX}{timestamp}{frameNumber}{levelStr}{categoryStr} {message}";
        }

        private static void WriteToFile(string message)
        {
            if (string.IsNullOrEmpty(logFilePath)) return;

            try
            {
                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to write to log file: {e.Message}");
            }
        }

        // Public API - General Logging
        public static void Verbose(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            WriteLog(LogLevel.Verbose, category, message, context);
        }

        public static void Debug(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            WriteLog(LogLevel.Debug, category, message, context);
        }

        public static void Info(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            WriteLog(LogLevel.Info, category, message, context);
        }

        public static void Warning(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            WriteLog(LogLevel.Warning, category, message, context);
        }

        public static void Error(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            WriteLog(LogLevel.Error, category, message, context);
        }

        public static void Critical(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            WriteLog(LogLevel.Critical, category, message, context);
        }

        // Specialized Logging Methods
        public static void Pop(string popName, string message, LogLevel level = LogLevel.Debug)
        {
            WriteLog(level, LogCategory.Population, $"POP[{popName}]: {message}");
        }

        public static void System(string systemName, string message, LogLevel level = LogLevel.Debug)
        {
            WriteLog(level, LogCategory.Systems, $"SYSTEM[{systemName}]: {message}");
        }

        public static void AI(string entityName, string message, LogLevel level = LogLevel.Debug)
        {
            WriteLog(level, LogCategory.AI, $"AI[{entityName}]: {message}");
        }

        public static void Quest(string questName, string message, LogLevel level = LogLevel.Info)
        {
            WriteLog(level, LogCategory.Quest, $"QUEST[{questName}]: {message}");
        }

        public static void Combat(string message, LogLevel level = LogLevel.Debug)
        {
            WriteLog(level, LogCategory.Combat, message);
        }

        public static void Performance(string message, LogLevel level = LogLevel.Warning)
        {
            WriteLog(level, LogCategory.Performance, $"PERF: {message}");
        }

        // Configuration Methods
        public static void SetLogLevel(LogLevel level)
        {
            MinLogLevel = level;
            Info($"Log level set to: {level}", LogCategory.Systems);
        }

        public static void EnableFileOutput(bool enable)
        {
            EnableFileLogging = enable;
            if (enable && !isInitialized)
            {
                Initialize();
            }
            Info($"File logging {(enable ? "enabled" : "disabled")}", LogCategory.Systems);
        }

        public static string GetLogFilePath()
        {
            return logFilePath;
        }
    }
}
#else
// Fallback for release builds - empty static class to prevent compilation errors
namespace Lineage.Debug
{
    public static class Log
    {
        public static void Verbose(object message, object category = null, object context = null) { }
        public static void Debug(object message, object category = null, object context = null) { }
        public static void Info(object message, object category = null, object context = null) { }
        public static void Warning(object message, object category = null, object context = null) { }
        public static void Error(object message, object category = null, object context = null) { }
        public static void Critical(object message, object category = null, object context = null) { }
        public static void Pop(string popName, string message, object level = null) { }
        public static void System(string systemName, string message, object level = null) { }
        public static void AI(string entityName, string message, object level = null) { }
        public static void Quest(string questName, string message, object level = null) { }
        public static void Combat(string message, object level = null) { }
        public static void Performance(string message, object level = null) { }
    }
}
#endif

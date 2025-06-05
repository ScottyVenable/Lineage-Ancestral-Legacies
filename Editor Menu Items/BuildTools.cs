using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Diagnostics;

namespace Lineage.Editor
{
    /// <summary>
    /// Build automation and project management tools
    /// </summary>
    public static class BuildTools
    {
        [MenuItem("Lineage/Build Tools/Quick Development Build", false, 600)]
        public static void QuickDevelopmentBuild()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenePaths();
            buildPlayerOptions.locationPathName = "Builds/Development/LineageGame.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.Development | BuildOptions.AllowDebugging;
            
            Directory.CreateDirectory("Builds/Development");
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log($"[Build Tools] Development build succeeded: {summary.outputPath}");
                EditorUtility.RevealInFinder(summary.outputPath);
            }
            else
            {
                UnityEngine.Debug.LogError($"[Build Tools] Build failed: {summary.result}");
            }
        }
        
        [MenuItem("Lineage/Build Tools/Release Build", false, 601)]
        public static void ReleaseBuild()
        {
            if (!EditorUtility.DisplayDialog("Release Build", 
                "This will create a release build. Continue?", "Yes", "Cancel"))
                return;
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenePaths();
            buildPlayerOptions.locationPathName = "Builds/Release/LineageGame.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.None;
            
            Directory.CreateDirectory("Builds/Release");
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log($"[Build Tools] Release build succeeded: {summary.outputPath}");
                EditorUtility.RevealInFinder(summary.outputPath);
            }
            else
            {
                UnityEngine.Debug.LogError($"[Build Tools] Build failed: {summary.result}");
            }
        }
        
        [MenuItem("Lineage/Build Tools/Clean Build Folders", false, 620)]
        public static void CleanBuildFolders()
        {
            if (Directory.Exists("Builds"))
            {
                if (EditorUtility.DisplayDialog("Clean Builds", 
                    "This will delete all build folders. Continue?", "Yes", "Cancel"))
                {
                    Directory.Delete("Builds", true);
                    UnityEngine.Debug.Log("[Build Tools] Cleaned all build folders");
                }
            }
            else
            {
                UnityEngine.Debug.Log("[Build Tools] No build folders to clean");
            }
        }
        
        [MenuItem("Lineage/Build Tools/Open Build Folder", false, 640)]
        public static void OpenBuildFolder()
        {
            if (Directory.Exists("Builds"))
            {
                EditorUtility.RevealInFinder("Builds");
            }
            else
            {
                Directory.CreateDirectory("Builds");
                EditorUtility.RevealInFinder("Builds");
                UnityEngine.Debug.Log("[Build Tools] Created Builds folder");
            }
        }
        
        [MenuItem("Lineage/Build Tools/Check Build Size", false, 660)]
        public static void CheckBuildSize()
        {
            if (Directory.Exists("Builds"))
            {
                long totalSize = GetDirectorySize("Builds");
                UnityEngine.Debug.Log($"[Build Tools] Total build size: {totalSize / (1024 * 1024)} MB");
                
                string[] buildFolders = Directory.GetDirectories("Builds");
                foreach (string folder in buildFolders)
                {
                    long folderSize = GetDirectorySize(folder);
                    string folderName = Path.GetFileName(folder);
                    UnityEngine.Debug.Log($"  {folderName}: {folderSize / (1024 * 1024)} MB");
                }
            }
            else
            {
                UnityEngine.Debug.Log("[Build Tools] No builds found");
            }
        }
        
        [MenuItem("Lineage/Build Tools/Validate Build Settings", false, 680)]
        public static void ValidateBuildSettings()
        {
            UnityEngine.Debug.Log("=== Build Settings Validation ===");
            
            // Check scenes in build
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            UnityEngine.Debug.Log($"Scenes in build: {scenes.Length}");
            
            int enabledScenes = 0;
            foreach (var scene in scenes)
            {
                if (scene.enabled)
                {
                    enabledScenes++;
                    UnityEngine.Debug.Log($"  ✓ {scene.path}");
                }
                else
                {
                    UnityEngine.Debug.Log($"  ✗ {scene.path} (disabled)");
                }
            }
            
            if (enabledScenes == 0)
            {
                UnityEngine.Debug.LogWarning("No scenes enabled in build settings!");
            }
            
            // Check player settings
            UnityEngine.Debug.Log($"Company Name: {PlayerSettings.companyName}");
            UnityEngine.Debug.Log($"Product Name: {PlayerSettings.productName}");
            UnityEngine.Debug.Log($"Version: {PlayerSettings.bundleVersion}");
            UnityEngine.Debug.Log($"Target Platform: {EditorUserBuildSettings.activeBuildTarget}");
        }
        
        private static string[] GetEnabledScenePaths()
        {
            return System.Array.ConvertAll(
                System.Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled),
                scene => scene.path
            );
        }
        
        private static long GetDirectorySize(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            long size = 0;
            
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                size += file.Length;
            }
            
            return size;
        }
    }
}

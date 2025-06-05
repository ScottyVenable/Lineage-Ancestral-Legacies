using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Lineage.Editor
{
    /// <summary>
    /// Project cleanup and maintenance tools
    /// </summary>
    public static class ProjectCleanupTools
    {
        [MenuItem("Lineage/Cleanup/Clean Empty Folders", false, 700)]
        public static void CleanEmptyFolders()
        {
            List<string> emptyFolders = FindEmptyFolders("Assets");
            
            if (emptyFolders.Count > 0)
            {
                if (EditorUtility.DisplayDialog("Clean Empty Folders", 
                    $"Found {emptyFolders.Count} empty folders. Remove them?", "Yes", "Cancel"))
                {
                    foreach (string folder in emptyFolders)
                    {
                        AssetDatabase.DeleteAsset(folder);
                    }
                    
                    AssetDatabase.Refresh();
                    Debug.Log($"[Cleanup] Removed {emptyFolders.Count} empty folders");
                }
            }
            else
            {
                Debug.Log("[Cleanup] No empty folders found");
            }
        }
        
        [MenuItem("Lineage/Cleanup/Remove Meta Files for Missing Assets", false, 720)]
        public static void RemoveOrphanedMetaFiles()
        {
            string[] metaFiles = Directory.GetFiles("Assets", "*.meta", SearchOption.AllDirectories);
            int removedCount = 0;
            
            foreach (string metaFile in metaFiles)
            {
                string assetFile = metaFile.Substring(0, metaFile.Length - 5); // Remove .meta extension
                
                if (!File.Exists(assetFile) && !Directory.Exists(assetFile))
                {
                    File.Delete(metaFile);
                    removedCount++;
                    Debug.Log($"[Cleanup] Removed orphaned meta file: {metaFile}");
                }
            }
            
            if (removedCount > 0)
            {
                AssetDatabase.Refresh();
                Debug.Log($"[Cleanup] Removed {removedCount} orphaned meta files");
            }
            else
            {
                Debug.Log("[Cleanup] No orphaned meta files found");
            }
        }
        
        [MenuItem("Lineage/Cleanup/Clear Unity Cache", false, 740)]
        public static void ClearUnityCache()
        {
            if (EditorUtility.DisplayDialog("Clear Unity Cache", 
                "This will clear Unity's cache folders. Continue?", "Yes", "Cancel"))
            {
                // Clear Library cache folders
                string[] cacheFolders = {
                    "Library/ArtifactDB",
                    "Library/SourceAssetDB",
                    "Library/ShaderCache",
                    "Library/PackageCache"
                };
                
                foreach (string folder in cacheFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        try
                        {
                            Directory.Delete(folder, true);
                            Debug.Log($"[Cleanup] Cleared cache folder: {folder}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"[Cleanup] Could not clear {folder}: {e.Message}");
                        }
                    }
                }
                
                AssetDatabase.Refresh();
                Debug.Log("[Cleanup] Unity cache cleared. Restart Unity for full effect.");
            }
        }
        
        [MenuItem("Lineage/Cleanup/Clear Console", false, 760)]
        public static void ClearConsole()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
            
            Debug.Log("[Cleanup] Console cleared");
        }
        
        [MenuItem("Lineage/Cleanup/Clear PlayerPrefs", false, 780)]
        public static void ClearPlayerPrefs()
        {
            if (EditorUtility.DisplayDialog("Clear PlayerPrefs", 
                "This will clear all PlayerPrefs data. Continue?", "Yes", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("[Cleanup] PlayerPrefs cleared");
            }
        }
        
        [MenuItem("Lineage/Cleanup/Remove Backup Files", false, 800)]
        public static void RemoveBackupFiles()
        {
            string[] backupExtensions = { "*.bak", "*.orig", "*.backup", "*~" };
            int removedCount = 0;
            
            foreach (string pattern in backupExtensions)
            {
                string[] backupFiles = Directory.GetFiles("Assets", pattern, SearchOption.AllDirectories);
                
                foreach (string file in backupFiles)
                {
                    File.Delete(file);
                    removedCount++;
                    Debug.Log($"[Cleanup] Removed backup file: {file}");
                }
            }
            
            if (removedCount > 0)
            {
                AssetDatabase.Refresh();
                Debug.Log($"[Cleanup] Removed {removedCount} backup files");
            }
            else
            {
                Debug.Log("[Cleanup] No backup files found");
            }
        }
        
        [MenuItem("Lineage/Cleanup/Full Project Cleanup", false, 820)]
        public static void FullProjectCleanup()
        {
            if (EditorUtility.DisplayDialog("Full Project Cleanup", 
                "This will run all cleanup operations. Continue?", "Yes", "Cancel"))
            {
                Debug.Log("=== Starting Full Project Cleanup ===");
                
                CleanEmptyFolders();
                RemoveOrphanedMetaFiles();
                RemoveBackupFiles();
                ClearConsole();
                
                // Force garbage collection
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
                
                Debug.Log("=== Full Project Cleanup Complete ===");
            }
        }
        
        private static List<string> FindEmptyFolders(string path)
        {
            List<string> emptyFolders = new List<string>();
            
            if (!Directory.Exists(path))
                return emptyFolders;
            
            string[] subDirectories = Directory.GetDirectories(path);
            
            foreach (string subDir in subDirectories)
            {
                string relativePath = subDir.Replace('\\', '/');
                
                // Recursively check subdirectories
                emptyFolders.AddRange(FindEmptyFolders(relativePath));
                
                // Check if current directory is empty
                if (IsDirectoryEmpty(relativePath))
                {
                    emptyFolders.Add(relativePath);
                }
            }
            
            return emptyFolders;
        }
        
        private static bool IsDirectoryEmpty(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            
            // Check for non-meta files
            foreach (string file in files)
            {
                if (!file.EndsWith(".meta"))
                {
                    return false;
                }
            }
            
            return dirs.Length == 0;
        }
    }
}

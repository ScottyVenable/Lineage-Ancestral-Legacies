using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

namespace Lineage.Editor
{
    /// <summary>
    /// Scene management and workflow tools for faster development
    /// </summary>
    public static class SceneManagementTools
    {
        [MenuItem("Lineage/Scene Tools/Quick Save All", false, 100)]
        public static void QuickSaveAll()
        {
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[Scene Tools] Saved all open scenes and assets");
        }
        
        [MenuItem("Lineage/Scene Tools/Create Scene Backup", false, 101)]
        public static void CreateSceneBackup()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.isDirty && string.IsNullOrEmpty(activeScene.path))
            {
                Debug.LogWarning("[Scene Tools] No active scene to backup");
                return;
            }
            
            string scenePath = activeScene.path;
            string backupPath = scenePath.Replace(".unity", $"_backup_{System.DateTime.Now:yyyyMMdd_HHmmss}.unity");
            
            AssetDatabase.CopyAsset(scenePath, backupPath);
            Debug.Log($"[Scene Tools] Created backup: {backupPath}");
        }
        
        [MenuItem("Lineage/Scene Tools/Load Main Scene", false, 120)]
        public static void LoadMainScene()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            if (sceneGuids.Length > 0)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[0]);
                EditorSceneManager.OpenScene(scenePath);
                Debug.Log($"[Scene Tools] Loaded scene: {scenePath}");
            }
            else
            {
                Debug.LogWarning("[Scene Tools] No scenes found in Assets/Scenes");
            }
        }
        
        [MenuItem("Lineage/Scene Tools/Clean Empty GameObjects", false, 140)]
        public static void CleanEmptyGameObjects()
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            int cleanedCount = 0;
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.transform.childCount == 0 && obj.GetComponents<Component>().Length == 1) // Only Transform
                {
                    if (EditorUtility.DisplayDialog("Clean Empty GameObject", 
                        $"Remove empty GameObject '{obj.name}'?", "Yes", "Skip"))
                    {
                        DestroyImmediate(obj);
                        cleanedCount++;
                    }
                }
            }
            
            Debug.Log($"[Scene Tools] Cleaned {cleanedCount} empty GameObjects");
        }
        
        [MenuItem("Lineage/Scene Tools/List All Scene Objects", false, 160)]
        public static void ListAllSceneObjects()
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            
            Debug.Log("=== Scene Object Report ===");
            Debug.Log($"Total GameObjects: {allObjects.Length}");
            
            var componentCounts = allObjects
                .SelectMany(obj => obj.GetComponents<Component>())
                .Where(comp => comp != null)
                .GroupBy(comp => comp.GetType().Name)
                .OrderByDescending(group => group.Count())
                .Take(10);
            
            Debug.Log("Top 10 Component Types:");
            foreach (var group in componentCounts)
            {
                Debug.Log($"  {group.Key}: {group.Count()}");
            }
        }
    }
}

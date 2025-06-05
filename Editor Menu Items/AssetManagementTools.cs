using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Lineage.Editor
{
    /// <summary>
    /// Asset organization and management tools
    /// </summary>
    public static class AssetManagementTools
    {
        [MenuItem("Lineage/Asset Tools/Find Missing Scripts", false, 200)]
        public static void FindMissingScripts()
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> objectsWithMissing = new List<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                Component[] components = obj.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    if (comp == null)
                    {
                        objectsWithMissing.Add(obj);
                        break;
                    }
                }
            }
            
            Debug.Log($"[Asset Tools] Found {objectsWithMissing.Count} objects with missing scripts:");
            foreach (GameObject obj in objectsWithMissing)
            {
                Debug.Log($"  - {obj.name} (Scene: {obj.scene.name})", obj);
            }
        }
        
        [MenuItem("Lineage/Asset Tools/Find Unused Assets", false, 201)]
        public static void FindUnusedAssets()
        {
            string[] allAssets = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/") && !path.Contains(".meta"))
                .ToArray();
            
            List<string> unusedAssets = new List<string>();
            
            foreach (string assetPath in allAssets)
            {
                string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);
                if (dependencies.Length <= 1) // Only depends on itself
                {
                    unusedAssets.Add(assetPath);
                }
            }
            
            Debug.Log($"[Asset Tools] Found {unusedAssets.Count} potentially unused assets:");
            foreach (string asset in unusedAssets.Take(20)) // Show first 20
            {
                Debug.Log($"  - {asset}");
            }
            
            if (unusedAssets.Count > 20)
            {
                Debug.Log($"  ... and {unusedAssets.Count - 20} more");
            }
        }
        
        [MenuItem("Lineage/Asset Tools/Organize Sprites by Folder", false, 220)]
        public static void OrganizeSprites()
        {
            string[] spriteGuids = AssetDatabase.FindAssets("t:Texture2D");
            int movedCount = 0;
            
            foreach (string guid in spriteGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                
                if (importer != null && importer.textureType == TextureImporterType.Sprite)
                {
                    string fileName = Path.GetFileName(assetPath);
                    string targetFolder = "Assets/Sprites/Organized";
                    
                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }
                    
                    string newPath = Path.Combine(targetFolder, fileName);
                    if (assetPath != newPath && !File.Exists(newPath))
                    {
                        AssetDatabase.MoveAsset(assetPath, newPath);
                        movedCount++;
                    }
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log($"[Asset Tools] Organized {movedCount} sprites into Assets/Sprites/Organized");
        }
        
        [MenuItem("Lineage/Asset Tools/Refresh All Assets", false, 240)]
        public static void RefreshAllAssets()
        {
            AssetDatabase.Refresh();
            Debug.Log("[Asset Tools] Refreshed all assets");
        }
        
        [MenuItem("Lineage/Asset Tools/Clear Asset Cache", false, 241)]
        public static void ClearAssetCache()
        {
            if (EditorUtility.DisplayDialog("Clear Asset Cache", 
                "This will clear Unity's asset cache. Continue?", "Yes", "Cancel"))
            {
                AssetDatabase.ReleaseCachedFileHandles();
                System.GC.Collect();
                Debug.Log("[Asset Tools] Cleared asset cache");
            }
        }
        
        [MenuItem("Lineage/Asset Tools/Generate Asset Report", false, 260)]
        public static void GenerateAssetReport()
        {
            var assetTypes = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/"))
                .GroupBy(path => Path.GetExtension(path).ToLower())
                .OrderByDescending(group => group.Count())
                .Take(15);
            
            Debug.Log("=== Asset Report ===");
            foreach (var group in assetTypes)
            {
                string extension = string.IsNullOrEmpty(group.Key) ? "(no extension)" : group.Key;
                Debug.Log($"{extension}: {group.Count()} files");
            }
        }
    }
}

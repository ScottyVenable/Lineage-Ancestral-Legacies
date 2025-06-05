using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Rendering;

namespace Lineage.Editor
{
    public static class AdvancedAssetPipelineTools
    {
        [MenuItem("Lineage/Advanced Asset Pipeline/Smart Texture Optimizer", priority = 700)]
        public static void SmartTextureOptimizer()
        {
            var textures = GetAllTextures();
            int optimized = 0;
            
            foreach (var texture in textures)
            {
                var path = AssetDatabase.GetAssetPath(texture);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                
                bool changed = false;
                
                // Optimize based on usage pattern
                if (IsUITexture(path))
                {
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }
                }
                else if (IsNormalMap(texture.name))
                {
                    if (importer.textureType != TextureImporterType.NormalMap)
                    {
                        importer.textureType = TextureImporterType.NormalMap;
                        changed = true;
                    }
                }
                
                // Optimize compression
                var settings = importer.GetPlatformTextureSettings("Standalone");
                if (texture.width > 2048 || texture.height > 2048)
                {
                    if (settings.format != TextureImporterFormat.DXT5)
                    {
                        settings.format = TextureImporterFormat.DXT5;
                        settings.compressionQuality = 100;
                        importer.SetPlatformTextureSettings(settings);
                        changed = true;
                    }
                }
                
                if (changed)
                {
                    AssetDatabase.ImportAsset(path);
                    optimized++;
                }
            }
            
            Debug.Log($"Smart Texture Optimizer: Optimized {optimized} textures");
            EditorUtility.DisplayDialog("Optimization Complete", 
                $"Optimized {optimized} textures for better performance", "OK");
        }
        
        [MenuItem("Lineage/Advanced Asset Pipeline/Audio Asset Optimizer", priority = 701)]
        public static void AudioAssetOptimizer()
        {
            var audioClips = GetAllAudioClips();
            int optimized = 0;
            
            foreach (var clip in audioClips)
            {
                var path = AssetDatabase.GetAssetPath(clip);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;
                
                bool changed = false;
                
                // Optimize based on audio type
                if (IsMusicFile(path))
                {
                    var settings = importer.GetOverrideSampleSettings("Standalone");
                    if (settings.compressionFormat != AudioCompressionFormat.Vorbis)
                    {
                        settings.compressionFormat = AudioCompressionFormat.Vorbis;
                        settings.quality = 0.7f;
                        importer.SetOverrideSampleSettings("Standalone", settings);
                        changed = true;
                    }
                }
                else if (IsSFXFile(path))
                {
                    var settings = importer.GetOverrideSampleSettings("Standalone");
                    if (settings.compressionFormat != AudioCompressionFormat.PCM)
                    {
                        settings.compressionFormat = AudioCompressionFormat.PCM;
                        importer.SetOverrideSampleSettings("Standalone", settings);
                        changed = true;
                    }
                }
                
                if (changed)
                {
                    AssetDatabase.ImportAsset(path);
                    optimized++;
                }
            }
            
            Debug.Log($"Audio Asset Optimizer: Optimized {optimized} audio clips");
            EditorUtility.DisplayDialog("Audio Optimization Complete", 
                $"Optimized {optimized} audio clips for better performance", "OK");
        }
        
        [MenuItem("Lineage/Advanced Asset Pipeline/Model Import Standardizer", priority = 702)]
        public static void ModelImportStandardizer()
        {
            var models = GetAllModels();
            int standardized = 0;
            
            foreach (var model in models)
            {
                var path = AssetDatabase.GetAssetPath(model);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;
                
                bool changed = false;
                
                // Standard import settings
                if (importer.importNormals != ModelImporterNormals.Import)
                {
                    importer.importNormals = ModelImporterNormals.Import;
                    changed = true;
                }
                
                if (importer.importTangents != ModelImporterTangents.CalculateMikk)
                {
                    importer.importTangents = ModelImporterTangents.CalculateMikk;
                    changed = true;
                }
                
                if (importer.meshCompression != ModelImporterMeshCompression.Off)
                {
                    importer.meshCompression = ModelImporterMeshCompression.Off;
                    changed = true;
                }
                
                if (!importer.isReadable)
                {
                    importer.isReadable = false; // Ensure it's set for performance
                    changed = true;
                }
                
                if (changed)
                {
                    AssetDatabase.ImportAsset(path);
                    standardized++;
                }
            }
            
            Debug.Log($"Model Import Standardizer: Standardized {standardized} models");
            EditorUtility.DisplayDialog("Standardization Complete", 
                $"Standardized import settings for {standardized} models", "OK");
        }
        
        [MenuItem("Lineage/Advanced Asset Pipeline/Asset Dependency Analyzer", priority = 703)]
        public static void AssetDependencyAnalyzer()
        {
            var window = EditorWindow.GetWindow<AssetDependencyWindow>();
            window.titleContent = new GUIContent("Asset Dependency Analyzer");
            window.Show();
        }
        
        [MenuItem("Lineage/Advanced Asset Pipeline/Batch Asset Renamer", priority = 704)]
        public static void BatchAssetRenamer()
        {
            var window = EditorWindow.GetWindow<BatchRenamerWindow>();
            window.titleContent = new GUIContent("Batch Asset Renamer");
            window.Show();
        }
        
        [MenuItem("Lineage/Advanced Asset Pipeline/Asset Usage Report", priority = 705)]
        public static void GenerateAssetUsageReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== LINEAGE ASSET USAGE REPORT ===");
            report.AppendLine($"Generated: {System.DateTime.Now}");
            report.AppendLine();
            
            // Texture analysis
            var textures = GetAllTextures();
            report.AppendLine($"TEXTURES ({textures.Length}):");
            var texturesBySize = textures.GroupBy(t => GetTextureSizeCategory(t)).OrderBy(g => g.Key);
            foreach (var group in texturesBySize)
            {
                report.AppendLine($"  {group.Key}: {group.Count()} textures");
            }
            report.AppendLine();
            
            // Audio analysis
            var audioClips = GetAllAudioClips();
            report.AppendLine($"AUDIO CLIPS ({audioClips.Length}):");
            var audioByType = audioClips.GroupBy(a => GetAudioType(AssetDatabase.GetAssetPath(a)));
            foreach (var group in audioByType)
            {
                report.AppendLine($"  {group.Key}: {group.Count()} clips");
            }
            report.AppendLine();
            
            // Model analysis
            var models = GetAllModels();
            report.AppendLine($"MODELS ({models.Length}):");
            report.AppendLine($"  Total polygon count: {models.Sum(m => GetPolygonCount(m))}");
            report.AppendLine();
            
            // Save report
            var reportPath = "Assets/AssetUsageReport.txt";
            File.WriteAllText(reportPath, report.ToString());
            AssetDatabase.Refresh();
            
            Debug.Log($"Asset Usage Report generated: {reportPath}");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(reportPath);
        }
        
        // Helper methods
        private static Texture2D[] GetAllTextures()
        {
            return Resources.FindObjectsOfTypeAll<Texture2D>()
                .Where(t => AssetDatabase.Contains(t))
                .ToArray();
        }
        
        private static AudioClip[] GetAllAudioClips()
        {
            return Resources.FindObjectsOfTypeAll<AudioClip>()
                .Where(a => AssetDatabase.Contains(a))
                .ToArray();
        }
        
        private static GameObject[] GetAllModels()
        {
            var guids = AssetDatabase.FindAssets("t:Model");
            return guids.Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(obj => obj != null)
                .ToArray();
        }
        
        private static bool IsUITexture(string path)
        {
            return path.Contains("/UI/") || path.Contains("/GUI/") || path.Contains("/Interface/");
        }
        
        private static bool IsNormalMap(string name)
        {
            return name.ToLower().Contains("normal") || name.ToLower().Contains("_n") || name.ToLower().EndsWith("_normal");
        }
        
        private static bool IsMusicFile(string path)
        {
            return path.Contains("/Music/") || path.Contains("/BGM/") || path.Contains("/Background/");
        }
        
        private static bool IsSFXFile(string path)
        {
            return path.Contains("/SFX/") || path.Contains("/Effects/") || path.Contains("/Sounds/");
        }
        
        private static string GetTextureSizeCategory(Texture2D texture)
        {
            var maxSize = Mathf.Max(texture.width, texture.height);
            if (maxSize <= 256) return "Small (≤256)";
            if (maxSize <= 512) return "Medium (≤512)";
            if (maxSize <= 1024) return "Large (≤1024)";
            return "XLarge (>1024)";
        }
        
        private static string GetAudioType(string path)
        {
            if (IsMusicFile(path)) return "Music";
            if (IsSFXFile(path)) return "SFX";
            return "Other";
        }
        
        private static int GetPolygonCount(GameObject model)
        {
            var meshFilters = model.GetComponentsInChildren<MeshFilter>();
            return meshFilters.Sum(mf => mf.sharedMesh != null ? mf.sharedMesh.triangles.Length / 3 : 0);
        }
    }
    
    // Asset Dependency Analyzer Window
    public class AssetDependencyWindow : EditorWindow
    {
        private Object selectedAsset;
        private Vector2 scrollPosition;
        private List<string> dependencies = new List<string>();
        private List<string> referencedBy = new List<string>();
        
        private void OnGUI()
        {
            GUILayout.Label("Asset Dependency Analyzer", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            selectedAsset = EditorGUILayout.ObjectField("Analyze Asset:", selectedAsset, typeof(Object), false);
            
            if (GUILayout.Button("Analyze Dependencies"))
            {
                AnalyzeAsset();
            }
            
            if (dependencies.Count > 0 || referencedBy.Count > 0)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                if (dependencies.Count > 0)
                {
                    GUILayout.Label($"Dependencies ({dependencies.Count}):", EditorStyles.boldLabel);
                    foreach (var dep in dependencies)
                    {
                        if (GUILayout.Button(dep, EditorStyles.linkLabel))
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<Object>(dep);
                            Selection.activeObject = asset;
                            EditorUtility.FocusProjectWindow();
                        }
                    }
                    GUILayout.Space(10);
                }
                
                if (referencedBy.Count > 0)
                {
                    GUILayout.Label($"Referenced By ({referencedBy.Count}):", EditorStyles.boldLabel);
                    foreach (var reference in referencedBy)
                    {
                        if (GUILayout.Button(reference, EditorStyles.linkLabel))
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<Object>(reference);
                            Selection.activeObject = asset;
                            EditorUtility.FocusProjectWindow();
                        }
                    }
                }
                
                EditorGUILayout.EndScrollView();
            }
        }
        
        private void AnalyzeAsset()
        {
            if (selectedAsset == null) return;
            
            var assetPath = AssetDatabase.GetAssetPath(selectedAsset);
            dependencies.Clear();
            referencedBy.Clear();
            
            // Get dependencies
            var deps = AssetDatabase.GetDependencies(assetPath, false);
            dependencies.AddRange(deps.Where(d => d != assetPath));
            
            // Find what references this asset
            var allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (var asset in allAssets)
            {
                if (asset == assetPath) continue;
                
                var assetDeps = AssetDatabase.GetDependencies(asset, false);
                if (assetDeps.Contains(assetPath))
                {
                    referencedBy.Add(asset);
                }
            }
        }
    }
    
    // Batch Renamer Window
    public class BatchRenamerWindow : EditorWindow
    {
        private string findText = "";
        private string replaceText = "";
        private string prefix = "";
        private string suffix = "";
        private bool useNumbering = false;
        private int startNumber = 1;
        private List<Object> selectedAssets = new List<Object>();
        
        private void OnGUI()
        {
            GUILayout.Label("Batch Asset Renamer", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Add Selected Assets"))
            {
                selectedAssets.AddRange(Selection.objects);
                selectedAssets = selectedAssets.Distinct().ToList();
            }
            
            GUILayout.Label($"Assets to rename: {selectedAssets.Count}");
            
            if (GUILayout.Button("Clear List"))
            {
                selectedAssets.Clear();
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("Rename Options:", EditorStyles.boldLabel);
            
            findText = EditorGUILayout.TextField("Find:", findText);
            replaceText = EditorGUILayout.TextField("Replace with:", replaceText);
            
            GUILayout.Space(5);
            
            prefix = EditorGUILayout.TextField("Add Prefix:", prefix);
            suffix = EditorGUILayout.TextField("Add Suffix:", suffix);
            
            GUILayout.Space(5);
            
            useNumbering = EditorGUILayout.Toggle("Add Numbering:", useNumbering);
            if (useNumbering)
            {
                startNumber = EditorGUILayout.IntField("Start Number:", startNumber);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Preview Changes"))
            {
                PreviewChanges();
            }
            
            if (GUILayout.Button("Apply Rename"))
            {
                ApplyRename();
            }
        }
        
        private void PreviewChanges()
        {
            Debug.Log("=== RENAME PREVIEW ===");
            int counter = startNumber;
            
            foreach (var asset in selectedAssets)
            {
                var oldName = asset.name;
                var newName = GenerateNewName(oldName, counter);
                Debug.Log($"{oldName} → {newName}");
                if (useNumbering) counter++;
            }
        }
        
        private void ApplyRename()
        {
            int counter = startNumber;
            int renamed = 0;
            
            foreach (var asset in selectedAssets)
            {
                var oldName = asset.name;
                var newName = GenerateNewName(oldName, counter);
                
                var assetPath = AssetDatabase.GetAssetPath(asset);
                AssetDatabase.RenameAsset(assetPath, newName);
                renamed++;
                
                if (useNumbering) counter++;
            }
            
            AssetDatabase.Refresh();
            Debug.Log($"Batch Rename: Renamed {renamed} assets");
            EditorUtility.DisplayDialog("Rename Complete", $"Renamed {renamed} assets", "OK");
        }
        
        private string GenerateNewName(string originalName, int number)
        {
            var newName = originalName;
            
            // Find and replace
            if (!string.IsNullOrEmpty(findText))
            {
                newName = newName.Replace(findText, replaceText);
            }
            
            // Add prefix
            if (!string.IsNullOrEmpty(prefix))
            {
                newName = prefix + newName;
            }
            
            // Add suffix
            if (!string.IsNullOrEmpty(suffix))
            {
                newName = newName + suffix;
            }
            
            // Add numbering
            if (useNumbering)
            {
                newName = newName + "_" + number.ToString("00");
            }
            
            return newName;
        }
    }
}

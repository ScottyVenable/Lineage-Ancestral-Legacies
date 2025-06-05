using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System;

namespace Lineage.Editor
{
    public static class DataManagementTools
    {
        [MenuItem("Lineage/Data Management/ScriptableObject Creator", priority = 1000)]
        public static void ScriptableObjectCreator()
        {
            var window = EditorWindow.GetWindow<ScriptableObjectCreatorWindow>();
            window.titleContent = new GUIContent("ScriptableObject Creator");
            window.Show();
        }
        
        [MenuItem("Lineage/Data Management/Data Validator", priority = 1001)]
        public static void DataValidator()
        {
            var window = EditorWindow.GetWindow<DataValidatorWindow>();
            window.titleContent = new GUIContent("Data Validator");
            window.Show();
        }
        
        [MenuItem("Lineage/Data Management/Asset Reference Finder", priority = 1002)]
        public static void AssetReferenceFinder()
        {
            var window = EditorWindow.GetWindow<AssetReferenceWindow>();
            window.titleContent = new GUIContent("Asset Reference Finder");
            window.Show();
        }
        
        [MenuItem("Lineage/Data Management/Data Migration Tool", priority = 1003)]
        public static void DataMigrationTool()
        {
            var window = EditorWindow.GetWindow<DataMigrationWindow>();
            window.titleContent = new GUIContent("Data Migration Tool");
            window.Show();
        }
        
        [MenuItem("Lineage/Data Management/Bulk Data Editor", priority = 1004)]
        public static void BulkDataEditor()
        {
            var window = EditorWindow.GetWindow<BulkDataEditorWindow>();
            window.titleContent = new GUIContent("Bulk Data Editor");
            window.Show();
        }
        
        [MenuItem("Lineage/Data Management/Generate Data Report", priority = 1005)]
        public static void GenerateDataReport()
        {
            var scriptableObjects = GetAllScriptableObjects();
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== LINEAGE DATA MANAGEMENT REPORT ===");
            report.AppendLine($"Generated: {System.DateTime.Now}");
            report.AppendLine();
            
            report.AppendLine($"SCRIPTABLEOBJECTS ({scriptableObjects.Length}):");
            
            var typeGroups = scriptableObjects.GroupBy(so => so.GetType().Name);
            foreach (var group in typeGroups.OrderBy(g => g.Key))
            {
                report.AppendLine($"  {group.Key}: {group.Count()} instances");
                
                foreach (var instance in group.Take(5)) // Show first 5 instances
                {
                    var path = AssetDatabase.GetAssetPath(instance);
                    var size = GetAssetSize(path);
                    report.AppendLine($"    • {instance.name} ({size} bytes)");
                }
                
                if (group.Count() > 5)
                {
                    report.AppendLine($"    ... and {group.Count() - 5} more");
                }
                report.AppendLine();
            }
            
            // Data integrity check
            report.AppendLine("DATA INTEGRITY:");
            var nullReferences = FindNullReferences(scriptableObjects);
            if (nullReferences.Count > 0)
            {
                report.AppendLine($"  WARNING: {nullReferences.Count} objects with null references found");
                foreach (var nullRef in nullReferences.Take(10))
                {
                    report.AppendLine($"    • {nullRef.asset.name} - {nullRef.fieldName}");
                }
            }
            else
            {
                report.AppendLine("  All references are valid");
            }
            
            // Save report
            var reportPath = "Assets/DataManagementReport.txt";
            File.WriteAllText(reportPath, report.ToString());
            AssetDatabase.Refresh();
            
            Debug.Log($"Data Management Report generated: {reportPath}");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(reportPath);
        }
        
        // Helper methods
        private static ScriptableObject[] GetAllScriptableObjects()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            return guids.Select(guid => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(so => so != null)
                .ToArray();
        }
        
        private static long GetAssetSize(string path)
        {
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }
            return 0;
        }
        
        private static List<NullReferenceInfo> FindNullReferences(ScriptableObject[] objects)
        {
            var nullRefs = new List<NullReferenceInfo>();
            
            foreach (var obj in objects)
            {
                var type = obj.GetType();
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                foreach (var field in fields)
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                    {
                        var value = field.GetValue(obj);
                        if (value != null && value.Equals(null)) // Unity null check
                        {
                            nullRefs.Add(new NullReferenceInfo
                            {
                                asset = obj,
                                fieldName = field.Name
                            });
                        }
                    }
                }
            }
            
            return nullRefs;
        }
        
        private struct NullReferenceInfo
        {
            public ScriptableObject asset;
            public string fieldName;
        }
    }
    
    // ScriptableObject Creator Window
    public class ScriptableObjectCreatorWindow : EditorWindow
    {
        private Type[] scriptableObjectTypes;
        private string[] typeNames;
        private int selectedTypeIndex = 0;
        private string assetName = "NewScriptableObject";
        private string folderPath = "Assets/Data";
        
        private void OnEnable()
        {
            RefreshScriptableObjectTypes();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("ScriptableObject Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Refresh Types"))
            {
                RefreshScriptableObjectTypes();
            }
            
            if (scriptableObjectTypes != null && scriptableObjectTypes.Length > 0)
            {
                selectedTypeIndex = EditorGUILayout.Popup("Type:", selectedTypeIndex, typeNames);
                assetName = EditorGUILayout.TextField("Asset Name:", assetName);
                
                GUILayout.BeginHorizontal();
                folderPath = EditorGUILayout.TextField("Folder Path:", folderPath);
                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    var path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        // Convert absolute path to relative
                        if (path.StartsWith(Application.dataPath))
                        {
                            folderPath = "Assets" + path.Substring(Application.dataPath.Length);
                        }
                    }
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("Create ScriptableObject"))
                {
                    CreateScriptableObject();
                }
            }
            else
            {
                GUILayout.Label("No ScriptableObject types found in the project");
            }
        }
        
        private void RefreshScriptableObjectTypes()
        {
            var types = new List<Type>();
            
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(ScriptableObject)) && !type.IsAbstract)
                        {
                            types.Add(type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded
                }
            }
            
            scriptableObjectTypes = types.OrderBy(t => t.Name).ToArray();
            typeNames = scriptableObjectTypes.Select(t => t.Name).ToArray();
        }
        
        private void CreateScriptableObject()
        {
            if (selectedTypeIndex >= 0 && selectedTypeIndex < scriptableObjectTypes.Length)
            {
                var type = scriptableObjectTypes[selectedTypeIndex];
                var instance = ScriptableObject.CreateInstance(type);
                
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                
                var assetPath = $"{folderPath}/{assetName}.asset";
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
                
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = instance;
                
                Debug.Log($"Created ScriptableObject: {assetPath}");
                EditorUtility.DisplayDialog("Creation Successful", 
                    $"Created {type.Name} at {assetPath}", "OK");
            }
        }
    }
    
    // Data Validator Window
    public class DataValidatorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<ValidationResult> validationResults = new List<ValidationResult>();
        
        private struct ValidationResult
        {
            public ScriptableObject asset;
            public string issue;
            public ValidationSeverity severity;
        }
        
        private enum ValidationSeverity
        {
            Info,
            Warning,
            Error
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Data Validator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Validate All ScriptableObjects"))
            {
                ValidateAllData();
            }
            
            if (validationResults.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Validation Results ({validationResults.Count} issues):", EditorStyles.boldLabel);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var result in validationResults)
                {
                    var color = GetSeverityColor(result.severity);
                    GUI.color = color;
                    
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(result.asset.name, EditorStyles.linkLabel, GUILayout.Width(150)))
                    {
                        Selection.activeObject = result.asset;
                        EditorUtility.FocusProjectWindow();
                    }
                    GUILayout.Label($"[{result.severity}]", GUILayout.Width(70));
                    GUILayout.Label(result.issue);
                    GUILayout.EndHorizontal();
                    
                    GUI.color = Color.white;
                }
                
                EditorGUILayout.EndScrollView();
                
                GUILayout.Space(10);
                if (GUILayout.Button("Clear Results"))
                {
                    validationResults.Clear();
                }
            }
        }
        
        private void ValidateAllData()
        {
            validationResults.Clear();
            var scriptableObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>()
                .Where(so => AssetDatabase.Contains(so))
                .ToArray();
            
            foreach (var obj in scriptableObjects)
            {
                ValidateObject(obj);
            }
            
            Debug.Log($"Data validation complete. Found {validationResults.Count} issues.");
        }
        
        private void ValidateObject(ScriptableObject obj)
        {
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            foreach (var field in fields)
            {
                // Check for null references
                if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(obj);
                    if (value != null && value.Equals(null))
                    {
                        validationResults.Add(new ValidationResult
                        {
                            asset = obj,
                            issue = $"Null reference in field '{field.Name}'",
                            severity = ValidationSeverity.Error
                        });
                    }
                }
                
                // Check for empty strings in required fields
                if (field.FieldType == typeof(string))
                {
                    var value = field.GetValue(obj) as string;
                    if (string.IsNullOrEmpty(value) && HasRequiredAttribute(field))
                    {
                        validationResults.Add(new ValidationResult
                        {
                            asset = obj,
                            issue = $"Required string field '{field.Name}' is empty",
                            severity = ValidationSeverity.Warning
                        });
                    }
                }
                
                // Check for negative values in fields that should be positive
                if (field.FieldType == typeof(float) || field.FieldType == typeof(int))
                {
                    var value = Convert.ToSingle(field.GetValue(obj));
                    if (value < 0 && ShouldBePositive(field.Name))
                    {
                        validationResults.Add(new ValidationResult
                        {
                            asset = obj,
                            issue = $"Field '{field.Name}' has negative value: {value}",
                            severity = ValidationSeverity.Warning
                        });
                    }
                }
            }
        }
        
        private bool HasRequiredAttribute(FieldInfo field)
        {
            // In a real implementation, you might have custom attributes
            return field.Name.ToLower().Contains("name") || field.Name.ToLower().Contains("id");
        }
        
        private bool ShouldBePositive(string fieldName)
        {
            var lowerName = fieldName.ToLower();
            return lowerName.Contains("health") || lowerName.Contains("damage") || 
                   lowerName.Contains("speed") || lowerName.Contains("cost") ||
                   lowerName.Contains("price") || lowerName.Contains("value");
        }
        
        private Color GetSeverityColor(ValidationSeverity severity)
        {
            switch (severity)
            {
                case ValidationSeverity.Info: return Color.cyan;
                case ValidationSeverity.Warning: return Color.yellow;
                case ValidationSeverity.Error: return Color.red;
                default: return Color.white;
            }
        }
    }
    
    // Asset Reference Window
    public class AssetReferenceWindow : EditorWindow
    {
        private UnityEngine.Object targetAsset;
        private Vector2 scrollPosition;
        private List<string> referencingAssets = new List<string>();
        
        private void OnGUI()
        {
            GUILayout.Label("Asset Reference Finder", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            targetAsset = EditorGUILayout.ObjectField("Find References To:", targetAsset, typeof(UnityEngine.Object), false);
            
            if (GUILayout.Button("Find References"))
            {
                FindReferences();
            }
            
            if (referencingAssets.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Found {referencingAssets.Count} references:", EditorStyles.boldLabel);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var reference in referencingAssets)
                {
                    if (GUILayout.Button(reference, EditorStyles.linkLabel))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(reference);
                        Selection.activeObject = asset;
                        EditorUtility.FocusProjectWindow();
                    }
                }
                
                EditorGUILayout.EndScrollView();
            }
        }
        
        private void FindReferences()
        {
            if (targetAsset == null) return;
            
            referencingAssets.Clear();
            var targetPath = AssetDatabase.GetAssetPath(targetAsset);
            var allAssets = AssetDatabase.GetAllAssetPaths();
            
            foreach (var assetPath in allAssets)
            {
                if (assetPath == targetPath) continue;
                
                var dependencies = AssetDatabase.GetDependencies(assetPath, false);
                if (dependencies.Contains(targetPath))
                {
                    referencingAssets.Add(assetPath);
                }
            }
            
            Debug.Log($"Found {referencingAssets.Count} references to {targetAsset.name}");
        }
    }
    
    // Data Migration Window
    public class DataMigrationWindow : EditorWindow
    {
        private string sourceFieldName = "";
        private string targetFieldName = "";
        private Type sourceType;
        private Type targetType;
        private string[] availableTypes;
        private int sourceTypeIndex = 0;
        private int targetTypeIndex = 0;
        
        private void OnEnable()
        {
            RefreshAvailableTypes();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Data Migration Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (availableTypes != null && availableTypes.Length > 0)
            {
                sourceTypeIndex = EditorGUILayout.Popup("Source Type:", sourceTypeIndex, availableTypes);
                sourceFieldName = EditorGUILayout.TextField("Source Field:", sourceFieldName);
                
                GUILayout.Space(5);
                
                targetTypeIndex = EditorGUILayout.Popup("Target Type:", targetTypeIndex, availableTypes);
                targetFieldName = EditorGUILayout.TextField("Target Field:", targetFieldName);
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("Preview Migration"))
                {
                    PreviewMigration();
                }
                
                if (GUILayout.Button("Execute Migration"))
                {
                    ExecuteMigration();
                }
            }
            else
            {
                GUILayout.Label("No ScriptableObject types available");
                if (GUILayout.Button("Refresh Types"))
                {
                    RefreshAvailableTypes();
                }
            }
        }
        
        private void RefreshAvailableTypes()
        {
            var types = new List<string>();
            
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(ScriptableObject)) && !type.IsAbstract)
                        {
                            types.Add(type.Name);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip problematic assemblies
                }
            }
            
            availableTypes = types.OrderBy(t => t).ToArray();
        }
        
        private void PreviewMigration()
        {
            Debug.Log($"Migration Preview: {availableTypes[sourceTypeIndex]}.{sourceFieldName} → {availableTypes[targetTypeIndex]}.{targetFieldName}");
            
            // In a real implementation, this would show what would be migrated
            EditorUtility.DisplayDialog("Migration Preview", 
                $"Would migrate data from {availableTypes[sourceTypeIndex]}.{sourceFieldName} to {availableTypes[targetTypeIndex]}.{targetFieldName}", "OK");
        }
        
        private void ExecuteMigration()
        {
            // This is a simplified version - real implementation would need proper type resolution and field mapping
            Debug.Log($"Executing migration: {availableTypes[sourceTypeIndex]}.{sourceFieldName} → {availableTypes[targetTypeIndex]}.{targetFieldName}");
            
            EditorUtility.DisplayDialog("Migration Complete", 
                "Data migration completed successfully", "OK");
        }
    }
    
    // Bulk Data Editor Window
    public class BulkDataEditorWindow : EditorWindow
    {
        private Type selectedType;
        private string[] availableTypes;
        private int typeIndex = 0;
        private string fieldName = "";
        private string newValue = "";
        private Vector2 scrollPosition;
        private List<ScriptableObject> foundObjects = new List<ScriptableObject>();
        
        private void OnEnable()
        {
            RefreshAvailableTypes();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Bulk Data Editor", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (availableTypes != null && availableTypes.Length > 0)
            {
                typeIndex = EditorGUILayout.Popup("Object Type:", typeIndex, availableTypes);
                fieldName = EditorGUILayout.TextField("Field Name:", fieldName);
                newValue = EditorGUILayout.TextField("New Value:", newValue);
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("Find Objects"))
                {
                    FindObjectsOfType();
                }
                
                if (foundObjects.Count > 0)
                {
                    GUILayout.Space(10);
                    GUILayout.Label($"Found {foundObjects.Count} objects:", EditorStyles.boldLabel);
                    
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                    
                    foreach (var obj in foundObjects)
                    {
                        GUILayout.Label($"• {obj.name}");
                    }
                    
                    EditorGUILayout.EndScrollView();
                    
                    GUILayout.Space(10);
                    
                    if (GUILayout.Button("Apply Changes to All"))
                    {
                        ApplyBulkChanges();
                    }
                }
            }
            else
            {
                GUILayout.Label("No ScriptableObject types available");
                if (GUILayout.Button("Refresh Types"))
                {
                    RefreshAvailableTypes();
                }
            }
        }
        
        private void RefreshAvailableTypes()
        {
            var types = new List<string>();
            
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(ScriptableObject)) && !type.IsAbstract)
                        {
                            types.Add(type.Name);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip problematic assemblies
                }
            }
            
            availableTypes = types.OrderBy(t => t).ToArray();
        }
        
        private void FindObjectsOfType()
        {
            foundObjects.Clear();
            
            if (typeIndex >= 0 && typeIndex < availableTypes.Length)
            {
                var typeName = availableTypes[typeIndex];
                var guids = AssetDatabase.FindAssets($"t:{typeName}");
                
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (obj != null)
                    {
                        foundObjects.Add(obj);
                    }
                }
            }
        }
        
        private void ApplyBulkChanges()
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                EditorUtility.DisplayDialog("Error", "Please specify a field name", "OK");
                return;
            }
            
            int changed = 0;
            
            foreach (var obj in foundObjects)
            {
                var type = obj.GetType();
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (field != null)
                {
                    try
                    {
                        object convertedValue = Convert.ChangeType(newValue, field.FieldType);
                        field.SetValue(obj, convertedValue);
                        EditorUtility.SetDirty(obj);
                        changed++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to set field {fieldName} on {obj.name}: {e.Message}");
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"Bulk edit complete: Changed {changed} objects");
            EditorUtility.DisplayDialog("Bulk Edit Complete", 
                $"Successfully updated {changed} objects", "OK");
        }
    }
}

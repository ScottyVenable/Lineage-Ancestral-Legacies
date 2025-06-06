using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Lineage.Database;
using Lineage.Entities;

namespace Lineage.Editor
{
    /// <summary>
    /// Editor tool to migrate existing PopData ScriptableObjects to GameData Entity system.
    /// </summary>
    public class PopDataMigrationTool : EditorWindow
    {
        private PopData sourcePopData;
        private string entityName = "Migrated Entity";
        private Database.Entity.ID entityID = Database.Entity.ID.Kaari;
        private bool createEntityDataFile = true;
        
        [MenuItem("Lineage/Tools/Pop Data Migration")]
        public static void ShowWindow()
        {
            GetWindow<PopDataMigrationTool>("Pop Data Migration Tool");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Pop Data Migration Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("This tool helps migrate PopData ScriptableObjects to the new GameData Entity system.");
            GUILayout.Space(10);
            
            // Source PopData selection
            GUILayout.Label("Source PopData:", EditorStyles.label);
            sourcePopData = (PopData)EditorGUILayout.ObjectField(sourcePopData, typeof(PopData), false);
            
            GUILayout.Space(10);
            
            // Entity configuration
            GUILayout.Label("Entity Configuration:", EditorStyles.label);
            entityName = EditorGUILayout.TextField("Entity Name:", entityName);
            entityID = (Entity.ID)EditorGUILayout.EnumPopup("Entity ID:", entityID);
            createEntityDataFile = EditorGUILayout.Toggle("Create Entity Data File:", createEntityDataFile);
            
            GUILayout.Space(20);
            
            // Migration button
            GUI.enabled = sourcePopData != null;
            if (GUILayout.Button("Migrate to GameData Entity", GUILayout.Height(30)))
            {
                MigratePopData();
            }
            GUI.enabled = true;
            
            GUILayout.Space(10);
            
            // Batch migration
            GUILayout.Label("Batch Migration:", EditorStyles.boldLabel);
            if (GUILayout.Button("Find and Migrate All PopData Assets"))
            {
                BatchMigrateAllPopData();
            }
            
            GUILayout.Space(10);
            
            // Help text
            EditorGUILayout.HelpBox(
                "Migration Process:\n" +
                "1. Select a PopData asset\n" +
                "2. Configure entity settings\n" +
                "3. Click 'Migrate' to convert to GameData Entity\n" +
                "4. The tool will create entity entries in GameData and optionally save as files\n\n" +
                "Batch migration will find all PopData assets and convert them automatically.",
                MessageType.Info);
        }
        
        private void MigratePopData()
        {
            if (sourcePopData == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a PopData asset to migrate.", "OK");
                return;
            }
            
            try
            {
                Database.Entity migratedEntity = ConvertPopDataToEntity(sourcePopData, entityName, entityID);
                  // Add to GameData database
                if (!GameData.entityDatabase.Contains(migratedEntity))
                {
                    GameData.entityDatabase.Add(migratedEntity);
                }
                
                // Optionally create a data file
                if (createEntityDataFile)
                {
                    CreateEntityDataFile(migratedEntity);
                }
                
                EditorUtility.DisplayDialog("Success", 
                    $"Successfully migrated '{sourcePopData.name}' to GameData Entity '{entityName}'!", "OK");
                    
                UnityEngine.Debug.Log($"Migrated PopData '{sourcePopData.name}' to Entity '{entityName}' (ID: {entityID})");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to migrate PopData: {e.Message}", "OK");
                UnityEngine.Debug.LogError($"Migration failed: {e}");
            }
        }
        
        private void BatchMigrateAllPopData()
        {
            string[] guids = AssetDatabase.FindAssets("t:PopData");
            int migratedCount = 0;
            
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                PopData popData = AssetDatabase.LoadAssetAtPath<PopData>(path);
                
                if (popData != null)
                {
                    try
                    {
                        Entity.ID autoID = (Entity.ID)(migratedCount % System.Enum.GetValues(typeof(Entity.ID)).Length);
                        Database.Entity migratedEntity = ConvertPopDataToEntity(popData, popData.name, autoID);
                          if (!GameData.entityDatabase.Contains(migratedEntity))
                        {
                            GameData.entityDatabase.Add(migratedEntity);
                            migratedCount++;
                        }
                        
                        if (createEntityDataFile)
                        {
                            CreateEntityDataFile(migratedEntity);
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Failed to migrate {popData.name}: {e.Message}");
                    }
                }
                
                EditorUtility.DisplayProgressBar("Migrating PopData", $"Processing {popData?.name ?? "Unknown"}", 
                    (float)i / guids.Length);
            }
            
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Batch Migration Complete", 
                $"Successfully migrated {migratedCount} PopData assets to GameData Entities!", "OK");
        }
        
        private Database.Entity ConvertPopDataToEntity(PopData popData, string name, Database.Entity.ID id)
        {
            // Create health struct from PopData
            Health health = new Health(popData.maxHealth);
            
            // Create entity with basic configuration
            Database.Entity entity = new Database.Entity(
                name: name,
                id: id,
                faction: "Village",
                description: $"Migrated from PopData asset '{popData.name}'",
                rarity: Rarity.Common,
                level: 1,
                aggressionType: Entity.AggressionType.Neutral,
                healthValue: health,
                usesMana: false,
                isAlive: true
            );
            
            // Map PopData values to Entity stats
            entity.stamina = new Stat(Stat.ID.Stamina, "Stamina", popData.maxEnergy);
            
            // Set default stats based on PopData (you can enhance this mapping)
            entity.strength = new Stat(Stat.ID.Strength, "Strength", Random.Range(8f, 12f));
            entity.agility = new Stat(Stat.ID.Agility, "Agility", Random.Range(8f, 12f));
            entity.intelligence = new Stat(Stat.ID.Intelligence, "Intelligence", Random.Range(8f, 12f));
            entity.charisma = new Stat(Stat.ID.Charisma, "Charisma", Random.Range(8f, 12f));
            entity.luck = new Stat(Stat.ID.Luck, "Luck", Random.Range(8f, 12f));
            
            // Convert starting traits if any (this would need implementation based on your trait system)
            if (popData.startingTraits != null)
            {
                foreach (var trait in popData.startingTraits)
                {
                    // Convert TraitSO to GameData Trait
                    // This mapping would depend on your existing trait system
                    UnityEngine.Debug.Log($"TODO: Convert trait {trait.name} to GameData trait system");
                }
            }
            
            // Initialize entity states
            entity.entityType = new List<Entity.EntityType> { Entity.EntityType.PlayerControlled };
            entity.InitializeStates();
            
            return entity;
        }
        
        private void CreateEntityDataFile(Database.Entity entity)
        {
            string fileName = $"Entity_{entity.entityName.Replace(" ", "_")}.json";
            string path = $"Assets/GameData/Entities/{fileName}";
            
            // Ensure directory exists
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // Serialize entity to JSON (simplified - you might want a more robust serialization)
            string json = JsonUtility.ToJson(entity, true);
            System.IO.File.WriteAllText(path, json);
            
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"Created entity data file: {path}");
        }
    }
}

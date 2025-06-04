using UnityEngine;
using UnityEditor;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Database;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Tools
{
    /// <summary>
    /// Migration utility to convert existing Pop GameObjects to use the new generic Entity system.
    /// This tool helps transition from the old Pop-specific scripts to the new modular Entity approach.
    /// </summary>
    public class EntityMigrationTool
    {
        /// <summary>
        /// Convert all Pop objects in the current scene to use the new Entity system
        /// </summary>
        public static void MigrateAllPopsInScene()
        {
            Pop[] allPops = Object.FindObjectsOfType<Pop>();
            List<Entities.Entity> convertedEntities = new List<Entities.Entity>();
            
            UnityEngine.Debug.Log($"Found {allPops.Length} Pop objects to migrate");
            
            foreach (Pop pop in allPops)
            {
                Entities.Entity convertedEntity = MigratePop(pop);
                if (convertedEntity != null)
                {
                    convertedEntities.Add(convertedEntity);
                }
            }
            
            UnityEngine.Debug.Log($"Successfully migrated {convertedEntities.Count} Pops to Entity system");
        }
        
        /// <summary>
        /// Convert a single Pop object to use the Entity system
        /// </summary>
        public static Entities.Entity MigratePop(Pop pop)
        {
            if (pop == null)
            {
                UnityEngine.Debug.LogError("Cannot migrate null Pop object");
                return null;
            }
            
            GameObject popGameObject = pop.gameObject;
            string originalName = pop.popName;
            
            UnityEngine.Debug.Log($"Migrating Pop '{originalName}' to Entity system...");
            
            try
            {
                // Preserve original data
                MigrationData migrationData = ExtractPopData(pop);
                
                // Remove the old Pop component
                Object.DestroyImmediate(pop);
                
                // Add the new Entity component
                Entities.Entity entity = popGameObject.AddComponent<Entities.Entity>();
                
                // Apply the preserved data
                ApplyMigrationData(entity, migrationData);
                
                UnityEngine.Debug.Log($"✓ Successfully migrated '{originalName}' to Entity system");
                return entity;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to migrate Pop '{originalName}': {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Extract data from Pop object before migration
        /// </summary>
        private static MigrationData ExtractPopData(Pop pop)
        {
            return new MigrationData
            {
                name = pop.popName,
                age = pop.age,
                position = pop.transform.position,
                rotation = pop.transform.rotation,
                // Add more data as needed
                hasHealthBar = pop.GetComponentInChildren<UnityEngine.UI.Image>() != null,
                // Extract component data
                hasInventory = pop.GetComponent<Lineage.Ancestral.Legacies.Systems.Inventory.InventoryComponent>() != null,
                hasNavMeshAgent = pop.GetComponent<UnityEngine.AI.NavMeshAgent>() != null,
            };
        }
        
        /// <summary>
        /// Apply preserved data to the new Entity
        /// </summary>
        private static void ApplyMigrationData(Entities.Entity entity, MigrationData data)
        {
            // Configure the Entity with Pop-specific settings
            entity.ConfigureAsEntity(Database.Entity.ID.Pop);
            
            // Apply preserved transform data
            entity.transform.position = data.position;
            entity.transform.rotation = data.rotation;
            
            // Set entity name through EntityData if available
            if (entity.EntityData != null)
            {
                var entityData = entity.EntityData.EntityData;
                entityData.entityName = data.name;
                entity.EntityData.SetEntityData(entityData);
            }
            
            UnityEngine.Debug.Log($"Applied migration data for entity '{data.name}'");
        }
        
        /// <summary>
        /// Data structure to hold Pop information during migration
        /// </summary>
        private struct MigrationData
        {
            public string name;
            public int age;
            public Vector3 position;
            public Quaternion rotation;
            public bool hasHealthBar;
            public bool hasInventory;
            public bool hasNavMeshAgent;
        }
        
        /// <summary>
        /// Validate that migration was successful
        /// </summary>
        public static bool ValidateMigration(Entities.Entity entity)
        {
            if (entity == null) return false;
            if (entity.EntityData == null) return false;
            if (string.IsNullOrEmpty(entity.EntityName)) return false;
            
            // Check that essential stats exist
            try
            {
                var health = entity.EntityData.GetStat(Stat.ID.Health);
                if (health.maxValue <= 0) return false;
                
                // For Pop entities, check Pop-specific stats
                if (entity.EntityData.EntityData.entityID == Database.Entity.ID.Pop)
                {
                    var thirst = entity.EntityData.GetStat(Stat.ID.Thirst);
                    var hunger = entity.EntityData.GetStat(Stat.ID.Hunger);
                    if (thirst.maxValue <= 0 || hunger.maxValue <= 0) return false;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Editor utility for easy migration in Unity Editor
    /// </summary>
    public class EntityMigrationWindow : EditorWindow
    {
        [MenuItem("Lineage/Tools/Entity Migration Tool")]
        public static void ShowWindow()
        {
            GetWindow<EntityMigrationWindow>("Entity Migration");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Entity System Migration Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("This tool helps migrate existing Pop objects to the new Entity system.");
            GUILayout.Space(10);
            
            if (GUILayout.Button("Migrate All Pops in Scene"))
            {
                if (EditorUtility.DisplayDialog("Confirm Migration", 
                    "This will convert all Pop objects in the current scene to use the new Entity system. This action cannot be undone. Continue?", 
                    "Yes, Migrate", "Cancel"))
                {
                    EntityMigrationTool.MigrateAllPopsInScene();
                }
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Find Pops in Scene"))
            {
                Pop[] pops = FindObjectsOfType<Pop>();
                UnityEngine.Debug.Log($"Found {pops.Length} Pop objects in scene:");
                foreach (Pop pop in pops)
                {
                    UnityEngine.Debug.Log($"- {pop.popName} at {pop.transform.position}");
                }
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Validate Entities in Scene"))
            {
                Entities.Entity[] entities = FindObjectsOfType<Entities.Entity>();
                int validCount = 0;
                foreach (Entities.Entity entity in entities)
                {
                    if (EntityMigrationTool.ValidateMigration(entity))
                    {
                        validCount++;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"Entity '{entity.EntityName}' failed validation", entity);
                    }
                }
                UnityEngine.Debug.Log($"Validated {validCount}/{entities.Length} entities successfully");
            }
        }
    }
#endif
}

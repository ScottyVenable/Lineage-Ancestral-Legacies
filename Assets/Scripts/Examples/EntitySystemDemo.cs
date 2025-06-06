using UnityEngine;
using Lineage.Entities;
using Lineage.Database;

namespace Lineage.Examples
{
    /// <summary>
    /// Demonstration script showing how to use the new generic Entity system.
    /// This script creates different types of entities and shows their capabilities.
    /// </summary>
    /// 
    /// todo: update this to use the new system we have.
    public class EntitySystemDemo : MonoBehaviour
    {
        [Header("Entity Type Data Assets")]
        public PopTypeData popTypeData;
        public AnimalTypeData animalTypeData;
        
        [Header("Demo Configuration")]
        public bool autoRunDemo = true;
        public float spawnRadius = 10f;
        
        private EntityData[] demoEntities;
        
        void Start()
        {
            if (autoRunDemo)
            {
                RunEntityDemo();
            }
        }
        
        public void RunEntityDemo()
        {
            Debug.Log("=== Entity System Demonstration ===");
            
            // Create different types of entities
            CreatePopEntities();
            CreateAnimalEntities();
            DemonstrateEntityCapabilities();
        }
        
        void CreatePopEntities()
        {
            Debug.Log("Creating Pop entities...");
            
            // Create a Pop using EntityFactory
            Vector3 popPosition = transform.position + new Vector3(2, 0, 0);
            Entities.Entity pop = EntityFactory.CreateEntity(Database.Entity.ID.Pop, popPosition, popTypeData, "Demo Pop");
            
            if (pop != null)
            {
                Debug.Log($"✓ Created Pop: {pop.EntityName} at {pop.transform.position}");
                
                // Demonstrate Pop-specific capabilities
                if (pop.EntityData != null)
                {
                    Debug.Log($"  - Health: {pop.Health}/{pop.MaxHealth}");
                    Debug.Log($"  - Can Craft: {popTypeData?.canCraft}");
                    Debug.Log($"  - Can Socialize: {popTypeData?.canSocialize}");
                }
            }
        }
        
        void CreateAnimalEntities()
        {
            Debug.Log("Creating Animal entities...");
            
            // Create a Wolf using EntityFactory
            Vector3 wolfPosition = transform.position + new Vector3(-2, 0, 2);
            Entities.Entity wolf = EntityFactory.CreateEntity(Database.Entity.ID.Wolf, wolfPosition, animalTypeData, "Demo Wolf");
            
            if (wolf != null)
            {
                Debug.Log($"✓ Created Wolf: {wolf.EntityName} at {wolf.transform.position}");
                
                // Demonstrate Animal-specific capabilities
                if (wolf.EntityData != null)
                {
                    Debug.Log($"  - Health: {wolf.Health}/{wolf.MaxHealth}");
                    Debug.Log($"  - Can Hunt: {animalTypeData?.canHunt}");
                    Debug.Log($"  - Pack Animal: {animalTypeData?.isPackAnimal}");
                }
            }
            
            // Create a Bear
            Vector3 bearPosition = transform.position + new Vector3(0, 0, 4);
            Entities.Entity bear = EntityFactory.CreateEntity(Database.Entity.ID.Bear, bearPosition, animalTypeData, "Demo Bear");
            
            if (bear != null)
            {
                Debug.Log($"✓ Created Bear: {bear.EntityName} at {bear.transform.position}");
            }
        }
        
        void DemonstrateEntityCapabilities()
        {
            Debug.Log("Demonstrating entity system capabilities...");
            
            // Find all entities in the scene
            Entities.Entity[] allEntities = FindObjectsOfType<Entities.Entity>();
            
            foreach (Entities.Entity entity in allEntities)
            {
                Debug.Log($"Entity: {entity.EntityName} (ID: {entity.EntityData?.EntityData.entityID})");
                
                // Demonstrate stat access
                if (entity.EntityData != null)
                {
                    var healthStat = entity.EntityData.GetStat(Stat.ID.Health);
                    Debug.Log($"  - Health: {healthStat.currentValue}/{healthStat.maxValue}");
                    
                    // Try to access Pop-specific stats
                    try
                    {
                        var hungerStat = entity.EntityData.GetStat(Stat.ID.Hunger);
                        Debug.Log($"  - Hunger: {hungerStat.currentValue}/{hungerStat.maxValue}");
                    }
                    catch
                    {
                        Debug.Log($"  - No hunger stat (not a Pop)");
                    }
                }
            }
        }
        
        public void TestEntityConversion()
        {
            Debug.Log("Testing entity conversion...");
            
            // Find any existing Pop objects
            Pop[] existingPops = FindObjectsOfType<Pop>();
            
            if (existingPops.Length > 0)
            {
                foreach (Pop pop in existingPops)
                {
                    Debug.Log($"Converting Pop '{pop.popName}' to Entity system...");
                    Entities.Entity convertedEntity = EntityFactory.ConvertPopToEntity(pop.gameObject, Database.Entity.ID.Pop, popTypeData);
                    
                    if (convertedEntity != null)
                    {
                        Debug.Log($"✓ Successfully converted to Entity: {convertedEntity.EntityName}");
                    }
                }
            }
            else
            {
                Debug.Log("No existing Pop objects found for conversion test.");
            }
        }
        
        public void TestEntityCreationFromPrefab()
        {
            Debug.Log("Testing entity creation from prefab...");
            
            // This would use a prefab with Entity component
            // GameObject prefab = Resources.Load<GameObject>("Prefabs/EntityPrefab");
            // Entity entity = EntityFactory.CreateEntityFromPrefab(prefab, transform.position, Database.Entity.ID.Pop);
            
            Debug.Log("Prefab creation test not implemented (requires prefab setup)");
        }
        
        void OnGUI()
        {
            // Simple GUI for testing
            GUILayout.BeginArea(new Rect(10, 120, 300, 200));
            
            GUILayout.Label("Entity System Demo", GUI.skin.box);
            
            if (GUILayout.Button("Run Demo"))
            {
                RunEntityDemo();
            }
            
            if (GUILayout.Button("Test Entity Conversion"))
            {
                TestEntityConversion();
            }
            
            if (GUILayout.Button("Test Prefab Creation"))
            {
                TestEntityCreationFromPrefab();
            }
            
            if (GUILayout.Button("Clear All Entities"))
            {
                Entities.Entity[] allEntities = FindObjectsOfType<Entities.Entity>();
                foreach (Entities.Entity entity in allEntities)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(entity.gameObject);
                    }
                }
                Debug.Log($"Cleared {allEntities.Length} entities");
            }
            
            GUILayout.EndArea();
        }
    }
}

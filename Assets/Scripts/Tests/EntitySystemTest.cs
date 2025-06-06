using UnityEngine;
using Lineage.Entities;
using Lineage.Database;
using Lineage.Systems;
using System;

namespace Lineage.Tests
{
    /// <summary>
    /// Test script to validate the new generic Entity system.
    /// This script tests Entity creation, configuration, and type-specific behavior.
    /// </summary>
    public class EntitySystemTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public GameObject entityPrefab; // Prefab with Entity component
        public PopTypeData popTypeData; // ScriptableObject for Pop-specific behavior
        
        [Header("Test Results")]
        public Entities.Entity testEntity;
        public bool testPassed = false;
        
        void Start()
        {
            RunEntitySystemTests();
        }
        
        /// <summary>
        /// Run comprehensive tests of the Entity system
        /// </summary>
        public void RunEntitySystemTests()
        {
            Debug.Log("=== Starting Entity System Tests ===");
            
            try
            {
                // Test 1: Create Entity using EntityFactory
                TestEntityCreation();
                
                // Test 2: Test Entity configuration and data access
                TestEntityConfiguration();
                
                // Test 3: Test type-specific behavior
                TestTypeSpecificBehavior();
                
                // Test 4: Test component integration
                TestComponentIntegration();
                
                testPassed = true;
                Debug.Log("=== All Entity System Tests PASSED! ===");
            }
            catch (System.Exception e)
            {
                testPassed = false;
                Debug.LogError($"Entity System Test FAILED: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        void TestEntityCreation()
        {
            Debug.Log("Test 1: Entity Creation");
            
            // Test creating entity with specific ID
            testEntity = EntityFactory.CreateEntity(Database.Entity.ID.Pop, transform.position);
            
            if (testEntity == null)
            {
                throw new System.Exception("EntityFactory.CreateEntity returned null");
            }
            
            if (testEntity.EntityData == null)
            {
                throw new System.Exception("Entity.EntityData is null after creation");
            }
            
            Debug.Log($"✓ Entity created successfully: {testEntity.EntityName}");
        }
        
        void TestEntityConfiguration()
        {
            Debug.Log("Test 2: Entity Configuration");
            
            // Test that entity has proper data from database
            if (string.IsNullOrEmpty(testEntity.EntityName))
            {
                throw new System.Exception("Entity name is null or empty");
            }
            
            if (testEntity.EntityLevel <= 0)
            {
                throw new System.Exception("Entity level is invalid");
            }
            
            // Test health system
            var healthStat = testEntity.EntityData.GetStat(Stat.ID.Health);
            if (healthStat.maxValue <= 0)
            {
                throw new System.Exception("Entity health max value is invalid");
            }
            
            Debug.Log($"✓ Entity configured properly: Level {testEntity.EntityLevel}, Health {healthStat.currentValue}/{healthStat.maxValue}");
        }
        
        void TestTypeSpecificBehavior()
        {
            Debug.Log("Test 3: Type-Specific Behavior");
            
            // Test Pop-specific behavior if this is a Pop entity
            if (testEntity.EntityData.EntityData.entityID == Database.Entity.ID.Pop)
            {
                // Test that Pop has thirst and hunger stats
                var thirst = testEntity.EntityData.GetStat(Stat.ID.Thirst);
                var hunger = testEntity.EntityData.GetStat(Stat.ID.Hunger);
                
                if (thirst.maxValue <= 0)
                {
                    throw new System.Exception("Pop entity missing thirst stat");
                }
                
                if (hunger.maxValue <= 0)
                {
                    throw new System.Exception("Pop entity missing hunger stat");
                }
                
                Debug.Log($"✓ Pop-specific stats validated: Thirst {thirst.currentValue}/{thirst.maxValue}, Hunger {hunger.currentValue}/{hunger.maxValue}");
            }
        }
        
        void TestComponentIntegration()
        {
            Debug.Log("Test 4: Component Integration");
            
            // Test that required components are present
            if (testEntity.EntityData == null)
            {
                throw new System.Exception("EntityDataComponent not found");
            }
              if (testEntity.Inventory == null && testEntity.GetComponent<Lineage.Systems.Inventory.InventoryComponent>() != null)
            {
                Debug.LogWarning("InventoryComponent present but not assigned to Entity.Inventory");
            }
            
            if (testEntity.Agent == null && testEntity.GetComponent<UnityEngine.AI.NavMeshAgent>() != null)
            {
                Debug.LogWarning("NavMeshAgent present but not assigned to Entity.Agent");
            }
            
            Debug.Log("✓ Component integration verified");
        }
        
        /// <summary>
        /// Test converting an existing Pop to use the new Entity system
        /// </summary>
        public void TestPopToEntityConversion()
        {
            Debug.Log("Testing Pop to Entity conversion...");
              // Find an existing Pop in the scene
            Pop existingPop = FindObjectOfType<Pop>();
            if (existingPop == null)
            {
                Debug.LogWarning("No existing Pop found for conversion test");
                return;
            }
            
            // Convert it using EntityFactory
            Lineage.Entities.Entity convertedEntity = EntityFactory.ConvertPopToEntity(existingPop.gameObject, Database.Entity.ID.Pop);
            
            if (convertedEntity != null)
            {
                Debug.Log($"✓ Successfully converted Pop '{existingPop.popName}' to Entity");
            }
            else
            {
                Debug.LogError("Failed to convert Pop to Entity");
            }
        }
        
        void OnGUI()
        {
            // Simple GUI to show test status
            GUI.Box(new Rect(10, 10, 300, 100), "Entity System Test");
            
            if (testPassed)
            {
                GUI.color = Color.green;
                GUI.Label(new Rect(20, 40, 280, 20), "All tests PASSED!");
            }
            else
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(20, 40, 280, 20), "Tests FAILED - Check console");
            }
            
            GUI.color = Color.white;
            if (GUI.Button(new Rect(20, 60, 100, 25), "Run Tests"))
            {
                RunEntitySystemTests();
            }
            
            if (GUI.Button(new Rect(130, 60, 150, 25), "Test Pop Conversion"))
            {
                TestPopToEntityConversion();
            }
        }
    }
}

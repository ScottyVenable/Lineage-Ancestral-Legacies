using UnityEngine;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Database;
using Lineage.Ancestral.Legacies.Systems;

namespace Lineage.Ancestral.Legacies.Tests
{
    /// <summary>
    /// Simple test script to validate the new generic Entity system.
    /// </summary>
    public class SimpleEntityTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public PopTypeData popTypeData;
        
        [Header("Test Results")]
        public Lineage.Ancestral.Legacies.Entities.Entity testEntity;
        public bool testPassed = false;
        
        void Start()
        {
            RunSimpleTest();
        }
        
        public void RunSimpleTest()
        {
            UnityEngine.Debug.Log("=== Simple Entity System Test ===");
            
            try
            {
                // Test 1: Create Entity using EntityFactory
                testEntity = EntityFactory.CreateEntity(Database.Entity.ID.Pop, transform.position);
                
                if (testEntity == null)
                {
                    throw new System.Exception("EntityFactory.CreateEntity returned null");
                }
                
                if (testEntity.EntityData == null)
                {
                    throw new System.Exception("Entity.EntityData is null after creation");
                }
                
                UnityEngine.Debug.Log($"✓ Entity created successfully: {testEntity.EntityName}");
                
                // Test 2: Check basic functionality
                var healthStat = testEntity.EntityData.GetStat(Stat.ID.Health);
                if (healthStat.maxValue <= 0)
                {
                    throw new System.Exception("Entity health max value is invalid");
                }
                
                UnityEngine.Debug.Log($"✓ Entity health validated: {healthStat.currentValue}/{healthStat.maxValue}");
                
                testPassed = true;
                UnityEngine.Debug.Log("=== Simple Entity Test PASSED! ===");
            }
            catch (System.Exception e)
            {
                testPassed = false;
                UnityEngine.Debug.LogError($"Entity Test FAILED: {e.Message}");
            }
        }
        
        void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 250, 80), "Simple Entity Test");
            
            if (testPassed)
            {
                GUI.color = Color.green;
                GUI.Label(new Rect(20, 40, 220, 20), "Test PASSED!");
            }
            else
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(20, 40, 220, 20), "Test FAILED");
            }
            
            GUI.color = Color.white;
            if (GUI.Button(new Rect(20, 60, 100, 25), "Run Test"))
            {
                RunSimpleTest();
            }
        }
    }
}

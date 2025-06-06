using UnityEngine;
using Lineage.Ancestral.Legacies.Database;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Debug
{
    /// <summary>
    /// Simple test component to validate database functionality and item system.
    /// Attach to a GameObject in a scene to run database tests.
    /// </summary>
    public class DatabaseTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestsOnStart = true;
        public bool logDetailedResults = true;

        void Start()
        {
            if (runTestsOnStart)
            {
                RunDatabaseTests();
            }
        }

        [ContextMenu("Run Database Tests")]
        public void RunDatabaseTests()
        {
            Log.Info("Starting Database System Tests...", Log.LogCategory.Systems);

            // Test 1: Database Initialization
            TestDatabaseInitialization();

            // Test 2: Item Repository Tests
            TestItemRepository();

            // Test 3: GameData API Tests
            TestGameDataAPI();

            // Test 4: Struct System Tests
            TestStructSystem();

            Log.Info("Database System Tests Complete!", Log.LogCategory.Systems);
        }

        private void TestDatabaseInitialization()
        {
            Log.Debug("Testing Database Initialization...", Log.LogCategory.Systems);
            
            try
            {
                GameData.InitializeAllDatabases();
                var counts = GameData.GetDatabaseCounts();
                
                Log.Info($"Database initialized successfully! Found {counts.Count} database types.", Log.LogCategory.Systems);
                
                if (logDetailedResults)
                {
                    foreach (var kvp in counts)
                    {
                        Log.Debug($"  {kvp.Key}: {kvp.Value} entries", Log.LogCategory.Systems);
                    }
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"Database initialization failed: {e.Message}", Log.LogCategory.Systems);
            }
        }

        private void TestItemRepository()
        {
            Log.Debug("Testing Item Repository...", Log.LogCategory.Systems);
            
            try
            {
                // Test getting items by ID (both int and enum)
                var item1 = GameData.GetItemByID(1);
                var item2 = GameData.GetItemByID(Item.ID.IronSword);
                
                Log.Info($"Retrieved item by int ID: {item1.itemName}", Log.LogCategory.Systems);
                Log.Info($"Retrieved item by enum ID: {item2.itemName}", Log.LogCategory.Systems);
                
                // Test getting items by type
                var weapons = ItemRepository.GetItemsByType(ItemType.Weapon);
                Log.Info($"Found {weapons.Count} weapons in database", Log.LogCategory.Systems);
                
                // Test getting items by rarity
                var rareItems = ItemRepository.GetItemsByRarity(ItemRarity.Rare);
                Log.Info($"Found {rareItems.Count} rare items in database", Log.LogCategory.Systems);
            }
            catch (System.Exception e)
            {
                Log.Error($"Item repository test failed: {e.Message}", Log.LogCategory.Systems);
            }
        }

        private void TestGameDataAPI()
        {
            Log.Debug("Testing GameData API backwards compatibility...", Log.LogCategory.Systems);
            
            try
            {
                // Test accessing databases through GameData facade
                int entityCount = GameData.entityDatabase.Count;
                int itemCount = GameData.itemDatabase.Count;
                int skillCount = GameData.skillDatabase.Count;
                
                Log.Info($"GameData API test: Entities={entityCount}, Items={itemCount}, Skills={skillCount}", Log.LogCategory.Systems);
                
                // Test specific retrieval methods
                var entity = GameData.GetEntityByID(1);
                var item = GameData.GetItemByID(Item.ID.HealthPotion);
                
                Log.Info($"Entity retrieval test: {entity.entityName}", Log.LogCategory.Systems);
                Log.Info($"Item retrieval test: {item.itemName}", Log.LogCategory.Systems);
            }
            catch (System.Exception e)
            {
                Log.Error($"GameData API test failed: {e.Message}", Log.LogCategory.Systems);
            }
        }

        private void TestStructSystem()
        {
            Log.Debug("Testing struct-based system...", Log.LogCategory.Systems);
            
            try
            {
                // Test creating new items using struct constructors
                var testItem1 = new Item("Test Item 1", Item.ID.IronSword, ItemType.Weapon, 2.5f, 1, 100);
                var testItem2 = new Item("Test Item 2", 999, ItemType.Miscellaneous, 1.0f, 1, 10);
                
                Log.Info($"Created test item 1: {testItem1.itemName} (ID: {testItem1.itemID})", Log.LogCategory.Systems);
                Log.Info($"Created test item 2: {testItem2.itemName} (ID: {testItem2.itemID})", Log.LogCategory.Systems);
                
                // Test struct equality
                var sameItem = new Item("Test Item 1", Item.ID.IronSword, ItemType.Weapon, 2.5f, 1, 100);
                bool areEqual = testItem1.itemID == sameItem.itemID && testItem1.itemName == sameItem.itemName;
                
                Log.Info($"Struct equality test: {areEqual}", Log.LogCategory.Systems);
                
                // Test equipment slot assignment
                Log.Info($"Item 1 equipment slot: {testItem1.equipSlot}", Log.LogCategory.Systems);
                Log.Info($"Item 2 equipment slot: {testItem2.equipSlot}", Log.LogCategory.Systems);
            }
            catch (System.Exception e)
            {
                Log.Error($"Struct system test failed: {e.Message}", Log.LogCategory.Systems);
            }
        }

        [ContextMenu("Clear All Databases")]
        public void ClearDatabases()
        {
            GameData.ClearAllDatabases();
            Log.Info("All databases cleared.", Log.LogCategory.Systems);
        }

        [ContextMenu("Force Reinitialize")]
        public void ForceReinitialize()
        {
            GameData.ForceReinitialize();
            Log.Info("Database system reinitialized.", Log.LogCategory.Systems);
        }
    }
}

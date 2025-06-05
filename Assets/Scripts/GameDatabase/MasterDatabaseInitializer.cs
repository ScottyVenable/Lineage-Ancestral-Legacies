using UnityEngine;
using System.Collections.Generic;
using Lineage.Ancestral.Legacies.Database;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Central database management class that coordinates all game data repositories.
    /// Provides unified access to all database collections and initialization.
    /// </summary>
    public static class MasterDatabaseInitializer
    {
        #region Initialization State
        private static bool _isInitialized = false;
        
        /// <summary>
        /// Gets whether all databases have been initialized.
        /// </summary>
        public static bool IsInitialized => _isInitialized;
        #endregion

        #region Database Access Properties
        // Entity Data
        public static List<Entity> entityDatabase => EntityRepository.AllEntities;
        public static List<NPC> npcDatabase => EntityRepository.AllNPCs;
        public static List<Population> populationDatabase => EntityRepository.AllPopulations;

        // Item Data
        public static List<Item> itemDatabase => ItemRepository.AllItems;

        // Skill & Buff Data
        public static List<Skill> skillDatabase => SkillRepository.AllSkills;
        public static List<Buff> buffDatabase => SkillRepository.AllBuffs;
        public static List<Stat> statDatabase => SkillRepository.AllStats;
        public static List<Trait> traitDatabase => SkillRepository.AllTraits;
        public static List<LevelingSystem> levelingSystemDatabase => SkillRepository.AllLevelingSystems;

        // Quest Data
        public static List<Quest> questDatabase => QuestRepository.AllQuests;
        public static List<Objective> objectiveDatabase => QuestRepository.AllObjectives;

        // Lore Data
        public static List<LoreEntry> loreDatabase => LoreRepository.AllLoreEntries;
        public static List<JournalEntry> journalDatabase => LoreRepository.AllJournalEntries;

        // Genetics Data
        public static List<Genetics> geneticsDatabase => GeneticsRepository.AllGenetics;        // World Data
        public static List<Location> locationDatabase => WorldRepository.AllLocations;
        public static List<Chunk> chunkDatabase => WorldRepository.AllChunks;

        // State Data
        public static List<State> stateDatabase => StateRepository.AllStates;

        // Legacy compatibility property
        public static List<LoreEntry> LoreEntries => LoreRepository.AllLoreEntries;
        #endregion

        #region Initialization Methods
        /// <summary>
        /// Initializes all game databases. Call this once at game startup.
        /// </summary>
        public static void InitializeAllDatabases()
        {
            if (_isInitialized)
            {
                Log.Warning("MasterDatabaseInitializer: Databases already initialized. Skipping re-initialization.");
                return;
            }

            try
            {
                Log.Info("MasterDatabaseInitializer: Starting database initialization...");                // Initialize all repositories
                EntityRepository.InitializeDatabase();
                ItemRepository.InitializeDatabase();
                SkillRepository.InitializeDatabase();
                QuestRepository.InitializeDatabase();
                LoreRepository.InitializeDatabase();
                GeneticsRepository.InitializeDatabase();
                WorldRepository.InitializeDatabase();
                StateRepository.InitializeDatabase();

                _isInitialized = true;
                Log.Info("MasterDatabaseInitializer: All databases initialized successfully!");
                LogDatabaseStats();
            }
            catch (System.Exception ex)
            {
                Log.Error($"MasterDatabaseInitializer: Failed to initialize databases. Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Clears all database contents. Use with caution!
        /// </summary>
        public static void ClearAllDatabases()
        {
            Log.Warning("MasterDatabaseInitializer: Clearing all databases...");            EntityRepository.AllEntities.Clear();
            EntityRepository.AllNPCs.Clear();
            EntityRepository.AllPopulations.Clear();
            ItemRepository.AllItems.Clear();
            SkillRepository.AllSkills.Clear();
            SkillRepository.AllBuffs.Clear();
            SkillRepository.AllStats.Clear();
            SkillRepository.AllTraits.Clear();
            SkillRepository.AllLevelingSystems.Clear();
            QuestRepository.AllQuests.Clear();
            QuestRepository.AllObjectives.Clear();
            LoreRepository.AllLoreEntries.Clear();
            LoreRepository.AllJournalEntries.Clear();
            GeneticsRepository.AllGenetics.Clear();
            WorldRepository.AllLocations.Clear();
            WorldRepository.AllChunks.Clear();
            StateRepository.AllStates.Clear();

            _isInitialized = false;
            Log.Info("MasterDatabaseInitializer: All databases cleared.");
        }

        /// <summary>
        /// Forces re-initialization of all databases.
        /// </summary>
        public static void ForceReinitialize()
        {
            _isInitialized = false;
            ClearAllDatabases();
            InitializeAllDatabases();
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Logs current database statistics to the console.
        /// </summary>
        public static void LogDatabaseStats()
        {            string stats = "=== Database Statistics ===\n" +
                          $"- Entities: {EntityRepository.AllEntities.Count}\n" +
                          $"- NPCs: {EntityRepository.AllNPCs.Count}\n" +
                          $"- Populations: {EntityRepository.AllPopulations.Count}\n" +
                          $"- Items: {ItemRepository.AllItems.Count}\n" +
                          $"- Skills: {SkillRepository.AllSkills.Count}\n" +
                          $"- Buffs: {SkillRepository.AllBuffs.Count}\n" +
                          $"- Stats: {SkillRepository.AllStats.Count}\n" +
                          $"- Traits: {SkillRepository.AllTraits.Count}\n" +
                          $"- Leveling Systems: {SkillRepository.AllLevelingSystems.Count}\n" +
                          $"- Quests: {QuestRepository.AllQuests.Count}\n" +
                          $"- Objectives: {QuestRepository.AllObjectives.Count}\n" +
                          $"- Lore Entries: {LoreRepository.AllLoreEntries.Count}\n" +
                          $"- Journal Entries: {LoreRepository.AllJournalEntries.Count}\n" +
                          $"- Genetics: {GeneticsRepository.AllGenetics.Count}\n" +
                          $"- Locations: {WorldRepository.AllLocations.Count}\n" +
                          $"- Chunks: {WorldRepository.AllChunks.Count}\n" +
                          $"- States: {StateRepository.AllStates.Count}\n" +
                          "==========================";

            Log.Info(stats);
        }

        /// <summary>
        /// Gets the total number of entries across all databases.
        /// </summary>
        /// <returns>Total count of all database entries.</returns>
        public static int GetTotalDatabaseCount()
        {            return EntityRepository.AllEntities.Count +
                   EntityRepository.AllNPCs.Count +
                   EntityRepository.AllPopulations.Count +
                   ItemRepository.AllItems.Count +
                   SkillRepository.AllSkills.Count +
                   SkillRepository.AllBuffs.Count +
                   SkillRepository.AllStats.Count +
                   SkillRepository.AllTraits.Count +
                   SkillRepository.AllLevelingSystems.Count +
                   QuestRepository.AllQuests.Count +
                   QuestRepository.AllObjectives.Count +
                   LoreRepository.AllLoreEntries.Count +
                   LoreRepository.AllJournalEntries.Count +
                   GeneticsRepository.AllGenetics.Count +
                   WorldRepository.AllLocations.Count +
                   WorldRepository.AllChunks.Count +
                   StateRepository.AllStates.Count;
        }
        #endregion
    }
}
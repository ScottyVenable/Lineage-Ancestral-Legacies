using UnityEngine;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Master database initializer that coordinates all repository initialization.
    /// This replaces the original GameData class functionality.
    /// </summary>
    public static class MasterDatabaseInitializer
    {
        /// <summary>
        /// Indicates whether the databases have been initialized.
        /// </summary>
        public static bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Initializes all game databases in the correct order.
        /// Call this method once during game startup.
        /// </summary>
        public static void InitializeAllDatabases()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("MasterDatabaseInitializer: Databases already initialized. Skipping re-initialization.");
                return;
            }

            Debug.Log("MasterDatabaseInitializer: Starting database initialization...");

            try
            {
                // Initialize databases in dependency order
                // Core enums and types are initialized first (they're static)
                
                // Initialize repositories
                StateRepository.InitializeDatabase();
                SkillRepository.InitializeDatabase();
                ItemRepository.InitializeDatabase();
                GeneticsRepository.InitializeDatabase();
                EntityRepository.InitializeDatabase();
                QuestRepository.InitializeDatabase();
                LoreRepository.InitializeDatabase();
                WorldRepository.InitializeDatabase();

                IsInitialized = true;
                Debug.Log("MasterDatabaseInitializer: All databases initialized successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"MasterDatabaseInitializer: Failed to initialize databases. Error: {ex.Message}");
                IsInitialized = false;
                throw;
            }
        }

        /// <summary>
        /// Clears all databases and resets the initialization state.
        /// Use with caution - this will remove all data!
        /// </summary>
        public static void ClearAllDatabases()
        {
            Debug.LogWarning("MasterDatabaseInitializer: Clearing all databases...");

            // Clear all repository data
            EntityRepository.AllEntities.Clear();
            EntityRepository.AllSettlements.Clear();
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
            
            WorldRepository.AllLocations.Clear();
            WorldRepository.AllChunks.Clear();
            
            GeneticsRepository.AllGenetics.Clear();
            StateRepository.AllStates.Clear();

            IsInitialized = false;
            Debug.Log("MasterDatabaseInitializer: All databases cleared.");
        }

        /// <summary>
        /// Reinitializes all databases (clears then initializes).
        /// </summary>
        public static void ReinitializeAllDatabases()
        {
            ClearAllDatabases();
            InitializeAllDatabases();
        }

        /// <summary>
        /// Gets a summary of database initialization status.
        /// </summary>
        /// <returns>A string containing database status information.</returns>
        public static string GetDatabaseStatus()
        {
            if (!IsInitialized)
            {
                return "Databases not initialized.";
            }

            return $"Database Status:\n" +
                   $"- Entities: {EntityRepository.AllEntities.Count}\n" +
                   $"- Settlements: {EntityRepository.AllSettlements.Count}\n" +
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
                   $"- Locations: {WorldRepository.AllLocations.Count}\n" +
                   $"- Chunks: {WorldRepository.AllChunks.Count}\n" +
                   $"- Genetics: {GeneticsRepository.AllGenetics.Count}\n" +
                   $"- States: {StateRepository.AllStates.Count}";
        }
    }

    /// <summary>
    /// Legacy GameData class for backward compatibility.
    /// This maintains the original static interface while redirecting to the new repositories.
    /// </summary>
    [System.Obsolete("Use the specific repositories (EntityRepository, ItemRepository, etc.) instead of GameData. This class is provided for backward compatibility only.")]
    public static class GameData
    {
        // Legacy database properties for backward compatibility
        public static List<Entity> entityDatabase => EntityRepository.AllEntities;
        public static List<Item> itemDatabase => ItemRepository.AllItems;
        public static List<Buff> buffDatabase => SkillRepository.AllBuffs;
        public static List<Quest> questDatabase => QuestRepository.AllQuests;
        public static List<Objective> objectiveDatabase => QuestRepository.AllObjectives;
        public static List<Stat> statDatabase => SkillRepository.AllStats;
        public static List<Trait> traitDatabase => SkillRepository.AllTraits;
        public static List<NPC> npcDatabase => EntityRepository.AllNPCs;
        public static List<LoreEntry> loreDatabase => LoreRepository.AllLoreEntries;
        public static List<JournalEntry> journalDatabase => LoreRepository.AllJournalEntries;
        public static List<Genetics> geneticsDatabase => GeneticsRepository.AllGenetics;
        public static List<LevelingSystem> levelingSystemDatabase => SkillRepository.AllLevelingSystems;
        public static List<Skill> skillDatabase => SkillRepository.AllSkills;
        public static List<Population> populationDatabase => EntityRepository.AllPopulations;

        // Legacy property for lore entries
        public static List<LoreEntry> LoreEntries
        {
            get => LoreRepository.AllLoreEntries;
            set => LoreRepository.LoreEntries = value;
        }

        /// <summary>
        /// Legacy initialization method - redirects to the new master initializer.
        /// </summary>
        [System.Obsolete("Use MasterDatabaseInitializer.InitializeAllDatabases() instead.")]
        public static void InitializeAllDatabases()
        {
            MasterDatabaseInitializer.InitializeAllDatabases();
        }
    }
}

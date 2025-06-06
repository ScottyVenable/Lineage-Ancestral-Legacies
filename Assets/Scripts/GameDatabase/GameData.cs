using System.Collections.Generic;
using System.Linq;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Backwards-compatible facade for accessing game data via the new repository system.
    /// </summary>
    public static class GameData
    {
        #region Database Accessors
        public static List<Entity> entityDatabase => EntityRepository.AllEntities;
        public static List<NPC> npcDatabase => EntityRepository.AllNPCs;
        public static List<Population> populationDatabase => EntityRepository.AllPopulations;

        public static List<Item> itemDatabase => ItemRepository.AllItems;

        public static List<Skill> skillDatabase => SkillRepository.AllSkills;
        public static List<Buff> buffDatabase => SkillRepository.AllBuffs;
        public static List<Stat> statDatabase => SkillRepository.AllStats;
        public static List<Trait> traitDatabase => SkillRepository.AllTraits;
        public static List<LevelingSystem> levelingSystemDatabase => SkillRepository.AllLevelingSystems;

        public static List<Quest> questDatabase => QuestRepository.AllQuests;
        public static List<Objective> objectiveDatabase => QuestRepository.AllObjectives;

        public static List<LoreEntry> loreDatabase => LoreRepository.AllLoreEntries;
        public static List<JournalEntry> journalDatabase => LoreRepository.AllJournalEntries;

        public static List<Genetics> geneticsDatabase => GeneticsRepository.AllGenetics;

        public static List<Location> locationDatabase => WorldRepository.AllLocations;
        public static List<Chunk> chunkDatabase => WorldRepository.AllChunks;

        public static List<State> stateDatabase => StateRepository.AllStates;
        #endregion

        #region Initialization Helpers
        public static void InitializeAllDatabases() => MasterDatabaseInitializer.InitializeAllDatabases();
        public static void ClearAllDatabases() => MasterDatabaseInitializer.ClearAllDatabases();
        public static void ForceReinitialize() => MasterDatabaseInitializer.ForceReinitialize();
        #endregion

        #region Utility Methods
        public static Dictionary<string, int> GetDatabaseCounts()
        {
            return new Dictionary<string, int>
            {
                {"Entities", EntityRepository.AllEntities.Count},
                {"NPCs", EntityRepository.AllNPCs.Count},
                {"Populations", EntityRepository.AllPopulations.Count},
                {"Items", ItemRepository.AllItems.Count},
                {"Skills", SkillRepository.AllSkills.Count},
                {"Buffs", SkillRepository.AllBuffs.Count},
                {"Stats", SkillRepository.AllStats.Count},
                {"Traits", SkillRepository.AllTraits.Count},
                {"Leveling Systems", SkillRepository.AllLevelingSystems.Count},
                {"Quests", QuestRepository.AllQuests.Count},
                {"Objectives", QuestRepository.AllObjectives.Count},
                {"Lore Entries", LoreRepository.AllLoreEntries.Count},
                {"Journal Entries", LoreRepository.AllJournalEntries.Count},
                {"Genetics", GeneticsRepository.AllGenetics.Count},
                {"Locations", WorldRepository.AllLocations.Count},
                {"Chunks", WorldRepository.AllChunks.Count},
                {"States", StateRepository.AllStates.Count}
            };        }
        #endregion

        #region Data Retrieval
        public static Entity GetEntityByID(int id) => EntityRepository.GetEntityByID(id);
        public static Item GetItemByID(int id) => ItemRepository.GetItemByID(id);
        public static Item GetItemByID(Item.ID id) => ItemRepository.GetItemByID(id);
        public static Trait GetTraitByID(Trait.ID id) => SkillRepository.GetTraitByID(id);
        public static Quest GetQuestByID(Quest.ID id) => QuestRepository.GetQuestByID(id);
        public static NPC GetNPCByName(string name) => EntityRepository.GetNPCByName(name);

        public static List<Entity> GetEntitiesByName(string name, bool exactMatch = true)
        {
            return exactMatch
                ? EntityRepository.AllEntities.Where(e => e.entityName == name).ToList()
                : EntityRepository.AllEntities.Where(e => e.entityName.Contains(name)).ToList();
        }
        #endregion
    }
}

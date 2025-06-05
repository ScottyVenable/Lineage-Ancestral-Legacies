using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Repository for managing quest and objective data.
    /// </summary>
    public static class QuestRepository
    {
        #region Databases
        public static List<Quest> AllQuests { get; private set; } = new List<Quest>();
        public static List<Objective> AllObjectives { get; private set; } = new List<Objective>();
        #endregion

        #region Quest Methods
        /// <summary>
        /// Gets a quest by its ID.
        /// </summary>
        /// <param name="questID">The ID of the quest to retrieve.</param>
        /// <returns>The quest with the specified ID, or default if not found.</returns>
        public static Quest GetQuestByID(Quest.ID questID)
        {
            return AllQuests.FirstOrDefault(q => q.questID == questID);
        }

        /// <summary>
        /// Gets quests by their status.
        /// </summary>
        /// <param name="status">The status to filter by.</param>
        /// <returns>A list of quests with the specified status.</returns>
        public static List<Quest> GetQuestsByStatus(Status status)
        {
            return AllQuests.Where(q => q.status == status).ToList();
        }

        /// <summary>
        /// Gets quests by their type.
        /// </summary>
        /// <param name="questType">The type to filter by.</param>
        /// <returns>A list of quests of the specified type.</returns>
        public static List<Quest> GetQuestsByType(Quest.Type questType)
        {
            return AllQuests.Where(q => q.questType == questType).ToList();
        }

        /// <summary>
        /// Gets active quests (in progress).
        /// </summary>
        /// <returns>A list of active quests.</returns>
        public static List<Quest> GetActiveQuests()
        {
            return AllQuests.Where(q => q.status == Status.InProgress).ToList();
        }

        /// <summary>
        /// Gets completed quests.
        /// </summary>
        /// <returns>A list of completed quests.</returns>
        public static List<Quest> GetCompletedQuests()
        {
            return AllQuests.Where(q => q.status == Status.Completed).ToList();
        }
        #endregion

        #region Objective Methods
        /// <summary>
        /// Gets an objective by its ID.
        /// </summary>
        /// <param name="objectiveID">The ID of the objective to retrieve.</param>
        /// <returns>The objective with the specified ID, or default if not found.</returns>
        public static Objective GetObjectiveByID(Objective.ID objectiveID)
        {
            return AllObjectives.FirstOrDefault(o => o.objectiveID == objectiveID);
        }

        /// <summary>
        /// Gets objectives by their completion status.
        /// </summary>
        /// <param name="isCompleted">Whether to get completed or incomplete objectives.</param>
        /// <returns>A list of objectives with the specified completion status.</returns>
        public static List<Objective> GetObjectivesByStatus(bool isCompleted)
        {
            return AllObjectives.Where(o => o.isCompleted == isCompleted).ToList();
        }

        /// <summary>
        /// Gets objectives by their difficulty level.
        /// </summary>
        /// <param name="difficulty">The difficulty level to filter by.</param>
        /// <returns>A list of objectives with the specified difficulty.</returns>
        public static List<Objective> GetObjectivesByDifficulty(Difficulty difficulty)
        {
            return AllObjectives.Where(o => o.difficultyLevel == difficulty).ToList();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes all quest-related databases with default data.
        /// </summary>
        public static void InitializeDatabase()
        {
            InitializeQuests();
            InitializeObjectives();

            Log.Info("QuestRepository: Database initialized successfully.", Log.LogCategory.Systems);
        }

        private static void InitializeQuests()
        {
            AllQuests.Clear();
            // TODO: Add default quest data initialization
        }

        private static void InitializeObjectives()
        {
            AllObjectives.Clear();
            // TODO: Add default objective data initialization
        }
        #endregion
    }
}

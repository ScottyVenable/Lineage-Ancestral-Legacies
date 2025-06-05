using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        public static List<Quest> GetQuestsByStatus(Quest.Status status)
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
            return AllQuests.Where(q => q.status == Quest.Status.InProgress).ToList();
        }

        /// <summary>
        /// Gets completed quests.
        /// </summary>
        /// <returns>A list of completed quests.</returns>
        public static List<Quest> GetCompletedQuests()
        {
            return AllQuests.Where(q => q.status == Quest.Status.Completed || q.status == Quest.Status.TurnedIn).ToList();
        }

        /// <summary>
        /// Gets available quests (not started but available).
        /// </summary>
        /// <returns>A list of available quests.</returns>
        public static List<Quest> GetAvailableQuests()
        {
            return AllQuests.Where(q => q.status == Quest.Status.Available || q.status == Quest.Status.NotStarted).ToList();
        }

        /// <summary>
        /// Gets main story quests.
        /// </summary>
        /// <returns>A list of main story quests.</returns>
        public static List<Quest> GetMainQuests()
        {
            return AllQuests.Where(q => q.questType == Quest.Type.Main).ToList();
        }

        /// <summary>
        /// Gets side quests.
        /// </summary>
        /// <returns>A list of side quests.</returns>
        public static List<Quest> GetSideQuests()
        {
            return AllQuests.Where(q => q.questType == Quest.Type.Side).ToList();
        }

        /// <summary>
        /// Adds or updates a quest in the database.
        /// </summary>
        /// <param name="quest">The quest to add or update.</param>
        public static void AddOrUpdateQuest(Quest quest)
        {
            var existingIndex = AllQuests.FindIndex(q => q.questID == quest.questID);
            if (existingIndex >= 0)
            {
                AllQuests[existingIndex] = quest;
            }
            else
            {
                AllQuests.Add(quest);
            }
        }

        /// <summary>
        /// Removes a quest from the database.
        /// </summary>
        /// <param name="questID">The ID of the quest to remove.</param>
        /// <returns>True if the quest was removed, false if not found.</returns>
        public static bool RemoveQuest(Quest.ID questID)
        {
            return AllQuests.RemoveAll(q => q.questID == questID) > 0;
        }

        /// <summary>
        /// Starts a quest by setting its status to InProgress.
        /// </summary>
        /// <param name="questID">The ID of the quest to start.</param>
        /// <returns>True if the quest was started, false if not found or cannot be started.</returns>
        public static bool StartQuest(Quest.ID questID)
        {
            var questIndex = AllQuests.FindIndex(q => q.questID == questID);
            if (questIndex >= 0 && AllQuests[questIndex].CanStart)
            {
                var quest = AllQuests[questIndex];
                quest.StartQuest();
                AllQuests[questIndex] = quest;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Completes a quest by setting its status to Completed.
        /// </summary>
        /// <param name="questID">The ID of the quest to complete.</param>
        /// <returns>True if the quest was completed, false if not found.</returns>
        public static bool CompleteQuest(Quest.ID questID)
        {
            var questIndex = AllQuests.FindIndex(q => q.questID == questID);
            if (questIndex >= 0)
            {
                var quest = AllQuests[questIndex];
                quest.CompleteQuest();
                AllQuests[questIndex] = quest;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fails a quest by setting its status to Failed.
        /// </summary>
        /// <param name="questID">The ID of the quest to fail.</param>
        /// <returns>True if the quest was failed, false if not found.</returns>
        public static bool FailQuest(Quest.ID questID)
        {
            var questIndex = AllQuests.FindIndex(q => q.questID == questID);
            if (questIndex >= 0)
            {
                var quest = AllQuests[questIndex];
                quest.FailQuest();
                AllQuests[questIndex] = quest;
                return true;
            }
            return false;
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
        /// Gets objectives by their difficulty.
        /// </summary>
        /// <param name="difficulty">The difficulty to filter by.</param>
        /// <returns>A list of objectives with the specified difficulty.</returns>
        public static List<Objective> GetObjectivesByDifficulty(Objective.Difficulty difficulty)
        {
            return AllObjectives.Where(o => o.difficulty == difficulty).ToList();
        }

        /// <summary>
        /// Gets completed objectives.
        /// </summary>
        /// <returns>A list of completed objectives.</returns>
        public static List<Objective> GetCompletedObjectives()
        {
            return AllObjectives.Where(o => o.isCompleted).ToList();
        }

        /// <summary>
        /// Gets incomplete objectives.
        /// </summary>
        /// <returns>A list of incomplete objectives.</returns>
        public static List<Objective> GetIncompleteObjectives()
        {
            return AllObjectives.Where(o => !o.isCompleted).ToList();
        }

        /// <summary>
        /// Adds or updates an objective in the database.
        /// </summary>
        /// <param name="objective">The objective to add or update.</param>
        public static void AddOrUpdateObjective(Objective objective)
        {
            var existingIndex = AllObjectives.FindIndex(o => o.objectiveID == objective.objectiveID);
            if (existingIndex >= 0)
            {
                AllObjectives[existingIndex] = objective;
            }
            else
            {
                AllObjectives.Add(objective);
            }
        }

        /// <summary>
        /// Removes an objective from the database.
        /// </summary>
        /// <param name="objectiveID">The ID of the objective to remove.</param>
        /// <returns>True if the objective was removed, false if not found.</returns>
        public static bool RemoveObjective(Objective.ID objectiveID)
        {
            return AllObjectives.RemoveAll(o => o.objectiveID == objectiveID) > 0;
        }

        /// <summary>
        /// Updates the progress of an objective.
        /// </summary>
        /// <param name="objectiveID">The ID of the objective to update.</param>
        /// <param name="amount">The amount of progress to add.</param>
        /// <returns>True if the objective was completed by this update, false otherwise.</returns>
        public static bool UpdateObjectiveProgress(Objective.ID objectiveID, int amount = 1)
        {
            var objectiveIndex = AllObjectives.FindIndex(o => o.objectiveID == objectiveID);
            if (objectiveIndex >= 0)
            {
                var objective = AllObjectives[objectiveIndex];
                bool completed = objective.UpdateProgress(amount);
                AllObjectives[objectiveIndex] = objective;
                return completed;
            }
            return false;
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the repository with default data.
        /// </summary>
        public static void Initialize()
        {
            InitializeDefaultQuests();
            InitializeDefaultObjectives();
        }

        private static void InitializeDefaultQuests()
        {
            AllQuests.Clear();
            
            // Add default quests
            AllQuests.Add(new Quest(Quest.ID.MainStory, "The Beginning", "Start your journey in the world of Lineage"));
            AllQuests.Add(new Quest(Quest.ID.Gathering, "Gather Resources", "Collect basic materials for survival"));
            AllQuests.Add(new Quest(Quest.ID.Crafting, "Learn to Craft", "Create your first tool"));
            AllQuests.Add(new Quest(Quest.ID.Exploration, "Explore the World", "Discover new locations"));
            AllQuests.Add(new Quest(Quest.ID.Social, "Meet the Locals", "Interact with NPCs in the area"));
        }

        private static void InitializeDefaultObjectives()
        {
            AllObjectives.Clear();
            
            // Add default objectives
            AllObjectives.Add(new Objective(Objective.ID.Collect, "Collect Wood", "Gather 10 pieces of wood", 10));
            AllObjectives.Add(new Objective(Objective.ID.Craft, "Craft Tool", "Create a basic tool", 1));
            AllObjectives.Add(new Objective(Objective.ID.TalkToNPC, "Talk to Elder", "Speak with the village elder", 1));
            AllObjectives.Add(new Objective(Objective.ID.Explore, "Visit Forest", "Explore the nearby forest", 1));
            AllObjectives.Add(new Objective(Objective.ID.Defeat, "Defeat Wolves", "Kill 3 wolves", 3, Objective.Difficulty.Medium));
        }
        #endregion
    }
}

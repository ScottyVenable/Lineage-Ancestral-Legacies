using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Represents a quest or mission that can be assigned to entities.
    /// </summary>
    public struct Quest
    {
        /// <summary>
        /// The unique identifiers for predefined or hardcoded quests. ID is usually the name of the quest.
        /// This can be used to reference specific quests in the game.
        /// </summary>
        public enum ID
        {
            MainStory = 0,
            SideQuest = 1,
            Exploration = 2,
            Combat = 3,
            Gathering = 4,
            Crafting = 5,
            Social = 6,
            Trade = 7,
            Survival = 8,
            Mystery = 9,
            Escort = 10,
            Delivery = 11,
            Rescue = 12,
            Hunt = 13,
            Build = 14,
            Diplomatic = 15
        }

        public enum Type
        {
            Main,
            Side,
            Daily,
            Weekly,
            Seasonal,
            Event,
            Chain,
            Repeatable,
            Investigation,
            GatherResources,
            KillTarget,
            ExploreArea,
            SocialInteraction,
            CraftItem,
            DeliverItem,
            EscortNPC,
            DefendLocation,
            SurviveTime
        }

        public enum Status
        {
            NotStarted,
            Available,
            InProgress,
            Completed,
            Failed,
            Abandoned,
            TurnedIn
        }

        public ID questID;
        public string questName;
        public string description;
        public Status status;
        public List<Objective> objectives;
        public List<Item> rewards;
        public int experienceReward;
        public int questCompletionPercentage; // Optional: Percentage of quest completion, useful for tracking progress
        public Type questType; // Optional: Type of the quest, useful for categorization or filtering

        public Quest(ID id, string name, string description)
        {
            questID = id;
            questName = name;
            this.description = description;
            status = Status.NotStarted;
            objectives = new List<Objective>(); ///todo: make a struct called Objectives and create objectives that can be added to quests that holds the name, description, and completion status.
            rewards = new List<Item>(); ///todo: add a tag to this item (if not currency) called "objectiveReward" to indicate that this item is a reward for completing the quest and show what quest it was rewarded for.
            experienceReward = 0;
            questCompletionPercentage = 0;
            questType = Type.GatherResources; // Default quest type
        }

        /// <summary>
        /// Starts the quest by setting its status to InProgress.
        /// </summary>
        public void StartQuest()
        {
            if (status == Status.NotStarted || status == Status.Available)
            {
                status = Status.InProgress;
            }
        }

        /// <summary>
        /// Completes the quest by setting its status to Completed.
        /// </summary>
        public void CompleteQuest()
        {
            status = Status.Completed;
            questCompletionPercentage = 100;
        }

        /// <summary>
        /// Fails the quest by setting its status to Failed.
        /// </summary>
        public void FailQuest()
        {
            status = Status.Failed;
        }

        /// <summary>
        /// Abandons the quest by setting its status to Abandoned.
        /// </summary>
        public void AbandonQuest()
        {
            status = Status.Abandoned;
        }

        /// <summary>
        /// Updates the quest completion percentage based on completed objectives.
        /// </summary>
        public void UpdateCompletionPercentage()
        {
            if (objectives == null || objectives.Count == 0)
            {
                questCompletionPercentage = 0;
                return;
            }

            int completedObjectives = 0;
            foreach (var objective in objectives)
            {
                if (objective.isCompleted)
                    completedObjectives++;
            }

            questCompletionPercentage = (completedObjectives * 100) / objectives.Count;

            // Auto-complete quest if all objectives are done
            if (questCompletionPercentage >= 100 && status == Status.InProgress)
            {
                CompleteQuest();
            }
        }

        /// <summary>
        /// Gets whether the quest can be started.
        /// </summary>
        public readonly bool CanStart => status == Status.NotStarted || status == Status.Available;

        /// <summary>
        /// Gets whether the quest is currently active.
        /// </summary>
        public readonly bool IsActive => status == Status.InProgress;

        /// <summary>
        /// Gets whether the quest is completed.
        /// </summary>
        public readonly bool IsCompleted => status == Status.Completed || status == Status.TurnedIn;

        /// <summary>
        /// Gets whether the quest has failed.
        /// </summary>
        public readonly bool HasFailed => status == Status.Failed || status == Status.Abandoned;
    }
}

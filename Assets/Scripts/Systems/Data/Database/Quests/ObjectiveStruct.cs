using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    public struct Objective
    {
        public enum ID
        {
            Collect = 0,
            Defeat = 1,
            Explore = 2,
            Craft = 3,
            Interact = 4,
            TalkToNPC = 5,
            DeliverItem = 6,
            EscortNPC = 7,
            DefendLocation = 8,
            SurviveTime = 9,
            ReachLocation = 10,
            UseSkill = 11,
            GainLevel = 12,
            ObtainItem = 13,
            CompleteQuest = 14,
            KillSpecific = 15
        }

        public enum Difficulty
        {
            Easy = 0,
            Medium = 1,
            Hard = 2,
            Expert = 3,
            Legendary = 4
        }

        public ID objectiveID;
        public string objectiveName;
        public string description;
        public bool isCompleted;
        public int currentProgress;
        public int targetProgress;
        public Difficulty difficulty;
        public List<Item> objectiveReward; // Optional reward item for completing this objective
        public Quest quest; // Optional reference to the quest this objective belongs to
        public List<string> tags; // Optional tags for categorization or special mechanics
        public List<NPC> relatedNPCs; // Optional list of NPCs related to this objective
        public Stat experienceRewardLineage; // Optional experience reward for completing this objective
        public Stat experienceRewardPersonal; // Optional personal experience reward for if an individual pop completes this objective

        /// <summary>
        /// Initializes a new instance of the <see cref="Objective"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the objective.</param>
        /// <param name="name">The name of the objective.</param>
        /// <param name="description">The description of the objective.</param>
        /// <param name="targetProgress">The target progress needed to complete the objective.</param>
        /// <param name="difficulty">The difficulty level of the objective.</param>
        public Objective(ID id, string name, string description, int targetProgress = 1, Difficulty difficulty = Difficulty.Easy)
        {
            objectiveID = id;
            objectiveName = name;
            this.description = description;
            isCompleted = false;
            currentProgress = 0;
            this.targetProgress = targetProgress;
            this.difficulty = difficulty;
            objectiveReward = new List<Item>();
            quest = default;
            tags = new List<string>();
            relatedNPCs = new List<NPC>();
            experienceRewardLineage = default;
            experienceRewardPersonal = default;
        }

        /// <summary>
        /// Updates the progress of this objective.
        /// </summary>
        /// <param name="amount">The amount of progress to add.</param>
        /// <returns>True if the objective was completed by this update, false otherwise.</returns>
        public bool UpdateProgress(int amount = 1)
        {
            if (isCompleted) return false;

            currentProgress += amount;
            if (currentProgress >= targetProgress)
            {
                currentProgress = targetProgress;
                isCompleted = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Completes the objective immediately.
        /// </summary>
        public void CompleteObjective()
        {
            currentProgress = targetProgress;
            isCompleted = true;
        }

        /// <summary>
        /// Resets the objective to its initial state.
        /// </summary>
        public void ResetObjective()
        {
            currentProgress = 0;
            isCompleted = false;
        }

        /// <summary>
        /// Gets the progress as a percentage (0-1).
        /// </summary>
        public readonly float ProgressPercentage => targetProgress > 0 ? (float)currentProgress / targetProgress : 0f;

        /// <summary>
        /// Gets the remaining progress needed to complete the objective.
        /// </summary>
        public readonly int RemainingProgress => Mathf.Max(0, targetProgress - currentProgress);

        /// <summary>
        /// Gets whether this objective is partially completed.
        /// </summary>
        public readonly bool IsPartiallyCompleted => currentProgress > 0 && currentProgress < targetProgress;

        /// <summary>
        /// Gets a formatted string showing the current progress.
        /// </summary>
        public readonly string ProgressString => $"{currentProgress}/{targetProgress}";

        /// <summary>
        /// Gets the difficulty multiplier for experience rewards.
        /// </summary>
        public readonly float DifficultyMultiplier
        {
            get
            {
                return difficulty switch
                {
                    Difficulty.Easy => 1f,
                    Difficulty.Medium => 1.25f,
                    Difficulty.Hard => 1.5f,
                    Difficulty.Expert => 2f,
                    Difficulty.Legendary => 3f,
                    _ => 1f,
                };
            }
        }
    }
}

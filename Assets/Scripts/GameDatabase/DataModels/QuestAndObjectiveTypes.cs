using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    #region Quest System

    /// <summary>
    /// Represents a quest or mission that can be assigned to entities.
    /// </summary>
    public struct Quest
    {
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
            Repeatable
        }

        public ID questID;
        public string questName;
        public string description;
        public Status status;
        public List<Objective> objectives;
        public List<Item> rewards;
        public int experienceReward;
        public int questCompletionPercentage;
        public Type questType;
    }

    public struct Objective
    {
        public enum ID
        {
            Kill = 0,
            Collect = 1,
            Talk = 2,
            Explore = 3,
            Craft = 4,
            Trade = 5,
            Escort = 6,
            Deliver = 7,
            Survive = 8,
            Build = 9,
            Learn = 10,
            Discover = 11,
            Protect = 12,
            Infiltrate = 13,
            Negotiate = 14,
            Investigate = 15
        }

        public ID objectiveID;
        public string objectiveName;
        public string description;
        public bool isCompleted;
        public List<Item> objectiveReward;
        public Quest quest;
        public List<string> tags;
        public List<NPC> relatedNPCs;
        public Stat experienceRewardLineage;
        public Stat experienceRewardPersonal;
        public Difficulty difficultyLevel;
    }

    #endregion
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Represents a skill in the game. Some can be influenced by the Entities stats, items, and buffs.
    /// Skills can be used to perform actions, craft items, or gain experience.
    /// </summary>
    public struct Skill
    {
        /// <summary>
        /// The unique identifiers for predefined or hardcoded skills. ID is usually the name of the skill.
        /// This can be used to reference specific skills in the game.
        /// </summary>
        public enum ID
        {
            Combat,
            Crafting,
            Magic,
            Survival,
            Social,
            Stealth,
            Athletics,
            Medicine,
            Engineering,
            Art,
            Music,
            Leadership,
            Trade,
            Exploration,
            Hunting,
            Fishing,
            Cooking,
            Alchemy,
            Enchanting,
            Blacksmithing
        }

        /// <summary>The unique ID for this skill.</summary>
        public ID skillID;
        /// <summary>The name of the skill (usually matches the type).</summary>
        public SkillType skillName;
        /// <summary>The type or category of this skill.</summary>
        public SkillType skillType;
        /// <summary>The current experience points for this skill.</summary>
        public string skillDescription; // Optional: Description of the skill's effects or uses
        /// <summary>The current experience points for this skill.</summary>
        public float experience;
        /// <summary>The current level of this skill.</summary>
        public int level;
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Skill"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the skill.</param>
        /// <param name="type">The <see cref="SkillType"/> of the skill.</param>
        /// <param name="initialExperience">The initial experience points for the skill.</param>
        /// <param name="initialLevel">The initial level of the skill.</param>
        public Skill(ID id, SkillType type, float initialExperience = 0f, int initialLevel = 1)
        {
            skillID = id;
            skillName = type;
            skillType = type;
            skillDescription = ""; // Default description
            experience = initialExperience;
            level = initialLevel;
            tags = new List<string>();
        }

        /// <summary>
        /// Adds experience to this skill and calculates level progression.
        /// </summary>
        /// <param name="expPoints">The amount of experience to add.</param>
        /// <returns>True if the skill leveled up, false otherwise.</returns>
        public bool AddExperience(float expPoints)
        {
            int oldLevel = level;
            experience += expPoints;
            
            // Simple leveling formula: level = sqrt(experience / 100)
            level = Mathf.FloorToInt(Mathf.Sqrt(experience / 100f)) + 1;
            
            return level > oldLevel;
        }

        /// <summary>
        /// Gets the experience required to reach the next level.
        /// </summary>
        public readonly float ExperienceToNextLevel
        {
            get
            {
                float nextLevelExp = (level * level) * 100f;
                return nextLevelExp - experience;
            }
        }

        /// <summary>
        /// Gets the progress towards the next level as a percentage.
        /// </summary>
        public readonly float LevelProgress
        {
            get
            {
                float currentLevelExp = ((level - 1) * (level - 1)) * 100f;
                float nextLevelExp = (level * level) * 100f;
                return (experience - currentLevelExp) / (nextLevelExp - currentLevelExp);
            }
        }
    }
}

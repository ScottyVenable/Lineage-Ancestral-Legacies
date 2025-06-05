using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Repository for managing skill, buff, and stat data.
    /// </summary>
    public static class SkillRepository
    {
        #region Databases
        public static List<Skill> AllSkills { get; private set; } = new List<Skill>();
        public static List<Buff> AllBuffs { get; private set; } = new List<Buff>();
        public static List<Stat> AllStats { get; private set; } = new List<Stat>();
        #endregion

        #region Skill Methods
        /// <summary>
        /// Gets a skill by its ID.
        /// </summary>
        /// <param name="skillID">The ID of the skill to retrieve.</param>
        /// <returns>The skill with the specified ID, or default if not found.</returns>
        public static Skill GetSkillByID(Skill.ID skillID)
        {
            return AllSkills.FirstOrDefault(s => s.skillID == skillID);
        }

        /// <summary>
        /// Gets skills by their type.
        /// </summary>
        /// <param name="skillType">The type to filter by.</param>
        /// <returns>A list of skills of the specified type.</returns>
        public static List<Skill> GetSkillsByType(SkillType skillType)
        {
            return AllSkills.Where(s => s.skillType == skillType).ToList();
        }

        /// <summary>
        /// Gets skills above a certain level.
        /// </summary>
        /// <param name="minLevel">The minimum level.</param>
        /// <returns>A list of skills at or above the specified level.</returns>
        public static List<Skill> GetSkillsByMinLevel(int minLevel)
        {
            return AllSkills.Where(s => s.level >= minLevel).ToList();
        }

        /// <summary>
        /// Adds or updates a skill in the database.
        /// </summary>
        /// <param name="skill">The skill to add or update.</param>
        public static void AddOrUpdateSkill(Skill skill)
        {
            var existingIndex = AllSkills.FindIndex(s => s.skillID == skill.skillID);
            if (existingIndex >= 0)
            {
                AllSkills[existingIndex] = skill;
            }
            else
            {
                AllSkills.Add(skill);
            }
        }

        /// <summary>
        /// Removes a skill from the database.
        /// </summary>
        /// <param name="skillID">The ID of the skill to remove.</param>
        /// <returns>True if the skill was removed, false if not found.</returns>
        public static bool RemoveSkill(Skill.ID skillID)
        {
            return AllSkills.RemoveAll(s => s.skillID == skillID) > 0;
        }
        #endregion

        #region Buff Methods
        /// <summary>
        /// Gets a buff by its ID.
        /// </summary>
        /// <param name="buffID">The ID of the buff to retrieve.</param>
        /// <returns>The buff with the specified ID, or default if not found.</returns>
        public static Buff GetBuffByID(Buff.ID buffID)
        {
            return AllBuffs.FirstOrDefault(b => b.buffID == buffID);
        }

        /// <summary>
        /// Gets buffs by their type.
        /// </summary>
        /// <param name="buffType">The type to filter by.</param>
        /// <returns>A list of buffs of the specified type.</returns>
        public static List<Buff> GetBuffsByType(BuffType buffType)
        {
            return AllBuffs.Where(b => b.buffType == buffType).ToList();
        }

        /// <summary>
        /// Gets beneficial buffs.
        /// </summary>
        /// <returns>A list of beneficial buffs.</returns>
        public static List<Buff> GetBeneficialBuffs()
        {
            return AllBuffs.Where(b => b.isBeneficial).ToList();
        }

        /// <summary>
        /// Gets detrimental buffs (debuffs).
        /// </summary>
        /// <returns>A list of detrimental buffs.</returns>
        public static List<Buff> GetDetrimentalBuffs()
        {
            return AllBuffs.Where(b => !b.isBeneficial).ToList();
        }

        /// <summary>
        /// Adds or updates a buff in the database.
        /// </summary>
        /// <param name="buff">The buff to add or update.</param>
        public static void AddOrUpdateBuff(Buff buff)
        {
            var existingIndex = AllBuffs.FindIndex(b => b.buffID == buff.buffID);
            if (existingIndex >= 0)
            {
                AllBuffs[existingIndex] = buff;
            }
            else
            {
                AllBuffs.Add(buff);
            }
        }

        /// <summary>
        /// Removes a buff from the database.
        /// </summary>
        /// <param name="buffID">The ID of the buff to remove.</param>
        /// <returns>True if the buff was removed, false if not found.</returns>
        public static bool RemoveBuff(Buff.ID buffID)
        {
            return AllBuffs.RemoveAll(b => b.buffID == buffID) > 0;
        }
        #endregion

        #region Stat Methods
        /// <summary>
        /// Gets a stat by its ID.
        /// </summary>
        /// <param name="statID">The ID of the stat to retrieve.</param>
        /// <returns>The stat with the specified ID, or default if not found.</returns>
        public static Stat GetStatByID(Stat.ID statID)
        {
            return AllStats.FirstOrDefault(s => s.statID == statID);
        }

        /// <summary>
        /// Gets all visible stats.
        /// </summary>
        /// <returns>A list of visible stats.</returns>
        public static List<Stat> GetVisibleStats()
        {
            return AllStats.Where(s => s.isVisible).ToList();
        }

        /// <summary>
        /// Gets all modifiable stats.
        /// </summary>
        /// <returns>A list of modifiable stats.</returns>
        public static List<Stat> GetModifiableStats()
        {
            return AllStats.Where(s => s.isModifiable).ToList();
        }

        /// <summary>
        /// Adds or updates a stat in the database.
        /// </summary>
        /// <param name="stat">The stat to add or update.</param>
        public static void AddOrUpdateStat(Stat stat)
        {
            var existingIndex = AllStats.FindIndex(s => s.statID == stat.statID);
            if (existingIndex >= 0)
            {
                AllStats[existingIndex] = stat;
            }
            else
            {
                AllStats.Add(stat);
            }
        }

        /// <summary>
        /// Removes a stat from the database.
        /// </summary>
        /// <param name="statID">The ID of the stat to remove.</param>
        /// <returns>True if the stat was removed, false if not found.</returns>
        public static bool RemoveStat(Stat.ID statID)
        {
            return AllStats.RemoveAll(s => s.statID == statID) > 0;
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the repository with default data.
        /// </summary>
        public static void Initialize()
        {
            InitializeDefaultSkills();
            InitializeDefaultBuffs();
            InitializeDefaultStats();
        }

        private static void InitializeDefaultSkills()
        {
            AllSkills.Clear();
            
            // Add default skills
            AllSkills.Add(new Skill(Skill.ID.Combat, SkillType.Combat, 0f, 1));
            AllSkills.Add(new Skill(Skill.ID.Crafting, SkillType.Crafting, 0f, 1));
            AllSkills.Add(new Skill(Skill.ID.Magic, SkillType.Magic, 0f, 1));
            AllSkills.Add(new Skill(Skill.ID.Survival, SkillType.Survival, 0f, 1));
            AllSkills.Add(new Skill(Skill.ID.Social, SkillType.Social, 0f, 1));
            AllSkills.Add(new Skill(Skill.ID.Stealth, SkillType.Stealth, 0f, 1));
            AllSkills.Add(new Skill(Skill.ID.Athletics, SkillType.Athletics, 0f, 1));
            AllSkills.Add(new Skill(Skill.ID.Medicine, SkillType.Medicine, 0f, 1));
            AllSkills.Add(new Skill(Skill.ID.Engineering, SkillType.Engineering, 0f, 1));
            AllSkills.Add(new Skill(Skill.ID.Art, SkillType.Art, 0f, 1));
        }

        private static void InitializeDefaultBuffs()
        {
            AllBuffs.Clear();
            
            // Add default buffs
            AllBuffs.Add(new Buff(Buff.ID.HealthRegen, "Health Regeneration", "Gradually restores health over time", BuffType.Temporary, 5f, 60f, true));
            AllBuffs.Add(new Buff(Buff.ID.ManaRegen, "Mana Regeneration", "Gradually restores mana over time", BuffType.Temporary, 3f, 60f, true));
            AllBuffs.Add(new Buff(Buff.ID.StrengthBoost, "Strength Boost", "Increases physical strength", BuffType.Temporary, 10f, 30f, true));
            AllBuffs.Add(new Buff(Buff.ID.SpeedBoost, "Speed Boost", "Increases movement speed", BuffType.Temporary, 1.5f, 20f, true));
            AllBuffs.Add(new Buff(Buff.ID.Poisoned, "Poisoned", "Gradually reduces health over time", BuffType.Temporary, -2f, 45f, false));
            AllBuffs.Add(new Buff(Buff.ID.Cursed, "Cursed", "Reduces all stats", BuffType.Temporary, -5f, 120f, false));
        }

        private static void InitializeDefaultStats()
        {
            AllStats.Clear();
            
            // Add default stats
            AllStats.Add(new Stat(Stat.ID.Health, "Health", "Current health points", 100f, 0f, 1000f));
            AllStats.Add(new Stat(Stat.ID.Mana, "Mana", "Current mana points", 50f, 0f, 500f));
            AllStats.Add(new Stat(Stat.ID.Stamina, "Stamina", "Current stamina points", 100f, 0f, 200f));
            AllStats.Add(new Stat(Stat.ID.Strength, "Strength", "Physical power", 10f, 1f, 100f));
            AllStats.Add(new Stat(Stat.ID.Dexterity, "Dexterity", "Agility and precision", 10f, 1f, 100f));
            AllStats.Add(new Stat(Stat.ID.Intelligence, "Intelligence", "Mental capacity", 10f, 1f, 100f));
            AllStats.Add(new Stat(Stat.ID.Wisdom, "Wisdom", "Perception and insight", 10f, 1f, 100f));
            AllStats.Add(new Stat(Stat.ID.Charisma, "Charisma", "Social influence", 10f, 1f, 100f));
        }
        #endregion
    }
}

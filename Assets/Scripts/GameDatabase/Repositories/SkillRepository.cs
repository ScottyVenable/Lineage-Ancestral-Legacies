using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Repository for managing skill, buff, stat, trait, and leveling system data.
    /// </summary>
    public static class SkillRepository
    {
        #region Databases
        public static List<Skill> AllSkills { get; private set; } = new List<Skill>();
        public static List<Buff> AllBuffs { get; private set; } = new List<Buff>();
        public static List<Stat> AllStats { get; private set; } = new List<Stat>();
        public static List<Trait> AllTraits { get; private set; } = new List<Trait>();
        public static List<LevelingSystem> AllLevelingSystems { get; private set; } = new List<LevelingSystem>();
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
        /// <param name="skillType">The type of skills to retrieve.</param>
        /// <returns>A list of skills of the specified type.</returns>
        public static List<Skill> GetSkillsByType(SkillType skillType)
        {
            return AllSkills.Where(s => s.skillType == skillType).ToList();
        }
        #endregion

        #region Buff Methods
        /// <summary>
        /// Gets a buff by its ID.
        /// </summary>
        /// <param name="buffID">The ID of the buff to retrieve.</param>
        /// <returns>The buff with the specified ID, or default if not found.</returns>
        public static Buff GetBuffByID(int buffID)
        {
            return AllBuffs.FirstOrDefault(b => b.buffID == buffID);
        }

        /// <summary>
        /// Gets buffs by their type.
        /// </summary>
        /// <param name="buffType">The type of buffs to retrieve.</param>
        /// <returns>A list of buffs of the specified type.</returns>
        public static List<Buff> GetBuffsByType(BuffType buffType)
        {
            return AllBuffs.Where(b => b.buffType == buffType).ToList();
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
        /// Gets stats by their type.
        /// </summary>
        /// <param name="statType">The type of stats to retrieve.</param>
        /// <returns>A list of stats of the specified type.</returns>
        public static List<Stat> GetStatsByType(StatType statType)
        {
            return AllStats.Where(s => s.statType == statType).ToList();
        }
        #endregion

        #region Trait Methods
        /// <summary>
        /// Gets a trait by its ID.
        /// </summary>
        /// <param name="traitID">The ID of the trait to retrieve.</param>
        /// <returns>The trait with the specified ID, or default if not found.</returns>
        public static Trait GetTraitByID(Trait.ID traitID)
        {
            return AllTraits.FirstOrDefault(t => t.traitID == traitID);
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes all skill-related databases with default data.
        /// </summary>
        public static void InitializeDatabase()
        {
            InitializeSkills();
            InitializeBuffs();
            InitializeStats();
            InitializeTraits();
            InitializeLevelingSystems();

            Log.Info("SkillRepository: Database initialized successfully.", Log.LogCategory.Systems);
        }

        private static void InitializeSkills()
        {
            AllSkills.Clear();
            // TODO: Add default skill data initialization
        }

        private static void InitializeBuffs()
        {
            AllBuffs.Clear();
            // TODO: Add default buff data initialization
        }

        private static void InitializeStats()
        {
            AllStats.Clear();
            // TODO: Add default stat data initialization
            
            // Example stat initialization
            var healthStat = new Stat
            {
                statID = Stat.ID.Health,
                statName = "Health",
                statDescription = "The life force of an entity",
                statType = StatType.Core,
                baseValue = 100f,
                currentValue = 100f,
                minValue = 0f,
                maxValue = 999f
            };
            
            AllStats.Add(healthStat);
        }

        private static void InitializeTraits()
        {
            AllTraits.Clear();
            // TODO: Add default trait data initialization
        }

        private static void InitializeLevelingSystems()
        {
            AllLevelingSystems.Clear();
            // TODO: Add default leveling system data initialization
        }
        #endregion
    }
}

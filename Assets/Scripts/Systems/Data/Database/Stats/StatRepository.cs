using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Static repository for managing and accessing stat definitions and operations.
    /// Provides CRUD operations, stat templates, and utility methods for stat management.
    /// </summary>
    public static class StatRepository
    {
        #region Private Fields

        private static Dictionary<StatID, Stat> _statTemplates;
        private static Dictionary<string, StatID> _statNameLookup;
        private static bool _isInitialized = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets all available stat templates.
        /// </summary>
        public static IReadOnlyDictionary<StatID, Stat> StatTemplates
        {
            get
            {
                EnsureInitialized();
                return _statTemplates;
            }
        }

        /// <summary>
        /// Gets the total number of registered stat templates.
        /// </summary>
        public static int Count
        {
            get
            {
                EnsureInitialized();
                return _statTemplates.Count;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Ensures the repository is initialized with default stat templates.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initializes the stat repository with default stat templates.
        /// </summary>
        public static void Initialize()
        {
            _statTemplates = new Dictionary<StatID, Stat>();
            _statNameLookup = new Dictionary<string, StatID>();

            // Initialize default stat templates
            InitializeDefaultStats();
            _isInitialized = true;

            Debug.Log($"[StatRepository] Initialized with {_statTemplates.Count} stat templates.");
        }

        /// <summary>
        /// Initializes default stat templates for common game statistics.
        /// </summary>
        private static void InitializeDefaultStats()
        {
            // Primary Stats (Core Resources)
            RegisterStat(new Stat(StatID.Health, "Health", 100f, 0f, 999f, StatType.Primary, 
                "The entity's life force and physical durability."));
            
            RegisterStat(new Stat(StatID.Mana, "Mana", 100f, 0f, 999f, StatType.Primary, 
                "The entity's magical energy used for spellcasting."));
            
            RegisterStat(new Stat(StatID.Stamina, "Stamina", 100f, 0f, 999f, StatType.Primary, 
                "The entity's physical endurance for actions and movement."));

            // Primary Attributes
            RegisterStat(new Stat(StatID.Strength, "Strength", 10f, 1f, 999f, StatType.Secondary, 
                "Physical power affecting damage and carrying capacity."));
            
            RegisterStat(new Stat(StatID.Agility, "Agility", 10f, 1f, 999f, StatType.Secondary, 
                "Speed and dexterity affecting movement and accuracy."));
            
            RegisterStat(new Stat(StatID.Intelligence, "Intelligence", 10f, 1f, 999f, StatType.Secondary, 
                "Mental acuity affecting mana and magical abilities."));
            
            RegisterStat(new Stat(StatID.Charisma, "Charisma", 10f, 1f, 999f, StatType.Secondary, 
                "Social influence and leadership capabilities."));

            // Combat Stats
            RegisterStat(new Stat(StatID.Attack, "Attack Power", 10f, 0f, 9999f, StatType.Secondary, 
                "Base physical damage dealt with attacks."));
            
            RegisterStat(new Stat(StatID.Defense, "Defense", 5f, 0f, 9999f, StatType.Secondary, 
                "Physical damage reduction and armor effectiveness."));
            
            RegisterStat(new Stat(StatID.MagicPower, "Magic Power", 10f, 0f, 9999f, StatType.Secondary, 
                "Base magical damage and spell effectiveness."));
            
            RegisterStat(new Stat(StatID.MagicDefense, "Magic Defense", 5f, 0f, 9999f, StatType.Secondary, 
                "Magical damage reduction and spell resistance."));

            // Advanced Combat Stats
            RegisterStat(new Stat(StatID.Speed, "Speed", 10f, 0f, 999f, StatType.Secondary, 
                "Movement speed and action priority in combat."));
            
            RegisterStat(new Stat(StatID.CriticalHitChance, "Critical Hit Chance", 5f, 0f, 100f, StatType.Secondary, 
                "Percentage chance to deal critical damage."));
            
            RegisterStat(new Stat(StatID.CriticalHitDamage, "Critical Hit Damage", 150f, 100f, 999f, StatType.Secondary, 
                "Damage multiplier for critical hits (as percentage)."));
            
            RegisterStat(new Stat(StatID.Luck, "Luck", 10f, 0f, 999f, StatType.Secondary, 
                "Affects random outcomes and rare item discovery."));

            // Character Progression
            RegisterStat(new Stat(StatID.Level, "Level", 1f, 1f, 999f, StatType.Tertiary, 
                "The entity's current experience level."));
            
            RegisterStat(new Stat(StatID.Experience, "Experience", 0f, 0f, 999999f, StatType.Tertiary, 
                "Accumulated experience points toward next level."));

            // Survival Stats
            RegisterStat(new Stat(StatID.Hunger, "Hunger", 100f, 0f, 100f, StatType.Primary, 
                "The entity's hunger level affecting health and performance."));
            
            RegisterStat(new Stat(StatID.Thirst, "Thirst", 100f, 0f, 100f, StatType.Primary, 
                "The entity's thirst level affecting health and performance."));
            
            RegisterStat(new Stat(StatID.Energy, "Energy", 100f, 0f, 100f, StatType.Primary, 
                "The entity's energy level affecting actions and abilities."));
            
            RegisterStat(new Stat(StatID.Rest, "Rest", 100f, 0f, 100f, StatType.Primary, 
                "The entity's rest level affecting stamina and health regeneration."));
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Registers a stat template in the repository.
        /// </summary>
        /// <param name="stat">The stat template to register.</param>
        /// <returns>True if successfully registered, false if StatID already exists.</returns>
        public static bool RegisterStat(Stat stat)
        {
            EnsureInitialized();
            
            StatID statId = (StatID)stat.statID.id;
            
            if (_statTemplates.ContainsKey(statId))
            {
                Debug.LogWarning($"[StatRepository] Stat with ID {statId} already registered. Use UpdateStat to modify.");
                return false;
            }

            _statTemplates[statId] = stat;
            _statNameLookup[stat.statName.ToLower()] = statId;
            return true;
        }

        /// <summary>
        /// Gets a stat template by StatID.
        /// </summary>
        /// <param name="statId">The StatID to retrieve.</param>
        /// <returns>The stat template if found, otherwise a default stat.</returns>
        public static Stat GetStat(StatID statId)
        {
            EnsureInitialized();
            
            if (_statTemplates.TryGetValue(statId, out Stat stat))
            {
                return stat;
            }

            Debug.LogWarning($"[StatRepository] Stat with ID {statId} not found. Returning default stat.");
            return new Stat(statId, statId.ToString(), 0f, 0f, 100f);
        }

        /// <summary>
        /// Gets a stat template by name (case-insensitive).
        /// </summary>
        /// <param name="statName">The name of the stat to retrieve.</param>
        /// <returns>The stat template if found, otherwise null.</returns>
        public static Stat? GetStatByName(string statName)
        {
            EnsureInitialized();
            
            if (string.IsNullOrEmpty(statName))
                return null;

            if (_statNameLookup.TryGetValue(statName.ToLower(), out StatID statId))
            {
                return _statTemplates[statId];
            }

            return null;
        }

        /// <summary>
        /// Updates an existing stat template.
        /// </summary>
        /// <param name="statId">The StatID to update.</param>
        /// <param name="stat">The new stat data.</param>
        /// <returns>True if successfully updated, false if stat doesn't exist.</returns>
        public static bool UpdateStat(StatID statId, Stat stat)
        {
            EnsureInitialized();
            
            if (!_statTemplates.ContainsKey(statId))
            {
                Debug.LogWarning($"[StatRepository] Cannot update non-existent stat {statId}.");
                return false;
            }

            // Remove old name lookup
            string oldName = _statTemplates[statId].statName.ToLower();
            _statNameLookup.Remove(oldName);

            // Update stat and add new name lookup
            _statTemplates[statId] = stat;
            _statNameLookup[stat.statName.ToLower()] = statId;
            
            return true;
        }

        /// <summary>
        /// Removes a stat template from the repository.
        /// </summary>
        /// <param name="statId">The StatID to remove.</param>
        /// <returns>True if successfully removed, false if stat doesn't exist.</returns>
        public static bool RemoveStat(StatID statId)
        {
            EnsureInitialized();
            
            if (!_statTemplates.TryGetValue(statId, out Stat stat))
            {
                return false;
            }

            _statTemplates.Remove(statId);
            _statNameLookup.Remove(stat.statName.ToLower());
            return true;
        }

        /// <summary>
        /// Checks if a stat template exists in the repository.
        /// </summary>
        /// <param name="statId">The StatID to check.</param>
        /// <returns>True if the stat exists.</returns>
        public static bool HasStat(StatID statId)
        {
            EnsureInitialized();
            return _statTemplates.ContainsKey(statId);
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Gets all stats of a specific type.
        /// </summary>
        /// <param name="statType">The stat type to filter by.</param>
        /// <returns>Collection of stats matching the specified type.</returns>
        public static IEnumerable<Stat> GetStatsByType(StatType statType)
        {
            EnsureInitialized();
            return _statTemplates.Values.Where(stat => stat.statType == statType);
        }

        /// <summary>
        /// Gets all primary stats (core resources like Health, Mana, Stamina).
        /// </summary>
        /// <returns>Collection of primary stats.</returns>
        public static IEnumerable<Stat> GetPrimaryStats()
        {
            return GetStatsByType(StatType.Primary);
        }

        /// <summary>
        /// Gets all secondary stats (attributes and derived stats).
        /// </summary>
        /// <returns>Collection of secondary stats.</returns>
        public static IEnumerable<Stat> GetSecondaryStats()
        {
            return GetStatsByType(StatType.Secondary);
        }

        /// <summary>
        /// Gets all tertiary stats (progression and utility stats).
        /// </summary>
        /// <returns>Collection of tertiary stats.</returns>
        public static IEnumerable<Stat> GetTertiaryStats()
        {
            return GetStatsByType(StatType.Tertiary);
        }

        /// <summary>
        /// Searches for stats by name pattern (case-insensitive).
        /// </summary>
        /// <param name="namePattern">The pattern to search for in stat names.</param>
        /// <returns>Collection of stats with names containing the pattern.</returns>
        public static IEnumerable<Stat> SearchStatsByName(string namePattern)
        {
            EnsureInitialized();
            
            if (string.IsNullOrEmpty(namePattern))
                return Enumerable.Empty<Stat>();

            string pattern = namePattern.ToLower();
            return _statTemplates.Values.Where(stat => stat.statName.ToLower().Contains(pattern));
        }

        /// <summary>
        /// Gets stats within a specific value range.
        /// </summary>
        /// <param name="minValue">Minimum base value.</param>
        /// <param name="maxValue">Maximum base value.</param>
        /// <returns>Collection of stats within the specified range.</returns>
        public static IEnumerable<Stat> GetStatsInRange(float minValue, float maxValue)
        {
            EnsureInitialized();
            return _statTemplates.Values.Where(stat => 
                stat.baseValue >= minValue && stat.baseValue <= maxValue);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Creates a new stat instance from a template with custom values.
        /// </summary>
        /// <param name="statId">The StatID template to use.</param>
        /// <param name="baseValue">Custom base value (uses template default if not specified).</param>
        /// <param name="minValue">Custom minimum value (uses template default if not specified).</param>
        /// <param name="maxValue">Custom maximum value (uses template default if not specified).</param>
        /// <returns>A new stat instance based on the template.</returns>
        public static Stat CreateStatInstance(StatID statId, float? baseValue = null, 
                                            float? minValue = null, float? maxValue = null)
        {
            Stat template = GetStat(statId);
            
            return new Stat(
                statId,
                template.statName,
                baseValue ?? template.baseValue,
                minValue ?? template.minValue,
                maxValue ?? template.maxValue,
                template.statType,
                template.statDescription
            );
        }

        /// <summary>
        /// Creates a complete stat set for a standard entity.
        /// </summary>
        /// <param name="level">The entity level for scaling stats.</param>
        /// <param name="includeSurvival">Whether to include survival stats (hunger, thirst, etc.).</param>
        /// <returns>Dictionary of StatID to Stat instances.</returns>
        public static Dictionary<StatID, Stat> CreateEntityStatSet(int level = 1, bool includeSurvival = false)
        {
            var statSet = new Dictionary<StatID, Stat>();
            
            // Core stats - scale with level
            float levelMultiplier = 1f + (level - 1) * 0.1f; // 10% increase per level
            
            statSet[StatID.Health] = CreateStatInstance(StatID.Health, 100f * levelMultiplier);
            statSet[StatID.Mana] = CreateStatInstance(StatID.Mana, 100f * levelMultiplier);
            statSet[StatID.Stamina] = CreateStatInstance(StatID.Stamina, 100f * levelMultiplier);
            
            // Attributes - moderate scaling
            float attributeBase = 10f + (level - 1) * 2f; // 2 points per level
            statSet[StatID.Strength] = CreateStatInstance(StatID.Strength, attributeBase);
            statSet[StatID.Agility] = CreateStatInstance(StatID.Agility, attributeBase);
            statSet[StatID.Intelligence] = CreateStatInstance(StatID.Intelligence, attributeBase);
            statSet[StatID.Charisma] = CreateStatInstance(StatID.Charisma, attributeBase);
            
            // Combat stats
            statSet[StatID.Attack] = CreateStatInstance(StatID.Attack, 10f + level * 3f);
            statSet[StatID.Defense] = CreateStatInstance(StatID.Defense, 5f + level * 2f);
            statSet[StatID.MagicPower] = CreateStatInstance(StatID.MagicPower, 10f + level * 3f);
            statSet[StatID.MagicDefense] = CreateStatInstance(StatID.MagicDefense, 5f + level * 2f);
            
            // Other stats
            statSet[StatID.Speed] = CreateStatInstance(StatID.Speed, 10f + level);
            statSet[StatID.CriticalHitChance] = CreateStatInstance(StatID.CriticalHitChance, 5f);
            statSet[StatID.CriticalHitDamage] = CreateStatInstance(StatID.CriticalHitDamage, 150f);
            statSet[StatID.Luck] = CreateStatInstance(StatID.Luck, 10f);
            
            // Progression
            statSet[StatID.Level] = CreateStatInstance(StatID.Level, level);
            statSet[StatID.Experience] = CreateStatInstance(StatID.Experience, 0f);
            
            // Survival stats (optional)
            if (includeSurvival)
            {
                statSet[StatID.Hunger] = CreateStatInstance(StatID.Hunger, 100f);
                statSet[StatID.Thirst] = CreateStatInstance(StatID.Thirst, 100f);
                statSet[StatID.Energy] = CreateStatInstance(StatID.Energy, 100f);
                statSet[StatID.Rest] = CreateStatInstance(StatID.Rest, 100f);
            }
            
            return statSet;
        }

        /// <summary>
        /// Gets repository statistics and information.
        /// </summary>
        /// <returns>Formatted string with repository statistics.</returns>
        public static string GetRepositoryStats()
        {
            EnsureInitialized();
            
            var statsByType = _statTemplates.Values.GroupBy(s => s.statType)
                                          .ToDictionary(g => g.Key, g => g.Count());
            
            return $"StatRepository Stats:\n" +
                   $"Total Stats: {_statTemplates.Count}\n" +
                   $"Primary: {statsByType.GetValueOrDefault(StatType.Primary, 0)}\n" +
                   $"Secondary: {statsByType.GetValueOrDefault(StatType.Secondary, 0)}\n" +
                   $"Tertiary: {statsByType.GetValueOrDefault(StatType.Tertiary, 0)}";
        }

        /// <summary>
        /// Clears all registered stats (mainly for testing).
        /// </summary>
        public static void Clear()
        {
            _statTemplates?.Clear();
            _statNameLookup?.Clear();
            _isInitialized = false;
        }

        #endregion
    }
}

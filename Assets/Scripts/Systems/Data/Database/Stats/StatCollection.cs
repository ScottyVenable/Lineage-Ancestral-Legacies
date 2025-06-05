using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// A collection of stats for an entity, providing methods for management, modification, and querying.
    /// This class handles stat collections for individual entities (players, NPCs, etc.).
    /// </summary>
    [System.Serializable]
    public class StatCollection
    {
        #region Fields

        [SerializeField] private Dictionary<StatID, Stat> _stats;
        [SerializeField] private Dictionary<StatID, StatModifiers> _temporaryModifiers;
        [SerializeField] private Dictionary<StatID, StatModifiers> _permanentModifiers;

        #endregion

        #region Properties

        /// <summary>
        /// Gets all stats in this collection.
        /// </summary>
        public IReadOnlyDictionary<StatID, Stat> Stats => _stats;

        /// <summary>
        /// Gets the number of stats in this collection.
        /// </summary>
        public int Count => _stats?.Count ?? 0;

        /// <summary>
        /// Gets or sets a stat by StatID.
        /// </summary>
        /// <param name="statId">The StatID to access.</param>
        /// <returns>The stat if it exists, otherwise a default stat.</returns>
        public Stat this[StatID statId]
        {
            get => GetStat(statId);
            set => SetStat(statId, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new empty StatCollection.
        /// </summary>
        public StatCollection()
        {
            _stats = new Dictionary<StatID, Stat>();
            _temporaryModifiers = new Dictionary<StatID, StatModifiers>();
            _permanentModifiers = new Dictionary<StatID, StatModifiers>();
        }

        /// <summary>
        /// Initializes a StatCollection with a predefined set of stats.
        /// </summary>
        /// <param name="stats">The initial stats to add to the collection.</param>
        public StatCollection(Dictionary<StatID, Stat> stats) : this()
        {
            if (stats != null)
            {
                foreach (var kvp in stats)
                {
                    _stats[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Creates a StatCollection from a stat repository template for a specific level.
        /// </summary>
        /// <param name="level">The entity level for scaling stats.</param>
        /// <param name="includeSurvival">Whether to include survival stats.</param>
        /// <returns>A new StatCollection with default entity stats.</returns>
        public static StatCollection CreateDefault(int level = 1, bool includeSurvival = false)
        {
            var stats = StatRepository.CreateEntityStatSet(level, includeSurvival);
            return new StatCollection(stats);
        }

        #endregion

        #region Basic Stat Operations

        /// <summary>
        /// Gets a stat by StatID.
        /// </summary>
        /// <param name="statId">The StatID to retrieve.</param>
        /// <returns>The stat if found, otherwise a default stat.</returns>
        public Stat GetStat(StatID statId)
        {
            if (_stats.TryGetValue(statId, out Stat stat))
            {
                // Apply modifiers to get the current effective values
                return ApplyModifiersToStat(stat, statId);
            }

            // Return template from repository if not found locally
            return StatRepository.GetStat(statId);
        }

        /// <summary>
        /// Sets a stat in the collection.
        /// </summary>
        /// <param name="statId">The StatID to set.</param>
        /// <param name="stat">The stat data to set.</param>
        public void SetStat(StatID statId, Stat stat)
        {
            _stats[statId] = stat;
        }

        /// <summary>
        /// Adds a stat to the collection if it doesn't already exist.
        /// </summary>
        /// <param name="statId">The StatID to add.</param>
        /// <param name="stat">The stat data to add.</param>
        /// <returns>True if added, false if it already exists.</returns>
        public bool AddStat(StatID statId, Stat stat)
        {
            if (_stats.ContainsKey(statId))
            {
                return false;
            }

            _stats[statId] = stat;
            return true;
        }

        /// <summary>
        /// Removes a stat from the collection.
        /// </summary>
        /// <param name="statId">The StatID to remove.</param>
        /// <returns>True if removed, false if it didn't exist.</returns>
        public bool RemoveStat(StatID statId)
        {
            bool removed = _stats.Remove(statId);
            if (removed)
            {
                // Also remove any modifiers for this stat
                _temporaryModifiers.Remove(statId);
                _permanentModifiers.Remove(statId);
            }
            return removed;
        }

        /// <summary>
        /// Checks if a stat exists in the collection.
        /// </summary>
        /// <param name="statId">The StatID to check.</param>
        /// <returns>True if the stat exists.</returns>
        public bool HasStat(StatID statId)
        {
            return _stats.ContainsKey(statId);
        }

        #endregion

        #region Stat Modification

        /// <summary>
        /// Modifies a stat's current value by a given amount.
        /// </summary>
        /// <param name="statId">The StatID to modify.</param>
        /// <param name="amount">The amount to add (can be negative).</param>
        public void ModifyStatValue(StatID statId, float amount)
        {
            if (_stats.TryGetValue(statId, out Stat stat))
            {
                stat.ModifyStat(amount);
                _stats[statId] = stat;
            }
        }

        /// <summary>
        /// Sets a stat's current value directly.
        /// </summary>
        /// <param name="statId">The StatID to modify.</param>
        /// <param name="value">The new current value.</param>
        public void SetStatValue(StatID statId, float value)
        {
            if (_stats.TryGetValue(statId, out Stat stat))
            {
                stat.SetCurrentValue(value);
                _stats[statId] = stat;
            }
        }

        /// <summary>
        /// Restores a stat to its maximum value.
        /// </summary>
        /// <param name="statId">The StatID to restore.</param>
        public void RestoreStatToMax(StatID statId)
        {
            if (_stats.TryGetValue(statId, out Stat stat))
            {
                stat.RestoreToMax();
                _stats[statId] = stat;
            }
        }

        /// <summary>
        /// Restores all stats to their maximum values.
        /// </summary>
        public void RestoreAllStatsToMax()
        {
            var statIds = _stats.Keys.ToList();
            foreach (var statId in statIds)
            {
                RestoreStatToMax(statId);
            }
        }

        #endregion

        #region Modifier Management

        /// <summary>
        /// Adds a temporary modifier to a stat.
        /// </summary>
        /// <param name="statId">The StatID to modify.</param>
        /// <param name="modifier">The modifier to apply.</param>
        public void AddTemporaryModifier(StatID statId, StatModifiers modifier)
        {
            if (_temporaryModifiers.TryGetValue(statId, out StatModifiers existing))
            {
                _temporaryModifiers[statId] = existing.CombineWith(modifier);
            }
            else
            {
                _temporaryModifiers[statId] = modifier;
            }
        }

        /// <summary>
        /// Adds a permanent modifier to a stat.
        /// </summary>
        /// <param name="statId">The StatID to modify.</param>
        /// <param name="modifier">The modifier to apply.</param>
        public void AddPermanentModifier(StatID statId, StatModifiers modifier)
        {
            if (_permanentModifiers.TryGetValue(statId, out StatModifiers existing))
            {
                _permanentModifiers[statId] = existing.CombineWith(modifier);
            }
            else
            {
                _permanentModifiers[statId] = modifier;
            }
        }

        /// <summary>
        /// Removes all temporary modifiers from a stat.
        /// </summary>
        /// <param name="statId">The StatID to clear modifiers from.</param>
        public void ClearTemporaryModifiers(StatID statId)
        {
            _temporaryModifiers.Remove(statId);
        }

        /// <summary>
        /// Removes all temporary modifiers from all stats.
        /// </summary>
        public void ClearAllTemporaryModifiers()
        {
            _temporaryModifiers.Clear();
        }

        /// <summary>
        /// Gets the combined modifiers (temporary + permanent) for a stat.
        /// </summary>
        /// <param name="statId">The StatID to get modifiers for.</param>
        /// <returns>The combined modifiers.</returns>
        public StatModifiers GetCombinedModifiers(StatID statId)
        {
            var temp = _temporaryModifiers.GetValueOrDefault(statId, new StatModifiers());
            var perm = _permanentModifiers.GetValueOrDefault(statId, new StatModifiers());
            return temp.CombineWith(perm);
        }

        /// <summary>
        /// Applies all modifiers to a stat and returns the modified stat.
        /// </summary>
        /// <param name="baseStat">The base stat to modify.</param>
        /// <param name="statId">The StatID for modifier lookup.</param>
        /// <returns>The stat with modifiers applied.</returns>
        private Stat ApplyModifiersToStat(Stat baseStat, StatID statId)
        {
            var modifiers = GetCombinedModifiers(statId);
            
            // Create a copy of the stat and apply modifiers
            var modifiedStat = baseStat;
            
            // Apply modifiers to the base value and update current value accordingly
            float modifiedBase = modifiers.ApplyTo(baseStat.baseValue);
            modifiedStat.SetBaseValue(modifiedBase, false);
            
            // Also modify the current value if it's not at base value
            float currentRatio = baseStat.baseValue > 0 ? baseStat.currentValue / baseStat.baseValue : 1f;
            float modifiedCurrent = modifiedBase * currentRatio;
            modifiedStat.SetCurrentValue(modifiedCurrent);
            
            return modifiedStat;
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Gets all stats of a specific type.
        /// </summary>
        /// <param name="statType">The stat type to filter by.</param>
        /// <returns>Dictionary of stats matching the specified type.</returns>
        public Dictionary<StatID, Stat> GetStatsByType(StatType statType)
        {
            return _stats.Where(kvp => kvp.Value.statType == statType)
                        .ToDictionary(kvp => kvp.Key, kvp => GetStat(kvp.Key));
        }

        /// <summary>
        /// Gets all primary stats (Health, Mana, Stamina, etc.).
        /// </summary>
        /// <returns>Dictionary of primary stats.</returns>
        public Dictionary<StatID, Stat> GetPrimaryStats()
        {
            return GetStatsByType(StatType.Primary);
        }

        /// <summary>
        /// Gets all secondary stats (attributes and derived stats).
        /// </summary>
        /// <returns>Dictionary of secondary stats.</returns>
        public Dictionary<StatID, Stat> GetSecondaryStats()
        {
            return GetStatsByType(StatType.Secondary);
        }

        /// <summary>
        /// Gets stats that are below a certain percentage of their maximum.
        /// </summary>
        /// <param name="threshold">The percentage threshold (0.0 to 1.0).</param>
        /// <returns>Dictionary of stats below the threshold.</returns>
        public Dictionary<StatID, Stat> GetStatsBelow(float threshold)
        {
            return _stats.Where(kvp => GetStat(kvp.Key).GetPercentage() < threshold)
                        .ToDictionary(kvp => kvp.Key, kvp => GetStat(kvp.Key));
        }

        /// <summary>
        /// Gets stats that are critically low (below 25%).
        /// </summary>
        /// <returns>Dictionary of critically low stats.</returns>
        public Dictionary<StatID, Stat> GetCriticalStats()
        {
            return GetStatsBelow(0.25f);
        }

        /// <summary>
        /// Gets stats that are empty (at minimum value).
        /// </summary>
        /// <returns>Dictionary of empty stats.</returns>
        public Dictionary<StatID, Stat> GetEmptyStats()
        {
            return _stats.Where(kvp => GetStat(kvp.Key).IsEmpty())
                        .ToDictionary(kvp => kvp.Key, kvp => GetStat(kvp.Key));
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the current value of a stat.
        /// </summary>
        /// <param name="statId">The StatID to get the value for.</param>
        /// <returns>The current value of the stat.</returns>
        public float GetStatValue(StatID statId)
        {
            return GetStat(statId).currentValue;
        }

        /// <summary>
        /// Gets the base value of a stat (before modifiers).
        /// </summary>
        /// <param name="statId">The StatID to get the base value for.</param>
        /// <returns>The base value of the stat.</returns>
        public float GetStatBaseValue(StatID statId)
        {
            if (_stats.TryGetValue(statId, out Stat stat))
            {
                return stat.baseValue;
            }
            return 0f;
        }

        /// <summary>
        /// Gets the percentage of a stat (current/max).
        /// </summary>
        /// <param name="statId">The StatID to get the percentage for.</param>
        /// <returns>The percentage (0.0 to 1.0).</returns>
        public float GetStatPercentage(StatID statId)
        {
            return GetStat(statId).GetPercentage();
        }

        /// <summary>
        /// Copies all stats from another StatCollection.
        /// </summary>
        /// <param name="other">The StatCollection to copy from.</param>
        /// <param name="overwrite">Whether to overwrite existing stats.</param>
        public void CopyFrom(StatCollection other, bool overwrite = false)
        {
            if (other?._stats == null) return;

            foreach (var kvp in other._stats)
            {
                if (overwrite || !_stats.ContainsKey(kvp.Key))
                {
                    _stats[kvp.Key] = kvp.Value;
                }
            }

            // Also copy modifiers if overwriting
            if (overwrite)
            {
                foreach (var kvp in other._temporaryModifiers)
                {
                    _temporaryModifiers[kvp.Key] = kvp.Value;
                }
                foreach (var kvp in other._permanentModifiers)
                {
                    _permanentModifiers[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Creates a summary string of all stats in the collection.
        /// </summary>
        /// <param name="includeModifiers">Whether to include modifier information.</param>
        /// <returns>Formatted string with stat information.</returns>
        public string GetSummary(bool includeModifiers = false)
        {
            if (_stats.Count == 0)
            {
                return "No stats in collection.";
            }

            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Stat Collection ({_stats.Count} stats):");

            var groupedStats = _stats.GroupBy(kvp => kvp.Value.statType);
            foreach (var group in groupedStats.OrderBy(g => g.Key))
            {
                summary.AppendLine($"\n{group.Key} Stats:");
                foreach (var kvp in group.OrderBy(x => x.Key))
                {
                    var stat = GetStat(kvp.Key);
                    summary.AppendLine($"  {stat.GetDisplayString()}");
                    
                    if (includeModifiers)
                    {
                        var modifiers = GetCombinedModifiers(kvp.Key);
                        if (modifiers.flatBonus != 0 || modifiers.percentageBonus != 0 || modifiers.multiplier != 1)
                        {
                            summary.AppendLine($"    Modifiers: +{modifiers.flatBonus:F1} flat, " +
                                             $"+{modifiers.percentageBonus * 100:F1}% bonus, " +
                                             $"x{modifiers.multiplier:F2} multiplier");
                        }
                    }
                }
            }

            return summary.ToString();
        }

        /// <summary>
        /// Validates the integrity of the stat collection.
        /// </summary>
        /// <returns>List of validation issues found (empty if valid).</returns>
        public List<string> ValidateIntegrity()
        {
            var issues = new List<string>();

            foreach (var kvp in _stats)
            {
                var stat = kvp.Value;
                
                if (stat.currentValue < stat.minValue)
                {
                    issues.Add($"{kvp.Key}: Current value ({stat.currentValue}) below minimum ({stat.minValue})");
                }
                
                if (stat.currentValue > stat.maxValue)
                {
                    issues.Add($"{kvp.Key}: Current value ({stat.currentValue}) above maximum ({stat.maxValue})");
                }
                
                if (stat.minValue > stat.maxValue)
                {
                    issues.Add($"{kvp.Key}: Minimum value ({stat.minValue}) greater than maximum ({stat.maxValue})");
                }
                
                if (string.IsNullOrEmpty(stat.statName))
                {
                    issues.Add($"{kvp.Key}: Missing stat name");
                }
            }

            return issues;
        }

        #endregion
    }
}

using UnityEngine;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Represents a specific statistic for an entity, such as Health, Mana, or Attack.
    /// Stats have base values, current values, and constraints (min/max).
    /// </summary>
    [System.Serializable]
    public struct Stat
    {
        #region Fields

        /// <summary>The unique identifier for this stat, often corresponds to StatID.</summary>
        public ID statID;
        
        /// <summary>The display name of the stat.</summary>
        public string statName;
        
        /// <summary>A short description of the stat.</summary>
        public string statDescription;
        
        /// <summary>The type or category of this stat.</summary>
        public StatType statType;
        
        /// <summary>The base value of the stat before any modifiers.</summary>
        public float baseValue;
        
        /// <summary>The current value of the stat after modifiers.</summary>
        public float currentValue;
        
        /// <summary>The minimum possible value for this stat.</summary>
        public float minValue;
        
        /// <summary>The maximum possible value for this stat.</summary>
        public float maxValue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Stat"/> struct.
        /// </summary>
        /// <param name="id">The ID of the stat.</param>
        /// <param name="name">The name of the stat.</param>
        /// <param name="baseVal">The base value of the stat.</param>
        /// <param name="minVal">The minimum value of the stat.</param>
        /// <param name="maxVal">The maximum value of the stat.</param>
        /// <param name="type">The type of the stat.</param>
        /// <param name="description">The description of the stat.</param>
        public Stat(ID id, string name, float baseVal = 0f, float minVal = 0f, float maxVal = 100f, 
                   StatType type = StatType.Primary, string description = "")
        {
            statID = id;
            statName = name;
            statDescription = description;
            statType = type;
            baseValue = baseVal;
            currentValue = baseVal;
            minValue = minVal;
            maxValue = maxVal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stat"/> struct using StatID.
        /// </summary>
        /// <param name="statId">The StatID enum value.</param>
        /// <param name="name">The name of the stat.</param>
        /// <param name="baseVal">The base value of the stat.</param>
        /// <param name="minVal">The minimum value of the stat.</param>
        /// <param name="maxVal">The maximum value of the stat.</param>
        /// <param name="type">The type of the stat.</param>
        /// <param name="description">The description of the stat.</param>
        public Stat(StatID statId, string name, float baseVal = 0f, float minVal = 0f, float maxVal = 100f, 
                   StatType type = StatType.Primary, string description = "")
        {
            statID = new ID((int)statId);
            statName = name;
            statDescription = description;
            statType = type;
            baseValue = baseVal;
            currentValue = baseVal;
            minValue = minVal;
            maxValue = maxVal;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current value of the stat as a percentage of its maximum value.
        /// </summary>
        /// <returns>The current value as a percentage (0.0 to 1.0), or 0 if max value is 0.</returns>
        public readonly float GetPercentage()
        {
            return maxValue > 0 ? currentValue / maxValue : 0f;
        }        /// <summary>
        /// Gets the current value of the stat as a percentage of its maximum value, formatted as a string.
        /// </summary>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        /// <returns>Formatted percentage string (e.g., "75.5%").</returns>
        public readonly string GetPercentageString(int decimalPlaces = 1)
        {
            string format = "F" + decimalPlaces;
            return $"{(GetPercentage() * 100f).ToString(format)}%";
        }

        /// <summary>
        /// Modifies the current value of the stat by a given amount, clamped by min/max values.
        /// </summary>
        /// <param name="amount">The amount to add to the current value (can be negative).</param>
        public void ModifyStat(float amount)
        {
            currentValue = Mathf.Clamp(currentValue + amount, minValue, maxValue);
        }

        /// <summary>
        /// Sets the current value of the stat, clamped by min/max values.
        /// </summary>
        /// <param name="value">The new value for the stat.</param>
        public void SetCurrentValue(float value)
        {
            currentValue = Mathf.Clamp(value, minValue, maxValue);
        }

        /// <summary>
        /// Sets the base value of the stat and updates current value accordingly.
        /// </summary>
        /// <param name="value">The new base value for the stat.</param>
        /// <param name="updateCurrent">Whether to also update the current value to match the base value.</param>
        public void SetBaseValue(float value, bool updateCurrent = false)
        {
            baseValue = value;
            if (updateCurrent)
            {
                SetCurrentValue(value);
            }
        }

        /// <summary>
        /// Restores the stat to its maximum value.
        /// </summary>
        public void RestoreToMax()
        {
            currentValue = maxValue;
        }

        /// <summary>
        /// Restores the stat to its base value.
        /// </summary>
        public void RestoreToBase()
        {
            currentValue = baseValue;
        }

        /// <summary>
        /// Checks if the stat is at its maximum value.
        /// </summary>
        /// <returns>True if current value equals max value.</returns>
        public readonly bool IsAtMax()
        {
            return Mathf.Approximately(currentValue, maxValue);
        }

        /// <summary>
        /// Checks if the stat is at its minimum value.
        /// </summary>
        /// <returns>True if current value equals min value.</returns>
        public readonly bool IsAtMin()
        {
            return Mathf.Approximately(currentValue, minValue);
        }

        /// <summary>
        /// Checks if the stat is empty (current value is at minimum).
        /// </summary>
        /// <returns>True if the stat is depleted.</returns>
        public readonly bool IsEmpty()
        {
            return IsAtMin();
        }

        /// <summary>
        /// Checks if the stat is full (current value is at maximum).
        /// </summary>
        /// <returns>True if the stat is at maximum capacity.</returns>
        public readonly bool IsFull()
        {
            return IsAtMax();
        }

        /// <summary>
        /// Gets the amount of the stat that is missing from maximum.
        /// </summary>
        /// <returns>The difference between max and current value.</returns>
        public readonly float GetMissingAmount()
        {
            return maxValue - currentValue;
        }

        /// <summary>
        /// Applies stat modifiers to the base value and returns the modified value.
        /// This does not change the current value - use for calculations only.
        /// </summary>
        /// <param name="modifiers">The stat modifiers to apply.</param>
        /// <returns>The calculated modified value.</returns>
        public readonly float CalculateModifiedValue(StatModifiers modifiers)
        {
            return modifiers.ApplyTo(baseValue);
        }

        /// <summary>
        /// Applies stat modifiers to the current stat permanently.
        /// </summary>
        /// <param name="modifiers">The stat modifiers to apply.</param>
        public void ApplyModifiers(StatModifiers modifiers)
        {
            float modifiedValue = modifiers.ApplyTo(currentValue);
            SetCurrentValue(modifiedValue);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets a user-friendly display string for the stat.
        /// </summary>
        /// <param name="showPercentage">Whether to show percentage instead of raw values.</param>
        /// <returns>Formatted stat string (e.g., "Health: 75/100" or "Health: 75%").</returns>
        public readonly string GetDisplayString(bool showPercentage = false)
        {
            if (showPercentage)
            {
                return $"{statName}: {GetPercentageString()}";
            }
            return $"{statName}: {currentValue:F0}/{maxValue:F0}";
        }

        /// <summary>
        /// Gets a detailed string representation of the stat including all values.
        /// </summary>
        /// <returns>Detailed stat information string.</returns>
        public readonly string GetDetailedString()
        {
            return $"{statName} ({statType}): Current={currentValue:F1}, Base={baseValue:F1}, " +
                   $"Range=[{minValue:F1}, {maxValue:F1}], {GetPercentageString()}";
        }

        /// <summary>
        /// Creates a copy of this stat with a different name.
        /// </summary>
        /// <param name="newName">The new name for the stat copy.</param>
        /// <returns>A new stat instance with the specified name.</returns>
        public readonly Stat WithName(string newName)
        {
            return new Stat(statID, newName, baseValue, minValue, maxValue, statType, statDescription);
        }

        /// <summary>
        /// Creates a copy of this stat with different min/max values.
        /// </summary>
        /// <param name="newMin">The new minimum value.</param>
        /// <param name="newMax">The new maximum value.</param>
        /// <returns>A new stat instance with the specified range.</returns>
        public readonly Stat WithRange(float newMin, float newMax)
        {
            return new Stat(statID, statName, baseValue, newMin, newMax, statType, statDescription);
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Creates a health stat with default values.
        /// </summary>
        /// <param name="maxHealth">The maximum health value.</param>
        /// <returns>A new health stat.</returns>
        public static Stat CreateHealth(float maxHealth = 100f)
        {
            return new Stat(StatID.Health, "Health", maxHealth, 0f, maxHealth, StatType.Primary, 
                          "The entity's life force and durability.");
        }

        /// <summary>
        /// Creates a mana stat with default values.
        /// </summary>
        /// <param name="maxMana">The maximum mana value.</param>
        /// <returns>A new mana stat.</returns>
        public static Stat CreateMana(float maxMana = 100f)
        {
            return new Stat(StatID.Mana, "Mana", maxMana, 0f, maxMana, StatType.Primary, 
                          "The entity's magical energy and spellcasting ability.");
        }

        /// <summary>
        /// Creates a stamina stat with default values.
        /// </summary>
        /// <param name="maxStamina">The maximum stamina value.</param>
        /// <returns>A new stamina stat.</returns>
        public static Stat CreateStamina(float maxStamina = 100f)
        {
            return new Stat(StatID.Stamina, "Stamina", maxStamina, 0f, maxStamina, StatType.Primary, 
                          "The entity's physical endurance and action capacity.");
        }

        /// <summary>
        /// Creates an attribute stat (Strength, Agility, Intelligence, etc.) with default values.
        /// </summary>
        /// <param name="statId">The specific attribute StatID.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="baseValue">The base value of the attribute.</param>
        /// <returns>A new attribute stat.</returns>
        public static Stat CreateAttribute(StatID statId, string name, float baseValue = 10f)
        {
            return new Stat(statId, name, baseValue, 0f, 999f, StatType.Secondary, 
                          $"The entity's {name.ToLower()} attribute.");
        }

        #endregion
    }
}

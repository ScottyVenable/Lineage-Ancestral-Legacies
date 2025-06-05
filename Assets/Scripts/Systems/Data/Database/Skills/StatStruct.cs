using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Represents a specific statistic for an entity, such as Health, Mana, or Attack
    /// </summary>
    public struct Stat
    {
        /// <summary>
        /// The unique identifiers for predefined or hardcoded stats. ID is usually the name of the stat.
        /// This can be used to reference specific stats in the game.
        /// </summary>
        public enum ID
        {
            Health,
            Mana,
            Stamina,
            Strength,
            Dexterity,
            Intelligence,
            Wisdom,
            Charisma,
            Constitution,
            Perception,
            Luck,
            Speed,
            AttackPower,
            Defense,
            MagicPower,
            MagicDefense,
            CriticalChance,
            CriticalDamage,
            Accuracy,
            Evasion
        }

        /// <summary>The unique ID for this stat.</summary>
        public ID statID;
        /// <summary>The name of the stat.</summary>
        public string statName;
        /// <summary>A description of what the stat does.</summary>
        public string statDescription;
        /// <summary>The base value of the stat before any modifiers.</summary>
        public float baseValue;
        /// <summary>The current value of the stat after modifiers.</summary>
        public float currentValue;
        /// <summary>The minimum allowed value for this stat.</summary>
        public float minValue;
        /// <summary>The maximum allowed value for this stat.</summary>
        public float maxValue;
        /// <summary>Whether this stat can be modified by buffs/debuffs.</summary>
        public bool isModifiable;
        /// <summary>Whether this stat is visible to the player.</summary>
        public bool isVisible;
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stat"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the stat.</param>
        /// <param name="name">The name of the stat.</param>
        /// <param name="description">The description of the stat.</param>
        /// <param name="baseValue">The base value of the stat.</param>
        /// <param name="minValue">The minimum allowed value.</param>
        /// <param name="maxValue">The maximum allowed value.</param>
        /// <param name="modifiable">Whether the stat can be modified.</param>
        /// <param name="visible">Whether the stat is visible to the player.</param>
        public Stat(ID id, string name, string description, float baseValue, 
                   float minValue = 0f, float maxValue = float.MaxValue, 
                   bool modifiable = true, bool visible = true)
        {
            statID = id;
            statName = name;
            statDescription = description;
            this.baseValue = baseValue;
            currentValue = baseValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
            isModifiable = modifiable;
            isVisible = visible;
            tags = new List<string>();
        }

        /// <summary>
        /// Applies stat modifiers to calculate the current value.
        /// </summary>
        /// <param name="modifiers">The modifiers to apply.</param>
        public void ApplyModifiers(StatModifiers modifiers)
        {
            if (!isModifiable) return;
            
            currentValue = modifiers.ApplyTo(baseValue);
            currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
        }

        /// <summary>
        /// Sets the base value and updates current value.
        /// </summary>
        /// <param name="value">The new base value.</param>
        public void SetBaseValue(float value)
        {
            baseValue = Mathf.Clamp(value, minValue, maxValue);
            currentValue = baseValue;
        }

        /// <summary>
        /// Modifies the current value directly (for temporary changes).
        /// </summary>
        /// <param name="amount">The amount to add to the current value.</param>
        public void ModifyCurrentValue(float amount)
        {
            currentValue = Mathf.Clamp(currentValue + amount, minValue, maxValue);
        }

        /// <summary>
        /// Gets the difference between current and base value.
        /// </summary>
        public readonly float ModifierValue => currentValue - baseValue;

        /// <summary>
        /// Gets whether this stat is currently modified from its base value.
        /// </summary>
        public readonly bool IsModified => !Mathf.Approximately(currentValue, baseValue);

        /// <summary>
        /// Gets the stat value as a percentage of its maximum.
        /// </summary>
        public readonly float Percentage => maxValue > 0f ? currentValue / maxValue : 0f;

        /// <summary>
        /// Gets whether this stat is at its minimum value.
        /// </summary>
        public readonly bool IsAtMinimum => Mathf.Approximately(currentValue, minValue);

        /// <summary>
        /// Gets whether this stat is at its maximum value.
        /// </summary>
        public readonly bool IsAtMaximum => Mathf.Approximately(currentValue, maxValue);
    }
}

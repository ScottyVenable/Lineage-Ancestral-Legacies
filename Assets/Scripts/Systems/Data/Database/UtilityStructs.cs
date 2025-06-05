using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    #region Utility Structs

    /// <summary>
    /// Represents health data with utility methods for managing current and maximum health.
    /// </summary>
    public struct Health
    {
        /// <summary>
        /// The current health value.
        /// </summary>
        public float current;
        /// <summary>
        /// The maximum possible health value.
        /// </summary>
        public float max;

        /// <summary>
        /// Initializes a new instance of the <see cref="Health"/> struct.
        /// </summary>
        /// <param name="max">The maximum health.</param>
        /// <param name="current">The current health. If less than 0, defaults to max health.</param>
        public Health(float max, float current = -1f)
        {
            this.max = max;
            this.current = current < 0 ? max : current;
        }

        /// <summary>
        /// Gets the health percentage (current / max).
        /// </summary>
        public readonly float Percentage => max > 0 ? current / max : 0f;
        /// <summary>
        /// Gets a value indicating whether the entity is alive (current health > 0).
        /// </summary>
        public readonly bool IsAlive => current > 0f;
        /// <summary>
        /// Gets a value indicating whether the health is above 50%.
        /// </summary>
        public readonly bool IsHealthy => Percentage > 0.5f;
        /// <summary>
        /// Gets a value indicating whether the health is below 25% (critical).
        /// </summary>
        public readonly bool IsCritical => Percentage < 0.25f;

        /// <summary>
        /// Reduces health by the specified damage amount.
        /// </summary>
        /// <param name="damage">The amount of damage to take.</param>
        /// <returns>A new Health struct with the updated health.</returns>
        public readonly Health TakeDamage(float damage)
        {
            return new Health(max, Mathf.Max(0f, current - damage));
        }

        /// <summary>
        /// Increases health by the specified amount, capped at maximum health.
        /// </summary>
        /// <param name="amount">The amount of health to restore.</param>
        /// <returns>A new Health struct with the updated health.</returns>
        public readonly Health Heal(float amount)
        {
            return new Health(max, Mathf.Min(max, current + amount));
        }
    }

    /// <summary>
    /// Represents age data for entities.
    /// </summary>
    public struct Age
    {
        /// <summary>
        /// The current age in years.
        /// </summary>
        public readonly int currentAge;
        /// <summary>
        /// The maximum age this entity can reach.
        /// </summary>
        public readonly int maxAge;
        public StatModifiers modifiers; // Optional modifiers for age-related effects

        /// <summary>
        /// Initializes a new instance of the <see cref="Age"/> struct.
        /// </summary>
        /// <param name="max">The maximum age.</param>
        /// <param name="current">The current age. If less than 0, defaults to 0.</param>
        /// <param name="ageModifiers">Optional modifiers for age-related effects.</param>
        public Age(int max, int current = -1, StatModifiers ageModifiers = default)
        {
            maxAge = max;
            currentAge = current < 0 ? 0 : current;
            modifiers = ageModifiers;
        }

        /// <summary>
        /// Gets the age percentage (current / max).
        /// </summary>
        public readonly float Percentage => maxAge > 0 ? (float)currentAge / maxAge : 0f;
        /// <summary>
        /// Gets a value indicating whether the entity is young (under 25% of max age).
        /// </summary>
        public readonly bool IsYoung => Percentage < 0.25f;
        /// <summary>
        /// Gets a value indicating whether the entity is adult (25-75% of max age).
        /// </summary>
        public readonly bool IsAdult => Percentage >= 0.25f && Percentage <= 0.75f;
        /// <summary>
        /// Gets a value indicating whether the entity is elderly (over 75% of max age).
        /// </summary>
        public readonly bool IsElderly => Percentage > 0.75f;
    }

    /// <summary>
    /// Represents entity size with predefined values for different size categories.
    /// </summary>
    public struct EntitySize
    {
        public enum Category
        {
            Tiny,
            Small,
            Medium,
            Large,
            Huge,
            Gargantuan
        }

        public Category category;
        public float width;
        public float height;
        public float length;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySize"/> struct.
        /// </summary>
        /// <param name="category">The size category.</param>
        public EntitySize(Category category)
        {
            this.category = category;
            switch (category)
            {
                case Category.Tiny:
                    width = height = length = 0.5f;
                    break;
                case Category.Small:
                    width = height = length = 1f;
                    break;
                case Category.Medium:
                    width = height = length = 1.5f;
                    break;
                case Category.Large:
                    width = height = length = 2f;
                    break;
                case Category.Huge:
                    width = height = length = 3f;
                    break;
                case Category.Gargantuan:
                    width = height = length = 4f;
                    break;
                default:
                    width = height = length = 1f;
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySize"/> struct with custom dimensions.
        /// </summary>
        /// <param name="width">The width of the entity.</param>
        /// <param name="height">The height of the entity.</param>
        /// <param name="length">The length of the entity.</param>
        public EntitySize(float width, float height, float length)
        {
            this.width = width;
            this.height = height;
            this.length = length;
            // Determine category based on dimensions
            float maxDimension = Mathf.Max(width, height, length);
            if (maxDimension <= 0.5f) category = Category.Tiny;
            else if (maxDimension <= 1f) category = Category.Small;
            else if (maxDimension <= 1.5f) category = Category.Medium;
            else if (maxDimension <= 2f) category = Category.Large;
            else if (maxDimension <= 3f) category = Category.Huge;
            else category = Category.Gargantuan;
        }

        /// <summary>
        /// Gets the volume of the entity.
        /// </summary>
        public readonly float Volume => width * height * length;
    }

    /// <summary>
    /// Represents stat modifiers for temporary or permanent effects, including flat and percentage bonuses.
    /// </summary>
    public struct StatModifiers
    {
        /// <summary>A flat bonus to add to the stat.</summary>
        public float flatBonus;
        /// <summary>A percentage bonus to multiply the stat by (1.0 = 100% bonus).</summary>
        public float percentageBonus;
        /// <summary>A multiplier to apply to the stat (1.0 = no change).</summary>
        public float multiplier;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatModifiers"/> struct.
        /// </summary>
        /// <param name="flat">The flat bonus to add.</param>
        /// <param name="percentage">The percentage bonus (1.0 = 100% bonus).</param>
        /// <param name="mult">The multiplier to apply.</param>
        public StatModifiers(float flat = 0f, float percentage = 0f, float mult = 1f)
        {
            flatBonus = flat;
            percentageBonus = percentage;
            multiplier = mult;
        }

        /// <summary>
        /// Applies the modifiers to a base value.
        /// </summary>
        /// <param name="baseValue">The base value to modify.</param>
        /// <returns>The modified value.</returns>
        public readonly float ApplyTo(float baseValue)
        {
            return (baseValue + flatBonus) * (1f + percentageBonus) * multiplier;
        }

        /// <summary>
        /// Combines this modifier with another modifier.
        /// </summary>
        /// <param name="other">The other modifier to combine with.</param>
        /// <returns>A new combined modifier.</returns>
        public readonly StatModifiers CombineWith(StatModifiers other)
        {
            return new StatModifiers(
                flatBonus + other.flatBonus,
                percentageBonus + other.percentageBonus,
                multiplier * other.multiplier
            );
        }
    }

    #endregion
}

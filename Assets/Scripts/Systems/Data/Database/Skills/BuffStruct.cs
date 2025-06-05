using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Represents a buff or debuff effect that can be applied to entities.
    /// </summary>
    public struct Buff
    {
        /// <summary>
        /// The unique identifiers for predefined or hardcoded buffs. ID is usually the name of the buff.
        /// This can be used to reference specific buffs in the game.
        /// </summary>
        public enum ID
        {
            HealthRegen,
            ManaRegen,
            StrengthBoost,
            SpeedBoost,
            DefenseBoost,
            AttackBoost,
            Lucky,
            Cursed,
            Poisoned,
            Blessed,
            Stunned,
            Paralyzed,
            Invisible,
            Protected,
            Weakened,
            Empowered
        }

        /// <summary>The unique ID for this buff.</summary>
        public ID buffID;
        /// <summary>The name of the buff.</summary>
        public string buffName;
        /// <summary>A description of what the buff does.</summary>
        public string buffDescription;
        /// <summary>The type or category of this buff.</summary>
        public BuffType buffType;
        /// <summary>The magnitude or strength of the buff effect.</summary>
        public float magnitude;
        /// <summary>The duration of the buff in seconds (0 = permanent).</summary>
        public float duration;
        /// <summary>The remaining time for this buff instance.</summary>
        public float remainingTime;
        /// <summary>Whether this buff is beneficial (true) or detrimental (false).</summary>
        public bool isBeneficial;
        /// <summary>Whether this buff can stack with itself.</summary>
        public bool isStackable;
        /// <summary>The current stack count if stackable.</summary>
        public int stackCount;
        /// <summary>The maximum number of stacks allowed.</summary>
        public int maxStacks;
        /// <summary>The stat modifiers this buff applies.</summary>
        public StatModifiers modifiers;
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Buff"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the buff.</param>
        /// <param name="name">The name of the buff.</param>
        /// <param name="description">The description of the buff.</param>
        /// <param name="type">The <see cref="BuffType"/> of the buff.</param>
        /// <param name="magnitude">The magnitude or strength of the effect.</param>
        /// <param name="duration">The duration in seconds (0 = permanent).</param>
        /// <param name="beneficial">Whether the buff is beneficial.</param>
        /// <param name="stackable">Whether the buff can stack.</param>
        /// <param name="maxStacks">The maximum number of stacks.</param>
        public Buff(ID id, string name, string description, BuffType type, float magnitude, float duration = 0f, 
                   bool beneficial = true, bool stackable = false, int maxStacks = 1)
        {
            buffID = id;
            buffName = name;
            buffDescription = description;
            buffType = type;
            this.magnitude = magnitude;
            this.duration = duration;
            remainingTime = duration;
            isBeneficial = beneficial;
            isStackable = stackable;
            stackCount = 1;
            this.maxStacks = maxStacks;
            modifiers = new StatModifiers();
            tags = new List<string>();
        }

        /// <summary>
        /// Updates the buff, reducing remaining time if applicable.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since last update.</param>
        /// <returns>True if the buff is still active, false if it should be removed.</returns>
        public bool Update(float deltaTime)
        {
            if (duration <= 0f) return true; // Permanent buff
            
            remainingTime -= deltaTime;
            return remainingTime > 0f;
        }

        /// <summary>
        /// Adds a stack to this buff if stackable.
        /// </summary>
        /// <returns>True if a stack was added, false if at max stacks or not stackable.</returns>
        public bool AddStack()
        {
            if (!isStackable || stackCount >= maxStacks) return false;
            
            stackCount++;
            return true;
        }

        /// <summary>
        /// Gets the effective magnitude considering stack count.
        /// </summary>
        public readonly float EffectiveMagnitude => magnitude * stackCount;

        /// <summary>
        /// Gets whether this buff has expired.
        /// </summary>
        public readonly bool IsExpired => duration > 0f && remainingTime <= 0f;

        /// <summary>
        /// Gets the remaining time as a percentage of total duration.
        /// </summary>
        public readonly float RemainingTimePercentage => duration > 0f ? remainingTime / duration : 1f;
    }
}

using UnityEngine;
using Lineage.Ancestral.Legacies.Managers;

namespace Lineage.Ancestral.Legacies.Systems.Needs
{
    /// <summary>
    /// Manages core needs for a pop: hunger, thirst, rest.
    /// </summary>
    [RequireComponent(typeof(MonoBehaviour))]
    public class NeedsComponent : MonoBehaviour
    {        [Header("Needs Settings")]
        public float hunger = 100f;
        public float thirst = 100f;
        public float rest = 100f;
        public float energy = 100f; // Added for save/load compatibility

        [Header("Decay Rates (per second)")]
        public float hungerDecayRate = 1f;
        public float thirstDecayRate = 1.2f;
        public float restDecayRate = 0.5f;
        public float energyDecayRate = 0.8f;

        /// <summary>
        /// Update needs over time.
        /// </summary>
        public void UpdateNeeds(float deltaTime)
        {
            // Consume food if available, otherwise hunger decreases
            if (ResourceManager.Instance != null && ResourceManager.Instance.currentFood > 0)
            {
                float foodToConsume = ResourceManager.Instance.foodConsumptionRate * deltaTime;
                if (ResourceManager.Instance.ConsumeFood(foodToConsume))
                {
                    // Restore some hunger when eating
                    hunger = Mathf.Clamp(hunger + (foodToConsume * 10f), 0, 100);
                }
                else
                {
                    // No food available, hunger decreases
                    hunger = Mathf.Clamp(hunger - hungerDecayRate * deltaTime, 0, 100);
                }
            }
            else
            {
                // No food available, hunger decreases faster
                hunger = Mathf.Clamp(hunger - hungerDecayRate * deltaTime * 2f, 0, 100);
            }            thirst = Mathf.Clamp(thirst - thirstDecayRate * deltaTime, 0, 100);
            rest = Mathf.Clamp(rest - restDecayRate * deltaTime, 0, 100);
            energy = Mathf.Clamp(energy - energyDecayRate * deltaTime, 0, 100);

            // TODO: Trigger events or state transitions when below thresholds
        }

        /// <summary>
        /// Check if basic needs are met for faith generation.
        /// </summary>
        public bool AreBasicNeedsMet()
        {
            return hunger > 30f && thirst > 30f && energy > 20f;
        }

        /// <summary>
        /// Satisfy a need through foraging or other means.
        /// </summary>
        public void SatisfyHunger(float amount)
        {
            hunger = Mathf.Clamp(hunger + amount, 0, 100);
        }

        public void SatisfyThirst(float amount)
        {
            thirst = Mathf.Clamp(thirst + amount, 0, 100);
        }

        public void SatisfyRest(float amount)
        {
            rest = Mathf.Clamp(rest + amount, 0, 100);
        }

        public void SatisfyEnergy(float amount)
        {
            energy = Mathf.Clamp(energy + amount, 0, 100);
        }
    }
}

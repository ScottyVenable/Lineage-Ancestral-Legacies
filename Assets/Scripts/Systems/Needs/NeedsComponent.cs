using UnityEngine;
using Lineage.Ancestral.Legacies.Managers;

namespace Lineage.Ancestral.Legacies.Systems.Needs
{
    /// <summary>
    /// Component that manages Pop needs like hunger, thirst, and energy.
    /// </summary>
    public class NeedsComponent : MonoBehaviour
    {
        [Header("Current Needs")]
        public float hunger = 100f;
        public float thirst = 100f;
        public float energy = 100f;
        public float rest = 100f;

        [Header("Decay Rates (per second)")]
        public float hungerDecayRate = 1f;
        public float thirstDecayRate = 1.5f;
        public float energyDecayRate = 0.8f;
        public float restDecayRate = 0.5f;

        private void Update()
        {
            // Decay needs over time
            hunger = Mathf.Max(0, hunger - hungerDecayRate * Time.deltaTime);
            thirst = Mathf.Max(0, thirst - thirstDecayRate * Time.deltaTime);
            energy = Mathf.Max(0, energy - energyDecayRate * Time.deltaTime);
            rest = Mathf.Max(0, rest - restDecayRate * Time.deltaTime);
        }

        /// <summary>
        /// Satisfies hunger by the given amount.
        /// </summary>
        public void EatFood(float amount)
        {
            hunger = Mathf.Min(100f, hunger + amount);
        }

        /// <summary>
        /// Satisfies thirst by the given amount.
        /// </summary>
        public void DrinkWater(float amount)
        {
            thirst = Mathf.Min(100f, thirst + amount);
        }

        /// <summary>
        /// Restores energy by the given amount.
        /// </summary>
        public void RestoreEnergy(float amount)
        {
            energy = Mathf.Min(100f, energy + amount);
        }

        /// <summary>
        /// Restores rest by the given amount.
        /// </summary>
        public void Sleep(float amount)
        {
            rest = Mathf.Min(100f, rest + amount);
        }
    }
}

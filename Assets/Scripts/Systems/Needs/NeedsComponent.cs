using UnityEngine;

namespace Lineage.Ancestral.Legacies.Systems.Needs
{
    /// <summary>
    /// Manages core needs for a pop: hunger, thirst, rest.
    /// </summary>
    [RequireComponent(typeof(MonoBehaviour))]
    public class NeedsComponent : MonoBehaviour
    {
        [Header("Needs Settings")]
        public float hunger = 100f;
        public float thirst = 100f;
        public float rest = 100f;

        [Header("Decay Rates (per second)")]
        public float hungerDecayRate = 1f;
        public float thirstDecayRate = 1.2f;
        public float restDecayRate = 0.5f;

        /// <summary>
        /// Update needs over time.
        /// </summary>
        public void UpdateNeeds(float deltaTime)
        {
            hunger = Mathf.Clamp(hunger - hungerDecayRate * deltaTime, 0, 100);
            thirst = Mathf.Clamp(thirst - thirstDecayRate * deltaTime, 0, 100);
            rest = Mathf.Clamp(rest - restDecayRate * deltaTime, 0, 100);

            // TODO: Trigger events or state transitions when below thresholds
        }
    }
}

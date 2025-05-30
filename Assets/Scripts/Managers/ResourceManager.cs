using UnityEngine;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Managers
{
    /// <summary>
    /// Global resource management for food, faith points, and other resources.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        // Constants for default values
        private const float DEFAULT_GIFT_FAITH_COST = 5f;
        private const float DEFAULT_GIFT_FOOD_GAIN = 20f;
        private const float DEFAULT_RESEARCH_FAITH_COST = 15f;
        private const float EFFICIENT_GATHERING_MULTIPLIER = 1.5f;

        [Header("Resources")]
        public float currentFood = 50f;
        public float currentFaithPoints = 10f;
        public float currentWood = 0f;

        [Header("Settings")]
        public float foodConsumptionRate = 1f; // Per pop per second
        public float faithGenerationRate = 0.5f; // Per pop per second when needs met

        public bool hasEfficientGathering = false; // Technology upgrade

        // Events for UI updates
        public System.Action<float> OnFoodChanged;
        public System.Action<float> OnFaithChanged;
        public System.Action<float> OnWoodChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Notify UI of initial values
            OnFoodChanged?.Invoke(currentFood);
            OnFaithChanged?.Invoke(currentFaithPoints);
            OnWoodChanged?.Invoke(currentWood);
        }

        public bool ConsumeFood(float amount)
        {
            if (currentFood >= amount)
            {
                currentFood -= amount;
                OnFoodChanged?.Invoke(currentFood);
                return true;
            }
            return false;
        }

        public void AddFood(float amount)
        {
            // Apply efficiency bonus if researched
            if (hasEfficientGathering)
                amount *= EFFICIENT_GATHERING_MULTIPLIER;

            currentFood += amount;
            OnFoodChanged?.Invoke(currentFood);
        }

        public bool ConsumeFaith(float amount)
        {
            if (currentFaithPoints >= amount)
            {
                currentFaithPoints -= amount;
                OnFaithChanged?.Invoke(currentFaithPoints);
                return true;
            }
            return false;
        }

        public void AddFaith(float amount)
        {
            currentFaithPoints += amount;
            OnFaithChanged?.Invoke(currentFaithPoints);
        }

        public void AddWood(float amount)
        {
            currentWood += amount;
            OnWoodChanged?.Invoke(currentWood);
        }

        public bool ConsumeWood(float amount)
        {
            if (currentWood >= amount)
            {
                currentWood -= amount;
                OnWoodChanged?.Invoke(currentWood);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Miracle: Gift of Sustenance - spend faith to add food
        /// </summary>
        public bool PerformGiftOfSustenance(float faithCost = DEFAULT_GIFT_FAITH_COST, float foodGain = DEFAULT_GIFT_FOOD_GAIN)
        {
            if (ConsumeFaith(faithCost))
            {
                AddFood(foodGain);
                Log.Info($"Gift of Sustenance performed! +{foodGain} food for {faithCost} faith", Log.LogCategory.Systems);
                return true;
            }
            return false;
        }        /// <summary>
        /// Technology: Research Efficient Gathering
        /// </summary>
        public bool ResearchEfficientGathering(float faithCost = DEFAULT_RESEARCH_FAITH_COST)
        {
            if (!hasEfficientGathering && ConsumeFaith(faithCost))
            {
                hasEfficientGathering = true;
                Log.Info("Efficient Gathering researched! Food generation increased by 50%", Log.LogCategory.Systems);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Setter methods for save/load functionality
        /// </summary>
        public void SetFood(float amount)
        {
            currentFood = amount;
            OnFoodChanged?.Invoke(currentFood);
        }

        public void SetFaith(float amount)
        {
            currentFaithPoints = amount;
            OnFaithChanged?.Invoke(currentFaithPoints);
        }

        public void SetWood(float amount)
        {
            currentWood = amount;
            OnWoodChanged?.Invoke(currentWood);
        }
    }
}

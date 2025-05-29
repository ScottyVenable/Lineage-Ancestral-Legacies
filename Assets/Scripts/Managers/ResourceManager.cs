using UnityEngine;

namespace Lineage.Ancestral.Legacies.Managers
{
    /// <summary>
    /// Global resource management for food, faith points, and other resources.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

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
                amount *= 1.5f;

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
        public bool PerformGiftOfSustenance(float faithCost = 5f, float foodGain = 20f)
        {
            if (ConsumeFaith(faithCost))
            {
                AddFood(foodGain);
                UnityEngine.Debug.Log($"Gift of Sustenance performed! +{foodGain} food for {faithCost} faith");
                return true;
            }
            return false;
        }        /// <summary>
        /// Technology: Research Efficient Gathering
        /// </summary>
        public bool ResearchEfficientGathering(float faithCost = 15f)
        {
            if (!hasEfficientGathering && ConsumeFaith(faithCost))
            {
                hasEfficientGathering = true;
                UnityEngine.Debug.Log("Efficient Gathering researched! Food generation increased by 50%");
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

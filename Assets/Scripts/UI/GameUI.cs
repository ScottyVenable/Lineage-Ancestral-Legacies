using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.UI
{
    /// <summary>
    /// Main game UI showing resources, population, and miracle buttons.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("Resource Display")]
        public TextMeshProUGUI foodText;
        public TextMeshProUGUI faithText;
        public TextMeshProUGUI woodText;
        public TextMeshProUGUI populationText;

        [Header("Action Buttons")]
        public Button giftOfSustenanceButton;
        public Button efficientGatheringButton;
        public Button improveShelterButton;
        public Button spawnPopButton;

        [Header("Button Costs (UI)")]
        public TextMeshProUGUI giftCostText;
        public TextMeshProUGUI gatheringCostText;
        public TextMeshProUGUI shelterCostText;

        private void Start()
        {
            // Subscribe to resource updates
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnFoodChanged += UpdateFoodDisplay;
                ResourceManager.Instance.OnFaithChanged += UpdateFaithDisplay;
                ResourceManager.Instance.OnWoodChanged += UpdateWoodDisplay;
            }

            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.OnPopulationChanged += UpdatePopulationDisplay;
                PopulationManager.Instance.OnPopulationCapChanged += UpdatePopulationDisplay;
            }

            // Setup button listeners
            if (giftOfSustenanceButton != null)
                giftOfSustenanceButton.onClick.AddListener(PerformGiftOfSustenance);
            
            if (efficientGatheringButton != null)
                efficientGatheringButton.onClick.AddListener(ResearchEfficientGathering);
            
            if (improveShelterButton != null)
                improveShelterButton.onClick.AddListener(ImproveShelter);
            
            if (spawnPopButton != null)
                spawnPopButton.onClick.AddListener(SpawnNewPop);

            // Set button cost text
            if (giftCostText != null) giftCostText.text = "Cost: 5 Faith";
            if (gatheringCostText != null) gatheringCostText.text = "Cost: 15 Faith";
            if (shelterCostText != null) shelterCostText.text = "Cost: 10 Faith";

            // Initial display update
            UpdateAllDisplays();
        }

        private void UpdateFoodDisplay(float food)
        {
            if (foodText != null)
                foodText.text = $"Food: {food:F1}";
        }

        private void UpdateFaithDisplay(float faith)
        {
            if (faithText != null)
                faithText.text = $"Faith: {faith:F1}";
            
            // Update button interactability
            UpdateButtonStates();
        }

        private void UpdateWoodDisplay(float wood)
        {
            if (woodText != null)
                woodText.text = $"Wood: {wood:F1}";
        }

        private void UpdatePopulationDisplay(int population)
        {
            if (populationText != null && PopulationManager.Instance != null)
                populationText.text = $"Population: {PopulationManager.Instance.currentPopulation}/{PopulationManager.Instance.populationCap}";
        }

        private void UpdateButtonStates()
        {
            if (ResourceManager.Instance == null) return;

            float currentFaith = ResourceManager.Instance.currentFaithPoints;
            
            if (giftOfSustenanceButton != null)
                giftOfSustenanceButton.interactable = currentFaith >= 5f;
            
            if (efficientGatheringButton != null)
                efficientGatheringButton.interactable = currentFaith >= 15f && !ResourceManager.Instance.hasEfficientGathering;
            
            if (improveShelterButton != null)
                improveShelterButton.interactable = currentFaith >= 10f;
            
            if (spawnPopButton != null && PopulationManager.Instance != null)
                spawnPopButton.interactable = PopulationManager.Instance.currentPopulation < PopulationManager.Instance.populationCap;
        }

        private void UpdateAllDisplays()
        {
            if (ResourceManager.Instance != null)
            {
                UpdateFoodDisplay(ResourceManager.Instance.currentFood);
                UpdateFaithDisplay(ResourceManager.Instance.currentFaithPoints);
                UpdateWoodDisplay(ResourceManager.Instance.currentWood);
            }

            if (PopulationManager.Instance != null)
            {
                UpdatePopulationDisplay(PopulationManager.Instance.currentPopulation);
            }
        }

        // Button Actions
        private void PerformGiftOfSustenance()
        {
            if (ResourceManager.Instance != null)
            {
                bool success = ResourceManager.Instance.PerformGiftOfSustenance();
                if (success)
                {                    // TODO: Add visual feedback (particles, sound)
                    Log.Info("Gift of Sustenance performed!", Log.LogCategory.UI);
                }
            }
        }

        private void ResearchEfficientGathering()
        {
            if (ResourceManager.Instance != null)
            {
                bool success = ResourceManager.Instance.ResearchEfficientGathering();
                if (success)
                {
                    // Hide the button after research
                    if (efficientGatheringButton != null)
                        efficientGatheringButton.gameObject.SetActive(false);
                }
            }
        }

        private void ImproveShelter()
        {
            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.ImproveShelter();
            }
        }

        private void SpawnNewPop()
        {
            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.SpawnPop();
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnFoodChanged -= UpdateFoodDisplay;
                ResourceManager.Instance.OnFaithChanged -= UpdateFaithDisplay;
                ResourceManager.Instance.OnWoodChanged -= UpdateWoodDisplay;
            }

            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.OnPopulationChanged -= UpdatePopulationDisplay;
                PopulationManager.Instance.OnPopulationCapChanged -= UpdatePopulationDisplay;
            }
        }
    }
}

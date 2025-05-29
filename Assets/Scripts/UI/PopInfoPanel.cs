using UnityEngine;
using UnityEngine.UI;

namespace Lineage.Ancestral.Legacies.UI
{
    /// <summary>
    /// Controller for the Pop Info Panel UI.
    /// </summary>
    public class PopInfoPanel : MonoBehaviour
    {
        public Text nameText;
        public Slider healthSlider;
        public Slider hungerSlider;
        public Slider thirstSlider;
        public Slider staminaSlider;
        public Text ageText;

        private Entities.Pop currentPop;

        public void Show(Entities.Pop pop)
        {
            currentPop = pop;
            gameObject.SetActive(true);
            UpdateUI();
        }

        public void Hide()
        {
            currentPop = null;
            gameObject.SetActive(false);
        }

        private void UpdateUI()
        {
            if (currentPop == null) return;

            nameText.text = currentPop.name;
            healthSlider.value = currentPop.health;
            hungerSlider.value = currentPop.hunger;
            thirstSlider.value = currentPop.thirst;
            staminaSlider.value = currentPop.stamina;
            ageText.text = currentPop.age.ToString();
        }

        private void Update()
        {
            if (currentPop != null)
                UpdateUI();
        }
    }
}

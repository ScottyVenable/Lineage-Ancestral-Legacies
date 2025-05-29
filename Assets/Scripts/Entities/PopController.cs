using UnityEngine;

namespace Lineage.Ancestral.Legacies.Entities
{
    /// <summary>
    /// Manages pop input and behavior triggers.
    /// </summary>
    public class PopController : MonoBehaviour
    {
        private Pop pop;

        private void Awake()
        {
            pop = GetComponent<Pop>();
        }

        private void OnMouseDown()
        {
            // TODO: Handle selection logic
            Debug.Log($"Selected pop: {pop.name}");
        }
    }
}

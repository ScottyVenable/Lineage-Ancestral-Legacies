using UnityEngine;

namespace Lineage.Ancestral.Legacies.Entities
{
    public class PopController : MonoBehaviour
    {
        private Pop pop;

        [Header("Selection")]
        public GameObject selectionCirclePrefab; // Assign in Inspector
        private GameObject selectionCircleInstance;

        private void Awake()
        {
            pop = GetComponent<Pop>();
        }

        private void OnMouseDown()
        {
            // Handle selection logic
            UnityEngine.Debug.Log($"Selected pop: {pop.name}");
            Select();
        }

        private void Select()
        {
            if (selectionCircleInstance == null && selectionCirclePrefab != null)
            {
                selectionCircleInstance = Instantiate(
                    selectionCirclePrefab,
                    transform.position + Vector3.down * 0.01f, // Slightly below the pop
                    Quaternion.identity,
                    transform // Parent to pop
                );
            }
        }

        public void Deselect()
        {
            if (selectionCircleInstance != null)
            {
                Destroy(selectionCircleInstance);
                selectionCircleInstance = null;
            }
        }
    }
}

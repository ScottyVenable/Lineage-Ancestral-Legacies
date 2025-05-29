using UnityEngine;
using UnityEngine.AI;
using Lineage.Ancestral.Legacies.AI;
using Lineage.Ancestral.Legacies.AI.States;

namespace Lineage.Ancestral.Legacies.Entities
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PopController : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Pop pop;

        [Header("Selection")]
        public GameObject selectionCirclePrefab; // Assign in Inspector
        private GameObject selectionCircleInstance;

        private void Awake()
        {
            pop = GetComponent<Pop>();
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        public void Select()
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

        public bool IsSelected()
        {
            return selectionCircleInstance != null;
        }
    }
}

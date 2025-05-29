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

        private void OnMouseDown()
        {
            // Handle selection logic
            UnityEngine.Debug.Log($"Selected pop: {pop.name}");
            Select();
        }

        private void Update()
        {
            // If selected and right-click anywhere, command movement via FSM
            if (selectionCircleInstance != null && Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var sm = pop.GetComponent<PopStateMachine>();
                    sm.ChangeState(new CommandedState(hit.point));
                }
            }
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

        /// <summary>
        /// Command the pop to move to a destination using NavMeshAgent.
        /// </summary>
        public void MoveTo(Vector3 destination)
        {
            agent.SetDestination(destination);
        }
    }
}

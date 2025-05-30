using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Lineage.Ancestral.Legacies.AI;
using Lineage.Ancestral.Legacies.AI.States;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Entities
{
    [RequireComponent(typeof(Pop))]
    public class PopController : MonoBehaviour
    {
        // Component references
        private Pop pop;
        private NavMeshAgent agent;

        // State
        private bool isSelected = false;
        private Transform selectionIndicator;

        private bool _hasStoredOriginalColor = false;
        private Color _originalColor = Color.white;

        private void Awake()
        {
            // Get required components
            pop = GetComponent<Pop>();
            agent = GetComponent<NavMeshAgent>();

            // Setup NavMeshAgent for 2D
            if (agent != null)
            {
                agent.updateRotation = false;
                agent.updateUpAxis = false;
            }

            // Create or find selection indicator
            SetupSelectionIndicator();
        }

        private void SetupSelectionIndicator()
        {
            // Look for existing indicator
            selectionIndicator = transform.Find("SelectionIndicator");

            // Create one if it doesn't exist
            if (selectionIndicator == null)
            {
                GameObject indicator = new GameObject("SelectionIndicator");
                selectionIndicator = indicator.transform;
                selectionIndicator.SetParent(transform);
                selectionIndicator.localPosition = Vector3.zero;

                // Add a visible element (circle sprite renderer)
                SpriteRenderer renderer = indicator.AddComponent<SpriteRenderer>();
                // You would assign a selection circle sprite here
                // renderer.sprite = selectionCircleSprite;

                // Just use color for now
                renderer.color = new Color(0f, 1f, 0f, 0.5f); // Semi-transparent green
                renderer.sortingOrder = 1;
            }

            // Initially hide it
            selectionIndicator.gameObject.SetActive(false);
        }

        public void OnSelected(bool selected)
        {
            isSelected = selected;

            var renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                if (selected)
                {
                    // Store original color
                    if (!_hasStoredOriginalColor)
                    {
                        _originalColor = renderer.color;
                        _hasStoredOriginalColor = true;
                    }
                    renderer.color = new Color(_originalColor.r * 1.2f, _originalColor.g * 1.2f, _originalColor.b * 1.2f, _originalColor.a);
                }
                else if (_hasStoredOriginalColor)
                {
                    renderer.color = _originalColor;
                }
            }

            // Show/hide selection indicator
            if (selectionIndicator != null)
            {
                selectionIndicator.gameObject.SetActive(selected);
            }
        }

        /// <summary>
        /// Move this Pop to a specific position using the command system.
        /// </summary>
        public void MoveTo(Vector3 targetPosition)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(targetPosition);

                // Play animation if available
                if (pop.animator != null)
                {
                    pop.animator.SetBool("IsMoving", true);
                }

                // Notify Pop's state machine
                var stateMachine = GetComponent<PopStateMachine>();
                if (stateMachine != null)
                {
                    // You might want to transition to a moving state here
                    // stateMachine.ChangeState(new MoveToState(destination));
                }
            }
        }

        private void Update()
        {
            // Update animation state based on movement
            if (pop.animator != null && agent != null)
            {
                bool isMoving = agent.velocity.magnitude > 0.1f;
                pop.animator.SetBool("IsMoving", isMoving);

                // Face the direction of movement
                if (isMoving)
                {
                    Vector3 direction = agent.velocity.normalized;

                    // Flip sprite based on movement direction
                    if (direction.x != 0)
                    {
                        transform.localScale = new Vector3(
                            Mathf.Abs(transform.localScale.x) * Mathf.Sign(direction.x),
                            transform.localScale.y,
                            transform.localScale.z
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Get the Pop component for debug and UI purposes.
        /// </summary>
        public Pop GetPop()
        {
            return pop;
        }

        /// <summary>
        /// Get the PopStateMachine for debug purposes.
        /// </summary>
        public AI.PopStateMachine GetStateMachine()
        {
            return pop?.GetComponent<AI.PopStateMachine>();
        }

        /// <summary>
        /// Returns current state name for debug display.
        /// </summary>
        public string GetCurrentStateName()
        {
            var stateMachine = GetStateMachine();
            return stateMachine?.currentState?.GetType().Name ?? "No State";
        }

        /// <summary>
        /// Force select this Pop (useful for debug/testing).
        /// </summary>
        public void ForceSelect()
        {
            var selectionManager = FindFirstObjectByType<Managers.SelectionManager>();
            if (selectionManager != null)
            {
                var selectedPops = selectionManager.GetSelectedPops();
                if (!selectedPops.Contains(gameObject))
                {
                    selectionManager.GetType()
                        .GetMethod("SelectPop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.Invoke(selectionManager, new object[] { gameObject });
                }
            }
        }
    }
}

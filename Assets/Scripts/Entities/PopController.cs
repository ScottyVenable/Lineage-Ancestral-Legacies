using UnityEngine;
using UnityEngine.AI; // REQUIRED for NavMeshAgent
using Lineage.Ancestral.Legacies.AI; // For PopStateMachine
using Lineage.Ancestral.Legacies.Entities; // For Pop
using Lineage.Ancestral.Legacies.Managers; // For SelectionManager in ForceSelect (optional)
using Lineage.Ancestral.Legacies.Debug; // For DebugConsoleManager / AdvancedLogger (optional)

namespace Lineage.Ancestral.Legacies.Entities
{
    [RequireComponent(typeof(Pop))]
    [RequireComponent(typeof(NavMeshAgent))] // Ensure NavMeshAgent is always present
    public class PopController : MonoBehaviour
    {
        // Component references
        private Pop pop;
        private NavMeshAgent agent;
        public NavMeshAgent Agent => agent; // Public getter for other scripts to safely access the agent

        // Selection State & Indicator
        private bool isSelected = false;
        private Transform selectionIndicator;
        private bool _hasStoredOriginalColor = false;
        private Color _originalColor = Color.white;
        private SpriteRenderer _popSpriteRenderer; // Cache Pop's main SpriteRenderer

        private void Awake()
        {
            // Get required components
            pop = GetComponent<Pop>();
            agent = GetComponent<NavMeshAgent>();

            if (pop == null)
            {
                UnityEngine.Debug.LogError($"PopController on {gameObject.name} is missing Pop component!", this);
                enabled = false;
                return;
            }
            if (agent == null)
            { // Should not happen due to RequireComponent
                UnityEngine.Debug.LogError($"PopController on {gameObject.name} is missing NavMeshAgent component!", this);
                enabled = false;
                return;
            }

            // Cache the main sprite renderer for selection tinting
            _popSpriteRenderer = GetComponent<SpriteRenderer>();
            if (_popSpriteRenderer == null) _popSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_popSpriteRenderer == null) UnityEngine.Debug.LogWarning($"PopController: Pop {pop.name} has no SpriteRenderer for selection tinting.", pop);


            // --- NavMeshAgent Setup for 2D Top-Down ---
            agent.updateRotation = false; // We'll handle sprite flipping manually if needed

            // For Unity 2022.2+ and the AI Navigation package, updateUpAxis is the correct property.
            // For older built-in NavMesh, you might need to ensure NavMesh is on XY plane
            // and agent's Z position is locked, or sprites are on a child object.
#if UNITY_2022_2_OR_NEWER
            agent.updateUpAxis = false;
#endif


            float initialSpeed = 3.5f; // A sensible default
            if (pop.popData != null)
            {
                //              initialSpeed = pop.needsComponent.; // Use PopData's move speed if available
            }
            else
            {
                initialSpeed = pop.entityDataComponent.EntityData.speed.baseValue; // Fallback if popData isn't loaded yet
            }
            agent.speed = initialSpeed;
            agent.acceleration = initialSpeed * 2.5f; // Adjust multiplier as needed for responsiveness
            agent.stoppingDistance = 0.1f;      // How close to the destination to stop
            agent.autoBraking = true;             // Agent slows down as it approaches destination

            SetupSelectionIndicator();
        }

        private void SetupSelectionIndicator()
        {
            selectionIndicator = transform.Find("SelectionIndicator");
            if (selectionIndicator == null)
            {
                GameObject indicatorGO = new GameObject("SelectionIndicator");
                selectionIndicator = indicatorGO.transform;
                selectionIndicator.SetParent(transform);
                selectionIndicator.localPosition = new Vector3(0, -0.3f, 0); // Position slightly below Pop's pivot

                // Adjust scale to be consistent regardless of Pop's scale (if Pop scale changes)
                selectionIndicator.localScale = new Vector3(
                    0.5f / transform.localScale.x,
                    0.5f / transform.localScale.y,
                    1f / transform.localScale.z
                );


                SpriteRenderer sr = indicatorGO.AddComponent<SpriteRenderer>();
                // TODO: Assign your actual selection circle sprite here. Example:
                // sr.sprite = Resources.Load<Sprite>("UI/SelectionCircleSprite"); 
                // For now, a placeholder color:
                sr.color = new Color(0.2f, 1f, 0.2f, 0.45f); // Semi-transparent bright green

                // Ensure it sorts correctly (likely behind the Pop, or on a UI layer if using world space UI)
                // If your Pop's sprite is on sortingOrder 0 in its layer, this puts indicator behind.
                sr.sortingLayerName = _popSpriteRenderer != null ? _popSpriteRenderer.sortingLayerName : "Default";
                sr.sortingOrder = _popSpriteRenderer != null ? _popSpriteRenderer.sortingOrder - 1 : -1;
            }
            selectionIndicator.gameObject.SetActive(false);
        }

        public void OnSelected(bool selected)
        {
            isSelected = selected;

            if (_popSpriteRenderer != null)
            {
                if (selected)
                {
                    if (!_hasStoredOriginalColor)
                    {
                        _originalColor = _popSpriteRenderer.color;
                        _hasStoredOriginalColor = true;
                    }
                    // Apply a selection tint (e.g., slightly brighter or a specific color)
                    _popSpriteRenderer.color = new Color(_originalColor.r * 0.7f + 0.3f, _originalColor.g * 0.7f + 0.3f, _originalColor.b * 0.7f + 0.3f, _originalColor.a); // Brighter
                }
                else if (_hasStoredOriginalColor)
                {
                    _popSpriteRenderer.color = _originalColor;
                    _hasStoredOriginalColor = false;
                }
            }

            if (selectionIndicator != null)
            {
                selectionIndicator.gameObject.SetActive(selected);
            }
        }

        public void MoveTo(Vector3 targetPosition)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = false; // CRITICAL: Ensure agent is allowed to move
                agent.SetDestination(targetPosition);

            }
            else if (agent != null) // Agent exists but isn't valid for pathing
            {
                UnityEngine.Debug.LogWarning($"PopController: Pop {pop.name} trying to MoveTo({targetPosition}) but agent is not active or on NavMesh. Agent.isActiveAndEnabled: {agent.isActiveAndEnabled}, Agent.isOnNavMesh: {agent.isOnNavMesh}", pop);
            }
            // No else for agent == null because Awake would have logged an error and disabled script.
        }

        // This is the method IdleState and other states will call
        public void StopMovement()
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                // Check if it has a path before trying to reset it to avoid benign errors.
                if (agent.hasPath)
                {
                    agent.ResetPath(); // Clears the current path and stops movement.
                }
                agent.isStopped = true; // Explicitly ensure it's marked as stopped.
                                        // This is the call that was likely causing the error if agent was not on NavMesh.
            }
            // No "else" warning here, as IdleState might call this before Pop is fully on NavMesh
            // if spawning isn't perfect yet. The PopulationManager fix is more critical.

            if (pop.Animator != null)
            {
                pop.Animator.SetBool("IsMoving", false);
            }
        }

        private void Update()
        {
            if (pop == null || agent == null || !agent.isActiveAndEnabled || pop.Animator == null)
            {
                // Not fully initialized or missing components, do nothing.
                return;
            }

            // Animation and Sprite Flipping
            bool isCurrentlyMoving = agent.velocity.magnitude > 0.05f; // Check if effectively moving
            pop.Animator.SetBool("IsMoving", isCurrentlyMoving);

            if (isCurrentlyMoving)
            {
                // Sprite flipping based on horizontal velocity
                if (Mathf.Abs(agent.velocity.x) > 0.01f) // Only flip if there's clear horizontal movement
                {
                    // Assuming Pop's visual root transform should be scaled for flipping
                    // If your sprite faces right by default:
                    float newXScale = Mathf.Abs(transform.localScale.x) * (agent.velocity.x > 0 ? 1 : -1);
                    if (transform.localScale.x != newXScale) // Only update if necessary
                    {
                        transform.localScale = new Vector3(newXScale, transform.localScale.y, transform.localScale.z);
                    }
                }
            }

            // Commented out for now.
            /*
            // Check for arrival at destination
            // (This logic was in FixedUpdate in previous advice, Update is also fine)
            if (agent.isOnNavMesh && !agent.pathPending && agent.hasPath)
            {
                // Using a small buffer for remainingDistance check can be more reliable
                if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
                {
                    // Also check if velocity is very low, indicating it has actually stopped
                    if (agent.velocity.sqrMagnitude < 0.01f)
                    {
                        pop.stateMachine?.currentState.(); // Notify StateMachine (if it exists and is set)

                        // It's good practice to clear the path once arrived to prevent agent from
                        // trying to "re-path" if small adjustments occur.
                        // And ensure animation is set to not moving.
                        if (agent.hasPath) agent.ResetPath(); // Check again before resetting
                        pop.Animator.SetBool("IsMoving", false);
                    }
                }
            }
            */
        }

        public void UpdateAgentSpeed(float newSpeed)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.speed = newSpeed;
                agent.acceleration = newSpeed * 2.5f; // Keep acceleration proportional or set as needed
            }
        }

        // --- Your existing utility methods ---
        public Pop GetPop() { return pop; }
        public PopStateMachine GetStateMachine() { return pop?.GetComponent<PopStateMachine>(); }

/*    
        public string GetCurrentStateName()
        {
            var sm = GetStateMachine();
            // Assuming your IState interface has a 'Name' property
            return sm?.currentState ? ?? "No State";
        }
*/
        public void ForceSelect()
        {
            var selectionManager = FindFirstObjectByType<Managers.SelectionManager>();
            if (selectionManager != null)
            {
                // This reflection call is a bit risky. It's better if SelectionManager has a public AddToSelection method.
                // For now, assuming it's necessary:
                var selectedPops = selectionManager.GetSelectedPops(); // Assuming GetSelectedPops() returns List<GameObject> or similar
                if (selectedPops != null && !selectedPops.Contains(gameObject))
                {
                    try
                    {
                        selectionManager.GetType()
                            .GetMethod("SelectPop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(selectionManager, new object[] { gameObject });
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError($"ForceSelect failed via reflection: {ex.Message}", this);
                    }
                }
            }
        }
    }
    
    // TODO: At some point we will probably migrate over to an AI class that handles all AI logic
}
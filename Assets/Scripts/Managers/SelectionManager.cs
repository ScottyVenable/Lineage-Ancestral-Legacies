using UnityEngine;
using System.Collections.Generic;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.AI.States;

namespace Lineage.Ancestral.Legacies.Managers
{
    /// <summary>
    /// Handles selection of pops and issuing commands to selected units.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        public static SelectionManager Instance { get; private set; }

        [Header("Selection")]
        public LayerMask popLayerMask = -1;
        public LayerMask groundLayerMask = -1;        private List<PopController> selectedPops = new List<PopController>();
        private Camera playerCamera;

        // Events
        public System.Action OnSelectionChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                playerCamera = Camera.main;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // Left click to select
            if (Input.GetMouseButtonDown(0))
            {
                HandleSelection();
            }

            // Right click to issue move commands to selected pops
            if (Input.GetMouseButtonDown(1) && selectedPops.Count > 0)
            {
                HandleMoveCommand();
            }
        }

        private void HandleSelection()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, popLayerMask))
            {
                PopController popController = hit.collider.GetComponent<PopController>();
                
                if (popController != null)
                {
                    // If not holding control, clear previous selection
                    if (!Input.GetKey(KeyCode.LeftControl))
                    {
                        ClearSelection();
                    }

                    // Toggle selection of this pop
                    if (selectedPops.Contains(popController))
                    {
                        DeselectPop(popController);
                    }
                    else
                    {
                        SelectPop(popController);
                    }
                }
            }
            else
            {
                // Clicked on empty space, clear selection if not holding control
                if (!Input.GetKey(KeyCode.LeftControl))
                {
                    ClearSelection();
                }
            }
        }

        private void HandleMoveCommand()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
            {
                Vector3 targetPosition = hit.point;

                // Issue move commands to all selected pops
                for (int i = 0; i < selectedPops.Count; i++)
                {
                    var popController = selectedPops[i];
                    if (popController != null)
                    {
                        // Spread out the destination positions for multiple pops
                        Vector3 offset = Vector3.zero;
                        if (selectedPops.Count > 1)
                        {
                            float angle = (360f / selectedPops.Count) * i;
                            float radius = 1f;
                            offset = new Vector3(
                                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                                0,
                                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                            );
                        }

                        var stateMachine = popController.GetComponent<Pop>()?.GetComponent<AI.PopStateMachine>();
                        if (stateMachine != null)
                        {
                            stateMachine.ChangeState(new CommandedState(targetPosition + offset));
                        }
                    }
                }

                UnityEngine.Debug.Log($"Commanded {selectedPops.Count} pops to move to {targetPosition}");
            }
        }        private void SelectPop(PopController popController)
        {
            if (!selectedPops.Contains(popController))
            {
                selectedPops.Add(popController);
                popController.Select();
                UnityEngine.Debug.Log($"Selected pop: {popController.name}");
                OnSelectionChanged?.Invoke();
            }
        }

        private void DeselectPop(PopController popController)
        {
            if (selectedPops.Contains(popController))
            {
                selectedPops.Remove(popController);
                popController.Deselect();
                UnityEngine.Debug.Log($"Deselected pop: {popController.name}");
                OnSelectionChanged?.Invoke();
            }
        }

        private void ClearSelection()
        {
            foreach (var popController in selectedPops)
            {
                if (popController != null)
                    popController.Deselect();
            }
            selectedPops.Clear();
            OnSelectionChanged?.Invoke();
        }

        public List<PopController> GetSelectedPops()
        {
            return new List<PopController>(selectedPops);
        }

        public bool HasSelection()
        {
            return selectedPops.Count > 0;
        }
    }
}

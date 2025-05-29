using UnityEngine;
using Lineage.Ancestral.Legacies.AI;
using Lineage.Ancestral.Legacies.AI.Examples;

namespace Lineage.Ancestral.Legacies.Testing
{
    /// <summary>
    /// Test script to demonstrate external state machine functionality.
    /// Add this to a Pop GameObject to test the external state machine system.
    /// </summary>
    public class ExternalStateMachineTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool attachExampleStateMachine = true;
        [SerializeField] private bool useManager = true;

        private PopStateMachine popStateMachine;
        private ExampleExternalStateMachine exampleSM;
        private ExternalStateMachineManager manager;

        private void Start()
        {
            popStateMachine = GetComponent<PopStateMachine>();
            if (popStateMachine == null)
            {
                UnityEngine.Debug.LogError($"[ExternalStateMachineTest] No PopStateMachine found on {gameObject.name}!");
                return;
            }

            if (attachExampleStateMachine)
            {
                if (useManager)
                {
                    TestWithManager();
                }
                else
                {
                    TestDirectAttachment();
                }
            }
        }

        private void TestWithManager()
        {
            // Add ExternalStateMachineManager if not present
            manager = GetComponent<ExternalStateMachineManager>();
            if (manager == null)
            {
                manager = gameObject.AddComponent<ExternalStateMachineManager>();
            }

            // Add example external state machine
            exampleSM = gameObject.AddComponent<ExampleExternalStateMachine>();
            exampleSM.Priority = 10;

            // Add to manager (this will automatically attach it)
            manager.AddExternalStateMachine(exampleSM);

            UnityEngine.Debug.Log($"[ExternalStateMachineTest] Attached ExampleExternalStateMachine to {gameObject.name} via manager");
        }

        private void TestDirectAttachment()
        {
            // Add and attach example external state machine directly
            exampleSM = gameObject.AddComponent<ExampleExternalStateMachine>();
            exampleSM.Priority = 10;
            
            popStateMachine.AttachExternalStateMachine(exampleSM);

            UnityEngine.Debug.Log($"[ExternalStateMachineTest] Attached ExampleExternalStateMachine to {gameObject.name} directly");
        }

        [ContextMenu("Test Toggle External State Machine")]
        private void ToggleExternalStateMachine()
        {
            if (exampleSM != null)
            {
                exampleSM.SetActive(!exampleSM.IsActive);
                UnityEngine.Debug.Log($"[ExternalStateMachineTest] ExampleExternalStateMachine active: {exampleSM.IsActive}");
            }
        }

        [ContextMenu("Test Detach External State Machine")]
        private void DetachExternalStateMachine()
        {
            if (popStateMachine != null && exampleSM != null)
            {
                if (useManager && manager != null)
                {
                    manager.RemoveExternalStateMachine(exampleSM);
                }
                else
                {
                    popStateMachine.DetachExternalStateMachine(exampleSM);
                }
                UnityEngine.Debug.Log($"[ExternalStateMachineTest] Detached ExampleExternalStateMachine from {gameObject.name}");
            }
        }

        [ContextMenu("Test Disable External State Machines")]
        private void DisableExternalStateMachines()
        {
            if (popStateMachine != null)
            {
                popStateMachine.AllowExternalStateMachines = false;
                UnityEngine.Debug.Log($"[ExternalStateMachineTest] Disabled external state machines on {gameObject.name}");
            }
        }

        [ContextMenu("Test Enable External State Machines")]
        private void EnableExternalStateMachines()
        {
            if (popStateMachine != null)
            {
                popStateMachine.AllowExternalStateMachines = true;
                UnityEngine.Debug.Log($"[ExternalStateMachineTest] Enabled external state machines on {gameObject.name}");
            }
        }

        private void OnGUI()
        {
            if (popStateMachine == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Pop: {gameObject.name}");
            GUILayout.Label($"Current State: {popStateMachine.currentState?.GetType().Name ?? "None"}");
            GUILayout.Label($"External SMs Allowed: {popStateMachine.AllowExternalStateMachines}");
            GUILayout.Label($"External SMs Count: {popStateMachine.GetExternalStateMachines().Count}");
            
            if (exampleSM != null)
            {
                GUILayout.Label($"Example SM Active: {exampleSM.IsActive}");
                GUILayout.Label($"Example SM Priority: {exampleSM.Priority}");
            }

            if (GUILayout.Button("Toggle Example SM"))
            {
                ToggleExternalStateMachine();
            }

            if (GUILayout.Button("Toggle External SMs"))
            {
                if (popStateMachine.AllowExternalStateMachines)
                    DisableExternalStateMachines();
                else
                    EnableExternalStateMachines();
            }

            GUILayout.EndArea();
        }
    }
}

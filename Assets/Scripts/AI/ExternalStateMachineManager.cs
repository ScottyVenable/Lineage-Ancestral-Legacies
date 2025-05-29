// Unity component for managing external state machines - Updated
using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.AI
{
    /// <summary>
    /// Unity component that manages external state machines for a Pop.
    /// This component automatically finds the PopStateMachine and allows you to attach
    /// external state machines through the Unity Inspector.
    /// </summary>
    public class ExternalStateMachineManager : MonoBehaviour
    {
        [Header("External State Machines")]
        [SerializeField] private List<MonoBehaviour> externalStateMachineComponents = new List<MonoBehaviour>();
        
        [Header("Settings")]
        [SerializeField] private bool attachOnStart = true;
        [SerializeField] private bool detachOnDestroy = true;

        private PopStateMachine popStateMachine;
        private List<IExternalStateMachine> attachedStateMachines = new List<IExternalStateMachine>();

        private void Start()
        {
            // Find the PopStateMachine on this GameObject or its children
            popStateMachine = GetComponent<PopStateMachine>();
            if (popStateMachine == null)
            {
                popStateMachine = GetComponentInChildren<PopStateMachine>();
            }
            
            if (popStateMachine == null)
            {
                UnityEngine.Debug.LogError($"[ExternalStateMachineManager] No PopStateMachine found on {gameObject.name} or its children!");
                return;
            }

            if (attachOnStart)
            {
                AttachAllStateMachines();
            }
        }

        private void OnDestroy()
        {
            if (detachOnDestroy)
            {
                DetachAllStateMachines();
            }
        }

        /// <summary>
        /// Attaches all configured external state machines to the PopStateMachine.
        /// </summary>
        public void AttachAllStateMachines()
        {
            if (popStateMachine == null)
                return;            foreach (var component in externalStateMachineComponents)
            {
                if (component is IExternalStateMachine externalStateMachine)
                {
                    popStateMachine.AttachExternalStateMachine(externalStateMachine);
                    attachedStateMachines.Add(externalStateMachine);
                    UnityEngine.Debug.Log($"[ExternalStateMachineManager] Attached {component.GetType().Name} to {gameObject.name}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[ExternalStateMachineManager] Component {component.GetType().Name} does not implement IExternalStateMachine!");
                }
            }
        }

        /// <summary>
        /// Detaches all external state machines from the PopStateMachine.
        /// </summary>
        public void DetachAllStateMachines()
        {
            if (popStateMachine == null)
                return;

            foreach (var stateMachine in attachedStateMachines)
            {
                popStateMachine.DetachExternalStateMachine(stateMachine);
            }
            attachedStateMachines.Clear();
        }

        /// <summary>
        /// Adds an external state machine component to be managed.
        /// </summary>
        public void AddExternalStateMachine(MonoBehaviour component)
        {
            if (component is IExternalStateMachine && !externalStateMachineComponents.Contains(component))
            {
                externalStateMachineComponents.Add(component);
                
                // If we're already running and should attach on start, attach this one now
                if (attachOnStart && popStateMachine != null && Application.isPlaying)
                {
                    var externalStateMachine = component as IExternalStateMachine;
                    popStateMachine.AttachExternalStateMachine(externalStateMachine);
                    attachedStateMachines.Add(externalStateMachine);
                }
            }
        }

        /// <summary>
        /// Removes an external state machine component from being managed.
        /// </summary>
        public void RemoveExternalStateMachine(MonoBehaviour component)
        {
            if (externalStateMachineComponents.Remove(component) && component is IExternalStateMachine externalStateMachine)
            {
                if (popStateMachine != null && attachedStateMachines.Contains(externalStateMachine))
                {
                    popStateMachine.DetachExternalStateMachine(externalStateMachine);
                    attachedStateMachines.Remove(externalStateMachine);
                }
            }
        }

        /// <summary>
        /// Gets the PopStateMachine this manager is controlling.
        /// </summary>
        public PopStateMachine PopStateMachine => popStateMachine;

        /// <summary>
        /// Gets the list of currently attached external state machines.
        /// </summary>
        public IReadOnlyList<IExternalStateMachine> AttachedStateMachines => attachedStateMachines.AsReadOnly();
    }
}

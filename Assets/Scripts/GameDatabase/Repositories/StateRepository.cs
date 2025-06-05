using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Repository for managing state data.
    /// </summary>
    public static class StateRepository
    {
        #region Database
        public static List<State> AllStates { get; private set; } = new List<State>();
        #endregion

        #region State Methods
        /// <summary>
        /// Gets a state by its ID.
        /// </summary>
        /// <param name="stateID">The ID of the state to retrieve.</param>
        /// <returns>The state with the specified ID, or default if not found.</returns>
        public static State GetStateByID(int stateID)
        {
            return AllStates.FirstOrDefault(s => s.stateID == stateID);
        }

        /// <summary>
        /// Gets a state by its name.
        /// </summary>
        /// <param name="stateName">The name of the state to retrieve.</param>
        /// <returns>The state with the specified name, or default if not found.</returns>
        public static State GetStateByName(string stateName)
        {
            return AllStates.FirstOrDefault(s => s.stateName == stateName);
        }

        /// <summary>
        /// Gets interruptible states.
        /// </summary>
        /// <returns>A list of states that can be interrupted.</returns>
        public static List<State> GetInterruptibleStates()
        {
            return AllStates.Where(s => s.canBeInterrupted).ToList();
        }

        /// <summary>
        /// Gets states by priority level.
        /// </summary>
        /// <param name="priority">The priority level to filter by.</param>
        /// <returns>A list of states with the specified priority.</returns>
        public static List<State> GetStatesByPriority(int priority)
        {
            return AllStates.Where(s => s.priority == priority).ToList();
        }

        /// <summary>
        /// Gets states ordered by priority (highest first).
        /// </summary>
        /// <returns>A list of states ordered by priority.</returns>
        public static List<State> GetStatesByPriorityDescending()
        {
            return AllStates.OrderByDescending(s => s.priority).ToList();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the state database with default data.
        /// </summary>
        public static void InitializeDatabase()
        {
            AllStates.Clear();
            
            // Initialize basic states
            InitializeBasicStates();
            
            Debug.Log("StateRepository: Database initialized successfully.");
        }

        private static void InitializeBasicStates()
        {
            var idleState = new State
            {
                stateID = (int)State.ID.Idle,
                stateName = "Idle",
                stateDescription = "Entity is standing idle",
                stateDuration = -1f, // Infinite duration
                priority = 0,
                canBeInterrupted = true,
                energyCostPerSecond = 0f
            };
            AllStates.Add(idleState);

            var movingState = new State
            {
                stateID = (int)State.ID.Moving,
                stateName = "Moving",
                stateDescription = "Entity is moving",
                stateDuration = -1f,
                priority = 1,
                canBeInterrupted = true,
                energyCostPerSecond = 1f
            };
            AllStates.Add(movingState);

            var attackingState = new State
            {
                stateID = (int)State.ID.Attacking,
                stateName = "Attacking",
                stateDescription = "Entity is attacking",
                stateDuration = 2f,
                priority = 5,
                canBeInterrupted = false,
                energyCostPerSecond = 3f
            };
            AllStates.Add(attackingState);

            // TODO: Add more default states as needed
        }
        #endregion
    }
}

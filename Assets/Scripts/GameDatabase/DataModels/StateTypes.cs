using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    #region State System

    public struct State
    {
        public enum ID
        {
            Idle = 0,
            Moving = 1,
            Attacking = 2,
            Defending = 3,
            Crafting = 4,
            Resting = 5,
            Eating = 6,
            Sleeping = 7,
            Dead = 8,
            Stunned = 9,
            Casting = 10,
            Channeling = 11,
            Interacting = 12,
            Trading = 13,
            Talking = 14,
            Searching = 15
        }

        public int stateID;
        public string stateName;
        public string stateDescription;
        public float stateDuration;
        public int priority;
        public bool canBeInterrupted;
        public float energyCostPerSecond;
    }

    #endregion
}

namespace Lineage.Behavior
{
    /// <summary>
    /// Behavior states that correspond to the Pop entity states in the database.
    /// Used by the behavior system to track and communicate AI state.
    /// </summary>
    public enum BehaviorState
    {
        Idle = 0,
        Moving = 1,
        Gathering = 2,
        Crafting = 3,
        Socializing = 4,
        Resting = 5,
        Searching = 6,
        Attacking = 7,
        Defending = 8,
        Fleeing = 9,
        Patrolling = 10,
        Guarding = 11,
        Exploring = 12,
        Building = 13,
        Harvesting = 14,
        Trading = 15,
        Learning = 16,
        Teaching = 17,
        Healing = 18,
        Eating = 19,
        Drinking = 20,
        Sleeping = 21,
        Working = 22,
        Hunting = 23,
        Hiding = 24,
        Celebrating = 25,
        Mourning = 26,
        Praying = 27,
        Researching = 28,
        Communicating = 29,
        Hauling = 30,
        Interacting = 31
    }
}

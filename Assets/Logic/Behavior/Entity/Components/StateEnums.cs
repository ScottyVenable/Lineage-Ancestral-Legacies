using System;
using Unity.Behavior;

[BlackboardEnum]
/// <summary>
/// Represents the various states an entity can be in.
/// Expanded to match the full GameData State.ID enumeration.
/// </summary>
public enum BehaviorState
{
	Idle = 0,
	Attacking = 1,
	Defending = 2,
	Fleeing = 3,
	Searching = 4,
	Resting = 5,
	Patrolling = 6,
	Interacting = 7,
	Hauling = 8,
	Gathering = 9,
	Hiding = 10,
	Socializing = 11,
	Crafting = 12,
	Healing = 13,
	Exploring = 14,
	Hunting = 15,
	Playing = 16,
	Fishing = 17,
	Farming = 18,
	Dead = 19,
	Unconscious = 20,
	Sleeping = 21
}

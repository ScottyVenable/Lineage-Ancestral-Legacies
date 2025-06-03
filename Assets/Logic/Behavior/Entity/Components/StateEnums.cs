using System;
using Unity.Behavior;

[BlackboardEnum]
/// <summary>
/// Represents the various states an entity can be in.
/// </summary>
public enum State
{
	Wandering,
	Foraging,
	Hauling,
	Attacking,
	Fleeing,
	Hunting,
	Eating,
	Socializing,
	Resting,
	Sleeping
}

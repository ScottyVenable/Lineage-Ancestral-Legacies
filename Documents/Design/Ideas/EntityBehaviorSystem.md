# Unity Behavior System Guide for Lineages: Ancestral Legacies

This document provides a comprehensive guide to understanding and utilizing the Unity Behavior package (often referred to as Behavior Trees or State Machines depending on the specific implementation style) for your RTS evolution survival settlement building game, "Lineages: Ancestral Legacies." We'll cover its core concepts, how it functions, and a strategy for transitioning your existing entity behaviors.

## 1. Introduction to the Unity Behavior System

The Unity Behavior system (specifically `com.unity.behavior`) provides a powerful framework for creating complex AI and logic using visual, graph-based editors. At its heart, it allows you to design **Behavior Graphs**, which are essentially state machines or behavior trees that define how an entity (like your "Pops" or Hominids) should act and react to various situations in the game world.

**Benefits for "Lineages: Ancestral Legacies":**

* **Visual Clarity:** Instead of managing complex conditional logic and state transitions purely in C# scripts (like your current `scr_HominidController.cs`), Behavior Graphs offer a visual representation of an entity's decision-making process. This makes it easier to understand, debug, and iterate on AI.
* **Modularity and Reusability:** You can create custom **Actions** (what an entity does) and **Conditions** (what triggers a change in behavior) that can be reused across different states and even different Behavior Graphs. Your existing `GetEntityDataAction.cs` and `SetEntityStateAction.cs` are perfect examples of this!
* **Scalability:** As your game grows in complexity with more sophisticated survival mechanics, evolutionary traits, and settlement interactions, a visual behavior system can handle this complexity more gracefully than deeply nested `if-else` statements or large `switch` cases.
* **Designer-Friendly:** While programmers set up the core actions and conditions, game designers can often tweak and assemble behaviors in the graph editor, facilitating faster iteration on gameplay.
* **Debugging Tools:** The visual nature of the graph, often with runtime highlighting of active states and transitions, can greatly aid in debugging AI issues. The **Behavior Differ** tool also helps manage changes when working in a team or with different versions of your graphs.

## 2. Core Concepts of the Unity Behavior System

Let's break down the key components you'll be working with:

### 2.1. Behavior Graph

This is the central asset where you define an entity's behavior. You've already started creating one with `EntityBehavior.asset`. A Behavior Graph consists of:

* **States:** These represent distinct behaviors or modes of operation for an entity. For your Pops, examples could include `Idle`, `Wandering`, `Foraging`, `FleeingFromThreat`, `BuildingShelter`, `Sleeping`, `Mating`, etc. Your `pop_states.json` and `StateEnums.cs` likely define many of these already.
* **Transitions:** These are the connections between states. A transition is typically governed by one or more **Conditions**. When a condition (or set of conditions) is met, the graph transitions from the current state to a new state.
    * *Example:* A Pop in an `Idle` state might transition to a `Foraging` state when its `HungerLevel` (a Condition) drops below a certain threshold.
* **Actions:** These are the specific operations an entity performs while in a particular state. Actions can be executed when a state is entered (`OnEnter`), every frame the state is active (`OnUpdate`), or when the state is exited (`OnExit`).
    * *Example:* In a `Foraging` state, an `OnUpdate` action might be `MoveToFoodSource`, and an `OnEnter` action could be `PlayForagingAnimation`.
* **Start Node:** Every graph has a starting point, indicating the initial state of the entity when the behavior begins.
* **Any State Node:** A special node that allows transitions from *any* other state if a certain high-priority condition is met (e.g., `HealthCritical` leading to a `Flee` state, regardless of what the Pop was doing).

### 2.2. Actor

An Actor is the `GameObject` in your scene that will execute a Behavior Graph. In your case, your Pop prefabs (like the one in `Assets/Entities/Pop/Pop.prefab`) will be the Actors. You'll typically add a component (often called something like `BehaviorExecutor` or `BehaviorRunner`) to the Actor `GameObject` and assign a Behavior Graph asset to it.

### 2.3. Blackboard

The Blackboard is a data container associated with an instance of a Behavior Graph. It allows you to store and share variables that the graph's Actions and Conditions can read from and write to. This is crucial for making decisions and personalizing behavior.

* **Variables:** Can be of various types (float, int, bool, Vector3, GameObject references, etc.).
* **Scope:** Data on the Blackboard is typically local to that specific Behavior Graph instance running on an Actor.
* **Usage:**
    * Storing an entity's current target (`TargetFoodSource`, `TargetEnemy`).
    * Tracking internal states not directly represented by graph states (e.g., `HasPickedUpItem`).
    * Passing parameters to Actions (e.g., `MovementSpeed` for a `MoveToAction`).
    * Storing sensory information (e.g., `NearestThreatDistance`).

### 2.4. Actions and Conditions

These are the building blocks of your graph's logic.

* **Actions:** C# classes that inherit from a base Action class provided by the Behavior system (e.g., `Unity.AI.Planner.Action`). They define a piece of executable logic.
    * **Built-in:** The package may come with common actions (e.g., `DebugLogAction`, basic movement).
    * **Custom:** You'll create most of your game-specific logic here. Your `GetEntityDataAction.cs` (presumably to fetch data from `EntityDataComponent.cs` or `PopData.cs`) and `SetEntityStateAction.cs` (likely to update the enum in `StateEnums.cs` or trigger animations via `Pop Animator Controller.controller`) are excellent examples.
* **Conditions:** C# classes that inherit from a base Condition class. They evaluate to true or false, determining if a transition should fire or if a conditional action should run.
    * **Built-in:** Basic comparisons (e.g., `IsBlackboardVariableTrue`).
    * **Custom:** Essential for game-specific logic (e.g., `IsHungryCondition`, `IsThreatNearbyCondition`, `HasRequiredMaterialsCondition`).

### 2.5. Behavior Differ

The Behavior Differ tool (as per the documentation) is designed to help you compare two Behavior Graph assets. This is particularly useful for:
* Seeing what changes have been made between versions.
* Merging changes if you're working in a team and multiple people modify the same graph.
* Understanding how a branched or experimental version of a behavior differs from the main one.

## 3. How the Behavior System Works at Runtime

1.  **Initialization:** When an Actor with a Behavior Graph becomes active, the graph is initialized. The `Start Node` determines the initial active state.
2.  **Tick/Update:** The Behavior Graph is "ticked" or updated regularly (usually every frame or at a fixed interval).
3.  **State Evaluation:**
    * **`OnUpdate` Actions:** If the current state has `OnUpdate` actions, they are executed.
    * **Transitions:** The system checks all outgoing transitions from the current active state.
    * **Condition Check:** For each transition, its associated Conditions are evaluated.
    * **Transition Firing:** If a transition's Conditions all evaluate to true, the transition "fires."
4.  **State Change:**
    * **`OnExit` Actions:** Actions in the old state's `OnExit` list are executed.
    * The graph moves to the new state.
    * **`OnEnter` Actions:** Actions in the new state's `OnEnter` list are executed.
5.  **Loop:** The process repeats from step 2.

## 4. Transitioning "Lineages: Ancestral Legacies" to the Behavior System

Migrating your existing AI from scripts like `scr_HominidController.cs` to Behavior Graphs is an iterative process. Here's a suggested strategy:

### 4.1. Analyze Existing Behaviors and Data

* **Identify Core Behaviors:** Go through `scr_HominidController.cs`, `Pop.cs`, and any related AI scripts. List all distinct behaviors your Pops can perform.
    * *Examples for your game:* Idle, Wander, Search for Food, Forage, Eat, Search for Water, Drink, Flee from Danger, Seek Shelter, Build, Craft, Mate, Socialize, Sleep, Defend Territory, Hunt.
* **Map to States:** For each core behavior, define a corresponding state in your `EntityBehavior.asset` graph (or create new graphs for different entity types if needed). Your `pop_states.json` and `StateEnums.cs` are valuable resources here.
* **Identify State Triggers (Conditions):** What makes a Pop switch from one behavior to another?
    * *Examples:* Low hunger/thirst (from `NeedsComponent.cs`), presence of a predator, availability of resources, completion of a task, time of day (from `TimeManager.cs`), player commands.
* **Identify Actions:** What actions does a Pop take within each state?
    * *Examples:* Moving to a location, playing an animation (using `Pop Animator Controller.controller`), interacting with a `ResourceNode.cs`, modifying data in `EntityDataComponent.cs` or `InventoryComponent.cs`.
* **Data Requirements (Blackboard):** What information does each behavior need?
    * *Examples:* Current target (GameObject), path to target, inventory contents, need levels, current task.

### 4.2. Create Custom Actions and Conditions

You've already started this with `GetEntityDataAction.cs` and `SetEntityStateAction.cs`. Expand on this foundation.

**General Structure for a Custom Action (Simplified):**
```csharp
// Example: ForageResourceAction.cs
using Unity.AI.Planner.DomainLanguage.TraitBased; // Or appropriate namespace
using UnityEngine;

// Assuming you have an ITraitBasedObjectData interface for your actor
// and specific components like InventoryComponent and NeedsComponent.

[TraitAction]
public class ForageResourceAction : MonoBehaviour, IAction // Or appropriate Action base class
{
    // Parameters exposed in the Behavior Graph editor
    public float ForagingDuration = 5f;
    public string ResourceTypeToForage; // e.g., "Berries", "Wood"
    // Potentially link to ItemSO from Assets/Scripts/Systems/Inventory/ItemSO.cs

    private float timer;
    private bool isForaging;
    // References to Actor components (can be set via Blackboard or GetComponent)
    private InventoryComponent inventory;
    private NeedsComponent needs;
    private Animator animator; // From your Pop Animator Controller

    public void OnActionEnter(IActor actor, ICurrentStateFull currentState)
    {
        // Get components from the actor
        // This might involve casting 'actor' or getting data from the blackboard
        // that was set by a GetEntityDataAction.
        // Example: inventory = ((MonoBehaviour)actor).GetComponent<InventoryComponent>();
        //          animator = ((MonoBehaviour)actor).GetComponent<Animator>();

        Debug.Log($"{actor.name} is starting to forage for {ResourceTypeToForage}.");
        timer = 0f;
        isForaging = true;
        // animator.SetTrigger("ForageStart"); // Example animation trigger
    }

    public ActionCompletionType OnActionUpdate(IActor actor, ICurrentStateFull currentState)
    {
        if (!isForaging)
            return ActionCompletionType.Failure;

        timer += Time.deltaTime;
        if (timer >= ForagingDuration)
        {
            // Logic to add resource to inventory
            // inventory.AddItem(ResourceTypeToForage, 1);
            // needs.UpdateNeed("Energy", -5); // Foraging costs energy
            Debug.Log($"{actor.name} finished foraging {ResourceTypeToForage}.");
            isForaging = false;
            // animator.SetTrigger("ForageEnd");
            return ActionCompletionType.Success;
        }
        return ActionCompletionType.Running;
    }

    public void OnActionExit(IActor actor, ICurrentStateFull currentState)
    {
        if (isForaging) // If exited prematurely
        {
            Debug.Log($"{actor.name} stopped foraging {ResourceTypeToForage} prematurely.");
            // animator.SetTrigger("ForageCancel");
        }
        isForaging = false;
    }
}

General Structure for a Custom Condition (Simplified):

// Example: IsHungryCondition.cs
using Unity.AI.Planner.DomainLanguage.TraitBased;
using UnityEngine;

[TraitCondition]
public class IsHungryCondition : MonoBehaviour, ICondition
{
    // Parameters
    public float HungerThreshold = 30f;

    // Reference to Actor's NeedsComponent
    private NeedsComponent needs;

    public bool Evaluate(IActor actor, ICurrentStateFull currentState)
    {
        // Get NeedsComponent, potentially via Blackboard or GetComponent
        // needs = ((MonoBehaviour)actor).GetComponent<NeedsComponent>();
        // if (needs != null)
        // {
        //     return needs.GetNeedLevel("Hunger") < HungerThreshold;
        // }
        return false; // Default if component not found
    }
}

Actions & Conditions to Create for "Lineages":

Actions:

MoveToPositionAction: Uses NavMeshAgent to move to a Vector3.

MoveToGameObjectAction: Moves to a target GameObject.

PlayAnimationAction: Triggers animations on the Pop's Animator (e.g., "Walk", "Forage", "Eat", "Attack").

GatherResourceAction: Interacts with a ResourceNode to add items to InventoryComponent.

ConsumeItemAction: Consumes an item from inventory to satisfy a need (e.g., eat berry to reduce hunger).

UpdateNeedAction: Directly modifies a need value in NeedsComponent.

FindClosestResourceNodeAction: Searches for the nearest resource of a specific type and stores it on the Blackboard.

FindSafeLocationAction: Identifies a safe spot to flee to.

AttackTargetAction: Performs an attack on a target entity.

BuildStructureAction: Contributes to building a structure.

WanderAction: Implements random movement within an area.

SocializeAction: Interacts with another Pop.

RestAction: Recovers energy.

Utilize your existing SetEntityStateAction to update StateEnums if other systems rely on it, or to set animation parameters.

Utilize your existing GetEntityDataAction to populate Blackboard variables from EntityDataComponent.

Conditions:

IsNeedBelowThresholdCondition (e.g., IsHungry, IsThirsty, IsTired).

IsThreatNearbyCondition: Detects hostile entities.

HasItemInInventoryCondition: Checks InventoryComponent.

IsTargetReachableCondition: Checks if NavMesh path exists.

IsDaytimeCondition/IsNighttimeCondition (integrates with TimeManager.cs).

HasBuildOrderCondition: Checks if the Pop has been assigned a construction task.

EvolutionaryTraitCondition: Checks if a Pop has a specific evolved trait influencing behavior.

4.3. Building the Behavior Graph(s)
Start with a Basic Loop:

Create states like Idle, Wander, CheckNeeds.

Idle: Default state. OnEnter could play an idle animation.

Wander: OnEnter picks a random point, OnUpdate uses MoveToPositionAction. Transition back to Idle after a timeout or on reaching the point.

CheckNeeds: A state that runs periodically. OnEnter could use GetEntityDataAction to read need levels from NeedsComponent onto the Blackboard.

Add Need Fulfillment:

From CheckNeeds, add transitions to FindFoodState if IsHungryCondition is true.

FindFoodState: OnEnter uses FindClosestResourceNodeAction (for "Food" type).

Transition to ForageState if food found (Blackboard variable TargetFoodSource is not null).

Transition back to Wander or Idle if no food found after a timeout.

ForageState: OnEnter uses MoveToGameObjectAction (to TargetFoodSource). OnUpdate could check distance. Once close, transition to GatherFoodState.

GatherFoodState: OnEnter uses PlayAnimationAction ("Foraging"), GatherResourceAction. OnUpdate checks if gathering is complete.

Transition to EatFoodState when food is gathered.

EatFoodState: OnEnter uses ConsumeItemAction, UpdateNeedAction (for hunger), PlayAnimationAction ("Eating").

Transition back to Idle or CheckNeeds.

Incorporate Existing Logic:

Gradually take pieces of logic from scr_HominidController.cs. For example, if it has a complex Update() method with many if-else blocks for different states, try to replicate that flow visually in the graph.

The scr_HominidController.cs might evolve into a simpler script that:

Holds references to essential components (Animator, NavMeshAgent, EntityDataComponent, NeedsComponent, InventoryComponent).

Provides public methods that can be called by custom Actions (though it's often cleaner for Actions to directly access components or use a well-defined API on the Actor).

Initializes and starts the Behavior Graph execution.

Data Flow with Blackboard and EntityDataComponent:

Use GetEntityDataAction to read data from EntityDataComponent (which holds PopData) onto the Blackboard at the beginning of relevant states or when data changes.

Actions can then read from the Blackboard.

If actions modify data that needs to be persistent or accessible by other systems, they can write back to the Blackboard, and another custom action (or the SetEntityStateAction if appropriate) could update the EntityDataComponent.

Handling Evolution:

Evolutionary traits can be stored in PopData / EntityDataComponent.

Create EvolutionaryTraitConditions that check for these traits.

These conditions can enable/disable certain transitions or branches of the Behavior Graph.

Traits might also provide parameters to actions (e.g., a "Stronger" trait increases damage in AttackTargetAction, read via Blackboard).

RTS Elements (Settlement Building, Commands):

Player commands can set variables on the Blackboard (e.g., AssignedTask = BuildShelter, BuildSiteLocation = ...).

The Behavior Graph can have states like AwaitingCommand, ExecuteBuildCommand.

ExecuteBuildCommand: Transitions to GatherMaterialsForBuilding, then MoveToBuildSite, then ConstructBuildingAction.

4.4. Step-by-Step Migration Example (Pop Foraging)
Current (Hypothetical) scr_HominidController.cs Snippet:

// void Update() {
//     if (currentAction == PopAction.Idle) {
//         if (needs.hunger < 30) {
//             currentAction = PopAction.Foraging;
//             FindFoodTarget();
//         } else {
//             Wander();
//         }
//     } else if (currentAction == PopAction.Foraging) {
//         if (targetFoodSource != null) {
//             MoveTowards(targetFoodSource.transform.position);
//             if (Vector3.Distance(transform.position, targetFoodSource.transform.position) < 1.0f) {
//                 Harvest(targetFoodSource);
//                 currentAction = PopAction.Eating;
//             }
//         } else {
//             currentAction = PopAction.Idle; // No food found
//         }
//     } // ... and so on
// }

Behavior Graph Migration:

States: Idle, Wander, CheckHunger, FindFood, MoveToFood, HarvestFood, EatFood.

Blackboard Variables:

float currentHunger

GameObject targetFoodSource

bool hasFoodItem

Graph Flow:

Idle State:

OnEnter: PlayAnimationAction ("Idle").

Transition to CheckHunger (after a short delay or periodically).

CheckHunger State:

OnEnter: GetEntityDataAction (to get hunger from NeedsComponent and store in currentHunger on Blackboard).

Transition to FindFood if IsBlackboardFloatLessCondition (currentHunger < 30).

Transition to Wander otherwise.

Wander State:

OnEnter: WanderAction (sets a random destination).

OnUpdate: MoveToPositionAction.

Transition to Idle on reaching destination or timeout.

High-priority transition from Any State to CheckHunger if a timer dictates it's time to check needs again.

FindFood State:

OnEnter: FindClosestResourceNodeAction (type "Food", output to targetFoodSource on Blackboard).

Transition to MoveToFood if IsBlackboardGameObjectSetCondition (targetFoodSource is not null).

Transition to Idle (or a "NoFoodFound" state) if targetFoodSource is null.

MoveToFood State:

OnEnter: MoveToGameObjectAction (target is targetFoodSource from Blackboard).

Transition to HarvestFood if IsWithinDistanceCondition (to targetFoodSource).

HarvestFood State:

OnEnter: PlayAnimationAction ("Forage"), GatherResourceAction (from targetFoodSource, adds item to InventoryComponent, sets hasFoodItem on Blackboard to true).

Transition to EatFood on success of GatherResourceAction.

EatFood State:

OnEnter: PlayAnimationAction ("Eat"), ConsumeItemAction (type "Food" from inventory), UpdateNeedAction (increase hunger).

Transition to Idle.

4.5. Iteration and Testing
Start Small: Implement one behavior (e.g., the hunger/foraging loop) thoroughly.

Test Frequently: Use debug logs in your Actions/Conditions and watch the graph execution in the editor.

Refactor scr_HominidController: As logic moves to the graph, simplify the C# controller. It might become a simple "Actor" script holding data references and initiating the graph.

Use the Behavior Differ: If you create variations of behaviors or if multiple people are working on the AI, use the differ to track and merge changes.

5. Example Behavior Graph Snippets for "Lineages"
Here are conceptual snippets of how parts of your Behavior Graphs might look:

A. Basic Survival Need (Hunger):

[Start] --> [Idle]
           |
           (Timer_CheckNeeds)
           v
[CheckNeeds] --(IsHungry_True)--> [FindFoodSource] --(FoodFound)--> [MoveToFood] --(ReachedFood)--> [ForageFood] --(FoodCollected)--> [EatFood] --> [Idle]
      |                                     |                                                                     |
      (IsHungry_False)                      (NoFoodFound)                                                         (ForageFailed)
      v                                     v                                                                     v
[Wander] -------------------------------->[Idle]------------------------------------------------------------------[Idle]

CheckNeeds State:

OnEnter: GetPopDataAction (reads hunger from NeedsComponent to Blackboard variable CurrentHunger).

IsHungry Condition (on transition from CheckNeeds to FindFoodSource):

Evaluates if Blackboard.CurrentHunger < HungerThreshold.

FindFoodSource State:

OnEnter: FindNearestResourceAction (type: Food, output to Blackboard TargetFood).

FoodFound Condition (on transition from FindFoodSource to MoveToFood):

Evaluates if Blackboard.TargetFood != null.

B. Threat Response:

[Any State] --(ThreatDetected_HighPriority)--> [AssessThreat]
                                                  |
                                                  +--(ShouldFlee)--> [Flee] --> [SeekShelter]
                                                  |
                                                  +--(ShouldFight)--> [EngageThreat] --> [Attack]

ThreatDetected Condition:

Checks for nearby hostile entities, perhaps using a sphere cast or trigger colliders. Sets DetectedThreat on Blackboard.

AssessThreat State:

OnEnter: GetPopDataAction (gets Pop's health, combat stats), GetThreatDataAction (gets threat's stats from Blackboard.DetectedThreat).

ShouldFlee Condition:

Compares Pop's stats vs. Threat's stats. True if Pop is weaker or low health.

Flee Action:

Calculates a direction away from Blackboard.DetectedThreat.

Uses MoveToPositionAction.

6. Advanced Considerations for "Lineages"
Hierarchical State Machines (Sub-Graphs): For very complex behaviors like "Combat" or "Settlement Management," you can encapsulate a set of related states and transitions into a sub-graph. This keeps your main graph cleaner.

Example: A Combat state in the main graph could actually be a sub-graph containing states like ApproachEnemy, MeleeAttack, RangedAttack, Dodge, Block.

Dynamic Behavior Switching: As Pops evolve or take on different roles (e.g., Forager, Builder, Guard), you might want to switch their entire Behavior Graph asset or dynamically alter parameters within their current graph.

Evolutionary Impact:

Traits (from TraitSO.cs) can directly influence Blackboard variables used by actions/conditions (e.g., a trait for "BetterForager" could increase the ForagingEfficiency variable used by GatherResourceAction).

Some traits might unlock entirely new branches or sub-graphs of behavior.

Group Behaviors: For RTS elements, you might need Pops to coordinate. This is more advanced but could involve:

Shared Blackboard data among a group.

Leader/follower behaviors.

Actions that broadcast messages or signals to nearby friendly Pops.

7. Conclusion
Transitioning to the Unity Behavior system is a significant step that can greatly enhance the sophistication and manageability of your AI in "Lineages: Ancestral Legacies." Your existing StateEnums.cs, custom actions like GetEntityDataAction.cs and SetEntityStateAction.cs, and the EntityBehavior.asset show you're already on the right track.

Key Takeaways for Your Transition:

Leverage Existing Work: Your StateEnums, PopData, and custom actions are valuable starting points.

Iterate: Start with one or two simple behaviors and gradually expand. Don't try to convert everything at once.

Focus on Custom Actions and Conditions: This is where your game's unique mechanics will be implemented.

Use the Blackboard Effectively: Manage state and data flow within your graphs.

Test Continuously: Debugging visual graphs can be more intuitive than pure code.

Foraging & Hauling System Plan for Unity
This plan details the steps to implement a basic foraging and hauling loop for units ("Pops") in Lineages: Ancestral Legacies. The initial focus will be on gathering berries from bushes and depositing them at a Gathering Hut.

I. Prerequisites & Assumptions:
GameData System: The ScriptableObject-based GameData system (as previously planned) is available for defining items and resource nodes.

Basic Unit Prefab: A simple unit prefab exists or will be created.

Tilemap & Basic Pathfinding: A tilemap exists, and a basic pathfinding solution (e.g., A* placeholder or initial implementation) is available or being developed concurrently to allow units to move from point A to B.

InventoryManager (User Mentioned): An InventoryManager script/system exists. We will define how unit-specific inventories interact with it or function independently for now. The key is that units will have their own inventory capacity.

II. Core Components to Develop/Enhance:
Resource Node (Berry Bush):

ScriptableObject Definition: BerryBush_ResourceNode_SO (inheriting from ResourceNode_SO).

itemYield: Set to a Berries_ResourceItem_SO.

maxGatherings: Define how many times it can be gathered before depletion (or if it regenerates).

baseGatheringTime: Time it takes to gather one "batch" of berries.

Prefab: A visual representation of a berry bush.

Component (ResourceSource.cs):

Attached to the Berry Bush prefab.

Holds a reference to its ResourceNode_SO data.

Manages current available resources (e.g., int currentBerriesAvailable).

Handles depletion and potentially regeneration logic.

Provides a method like public Item_SO GatherResource(out int amountGathered).

Gatherable Item (Berries):

ScriptableObject Definition: Berries_ResourceItem_SO (inheriting from ResourceItem_SO).

itemName: "Berries"

icon: Sprite for berries.

maxStack: Relevant for inventory.

weight: If your inventory system uses weight.

Gathering Hut (Drop-off Point):

Prefab: A visual representation of a gathering hut.

Component (ResourceDropOff.cs):

Attached to the Gathering Hut prefab.

Acts as a target for units to deposit items.

Method: public void DepositItems(List<InventorySlotData> itemsToDeposit) (or similar, depending on your inventory structure). This method would then transfer items to a global resource pool or the hut's own storage.

Unit ("Pop") Enhancements:

Component (UnitInventory.cs or similar):

Attached to the Unit prefab.

Manages the unit's personal inventory (e.g., a List<InventorySlotData>).

public int inventoryCapacityThreshold; (as requested, for when to return). This could be item count, total weight, or a percentage of max capacity.

public int maxInventorySlots; / public float maxInventoryWeight;

Methods:

bool AddItem(Item_SO item, int quantity)

bool RemoveItem(Item_SO item, int quantity)

bool IsInventoryFull() (based on inventoryCapacityThreshold or absolute max)

List<InventorySlotData> GetContentsAndClear() (for depositing)

GetCurrentLoad() (to check against inventoryCapacityThreshold)

Component (ForagerAI.cs or UnitBehavior_Forage.cs):

Attached to the Unit prefab.

A state machine to manage the foraging/hauling behavior.

States:

IDLE

MOVING_TO_RESOURCE

GATHERING

MOVING_TO_DROPOFF

DEPOSITING

Needs references to pathfinding system, UnitInventory.

Needs methods to find the nearest available ResourceSource (berry bush) and ResourceDropOff (gathering hut).

III. Detailed Task List & Implementation Steps:
Phase 1: Data & Prefab Setup
Task: Define Core ScriptableObjects.

Create Berries_ResourceItem_SO asset in your project. Assign its properties (name, icon, etc.).

Create BerryBush_ResourceNode_SO asset.

Assign the Berries_ResourceItem_SO to its itemYield field.

Set baseGatheringTime, minYieldAmount, maxYieldAmount, maxGatherings.

Task: Create Basic Prefabs.

Berry Bush Prefab:

Create a simple 3D model or 2D sprite for a berry bush.

Add a Collider (e.g., SphereCollider or BoxCollider) to make it selectable/targetable.

Create and attach the ResourceSource.cs script.

In the Inspector for this prefab, assign the BerryBush_ResourceNode_SO to the script.

Gathering Hut Prefab:

Create a simple 3D model or 2D sprite for a gathering hut.

Add a Collider.

Create and attach the ResourceDropOff.cs script.

Unit Prefab (if not already done):

Ensure it has a basic visual and a Collider.

(Movement components will be assumed or added as part of pathfinding).

Phase 2: Unit Inventory & Basic AI Structure
Task: Implement UnitInventory.cs.

Define InventorySlotData (if not already part of your InventoryManager):

// Example:
[System.Serializable]
public class InventorySlotData
{
    public Item_SO item;
    public int quantity;
}

Implement the list to hold InventorySlotData.

Implement inventoryCapacityThreshold and maxInventorySlots/maxInventoryWeight.

Implement AddItem, RemoveItem, IsInventoryFull, GetCurrentLoad, and GetContentsAndClear.

Add this component to your Unit prefab. Configure its capacity in the Inspector.

Task: Implement ForagerAI.cs (State Machine Shell).

Define an enum ForagerState { IDLE, MOVING_TO_RESOURCE, GATHERING, MOVING_TO_DROPOFF, DEPOSITING }.

Add private ForagerState currentState;.

Add private ResourceSource currentTargetResource;.

Add private ResourceDropOff currentDropOffTarget;.

Add references for UnitInventory and your pathfinding component (e.g., NavMeshAgent if using Unity's NavMesh, or your custom pathfinder).

Create an Update() method that calls a function based on currentState (e.g., HandleIdleState(), HandleGatheringState()).

Add this component to your Unit prefab.

Phase 3: Resource Interaction Logic
Task: Implement ResourceSource.cs (Berry Bush script).

public ResourceNode_SO nodeData; (assigned in Inspector).

private int currentYieldLeft; (initialized from nodeData.maxGatherings or similar).

void Start(): Initialize currentYieldLeft.

public Item_SO GatherResource(out int amountGathered):

If currentYieldLeft <= 0, return null.

Decrement currentYieldLeft.

amountGathered = Random.Range(nodeData.minYieldAmount, nodeData.maxYieldAmount + 1);

Return nodeData.itemYield.

(Optional: If depleted, change visuals or disable the GameObject).

Task: Implement ResourceDropOff.cs (Gathering Hut script).

public void DepositItems(List<InventorySlotData> itemsToDeposit, UnitInventory depositorInventory):

Loop through itemsToDeposit.

For each item, add it to a global resource count or the hut's specific storage (e.g., GlobalResourceManager.Instance.AddResource(item.item, item.quantity);).

depositorInventory.ClearSpecificItems(itemsToDeposit) or the depositor calls its own clear method after this.

Provide feedback (debug log, visual effect).

Phase 4: Foraging AI State Implementation
Task: Implement ForagerAI.cs - State: IDLE & Find Resource.

HandleIdleState():

If inventory is full (unitInventory.IsInventoryFull()), transition to Find Drop-Off logic.

Else, scan for the nearest available ResourceSource (e.g., FindNearestBerryBush()).

FindNearestBerryBush(): Use Physics.OverlapSphere or a custom spatial partitioning system to find GameObjects with the ResourceSource component and the correct ResourceNode_SO type (or a tag). Filter out depleted ones.

If a bush is found, set currentTargetResource, and transition to MOVING_TO_RESOURCE.

Task: Implement ForagerAI.cs - State: MOVING_TO_RESOURCE.

HandleMovingToResourceState():

If currentTargetResource is null or depleted, transition back to IDLE (to find a new one).

Use pathfinding to move towards currentTargetResource.transform.position.

Once within a certain distance (e.g., interaction range), transition to GATHERING.

Task: Implement ForagerAI.cs - State: GATHERING.

HandleGatheringState():

Start a timer based on currentTargetResource.nodeData.baseGatheringTime.

When timer completes:

Call Item_SO gatheredItem = currentTargetResource.GatherResource(out int amount);.

If gatheredItem is not null:

unitInventory.AddItem(gatheredItem, amount);.

Log success.

Else (bush depleted or error), transition to IDLE.

If unitInventory.IsInventoryFull(), set currentTargetResource = null; and transition to Find Drop-Off logic (which will then lead to MOVING_TO_DROPOFF).

Else (can still gather more from this bush and not full), restart gathering timer or transition briefly to IDLE to re-evaluate (allows interruption).

Task: Implement ForagerAI.cs - State: Find Drop-Off & MOVING_TO_DROPOFF.

Find Drop-Off Logic (can be part of HandleIdleState or a separate method called when inventory is full):

Scan for the nearest ResourceDropOff (e.g., FindNearestGatheringHut()).

If found, set currentDropOffTarget and transition to MOVING_TO_DROPOFF.

HandleMovingToDropOffState():

If currentDropOffTarget is null, transition to IDLE (problem!).

Use pathfinding to move towards currentDropOffTarget.transform.position.

Once within interaction range, transition to DEPOSITING.

Task: Implement ForagerAI.cs - State: DEPOSITING.

HandleDepositingState():

Call currentDropOffTarget.DepositItems(unitInventory.GetContents(), unitInventory);.

The DepositItems method (or the unit itself after the call) should clear the unit's inventory. A robust way is for GetContentsAndClear() to return the items and empty the list simultaneously.

Set currentDropOffTarget = null;.

Transition to IDLE.

Phase 5: Testing & Iteration
Task: Scene Setup.

Place some Berry Bush prefabs and a Gathering Hut prefab on your tilemap.

Place a Unit prefab.

Task: Initial Testing.

Observe if the unit correctly identifies bushes, moves to them, "gathers" (check debug logs and inventory changes), moves to the hut when full, and deposits.

Use your debug console/tools extensively!

Task: Refine and Add Visuals.

Add simple animations or visual feedback for gathering (e.g., unit plays an animation, bush jiggles).

Add feedback for depositing.

Improve targeting logic (e.g., what if multiple units target the same bush?).

Consider what happens if a resource depletes while a unit is en route.

IV. Key Variables & Considerations:
inventoryCapacityThreshold: Ensure this is configurable on the UnitInventory (perhaps as a percentage of max capacity or a fixed item count/weight).

Finding Objects: The methods FindNearestBerryBush() and FindNearestGatheringHut() are critical.

Simple approach: GameObject.FindObjectsOfType<ResourceSource>() and iterate to find the closest. This is okay for few objects but inefficient for many.

Better approach: Maintain lists of available resources/drop-offs in a manager, or use spatial partitioning (e.g., a grid system, Quadtree/Octree) for faster querying.

Concurrency: What if multiple units want to gather from the same bush or use the same hut?

Initial: Let them. First come, first served.

Advanced: Reservation system (unit "claims" a target).

Interruption: Can units be interrupted (e.g., by an enemy attack)? The state machine should handle transitions out of current tasks.

Player Control: How are units assigned this task? (Manual assignment, designated "foraging zone," automatic behavior for idle units?) This plan focuses on the autonomous loop first.

This plan provides a solid roadmap, Scotty. Start simple with each step, get the core logic working with debug logs, and then layer on more complexity and polish. This will be a very satisfying system to see in action!
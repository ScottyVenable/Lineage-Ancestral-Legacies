# NavMesh Setup Guide for Pop Entities

## Overview
Your Pop entities are now fully configured for NavMesh navigation! This guide will help you set up the NavMesh in your Unity scene so your Pops can move around properly.

## Pop Entity NavMesh Integration ✅ COMPLETED

### What's Already Done:
1. **Pop.cs** - Added `[RequireComponent(typeof(NavMeshAgent))]` attribute
2. **Pop.cs** - NavMeshAgent component reference and initialization
3. **Pop.cs** - NavMeshAgent configuration with appropriate settings
4. **Pop.cs** - Complete navigation API with methods:
   - `MoveTo(Vector3 targetPosition)` - Move to world position
   - `MoveTo(Transform target)` - Move to transform location
   - `StopMovement()` - Stop current movement
   - `IsMoving()` - Check if currently moving
   - `HasReachedDestination()` - Check if reached destination
   - `GetMovementSpeed()` - Get current movement speed

5. **PopController.cs** - Enhanced with NavMeshAgent integration
6. **SelectionManager.cs** - Already configured to use PopController.MoveTo() for right-click movement

## Unity Scene Setup (Steps You Need to Do)

### Step 1: Install AI Navigation Package (if not already installed)
1. Open **Window → Package Manager**
2. Change dropdown from "In Project" to "Unity Registry"
3. Search for "AI Navigation"
4. Click **Install**

### Step 2: Mark Terrain/Ground Objects as Navigation Static
1. Select your ground/terrain objects in the scene
2. In the **Inspector**, click the **Static** dropdown (top right)
3. Check **Navigation Static**
4. Do this for all walkable surfaces (floors, platforms, ramps, etc.)

### Step 3: Set Up Navigation Areas (Optional)
1. Go to **Window → AI → Navigation**
2. Click the **Areas** tab
3. You can define different area types like:
   - Walkable (default, cost: 1)
   - Not Walkable (cost: 1, but agents avoid)
   - Jump (cost: 2, for areas requiring jumping)

### Step 4: Bake the NavMesh
1. Go to **Window → AI → Navigation** 
2. Click the **Bake** tab
3. Configure settings:
   - **Agent Radius**: 0.5 (for character width)
   - **Agent Height**: 2.0 (for character height)
   - **Max Slope**: 45 (maximum walkable slope)
   - **Step Height**: 0.4 (max step/curb height)
4. Click **Bake** button
5. You should see blue overlay on walkable areas

### Step 5: Configure Your Pop Prefabs
1. Select your Pop prefab
2. Ensure it has a **NavMeshAgent** component (should auto-add due to RequireComponent)
3. Configure NavMeshAgent settings:
   - **Speed**: 3.5 (already set in code)
   - **Angular Speed**: 120 (already set in code)
   - **Acceleration**: 8 (already set in code)
   - **Stopping Distance**: 0.5 (already set in code)
   - **Auto Braking**: True (already set in code)

### Step 6: Test Pop Movement
1. Place Pop entities in your scene
2. Make sure they're positioned on the NavMesh (blue areas)
3. In Play mode:
   - Left-click to select Pops
   - Right-click on walkable areas to command movement
   - Pops should pathfind and move to the target location

## Navigation API Usage

### Basic Movement
```csharp
// Move to a specific position
pop.MoveTo(new Vector3(10, 0, 5));

// Move to another object
pop.MoveTo(targetObject.transform);

// Stop movement
pop.StopMovement();
```

### Movement Checking
```csharp
// Check if pop is currently moving
if (pop.IsMoving())
{
    Debug.Log("Pop is moving");
}

// Check if pop reached destination
if (pop.HasReachedDestination())
{
    Debug.Log("Pop reached destination");
}

// Get current speed
float speed = pop.GetMovementSpeed();
```

### Through PopController
```csharp
PopController controller = pop.GetComponent<PopController>();
controller.MoveTo(targetPosition);
```

## Troubleshooting

### Pop Won't Move
1. **Check NavMesh**: Ensure the destination is on the blue NavMesh areas
2. **Check Agent Status**: Make sure `agent.isOnNavMesh` is true
3. **Check Agent Enabled**: Ensure `agent.isActiveAndEnabled` is true
4. **Check Console**: Look for warning messages about NavMeshAgent status

### Pop Moves Strangely
1. **Adjust Agent Settings**: Try different values for speed, acceleration, angular speed
2. **Check NavMesh Quality**: Rebake with different settings if pathfinding is poor
3. **Check Obstacles**: Ensure NavMeshObstacle components are properly configured

### Performance Issues
1. **Limit Agent Count**: Too many agents can impact performance
2. **Reduce Update Frequency**: Consider updating agent destinations less frequently
3. **Use NavMesh Obstacles**: For dynamic objects that should block paths

## Advanced Features

### Navigation Obstacles
For dynamic objects that should block agent paths:
1. Add **NavMeshObstacle** component
2. Configure **Shape** and **Size**
3. Enable **Carve** for moving obstacles

### Off-Mesh Links
For connections between separated NavMesh areas:
1. Add **NavMeshLink** component
2. Set **Start Point** and **End Point**
3. Configure **Area Type** and **Bidirectional**

### Custom Navigation Areas
For different movement costs (water, mud, etc.):
1. Create custom Navigation Areas in the Navigation window
2. Assign different costs
3. Paint areas on your terrain/objects

## Integration with Unity Behavior System

Your Pops are ready to work with Unity's Behavior system. The NavigateToTargetAction already has proper validation and fallback handling for NavMeshAgent issues.

## Next Steps

1. Set up your scene's NavMesh following the steps above
2. Test Pop movement with selection and right-click commands
3. Consider creating behavior trees using Unity Behavior package for complex AI
4. Add navigation-based actions to your Behavior trees using the Pop's navigation API

Your Pop entities are now fully NavMesh-ready! 🎉

# Entity Behavior Tree Setup Guide

## Quick Start Guide

Follow these steps to implement the comprehensive behavior tree system in your Lineage Ancestral Legacies project.

### 1. Prerequisites

Ensure you have:
- Unity Behavior package installed
- Your existing `Entity` component system working
- NavMesh baked in your scenes
- Entity GameObjects with `NavMeshAgent` components

### 2. Component Setup

For each entity that needs AI behavior:

1. **Add Required Components**:
   ```csharp
   // Core behavior components
   BehaviorAuthoring
   EntityBehaviorConfigurator
   BehaviorTreeDebugger (optional, for testing)
     // Ensure these exist
   Entity
   NavMeshAgent
   ```

2. **Configure EntityBehaviorConfigurator**:
   - Set `Behavior Type` (Pop, Animal, Monster, NPC)
   - Assign appropriate `Behavior Graph` asset (if using Behavior Graphs)
   - Adjust threshold values based on entity type
   - Set search/wander radius values

### 3. Behavior Graph Assets

Create behavior graph assets for different entity types:

1. **Entity Basic Behavior** (provided template):
   - Focuses on needs management (hunger, thirst, energy)
   - Includes interactions based on the type of entity. (Animals dont need to craft, Pops don't need to graze, etc.)
   - Default exploration, idle, and common behaviors universal to all entities

2. **Animal Behavior** (create new):
   - Get its tags ("carnivore", "herbivore", etc. and assign it)
   - Hunting and patrolling patterns based on subtype (Carnivores may hunt when hungry)
   - Flee behavior when threatened (maybe adding what the flee threshold is for an entity in the Database data at some point.)
   - Territory-based movement

3. **Monster Behavior** (create new):
   - Aggressive hunting patterns
   - Attack nearest targets
   - Patrol designated areas
   - Behaviors for AI types (how does a wolf attack/behave vs a bandit?)

### 4. Resource Tagging

For resource gathering to work, tag your resource objects:
- "Food" - for food resources, maybe with a subtag of if it can be Foraged or needs to be Harvested with certain tools or skills.
- "Water" - for water sources, and what is required to collect it. Is the water dirty or clean?
- "Gatherable" - for general resources like wood or stone.
- "Crafting" - for crafting materials, may still be for Gatherable items but the item itself might have "crafting_component" tag if it is needed for crafting.

### 5. Testing Setup

1. **Add BehaviorTreeDebugger** to test entities
2. Use manual testing buttons to trigger:
   - Critical hunger/thirst/energy states
   - Restore all needs for testing
3. Enable debug logging to monitor behavior changes
4. Use scene gizmos to visualize entity states

### 6. Blackboard Variables

Each behavior graph should include these variables:
```
Core Variables:
- Self (GameObject) - Reference to the entity
- CriticalHungerThreshold (float) - When to prioritize food
- CriticalThirstThreshold (float) - When to prioritize water
- CriticalEnergyThreshold (float) - When to rest
- SearchRadius (float) - How far to look for resources
- WanderRadius (float) - How far to explore

Target Variables:
- FoundResource (GameObject) - Current resource target
- SocialTarget (GameObject) - Current social interaction target
- WanderPosition (Vector3) - Current exploration destination
```

## Behavior Tree Examples

### Basic Pop Decision Tree

```
Selector (Main)
├── Selector (Critical Needs)
│   ├── Sequence (Hunger < 20)
│   │   ├── FindNearestResource (Food)
│   │   ├── MoveToPosition
│   │   └── GatherResource
│   ├── Sequence (Thirst < 15)
│   │   ├── FindNearestResource (Water)
│   │   ├── MoveToPosition
│   │   └── FulfillEntityNeed (Thirst)
│   └── Sequence (Energy < 10)
│       └── RestEntity
├── Selector (Normal Activities)
│   ├── Sequence (Resource Gathering)
│   │   ├── CheckEntityStat (Energy > 30)
│   │   ├── FindNearestResource (Gatherable)
│   │   └── GatherResource
│   ├── Sequence (Social Interaction)
│   │   ├── FindSocialTarget
│   │   └── SocializeWith
│   └── WanderAround
└── WaitRandom (Idle)
```

### Animal Hunting Behavior

```
Selector (Main)
├── Sequence (Low Health - Flee)
│   ├── CheckEntityStat (Health < 25)
│   └── WanderAround (Fast, away from threats)
├── Sequence (Hunt for Food)
│   ├── CheckEntityStat (Hunger < 50)
│   ├── FindNearestResource (Prey)
│   ├── MoveToPosition
│   └── SetEntityState (Hunting)
├── PatrolArea (Territory)
└── RestEntity
```

## Customization Tips

### Creating Custom Behaviors

1. **Extend Action Classes**:
   ```csharp
   [NodeDescription(name: "CustomAction", story: "Custom [Agent] behavior")]
   public partial class CustomAction : Action
   {
       // Your custom logic here
   }
   ```

2. **Add New Conditions**:
   ```csharp
   [NodeDescription(name: "CustomCondition", story: "[Agent] meets custom criteria")]
   public partial class CustomCondition : Condition
   {
       // Your condition logic here
   }
   ```

3. **Entity-Specific Parameters**:
   - Use EntityBehaviorConfigurator to set entity-specific values
   - Modify blackboard variables at runtime based on entity progression
   - Create behavior variations for different entity specializations

### Performance Optimization

1. **Reduce Update Frequency**:
   - Use longer duration actions where appropriate
   - Implement behavior state caching
   - Use events instead of constant polling

2. **Efficient Resource Finding**:
   - Consider spatial partitioning for large numbers of resources
   - Cache nearby resources when possible
   - Use Unity's Job System for complex calculations

### Integration with Game Systems

1. **Inventory System**:
   - Extend gathering actions to add items to inventory
   - Create crafting behavior sequences
   - Implement resource drop-off behaviors

2. **Combat System**:
   - Add health checking conditions
   - Implement attack/defend state machines
   - Create threat detection and response behaviors

3. **Building System**:
   - Add construction behaviors
   - Implement hauling materials to build sites
   - Create collaborative building patterns

## Troubleshooting

### Common Issues

1. **Entities Not Moving**:
   - Check NavMeshAgent is enabled
   - Verify NavMesh is baked in scene
   - Ensure Entity component is accessible

2. **Behaviors Not Triggering**:
   - Verify EntityDataComponent is initialized
   - Check stat threshold values
   - Enable debug logging to trace decisions

3. **Performance Issues**:
   - Reduce search radius values
   - Increase action durations
   - Limit number of entities with complex behaviors

### Debug Tools

1. **Use BehaviorTreeDebugger** for real-time monitoring
2. **Check Unity Behavior Debugger** window for graph execution
3. **Enable Entity component event logging** for stat changes
4. **Use Scene view gizmos** to visualize entity states and ranges

## Next Steps

1. **Test Basic Setup**: Start with simple Entity behaviors
2. **Expand Gradually**: Add more complex behaviors as needed
3. **Performance Testing**: Monitor with multiple entities
4. **Customize for Game**: Adapt behaviors to your specific gameplay needs
5. **Player Feedback**: Adjust AI based on player experience

This behavior tree system provides a solid foundation that can be expanded as your game grows. The modular design allows you to add new behaviors without breaking existing functionality.

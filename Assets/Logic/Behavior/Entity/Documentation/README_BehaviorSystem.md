# Unity Behavior Tree System for Lineage Ancestral Legacies

## Overview

This comprehensive behavior tree system leverages Unity's Behavior package to create intelligent AI for your entity system. It fully integrates with your existing `Entity` component system and Database.Entity data structures.

## Architecture

### Core Components

1. **State Management**: Extended `StateEnums.cs` to include all 22 states from your database
2. **Data Integration**: Actions that directly interact with `Entity` component
3. **Modular Behaviors**: Reusable behavior components for different AI patterns
4. **Stat-Driven AI**: Conditions based on entity stats (health, hunger, thirst, energy, etc.)

## Behavior Node Categories

### 1. Core Actions
- `GetEntityDataAction`: Retrieves entity data from GameObjects
- `SetEntityStateAction`: Changes entity states in the database system
- `ModifyEntityStatAction`: Modifies entity stats (health, mana, hunger, etc.)

### 2. Condition Nodes
- `CheckEntityStatCondition`: Checks if stats meet criteria (hunger < 20, energy > 50, etc.)
- `CheckEntityStateCondition`: Checks current entity state

### 3. Movement Actions
- `MoveToPositionAction`: NavMesh-based movement to specific positions
- `WanderAroundAction`: Random exploration within radius
- `PatrolAreaAction`: Patrol between waypoints

### 4. Needs Management
- `ProcessEntityNeedsAction`: Handles ongoing needs decay (hunger, thirst, energy, rest)
- `FulfillEntityNeedAction`: Restores specific needs (eating, drinking, resting)
- `RestEntityAction`: Specialized resting behavior with stat recovery

### 5. Resource Gathering
- `FindNearestResourceAction`: Locates resources by tag within search radius
- `GatherResourceAction`: Performs resource gathering with energy costs

### 6. Social Interactions
- `FindSocialTargetAction`: Finds other entities for social interaction
- `SocializeWithAction`: Handles social behavior between entities

### 7. Utility Actions
- `WaitRandomAction`: Random duration waiting/idle behavior

## Behavior Tree Templates

### Basic Entity Behavior Structure
```
Start
└── Selector (Main Decision Tree)
    ├── Selector (Critical Needs - Highest Priority)
    │   ├── Sequence (Critical Hunger)
    │   │   ├── CheckEntityStat (Hunger < 20)
    │   │   ├── FindNearestResource (Food)
    │   │   ├── MoveToPosition (Food Location)
    │   │   └── FulfillEntityNeed (Hunger)
    │   ├── Sequence (Critical Thirst)
    │   │   ├── CheckEntityStat (Thirst < 15)
    │   │   ├── FindNearestResource (Water)
    │   │   ├── MoveToPosition (Water Location)
    │   │   └── FulfillEntityNeed (Thirst)
    │   └── Sequence (Critical Energy)
    │       ├── CheckEntityStat (Energy < 10)
    │       └── RestEntity
    ├── Selector (Normal Behaviors)
    │   ├── Sequence (Resource Gathering)
    │   │   ├── CheckEntityStat (Energy > 30)
    │   │   ├── FindNearestResource (Gatherable)
    │   │   ├── MoveToPosition (Resource Location)
    │   │   └── GatherResource
    │   ├── Sequence (Social Interaction)
    │   │   ├── CheckEntityStat (Charisma > 20)
    │   │   ├── FindSocialTarget
    │   │   ├── MoveToPosition (Social Target)
    │   │   └── SocializeWith
    │   └── Sequence (Exploration)
    │       ├── CheckEntityStat (Energy > 20)
    │       └── WanderAround
    └── WaitRandom (Default Idle)
```

## Entity Type Variations

### Pop Entities (Civilian)
- **Primary Behaviors**: Gathering, Crafting, Socializing, Resting
- **Decision Factors**: Hunger, Thirst, Energy, Rest, Social needs
- **States Used**: Idle, Gathering, Crafting, Socializing, Resting, Exploring

### Animal Entities
- **Primary Behaviors**: Hunting, Patrolling, Fleeing, Resting
- **Decision Factors**: Health, Energy, Aggression level, Entity subtypes (carnivore, herbivore)
- **Subtype Behaviors**: 
  - Carnivores may hunt when hungry
  - Herbivores may graze on vegetation
  - Flee thresholds based on entity configuration
- **States Used**: Idle, Hunting, Patrolling, Fleeing, Resting, Hiding

### Monster Entities (Aggressive)
- **Primary Behaviors**: Hunting, Attacking, Patrolling
- **Decision Factors**: Health, Attack power, Aggression type, AI subtype (wolf vs bandit behavior)
- **Subtype Behaviors**:
  - Wolf: Pack hunting, territorial behavior
  - Bandit: Tool-based attacks, strategic positioning
- **States Used**: Idle, Attacking, Hunting, Patrolling, Fleeing (when low health)

## Integration Points

### Entity Component Integration
All behavior actions automatically:
- Access entity stats through `Entity.GetStat()`
- Modify stats through `Entity.ModifyStat()`
- Change states through `Entity.ChangeState()` 
- Listen to state changes via entity events

### NavMesh Integration
- Movement actions use NavMesh components for pathfinding
- Maintains compatibility with existing selection and UI systems

### Database System Integration
- Direct mapping between `BehaviorState` enum and `Database.State.ID`
- Full access to all 19 stat types from the database
- Leverages entity type and aggression type data

### Resource Tagging System
The behavior system uses a comprehensive tagging system for resource identification:
- **"Food"** - Food resources with subtags for collection requirements:
  - Foraging (no tools required)
  - Harvesting (tools/skills required)
- **"Water"** - Water sources with quality indicators:
  - Clean water (immediate consumption)
  - Dirty water (requires processing). Potential debuff of getting sick if consuming. 
- **"Gatherable"** - General resources (wood, stone, etc.)
- **"Crafting"** - Crafting materials with "crafting_component" subtags for recipe requirements

## Stat-Based Decision Making

The system uses entity stats to drive intelligent behavior:

```csharp
// Example: Entity decides to rest when energy is low
CheckEntityStat: Energy < 20 → RestEntity (TODO: Decide if rest is different then sleep?)
CheckEntityStat: Hunger < 30 → FindFood → GatherResource
CheckEntityStat: Thirst < 25 → FindWater → DrinkWater
```

## Blackboard Variable Configuration

Each behavior graph utilizes these essential variables:

### Core Variables:
- **Self** (GameObject) - Reference to the entity
- **CriticalHungerThreshold** (float) - When to prioritize food seeking either from settlement storage or from gathering nearby.
- **CriticalThirstThreshold** (float) - When to prioritize water seeking  
- **CriticalEnergyThreshold** (float) - When to initiate rest behavior
- **SearchRadius** (float) - Maximum distance for resource detection
- **WanderRadius** (float) - Exploration boundary limits

### Target Variables:
- **FoundResource** (GameObject) - Current resource target
- **SocialTarget** (GameObject) - Current social interaction target  
- **WanderPosition** (Vector3) - Current exploration destination

## Customization Examples

### Creating Custom Behavior Patterns

1. **Craftsman Entity Behavior**:
   - High priority on finding crafting materials
   - Prefers crafting when materials available
   - Social interaction during breaks

2. **Guard Entity Behavior**:
   - Patrol specific areas
   - React to threats
   - Alert other entities

3. **Explorer Entity Behavior**:
   - Extended wandering radius
   - Resource discovery focus
   - Map exploration patterns

4. **Gatherer Entity Behavior**:
   - High priority on resource collection
   - Efficient path planning between resource nodes
   - Energy management for sustained gatheringPriority

5. **Hunter Entity Behavior**:
   - High priority on tracking and hunting animal entities either tagged by the player or generalized.
   - Energy-efficient stalking patterns to conserve stamina
   - Tool/weapon requirements for successful hunts
   - Return to settlement with hunted resources
   - Avoid dangerous monsters while hunting weaker prey

## Performance Considerations

- **Stat Checking**: Efficient direct access to Entity component
- **NavMesh Integration**: Leverages Unity's optimized pathfinding
- **Event-Driven**: Uses events for stat changes to minimize polling
- **Modular Design**: Reusable components reduce memory overhead

## Future Enhancements

1. **Inventory Integration**: Add inventory-aware gathering behaviors
2. **Combat System**: Expand attack/defend behaviors with damage calculations
3. **Building System**: Add construction and building behaviors
4. **Advanced AI**: Implement planning systems for long-term goals
5. **Social Dynamics**: Complex relationship systems affecting behavior choices

## Usage Instructions

1. **Setup**: Attach `BehaviorAuthoring` component to entity GameObjects
2. **Configuration**: Assign appropriate behavior graph assets (PopBasicBehavior, etc.)
3. **Blackboard**: Configure entity-specific variables in the behavior graph
4. **Testing**: Use debug logs to monitor behavior transitions and stat changes

This system provides a robust foundation for entity AI that scales from simple idle behaviors to complex multi-agent interactions, all while leveraging your existing game systems and data structures.

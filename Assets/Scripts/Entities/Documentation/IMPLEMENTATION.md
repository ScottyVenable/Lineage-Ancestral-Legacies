# Universal Entity System - Implementation Guide

## 🎯 Overview

This guide provides step-by-step instructions for implementing the Universal Entity System in your Unity project. The system replaces entity-specific scripts (like `Pop.cs`, `Wolf.cs`) with a single configurable `Entity.cs` component that handles all entity types through `EntityTypeData` ScriptableObject assets.

## 🏗️ System Architecture

```
Universal Entity System
├── Entity.cs (Universal Component)
├── EntityTypeData.cs (Configuration ScriptableObject)
├── Database.cs (Data Structures)
└── Blackboard Integration (Behavior Trees)
```

### Key Benefits
- ✅ **Single Component**: One `Entity.cs` handles all entity types
- ✅ **Configuration-Driven**: No coding required for new entity types
- ✅ **Designer-Friendly**: Visual configuration through Unity Inspector
- ✅ **Performance Optimized**: Efficient stat management and event system
- ✅ **AI-Ready**: Built-in behavior tree integration

---

## 📋 Prerequisites

Before implementing, ensure you have:

1. **Required Scripts**:
   - `Entity.cs` in `Assets/Scripts/Entities/`
   - `EntityTypeData.cs` in `Assets/Scripts/Entities/`
   - `Database.cs` in `Assets/Scripts/Systems/Data/`

2. **Required Folders**:
   ```
   Assets/Data/EntityTypes/     # For EntityTypeData assets
   Assets/Prefabs/Entities/     # For entity prefabs
   ```

3. **Unity Components** (optional but recommended):
   - NavMeshAgent (for movement)
   - SpriteRenderer or MeshRenderer (for visuals)
   - Animator (for animations)
   - InventoryComponent (for inventory systems)

---

## 🚀 Quick Start Implementation

### Step 1: Create EntityTypeData Asset

1. **Right-click** in `Assets/Data/EntityTypes/`
2. Select **Create > Lineage > Entity Type Data**
3. Name it descriptively (e.g., `WolfBehavior`, `PopBehavior`)

### Step 2: Configure Entity Type

In the EntityTypeData Inspector:

```yaml
Entity Type Configuration:
  Entity Type: Animal           # Pop, Animal, Monster, NPC
  Entity Type Name: "Wolf"

Behavior Configuration:
  Has Needs Decay: true
  Has Aging: true
  Can Craft: false
  Can Socialize: false
  Can Gather: true
  Has Territory: true           # Wolf-specific
  Can Hunt: true               # Wolf-specific
  Can Flee: true

Behavior Subtypes:
  - "carnivore"
  - "wolf"
  - "pack"

Enhanced Resource Configuration:
  - "Food"
  - "Water"
  - "Meat"

Needs Decay Rates:
  Hunger Decay Rate: 1.0
  Thirst Decay Rate: 1.5
  Energy Decay Rate: 0.8
  Rest Decay Rate: 0.5

Combat Configuration:
  Flee Health Threshold: 25.0
  Aggression Level: 75.0

Movement Configuration:
  Wander Radius: 15.0
  Movement Speed: 5.5
  Territory Radius: 25.0
```

### Step 3: Create Entity Prefab

1. Create a **GameObject** in the scene
2. Add the **Entity component**
3. Assign your **EntityTypeData** asset to the `Entity Type Data` field
4. Set **Entity ID** (0 for new entities, specific ID for database entities)
5. Add optional components:
   - **NavMeshAgent** (for AI movement)
   - **SpriteRenderer** (for 2D visuals)
   - **Animator** (for animations)

### Step 4: Save as Prefab

1. Drag the configured GameObject to `Assets/Prefabs/Entities/`
2. Name it descriptively (e.g., `Wolf_Prefab`)
3. Delete the original GameObject from the scene

---

## 🐺 Complete Wolf Example

Let's walk through creating a complete Wolf entity from scratch:

### 1. Create Wolf EntityTypeData

**File**: `Assets/Data/EntityTypes/WolfBehavior.asset`

**Configuration**:
```yaml
Entity Type Configuration:
  Entity Type: Animal
  Entity Type Name: "Forest Wolf"

Behavior Configuration:
  Has Needs Decay: true
  Has Aging: true
  Can Craft: false
  Can Socialize: true          # Wolves are pack animals
  Can Gather: false           # Wolves hunt, don't gather
  Has Territory: true
  Can Hunt: true
  Can Flee: true

Behavior Subtypes:
  - "carnivore"
  - "wolf"
  - "pack"
  - "territorial"

Enhanced Resource Configuration:
  - "Food"
  - "Water"
  - "Meat"
  - "Territory"

Blackboard Configuration:
  Custom Variables:
    - Key: "PackSize", Type: Int, Value: 3
    - Key: "TerritoryCenter", Type: String, Value: "Forest_01"
    - Key: "HuntingRange", Type: Float, Value: 50.0
    - Key: "IsAlpha", Type: Bool, Value: false

Needs Decay Rates:
  Hunger Decay Rate: 1.2      # Wolves get hungry faster
  Thirst Decay Rate: 1.0
  Energy Decay Rate: 0.9
  Rest Decay Rate: 0.6

Aging Configuration:
  Aging Rate: 1.0
  Max Age: 80                 # Wolf lifespan in game time

Combat Configuration:
  Flee Health Threshold: 20.0  # Wolves are brave
  Aggression Level: 80.0       # High aggression

Movement Configuration:
  Wander Radius: 20.0
  Movement Speed: 6.0          # Faster than humans
  Territory Radius: 40.0       # Large territory

Social Configuration:
  Social Radius: 10.0
  Max Social Group: 8          # Pack size limit

Resource Gathering:
  Preferred Resources:
    - Meat
  Gathering Efficiency: 1.5    # Good at getting meat
```

### 2. Create Wolf Prefab

**Setup Process**:

1. **Create GameObject**: Name it "Wolf"
2. **Add Entity Component**:
   ```
   Entity Configuration:
     Entity ID: 0 (for new wolf)
     Entity Type Data: WolfBehavior (assign the asset)
   ```

3. **Add Visual Components**:
   ```csharp
   // Add SpriteRenderer for 2D or MeshRenderer for 3D
   SpriteRenderer wolfSprite = wolf.AddComponent<SpriteRenderer>();
   wolfSprite.sprite = yourWolfSprite;
   
   // Add Animator for animations
   Animator wolfAnimator = wolf.AddComponent<Animator>();
   wolfAnimator.runtimeAnimatorController = yourWolfAnimatorController;
   ```

4. **Add AI Components**:
   ```csharp
   // Add NavMeshAgent for movement
   NavMeshAgent navAgent = wolf.AddComponent<NavMeshAgent>();
   navAgent.speed = 6.0f;
   navAgent.acceleration = 12.0f;
   navAgent.angularSpeed = 240.0f;
   navAgent.stoppingDistance = 1.0f;
   
   // Optional: Add colliders
   CapsuleCollider wolfCollider = wolf.AddComponent<CapsuleCollider>();
   wolfCollider.isTrigger = false;
   wolfCollider.radius = 0.5f;
   wolfCollider.height = 1.0f;
   ```

5. **Configure Transform**:
   ```
   Position: (0, 0, 0)
   Rotation: (0, 0, 0)
   Scale: (1, 1, 1)
   ```

### 3. Runtime Behavior

Once spawned, the Wolf will automatically:

- ✅ **Initialize** with pack behavior and territorial instincts
- ✅ **Hunt** for food when hungry (due to carnivore tag)
- ✅ **Patrol** its territory (due to hasTerritory = true)
- ✅ **Form packs** with other wolves (due to pack tag)
- ✅ **Flee** when health drops below 20%
- ✅ **Age** over time and eventually die of old age
- ✅ **Update blackboard** variables for behavior tree integration

---

## 🧑‍💻 Developer Implementation Checklist

### For Each New Entity Type:

#### ✅ **Phase 1: Configuration**
- [ ] Create EntityTypeData asset in `Assets/Data/EntityTypes/`
- [ ] Set appropriate EntityType enum (Pop, Animal, Monster, NPC)
- [ ] Configure behavior flags (canHunt, hasTerritory, etc.)
- [ ] Add behavior subtags for specialization
- [ ] Set resource tags for AI interaction
- [ ] Configure decay rates and thresholds
- [ ] Add custom blackboard variables if needed

#### ✅ **Phase 2: Prefab Creation**
- [ ] Create GameObject with descriptive name
- [ ] Add Entity component
- [ ] Assign EntityTypeData asset
- [ ] Set Entity ID (0 for new, specific for existing)
- [ ] Add visual components (SpriteRenderer/MeshRenderer)
- [ ] Add NavMeshAgent for AI movement
- [ ] Add Animator for animations
- [ ] Add colliders for physics interaction
- [ ] Save as prefab in `Assets/Prefabs/Entities/`

#### ✅ **Phase 3: Testing**
- [ ] Spawn prefab in test scene
- [ ] Verify Entity.isInitialized becomes true
- [ ] Check that stats are properly loaded
- [ ] Test behavior subtags are applied
- [ ] Verify resource tags are set
- [ ] Test stat modification (health, hunger, etc.)
- [ ] Check blackboard variables are populated
- [ ] Test needs decay if enabled
- [ ] Verify type-specific behaviors trigger

#### ✅ **Phase 4: Integration**
- [ ] Test with behavior tree system (if using)
- [ ] Verify database integration works
- [ ] Check event system fires correctly
- [ ] Test with other entities for interaction
- [ ] Performance test with multiple instances

---

## 🔧 Advanced Configuration Examples

### Example 1: Bandit Monster

```yaml
# Assets/Data/EntityTypes/BanditBehavior.asset
Entity Type: Monster
Entity Type Name: "Forest Bandit"

Behavior Subtypes:
  - "bandit"
  - "opportunistic"
  - "raider"

Resource Tags:
  - "Loot"
  - "Weapons"
  - "Gold"

Custom Blackboard Variables:
  - Key: "StealChance", Type: Float, Value: 0.3
  - Key: "FleeWhenOutnumbered", Type: Bool, Value: true
  - Key: "PreferredTarget", Type: String, Value: "Pop"

Aggression Level: 90.0
Flee Health Threshold: 15.0
```

### Example 2: Merchant NPC

```yaml
# Assets/Data/EntityTypes/MerchantBehavior.asset
Entity Type: NPC
Entity Type Name: "Traveling Merchant"

Behavior Configuration:
  Can Craft: false
  Can Socialize: true
  Has Territory: false
  Can Hunt: false

Behavior Subtypes:
  - "trader"
  - "peaceful"
  - "traveler"

Resource Tags:
  - "Trade"
  - "Gold"
  - "Goods"

Custom Blackboard Variables:
  - Key: "TradeRadius", Type: Float, Value: 15.0
  - Key: "MinTradeValue", Type: Int, Value: 10
  - Key: "HasRareItems", Type: Bool, Value: true

Social Configuration:
  Social Radius: 8.0
  Max Social Group: 12
```

### Example 3: Player-Controlled Pop

```yaml
# Assets/Data/EntityTypes/PlayerPopBehavior.asset
Entity Type: Pop
Entity Type Name: "Player Character"

Behavior Configuration:
  Has Needs Decay: true        # Player needs to eat/drink
  Has Aging: false            # Player doesn't age
  Can Craft: true
  Can Socialize: true
  Can Gather: true
  Has Territory: false

Behavior Subtypes:
  - "player"
  - "crafter"
  - "explorer"

Resource Tags:
  - "Food"
  - "Water"
  - "Gatherable"
  - "Crafting"
  - "Tools"

Needs Decay Rates:
  Hunger Decay Rate: 0.8      # Slower decay for player
  Thirst Decay Rate: 1.0
  Energy Decay Rate: 0.6
  Rest Decay Rate: 0.4

Custom Blackboard Variables:
  - Key: "IsPlayer", Type: Bool, Value: true
  - Key: "QuestProgress", Type: Int, Value: 0
  - Key: "ExperiencePoints", Type: Float, Value: 0.0
```

---

## 🎮 Runtime Usage Examples

### Spawning Entities via Code

```csharp
using UnityEngine;
using Lineage.Entities;

public class EntitySpawner : MonoBehaviour
{
    [SerializeField] private GameObject wolfPrefab;
    [SerializeField] private EntityTypeData wolfBehavior;
    
    public void SpawnWolf()
    {
        // Instantiate the prefab
        GameObject newWolf = Instantiate(wolfPrefab, transform.position, Quaternion.identity);
        
        // Get the Entity component
        Entity wolfEntity = newWolf.GetComponent<Entity>();
        
        // Configure if needed (usually done in prefab)
        wolfEntity.ConfigureAsEntity(0, wolfBehavior);
        
        // The entity will automatically initialize and begin behaving
        Debug.Log($"Spawned wolf: {wolfEntity.EntityName}");
    }
}
```

### Modifying Entity Stats

```csharp
public void FeedWolf(Entity wolfEntity)
{
    // Restore hunger
    wolfEntity.ModifyStatEnhanced(Stat.ID.Hunger, 30f, "Fed by player");
    
    // Check wolf's condition
    if (wolfEntity.Health < 50f)
    {
        wolfEntity.ModifyStatEnhanced(Stat.ID.Health, 20f, "Healing from food");
    }
    
    // Add friendship resource tag
    wolfEntity.AddResourceTag("Friendly");
}
```

### Behavior Tree Integration

```csharp
public class WolfBehaviorNode : MonoBehaviour
{
    private Entity wolfEntity;
    
    void Start()
    {
        wolfEntity = GetComponent<Entity>();
    }
    
    public bool ShouldHunt()
    {
        // Access blackboard variables
        float hunger = wolfEntity.GetBlackboardVariable<float>("Hunger", 100f);
        bool isInPack = wolfEntity.GetBlackboardVariable<bool>("IsInPack", false);
        
        // Wolf hunts when hungry and in pack
        return hunger < 30f && isInPack;
    }
    
    public void StartHunting()
    {
        // Change behavior state
        wolfEntity.ChangeBehaviorState("Hunting");
        
        // Update blackboard
        wolfEntity.SetBlackboardVariable("IsHunting", true);
        wolfEntity.SetBlackboardVariable("HuntStartTime", Time.time);
    }
}
```

---

## 🐛 Troubleshooting Guide

### Common Issues & Solutions

#### ❌ **Issue**: Entity doesn't initialize
**Symptoms**: `entity.isInitialized` remains false
**Solutions**:
- Ensure EntityTypeData is assigned in inspector
- Check that Entity ID is set (0 for new entities)
- Verify Database.cs is properly configured
- Check console for initialization errors

#### ❌ **Issue**: Stats not updating
**Symptoms**: ModifyStat calls don't change values
**Solutions**:
- Use `ModifyStat()` or `ModifyStatEnhanced()` methods
- Don't modify serialized fields directly
- Check that stat exists in Database.Entity structure
- Verify stat ID is correct

#### ❌ **Issue**: Behavior subtags not working
**Symptoms**: Entity doesn't exhibit expected behavior
**Solutions**:
- Check EntityTypeData configuration is correct
- Verify subtags are spelled correctly
- Ensure OnInitialize is called (happens in Start())
- Check that behavior tree is reading blackboard variables

#### ❌ **Issue**: Resource tags not applying
**Symptoms**: AI doesn't interact with entity as expected
**Solutions**:
- Verify resource tags in EntityTypeData
- Check AddResourceTag() calls in initialization
- Use HasResourceTag() to verify tags are set
- Check resource tag spelling and case sensitivity

#### ❌ **Issue**: NavMeshAgent errors
**Symptoms**: Movement doesn't work, console errors
**Solutions**:
- Ensure NavMesh is baked in scene
- Add NavMeshAgent component to prefab
- Set proper NavMeshAgent settings (speed, radius)
- Check that destination is on NavMesh

---

## 🎯 Best Practices

### Performance Optimization

1. **Prefab Reuse**: Create prefab variants instead of unique prefabs for similar entities
2. **Blackboard Efficiency**: Only add necessary blackboard variables
3. **Event Cleanup**: Unsubscribe from events in OnDestroy
4. **Batch Operations**: Use entity pools for frequently spawned entities

### Code Organization

1. **Naming Convention**: Use descriptive names for EntityTypeData assets
2. **Folder Structure**: Organize by entity category (Animals, NPCs, Monsters)
3. **Version Control**: Keep EntityTypeData assets in version control
4. **Documentation**: Comment custom blackboard variables

### Design Guidelines

1. **Start Simple**: Begin with basic configuration, add complexity gradually
2. **Test Incrementally**: Test each behavior flag individually
3. **Use Inheritance**: Create base configurations for similar entity types
4. **Balance Values**: Playtest decay rates and thresholds extensively

---

## 📚 Additional Resources

### Related Documentation
- `ENTITY_README.md` - System overview and architecture
- `MIGRATION_GUIDE.md` - Migrating from old entity system
- `QUICK_REFERENCE.md` - API reference and examples

### Code Examples
- `Assets/Scripts/Examples/EntitySystemDemo.cs` - Basic usage examples
- `Assets/Scripts/Tests/EntitySystemTest.cs` - Unit tests and validation

### Unity Integration
- **NavMesh Documentation**: For AI movement setup
- **Behavior Trees**: For advanced AI behavior implementation
- **ScriptableObjects**: For creating custom EntityTypeData variants

---

## 🎉 Conclusion

The Universal Entity System provides a powerful, flexible foundation for all entities in your game. By following this implementation guide, you can:

- ✅ Create any entity type without writing code
- ✅ Maintain consistent behavior across all entities
- ✅ Easily balance and tune entity characteristics
- ✅ Integrate seamlessly with AI and behavior systems
- ✅ Scale your game with minimal technical debt

**Happy coding!** 🚀

---

*Last updated: June 4, 2025*
*System Version: 2.0*
*Author: Universal Entity System Team*

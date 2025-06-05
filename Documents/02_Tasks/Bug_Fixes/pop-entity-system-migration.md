# Pop Entity System Migration Task

## 📋 Task Overview

**Priority**: High  
**Type**: Architecture Migration
**Status**: Not Started  

### 🎯 Objective
Migrate the Pop entity system from the deprecated 3-script architecture (Pop.cs + EntityDataComponent.cs + configuration) to the new unified 2-script Entity system (Entity.cs + EntityTypeData.cs), ensuring all functionality is preserved while eliminating compilation errors.

## 🚨 Current Issues

### Compilation Errors
```
Assets\Scripts\Managers\SettlementManager.cs(172,52): error CS0246: The type or namespace name 'EntityDataComponent' could not be found
Assets\Scripts\Entities\Pop\Pop.cs(5,34): error CS0234: The type or namespace name 'Components' does not exist in the namespace 'Lineage.Ancestral.Legacies'
```

### Deprecated Components
- `EntityDataComponent.cs` - **OBSOLETE** (stat management now in Entity.cs)
- Pop-specific methods in `Pop.cs` that duplicate Entity.cs functionality
- References to deprecated namespace `Lineage.Ancestral.Legacies.Components`

## 🏗️ Migration Architecture

### Before (Old System - 3 Scripts)
```
GameObject (Pop)
├── Pop.cs (entity-specific behavior)
├── EntityDataComponent.cs (stat management) ❌ DEPRECATED
└── Configuration files
```

### After (New System - 2 Scripts)
```
GameObject (Pop)
├── Entity.cs (universal behavior component)
├── EntityTypeData.asset (configuration ScriptableObject)
└── Pop.cs (Pop-specific behavior only) ✅ STREAMLINED
```

## 📁 Files Requiring Changes

### 🔴 Critical (Compilation Errors)
1. **`Assets/Scripts/Entities/Pop/Pop.cs`**
   - Remove `using Lineage.Ancestral.Legacies.Components;`
   - Replace `EntityDataComponent` references with `Entity` component
   - Update method calls to use Entity.cs API

2. **`Assets/Scripts/Managers/SettlementManager.cs`**
   - Replace all `EntityDataComponent` references (11 locations)
   - Update method calls to use Entity.cs API
   - Fix component retrieval patterns

### 🟡 Secondary (Cleanup)
3. **`Assets/Scripts/Components/EntityDataComponent.cs`**
   - **DELETE** after migration is complete and tested

4. **Pop Prefabs**
   - Remove `EntityDataComponent` components
   - Ensure `Entity` components are properly configured
   - Assign appropriate `EntityTypeData` assets

## 🔧 Implementation Plan

### Phase 1: Fix Compilation Errors

#### Step 1.1: Update Pop.cs
```csharp
// BEFORE
using Lineage.Ancestral.Legacies.Components;
private EntityDataComponent entityDataComponent;

// AFTER  
using Lineage.Ancestral.Legacies.Entities;
private Entity entity;
```

#### Step 1.2: Migrate Pop.cs Methods
Replace deprecated EntityDataComponent methods with Entity.cs equivalents:

| **Deprecated Method** | **New Entity.cs Method** |
|----------------------|--------------------------|
| `entityDataComponent.GetHunger()` | `entity.Hunger` (property) |
| `entityDataComponent.GetThirst()` | `entity.Thirst` (property) |
| `entityDataComponent.GetEnergy()` | `entity.Energy` (property) |
| `entityDataComponent.EatFood(amount)` | `entity.ModifyStat(Stat.ID.Hunger, amount)` |
| `entityDataComponent.DrinkWater(amount)` | `entity.ModifyStat(Stat.ID.Thirst, amount)` |
| `entityDataComponent.RestoreEnergy(amount)` | `entity.ModifyStat(Stat.ID.Energy, amount)` |
| `entityDataComponent.HasCriticalNeeds()` | Custom logic using Entity properties |

#### Step 1.3: Update SettlementManager.cs
Replace all EntityDataComponent references in SettlementManager methods:
- `UpdatePopNeeds()`
- `GetPopsByNeed()`
- `AssignPopToWork()`
- `CheckPopHealth()`
- Population management methods

### Phase 2: Implement Missing Functionality

#### Step 2.1: Create HasCriticalNeeds() Logic
```csharp
// New implementation in Pop.cs
public bool HasCriticalNeeds()
{
    return entity.Hunger < 20f || 
           entity.Thirst < 20f || 
           entity.Energy < 20f || 
           entity.Health < 30f;
}
```

#### Step 2.2: Add Pop-Specific Behavior Methods
```csharp
public void EatFood(float amount)
{
    entity.ModifyStat(Stat.ID.Hunger, amount);
    Debug.Log($"{entity.EntityName} ate food, hunger restored by {amount}");
}

public void DrinkWater(float amount) 
{
    entity.ModifyStat(Stat.ID.Thirst, amount);
    Debug.Log($"{entity.EntityName} drank water, thirst restored by {amount}");
}

public void RestoreEnergy(float amount)
{
    entity.ModifyStat(Stat.ID.Energy, amount);
    Debug.Log($"{entity.EntityName} rested, energy restored by {amount}");
}

public void Sleep(float amount)
{
    entity.ModifyStat(Stat.ID.Rest, amount);
    Debug.Log($"{entity.EntityName} slept, rest restored by {amount}");
}
```

### Phase 3: Prefab Migration

#### Step 3.1: Update Pop Prefabs
1. **Remove EntityDataComponent**: Delete from all Pop prefabs
2. **Add Entity Component**: Ensure Entity.cs is attached
3. **Configure EntityTypeData**: Assign PopBehavior.asset or create new one
4. **Set EntityID**: Configure appropriate database ID (0 for new entities)

#### Step 3.2: Create PopBehavior EntityTypeData Asset
```yaml
Entity Type: Pop
Entity Type Name: "Pop"

Behavior Configuration:
  Has Needs Decay: true
  Has Aging: true  
  Can Craft: true
  Can Socialize: true
  Can Gather: true
  Can Reproduce: true

Behavior Subtags:
  - "social"
  - "crafting"
  - "gathering"

Resource Tags:
  - "Food"
  - "Water" 
  - "Gatherable"
  - "Crafting"

Needs Decay Rates:
  Hunger Decay Rate: 1.0
  Thirst Decay Rate: 1.2
  Energy Decay Rate: 0.8
  Rest Decay Rate: 0.6
```

### Phase 4: Testing & Cleanup

#### Step 4.1: Validation Tests
- [ ] Pop prefabs instantiate without errors
- [ ] Pop stats update correctly
- [ ] Needs decay system works
- [ ] Settlement manager population tracking works
- [ ] All Pop-specific behaviors function
- [ ] No performance regressions

#### Step 4.2: Code Cleanup
- [ ] Delete `EntityDataComponent.cs`
- [ ] Remove unused using statements
- [ ] Update documentation
- [ ] Clean up any remaining deprecated references

## 🧪 Testing Checklist

### ✅ Functionality Tests
- [ ] **Pop Creation**: New Pops spawn correctly with Entity component
- [ ] **Stat Management**: Hunger, thirst, energy stats update properly
- [ ] **Needs Decay**: Automatic decay system functions
- [ ] **Settlement Integration**: SettlementManager tracks Pops correctly
- [ ] **Behavior Systems**: Pop-specific actions (eating, drinking, sleeping) work
- [ ] **Database Integration**: Pop data loads from database correctly
- [ ] **Event System**: Stat change events trigger properly

### ✅ Performance Tests  
- [ ] **Memory Usage**: No memory leaks from component changes
- [ ] **Frame Rate**: No performance degradation
- [ ] **Instantiation**: Pop prefab instantiation time acceptable

### ✅ Integration Tests
- [ ] **Save/Load**: Pop data persists correctly
- [ ] **UI Updates**: Pop stats display correctly in UI
- [ ] **AI Behavior**: Pop AI behaviors function with new system
- [ ] **Multiplayer**: No issues with networked Pop entities

## 📚 API Reference

### Entity.cs Properties (Read-Only)
```csharp
entity.Health       // Current health value
entity.Hunger       // Current hunger value  
entity.Thirst       // Current thirst value
entity.Energy       // Current energy value
entity.Stamina      // Current stamina value
entity.EntityName   // Entity name from database
entity.Age          // Current age
```

### Entity.cs Methods
```csharp
entity.ModifyStat(Stat.ID statID, float amount)           // Modify any stat
entity.GetStat(Stat.ID statID)                           // Get stat object
entity.ChangeState(State.ID newState)                    // Change entity state
entity.ConfigureAsEntity(int id, EntityTypeData data)    // Configure entity
```

## ⚠️ Known Issues & Considerations

### Potential Breaking Changes
1. **Method Signatures**: Some Pop-specific methods may have different signatures
2. **Event Timing**: Stat change events may fire at different times
3. **Performance**: Initial migration may require performance tuning

### Migration Risks
- **Data Loss**: Ensure all Pop stat data is preserved during migration
- **Behavior Changes**: Verify all Pop behaviors work identically
- **Prefab Issues**: Some prefabs may need manual reconfiguration

## 📋 Acceptance Criteria

### ✅ Primary Goals
- [ ] All compilation errors resolved
- [ ] Pop entities function identically to before migration
- [ ] No functionality regression
- [ ] EntityDataComponent.cs successfully removed
- [ ] All tests pass

### ✅ Secondary Goals  
- [ ] Code is cleaner and more maintainable
- [ ] Performance is equal or better
- [ ] Documentation is updated
- [ ] Migration guide created for future reference

## 🔗 Related Documentation

- `Assets/Scripts/Entities/Documentation/ENTITY_README.md` - New Entity system overview
- `Assets/Scripts/Entities/Documentation/MIGRATION_GUIDE.md` - General migration guide
- `Assets/Scripts/Entities/Documentation/QUICK_REFERENCE.md` - API reference
- `Documents/02_Tasks/Bug_Fixes/entitydatacomponent-database-integration-fix.md` - Related task

## 📝 Notes

- **Database Compatibility**: Ensure Pop entity IDs remain consistent
- **Backup Recommendation**: Create project backup before starting migration
- **Incremental Testing**: Test each phase before proceeding to next
- **Team Communication**: Notify team members of API changes

---

**Created**: June 5, 2025  
**Last Updated**: June 5, 2025  
**Assignee**: Development Team  
**Priority**: High - Blocking compilation

# Entity System Quick Reference

## Two-Script Pattern Overview

### 🎯 **Entity.cs** (Behavior Component)
- Add to every entity prefab
- Handles database integration
- Manages stats and needs decay
- Coordinates with other components
- **One script fits all entity types**

### ⚙️ **EntityTypeData.cs** (Configuration Asset)
- **Single script** for all entity types
- Define entityType via Inspector dropdown (Pop, Animal, Monster, NPC)
- Auto-configures capabilities based on entityType
- Reusable across multiple prefabs
- **Designer-configurable, no code needed**

## Quick Setup Checklist

### For New Entity Type:
- [ ] Create EntityTypeData asset in `Assets/Data/EntityTypes/`
- [ ] Configure EntityType enum and capability flags
- [ ] Create prefab with Entity component
- [ ] Assign EntityTypeData to Entity component
- [ ] Set EntityID (0 for new, specific ID for existing)
- [ ] Test in play mode

### For Existing Entity Migration:
- [ ] Replace entity-specific script (Pop.cs) with Entity.cs
- [ ] Create matching EntityTypeData asset
- [ ] Update prefab references
- [ ] Test database integration
- [ ] Validate stat management

## Common EntityTypeData Configurations

### Pop Entity
```csharp
entityType = EntityType.Pop
canCraft = true
canSocialize = true  
hasNeedsDecay = true
canReproduce = true
canAge = true
```

### Wolf Entity
```csharp
entityType = EntityType.Animal
canCraft = false
canSocialize = false
hasNeedsDecay = true
canReproduce = true
canAge = true
```

### NPC Entity
```csharp
entityType = EntityType.NPC
canCraft = false
canSocialize = true
hasNeedsDecay = false
canReproduce = false
canAge = false
```

## Key Benefits
- ✅ **One Entity.cs script** for all entity types
- ✅ **Reusable configurations** via ScriptableObjects
- ✅ **Database-driven** entity data
- ✅ **Designer-friendly** workflow
- ✅ **Modular and extensible**

## File Locations
- **Scripts**: `Assets/Scripts/Entities/`
- **Data Assets**: `Assets/Data/EntityTypes/`
- **Prefabs**: `Assets/Prefabs/`
- **Tests**: `Assets/Scripts/Tests/`

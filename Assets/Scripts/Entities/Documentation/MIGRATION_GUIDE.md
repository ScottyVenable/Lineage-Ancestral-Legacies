# Entity System Migration Guide

## Overview
This guide helps you migrate from the old entity-specific script system to the new unified Entity system.

## Old vs New Architecture

### Before (Old System)
```
GameObject (Pop)
├── Pop.cs (entity-specific logic)
├── EntityDataComponent.cs (stat management)
└── Other components...
```

### After (New System)
```
GameObject (Pop)  
├── Entity.cs (unified logic)
├── EntityTypeData.asset (configuration)
└── Other components...
```

## Step-by-Step Migration

### 1. Backup Your Project
```bash
# Create a backup before starting migration
git commit -am "Pre-migration backup"
# or copy project folder
```

### 2. Create EntityTypeData Assets

#### For Pop Entities:
1. Right-click in `Assets/Data/EntityTypes/`
2. Create > Lineage > Entity Type Data
3. Name: "PopBehavior"
4. Configure:
   ```
   Entity Type: Pop (from dropdown)
   ```
   EntityTypeData will auto-configure Pop capabilities

#### For Animal Entities:
1. Create "WolfBehavior", "BearBehavior", etc.
2. Configure:
   ```
   Entity Type: Animal (from dropdown)
   ```
   Configure animal-specific settings as needed

### 3. Update Prefabs

#### For Each Entity Prefab:
1. **Remove old components:**
   - Remove `Pop.cs` (or other entity-specific scripts)
   - Remove `EntityDataComponent.cs`

2. **Add new components:**
   - Add `Entity.cs` component
   - Assign appropriate `EntityTypeData` asset

3. **Configure Entity ID:**
   - Set to `0` for new entities (system assigns ID)
   - Set to specific ID for existing database entities

### 4. Update Code References

#### Find and Replace:
```csharp
// Old references to Pop.cs
Pop popComponent = GetComponent<Pop>();

// New references to Entity.cs
Entities.Entity entity = GetComponent<Entities.Entity>();
```

#### Stat Access:
```csharp
// Old way (EntityDataComponent)
EntityDataComponent data = GetComponent<EntityDataComponent>();
int health = data.GetStat("health");

// New way (Entity)
Entities.Entity entity = GetComponent<Entities.Entity>();
int health = entity.GetStat("health");
```

### 5. Database Integration

#### Entity ID Management:
```csharp
// Old system - manual ID assignment
Pop pop = Instantiate(popPrefab);
pop.EntityID = Database.CreateNewEntity();

// New system - automatic if EntityID = 0
Entities.Entity entity = Instantiate(entityPrefab).GetComponent<Entities.Entity>();
// Entity automatically gets new ID from database if EntityID = 0
```

### 6. Testing Migration

#### Use Migration Tools:
```csharp
// Run the migration test tool
EntityMigrationToolFixed.TestMigration();

// Run system validation
EntitySystemTestFixed.RunAllTests();
```

#### Manual Testing:
1. Load scene with migrated entities
2. Verify entities load correctly
3. Test stat management
4. Test needs decay system
5. Verify database integration

## Common Migration Issues

### Issue: Missing EntityTypeData Reference
**Symptom:** Entity component shows "Entity Type Data" field empty
**Solution:** Assign appropriate EntityTypeData asset to the field

### Issue: EntityID Not Set
**Symptom:** Entity doesn't load data from database  
**Solution:** Set EntityID to 0 for new entities, or specific ID for existing ones

### Issue: Namespace Conflicts
**Symptom:** Compiler errors about ambiguous Entity references
**Solution:** Use fully qualified names:
```csharp
Entities.Entity entity;          // Your entity component
Database.Entity databaseEntity;  // Database record
```

### Issue: Missing Stat Data
**Symptom:** GetStat() returns 0 or default values
**Solution:** Ensure EntityID is valid and database contains entity record

## Verification Checklist

### ✅ Migration Complete When:
- [ ] All entity prefabs use Entity.cs component
- [ ] All EntityTypeData assets created and configured
- [ ] All code references updated to use Entity.cs
- [ ] EntityDataComponent.cs removed from prefabs
- [ ] Entity-specific scripts (Pop.cs) removed from prefabs
- [ ] Database integration working (entities load data)
- [ ] Stat management working (GetStat/ModifyStat)
- [ ] Needs decay system working (if enabled)
- [ ] All tests pass

### ✅ Performance Verification:
- [ ] Entity instantiation speed acceptable
- [ ] Memory usage not increased
- [ ] No factory overhead
- [ ] Database queries efficient

## Rollback Plan

If migration fails:
1. Revert to backup/git commit
2. Analyze specific issues
3. Fix problems incrementally
4. Re-run migration tools
5. Test thoroughly before proceeding

## Benefits After Migration

### ✅ Reduced Complexity:
- Fewer scripts per entity type
- Unified behavior system
- Centralized configuration

### ✅ Improved Workflow:
- Designers can create new entity types without code
- Artists work with familiar prefab system
- Programmers maintain single Entity.cs script

### ✅ Better Performance:
- No factory pattern overhead
- Shared ScriptableObject configurations
- Direct prefab instantiation

---

## Need Help?

- Check `EntitySystemTestFixed.cs` for validation
- Use `EntityMigrationToolFixed.cs` for automated migration
- Refer to `README.md` for architecture details
- Review `EXAMPLES.md` for configuration examples

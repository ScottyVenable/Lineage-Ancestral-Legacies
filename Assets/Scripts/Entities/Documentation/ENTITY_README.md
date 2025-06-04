# Entity System Architecture Documentation

## Overview

The Entity System uses a **two-script architecture pattern** that separates behavioral logic from configuration data. This pattern provides a clean, modular approach for creating database-driven game objects while maintaining Unity's prefab workflow.

## Architecture Pattern

### Two-Script System Components

1. **Entity.cs** - The behavioral component that handles:
   - Database integration and entity ID management
   - Stat management (health, hunger, happiness, etc.)
   - Needs decay system
   - Event coordination
   - Unity component interaction

2. **EntityTypeData.cs** - The **single, generic** configuration ScriptableObject that defines:
   - Entity type classification via `EntityType` enum (Pop, Animal, Monster, NPC)
   - Capability flags (canCraft, canSocialize, hasNeedsDecay, etc.)
   - Type-specific behavior toggles that auto-configure based on entityType
   - Reusable configuration profiles

## How It Works

### Database Integration
```csharp
// Entity automatically loads data from database using EntityID
public int EntityID { get; set; } // Links to Database.Entity record
```

### Configuration System
```csharp
// ONE EntityTypeData.cs handles ALL entity types through configuration
[CreateAssetMenu(fileName = "EntityTypeData", menuName = "Lineage/Entity Type Data")]
public class EntityTypeData : ScriptableObject
{
    public EntityType entityType;  // Pop, Animal, Monster, NPC
    public bool canCraft;
    public bool canSocialize;
    public bool hasNeedsDecay;
    // ... EntityTypeData morphs behavior based on entityType
}
```

### Prefab Setup
1. Create a GameObject prefab
2. Add the `Entity` component
3. Assign an `EntityTypeData` ScriptableObject
4. Configure the Entity ID (or leave 0 for new entities)

## Usage Examples

### Creating a Pop Entity
```csharp
// 1. Create EntityTypeData ScriptableObject asset
// 2. Set entityType = Pop in Inspector dropdown
// 3. EntityTypeData auto-configures Pop capabilities (canCraft = true, etc.)
// 4. Assign to Pop prefab's Entity component
// 5. Pop prefab is ready to use
```

### Creating an Animal Entity
```csharp
// 1. Create EntityTypeData ScriptableObject asset  
// 2. Set entityType = Animal in Inspector dropdown
// 3. Configure animal-specific settings (hasTerritory = true, etc.)
// 4. Assign to Wolf/Bear/etc. prefab's Entity component
// 5. Animal prefab is ready to use
```

## Benefits of This Pattern

### ✅ Advantages
- **Single EntityTypeData Script**: Only one EntityTypeData.cs for all entity types
- **Configuration-Driven Behavior**: EntityType enum determines behavior, not inheritance
- **Reusable Configurations**: Same EntityTypeData asset can be shared across multiple prefabs
- **Designer-Friendly**: Artists configure entity type via Inspector dropdown
- **Database-Driven**: All entity data comes from centralized database
- **Modular**: Easy to add new entity types by creating new ScriptableObjects
- **Minimal Files**: Only 2 scripts total (Entity.cs + EntityTypeData.cs)

### 🔑 Key Design Principle
**NO separate script files for each entity type!**
- ❌ Don't create: PopTypeData.cs, AnimalTypeData.cs, NPCTypeData.cs
- ✅ Do create: Multiple EntityTypeData.asset files with different configurations

**Configuration through ScriptableObject assets, not inheritance!**
- ❌ Class inheritance: PopTypeData : EntityTypeData
- ✅ Asset configuration: EntityTypeData.entityType = Pop

### 🔄 Workflow
1. **Designer**: Creates EntityTypeData assets for different entity types
2. **Artist**: Creates prefabs with Entity component + EntityTypeData reference
3. **Programmer**: Entity.cs handles all behavioral logic automatically
4. **Runtime**: Database provides entity-specific data, EntityTypeData provides type behavior

## File Structure

```
Assets/Scripts/Entities/
├── Entity.cs                    # Main behavioral component
├── EntityTypeData.cs           # SINGLE generic configuration script
└── Documentation/
    └── README.md               # This documentation

Assets/Data/EntityTypes/         # ScriptableObject assets (not scripts!)
├── PopBehavior.asset           # EntityTypeData with entityType = Pop
├── WolfBehavior.asset          # EntityTypeData with entityType = Animal
├── BearBehavior.asset          # EntityTypeData with entityType = Animal
├── NPCBehavior.asset           # EntityTypeData with entityType = NPC
└── MerchantBehavior.asset      # EntityTypeData with entityType = NPC

Assets/Prefabs/                 # Prefab assets
├── Pop.prefab                  # Uses PopBehavior.asset
├── Wolf.prefab                 # Uses WolfBehavior.asset
└── Bear.prefab                 # Uses BearBehavior.asset
```

## Implementation Notes

### Entity ID Management
- **New Entities**: Set EntityID = 0, system will assign new ID from database
- **Existing Entities**: Set EntityID to load specific database record
- **Validation**: Entity.cs validates database connection on Start()

### Stat System
- Stats are loaded from database based on EntityID
- EntityTypeData doesn't store stat values (database handles that)
- Entity.cs provides GetStat() and ModifyStat() methods
- Needs decay runs automatically if hasNeedsDecay = true

### Event System
- Entity.cs coordinates between components
- Events triggered on stat changes, needs updates, etc.
- Other components can subscribe to entity events

## Migration from Old System

### Before (3 scripts):
- Pop.cs (entity-specific script)
- EntityDataComponent.cs (stat management)
- PopTypeData.cs (configuration)

### After (2 scripts):
- Entity.cs (replaces Pop.cs + EntityDataComponent.cs)
- EntityTypeData.cs (unified configuration - ONE script for all types)

### Migration Steps:
1. Replace Pop.cs component with Entity.cs on prefabs
2. Create EntityTypeData asset for each entity type
3. Update prefab references to use new system
4. Test database integration and stat management

## Future Extensibility

### Adding New Entity Types:
1. Add new enum value to EntityType
2. Create new EntityTypeData asset
3. Configure capability flags
4. No code changes needed!

### Adding New Capabilities:
1. Add boolean flag to EntityTypeData.cs
2. Implement logic in Entity.cs
3. Update existing EntityTypeData assets
4. System automatically supports new capabilities

## Performance Considerations

- **Memory**: ScriptableObjects are shared, reducing memory usage
- **Instantiation**: Direct prefab instantiation, no factory overhead
- **Database**: Entity data loaded once on initialization
- **Updates**: Only entities with hasNeedsDecay update continuously

## Testing

Use `EntitySystemTestFixed.cs` and `EntityMigrationToolFixed.cs` for:
- Validating entity creation
- Testing database integration
- Migrating existing entities
- Performance benchmarking

---

This architecture provides a clean, maintainable foundation for all database-driven entities in the game while preserving Unity's prefab-based workflow.

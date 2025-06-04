# EntityTypeData Asset Examples

These are example configurations for different entity types. In Unity, these would be created as ScriptableObject assets via the menu: **Assets > Create > Lineage > Entity Type Data**

## PopTypeData.asset
```yaml
entityType: Pop
canCraft: true
canSocialize: true
hasNeedsDecay: true
canReproduce: true
canAge: true
canTrade: true
canLearn: true
canWork: true
hasInventory: true
canBuild: true
```

## WolfTypeData.asset
```yaml
entityType: Animal
canCraft: false
canSocialize: false
hasNeedsDecay: true
canReproduce: true
canAge: true
canTrade: false
canLearn: false
canWork: false
hasInventory: false
canBuild: false
```

## BearTypeData.asset
```yaml
entityType: Animal
canCraft: false
canSocialize: false
hasNeedsDecay: true
canReproduce: true
canAge: true
canTrade: false
canLearn: false
canWork: false
hasInventory: false
canBuild: false
```

## NPCTypeData.asset
```yaml
entityType: NPC
canCraft: false
canSocialize: true
hasNeedsDecay: false
canReproduce: false
canAge: false
canTrade: true
canLearn: false
canWork: false
hasInventory: true
canBuild: false
```

## MonsterTypeData.asset
```yaml
entityType: Monster
canCraft: false
canSocialize: false
hasNeedsDecay: true
canReproduce: false
canAge: false
canTrade: false
canLearn: false
canWork: false
hasInventory: false
canBuild: false
```

## MerchantTypeData.asset
```yaml
entityType: NPC
canCraft: false
canSocialize: true
hasNeedsDecay: false
canReproduce: false
canAge: false
canTrade: true
canLearn: false
canWork: false
hasInventory: true
canBuild: false
```

## GuardTypeData.asset
```yaml
entityType: NPC
canCraft: false
canSocialize: true
hasNeedsDecay: false
canReproduce: false
canAge: false
canTrade: false
canLearn: false
canWork: true
hasInventory: true
canBuild: false
```

---

## How to Create These in Unity:

1. **Right-click in Project window**
2. **Select**: Assets > Create > Lineage > Entity Type Data
3. **Name the asset** (e.g., "PopTypeData")
4. **Configure the settings** in the Inspector
5. **Save the asset** in `Assets/Data/EntityTypes/`

## Usage:
- Assign these assets to the `Entity Type Data` field on Entity components in prefabs
- Multiple prefabs can share the same EntityTypeData asset
- Modify the asset to change behavior for all entities using it

# GameData Hierarchy Overview

The Unified GameData System organizes all static definitions in a single struct accessible at `global.GameData`. Below is a condensed view of its planned structure.

```text
GameData
├─ Entity
│  ├─ Pop
│  │  ├─ GEN1
│  │  └─ ...
│  ├─ Animal
│  │  └─ WOLF
│  ├─ Structure
│  └─ ResourceNode
├─ Items
│  ├─ Resource
│  │  └─ FLINT
│  ├─ Tool
│  └─ ...
├─ Recipes
│  └─ STONE_AXE
├─ SpawnFormations
│  ├─ Type
│  │  ├─ SINGLE_POINT
│  │  ├─ CLUSTERED
│  │  └─ ...
│  └─ DefaultParams
├─ Traits
├─ Skills
├─ WorldConstants
├─ LootTables
└─ (additional categories as needed)
```

Each entry stores a full struct describing that entity, item, recipe or definition. Refer to the design doc for detailed examples of what data is included in each profile.

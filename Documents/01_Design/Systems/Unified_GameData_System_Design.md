# Unified GameData System Design for Lineage: Ancestral Legacies

**Version:** 1.0
**Date:** June 7, 2025
**Project:** Lineage: Ancestral Legacies

## 1. Introduction

### 1.1. Purpose
This document outlines the design for a unified, optimized, and robust GameData system for *Lineage: Ancestral Legacies*. It synthesizes concepts from previous design documents to create a comprehensive approach leveraging Unity's ScriptableObject architecture.

### 1.2. Goals
*   **Centralization & Clarity:** Provide a single source of truth for all static game data, making it easily understandable and accessible.
*   **Efficiency:** Utilize performant data structures and loading strategies.
*   **Maintainability:** Simplify data updates, debugging, and expansion.
*   **Scalability:** Design a system that can grow with the project and accommodate new data types and content, including modding.
*   **Robustness:** Minimize data-related bugs through strong typing, clear contracts, and editor integration.
*   **Designer-Friendly:** Enable easy data creation, modification, and management within the Unity Editor.

## 2. Core Philosophy
The system is built upon these core principles:
*   **Data-Driven Design:** Game logic will be driven by data defined in ScriptableObjects.
*   **ScriptableObjects as Primary Data Source:** All static game definitions (Entities, Items, Skills, Recipes, etc.) will be represented by ScriptableObjects.
*   **Centralized Access:** A `GameDataManager` will provide a global access point to all loaded game data.
*   **Tagging for Flexibility:** A comprehensive tagging system will allow for dynamic querying and categorization of data.

## 3. System Architecture

### 3.1. `GameDataManager`
A singleton class responsible for:
*   **Loading Data:** Loading all `GameDataSO` assets from specified locations (e.g., `Assets/GameData/` via `Resources.LoadAll` or Addressables for future scalability) at game initialization.
*   **Caching Data:** Storing loaded data in dictionaries for fast runtime access (e.g., by unique ID string).
*   **Providing Accessors:** Offering static methods to retrieve game data by ID, type, or tags.
    *   `GameDataManager.Instance.GetEntityDefinition(string entityID)`
    *   `GameDataManager.Instance.GetItemDefinition(string itemID)`
    *   `GameDataManager.Instance.GetAllDefinitionsOfType<T>() where T : GameDataSO`
    *   `GameDataManager.Instance.GetDefinitionsWithTag(Tag_SO tag)`
    *   `GameDataManager.Instance.GetDefinitionsWithTags(List<Tag_SO> tags, bool matchAll = true)`
*   **Namespace:** `Lineage.Core`
*   **Assembly:** `LineageScripts`

### 3.2. Base ScriptableObject: `GameDataSO`
All specific data definition ScriptableObjects will inherit from a base class.
```csharp
// Placed in Assets/Scripts/GameDatabase/Core/ or similar
namespace Lineage.Core
{
    public abstract class GameDataSO : ScriptableObject
    {
        [Tooltip("Unique identifier for this game data entry. E.g., ITEM_SWORD_IRON, ENTITY_POP_GEN1.")]
        public string uniqueID; // Must be unique across all SOs of the same broad type, or globally if necessary.

        [Tooltip("Display name for UI purposes.")]
        public string displayName;

        [TextArea(3, 5)]
        [Tooltip("In-game description or designer notes.")]
        public string description;

        [Tooltip("Tags associated with this data entry for categorization and filtering.")]
        public List<Tag_SO> tags = new List<Tag_SO>();
    }
}
```

### 3.3. Tag System
*   **`Tag_SO.cs`**: A simple ScriptableObject to define a reusable tag.
    ```csharp
    // Placed in Assets/GameData/_Core/Tags/ or Assets/Scripts/GameDatabase/Core/
    using UnityEngine;

    namespace Lineage.Core
    {
        [CreateAssetMenu(fileName = "NewTag", menuName = "GameData/Core/Tag Definition")]
        public class Tag_SO : ScriptableObject
        {
            [Tooltip("The actual tag value, e.g., 'Weapon', 'Edible', 'Hominid', 'Tier1'. Should be unique.")]
            public string tagName;
            // Optional: Add a description field for clarity in the editor.
        }
    }
    ```
*   Usage: `GameDataSO` and its derivatives will have a `List<Tag_SO>` field.

### 3.4. Specific ScriptableObject Definitions
Examples of derived ScriptableObjects (to be expanded based on GDD and `GameData.md`):

*   **`EntityDefinitionSO.cs`** (for Pops, Animals, Structures, Resource Nodes, Hazards)
    ```csharp
    // Namespace: Lineage.Core.Entities
    // Assembly: LineageScripts
    // Example Fields: baseStats (struct/SO), modelPrefab, icon, entityCategoryTag (e.g., Pop, Animal), skillList, traitList, etc.
    [CreateAssetMenu(fileName = "NewEntityDef", menuName = "GameData/Entities/Entity Definition")]
    public class EntityDefinitionSO : GameDataSO
    {
        // public PopStats_SO baseStats; // Reference another SO for complex stats
        public GameObject prefab;
        public Sprite icon;
        // Add fields for AI behavior, factions, abilities, etc.
        // public List<SkillDefinition_SO> startingSkills;
        // public List<TraitDefinition_SO> inherentTraits;
    }
    ```

*   **`ItemDefinitionSO.cs`**
    ```csharp
    // Namespace: Lineage.Core.Items
    // Assembly: LineageScripts
    // Example Fields: itemTypeTag (e.g., Weapon, Consumable, Resource), maxStackSize, equipableSlotTag, effects (list of Effect_SO), craftingRecipe (Recipe_SO).
    [CreateAssetMenu(fileName = "NewItemDef", menuName = "GameData/Items/Item Definition")]
    public class ItemDefinitionSO : GameDataSO
    {
        public int maxStackSize = 1;
        public Sprite itemIcon;
        // public EquipmentSlot_SO equipSlot; // If equippable
        // public List<EffectDefinition_SO> onUseEffects; // If consumable/usable
        // public float weight;
    }
    ```

*   **`RecipeDefinitionSO.cs`**
    ```csharp
    // Namespace: Lineage.Core.Crafting
    // Assembly: LineageScripts
    [CreateAssetMenu(fileName = "NewRecipeDef", menuName = "GameData/Recipes/Recipe Definition")]
    public class RecipeDefinitionSO : GameDataSO
    {
        public List<Ingredient> ingredients; // Ingredient class: ItemDefinitionSO item, int quantity
        public ItemDefinitionSO outputItem;
        public int outputQuantity = 1;
        public float craftingTimeSeconds;
        // public SkillDefinition_SO requiredSkill;
        // public int requiredSkillLevel;
        // public Tag_SO craftingStationTag; // Tag of the required crafting station
    }

    [System.Serializable]
    public class Ingredient
    {
        public ItemDefinitionSO itemDefinition;
        public int quantity;
    }
    ```

*   **Other SOs:** `SkillDefinitionSO`, `TraitDefinitionSO`, `BuffDebuffSO`, `QuestDefinitionSO`, `TileDefinitionSO`, `StructureDefinitionSO` etc., will follow similar patterns, inheriting from `GameDataSO`.

## 4. `Assets/GameData/` Folder Structure
This structure, adapted from `GameData.md` and `Modular_Data_Transition_Plan.md`, will house all ScriptableObject assets.
```
Assets/
└── GameData/
    ├── _Core/
    │   ├── Tags/                 # Tag_SO assets (e.g., Weapon.asset, Edible.asset)
    │   └── Enums/                # (If any enums need to be SOs for designers)
    ├── Entities/
    │   ├── Definitions/          # EntityDefinitionSO assets
    │   │   ├── Pops/
    │   │   ├── Animals/
    │   │   ├── Structures/
    │   │   └── ResourceNodes/
    │   └── EntityProperties/     # (e.g., PopStats_SO, BehaviorTree_SO - if used)
    ├── Items/
    │   ├── Definitions/          # ItemDefinitionSO assets
    │   │   ├── Weapons/
    │   │   ├── Armor/
    │   │   ├── Consumables/
    │   │   └── Resources/
    │   └── ItemProperties/       # (e.g., Effect_SO - if used as separate assets)
    ├── Abilities/                # SkillDefinitionSO, AbilityEffect_SO assets
    ├── StatusEffects/            # BuffDebuffSO assets
    ├── Quests/                   # QuestDefinitionSO assets
    ├── Recipes/                  # RecipeDefinitionSO assets
    ├── World/
    │   ├── Tiles/                # TileDefinitionSO assets
    │   └── Biomes/               # BiomeDefinition_SO assets
    └── LootTables/               # LootTable_SO assets
    └── NameLists/                # NameList_SO assets (for procedural generation)
```
*   **Naming Convention for Assets:** `[SpecificName]_[DataTypeSuffix].asset` (e.g., `IronSword_Item.asset`, `Gen1_Entity.asset`, `Foraging_Skill.asset`). Suffix helps clarify asset type in project view.

## 5. Data Loading and Initialization
1.  The `GameDataManager` will be a persistent singleton (e.g., initialized via a prefab in a startup scene or `[RuntimeInitializeOnLoadMethod]`).
2.  On awake/start, it will load all ScriptableObjects from designated subfolders within `Assets/Resources/GameData/` (or use Addressables in the future).
    *   Example: `Resources.LoadAll<ItemDefinitionSO>("GameData/Items/Definitions");`
3.  Loaded assets will be stored in dictionaries keyed by their `uniqueID` for O(1) access.
    *   `private Dictionary<string, ItemDefinitionSO> _itemDefinitions;`
    *   `private Dictionary<string, EntityDefinitionSO> _entityDefinitions;`
4.  Validation: During loading, check for duplicate `uniqueID`s and log errors.

## 6. Accessing Game Data in Systems
Game systems (Inventory, Crafting, AI, Spawning, UI) will access data through the `GameDataManager`.
```csharp
// Example: Spawning an entity
EntityDefinitionSO entityDef = GameDataManager.Instance.GetEntityDefinition("POP_GEN1");
if (entityDef != null && entityDef.prefab != null)
{
    GameObject newEntityInstance = Instantiate(entityDef.prefab);
    // Initialize instance with data from entityDef
    // newEntityInstance.GetComponent<Pop>().Initialize(entityDef);
}

// Example: Checking if an item is a weapon
ItemDefinitionSO itemDef = GameDataManager.Instance.GetItemDefinition("IRON_AXE");
Tag_SO weaponTag = GameDataManager.Instance.GetTagDefinition("Weapon"); // Assuming GetTagDefinition exists
if (itemDef != null && weaponTag != null && itemDef.tags.Contains(weaponTag))
{
    // It's a weapon
}
```

## 7. Editor Tool Integration
*   Existing editor tools (`DatabaseEditorWindow`, `ItemCreatorWindow`, etc.) will be refactored to:
    *   Create, read, and modify these ScriptableObject assets.
    *   Save/load assets to/from the defined `Assets/GameData/` structure.
    *   Utilize `AssetDatabase` for managing SO assets.
    *   Potentially include dropdowns populated from `GameDataManager` (e.g., for selecting `Tag_SO`s or linking `ItemDefinitionSO`s in recipes).

## 8. Modding Support (Future Consideration)
*   The system can be extended to load additional ScriptableObjects or JSON files from a `Mods` directory.
*   JSON data would need to be deserialized into corresponding C# classes/structs that mirror the SO structure, then potentially converted or wrapped into runtime data accessible via `GameDataManager`.
*   Tags will be crucial for mod compatibility and discovery.

## 9. Benefits of this System
*   **Type Safety:** C# and ScriptableObjects provide strong typing, reducing runtime errors.
*   **Performance:** ScriptableObjects are efficient as they are shared assets. Dictionary lookups are fast.
*   **Organization:** Clear folder structure and naming conventions improve project navigability.
*   **Reduced Coupling:** Game systems depend on the `GameDataManager` and data contracts (SO definitions), not concrete data implementations scattered across the codebase.
*   **Ease of Use:** Designers can manage game data directly in the Unity Inspector.
*   **Testability:** Data can be easily mocked or substituted for testing.

This unified GameData system provides a solid foundation for *Lineage: Ancestral Legacies*, promoting organized, efficient, and scalable data management.

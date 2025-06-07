# Game Data Storage Refactor Plan

**Document Version:** 1.0
**Date:** June 6, 2025

## 1. Introduction and Goals

This document outlines the plan for transitioning and centralizing all modular game data, with a primary focus on **Entities** and **Resources**, into a dedicated `Assets/GameData/` directory within the Unity project. The core goal is to leverage ScriptableObjects for managing this data, improving organization, workflow for designers, and overall project scalability.

This plan aims to:
- Establish a clear and consistent structure for storing game data assets.
- Transition existing and future game data (Entities, Resources, Items, etc.) to a ScriptableObject-based system.
- Facilitate easier data creation, modification, and balancing through Unity's native inspector and custom editor tools.
- Improve runtime data loading and management.

## 2. Scope of Data

The primary data categories for this refactor include, but are not limited to:

-   **Entities**:
    -   Player character definitions
    -   NPC archetypes and specific instances
    -   Creature/Monster types
    -   Interactable objects and props with game logic significance
-   **Resources**:
    -   Definitions of harvestable resource nodes (e.g., ore veins, herb patches, fishing spots).
    -   Types of raw materials/resources obtained from nodes or other sources (e.g., Iron Ore, Mystical Herb, Wood Log). This may overlap with "Items" but focuses on the raw collectible aspect.
-   **Other Key Data (to be included in the `Assets/GameData/` structure)**:
    -   Items (Weapons, Armor, Consumables, Quest Items, Crafting Materials)
    -   Skills and Abilities
    -   Quests and Objectives
    -   Lore Entries and Journal Systems
    -   Buffs, Debuffs, and Status Effects
    -   Traits and Genetic Profiles
    -   Dialogue Trees and NPC Interactions
    -   Global Game Settings and Balance Parameters

## 3. Proposed `Assets/GameData/` Folder Structure

A hierarchical folder structure will be adopted within `Assets/GameData/` to maintain clarity:

```
Assets/GameData/
├── Entities/
│   ├── Characters/         # Player, Humanoid NPCs
│   ├── Creatures/          # Monsters, Animals
│   └── Props/              # Interactable environment elements
├── Resources/
│   ├── Nodes/              # Definitions for harvestable nodes (e.g., MiningVein_Iron.asset)
│   └── Types/              # Definitions for resource types (e.g., Resource_IronOre.asset) - if distinct from Item definitions
├── Items/
│   ├── Weapons/
│   ├── Armor/
│   ├── Consumables/
│   ├── Materials/          # Crafting components, refined resources
│   └── QuestItems/
├── Skills/
│   ├── Combat/
│   └── Utility/
├── Quests/
│   ├── MainStory/
│   └── SideQuests/
├── Lore/
│   └── Entries/
├── Buffs/
├── Traits/
├── Dialogue/
└── GameSettings/           # Global balance, difficulty, etc.
```

## 4. Data Representation: ScriptableObjects

-   Each distinct piece of game data (e.g., a specific sword, an NPC type, an iron ore resource definition) will be an instance of a ScriptableObject (`.asset` file).
-   Base C# classes will be defined for each major data category, inheriting from `ScriptableObject`.
    -   Example: `public class EntityData : ScriptableObject { ... }`
    -   Example: `public class ResourceNodeData : ScriptableObject { ... }`
    -   Example: `public class ItemData : ScriptableObject { ... }`
-   These classes will contain serializable fields representing the data attributes (e.g., `entityName`, `health`, `resourceYield`, `itemIcon`, `damageValue`).
-   Unique IDs (e.g., `string` or `GUID`) should be implemented for each ScriptableObject to allow for robust referencing and lookup.

## 5. Transition and Implementation Strategy

**Phase 1: Core ScriptableObject Definition**
-   Define and implement the base C# classes for `EntityData`, `ResourceNodeData`, `ResourceTypeData` (if needed), `ItemData`, `SkillData`, etc.
-   Establish common interfaces or base classes for shared properties (e.g., `IGameDataObject` with `UniqueID`, `DisplayName`, `Description`).

**Phase 2: Data Migration / Creation**
-   **Identify Existing Data Sources**: Determine where current Entity, Resource, and other game data resides (e.g., hardcoded in scripts, enums, JSON/CSV files, existing placeholder ScriptableObjects).
-   **Develop Migration Tools (if necessary)**: Create editor scripts to automate the conversion of existing data into the new ScriptableObject asset format.
-   **Manual Data Entry/Refinement**: For new data or data not easily migrated, use custom or enhanced editor windows to create and populate the ScriptableObject assets within the `Assets/GameData/` structure.

**Phase 3: System Refactoring**
-   **Update `GameData.cs` and Repositories**: Modify the central data access points and repository classes to load and manage data from the new ScriptableObject assets. This will involve changing how data is fetched (e.g., from dictionaries populated by ScriptableObjects).
-   **Refactor Game Logic**: Update all game systems and components (e.g., `InventoryComponent`, `CombatSystem`, `QuestManager`, entity spawning logic, resource harvesting mechanics) to use the new ScriptableObject data structures.
-   **Implement Data Loading Strategy**:
    -   **Initial Load**: Consider loading essential data at game start.
    -   **Addressable Assets System**: For scalability and memory management, integrate the Addressables system to load/unload data assets as needed. This is highly recommended for "giant modular data."

**Phase 4: Editor Tool Adaptation**
-   Review and update all relevant editor tools in `Assets/Scripts/Editor/StudioTools/` (e.g., `EntityCreatorWindow`, `ItemCreatorWindow`, `DatabaseInspectorWindow`).
-   Tools should now allow browsing, creating, editing, and linking ScriptableObject assets stored in `Assets/GameData/`.
-   Enhance validation tools (`DataValidatorWindow`) to check integrity and references within the new ScriptableObject-based data.

## 6. Storing and Managing Data

-   **Version Control**: ScriptableObject `.asset` files are text-based (if Unity serialization is set to Force Text) and generally version control friendly. This allows for better tracking of changes and collaboration.
-   **Data Organization**: The defined folder structure within `Assets/GameData/` is key. Consistent naming conventions for assets should also be enforced (e.g., `EntityType_EntityName.asset`, `Item_ItemName.asset`).
-   **Backup Strategy**: Regular project backups are crucial, especially during major refactoring.

## 7. Focus on "Entities" and "Resources"

-   **Entities**:
    -   `EntityData` ScriptableObjects will define base stats, models, abilities, loot tables (references to `ItemData`), AI behavior profiles, etc.
    -   Prefabs for entities will have components that reference these `EntityData` assets to configure themselves.
-   **Resources**:
    -   `ResourceNodeData` ScriptableObjects will define the type of resource, quantity, respawn time, tools required for harvesting, and the visual representation (prefab) of the node.
    -   `ResourceTypeData` (or `ItemData` for the yielded resource) will define the actual resource item obtained (e.g., icon, stack size, value).
    -   Harvesting systems will interact with `ResourceNodeData` and grant `ResourceTypeData`/`ItemData`.

## 8. Potential Challenges and Mitigation

-   **Scope Creep**: Stick to the defined phases. Address one data system at a time if necessary.
-   **Data Integrity**: Implement robust ID systems and validation checks to prevent broken references between ScriptableObjects.
-   **Refactoring Effort**: This is a significant undertaking. Allocate sufficient time and test thoroughly at each stage.
-   **Learning Curve for Addressables**: If adopting Addressables, ensure the team has time to learn and implement it correctly.
-   **Merge Conflicts**: While better than code for data, merge conflicts on `.asset` files can still occur. Encourage frequent commits and communication.

## 9. Next Steps

1.  Prioritize the definition of core ScriptableObject C# classes for Entities and Resources.
2.  Develop a prototype migration script for one type of existing data (if applicable).
3.  Begin adapting one key game system (e.g., item spawning or entity instantiation) to use the new data format.
4.  Adapt a corresponding editor tool (e.g., `ItemCreatorWindow`) to work with the new ScriptableObjects.

# Modular Data Transition Plan

**Date:** June 6, 2025
**Project:** Lineage Ancestral Legacies

## 1. Introduction

### 1.1. Purpose
This document outlines the strategy and steps for transitioning and organizing all modular game data (Entities, Items, Resources, Skills, etc.) into a new, centralized `Assets/GameData/` folder within the Unity project.

### 1.2. Goals
*   **Improved Organization:** Centralize all game data assets for easier discovery and management.
*   **Scalability:** Create a data structure that can easily accommodate new data types and a growing volume of assets.
*   **Maintainability:** Simplify the process of updating, modifying, and debugging game data.
*   **Clear Separation:** Ensure a distinct separation between game data assets and game logic/code.
*   **Editor Tool Integration:** Facilitate seamless integration with existing and future editor tools for data management.

## 2. Current Data State Overview

*   The game currently utilizes a C# struct-based system for core data types (e.g., `Entity`, `Item`, `Skill`).
*   Data access is managed through a repository pattern (`ItemRepository`, `SkillRepository`, etc.) and a `GameData.cs` facade.
*   `MasterDatabaseInitializer.cs` is responsible for loading initial game data.
*   Existing data assets (ScriptableObjects, potentially other formats) might be located in various project folders or need formalization into ScriptableObjects.
*   Several editor tools (`DatabaseEditorWindow`, `NPCEditorWindow`, etc.) exist for managing game data and will require updates to align with the new structure.

## 3. Proposed `Assets/GameData/` Structure

The new `Assets/GameData/` folder will house ScriptableObjects, each representing a piece of game data or a collection.

```
Assets/
└── GameData/
    ├── Entities/
    │   ├── Races/               (e.g., HumanRace.asset, ElfRace.asset)
    │   ├── Classes/             (e.g., WarriorClass.asset, MageClass.asset)
    │   ├── Creatures/           (e.g., GoblinData.asset, WolfData.asset)
    │   ├── NPCs/                (e.g., MerchantNPCData.asset, QuestGiverNPCData.asset)
    │   └── PlayerTemplates/     (e.g., DefaultPlayerTemplate.asset)
    ├── Items/
    │   ├── Weapons/
    │   ├── Armor/
    │   ├── Consumables/
    │   ├── CraftingMaterials/
    │   └── QuestItems/
    ├── Resources/  (In-game gatherable/crafting resources)
    │   ├── Ores/
    │   ├── Woods/
    │   ├── Herbs/
    ├── Skills/
    │   ├── Combat/
    │   ├── Utility/
    │   ├── Crafting/
    │   └── Passive/
    ├── BuffsDebuffs/
    │   ├── Buffs/
    │   └── Debuffs/
    ├── Quests/
    │   ├── MainQuests/
    │   └── SideQuests/
    ├── Dialogue/
    │   └── NPCConversations/
    ├── WorldData/
    │   ├── Regions/
    │   ├── Locations/
    │   └── PointsOfInterest/
    ├── GameSettings/
    │   ├── DifficultyLevels/
    │   ├── BalanceValues/      (e.g., GlobalDamageModifiers.asset)
    │   └── ProgressionTuning/
    └── _MasterDatabases/        (Optional: ScriptableObjects that list/reference other data assets for easier loading)
        └── MasterEntityList.asset
        └── MasterItemList.asset
```

*   Each category will contain ScriptableObjects (e.g., `EntityDefinition.asset`, `ItemDefinition.asset`).
*   Naming convention for assets: `[SpecificName][DataType].asset` (e.g., `IronSwordWeapon.asset`, `HealthPotionConsumable.asset`).

## 4. Transition Steps

### Phase 1: Preparation & Setup (Estimated: 2-3 days)
1.  **Full Project Backup:** Create a complete backup of the Unity project.
2.  **Version Control Branch:** Create a new branch in Git for this transition (e.g., `feature/data-restructure`).
3.  **Create Folder Structure:** Manually create the `Assets/GameData/` directory and its primary subdirectories as outlined in Section 3.
4.  **Define/Refine Core ScriptableObject Types:**
    *   Ensure robust ScriptableObject classes exist for each major data category (e.g., `EntitySO`, `ItemSO`, `SkillSO`). These will hold the data previously defined in structs or will be containers for lists of such structs.
    *   Example:
        ```csharp
        // In LineageScripts assembly, e.g., Assets/Scripts/DataModels/ScriptableObjects/EntitySO.cs
        namespace Lineage.Core.Data
        {
            [CreateAssetMenu(fileName = "NewEntityData", menuName = "Lineage/Game Data/Entity")]
            public class EntitySO : ScriptableObject
            {
                public Entity Data; // Assuming Entity is your struct
                // Add any other metadata or editor-specific fields
            }
        }
        ```
5.  **Inventory Existing Data:**
    *   Identify all current locations and formats of game data (e.g., existing ScriptableObjects, prefabs with data components, JSON/CSV files).
    *   Create a checklist of all data to be migrated.

### Phase 2: Data Migration (Estimated: 5-10 days, depending on volume)
*(Iterate per data category)*

1.  **Entities:**
    *   For each existing entity, create a corresponding `EntitySO` (or similar) asset in the appropriate `Assets/GameData/Entities/` subfolder.
    *   Populate the ScriptableObject fields with data from the old source.
    *   Automate with editor scripts if feasible for large datasets.
2.  **Items:**
    *   Create `ItemSO` assets in `Assets/GameData/Items/`.
    *   Populate with data.
3.  **Resources (In-game):**
    *   Create `ResourceSO` assets in `Assets/GameData/Resources/`.
    *   Populate with data.
4.  **Skills, Buffs/Debuffs, Quests, Dialogue, etc.:**
    *   Repeat the process for each remaining data category, creating and populating their respective ScriptableObjects in the designated folders.
5.  **Update Data Loading Mechanisms:**
    *   Modify `MasterDatabaseInitializer.cs` and repository classes (`ItemRepository`, `EntityRepository`, etc.).
    *   Change logic to load ScriptableObjects from the new `Assets/GameData/` paths.
        *   Example: Use `AssetDatabase.FindAssets("t:ItemSO", new[] {"Assets/GameData/Items"})` to find all Item ScriptableObjects.
        *   Consider using a "Master List" ScriptableObject (e.g., `MasterItemListSO` in `Assets/GameData/_MasterDatabases/`) that holds references to all `ItemSO` assets. This can simplify loading and management.
    *   Ensure the `GameData.cs` facade correctly interfaces with the updated repositories.

### Phase 3: Editor Tool Adjustments (Estimated: 5-7 days)
1.  **Update Save/Load Paths:**
    *   Modify all relevant editor windows (`DatabaseEditorWindow`, `NPCEditorWindow`, `EntityCreatorWindow`, `SkillsBuffsEditorWindow_Fixed`, etc.) to use the new `Assets/GameData/` subfolders as default save locations for new ScriptableObjects.
    *   Update browsing/loading functionality to look for assets in these new locations.
2.  **Adapt to ScriptableObject Workflow:**
    *   Ensure tools can create, read, update, and delete the new ScriptableObject assets.
    *   If tools were previously working with in-memory structs directly for editing, they might need to now load an SO, modify its data, and then use `EditorUtility.SetDirty()` and `AssetDatabase.SaveAssets()`.
3.  **Test All Editor Tools:** Thoroughly test each editor window and its functionalities.

### Phase 4: System Integration & Testing (Estimated: 7-10 days)
1.  **Update Game Systems:**
    *   Refactor any game systems (Inventory, Combat, Quest System, etc.) that previously accessed data directly or through old paths to now use the updated `GameData` facade and repositories.
2.  **Scene/Prefab References:**
    *   Identify and update any direct references to old data assets in scenes or prefabs. This is a critical step to avoid broken links. Prioritize loading data via IDs through the `GameData` facade.
3.  **Comprehensive Gameplay Testing:**
    *   Playtest all major game features that rely on the migrated data.
    *   Focus on data loading, saving, entity spawning, item usage, skill execution, quest progression, etc.
4.  **Data Integrity Checks:**
    *   Implement simple editor tools or runtime checks to verify data consistency (e.g., all referenced IDs exist).
5.  **Performance Profiling:**
    *   Monitor data loading times and runtime performance. Optimize if necessary.

## 5. Potential Challenges & Mitigations

*   **Broken References:** Moving/changing asset types can break existing links in scenes, prefabs, or other ScriptableObjects.
    *   **Mitigation:** Prioritize data access via IDs through the `GameData` facade. Use Unity's "Find References in Project." Perform systematic checks.
*   **Large Volume of Data:** Manual migration can be extremely time-consuming and error-prone.
    *   **Mitigation:** Develop simple editor scripts to automate:
        *   Creating ScriptableObject assets from existing data sources (if structured).
        *   Moving and renaming assets based on rules.
*   **Merge Conflicts (Team Environment):**
    *   **Mitigation:** Communicate clearly with the team. Plan the transition during a period of focused effort on this task. Merge the `feature/data-restructure` branch frequently into the main development branch (or vice-versa) once stable milestones are reached.
*   **Updating Legacy Code:** Code tightly coupled to old data structures/paths will require careful refactoring.
    *   **Mitigation:** Tackle system by system. Use the `GameData` facade as an abstraction layer to minimize direct dependencies on the underlying data structure in game logic.
*   **Editor Tool Complexity:** Some editor tools might require significant rewrites.
    *   **Mitigation:** Prioritize core functionality. Consider a phased rollout of updated tool features.

## 6. Timeline (High-Level Estimate)

*   **Phase 1 (Preparation & Setup):** 2-3 Days
*   **Phase 2 (Data Migration):** 5-10 Days
*   **Phase 3 (Editor Tool Adjustments):** 5-7 Days
*   **Phase 4 (System Integration & Testing):** 7-10 Days
*   **Contingency:** 5 Days

**Total Estimated Time:** Approximately 4-6 weeks.

## 7. Conclusion

Transitioning to a structured `Assets/GameData/` folder will significantly enhance the project's organization, scalability, and maintainability. While a considerable undertaking, the long-term benefits for development efficiency and data integrity are substantial. The next step is to begin Phase 1: Preparation & Setup.

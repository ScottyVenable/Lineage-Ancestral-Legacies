# Database.cs Modularization Implementation Plan

This plan outlines the steps to refactor the monolithic `Database.cs` file into a more organized, modular structure. The goal is to improve readability, maintainability, and overall project organization without creating an excessive number of small files.

---

## Phase 1: Preparation & Strategic Decisions

**Goal:** Plan the new structure and ensure a safe working environment.

- [ ] **Backup Your Entire Unity Project!**  
    *Task:* Before making any large-scale refactoring changes, create a full backup of your project (e.g., zip it up, commit to version control on a new branch).  
    *Recommended Branch Name:* `feature/database-modularization`  
    *How to Create the Branch:*  
    If you are using Git, open a terminal in your project root and run:
    ```powershell
    git checkout -b feature/database-modularization
    git add .
    git commit -m "Initial commit for database modularization refactor"
    ```
    *Why:* Safety first! If anything goes wrong, you can revert.

- [ ] **Define Your Modularization Strategy for Data Structures**  
    *Task:* Decide how you will group your data definitions (classes, structs, enums). The recommended approach is by "domain" or "category."  
    *Example Groupings (to be refined by you):*
        - `EntityRelatedTypes.cs` (for `Entity` class, `Health` struct, related enums)
        - `ItemRelatedTypes.cs` (for `Item` class, related enums)
        - `SkillAndBuffTypes.cs` (for `Skill`, `Buff`, `Stat`, `StatModifiers` classes/structs, related enums)
        - `QuestAndObjectiveTypes.cs`
        - `LoreAndJournalTypes.cs`
        - `WorldDataTypes.cs` (for `Settlement`, `Population`, `Location`, `Chunk`)
        - `GeneticAndTraitTypes.cs`
        - `GlobalEnums.cs` (for enums used across multiple domains, if any)  
    *Why:* Establishes a clear organization before you start moving code.

- [ ] **Decide on Namespace Structure**  
    *Task:* Confirm your namespace strategy.  
    *Options:*
        1. Keep everything under `Lineage.Ancestral.Legacies.Database`.
        2. Use sub-namespaces for more clarity, e.g., `Lineage.Ancestral.Legacies.Database.Models` for the data structures, and `Lineage.Ancestral.Legacies.Database.Management` for the static `GameData` parts or repositories.  
    *Why:* Affects using statements throughout your project.

- [ ] **Plan the New Folder Structure**  
    *Task:* Create a new folder in your Unity project, e.g., `Assets/Scripts/GameDatabase/` (or similar).  
    *Sub-folders could be:*  
        - `DataModels/`
        - `Repositories/` (if you choose that for `GameData`)
        - `Enums/`  
    *Why:* Keeps the new files organized in the Project view.

- [ ] **Strategy for GameData Static Class (Database Lists & Initializers)**  
    *Task:* Decide how to break up the static `GameData` class which holds the `public static List<T> ...Database` fields and the `Initialize...Database()` methods.  
    *Options:*
        - **Option A (Partial Classes):** Keep `GameData` as the access point but split its definition across multiple files using `public static partial class GameData`. E.g., `GameData.Entities.cs`, `GameData.Items.cs`.
        - **Option B (Separate Static Repositories):** Create new static classes per domain, e.g., `EntityRepository.cs` (with `public static List<Entity> AllEntities;`), `ItemRepository.cs`, etc. Then have a `MasterDatabaseInitializer.cs` to call all their individual init methods.  
    *Why:* Determines how runtime code and potentially some tools will access these master lists. Option A means less change to existing access code. Option B is arguably cleaner separation.

---

## Phase 2: Refactoring the Code

**Goal:** Physically move the code into the new structure.

- [ ] **Create New C# Script Files**  
    *Task:* In your new `Assets/Scripts/GameDatabase/` folder (and subfolders), create the empty C# script files based on the domain groupings decided in Phase 1 (e.g., `EntityRelatedTypes.cs`, `ItemRelatedTypes.cs`).

- [ ] **Move Data Structure Definitions**  
    *Task:* Carefully cut and paste the class, struct, and enum definitions from the original `Database.cs` into their new respective files.  
    - Ensure each new file has the correct `using UnityEngine;`, `using System.Collections.Generic;`, etc., at the top.
    - Ensure all moved code is within the chosen namespace(s).
    - **Focus:** Get the definitions moved first.

- [ ] **Refactor the GameData Static Class**  
    *Task:* Implement your chosen strategy (Partial Classes or Separate Repositories).  
    - Move the static list declarations (`public static List<Entity> entityDatabase;`) and their corresponding `Initialize...Database()` methods into the new partial class files or new repository class files.
    - If using separate repositories, create and implement `MasterDatabaseInitializer.cs`.  
    *Why:* This separates the data storage/management logic.

- [ ] **Update InitializeAllDatabases()**  
    *Task:* Ensure the main `InitializeAllDatabases()` method (wherever it now resides – possibly in `GameData.Core.cs` if using partials, or in `MasterDatabaseInitializer.cs`) correctly calls all the individual `Initialize...Database()` or `Initialize...Repository()` methods.

- [ ] **Clean Up Original Database.cs**  
    *Task:* As you move sections of code out of the original `Database.cs`, you can delete them from that file. The goal is to eventually have a very slim `Database.cs` or even an empty one if all its contents are successfully migrated. (Do this cautiously, perhaps commenting out sections first).

---

## Phase 3: Updating Project-Wide Code References

**Goal:** Make sure all other scripts in your project can still find and use the database types and data.

- [ ] **Update using Directives**  
    *Task:* Go through all your runtime gameplay scripts and all your StudioTools editor scripts.  
    - If you changed or added sub-namespaces for your database code, update the using statements at the top of these files accordingly (e.g., `using Lineage.Ancestral.Legacies.Database.Models;`).  
    *Tip:* Unity's compiler errors will guide you here if types can't be found.

- [ ] **Update Data Access Logic (if GameData access changed)**  
    *Task:* If you switched from a monolithic `GameData.entityDatabase` to something like `EntityRepository.AllEntities` (Option B for GameData refactor), you'll need to find and replace these access points in your runtime code and potentially in some StudioTools (especially those that might display overall database stats or allow selection from these master lists).  
    *Note:* If your StudioTools primarily load/edit ScriptableObject assets, this step will have less impact on them. The `GenericEditorUIDrawer` doesn't care how the list that holds the object is structured, only about the object's type itself.

- [ ] **Compile & Fix Initial Errors**  
    *Task:* Let Unity compile. Address any compiler errors that arise from moved types, incorrect namespaces, or changed access patterns. This will be an iterative process.

---

## Phase 4: Thorough Testing & Validation

**Goal:** Confirm that the game and all tools work correctly with the new modular database structure.

- [ ] **Test Runtime Game Logic**  
    *Task:* Playtest your game. Focus on areas that:
        - Spawn entities
        - Use items, skills, buffs
        - Trigger quests or lore
        - Involve NPC interactions
        - Basically, anything that reads from your game database  
    *Why:* To ensure the game still functions as expected.

- [ ] **Test ALL StudioTools Extensively**  
    *Task:*  
        - Creator/Designer Tools: Can you still create new ScriptableObject assets for entities, items, etc.? Do they save correctly?
        - Do dropdowns and editors still populate as expected?
        - Are all references and lookups working?
        - Test any custom editors or database viewers.

---

**Tip:** Take your time with each phase. Modularization is a big step, but it will pay off in maintainability and clarity for your project!

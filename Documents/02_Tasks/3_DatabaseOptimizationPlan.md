# Database.cs Modularization Implementation Plan

This plan details how to refactor the monolithic `Database.cs` into a modular, maintainable structure for improved readability and project organization.

---

## Phase 1: Preparation & Strategic Decisions

### 1. Backup Your Project
- **[x]** Backup your entire Unity project (zip or commit to a new branch).
- **Why:** Ensures you can revert if needed.

### 2. Define Modularization Strategy for Data Structures
- **[x]** Decide how to group data definitions (classes, structs, enums), typically by domain/category.
- **Example Groupings:**
    - `EntityRelatedTypes.cs` (Entity, Health, related enums) ✓
    - `ItemRelatedTypes.cs` (Item, related enums) ✓
    - `SkillAndBuffTypes.cs` (Skill, Buff, Stat, StatModifiers, related enums) ✓
    - `QuestAndObjectiveTypes.cs` ✓
    - `LoreAndJournalTypes.cs` ✓
    - `WorldDataTypes.cs` (Settlement, Population, Location, Chunk) ✓
    - `GeneticTypes.cs` ✓
    - `StateTypes.cs` ✓
    - `GlobalEnums.cs` (enums used across domains) ✓
- **Why:** Establishes clear organization before moving code.

### 3. Decide on Namespace Structure
- **[x]** Choose a namespace strategy:
    - Option 1: `Lineage.Ancestral.Legacies.Database` ✓
    - Option 2: Sub-namespaces (e.g., `.Models`, `.Management`)
- **Why:** Impacts using statements project-wide.

### 4. Plan New Folder Structure
- **[x]** Create a new folder, e.g., `Assets/Scripts/GameDatabase/` with subfolders like `DataModels/`, `Repositories/`, `Enums/`. ✓
- **Why:** Keeps files organized in Unity's Project view.

### 5. Strategy for GameData Static Class
- **[x]** Decide how to split the static `GameData` class:
    - **Option A:** Partial classes (e.g., `GameData.Entities.cs`)
    - **Option B:** Separate static repositories (e.g., `EntityRepository.cs`, `ItemRepository.cs`) with a `MasterDatabaseInitializer.cs` ✓
- **Why:** Determines data access patterns and separation.

---

## Phase 2: Refactoring the Code

### 1. Create New Script Files
- **[x]** In `Assets/Scripts/GameDatabase/`, create empty C# files for each domain grouping. ✓

### 2. Move Data Structure Definitions
- **[x]** Move class, struct, and enum definitions from `Database.cs` to new files. ✓
- Ensure correct `using` directives and namespaces.

### 3. Refactor GameData Static Class
- **[x]** Implement chosen strategy (partial classes or repositories). ✓
- Move static list declarations and initializers to new files.
- If using repositories, implement `MasterDatabaseInitializer.cs`. ✓

### 4. Update InitializeAllDatabases()
- **[x]** Ensure the main initializer calls all individual database/repository initializers. ✓

### 5. Clean Up Original Database.cs
- **[x]** Remove migrated code from `Database.cs` (comment out first if needed). ✓

---

## Phase 3: Updating Project-Wide Code References

### 1. Update Using Directives
- **[~]** Update `using` statements in all scripts to match new namespaces. (In Progress - some compilation errors remain)

### 2. Update Data Access Logic
- **[~]** Update code accessing `GameData` if the access pattern changed (e.g., to `EntityRepository.AllEntities`). (In Progress - some compilation errors remain)

### 3. Compile & Fix Errors
- **[~]** Let Unity compile and fix any errors from moved types or changed access. (In Progress - missing using System.Collections.Generic and debug logging issues)

---

## Phase 4: Thorough Testing & Validation

### 1. Test Runtime Game Logic
- **[ ]** Playtest all features that use the database (spawning, items, skills, quests, NPCs).

### 2. Test StudioTools
- **[ ]** Verify all editor tools (creation, editing, analysis, validation, menu integration) work as expected.

### 3. Test Reflection-Based UI
- **[ ]** Ensure tools like `GenericEditorUIDrawer` still reflect on data classes correctly.

---

## Phase 5: Cleanup & Documentation

### 1. Final Removal of Old Database.cs Content
- **[x]** Delete or archive the now-empty `Database.cs`. ✓ (Archived as Database_Original_Backup.cs.backup)

### 2. Update Documentation
- **[ ]** Update design docs to reflect new file layout, namespaces, and data access.

### 3. Commit Changes
- **[ ]** Commit all changes with a clear message.

---

## Current Status & Remaining Issues

### ✅ **Completed**
- All major database refactoring has been completed
- New modular folder structure created: `Assets/Scripts/GameDatabase/`
- Data types separated into logical groups in `DataModels/` folder
- Repository pattern implemented in `Repositories/` folder
- `MasterDatabaseInitializer.cs` created to manage all repositories
- Original `Database.cs` archived as backup

### ⚠️ **Known Issues to Fix**
1. **Missing using directives** - Need to add `using System.Collections.Generic;` to files using `List<T>`
2. **Debug logging compilation errors** - `Lineage.Ancestral.Legacies.Debug.Log` method signatures need fixing
3. **Repository ambiguity issues** - Some properties are duplicated causing compiler ambiguity
4. **Project-wide using statement updates** - Other scripts may need namespace updates

### 📋 **Next Steps**
1. Fix compilation errors in `MasterDatabaseInitializer.cs` and repositories
2. Update all scripts throughout the project to use new repository access patterns
3. Test all editor tools and runtime functionality
4. Update documentation to reflect new structure

---

**Result:**  
Your new modular GameDatabase structure will be easier to maintain and scale as your project grows!

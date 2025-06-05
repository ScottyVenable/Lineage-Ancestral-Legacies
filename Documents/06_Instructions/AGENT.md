# Lineages: Ancestral Legacies - AI Contributor Guide (AGENT.MD)

Hey there, AI assistant! Welcome to the development of "Lineages: Ancestral Legacies," an ambitious evolution simulation game being built in Unity with C#. Your help is super valuable, and this guide is here to make our collaboration smooth and effective. Think of me as your lead dev, Scotty!

---

## 1. Project Overview: "Lineages: Ancestral Legacies"

* **Concept:** A rich, dynamic evolution simulation game inspired by the depth of titles like Dwarf Fortress. Every entity (creatures, NPCs, populations) has unique characteristics, histories, skills, appearances, inventories, and more. Their interactions and evolution generate emergent narratives and gameplay.
* **Engine:** Unity (latest stable version preferred).
* **Primary Language:** C#.

---

## 2. Core Technologies & Architectural Pillars

Understanding these is key to contributing effectively:

* **Modular Database (`Assets/Scripts/GameDatabase/`):**
    * Core game data structures (e.g., `Entity`, `Item`, `Skill`, `Buff`, `Quest`, `Trait`, `Settlement`, `Population`, `LoreEntry`, `Genetics`) are defined in C# classes, organized into domain-specific files (e.g., `EntityRelatedTypes.cs`, `ItemRelatedTypes.cs`).
    * **Namespace:** Primarily `Lineage.Ancestral.Legacies.Database` and sub-namespaces like `.Models`, `.Management`, etc.
    * **Assembly Structure:** Modular assemblies (see `.csproj` and copilot-instructions.md) such as `LineageScripts` (core), `LineageBehavior` (logic), and `LineageScripts.Editor` (editor tools).
* **ScriptableObjects for Data Persistence (`Assets/GameData/`):**
    * Most game data (item definitions, entity templates, quest definitions, etc.) is stored as `ScriptableObject` assets.
    * These assets are the **source of truth** for game content definition.
    * Tools should create, edit, and manage these `.asset` files.
    * **Modding Support:** JSON import/export and extensibility for designer/modder workflows (see `GameData.md`).
* **StudioTools System (`Assets/Scripts/Editor/StudioTools/`):**
    * Custom Unity Editor windows for creating, editing, visualizing, and managing all game data.
    * **Key Components:**
        * `BaseStudioEditorWindow`: Base class for tool windows.
        * `GenericEditorUIDrawer`: Uses Reflection to dynamically draw UI for ScriptableObject fields—**use this for all new tool UIs**.
        * `StudioMenuItems.cs`: Uses `[StudioToolMenuItem]` attributes for automatic menu integration.
        * **Graph-Based Editors:** Planned for AI, quests, dialogue (see design docs).
    * **Editor Assembly:** All editor scripts must be in `Assets/Scripts/Editor/` or subfolders, and in the `LineageScripts.Editor` assembly.
* **Git Version Control:**
    * Git is used for all source and asset management.
    * **Branching:** Feature branches for new work (`feature/xyz`), `develop` for integration, `main` for stable releases.
    * **.gitignore:** Standard Unity ignores plus custom rules for build, cache, and temp files.

---

## 3. Key Code Locations & Structure

* **Core Data Definitions:** `Assets/Scripts/GameDatabase/` (e.g., `EntityRelatedTypes.cs`, `ItemRelatedTypes.cs`, etc.)
* **ScriptableObject Assets:** `Assets/GameData/` (with subfolders like `Entities/`, `Items/`, `Quests/`, etc.)
* **StudioTools Editor Scripts:** `Assets/Scripts/Editor/StudioTools/`
* **Runtime Gameplay Logic:** `Assets/Scripts/Logic/`, `Assets/Scripts/Systems/`, `Assets/Scripts/Managers/`
* **UI Components:** `Assets/UI/`
* **Prefabs:** `Assets/Prefabs/` (organized by feature)
* **Data Assets:** `Assets/Data/` (for ScriptableObjects)
* **Unity Scenes:** `Assets/Scenes/`

---

## 4. Contribution & Style Guidelines

* **Namespaces:**  
    * Use `Lineage.Core`, `Lineage.Behavior`, `Lineage.Editor`, etc., following the modular assembly structure.
* **Comments & Documentation:**  
    * Use XML documentation (`/// <summary>...</summary>`) for all public classes, methods, and properties.
    * Inline comments for complex logic or non-obvious decisions.
* **ScriptableObject Focus:**  
    * All new game content types (items, entities, etc.) should use ScriptableObject assets for persistence.
    * StudioTools must create, load, and modify these assets.
* **StudioTools Development Principles:**  
    * Inherit from `BaseStudioEditorWindow` for new tools.
    * Use `GenericEditorUIDrawer` for UI—**avoid manual UI code unless absolutely necessary**.
    * Use `[StudioToolMenuItem("Path/In/Menu", priority)]` for menu integration.
    * Always call `Undo.RecordObject()` before modifying any `UnityEngine.Object` in editor tools.
* **Modularity:**  
    * Respect the modular design of assemblies and keep responsibilities focused.
* **Error Handling & User Feedback:**  
    * Use try-catch for risky operations (file I/O, parsing).
    * Provide user-friendly error/status messages in the tool UI (e.g., `EditorGUILayout.HelpBox`).
    * Use `Debug.LogWarning`/`Debug.LogError` for dev-facing issues, not user-facing.
* **Code Readability & Simplicity:**  
    * Prefer clarity over cleverness.
    * Use PascalCase for classes/methods/properties, camelCase for locals/private fields.
* **No Magic Numbers/Strings:**  
    * Use constants, enums, or config variables.

---

## 5. How to Validate Changes

* **Unity Compilation:**  
    * Code must compile without errors or warnings.
* **StudioTool Functionality:**  
    * Manually test new/modified StudioTools.
    * Ensure asset creation, loading, and editing works.
    * UI must be responsive and Undo/Redo must work.
    * Error messages must be clear.
* **Runtime Game Impact:**  
    * Data created/modified by StudioTools must be correctly used by runtime systems.
    * E.g., entity stat changes in assets must reflect in-game.
* **Testing:**  
    * (Planned) Use Unity Test Framework for automated tests.
    * See `.vscode/settings.json` for test generation instructions.

---

## 6. How You (AI Agent) Should Work & Present Work

* **Ask for Context:**  
    * If you need more info about a system, data structure, or task, ask for relevant docs (e.g., StudioTools plan, modularization, tool specs).
* **Iterative Approach:**  
    * For complex tasks (e.g., new StudioTool), provide a foundational structure and refine with feedback.
* **Code Presentation:**  
    * For new scripts or major changes, provide the complete C# file.
    * For small changes, clear code snippets are fine.
    * Always include necessary `using` statements and namespace declarations.
* **Explain Your Work:**  
    * Briefly explain logic behind significant contributions or architectural suggestions, especially if deviating from established patterns.
* **Documentation:**  
    * Suggest where to add XML comments or design doc notes for new features or complex logic.
* **Pull Request (PR) Messages:**  
    * **Title:** `[Domain/Tool] Brief Description of Change` (e.g., `[StudioTools/ItemCreator] Implement Save/Load for ItemSO`)
    * **Body:**  
        * Brief summary of changes.
        * Link to relevant task/issue.
        * How to test the changes.

---

## 7. Coding Conventions & Patterns

* **Unity Project Structure:**  
    * Core scripts in `Assets/Scripts/` (`LineageScripts` assembly)
    * Gameplay logic in `Assets/Logic/` (`LineageBehavior` assembly)
    * Editor tools in `Assets/Scripts/Editor/` (`LineageScripts.Editor` assembly)
    * UI in `Assets/UI/` (`Assembly-CSharp`)
    * Prefabs in `Assets/Prefabs/` (by feature)
    * Data assets in `Assets/Data/`
* **Naming Conventions:**  
    * Namespaces: `Lineage.Core`, `Lineage.Behavior`, `Lineage.Editor`
    * Classes: PascalCase (`PlayerController`)
    * Private fields: `_underscore`
    * `[SerializeField]` fields: no underscore
    * Methods: PascalCase
    * Events: PascalCase, descriptive (`OnPlayerHealthChanged`)
* **File Organization:**  
    * Group related scripts by feature.
    * Keep MonoBehaviours separate from data classes.
    * Interfaces in Core assembly.
    * Use partial classes for large components.

---

## 8. Useful Tools & Scripts

* **Editor Menu Tools:**  
    * See `Editor Menu Items/` for project cleanup, asset pipeline, workflow automation, and development utilities.
    * PowerShell scripts in `IDE Commands/` for code navigation, quick builds, and project status.
* **Code Navigation:**  
    * Use `code-nav.ps1` for searching classes, methods, usages, TODOs, etc.
    * Use `dev-quick.ps1` for common dev actions (build, clean, status, open docs).
* **Asset Management:**  
    * Use StudioTools for all ScriptableObject asset creation and editing.
    * Use project cleanup tools to remove empty folders, orphaned meta files, and clear caches.

---

## 9. Documentation & Further Reading

* **Design Docs:**  
    * See `Documents/01_Design/` for system overviews, GameData plans, and roadmaps.
    * `GameData.md` for ScriptableObject and JSON modding architecture.
    * `Unified GameData System.md` for hierarchical data design (GML reference, but principles apply).
* **System Status:**  
    * See `Assets/Logic/Behavior/Entity/Documentation/SYSTEM_STATUS.md` for AI/behavior system status.
    * See `Assets/Scripts/Entities/Documentation/ENTITY_README.md` for entity system architecture.

---

**If you need more context or a specific design doc, just ask!**

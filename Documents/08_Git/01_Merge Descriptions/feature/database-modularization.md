# Merge Description: Modularize `Database.cs` for Improved Organization

**Branch:** `feature/database-modularization`

## Purpose

This merge refactors the monolithic `Database.cs` into a modular, organized structure to improve readability, maintainability, and scalability as the "Lineages: Ancestral Legacies" project grows.

## Key Changes

- **Decomposition:** Split `Database.cs` into multiple domain-specific scripts (e.g., `EntityRelatedTypes.cs`, `ItemRelatedTypes.cs`, `SkillAndBuffTypes.cs`) under `Assets/Scripts/GameDatabase/`.
- **Logical Grouping:** Grouped data structures (classes, structs, enums) by functional domain for easier navigation.
- **GameData Refactor:** Refactored the static `GameData` class into partial classes or separate static repositories (e.g., `EntityRepository`, `ItemRepository`).
- **Namespace Consistency:** All database-related code now uses the `Lineage.Ancestral.Legacies.Database` namespace and sub-namespaces.
- **Updated References:** Adjusted using directives and data access points in runtime scripts and StudioTools to match the new structure.

## Impact & Benefits

- **Readability:** Domain-specific code is easier to locate and understand.
- **Maintainability:** Changes to data types require edits to smaller, focused files.
- **Organization:** Core game data structure is cleaner and more scalable.
- **Merge Conflict Reduction:** Smaller files reduce the likelihood of conflicts.
- **Foundation for Growth:** Modular structure supports future data types and systems.

## Testing & Validation

- Merged with latest main/develop branch and thoroughly tested.
- Verified runtime logic (entity spawning, item usage, quests, etc.) with the new structure.
- Confirmed StudioTools (Creators, Editors, Analyzers, Utilities) work with modular types and ScriptableObjects.
- Reflection-based UI components (e.g., `GenericEditorUIDrawer`) validated with new modular files.

## Notes

This refactor focuses on C# script and in-memory data organization. ScriptableObject asset workflows remain unchanged; all tools continue to operate as expected.
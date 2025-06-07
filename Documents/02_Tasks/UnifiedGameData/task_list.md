# Unified GameData System – Task List

A series of tasks to implement and maintain the GameData structure. Tasks are grouped by phase and can be checked off as work progresses.

## Phase 1 – Core Structure

- [ ] Create `scr_gamedata_init.gml` to build `global.GameData` hierarchy
- [ ] Define top level categories: `Entity`, `Items`, `Recipes`, `SpawnFormations`, `Traits`, `Skills`, `WorldConstants`, `LootTables`
- [ ] Load default JSON files for each category when user data is missing
- [ ] Implement `GetProfileFromUniqueID` helper for enum-style lookups

## Phase 2 – Entity & Item Profiles

- [ ] Expand `entity_data.json` and `item_data.json` to include base stats and display properties
- [ ] Implement spawn helpers using profile references instead of ID lookups
- [ ] Integrate needs configuration from entity profiles into runtime pop initialization

## Phase 3 – Gameplay Systems Integration

- [ ] Update crafting logic to pull recipes and item data from `global.GameData`
- [ ] Use GameData traits and skills when applying effects to pops
- [ ] Connect spawn formation parameters with world generation scripts
- [ ] Update UI scripts to display names and icons directly from data profiles

## Phase 4 – Maintenance & Expansion

- [ ] Document examples in `Utilizing the Unified GameData System.md`
- [ ] Add convenience scripts for debugging or inspecting GameData at runtime
- [ ] Regularly update default JSON definitions to match game design changes

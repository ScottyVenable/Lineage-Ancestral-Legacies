# Unified GameData System - Idea Document
**Project:** Lineage: Ancestral Legacies
**Author:** Gemini (for Scotty Venable)
**Version:** 1.1
**Date:** May 26, 2025

## 1. Vision & Goals

The core vision for the data management in *Lineage: Ancestral Legacies* is to establish a **single, hierarchical, globally accessible data structure, `global.GameData`**, that serves as the definitive and self-contained source for all static game definitions. This approach aims to:

* **Centralize All Static Data:** Consolidate definitions for entity profiles (Pops, Animals, Structures, Resource Nodes, Hazards), inventory items, crafting recipes, spawn formation rules, trait/skill details, and other static game parameters into one unified location.
* **Maximize Readability & Maintainability:** Implement intuitive, hierarchical paths (e.g., `GameData.Entity.Pop.GEN1`, `GameData.Items.Resource.FLINT`) for accessing data profiles. This will make the game's codebase cleaner, easier to understand, and simpler to modify.
* **Streamline Development Workflow:** Significantly reduce the complexity of data management by making `global.GameData` the comprehensive "database." The goal is to achieve a "self-sufficient code containing everything" for static definitions, minimizing the need for disparate data scripts or large, flat enums for primary identification.
* **Enable Robust Data-Driven Logic:** Allow core game systems (entity spawning, crafting, AI behavior, world generation, UI display) to be highly data-driven by directly pulling configurations, parameters, and definitions from `GameData`.
* **Provide Coder-Friendly Access:** Ensure that retrieving the complete static data profile for any game concept is direct, straightforward, and requires minimal intermediate lookup steps. Accessing a path in `GameData` should yield the full definition.

This system will integrate and potentially supersede previous data management methods (like a large, flat `EntityType` enum used as a key to a separate database) by making the `GameData` structure itself the primary data store and access method.

## 2. Core Concept: `global.GameData` - The Unified Database

`global.GameData` will be a master struct, meticulously organized with nested structs to create a logical and navigable hierarchy.

* **Initialization:** A dedicated script, `scr_gamedata_init.gml`, will run once at game start. This script will be responsible for defining the entire `global.GameData` structure and populating it with all static game definitions. This script will be the central point for defining game content.
* **Top-Level Categories:** The first level of `global.GameData` will delineate broad categories of game information:
    * `Entity`: Contains profiles for all spawnable/interactive world entities.
    * `Items`: Contains definitions for all inventory items.
    * `Recipes`: Contains definitions for all crafting recipes.
    * `SpawnFormations`: Defines types and parameters for entity spawning patterns.
    * `Traits`: Definitions for all pop/entity traits.
    * `Skills`: Definitions for all skills.
    * `WorldConstants`: Global game parameters, difficulty settings, etc.
    * `LootTables`: Definitions for loot drops.
    * *(Other categories as needed)*

## 3. Detailed Data Structure within `GameData`

The key principle is that accessing a specific path within `global.GameData` (e.g., `GameData.Entity.Pop.GEN1`) will directly provide the **complete static data profile struct** for that concept.

### 3.1. Entity Profiles (`GameData.Entity.<Category>.<SubCategory>.<ProfileName>`)

Each entity profile is a struct containing all static data required to define and spawn that entity type.

* **Example Path:** `global.GameData.Entity.Pop.GEN1`
* **Example Data Structure for `GameData.Entity.Pop.GEN1`:**
    ```gml
    // Located at global.GameData.Entity.Pop.GEN1
    GEN1: {
        // Identity & Display
        profile_id_string: "POP_PROFILE_GEN1_HOMO_HABILIS", // Unique string for internal reference, logging
        display_name_key: "pop_name_homo_habilis", // Key for localization, or direct display name
        description_key: "pop_desc_homo_habilis",

        // Core Spawning & Visual Data
        object_to_spawn: obj_pop, // The GameMaker object asset to instance
        sprite_info: { // Struct for sprite assets
            idle: spr_habilis_idle,
            walk_prefix: "spr_habilis_walk_", // e.g., spr_habilis_walk_N, spr_habilis_walk_S
            portrait: spr_habilis_portrait
        },
        base_scale: 1.0,
        tags: ["hominid", "sentient", "pop", "early_human", "tool_user", "scavenger"],

        // Stats & Attributes
        base_max_health: 80,
        base_max_stamina: 100,
        base_speed_walk: 1.8, // units per second
        base_perception_radius: 140, // pixels
        base_carrying_capacity: 7,
        diet_type_tags: ["omnivorous_scavenger", "eats_fruit", "eats_marrow", "plant_based_fallback"],
        base_resistances: { physical: 0, fire: -10, cold: 5 }, // Example resistances

        // Skills & Traits (references to profiles in GameData.Skills and GameData.Traits)
        base_skill_aptitudes: { // How quickly they learn certain skills
            [global.GameData.Skills.Type.FORAGING]: 1.2,
            [global.GameData.Skills.Type.CRAFTING_PRIMITIVE]: 1.1,
            [global.GameData.Skills.Type.SCAVENGING]: 1.3
        },
        innate_trait_profiles: [ // Array of paths to trait profiles
            global.GameData.Traits.Pop.BASIC_TOOL_USE,
            global.GameData.Traits.Generic.CURIOUS_MIND_LOW
        ],

        // AI & Behavior
        default_ai_behavior_package_id: "AI_HOMINID_EARLY_SCAVENGER", // ID for AI controller
        faction_default_id: "FACTION_PLAYER_TRIBE_EARLY",

        // World Generation & Spawning Defaults for this Profile
        default_spawn_amount_range: { min: 3, max: 5 },
        default_spawn_formation_type: global.GameData.SpawnFormations.Type.CLUSTERED, // Enum value
        default_formation_params: { // Parameters for its preferred CLUSTERED formation
            radius: 50,
            min_spacing_from_others: 24, // Minimum distance between spawned entities
            placement_attempts_per_entity: 10 // How many times to try placing before giving up (for dense areas)
        },

        // Loot & Resources
        loot_table_profile_path: global.GameData.LootTables.HOMINID_EARLY, // Path to a loot table definition

        // Other specific data...
        brain_size_cc_approx: 650
    }
    ```
* **Hierarchical Organization:** Entity profiles will be nested logically:
    * `GameData.Entity.Pop.<ProfileName>` (e.g., `GEN1`, `GEN2_ERECTUS`, `SAPIENS_MODERN`)
    * `GameData.Entity.Pop.Role.<RoleName>` (e.g., `HUNTER`, `CRAFTER` - these might inherit/modify a base Pop profile)
    * `GameData.Entity.Animal.<Category>.<ProfileName>` (e.g., `Predator.WOLF`, `Herbivore.DEER`, `Domesticated.DOG`)
    * `GameData.Entity.Structure.<Category>.<ProfileName>` (e.g., `Functional.TOOL_HUT`, `Storage.GRANARY`)
    * `GameData.Entity.ResourceNode.<ProfileName>` (e.g., `FLINT_DEPOSIT`, `BERRY_BUSH_RED`)
    * `GameData.Entity.Hazard.<Category>.<ProfileName>` (e.g., `AreaEffect.QUICKSAND`, `Event.ROCKSLIDE`)

### 3.2. Item Definitions (`GameData.Items.<Category>.<ItemName>`)

Each item profile contains all static data for an inventory item. Loaded from `item_data.json` and `default_item_data.json`.

* **Example Path:** `global.GameData.Items.Resource.FLINT`
* **Example Data Structure:**
    ```gml
    FLINT: {
        item_id_string: "ITEM_RESOURCE_FLINT", // Unique string ID
        display_name_key: "item_name_flint",
        description_key: "item_desc_flint",
        sprite_inventory: spr_item_flint_icon, // Icon for UI
        sprite_world: spr_item_flint_dropped, // Optional: if it can be dropped
        max_stack_size: 50,
        tags: ["resource", "stone", "crafting_material", "tool_component", "sharp"],
        value_barter_base: 2,
        weight_per_unit: 0.1,
        flammable: false,
        tool_properties: undefined, // If it were a tool, this would be a struct
        food_properties: undefined  // If it were food, this would be a struct
    }
    ```

### 3.3. Recipe Definitions (`GameData.Recipes.<CraftedItemProfileName>` or `<RecipeIDString>`)

Each recipe profile defines how to craft a specific item. Loaded from `recipes.json` and `default_recipes.json`.

* **Example Path:** `global.GameData.Recipes.STONE_AXE`
* **Example Data Structure:**
    ```gml
    STONE_AXE: {
        recipe_id_string: "RECIPE_TOOL_STONE_AXE",
        display_name_key: "recipe_name_stone_axe",
        produces_item_profile_path: global.GameData.Items.Tool.STONE_AXE, // Path to the item it creates
        produces_quantity: 1,
        ingredients: [ // Array of ingredient structs
            { item_profile_path: global.GameData.Items.Resource.FLINT, quantity: 2 },
            { item_profile_path: global.GameData.Items.Resource.STICK, quantity: 1 }
        ],
        base_crafting_time_seconds: 10,
        required_skill_profile: { // Skill needed
            skill_profile_path: global.GameData.Skills.CRAFTING_PRIMITIVE,
            min_level_required: 1
        },
        required_crafting_station_tags: ["workbench_basic", "tool_station"], // Structure must have one of these tags
        xp_reward_skill_path: global.GameData.Skills.CRAFTING_PRIMITIVE, // Which skill gets XP
        xp_reward_amount: 5,
        unlock_condition_id: "TECH_STONE_TOOL_BASICS" // Optional: ID of a tech needed
    }
    ```

### 3.4. Spawn Formation Definitions (`GameData.SpawnFormations`)

This section defines the *types* of formations and potentially default parameters.

* **`GameData.SpawnFormations.Type` (Enum-like Struct):**
    ```gml
    Type: { // Using a struct to emulate an enum for clarity
        SINGLE_POINT: 0,
        CLUSTERED: 1,
        LINE: 2,
        GRID: 3,
        PACK_SCATTER: 4 // For more organic animal groups
        // ... other formation types
    }
    ```
* **`GameData.SpawnFormations.DefaultParams.<FormationTypeName>` (Optional):**
    Stores default parameters for each formation type if not overridden by an entity's profile or a specific spawn call.
    ```gml
    // Example:
    // DefaultParams: {
    //     CLUSTERED: { default_radius: 60, default_min_spacing: 16, default_attempts: 5 },
    //     LINE: { default_spacing: 32, default_axis: "X" }
    // }
    ```
    The actual *logic* for placing entities (the GML code) will reside in separate functions, selected based on the `Type` enum.

### 3.5. Trait & Skill Definitions (Brief)

* `GameData.Traits.<Category>.<TraitName>`: Structs defining trait effects, incompatibilities, etc.
* `GameData.Skills.<SkillName>` or `GameData.Skills.Type.<EnumName>`: Structs defining skill descriptions, how they affect actions, XP progression curves.

### 3.6. Pop State Definitions (`GameData.PopStates.<StateName>`)

Defines the available behavioral states for pops. Loaded from `pop_states.json` and `default_pop_states.json`.

*   **Example Path:** `global.GameData.PopStates.Idle` (Note: The loader script currently places this at `global.GameData.pop_states.Idle` - this document reflects a potential refinement in path, adjust if loader is kept as is).
*   **Example Data Structure (as loaded):**
    ```gml
    // global.GameData.pop_states (actual current structure)
    // {
    //   "Idle": { "id": 0, "name": "Idle" },
    //   "Foraging": { "id": 1, "name": "Foraging" }
    // }

    // Idealized path in GameData for consistency:
    // Idle: { // Accessed via global.GameData.PopStates.Idle
    //     id: 0,
    //     name: "Idle",
    //     // Future: animation_prefix, sound_event, interrupt_priority
    // }
    ```

### 3.7. Needs System Integration (Conceptual)

The Needs System (`scr_needs_update.gml`) currently initializes needs directly on pop instances (e.g., `pop.needs = { hunger: 50, thirst: 50 }`)

*   **Data-Driven Enhancements:**
    *   **Base Need Parameters:** Could be loaded from `GameData.Entity.<Category>.<ProfileName>.base_needs` (e.g., `global.GameData.Entity.Pop.GEN1.base_needs = { hunger_max: 100, hunger_decay_rate: 0.01 }`).
    *   `scr_gamedata_init` or the entity loader would populate these into `global.GameData` from `entity_data.json`.
    *   Pop initialization would then use these `global.GameData` values to set up their instance-specific `needs` struct.
    *   `scr_needs_update` would reference these base parameters from the pop's `staticProfileData.base_needs` or a direct link for decay rates and thresholds.

## 4. Identifiers & Data Access

### 4.1. `GameData` Paths as Primary Identifiers

The primary method of identifying and accessing a static data profile is by its full path within `global.GameData`.
* **Example:** `var gen1_hominid_profile = global.GameData.Entity.Pop.GEN1;`
* This `gen1_hominid_profile` variable now directly holds the struct containing all static data for "GEN1" Hominids. No further "get data" function is needed for this initial retrieval of the profile.

### 4.2. Optional `UniqueID` Enum for Convenience (Your `EntityID` idea)

For scenarios where passing full struct references is less convenient (e.g., network messages, simple `switch` statements, save game references), a flat `UniqueID` enum can be implemented.

* **Definition:**
    ```gml
    enum UniqueID {
        // Entity Profiles
        PROFILE_ENTITY_POP_GEN1,
        PROFILE_ENTITY_ANIMAL_WOLF,
        // Item Profiles
        PROFILE_ITEM_FLINT,
        // Recipe Profiles
        PROFILE_RECIPE_STONE_AXE,
        // ... one unique ID for every distinct profile in GameData
    }
    ```
* **Helper Function: `GetProfileFromUniqueID(uid_enum)`**
    This function maps the simple `UniqueID` enum back to the actual data profile struct in `global.GameData`.
    ```gml
    function GetProfileFromUniqueID(uid_enum) {
        switch (uid_enum) {
            case UniqueID.PROFILE_ENTITY_POP_GEN1: return global.GameData.Entity.Pop.GEN1;
            case UniqueID.PROFILE_ITEM_FLINT:    return global.GameData.Items.Resource.FLINT;
            // ... all other mappings
            default:
                show_debug_message($"ERROR: Unknown UniqueID: {uid_enum}");
                return undefined;
        }
    }
    ```

### 4.3. Accessing Data from a Profile

Once you have the profile struct (either directly via path or via `GetProfileFromUniqueID`):
`var max_hp = gen1_hominid_profile.base_max_health;`
`var object_asset = gen1_hominid_profile.object_to_spawn;`

## 5. Key System Implementations

### 5.1. Initialization (`scr_gamedata_init.gml` & `scr_load_external_data_all.gml`)

*   `scr_gamedata_init.gml`: Sets up the `global.GameData` struct and calls `scr_load_external_data_all`.
*   `scr_load_external_data_all.gml`:
    *   Handles loading from JSON files (e.g., `item_data.json`, `resource_node_data.json`, `structure_data.json`, `entity_data.json`, `name_data.json`, `pop_states.json`, `recipes.json`).
    *   Implements fallback to `default_*.json` files if primary files are missing or corrupt.
    *   Populates the corresponding sections of `global.GameData` (e.g., `global.GameData.items`, `global.GameData.pop_states`, `global.GameData.recipes`).
    *   **Current Data Loading:** The system now robustly loads JSON definitions for items, resource nodes, structures, entities, pop names, pop states, and recipes into `global.GameData`.

### 5.2. Entity Spawning System

This system leverages `GameData` for all definitions.

* **5.2.1. Low-Level: `spawn_single_instance(entity_profile_struct, x, y, optional_initial_state_overrides = {})`**
    * **Input:** Takes the *full data profile struct* (e.g., `global.GameData.Entity.Pop.GEN1`).
    * **Action:**
        1.  `var new_inst = instance_create_layer(x, y, "Instances", entity_profile_struct.object_to_spawn);`
        2.  `new_inst.staticProfileData = struct_clone(entity_profile_struct);` (Crucial: instance gets a *copy* of the static profile).
        3.  `new_inst.uniqueProfileIDStringRef = entity_profile_struct.profile_id_string;` (For reference).
        4.  Initialize instance-specific runtime variables (e.g., `current_health = staticProfileData.base_max_health`).
        5.  Apply any `optional_initial_state_overrides` (e.g., starting with half health, specific faction).
        6.  If `new_inst` has an `initialize_from_profile()` method, call it. This method within the instance's object code can perform further setup using its `staticProfileData`.
    * **Output:** The ID of the newly created instance, or `noone`.

* **5.2.2. High-Level: `world_gen_spawn(subject_identifier, amount_override, formation_type_enum, area_params_struct)`**
    * **`subject_identifier`**: Can be a direct reference to a profile struct in `GameData` (e.g., `GameData.Entity.Pop.GEN1`) OR a `UniqueID` enum value (e.g., `UniqueID.PROFILE_ENTITY_POP_GEN1`).
    * **Action:**
        1.  **Resolve Profile:**
            ```gml
            var _profile_struct;
            if (is_struct(subject_identifier)) {
                _profile_struct = subject_identifier;
            } else if (is_real(subject_identifier)) { // Assumed UniqueID enum
                _profile_struct = GetProfileFromUniqueID(subject_identifier);
            }
            if (_profile_struct == undefined || !variable_struct_exists(_profile_struct, "object_to_spawn")) {
                show_debug_message("ERROR (world_gen_spawn): Invalid or unspawnable subject profile.");
                return []; // Return empty array on failure
            }
            ```
        2.  **Determine Amount:** Use `amount_override`. If not provided, use `_profile_struct.default_spawn_amount_range` (calculating a random value within the min/max). Fallback to 1 if no amount info is found.
        3.  **Validate Spawnability:** Ensure `_profile_struct.object_to_spawn` is a valid GameMaker object.
        4.  **Loop `_amount_to_spawn` times:**
            * **Get Formation Parameters:** Use `area_params_struct`. If not provided, use `_profile_struct.default_formation_params`. If still none, use sensible defaults (e.g., center of `area_params_struct` or 0,0).
            * **Calculate Position:** Call a specific placement function based on `formation_type_enum` (see 5.2.3).
                `var _pos = _calculate_spawn_position(formation_type_enum, _resolved_formation_params, current_loop_index, _amount_to_spawn);`
            * **Spawn:** `var _newly_spawned = spawn_single_instance(_profile_struct, _pos.x, _pos.y);`
            * Add `_newly_spawned` to a list of spawned instances.
    * **Output:** An array containing the instance IDs of all successfully spawned entities.

* **5.2.3. Spawn Formation Logic Functions (e.g., `_calculate_spawn_position(formation_enum, params, index, total)`)**
    * This function acts as a dispatcher or contains a `switch` on `formation_enum`.
    * Each `case` (e.g., `GameData.SpawnFormations.Type.CLUSTERED`) calls a dedicated GML function that implements the specific placement algorithm (e.g., `_place_entity_in_cluster(params, index, total)`).
    * These placement functions take parameters (like center point, radius, spacing, line endpoints) from the `params` struct and return an `{x, y}` position.
    * **Example Call within `world_gen_spawn`:**
        ```gml
        // Inside the loop of world_gen_spawn
        var _spawn_x, _spawn_y;
        var _current_formation_params = area_params_struct ?? _profile_struct.default_formation_params ?? { x:0, y:0, radius:10 }; // Example fallback

        switch (formation_type_enum) {
            case global.GameData.SpawnFormations.Type.CLUSTERED:
                var _pos_data = _calculate_clustered_spawn_pos(_current_formation_params, i, _amount_to_spawn);
                _spawn_x = _pos_data.x; _spawn_y = _pos_data.y;
                break;
            case global.GameData.SpawnFormations.Type.LINE:
                var _pos_data = _calculate_line_spawn_pos(_current_formation_params, i, _amount_to_spawn);
                _spawn_x = _pos_data.x; _spawn_y = _pos_data.y;
                break;
            // ... other cases
        }
        spawn_single_instance(_profile_struct, _spawn_x, _spawn_y);
        ```

### 5.3. Crafting System (Brief Example)

* A pop initiates crafting `GameData.Recipes.STONE_AXE`.
* The system checks `GameData.Recipes.STONE_AXE.ingredients`.
* For each ingredient, it accesses the `item_profile_path` (e.g., `GameData.Items.Resource.FLINT`) to get the item's definition, then checks the pop's inventory for `GameData.Items.Resource.FLINT` and the required quantity.
* Crafting time is modified by `GameData.Recipes.STONE_AXE.base_crafting_time_seconds` and the pop's skill level in `GameData.Skills.CRAFTING_PRIMITIVE` (referenced in the recipe).
* The resulting item is based on `GameData.Recipes.STONE_AXE.produces_item_profile_path`.

## 6. Benefits of the Unified `GameData` System

* **Single Source of Truth:** All static game data resides in one well-organized global structure.
* **Improved Readability:** Code becomes more expressive (e.g., `if (pop_data.faction_default_id == global.GameData.Factions.PLAYER_ALLY.id_string)`).
* **Reduced Complexity:** Eliminates the need to manage parallel enums and database lookup functions for basic data retrieval. The path to the data *is* the lookup.
* **Ease of Maintenance:** Changes to entity stats, item properties, or recipes are made in one central place (`scr_gamedata_init.gml`).
* **Enhanced Data-Driven Design:** Systems can easily query and use rich data profiles, allowing for complex emergent behaviors and easy content expansion.
* **Coder-Friendly:** Directly accessing `GameData.Entity.Pop.GEN1` to get its full profile is intuitive and reduces the "cognitive load" of remembering multiple data access patterns.

## 7. TLDR / Summary

The goal is to establish `global.GameData` as the comprehensive, hierarchical database for all static game definitions in *Lineage: Ancestral Legacies*. Accessing a path like `GameData.Entity.Pop.GEN1` will directly yield the complete data profile (stats, sprites, default behaviors, etc.) for that "GEN1" Hominid concept. This approach streamlines data access, enhances code readability, and makes the game highly data-driven. 

**Recent Additions Reflected:**
*   **Pop States:** Defined in `pop_states.json`, loaded into `global.GameData.pop_states`. Drives `obj_pop` state machine.
*   **Needs System:** Basic needs (hunger, thirst) managed by `scr_needs_update.gml`, affecting pop state. Future data-driving via `entity_data.json`.
*   **Crafting System:** Recipes defined in `recipes.json`, loaded into `global.GameData.recipes`. Logic in `scr_crafting_functions.gml` for checking and performing crafts.

This system fulfills the desire for a self-sufficient system where the data structure itself is the primary means of identifying and retrieving game content definitions. Functions like `spawn` will take these `GameData` profile paths (or a simplified `UniqueID` enum that maps to them) as input, directly using the rich profile data to orchestrate complex actions like patterned group spawning.

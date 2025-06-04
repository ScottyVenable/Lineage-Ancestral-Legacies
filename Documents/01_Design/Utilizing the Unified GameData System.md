# Utilizing the Unified `global.GameData` System
**Project:** Lineage: Ancestral Legacies
**Focus:** Practical application of the `global.GameData` structure in GameMaker Language.

## 1. Core Principle: Direct Access to Data Profiles

The fundamental shift with this system is that `global.GameData` **is your database**. When you need the static definitions for an entity, item, recipe, etc., you access its profile struct directly via its path in `global.GameData`.

* **Direct Path Access:**
    ```gml
    var gen1_pop_profile = global.GameData.Entity.Pop.GEN1;
    var flint_item_profile = global.GameData.Items.Resource.FLINT;
    var stone_axe_recipe_profile = global.GameData.Recipes[ID.RECIPE_STONE_AXE]; // Using ID enum as key for recipes
    ```
    The variables `gen1_pop_profile`, `flint_item_profile`, etc., now hold the *complete static data structs* as defined in `scr_database.gml`.

* **Access via `ID` Enum (using `GetProfileFromUniqueID`):**
    If you're passing around simpler identifiers, your `ID` enum and `GetProfileFromUniqueID` function come into play:
    ```gml
    var pop_profile_to_use = GetProfileFromUniqueID(ID.POP_GEN2);
    var item_data = GetProfileFromUniqueID(ID.ITEM_STICK);
    ```pop_profile_to_use` and `item_data` will again hold the full profile structs.

Once you have the profile struct, you can directly access its properties:
`var pop_max_health_base = gen1_pop_profile.StatsBase.Health.base_max;`
`var flint_stack_size = flint_item_profile.max_stack_size;`

## 2. System Implementation Examples

Let's see how `GameData` is utilized in key game systems.

### 2.1. Entity Spawning

Spawning entities is a multi-step process that heavily relies on `GameData`.

**A. Low-Level: `spawn_single_instance(entity_profile_struct, x, y, optional_overrides = {})`**
This function is responsible for creating one instance and initializing it.

* **Input:** The *full data profile struct* (e.g., `global.GameData.Entity.Pop.GEN1`).
* **Process:**
    1.  Retrieves `object_to_spawn` from the `entity_profile_struct`.
    2.  Creates the instance: `var new_inst = instance_create_layer(x, y, "Instances", entity_profile_struct.object_to_spawn);`
    3.  **Crucially, assigns a clone of the static profile to the instance:**
        `new_inst.staticProfileData = struct_clone(entity_profile_struct);`
        `new_inst.profileIDStringRef = entity_profile_struct.profile_id_string;`
    4.  The instance then typically calls its own `initialize_from_profile()` method.
* **`obj_pop`'s `initialize_from_profile()` Method (Example):**
    This method (defined within `obj_pop` or its parent) uses `self.staticProfileData` to set up runtime properties.
    ```gml
    // In obj_pop, within a method called initialize_from_profile()
    // This method is called by spawn_single_instance after staticProfileData is assigned.

    // 1. Generate a unique instance name (logic for this would be separate)
    self.instance_display_name = _generate_random_hominid_name_from_profile(self.staticProfileData);

    // 2. Roll Ability Scores for this specific instance
    self.abilityScores_runtime = {}; // Store this instance's rolled scores
    var _ranges = self.staticProfileData.AbilityScoreRanges;
    for (var _score_name in _ranges) {
        var _range_struct = _ranges[$ _score_name];
        self.abilityScores_runtime[$ _score_name] = irandom_range(_range_struct.min, _range_struct.max);
    }

    // 3. Calculate and Initialize Runtime Stats based on rolled abilities and base stats
    self.stats_runtime = {};
    var _base_stats = self.staticProfileData.StatsBase;
    var _rolled_scores = self.abilityScores_runtime;

    // Health (example: base + bonus from Constitution)
    var _con_bonus_health = (_rolled_scores.Constitution - 10) * 2; // Example: +2 HP per point over 10 Con
    self.stats_runtime.max_health = _base_stats.Health.base_max + _con_bonus_health;
    self.stats_runtime.current_health = self.stats_runtime.max_health;

    // Stamina (similar calculation)
    var _con_bonus_stamina = (_rolled_scores.Constitution - 10) * 3;
    self.stats_runtime.max_stamina = _base_stats.Stamina.base_max + _con_bonus_stamina;
    self.stats_runtime.current_stamina = self.stats_runtime.max_stamina;

    // Carrying Capacity
    var _cap_formula = _base_stats.carrying_capacity_formula;
    self.stats_runtime.max_carrying_capacity = (_cap_formula.base_value ?? 0) + (_rolled_scores.Strength * (_cap_formula.strength_multiplier ?? 1));
    self.stats_runtime.current_carrying_weight = 0;

    // Speed & Perception (can also be modified by rolled Dex/Wis or traits)
    self.stats_runtime.walk_speed = _base_stats.Speed.walk;
    self.stats_runtime.run_speed = _base_stats.Speed.run;
    self.stats_runtime.perception_radius = _base_stats.Perception.base_radius + ((_rolled_scores.Wisdom - 10) * 5); // Example

    // 4. Initialize Inventory
    self.inventory_data = inventory_create(); // Assuming inventory_create() function exists

    // 5. Initialize Skills (starting levels might be based on aptitudes or base values)
    self.skill_levels_runtime = {};
    var _aptitudes = self.staticProfileData.base_skill_aptitudes;
    for (var _skill_type_enum_str in _aptitudes) { // Keys are stringified enums
        // Example: Start with level based on aptitude, or just aptitude value
        self.skill_levels_runtime[$ _skill_type_enum_str] = _aptitudes[$ _skill_type_enum_str];
    }

    // 6. Apply Innate Traits
    self.active_traits_runtime = [];
    if (variable_struct_exists(self.staticProfileData, "innate_trait_profile_paths")) {
        for (var i = 0; i < array_length(self.staticProfileData.innate_trait_profile_paths); i++) {
            var _trait_profile_path_or_id = self.staticProfileData.innate_trait_profile_paths[i];
            // Assuming trait_profile_path_or_id is either a direct path to the trait struct
            // or a UniqueID enum that GetProfileFromUniqueID can resolve to the trait struct.
            var _trait_data = is_struct(_trait_profile_path_or_id) ? _trait_profile_path_or_id : GetProfileFromUniqueID(_trait_profile_path_or_id);
            if (_trait_data != undefined) {
                array_push(self.active_traits_runtime, _trait_data); // Store the full trait profile
                // Apply immediate effects of the trait if any
            }
        }
    }
    // 7. Set AI package
    self.ai_package_id = self.staticProfileData.default_ai_behavior_package_id;
    self.current_faction_id = self.staticProfileData.faction_default_id;

    show_debug_message($"Initialized Pop Instance: {self.instance_display_name} (Profile: {self.profileIDStringRef})");
    ```

**B. High-Level: `world_gen_spawn(subject_identifier, amount_override, formation_type_enum, area_params_struct)`**
This function orchestrates spawning multiple entities.

* **Input:**
    * `subject_identifier`: A direct path to a profile in `GameData` (e.g., `global.GameData.Entity.Pop.GEN1`) OR an `ID` enum value (e.g., `ID.POP_GEN1`).
    * `amount_override`: Specific number to spawn.
    * `formation_type_enum`: e.g., `global.GameData.SpawnFormations.Type.CLUSTERED`.
    * `area_params_struct`: e.g., `{ x: 100, y: 100, radius: 50 }`.
* **Process:**
    1.  **Resolve Profile:** Gets the full entity profile struct from `subject_identifier` (using `GetProfileFromUniqueID` if an `ID` enum is passed).
        ```gml
        var _profile_struct;
        if (is_struct(subject_identifier)) { _profile_struct = subject_identifier; }
        else { _profile_struct = GetProfileFromUniqueID(subject_identifier); }
        // Error check _profile_struct
        ```
    2.  **Determine Amount:** Uses `amount_override` or falls back to `_profile_struct.default_spawn_amount_range`.
    3.  **Loop & Place:** For each entity to spawn:
        * Calculates spawn position (X, Y) using a dedicated placement function (e.g., `_calculate_clustered_spawn_pos`) selected based on `formation_type_enum`. This function uses `area_params_struct` and/or `_profile_struct.default_formation_params`.
        * Calls `spawn_single_instance(_profile_struct, calculated_x, calculated_y);`
* **Example Call:**
    ```gml
    var gen1_profile_ref = global.GameData.Entity.Pop.GEN1;
    var formation = global.GameData.SpawnFormations.Type.CLUSTERED;
    var area = { x_center: 250, y_center: 400, radius: 60 };
    obj_world_gen_controller.world_gen_spawn(gen1_profile_ref, 5, formation, area);

    // Or using the ID enum:
    obj_world_gen_controller.world_gen_spawn(ID.POP_GEN1, 3, formation, area);
    ```

### 2.2. Crafting System

`GameData` provides recipe definitions and item properties.

* **Starting a Craft:**
    1.  Player selects a recipe, e.g., Stone Axe. The UI might get the recipe ID `ID.RECIPE_STONE_AXE`.
    2.  Fetch the recipe profile:
        `var recipe_profile = GetProfileFromUniqueID(ID.RECIPE_STONE_AXE);`
        Or directly if keyed by item: `var recipe_profile = global.GameData.Recipes[ID.RECIPE_STONE_AXE];`
    3.  Check crafter's inventory against `recipe_profile.ingredients`:
        ```gml
        var can_craft = true;
        for (var i = 0; i < array_length(recipe_profile.ingredients); i++) {
            var _ingredient_info = recipe_profile.ingredients[i];
            var _item_profile_for_ingredient = _ingredient_info.item_profile_path; // This IS the item's data struct
            // Or: var _item_profile_for_ingredient = GetProfileFromUniqueID(_ingredient_info.item_id_enum_ref);

            if (!inventory_has_resources(crafter_instance.inventory_data, _item_profile_for_ingredient.item_id_string, _ingredient_info.quantity)) {
                can_craft = false;
                break;
            }
        }
        ```
    4.  If `can_craft`, consume ingredients (using their `item_id_string` from their profiles) and calculate crafting time using `recipe_profile.base_crafting_time_seconds` and crafter's skill (whose profile is at `recipe_profile.required_skill_profile.skill_profile_path`).
    5.  On completion, the item produced is defined by `recipe_profile.produces_item_profile_path`.

### 2.3. AI Behavior

An AI controller for a Pop instance would use its assigned `staticProfileData`.

* **Decision Making:**
    ```gml
    // In obj_pop's Step or AI script
    // self.staticProfileData was assigned at spawn

    // What to eat?
    if (self.needs_food) {
        var diet_tags = self.staticProfileData.diet_type_tags;
        // AI searches for food sources matching these tags.
    }

    // What tools can I use/make?
    var tool_tags = self.staticProfileData.tool_use_level_tags;
    // AI might prioritize crafting items whose recipes require tools it can use.

    // How should I react to threats?
    var ai_package = self.staticProfileData.default_ai_behavior_package_id;
    // This ID would trigger specific behavior trees or state machines.
    ```

### 2.4. UI Display

Displaying information about entities or items.

* **Showing Item Info in Inventory UI:**
    1.  Inventory stores item `profile_id_string` or `ID` enum and quantity.
    2.  When displaying, fetch the item profile:
        `var item_profile = GetProfileFromUniqueID(inventory_slot.item_id_enum);`
        Or `var item_profile = global.GameData.Items.Resource[inventory_slot.item_profile_id_string];` (if using string IDs as keys)
    3.  Access display properties:
        `draw_text(x, y, item_profile.display_name_key);` (Assuming localization)
        `draw_sprite(item_profile.sprite_inventory, 0, x_icon, y_icon);`

* **Displaying Pop Stats:**
    * The pop instance has `instance_display_name`, `stats_runtime` (with current/max health, etc.), `abilityScores_runtime`, `skill_levels_runtime`. These are displayed directly.
    * To show what *type* of pop it is: `draw_text(x,y, pop_instance.staticProfileData.name_display_type);`

## 3. Summary of Data Flow

1.  **Define:** All static data (blueprints, templates, definitions) is meticulously defined in `scr_database.gml` within the `global.GameData` struct.
2.  **Access Profile:** Game systems get a *reference* to a specific data profile struct directly via its path in `GameData` (e.g., `global.GameData.Entity.Pop.GEN1`) or via the `ID` enum and `GetProfileFromUniqueID()`.
3.  **Utilize Profile Data:**
    * For **spawning**, the profile's `object_to_spawn` is used, and a *clone* of the profile is given to the new instance (`staticProfileData`). The instance then uses this blueprint to roll its unique stats, initialize skills, etc.
    * For **crafting**, recipe profiles provide ingredients (which are paths to item profiles), times, and outputs.
    * For **AI**, entity profiles provide behavioral cues, diet, and capabilities.
    * For **UI**, item/entity profiles provide names, descriptions, and sprites.

This unified system makes `GameData` the central, easily navigable repository of your game's static content, enabling clean, data-driven logic across all your game mechanics.

---

## TLDR / Summary of What You Were Looking For

You're looking for a system where `global.GameData` serves as the single, comprehensive database for all static game definitions (entities, items, recipes, etc.). Accessing a path like `GameData.Entity.Pop.GEN1` directly provides the complete data profile for that "GEN1" Hominid concept—including its base stats, sprite info, default behaviors, and even rules for randomization. This eliminates the need for separate, large `EntityType` enums as primary keys to other data tables. Core game functions, such as `spawn(GameData.Entity.Pop.GEN1, amount, formation_details)`, will take these direct profile references (or a simplified `ID` enum that maps to them) as input, using the rich data within the profile to orchestrate complex actions like patterned group spawning or crafting, making the code intuitive and data-driven.

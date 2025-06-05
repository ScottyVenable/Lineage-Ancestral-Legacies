# Project To-Do List: Lineage - Ancestral Legacies (Target: Alpha 0.2)

**Overall Goal for Alpha 0.2:** Achieve a stable, playable prototype demonstrating the core survival loop: Pops have needs, can interact with the world to satisfy them (foraging, basic crafting), and respond to player commands. The game should be testable for basic mechanics and provide a foundation for future features.

---

## I. Core Data System & Initialization (Foundation)

Status: Mostly complete, needs final checks and potential additions.
Tasks:
    [ ] **Finalize `default_[category].json` Files:**
        [ ] Create/Verify `default_items.json` with placeholder item(s) if `items.json` is missing.
        [ ] Create/Verify `default_resource_nodes.json` with a placeholder node if `resource_node_data.json` is missing.
        [ ] Create/Verify `default_structures.json` if `structure_data.json` is missing (even if no structures are used in Alpha 0.2, for system completeness).
        [ ] Create/Verify `default_entities.json` with a basic pop profile if `entity_data.json` is missing.
        [ ] Create/Verify `default_pop_names.json` if `name_data.json` is missing.
        [ ] Create/Verify `default_pop_states.json` with core states (Idle, Foraging, Resting) if `pop_states.json` is missing.
        [ ] Create/Verify `default_recipes.json` with a placeholder recipe if `recipes.json` is missing.
        [ ] In GameMaker Studio 2, go to `Included Files` and add each of these `default_*.json` files.
        [ ] Test `scr_load_external_data_all` by temporarily renaming a primary JSON (e.g., `items.json` to `items.json.bak`) and ensuring the game loads the corresponding `default_items.json` without crashing.
    [ ] **Expand `entity_data.json` for Basic Needs & Stats:**
        [ ] Open `entity_data.json` (or `default_entities.json`).
        [ ] For the primary pop profile (e.g., "GEN1_HOMINID"), add a `base_needs` struct.
        [ ] Inside `base_needs`, define:
            [ ] `hunger: { max: 100, decay_rate_per_second: 0.1, critical_threshold: 20 }` (adjust values as needed)
            [ ] `thirst: { max: 100, decay_rate_per_second: 0.15, critical_threshold: 20 }`
            [ ] `energy: { max: 100, decay_rate_moving: 0.2, decay_rate_working: 0.3, recovery_rate_resting: 0.5, critical_threshold: 10 }`
        [ ] Modify `scr_load_external_data_all` (specifically the entity loading part) to parse this `base_needs` struct and store it within `global.GameData.Entity.Pop.[ProfileName].base_needs`.
        [ ] In `obj_pop`'s creation or initialization, ensure `self.needs` struct is populated using these values from `self.staticProfileData.base_needs`.
    [ ] **Review `Utilizing the Unified GameData System.md`:**
        [ ] Read through the document.
        [ ] Add a new section or update existing ones with code snippets showing how `global.GameData.Entity.Pop.[ProfileName].base_needs` is accessed during pop initialization.
        [ ] Add examples of how `global.GameData.pop_states` and `global.GameData.recipes` are used in `obj_pop` and crafting scripts respectively.

---

## II. Pop Lifecycle & Core Behaviors (Essential for Playability)

Status: Basic states and needs are in; needs more depth and autonomous actions.
Tasks:
    [ ] **Implement "Energy/Stamina" Need:**
        [ ] In `obj_pop`'s Create event (or initialization script):
            [ ] Add `energy: self.staticProfileData.base_needs.energy.max` to the `self.needs` struct.
            [ ] Initialize `self.needs_config.energy_decay_moving = self.staticProfileData.base_needs.energy.decay_rate_moving`, etc. for all energy parameters.
        [ ] In `scr_needs_update` (or `obj_pop`'s Step event where needs are updated):
            [ ] If pop is moving, `self.needs.energy -= self.needs_config.energy_decay_moving / room_speed`.
            [ ] If pop is in "Foraging" or "Crafting" state, `self.needs.energy -= self.needs_config.energy_decay_working / room_speed`.
            [ ] Ensure energy doesn't drop below 0.
        [ ] Modify pop's movement speed: `current_move_speed = base_speed * (self.needs.energy > 20 ? 1 : 0.5)`.
        [ ] Modify work efficiency (e.g., foraging/crafting time) if energy is low (conceptual for now, can be a multiplier).
    [ ] **Enhance "Resting" State:**
        [ ] In `obj_pop`'s state machine logic (e.g., in `scr_pop_determine_next_state` or Step event):
            [ ] If `self.needs.energy <= self.staticProfileData.base_needs.energy.critical_threshold`, prioritize transitioning to `PopState.RESTING`.
        [ ] In the "Resting" state's execution logic (`on_execute_state_script` or within the state's block in Step):
            [ ] `self.needs.energy += self.staticProfileData.base_needs.energy.recovery_rate_resting / room_speed`.
            [ ] Ensure energy doesn't exceed `self.staticProfileData.base_needs.energy.max`.
            [ ] Pop should play a "resting" animation/sprite if available.
        [ ] For Alpha 0.2, "Rest Spot" can be anywhere; pop just stops moving. (Future: seek `obj_shelter` or `obj_rest_spot`).
    [ ] **Autonomous Need Fulfillment (Basic):**
        [ ] Create a new script `scr_pop_autonomous_behavior_check(pop_instance)`.
        [ ] In `scr_pop_autonomous_behavior_check`:
            [ ] If `pop_instance.needs.hunger <= pop_instance.staticProfileData.base_needs.hunger.critical_threshold`:
                [ ] Find nearest `obj_redBerryBush` (or any known food source with tag "food_source").
                [ ] If found, set `pop_instance.target_object = nearest_food_source_id`.
                [ ] Transition `pop_instance` to `PopState.FORAGING`.
                [ ] Return `true` (action taken).
            [ ] Else if `pop_instance.needs.thirst <= pop_instance.staticProfileData.base_needs.thirst.critical_threshold`:
                [ ] Find nearest `obj_water_source`.
                [ ] If found, set `pop_instance.target_object = nearest_water_source_id`.
                [ ] Transition `pop_instance` to a new `PopState.DRINKING` (or a generic "SatisfyThirst" state).
                [ ] Return `true`.
            [ ] Else if `pop_instance.needs.energy <= pop_instance.staticProfileData.base_needs.energy.critical_threshold`:
                [ ] Transition `pop_instance` to `PopState.RESTING`.
                [ ] Return `true`.
            [ ] Return `false` (no autonomous action taken).
        [ ] Call `scr_pop_autonomous_behavior_check(self)` in `obj_pop`'s Step event, likely when in `PopState.IDLE` or if no current command.
    [ ] **Pop Death:**
        [ ] In `scr_needs_update`:
            [ ] If `self.needs.hunger <= 0` or `self.needs.thirst <= 0`, set a flag e.g., `self.is_dying = true; self.death_timer = room_speed * 5;` (5 seconds to die).
            [ ] If `self.is_dying` is true, decrement `self.death_timer`. If `self.death_timer <= 0`, then `instance_destroy()`.
        [ ] In `obj_pop`'s Destroy Event: `show_debug_message(self.instance_display_name + " has perished.")`.
    [ ] **Basic Pop Spawning (Manual for Alpha 0.2):**
        [ ] In `obj_controller`'s Step event or a Key Press event (e.g., 'P'):
            [ ] `var _profile = global.GameData.Entity.Pop.GEN1_HOMINID;` (or your default pop profile name).
            [ ] `spawn_single_instance(_profile, mouse_x, mouse_y);`
        [ ] Ensure `spawn_single_instance` script is robust and correctly initializes the new pop.

---

## III. Player Interaction & Commands (Core Gameplay Loop)

Status: Basic selection and movement. Needs more robust command feedback.
Tasks:
    [ ] **Command: Forage Specific Resource Node:**
        [ ] In `obj_controller`'s Global Right Mouse Pressed event (or similar input handling):
            [ ] If a pop is selected and the mouse is over an instance of `obj_redBerryBush` (or any object with a "forageable" tag):
                [ ] For each selected pop:
                    [ ] `pop_instance.commanded_target = instance_nearest(mouse_x, mouse_y, obj_redBerryBush);`
                    [ ] `pop_instance.current_state_id = global.GameData.pop_states.Foraging.id;`
                    [ ] `pop_instance.current_state_name = global.GameData.pop_states.Foraging.name;`
                    [ ] (Ensure pop's foraging logic uses `self.commanded_target`).
    [ ] **Command: Move to Water Source & Drink (New):**
        [ ] In `obj_controller`'s input handling:
            [ ] If a pop is selected and mouse is over `obj_water_source`:
                [ ] For each selected pop:
                    [ ] `pop_instance.commanded_target = instance_nearest(mouse_x, mouse_y, obj_water_source);`
                    [ ] Create a new state "Drinking" in `pop_states.json` (e.g., `id: 3, name: "Drinking"`).
                    [ ] `pop_instance.current_state_id = global.GameData.pop_states.Drinking.id;`
                    [ ] `pop_instance.current_state_name = global.GameData.pop_states.Drinking.name;`
        [ ] In `obj_pop`'s state logic for "Drinking":
            [ ] Move to `commanded_target`.
            [ ] Once at target, replenish `self.needs.thirst` over a short duration (e.g., 2 seconds).
            [ ] Transition to `PopState.IDLE` once thirst is full or duration ends.
    [ ] **Visual Feedback for Commands:**
        [ ] Create a simple sprite `spr_target_indicator` (e.g., a small circle or arrow).
        [ ] When a pop receives a command, create a temporary instance of an object `obj_target_indicator` at the target's position, or draw the sprite directly.
        [ ] `obj_target_indicator` could destroy itself after 1-2 seconds.
        [ ] For pop's current task: In `obj_pop`'s Draw event, if it has a `commanded_target`, draw a line to it or a small icon above the pop.

---

## IV. World Interaction & Resources (Making the World Live)

Status: Basic resource nodes exist. Needs depletion and simple interaction.
Tasks:
    [ ] **Resource Node Depletion & Regrowth (Simple):**
        [ ] In `obj_redBerryBush`'s Create event:
            [ ] `self.current_berries = self.staticProfileData.resource_yield_max;` (assuming this is in its GameData profile).
            [ ] `self.is_depleted = false;`
            [ ] `self.regrowth_timer_max = room_speed * 60;` (e.g., 60 seconds to regrow).
            [ ] `self.regrowth_timer = 0;`
        [ ] When a pop forages from it (e.g., in `obj_pop`'s Foraging state logic):
            [ ] `target_bush.current_berries -= 1;`
            [ ] If `target_bush.current_berries <= 0`:
                [ ] `target_bush.is_depleted = true;`
                [ ] `target_bush.sprite_index = spr_berryBush_depleted;` (or change `image_index`).
                [ ] `target_bush.regrowth_timer = target_bush.regrowth_timer_max;`
        [ ] In `obj_redBerryBush`'s Step event:
            [ ] If `self.is_depleted` and `self.regrowth_timer > 0`:
                [ ] `self.regrowth_timer -= 1;`
                [ ] If `self.regrowth_timer <= 0`:
                    [ ] `self.is_depleted = false;`
                    [ ] `self.current_berries = self.staticProfileData.resource_yield_max;`
                    [ ] `self.sprite_index = spr_berryBush_full;`
        [ ] Pops should not be able to forage from a bush if `is_depleted` is true.
    [ ] **Create `obj_water_source`:**
        [ ] Create a new object `obj_water_source`.
        [ ] Assign a sprite (e.g., `spr_water_puddle`).
        [ ] In `entity_data.json` (or `environment_data.json` if you make one):
            [ ] Define a profile for it, e.g., `WATER_PUDDLE: { object_to_spawn: obj_water_source, display_name_key: "Water Puddle", tags: ["water_source"] }`.
        [ ] Place some instances in the test room.
        [ ] Ensure `obj_pop`'s "Drinking" state logic correctly interacts with it (moves to it, simulates drinking).

---

## V. Crafting System (Barebones Implementation)

Status: JSON loading and basic logic are in. Needs player initiation and item creation.
Tasks:
    [ ] **Player-Initiated Crafting (Simplest UI):**
        [ ] In `obj_controller` (or a dedicated UI object), Key Press 'K' event:
            [ ] If a pop is selected (`global.selected_pop != noone`):
                [ ] `var _pop_inst = global.selected_pop;`
                [ ] Loop through `global.GameData.recipes`:
                    [ ] `var _recipe_id = ...;` // get recipe key/id
                    [ ] `if (scr_crafting_can_craft(_pop_inst, _recipe_id)) {`
                        [ ] `scr_crafting_perform_craft(_pop_inst, _recipe_id);`
                        [ ] `show_debug_message("Attempting to craft: " + _recipe_id);`
                        [ ] `break; // Craft first available for simplicity`
                    [ ] `}`
    [ ] **Crafting Process & Item Creation:**
        [ ] In `scr_crafting_perform_craft(_pop_inst, _recipe_id)`:
            [ ] `var _recipe_data = global.GameData.recipes[_recipe_id];`
            [ ] Set pop's state to "Crafting" and target to self (or a conceptual crafting spot).
            [ ] `_pop_inst.crafting_timer_max = (_recipe_data.crafting_time_seconds ?? 5) * room_speed;`
            [ ] `_pop_inst.crafting_timer = _pop_inst.crafting_timer_max;`
            [ ] `_pop_inst.current_recipe_to_craft = _recipe_id;`
        [ ] In `obj_pop`'s "Crafting" state logic:
            [ ] Decrement `self.crafting_timer`.
            [ ] If `self.crafting_timer <= 0`:
                [ ] `var _recipe_data = global.GameData.recipes[self.current_recipe_to_craft];`
                [ ] Loop through `_recipe_data.ingredients` and call `scr_inventory_remove` for each.
                [ ] Call `scr_inventory_add(_pop_inst.inventory_data, _recipe_data.result.id, _recipe_data.result.count)`.
                [ ] `show_debug_message("Crafted: " + _recipe_data.result.id);`
                [ ] Transition to `PopState.IDLE`.
    [ ] **Define 1-2 Very Simple Recipes in `recipes.json`:**
        [ ] Open `recipes.json`.
        [ ] Add:
            ```json
            "sharp_stone_basic": {
                "ingredients": { "stone_item": 1 },
                "result": { "id": "sharp_stone_item", "count": 1 },
                "description": "A sharpened piece of stone.",
                "crafting_time_seconds": 3
            },
            "stick_club_basic": {
                "ingredients": { "wood_stick_item": 1, "sharp_stone_item": 1 }, // Assuming sharp stone is consumed or acts as a tool check
                "result": { "id": "stick_club_item", "count": 1 },
                "description": "A simple club.",
                "crafting_time_seconds": 5
            }
            ```
        [ ] Ensure `item_data.json` has definitions for `stone_item`, `sharp_stone_item`, `wood_stick_item`, `stick_club_item`. Give them basic properties (name, sprite).

---

## VI. User Interface (UI) - Minimal Viable Product

Status: Basic Pop Info Panel exists. Needs to be fully functional for core info.
Tasks:
    [ ] **Pop Info Panel - Needs Display:**
        [ ] In `obj_UIPanel_Generic`'s Draw event (or wherever pop info is drawn):
            [ ] If selected pop is valid:
                [ ] Draw text: "Hunger: " + string(selected_pop.needs.hunger) + "/" + string(selected_pop.staticProfileData.base_needs.hunger.max)
                [ ] Draw text: "Thirst: " + string(selected_pop.needs.thirst) + "/" + string(selected_pop.staticProfileData.base_needs.thirst.max)
                [ ] Draw text: "Energy: " + string(selected_pop.needs.energy) + "/" + string(selected_pop.staticProfileData.base_needs.energy.max)
            [ ] (Consider simple bars later, text is fine for Alpha 0.2).
    [ ] **Pop Info Panel - Inventory Display:**
        [ ] Ensure `scr_inventory_draw` (or equivalent) is called in the Pop Info Panel's draw event, passing the selected pop's `inventory_data`.
        [ ] Verify item icons and quantities appear correctly.
    [ ] **Pop Info Panel - Current State Display:**
        [ ] In Pop Info Panel's Draw event:
            [ ] Draw text: "State: " + selected_pop.current_state_name.
            [ ] If pop has a `commanded_target` and it's an instance: Draw text: "Target: " + object_get_name(selected_pop.commanded_target.object_index).
    [ ] **Basic Game Notifications (Debug Log for now is fine):**
        [ ] Review code sections for pop death, item crafting, resource depletion, and critical needs.
        [ ] Ensure `show_debug_message()` calls are present and provide clear information for these events.

---

## VII. Stability & Refinement (Crucial for Alpha)

Tasks:
    [ ] **Bug Fixing:**
        [ ] Systematically test each implemented feature from sections II-VI.
        [ ] Specifically test pop state transitions (manual and autonomous).
        [ ] Test resource interactions and crafting.
        [ ] Identify and fix any `variable not set` errors or crashes related to GameMaker version quirks or new code.
    [ ] **Universal Needs System Refactor (VI. Additional Tasks from old list):**
        [ ] Review `scr_needs_update`. Identify parts tightly coupled to `obj_pop` specifically.
        [ ] Sketch out how `scr_needs_update` could take a generic `instance_id` and access its `needs` and `staticProfileData.base_needs` structs.
        [ ] For Alpha 0.2, the main goal is robustness for pops. Full universality can be deferred if it complicates Alpha 0.2 delivery, but initial thought is good.
    [ ] **Code Review & Cleanup (Self-Review):**
        [ ] For each new/modified script (`scr_pop_autonomous_behavior_check`, changes to `obj_pop`, `obj_controller`, `scr_crafting_perform_craft`, etc.):
            [ ] Verify adherence to `TEMPLATE_SCRIPT.txt` (header, regions).
            [ ] Add comments explaining logic, especially for new decision-making processes.
        [ ] Search for and remove any old/commented-out code that is no longer relevant to Alpha 0.2 features.
    [ ] **Basic Save/Load (Placeholder - Low Priority for Alpha 0.2 but consider structure):**
        [ ] Create a text file `save_game_plan.txt`.
        [ ] List data to save:
            [ ] For each pop: instance ID (or a persistent unique ID), x, y, current needs values (hunger, thirst, energy), inventory contents (item IDs and quantities).
            [ ] For each resource node: instance ID, current resource amount, regrowth timer.
        [ ] Briefly outline how `ds_map` or JSON strings could be used for saving/loading this data. (No coding yet).

---

## VIII. Documentation Updates

Tasks:
    [ ] **Update `README.md`:**
        [ ] Add a "Current Features (Alpha 0.2 Target)" section listing the main implemented functionalities.
        [ ] Add a "Known Issues/Limitations (Alpha 0.2)" section.
    [ ] **Update `GDD.md`:**
        [ ] Review sections on Pop Behavior (3.2), Needs (3.1), Crafting (3.5), Resources (3.4, 3.6).
        [ ] Add brief notes or sub-points reflecting the Alpha 0.2 level of implementation (e.g., "Resting state implemented for energy recovery," "Water sources provide thirst satisfaction," "Crafting initiated via debug key").
    [ ] **Ensure `DOCUMENT_FORMATTING_GUIDELINES.md` is followed for all new documentation.**
        [ ] Quick check of version numbers and formatting in updated docs.

---

This detailed breakdown should help a lot! It turns the bigger goals into smaller, more manageable steps. Remember, the aim for Alpha 0.2 is a functional, if simple, core experience. Good luck!

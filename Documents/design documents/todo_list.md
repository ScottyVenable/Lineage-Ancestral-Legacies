# Project To-Do List: Lineage - Ancestral Legacies (Target: Alpha 0.2)

**Overall Goal for Alpha 0.2:** Achieve a stable, playable prototype demonstrating the core survival loop: Pops have needs (hunger, thirst, basic energy), can interact with the world to satisfy them (foraging, basic crafting), and respond to player commands. The game should be testable for basic mechanics and provide a foundation for future features.

---

## I. Core Data System & Initialization (Foundation)

Status: Mostly complete, needs final checks and potential additions.
Tasks:
    [ ] **Finalize `default_[category].json` Files:**
        [ ] Create/Verify `default_items.json` with placeholder item(s) if `items.json` is missing.
        [ ] Create/Verify `default_resource_nodes.json` with a placeholder node if `resource_node_data.json` is missing.
        [ ] Create/Verify `default_structures.json` if `structure_data.json` is missing (even if no structures are used in Alpha 0.2, for system completeness).
        [ ] Create/Verify `default_entities.json` with a basic pop profile if `entity_data.json` is missing.
        [ ] Create/Verify `default_pop_names.json` if `pop_name_data.json` is missing.
        [ ] Create/Verify `default_pop_states.json` with core states (Idle, Foraging, Commanded_Wait/Rest) if `pop_states.json` is missing.
        [ ] Create/Verify `default_recipes.json` with a placeholder recipe if `recipes.json` is missing.
        [ ] In GameMaker Studio 2, go to `Included Files` and add each of these `default_*.json` files.
        [ ] Test `scr_load_external_data_all` by temporarily renaming a primary JSON (e.g., `items.json` to `items.json.bak`) and ensuring the game loads the corresponding `default_items.json` without crashing.
    [ ] **Expand `entity_data.json` for Basic Needs & Stats:**
        [ ] Open `entity_data.json` (or `default_entities.json`).
        [ ] For the primary pop profile (e.g., "GEN1_HOMINID"), add a `base_needs` struct.
        [ ] Inside `base_needs`, define:
            [ ] `hunger: { max: 100, decay_rate_per_second: 0.1, critical_threshold: 20 }` (adjust values as needed)
            [ ] `thirst: { max: 100, decay_rate_per_second: 0.15, critical_threshold: 20 }`
            [ ] `energy: { max: 100, decay_rate_moving: 0.2, decay_rate_working: 0.3, recovery_rate_waiting: 0.1, critical_threshold: 10 }` (Note: `recovery_rate_waiting`)
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
    [ ] **Implement Simplified "Energy/Stamina" Need:**
        [ ] In `obj_pop`'s Create event (or initialization script):
            [ ] Add `energy: self.staticProfileData.base_needs.energy.max` to the `self.needs` struct.
            [ ] Initialize `self.needs_config.energy_decay_moving = self.staticProfileData.base_needs.energy.decay_rate_moving`, etc. for energy parameters.
        [ ] In `scr_needs_update` (or `obj_pop`'s Step event where needs are updated):
            [ ] If pop is moving (and not in "Waiting/Resting" state), `self.needs.energy -= self.needs_config.energy_decay_moving / room_speed`.
            [ ] If pop is in "Foraging" or "Crafting" state, `self.needs.energy -= self.needs_config.energy_decay_working / room_speed`.
            [ ] Ensure energy doesn't drop below 0.
        [ ] Modify pop's movement speed: `current_move_speed = base_speed * (self.needs.energy > self.staticProfileData.base_needs.energy.critical_threshold ? 1 : 0.5)`. (Pop slows down if energy is critical).
    [ ] **Implement Player-Commanded "Wait/Rest" for Energy Recovery:**
        [ ] Create/Update a "Commanded_Wait" or "Player_Rest" state in `pop_states.json` (e.g., `id: 4, name: "Waiting"`).
        [ ] Player can command a pop to enter this state (e.g., via a hotkey 'C' as per GDD, or right-click option).
        [ ] In `obj_pop`'s state logic for "Commanded_Wait/Player_Rest":
            [ ] Pop stops current actions and remains idle.
            [ ] `self.needs.energy += self.staticProfileData.base_needs.energy.recovery_rate_waiting / room_speed`.
            [ ] Ensure energy doesn't exceed `self.staticProfileData.base_needs.energy.max`.
            [ ] Pop remains in this state until energy is full or player gives a new command.
    [ ] **Autonomous Need Fulfillment (Focus on Hunger & Thirst):**
        [ ] Create/Update script `scr_pop_autonomous_behavior_check(pop_instance)`.
        [ ] In `scr_pop_autonomous_behavior_check`:
            [ ] If `pop_instance.needs.hunger <= pop_instance.staticProfileData.base_needs.hunger.critical_threshold`:
                [ ] Find nearest `obj_resourceNode` with a "food_source" tag.
                [ ] If found, set `pop_instance.target_object = nearest_food_source_id`.
                [ ] Transition `pop_instance` to `PopState.FORAGING`.
                [ ] Return `true` (action taken).
            [ ] Else if `pop_instance.needs.thirst <= pop_instance.staticProfileData.base_needs.thirst.critical_threshold`:
                [ ] Find nearest `obj_water_source`.
                [ ] If found, set `pop_instance.target_object = nearest_water_source_id`.
                [ ] Transition `pop_instance` to `PopState.DRINKING`.
                [ ] Return `true`.
            [ ] (No autonomous transition to resting for low energy in Alpha 0.2; player manages this via "Wait/Rest" command).
            [ ] Return `false` (no autonomous action taken).
        [ ] Call `scr_pop_autonomous_behavior_check(self)` in `obj_pop`'s Step event, likely when in `PopState.IDLE` or if no current command.
    [ ] **Pop Death (Due to Hunger/Thirst):**
        [ ] In `scr_needs_update`:
            [ ] If `self.needs.hunger <= 0` or `self.needs.thirst <= 0`, set a flag e.g., `self.is_dying = true; self.death_timer = room_speed * 5;` (5 seconds to die).
            [ ] If `self.is_dying` is true, decrement `self.death_timer`. If `self.death_timer <= 0`, then `instance_destroy()`.
        [ ] In `obj_pop`'s Destroy Event: `show_debug_message(self.instance_display_name + " has perished due to unmet needs.")`.
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
            [ ] If a pop is selected and the mouse is over an instance of `obj_resourceNode` with a "forageable" tag:
                [ ] For each selected pop:
                    [ ] `pop_instance.commanded_target = instance_nearest(mouse_x, mouse_y, obj_resourceNode);`
                    [ ] `pop_instance.current_state_id = global.GameData.pop_states.Foraging.id;`
                    [ ] `pop_instance.current_state_name = global.GameData.pop_states.Foraging.name;`
    [ ] **Command: Move to Water Source & Drink (New):**
        [ ] In `obj_controller`'s input handling:
            [ ] If a pop is selected and mouse is over `obj_water_source`:
                [ ] For each selected pop:
                    [ ] `pop_instance.commanded_target = instance_nearest(mouse_x, mouse_y, obj_water_source);`
                    [ ] Create/Update "Drinking" state in `pop_states.json` (e.g., `id: 3, name: "Drinking"`).
                    [ ] `pop_instance.current_state_id = global.GameData.pop_states.Drinking.id;`
                    [ ] `pop_instance.current_state_name = global.GameData.pop_states.Drinking.name;`
        [ ] In `obj_pop`'s state logic for "Drinking":
            [ ] Move to `commanded_target`.
            [ ] Once at target, replenish `self.needs.thirst` over a short duration (e.g., 2 seconds).
            [ ] Transition to `PopState.IDLE` once thirst is full or duration ends.
    [ ] **Command: Wait/Rest (for Energy Recovery):**
        [ ] In `obj_controller` (e.g., Key Press 'C'):
            [ ] If pop(s) selected, command them to enter the "Commanded_Wait/Player_Rest" state.
    [ ] **Visual Feedback for Commands:**
        [ ] Create a simple sprite `spr_target_indicator` (e.g., a small circle or arrow).
        [ ] When a pop receives a command, create a temporary instance of an object `obj_target_indicator` at the target's position, or draw the sprite directly.
        [ ] `obj_target_indicator` could destroy itself after 1-2 seconds.
        [ ] For pop's current task: In `obj_pop`'s Draw event, if it has a `commanded_target`, draw a line to it or a small icon above the pop.

---

## IV. World Interaction & Resources (Making the World Live)

Status: Basic resource nodes exist. Needs depletion, simple interaction, and generalization.
Tasks:
    [ ] **Generalize Resource Nodes (e.g., `obj_redBerryBush` to `obj_resourceNode`):**
        [ ] **Design `obj_resourceNode` Parent Object:**
            [ ] Create a new object `obj_resourceNode`.
            [ ] This object will be the parent for all specific resource types (like berry bushes, stone deposits, etc.).
            [ ] In its Create event, it should initialize common properties based on `self.staticProfileData` (which will be assigned by `spawn_single_instance` using a profile from `global.GameData.Entity.ResourceNode`).
                [ ] `self.resource_item_id = self.staticProfileData.yields_item_id;`
                [ ] `self.current_resource_amount = self.staticProfileData.resource_yield_max;`
                [ ] `self.is_depleted = false;`
                [ ] `self.regrowth_timer_max = (self.staticProfileData.regrowth_seconds ?? 60) * room_speed;`
                [ ] `self.regrowth_timer = 0;`
                [ ] `self.sprite_full = self.staticProfileData.sprite_full;`
                [ ] `self.sprite_depleted = self.staticProfileData.sprite_depleted;`
                [ ] `self.sprite_index = self.sprite_full;` // Initial sprite
        [ ] **Define Resource Node Profiles in `resource_node_data.json` (or `entity_data.json`):**
            [ ] Create/Update `resource_node_data.json`.
            [ ] Define profiles for different resource nodes, e.g.:
                ```json
                "BERRY_BUSH_RED": {
                    "profile_id_string": "RN_BERRY_BUSH_RED",
                    "object_to_spawn": "obj_resourceNode", // All resource nodes will spawn this object
                    "display_name_key": "resource_name_berry_bush_red",
                    "tags": ["forageable", "food_source", "plant"],
                    "yields_item_id": "berry_red_item", // From item_data.json
                    "resource_yield_max": 5,
                    "resource_yield_per_gather": 1,
                    "regrowth_seconds": 60,
                    "sprite_full": "spr_berryBush_full", // Asset name
                    "sprite_depleted": "spr_berryBush_depleted" // Asset name
                },
                "FLINT_DEPOSIT_SMALL": {
                    "profile_id_string": "RN_FLINT_DEPOSIT_SMALL",
                    "object_to_spawn": "obj_resourceNode",
                    "display_name_key": "resource_name_flint_deposit",
                    "tags": ["minable", "stone_source"],
                    "yields_item_id": "flint_stone_item",
                    "resource_yield_max": 3,
                    "resource_yield_per_gather": 1,
                    "regrowth_seconds": 300, // Longer for non-plant resources
                    "sprite_full": "spr_flintDeposit_full",
                    "sprite_depleted": "spr_flintDeposit_empty"
                }
                ```
            [ ] Ensure `scr_load_external_data_all` loads these profiles into `global.GameData.Entity.ResourceNode`.
        [ ] **Implement `obj_resourceNode` Core Logic (Depletion/Regrowth):**
            [ ] In `obj_resourceNode`'s Step event:
                [ ] If `self.is_depleted` and `self.regrowth_timer > 0`:
                    [ ] `self.regrowth_timer -= 1;`
                    [ ] If `self.regrowth_timer <= 0`:
                        [ ] `self.is_depleted = false;`
                        [ ] `self.current_resource_amount = self.staticProfileData.resource_yield_max;`
                        [ ] `self.sprite_index = self.sprite_full;`
            [ ] Create a user event or script function `scr_resourceNode_yield(node_instance)`:
                [ ] If `!node_instance.is_depleted` and `node_instance.current_resource_amount > 0`:
                    [ ] `node_instance.current_resource_amount -= node_instance.staticProfileData.resource_yield_per_gather;`
                    [ ] If `node_instance.current_resource_amount <= 0`:
                        [ ] `node_instance.is_depleted = true;`
                        [ ] `node_instance.sprite_index = node_instance.sprite_depleted;`
                        [ ] `node_instance.regrowth_timer = node_instance.regrowth_timer_max;`
                    [ ] Return `node_instance.staticProfileData.yields_item_id`.
                [ ] Else return `undefined` or `noone`.
        [ ] **Remove `obj_redBerryBush` (or make it a child for visual distinction if needed, but logic moves to parent):**
            [ ] Delete `obj_redBerryBush` if its logic is fully moved to `obj_resourceNode` and its visual appearance is handled by sprites defined in the profile.
            [ ] Update any room instances of `obj_redBerryBush` to be `obj_resourceNode` and assign them the "BERRY_BUSH_RED" profile (this might need a manual step in room editor or a conversion script at game start if you have many).
        [ ] **Update Pop Foraging Logic:**
            [ ] `obj_pop`'s foraging state should now target instances of `obj_resourceNode`.
            [ ] When foraging, call `scr_resourceNode_yield(target_node_instance)` to get the item.
            [ ] Add the yielded item to pop's inventory.
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
                "ingredients": { "flint_stone_item": 1 }, // Assuming flint_stone_item from generalized node
                "result": { "id": "sharp_stone_tool_item", "count": 1 }, // Differentiate tool from material
                "description": "A sharpened piece of flint.",
                "crafting_time_seconds": 3
            },
            "stick_club_basic": {
                "ingredients": { "wood_stick_item": 1, "sharp_stone_tool_item": 1 }, // Using the tool
                "result": { "id": "stick_club_item", "count": 1 },
                "description": "A simple club.",
                "crafting_time_seconds": 5
            }
            ```
        [ ] Ensure `item_data.json` has definitions for `flint_stone_item`, `sharp_stone_tool_item`, `wood_stick_item`, `stick_club_item`. Give them basic properties (name, sprite).

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
            [ ] If pop has a `commanded_target` and it's an instance: Draw text: "Target: " + (instance_exists(selected_pop.commanded_target) ? selected_pop.commanded_target.staticProfileData.display_name_key ?? object_get_name(selected_pop.commanded_target.object_index) : "None").
    [ ] **Basic Game Notifications (Debug Log for now is fine):**
        [ ] Review code sections for pop death, item crafting, resource depletion, and critical needs.
        [ ] Ensure `show_debug_message()` calls are present and provide clear information for these events.

---

## VII. Stability & Refinement (Crucial for Alpha)

Tasks:
    [ ] **Bug Fixing:**
        [ ] Systematically test each implemented feature from sections II-VI.
        [ ] Specifically test pop state transitions (manual and autonomous).
        [ ] Test resource interactions (especially with generalized `obj_resourceNode`) and crafting.
        [ ] Identify and fix any `variable not set` errors or crashes related to GameMaker version quirks or new code.
    [ ] **Universal Needs System Refactor (VI. Additional Tasks from old list):**
        [ ] Review `scr_needs_update`. Identify parts tightly coupled to `obj_pop` specifically.
        [ ] Sketch out how `scr_needs_update` could take a generic `instance_id` and access its `needs` and `staticProfileData.base_needs` structs.
        [ ] For Alpha 0.2, the main goal is robustness for pops. Full universality can be deferred if it complicates Alpha 0.2 delivery, but initial thought is good.
    [ ] **Code Review & Cleanup (Self-Review):**
        [ ] For each new/modified script (`scr_pop_autonomous_behavior_check`, changes to `obj_pop`, `obj_controller`, `scr_crafting_perform_craft`, `obj_resourceNode`, etc.):
            [ ] Verify adherence to `TEMPLATE_SCRIPT.txt` (header, regions).
            [ ] Add comments explaining logic, especially for new decision-making processes.
        [ ] Search for and remove any old/commented-out code that is no longer relevant to Alpha 0.2 features.
    [ ] **Basic Save/Load (Placeholder - Low Priority for Alpha 0.2 but consider structure):**
        [ ] Create a text file `save_game_plan.txt`.
        [ ] List data to save:
            [ ] For each pop: instance ID (or a persistent unique ID), x, y, current needs values (hunger, thirst, energy), inventory contents (item IDs and quantities), `profile_id_string`.
            [ ] For each `obj_resourceNode`: instance ID, x, y, `profile_id_string`, current resource amount, regrowth timer, `is_depleted` status.
        [ ] Briefly outline how `ds_map` or JSON strings could be used for saving/loading this data. (No coding yet).

---

## VIII. Documentation Updates

Tasks:
    [ ] **Update `README.md`:**
        [ ] Add a "Current Features (Alpha 0.2 Target)" section listing the main implemented functionalities (including generalized resource nodes and simplified energy).
        [ ] Add a "Known Issues/Limitations (Alpha 0.2)" section.
    [ ] **Update `GDD.md`:**
        [ ] Review sections on Pop Behavior (3.2), Needs (3.1), Crafting (3.5), Resources (3.4, 3.6).
        [ ] Add brief notes or sub-points reflecting the Alpha 0.2 level of implementation (e.g., "Energy need is present and affects speed; recovery is player-managed via 'Wait' command for Alpha 0.2," "Resource nodes are generalized using `obj_resourceNode` and defined by profiles," "Water sources provide thirst satisfaction," "Crafting initiated via debug key").
    [ ] **Ensure `DOCUMENT_FORMATTING_GUIDELINES.md` is followed for all new documentation.**
        [ ] Quick check of version numbers and formatting in updated docs.

---

This detailed breakdown should help a lot! It turns the bigger goals into smaller, more manageable steps. Remember, the aim for Alpha 0.2 is a functional, if simple, core experience. Good luck!

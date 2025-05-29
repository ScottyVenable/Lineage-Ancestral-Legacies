<!-- filepath: g:\My Drive\Entertainment\Game Development\GameMaker Projects\Lineage - Ancestral Legacies (GameMaker)\GameMaker Project\Lineage\design documents\Idea Documents\coding_ideas.md -->
# Coding Ideas

## Pop State System Refactor

**Date Added:** May 26, 2025

**Concept:**
Transition the current pop state system to be more data-driven and universal.

**Details:**
*   **State Definitions in Database:** Store pop states (e.g., `FORAGING`, `HAULING`, `IDLE`, `COMMANDED`) as entries in a central database file (e.g., `scr_database.gml` or a JSON file).
*   **State Behavior Structs:** For each state, define a struct that outlines its associated behaviors, parameters, and valid transitions. This struct could include:
    *   `on_enter_state_script`: Script to run when entering the state.
    *   `on_execute_state_script`: Script to run each step while in the state (the main behavior logic).
    *   `on_exit_state_script`: Script to run when exiting the state.
    *   `valid_target_tags`: An array of tags or enums that this state can interact with (e.g., "forageable_resource", "stockpile", "construction_site").
    *   `interruptible_by`: A list of conditions or other states that can interrupt this state (e.g., hunger, threat, new command).
    *   `required_parameters`: List of parameters the state needs to function (e.g., `target_object_id` for foraging).
*   **Universal State Handler:** Create a more generic state machine within `obj_pop` that reads the current state's definition from the database and executes the appropriate scripts.
*   **Benefits:**
    *   **Modularity:** Easier to add, remove, or modify states without deep-diving into `obj_pop`'s step event.
    *   **Clarity:** Centralizes state logic and makes it easier to understand how each state functions.
    *   **Scalability:** Simplifies the addition of new behaviors and interactions.
    *   **Debugging:** Potentially easier to debug state transitions and behaviors by inspecting the data definitions.

**Potential Challenges:**
*   Initial setup and migration of existing states.
*   Ensuring the database and struct definitions are flexible enough to handle diverse state requirements.
*   Performance considerations if database lookups are frequent (though likely negligible if cached on pop creation or state change).

---

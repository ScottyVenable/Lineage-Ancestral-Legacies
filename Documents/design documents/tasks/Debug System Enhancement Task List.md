# Debug System Enhancement Task List

## I. Refinement & Consistency

- [ ] Standardize `Enabled` Flags: Ensure all primary debug categories (`global.Debug.Logging`, `global.Debug.Rendering`, `global.Debug.Entities`, etc.) consistently use a simple `.Enabled` flag directly under them for their master switch, rather than some having it (like `DevTools.Enabled`) and others having a different master switch (like `Logging.AllEnabled`). `Logging.AllEnabled` can still exist, but `Logging.Enabled` could be the true master for that category, which then respects `AllEnabled` and individual sub-flags.
- [ ] Enum for Logging Categories: Create a `DebugLogCategory` enum (e.g., `enum DebugLogCategory { WORLDGEN, POPS, AI, UI, ERROR, GENERAL }`) to be used with `queue_debug_message` and for the `global.Debug.Logging.[Category]Enabled` flags. This improves type safety and reduces string comparisons.
    - [ ] Update `global.Debug.Logging` flags to use these enum members as keys if possible, or map strings to them.
- [ ] Review `DebugStatus` Enum Logic:
    - [ ] Clarify the exact behavior for `DebugStatus.PAUSED`. Does it just stop messages from being added to the queue, or does it also pause on-screen rendering of debug info?
    - [ ] Ensure `DebugStatus.ERROR_ONLY` robustly suppresses all non-error logs while still allowing error-specific visuals/logging if `global.Debug.Errors.Enabled` is true.
- [ ] Consolidate `Entities` and `AI` Debugging: Some flags in `global.Debug.Entities` (like `ViewActiveState`, `ViewPerception`) overlap conceptually with `global.Debug.AI` (like `ShowAgentStates`, `ShowPerceptionRanges`). Review if these can be merged or made more distinct to avoid redundancy. For example, `Entities.ViewActiveState` might be generic, while `AI.ShowDetailedAIState` is more specific.
- [ ] Global Tile Size: You changed `TILE_SIZE` to `global.TILE_SIZE` in `scr_pop_hauling`. Ensure this global variable is initialized *before* `obj_debug_controller` if any debug rendering relies on it during its own Create event (unlikely, but good to keep in mind for initialization order).

## II. Interactivity & Control (Runtime Changes)

- [ ] Implement Dev Console Commands:
    - [ ] Create a command parser for the dev console.
    - [ ] Add commands to toggle boolean debug flags (e.g., `debug_toggle Rendering.ShowBoundingBoxes`).
    - [ ] Add command to set `global.Debug.Status` (e.g., `debug_set_status FULL`).
    - [ ] Add command to list current status of all major debug flags.
- [ ] Implement Dev Quick Menu:
    - [ ] Design a simple UI for the quick menu.
    - [ ] Add buttons/options to toggle frequently used debug flags.
- [ ] Debug Profiles/Presets:
    - [ ] Define a struct format for debug profiles.
    - [ ] Add functionality to save the current `global.Debug` settings to a named profile (e.g., in a `ds_map` stored in `global.Debug.Profiles`).
    - [ ] Add functionality to load a named profile, applying its settings.
    - [ ] (Optional) Save/load profiles to a JSON file for persistence between game sessions.

## III. Enhanced Output & Visualization

- [ ] Refine `global.debug_message_queue` Handling:
    - [ ] Modify `queue_debug_message` to store messages as structs: `{ timestamp: current_time, category: DebugLogCategory.ENUM_MEMBER, message: "...", severity: DebugSeverity.INFO }`. (Requires `DebugSeverity` enum: `INFO, WARNING, ERROR, CRITICAL`).
    - [ ] Implement on-screen log display in `obj_debug_controller` (Draw GUI event) that processes this queue.
        - [ ] Show only the last N messages.
        - [ ] Color-code messages by category or severity.
        - [ ] Allow scrolling through the on-screen log history (if desired).
- [ ] Contextual Entity Debug Display:
    - [ ] Implement logic to show a debug overlay when hovering over or clicking an entity (if `global.Debug.Entities.Enabled` is true).
    - [ ] Use `global.Debug.Entities.List` to "watch" specific entities for more detailed, persistent on-screen info.
- [ ] Implement `global.Debug.Errors.ShowErrorLocations`:
    - [ ] In your global error handler, if this flag is true, attempt to get the `x, y` of the instance causing an error (if available in the error object) and draw a temporary marker.
- [ ] Custom FPS/Performance Display:
    - [ ] If `global.Debug.Rendering.ShowFPS` is true, draw `fps_real`, `current_time - delta_time` (for frame time), and potentially `gpu_get_texture_memory_usage()` or instance counts.

## IV. System Integration & Helper Functions

- [ ] Create `scr_debug_helpers.gml`:
    - [ ] Implement `dlog_is_enabled(category_enum)` as discussed, for other scripts to easily check if logging for a category is active.
    - [ ] Implement `dlog_is_render_flag_enabled(render_flag_name_string)` (e.g., `dlog_is_render_flag_enabled("ShowBoundingBoxes")`).
    - [ ] Create a robust `queue_debug_message(category_enum, message_string, severity_enum)` function that uses the new struct format and respects all master switches and `DebugStatus`.
- [ ] Integrate with Global Error Handler:
    - [ ] Set up `exception_unhandled_handler()` to point to a custom error handling script.
    - [ ] This script should use `global.Debug.Errors.Messages` for standardized error outputs.
    - [ ] It should respect `global.Debug.ErrorHandling` flags (GracefulCatch, StrictMode).
    - [ ] Implement `global.Debug.Errors.LogToExternalFile` functionality within this handler.
- [ ] Assertion System:
    - [ ] Create a simple `assert(condition, message_key_or_string)` script.
    - [ ] If `condition` is false and `global.Debug.Errors.ShowAssertions` is true, it should log/display an assertion failure message (potentially using `global.Debug.Errors.Messages.AssertionFailed` and the provided message). It could also optionally pause the game or throw an error depending on `StrictMode`.

## V. Error Handling & Robustness (Within Debug System Itself)

- [ ] Review Initialization Order: Double-check that `obj_debug_controller` is one of the very first objects created to ensure `global.Debug` is available for all other systems.
- [ ] Safeguard `ds_list_create()`: While you have checks for `ds_exists` in the conditional logic, ensure `global.debug_message_queue` and `global.Debug.Entities.List` are only destroyed in a `Clean Up` event or `Game End` event for `obj_debug_controller` to prevent memory leaks.
- [ ] Validate Enum Usage: Ensure `DebugStatus` enum values are consistently used and compared correctly (e.g., `global.Debug.Status == DebugStatus.FULL`).

## VI. New Debug Feature Ideas

- [ ] Performance Profiling Snippets:
    - [ ] `global.Debug.Performance.Enabled = false;`
    - [ ] `global.Debug.Performance.TrackedSections = ds_map_create();`
    - [ ] Helper functions: `debug_perf_start("section_name")` (records `get_timer()`) and `debug_perf_end("section_name")` (calculates duration, stores/averages in `TrackedSections`).
    - [ ] Display these timings on screen or log them.
- [ ] Time Control Debug:
    - [ ] `global.Debug.Time.GameSpeedMultiplier = 1.0;`
    - [ ] `global.Debug.Time.StepFrameByFrame = false;`
    - [ ] Allow dev console to change game speed or advance one frame at a time. (Requires game loop to respect these).
- [ ] Entity Spawning/Manipulation Tool:
    - [ ] As part of `DevTools`, a simple UI or command to spawn any entity from `global.GameData` at the mouse cursor, or modify selected entity's stats.
- [ ] Save/Load System Debugging:
    - [ ] `global.Debug.SaveLoad.LogDetailedSteps = false;`
    - [ ] `global.Debug.SaveLoad.ValidateDataOnLoad = false;` (Perform extra checks on loaded data structures).

This list is pretty extensive, so definitely prioritize based on what you feel would give you the most benefit for your current stage of development! You've got a great setup to build upon.

This list should give you plenty to think about and work on for your debug system. Remember, a good debug system is an investment that pays off massively in the long run by making it easier to find and fix issues, and understand what your game is doing under the hood.

Let me know if you want to dive into any of these points in more detail! Happy coding!
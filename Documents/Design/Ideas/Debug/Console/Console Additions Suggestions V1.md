## Debug System Expansion Ideas (Console Focus)

Let's think about expanding into more specific areas of your game:

### 1. Game Logic & Event System:
* **Purpose:** Directly trigger or inspect game-wide events, quests (future), or specific logical conditions.
* **Command Ideas:**
    * `event.trigger [EventID_or_Name] {param1:value1, param2:value2}`: Triggers a specific game event by its ID or a unique name. The data block can pass event-specific parameters.
        * *Interacts with:* A future `EventManager` or your main game loop.
    * `event.list [active|available|triggered]` : Lists active, all available, or recently triggered game events.
        * *Interacts with:* `EventManager`.
    * `quest.status [QuestID_or_Name]` : (Future) Shows the current status, objectives, and progress of a specific quest.
        * *Interacts with:* A future `QuestManager`.
    * `quest.set_stage [QuestID, StageID] {bypassConditions:true}`: (Future) Advances a quest to a specific stage, optionally bypassing its usual trigger conditions.
        * *Interacts with:* `QuestManager`.

### 2. Advanced AI & Behavior Debugging:
* **Purpose:** Deeper inspection and manipulation of Pop AI beyond just setting a general state.
* **Command Ideas:**
    * `entity.ai.get_decision_info ["PopID_or_Name"]`: Logs the current considerations and scores for a Pop's AI decision-making process (what it *wants* to do).
        * *Interacts with:* `PopStateMachine.cs`, and the specific AI decision logic.
    * `entity.ai.force_action ["PopID_or_Name", ActionName] {targetID:"TargetObjectID", duration:10.0}`: Compels a Pop to attempt a specific low-level action (e.g., "EatFrom", "AttackTarget", "PickupItem") with specified parameters.
        * *Interacts with:* `PopStateMachine.cs`, and the Pop's action handling methods.
    * `entity.ai.get_memory ["PopID_or_Name", MemoryKey]` : (Future) If Pops have a memory system, this would retrieve specific memories.
        * *Interacts with:* A future Pop memory component.
    * `entity.ai.set_target_override ["PopID_or_Name", "TargetObjectID_or_SpecialKeyword"]`: Overrides a Pop's current interaction or movement target.
        * *Interacts with:* `PopStateMachine.cs` or `PopController.cs`.

### 3. Evolution, Genetics & Traits:
* **Purpose:** Inspect and manipulate the core evolutionary aspects of Pops.
* **Command Ideas:**
    * `entity.traits.list ["PopID_or_Name"]` : Lists all traits of a specific Pop, including their source (inherited, learned, temporary). (Expanding on the previous idea).
        * *Interacts with:* Pop's trait component/data.
    * `entity.traits.add ["PopID_or_Name", "TraitID_or_Name"] {duration:temp, source:"mutation"}`: Adds a trait, optionally with a temporary duration or a specified source.
        * *Interacts with:* Pop's trait component, `TraitSO.cs`.
    * `entity.traits.remove ["PopID_or_Name", "TraitID_or_Name"]`
    * `lineage.evolution.add_ep [Amount]` / `lineage.evolution.set_ep [Amount]`: Adds or sets Evolution Points for the current lineage.
        * *Interacts with:* A system managing Evolution Points (potentially `ResourceManager.cs` or a dedicated `EvolutionManager`).
    * `show.gamedata.traits [all|physical|mental|social]` : (Enhancement of previous) Lists all available traits in the game, filterable by category.
        * *Interacts with:* A `TraitDatabase` or system holding all `TraitSO`s.

### 4. World & Environment Manipulation:
* **Purpose:** Control aspects of the game world for testing.
* **Command Ideas:**
    * `world.environment.set_param [ParameterName, Value]` (e.g., `world.environment.set_param [Temperature, 25]`)
        * *Interacts with:* A future `EnvironmentManager`.
    * `world.spawn.resource_node ["NodeID_or_Name", x, y, z] {quantity:100}`: Spawns a specific resource node at coordinates.
        * *Interacts with:* A resource spawning system or `ResourceManager`.
    * `world.daynight.set_cycle_speed [Multiplier]` : Adjusts the speed of the day/night cycle.
        * *Interacts with:* `TimeManager.cs`.

### 5. UI Debugging:
* **Purpose:** Test and manipulate UI elements.
* **Command Ideas:**
    * `ui.toggle_panel [PanelName]` : Toggles visibility of a specific UI panel.
        * *Interacts with:* `GameUI.cs` or individual panel scripts.
    * `ui.inspect_element [ElementName_or_Path]` : Logs properties of a UI element.
    * `ui.trigger_notification {title:"Test", message:"This is a test notification.", duration:5.0, type:"info"}`
        * *Interacts with:* Your notification system (if separate from general `LogToConsole`).

---
## Task List for Debug System Expansion

Here's a breakdown of tasks to implement these ideas. Each generally involves:
1.  Defining the command structure (full name, usage string).
2.  Writing the command handler method in `DebugConsoleManager.cs`.
3.  Implementing the logic within the handler, often by calling methods on other managers/systems.
4.  Registering the command in `RegisterCommands()`.

**I. Game Logic & Event System**
    * **Task: `event.trigger` command**
        * **Name:** `event.trigger`
        * **Usage:** `event.trigger [EventID_or_Name] {param1:value1, ...}`
        * **Desc:** Triggers a game event with optional parameters.
        * **Handler:** `TriggerEventCommand(List<string> positionalArgs, Dictionary<string, object> dataBlockArgs)`
            * Needs an `EventManager.TriggerEvent(string eventName, Dictionary<string, object> parameters)` method.
    * **Task: `event.list` command**
        * **Name:** `event.list`
        * **Usage:** `event.list [active|available|triggered]`
        * **Desc:** Lists game events.
        * **Handler:** `ListEventsCommand(...)`
            * Needs `EventManager` methods like `GetActiveEvents()`, `GetAllEventDefinitions()`, `GetRecentlyTriggeredEvents()`.

**II. Advanced AI & Behavior Debugging**
    * **Task: `entity.ai.get_decision_info` command**
        * **Name:** `entity.ai.get_decision_info`
        * **Usage:** `entity.ai.get_decision_info ["PopID_or_Name"]` (uses selected if no arg)
        * **Desc:** Logs AI decision-making details for a Pop.
        * **Handler:** `GetPopAIDecisionInfoCommand(...)`
            * Pop's `PopStateMachine` or AI component needs a method to expose this debug info.
    * **Task: `entity.ai.force_action` command**
        * **Name:** `entity.ai.force_action`
        * **Usage:** `entity.ai.force_action ["PopID", "ActionName"] {targetID:"Target", ...}`
        * **Desc:** Forces a Pop to perform a low-level action.
        * **Handler:** `ForcePopActionCommand(...)`
            * `PopStateMachine` or `PopController` needs a method like `ForceAction(string actionName, Dictionary<string, object> actionParams)`.

**III. Evolution, Genetics & Traits**
    * **Task: Enhance `entity.traits.list` command**
        * Modify existing `PopInfoCommand` or create `entity.traits.list`.
        * **Usage:** `entity.traits.list ["PopID_or_Name"]`
        * **Desc:** Lists traits with source/duration.
        * **Handler:** Update/create handler to fetch detailed trait info.
    * **Task: `entity.traits.add` command (with duration/source)**
        * **Name:** `entity.traits.add`
        * **Usage:** `entity.traits.add ["PopID", "TraitID"] {duration:float, source:"mutation"}`
        * **Desc:** Adds a trait to a Pop.
        * **Handler:** `AddTraitToPopCommand(...)`
            * Pop's trait system needs to handle adding traits with optional temporary duration and source tracking.
    * **Task: `entity.traits.remove` command**
        * **Name:** `entity.traits.remove`
        * **Usage:** `entity.traits.remove ["PopID", "TraitID"]`
        * **Desc:** Removes a trait from a Pop.
        * **Handler:** `RemoveTraitFromPopCommand(...)`
    * **Task: `lineage.evolution.add_ep` & `lineage.evolution.set_ep` commands**
        * **Names:** `lineage.evolution.add_ep`, `lineage.evolution.set_ep`
        * **Usage:** `lineage.evolution.add_ep [Amount]`, `lineage.evolution.set_ep [Amount]`
        * **Desc:** Modifies lineage Evolution Points.
        * **Handlers:** `AddEvolutionPointsCommand(...)`, `SetEvolutionPointsCommand(...)`
            * Interacts with the EP management system.
    * **Task: `show.gamedata.traits` command**
        * **Name:** `show.gamedata.traits`
        * **Usage:** `show.gamedata.traits [all|physical|mental|social|TraitTag]`
        * **Desc:** Lists all available traits, filterable by category/tag.
        * **Handler:** `ShowGameDataTraitsCommand(...)`
            * Requires a way to access all `TraitSO`s and filter them.

**IV. World & Environment Manipulation**
    * **Task: `world.environment.set_param` command**
        * **Name:** `world.environment.set_param`
        * **Usage:** `world.environment.set_param [ParameterName, Value]`
        * **Desc:** Sets an environmental parameter.
        * **Handler:** `SetEnvironmentParameterCommand(...)`
            * Needs an `EnvironmentManager` with a method like `SetParameter(string paramName, object value)`.
    * **Task: `world.spawn.resource_node` command**
        * **Name:** `world.spawn.resource_node`
        * **Usage:** `world.spawn.resource_node ["NodeID", X, Y, Z] {quantity:100}`
        * **Desc:** Spawns a resource node.
        * **Handler:** `SpawnResourceNodeCommand(...)`
            * Needs a system to spawn resource nodes by ID/type at a location.
    * **Task: `world.daynight.set_cycle_speed` command**
        * **Name:** `world.daynight.set_cycle_speed`
        * **Usage:** `world.daynight.set_cycle_speed [Multiplier]`
        * **Desc:** Adjusts day/night cycle speed.
        * **Handler:** `SetDayNightCycleSpeedCommand(...)`
            * `TimeManager.cs` needs a public method to adjust `realTimeToGameDayRatio` or a new speed multiplier.

**V. UI Debugging**
    * **Task: `ui.toggle_panel` command**
        * **Name:** `ui.toggle_panel`
        * **Usage:** `ui.toggle_panel [PanelName]`
        * **Desc:** Toggles visibility of a named UI panel.
        * **Handler:** `ToggleUIPanelCommand(...)`
            * `GameUI.cs` needs a way to find and toggle panels by name (e.g., store panels in a dictionary).
    * **Task: `ui.trigger_notification` command**
        * **Name:** `ui.trigger_notification`
        * **Usage:** `ui.trigger_notification {title:"Title", message:"Msg", type:"info", duration:5.0}`
        * **Desc:** Displays a test UI notification.
        * **Handler:** `TriggerUINotificationCommand(...)`
            * Interacts with your UI notification system.

**VI. General `DebugConsoleManager.cs` Enhancements**
    * **Task: Refine `GetTargetPop` for Name-Based Search**
        * Modify `GetTargetPop(string popIdentifier)` to iterate `FindObjectsByType<Pop>()` and compare `pop.name` (case-insensitively) if `popIdentifier` is not an int ID and no single pop is selected.
    * **Task: Complete Placeholder Commands**
        * Implement the logic for `ToggleDebugCommand`, `ToggleStatsCommand`, `DebugPopPathsCommand` by making them call the appropriate methods in `DebugManager.cs` or other relevant debug components.
    * **Task: Improve Log Coloring in `LogToConsole`**
        * Allow more specific color tags (e.g., `<color=yellow>Warning:</color>`) to be passed through or add more `isError`-like flags for different log types (warning, success, command output).
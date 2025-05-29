# ✅ Unity Migration Master Task List for *Primitive Ascend*

This is a unified task list pulling from your GDD, recent dev logs, and engine switch to Unity. It's organized by core systems with actionable subtasks.

---

## 🎮 Core Gameplay Systems

### ✅ Pop Entity System (Unity Prefab)

- [ ] Create `Pop` prefab with base SpriteRenderer and Animator
- [ ] Add `PopController.cs` MonoBehaviour to manage behavior
- [ ] Create `PopData` class for stats, traits, inventory
- [ ] Setup `ScriptableObject` system for traits and roles

### 🧠 Finite State Machine (FSM)

- [ ] Create `IState` interface or abstract base class
- [ ] Implement `IdleState`, `WanderState`, `ForageState`, `CommandedState`, `WaitState`
- [ ] Integrate FSM with PopController behavior
- [ ] Add priority handling for state transitions

### 🍓 Foraging System

- [ ] Design `BerryBush` prefab with berry count and regrowth logic
- [ ] Add interaction collider and click detection
- [ ] Create `ForageState` behavior with berry harvesting logic
- [ ] Trigger sprite sway with spring physics (Unity Joint or Animation)

### 📦 Inventory System

- [ ] Create `Inventory.cs` component using `Dictionary<ItemSO, int>`
- [ ] Create base `ItemSO` ScriptableObject
- [ ] Add inventory UI panel (Unity UI or UI Toolkit)
- [ ] Implement `AddItem`, `RemoveItem`, and display functions

### 🧬 Genetic Trait System

- [ ] Design `TraitSO` with type, modifiers, visual effects
- [ ] Build `GeneticTreeUI` (modular, category tabs)
- [ ] Hook up Evolution Point system
- [ ] Implement basic inheritance & mutation logic

---

## 🏞️ Environment & Resource Interactions

### 🌳 Biomes & Tilemaps

- [ ] Setup base Tilemap and Rule Tiles for one biome (e.g., forest)
- [ ] Add `ResourceNode.cs` for bush, rock, tree types
- [ ] Setup regrowth and resource depletion logic

### 🌬️ Weather & Day-Night Cycle (Optional Alpha Scope)

- [ ] Simple time-of-day lighting tint
- [ ] Placeholder for fog/rain/snow particle systems

---

## 👥 Dynamic Portraits

- [ ] Create layers for hair, eyes, mouth, skin tone, build
- [ ] Setup dynamic sprite combiner in `PortraitManager.cs`
- [ ] Reflect inherited traits visually
- [ ] Add portrait preview to Pop Info UI

---

## 🧠 Procedural Language System (Prototype Phase)

- [ ] Create `LanguageManager.cs`
- [ ] Load phoneme data from JSON or TextAsset
- [ ] Auto-generate word entries when new concepts are encountered
- [ ] Track concept → word mapping in a dictionary
- [ ] Display tribal speech bubbles using `TextMeshPro`
- [ ] Build simple UI for Lexicon (editable translations)

---

## 📊 UI Systems

### Pop Info Panel

- [ ] Show name, age, state, needs, inventory
- [ ] Hook up portrait and selected pop data

### Inventory Sidebar

- [ ] Toggleable panel with scrollable item list
- [ ] Filter by category (Food, Tools, Materials)

### Task/Command UI

- [ ] Right-click to trigger contextual action (Move, Forage, etc.)
- [ ] Highlight valid targets
- [ ] Visual selection box (drag)

---

## 🎥 Camera, Controls, & Feedback

- [ ] WASD or edge scrolling for camera movement
- [ ] Mouse wheel zoom (clamped)
- [ ] Click and drag to select multiple pops
- [ ] Floating text for berry harvesting
- [ ] Basic audio feedback (clicks, foraging, footsteps)

---

## 🛠 Technical Utilities

- [ ] Object pooling for berries/resources
- [ ] Serialization for saves (JSON or Binary)
- [ ] Build Dev Console for spawning, state checking, etc.

---

## 🔜 Next Major Milestone: Alpha Playable Loop

**Goal:** A single biome with:

- Selectable pops
- Movement, foraging, and idle/wander FSM
- Inventory and berry bushes
- Minimal UI: Info panel + Inventory

---

Let me know when you're ready to dive into any specific section—we can generate scaffolding scripts or system diagrams for Unity!

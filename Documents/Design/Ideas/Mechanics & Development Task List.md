Lineage: Ancestral Legacies - Mechanics & Development Task List
This document breaks down the core mechanics for "Lineage: Ancestral Legacies" into actionable development tasks, with considerations for Unity implementation and a phased approach towards an MVP and early alpha.

Phase 1: Core Loop Foundation (MVP - Minimum Viable Product)
Goal: Establish the absolute basic loop of Kaari survival, player interaction via Faith, and a glimpse of the legacy. Focus on getting something playable quickly to test core feelings.

1.1. Basic Kaari & Settlement Management
Task: Kaari Agent & Needs

Description: Create a basic Kaari "pop" unit. They need to exist, require food, and be able to "die" if food runs out.

Unity Implementation:

Kaari Prefab with a simple script (KaariAgent.cs).

Variables: currentFood, foodConsumptionRate, isAlive.

Basic AI: If 3D, use NavMeshAgent for potential future movement. If 2D, simple sprite and position. For MVP, they might not even move, just exist as a count.

Global KaariManager.cs to track all Kaari, total population, and handle spawning/despawning.

MVP Goal: Player can see a population count. If food runs out, population decreases.

Task: Basic Resource - Food

Description: Implement food as the primary resource. Kaari generate it (passively for MVP, or via a single action).

Unity Implementation:

Global ResourceManager.cs with currentFoodAmount.

Kaari contribute a small amount of food per tick/second if isAlive.

Simple UI Text element to display currentFoodAmount.

MVP Goal: Food count goes up with living Kaari, goes down as they consume it.

Task: Basic "Structure" - Population Cap

Description: Implement a simple population cap, perhaps represented by a single "Shelter Score" that increases with a basic action.

Unity Implementation:

ResourceManager.cs or SettlementManager.cs has populationCap.

A UI button "Improve Shelter" could increment this cap for a small cost (maybe time, or just a click for MVP).

MVP Goal: Player understands there's a limit to Kaari and can perform an action to increase it.

1.2. Rudimentary Belief & Influence (Architect)
Task: Basic Faith Generation

Description: Kaari generate "Faith Points" passively if their basic needs (food) are met.

Unity Implementation:

ResourceManager.cs adds currentFaithPoints.

If currentFoodAmount > 0, currentFaithPoints increase slowly over time.

Simple UI Text element to display currentFaithPoints.

MVP Goal: Player sees Faith accumulating when Kaari are not starving.

Task: Single Minor Miracle

Description: Player can spend Faith Points to perform one very basic miracle: "Gift of Sustenance" (spawns a small amount of food).

Unity Implementation:

UI Button "Gift of Sustenance" (cost X Faith).

OnClick: If currentFaithPoints >= cost, subtract cost and add a fixed amount to currentFoodAmount.

Basic particle effect or sound on successful miracle.

MVP Goal: Player has a way to interact using Faith, directly impacting survival.

1.3. Minimal Evolution/Progression
Task: Single "Technology/Insight"

Description: One simple upgrade: "Efficient Gathering" – increases the rate Kaari generate food or reduces consumption.

Unity Implementation:

UI Button "Research Efficient Gathering" (cost X Faith or triggers after Y time with living Kaari).

Boolean flag hasEfficientGathering.

If true, modify food generation/consumption rates.

MVP Goal: Player sees a tangible benefit from a simple progression mechanic.

1.4. Architect's Agency (Implicit)
Description: The player's agency is demonstrated through the "Gift of Sustenance" miracle. No separate system needed for MVP.

MVP Goal: Player feels like they are the guiding hand, even in a limited way.

Phase 2: Expanding Core Mechanics (Early Alpha)
Goal: Flesh out the core systems, introduce more player choices, basic persistence, and the beginnings of distinct Kaari roles and culture.

2.1. Settlement Management (Expanded)
Task: Kaari Roles (Forager, Builder)

Description: Allow assignment of Kaari to basic roles. Foragers gather food. Builders construct/improve structures.

Unity Implementation:

KaariAgent.cs: Add enum Role {None, Forager, Builder}.

UI to assign roles (e.g., +/- buttons for each role, drawing from an "unassigned" pool).

Forager role: Actively increases currentFoodAmount.

Builder role: Enables/speeds up structure creation.

Alpha Goal: Player actively manages Kaari workforce.

Task: Basic Structures (Fire Pit, Crude Huts)

Description: Introduce buildable structures. Fire Pit (provides Warmth). Crude Huts (increase pop cap, basic Morale).

Unity Implementation:

Structure Prefabs (simple visual representations).

Grid-based or simple click-to-place system.

Structure.cs script on prefabs: health, build time, effects (e.g., Fire Pit radiates "Warmth" in an area).

Builders assigned to a construction site reduce build time.

Alpha Goal: Player can build a small, functional settlement.

Task: New Resources & Needs (Wood, Warmth, Basic Morale)

Description: Add Wood (gathered by Foragers or a new Woodcutter role), Warmth (as a Kaari need, fulfilled by Fire Pits), and a simple Morale system (affected by food, shelter, warmth).

Unity Implementation:

ResourceManager.cs: Add currentWood, globalWarmthLevel (or Kaari have individual currentWarmth).

KaariAgent.cs: Add currentWarmth, currentMorale. Needs update based on environment/resources.

Low morale could mean reduced work efficiency.

Alpha Goal: More complex survival challenge and settlement dynamics.

2.2. Belief & Influence (Expanded)
Task: More Miracles/Influences

Description: Add 1-2 more ways to spend Faith (e.g., "Inspire Courage" – temporary boost to Hunter success/Builder speed, "Minor Healing" – chance to save a dying Kaari).

Unity Implementation:

New UI buttons and corresponding functions in ArchitectAbilities.cs.

Effects could be temporary stat boosts or event triggers.

Alpha Goal: More strategic options for using Faith.

Task: Faith Fluctuation Dynamics

Description: Faith now increases with positive events (successful hunts, new building, ritual success) and decreases with negative ones (Kaari deaths, starvation, failed guidance).

Unity Implementation:

Event system (Unity Events or custom delegates) to signal these occurrences.

FaithManager.cs listens to these events and adjusts currentFaithPoints.

Alpha Goal: Faith feels more responsive to the game state.

2.3. Ancestral Echoes (Proof of Concept)
Task: Basic Echo Logging & Recall

Description: Game saves ONE type of significant event (e.g., "First Chieftain's Name and Fate" or "Location of First Sacred Site Founded"). On a new playthrough, this piece of information is presented to the player (e.g., a dream, an old carving).

Unity Implementation:

Simple save system (JSON file or PlayerPrefs for this single piece of data).

EchoManager.cs handles saving the specific echo.

On new game start, EchoManager.cs loads the echo and triggers a simple UI display or event.

Alpha Goal: Player sees that the game remembers something from a previous attempt.

2.4. Evolution & Legacy (Basic Tech Tree)
Task: Small, Functional Tech Tree

Description: Implement a UI for a small tech tree (3-5 nodes). Branches: Survival (e.g., "Improved Tools," "Fire Mastery") and early Culture (e.g., "First Ritual," "Storytelling").

Unity Implementation:

ScriptableObjects for Tech Nodes (TechNodeSO.cs): prerequisites, costs (Faith, time, resources), unlocked effects/buildings/rituals.

UI to display the tree, show connections, and allow unlocking.

TechManager.cs to handle unlocking logic and applying effects.

Alpha Goal: Player can make strategic choices in guiding Kaari development.

2.5. Ritual System (Basic)
Task: First Unlockable Ritual

Description: One ritual unlocked via the tech tree (e.g., "Plea for Sustenance" or "Ritual of Gratitude").

Description: Kaari (perhaps a new "Spiritualist" role, or all Kaari participate) perform it. Costs Faith/resources. Has a chance of a positive outcome (e.g., food bonus, temporary morale boost) or a neutral/slightly negative one.

Unity Implementation:

Ritual.cs (ScriptableObject or class): defines costs, duration, possible outcomes, and probabilities.

UI to initiate the ritual.

Visual feedback (Kaari gathering, simple animation/particles).

RitualManager.cs to process the ritual and trigger outcomes.

Alpha Goal: Introduce the concept of active worship and its uncertain rewards.

Phase 3: Deepening Systems & Adding Flavor (Towards Beta)
Goal: Implement the full scope of the core mechanics, introduce more complex interactions, the beginnings of the corruption system, and more robust persistence.

3.1. Settlement Management (Advanced)
Task: Spiritualist Role & Advanced Structures

Implement the Spiritualist Kaari role (conducts rituals, boosts Faith generation).

Design and implement advanced structures (Shrines, Temples, improved housing, defensive structures if applicable).

Unity Focus: New AI behaviors for Spiritualist, new building prefabs with more complex effects.

Task: Full Resource & Needs Suite

Implement all planned resources and Kaari needs.

Develop a more nuanced Morale system with multiple contributing factors and clearer consequences.

Unity Focus: Refine ResourceManager.cs and KaariAgent.cs for greater complexity.

3.2. Belief & Influence (Full Implementation)
Task: Low Faith Consequences

Implement mechanics for when Faith is critically low: emergence of secular ideologies (Kaari less responsive to miracles, certain Faith-based buildings lose effectiveness), seeds of rebellion (reduced productivity, negative events), or early cults (small groups with different beliefs – could be simple for now).

Unity Focus: AI state changes, event triggers, potentially new UI elements to represent dissent.

Task: Unlock Spiritual Buildings/Techs via Faith

Ensure the tech tree and building system have significant branches that are heavily reliant on high Faith levels or specific Faith-based unlocks.

Unity Focus: Link TechManager.cs and building unlock logic to FaithManager.cs.

3.3. Ancestral Echoes System (Full Echo Log & Basic Seeding)
Task: Comprehensive Echo Logging

Implement the "Echo Log" to save multiple types of echoes: Personal (named pops, key events), Structural (ritual sites, ruins, monuments), Cultural (traditions like cannibalism, taboos, significant discoveries).

Include "Echo Weight" for each echo to determine its significance.

Unity Focus: Robust save/load system (likely custom JSON or binary serialization, not just PlayerPrefs). Data structures for different echo types.

Task: Procedural Seeding from Echoes

In new playthroughs, the game reads the Echo Log.

Procedurally seed elements into the new world based on weighted echoes:

Visual map changes (e.g., a "scarred" area where a great battle happened).

Ruins of past structures at their approximate locations.

Rare resources or unique environmental features tied to past events.

Events that reference past pop names, tribe fates, or rituals.

Unity Focus: Modify world generation scripts to incorporate Echo Log data. Create prefabs for echoed ruins/landmarks. Event system to trigger echo-related narratives.

3.4. Evolution and Legacy Progression (Full Tech Trees)
Task: Expanded Tech Trees

Fully develop the Survival, Culture, and Spirituality tech trees with diverse options and interdependencies.

Unity Focus: Populate TechNodeSOs, design compelling UI for larger trees.

Task: Basic Legacy Stats (Meta-Progression)

Implement saving of a few key "Legacy Stats" between all playthroughs (meta-progression):

Trait unlocks (e.g., "Kaari are slightly more resilient to cold" if a past lineage survived many harsh winters).

One or two "Mythic Narratives" (short text strings generated based on major past events, which might appear as ancient lore in future games).

Unity Focus: Separate global save file for meta-progression data.

3.5. Wendigo & Dark Echo System (Introduction)
Task: Corruption Points & Basic Manifestation

Implement "Corruption Points" – a hidden variable that can increase based on specific negative actions (e.g., cannibalism, failed dark rituals, mass death events near sacred Elder Kin sites).

If a "Cannibalism" cultural echo is formed (due to extreme starvation in a past game) and Corruption Points are high in an area in a new game:

Trigger text-based events: "Kaari report unsettling whispers in the woods," "Strange tracks found."

No actual Wendigo creature yet, just the ominous build-up.

Unity Focus: System to track and save/load Corruption Points (could be regional). Event triggers based on Corruption levels and specific echoes.

3.6. The Architect’s Agency (Divine Traits - Basic)
Task: Unlock First Divine Traits

Allow the player (Architect) to unlock 1-2 "Divine Traits" as part of meta-progression (e.g., "Protector of Harvests" – small global bonus to food production if previous Kaari lineages excelled at agriculture).

These traits are persistent across all playthroughs.

Unity Focus: Link to the Legacy Stats meta-progression save file. Apply bonuses at the start of new games.

3.7. Supporting Systems (Initial Implementation)
Task: Dynamic World Seeding (Enhancement)

Further integrate Echo data into world seed generation to create more varied and historically relevant starting conditions.

Unity Focus: Refine procedural generation algorithms.

Task: Pop Memory Traits (Basic)

When new Kaari are generated, give them a small chance to inherit a simple "Echo Trait" based on significant local echoes (e.g., "Descendant of the Fire-Bringer" – slightly better at tasks involving fire, if a major fire-related echo is nearby).

Unity Focus: Logic during Kaari generation to check nearby/relevant echoes and assign traits. Traits could be simple boolean flags or ScriptableObjects modifying stats.

Phase 4 & Beyond (Beta, Polish, Post-Launch)
Full Wendigo & Dark Echo System: Implement Wendigo creatures (AI, models, animations), haunted ruins with interactive elements, nightmares affecting Kaari.

Advanced Ancestral Echoes: Deeper narrative integration, more complex seeded events, visual storytelling through the environment.

The Architect’s Agency: Full suite of Divine Traits, mechanics for the Architect being forgotten and reclaimed through myth rediscovery or prophecy.

Low Faith Consequences (Advanced): Fully fleshed-out secularism, rebellions, and competing cults with their own mechanics and AI.

Elder Kin Interactions: More direct (though still rare and impactful) interactions and consequences.

Optional: Echo Journal/Tablet: In-game codex for players to track discovered echoes and piece together the overarching history.

Extensive Balancing & Polishing.

Sound Design & Music.

Tutorial / First Time User Experience.

This phased approach should help you build momentum and tackle this ambitious design systematically. Remember to test frequently, especially the core loop in Phase 1, to ensure the foundational experience is engaging! Good luck, Scotty, this sounds like an amazing project!

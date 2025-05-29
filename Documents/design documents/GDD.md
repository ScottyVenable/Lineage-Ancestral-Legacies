## **Lineage: Ancestral Legacies - Design Document**
**Version:** v2025.5.26.1
**Last Updated:** 2025-05-26

---

### 1. 🎯 Game Overview

**Genre:** Evolutionary RTS / Colony Simulation Strategy  
**Perspective:** 2D Top-Down Pixel Art  
**Platform:** PC (Windows, potentially Mac/Linux later) - Developed in Unity (current development version migrated from GameMaker Studio 2)  
**Target Audience:**
*   Strategy and simulation enthusiasts (e.g., RimWorld, Dwarf Fortress, Crusader Kings, Oxygen Not Included).
*   Players drawn to evolutionary storytelling, emergent behavior, genetic systems, and deep resource/colony management.
*   Players who appreciate sandbox experiences with long-term progression and legacy-building.
**Engine:** Unity (current version; previously GameMaker Studio 2)

#### 1.1. Game Summary (Elevator Pitch)
_Lineage: Ancestral Legacies_ invites you to shepherd a tribe of early hominids from their first fragile steps towards survival to becoming a complex, evolving society. You are the guiding spirit of their lineage, making critical decisions that shape their physical traits, mental capacities, cultural practices, and lasting legacy across generations. Manage resources, navigate perilous environments, unlock genetic potential, witness emergent social dynamics, and forge unique ancestral stories in a vibrant, procedurally-influenced world.

#### 1.2. Player Fantasy
*   **The Ancestral Guide:** Players feel like they are nurturing and guiding the destiny of a nascent species.
*   **The Evolutionary Architect:** Players experience the satisfaction of seeing their choices manifest in the genetic and cultural evolution of their pops.
*   **The Storyteller:** Players witness and indirectly shape unique, emergent narratives of survival, triumph, tragedy, and societal development.
*   **The Colony Manager:** Players enjoy the challenge of building a thriving, self-sufficient settlement against the odds.

#### 1.3. Unique Selling Propositions (USPs)
1.  **Deep Evolutionary Trait System:** Go beyond simple stat upgrades. A modular genetic tree allows for visible and impactful physical, mental, and emotional evolution, with traits passed down and mutating through generations.
2.  **Emergent Generational Storytelling:** Each pop is an individual with inherited traits, developing personalities, and relationships, leading to unscripted, memorable stories that span family trees.
3.  **Procedural Language Evolution:** A groundbreaking system where tribes develop unique languages, allowing players to decipher, learn, and even influence linguistic development (see Section 7.5).
4.  **Branching Tribal Legacies:** Guide your initial tribe to split and diverge, adapting uniquely to different biomes and challenges, creating a tapestry of interconnected (or rival) cultures.
5.  **Accessible Complexity:** Aims to provide deep simulation with intuitive controls and clear UI, making complex systems understandable and engaging.

---

### 2. 🧠 Core Pillars

1.  **Survive:** The fundamental challenge. Pops must satisfy basic needs (hunger, thirst, rest, safety, warmth) by interacting with their environment. This involves:
    *   **Resource Gathering:** Foraging berries, hunting animals (from small game to larger, riskier prey), fishing, gathering wood, stone, herbs.
    *   **Shelter & Safety:** Building rudimentary shelters for protection from elements and predators. Managing fire for warmth, cooking, and light.
    *   **Threat Avoidance:** Evading or confronting predators (e.g., sabertooth cats, dire wolves), navigating environmental hazards (e.g., storms, harsh temperatures).
    *   **Health & Wellness:** Dealing with injuries, simple illnesses (initially), and the effects of malnutrition or exhaustion.

2.  **Evolve:** The engine of long-term change. Pops earn Evolution Points (EP) through various actions (successful hunts, complex crafting, social breakthroughs, raising offspring, exploration). EP is spent on:
    *   **Genetic Trait Tree:** A branching tree with distinct paths for Physical, Mental, and Emotional traits.
        *   **Physical:** Strength, speed, agility, constitution (health), sensory acuity (sight, hearing, smell), tool-use aptitude, specific adaptations (e.g., cold resistance, climbing ability).
        *   **Mental:** Learning speed, crafting efficiency, problem-solving (e.g., inventing new tool uses), memory, pattern recognition, foresight (e.g., better food preservation).
        *   **Emotional/Social:** Empathy, aggression, bravery, curiosity, communication effectiveness, group cohesion, leadership potential.
    *   **Visible Changes:** Many traits will have subtle (or significant) visual representation on pop sprites or portraits.
    *   **Inheritance & Mutation:** Offspring inherit a mix of parental traits, with a chance for random mutations (positive, neutral, or negative), driving diversity.

3.  **Branch:** Societal divergence and adaptation. As tribes grow, internal pressures (resource scarcity, social factions, leadership disputes) or external opportunities (newly discovered fertile lands) can lead to:
    *   **Tribe Splitting:** Player can influence or react to events leading to a segment of the population forming a new tribe.
    *   **Environmental Specialization:** New tribes adapt to their specific biomes, developing unique traits, technologies, and cultural practices suited to their surroundings (e.g., a coastal tribe might excel at fishing and boat-making).
    *   **Cultural Drift:** Separated tribes will see their languages, beliefs, and social norms evolve independently over time.

4.  **Legacy:** The enduring impact of the player's guidance. This is about building something that lasts beyond individual pops:
    *   **Family Trees:** Detailed, interactive family trees to track lineage, genetic inheritance, and key life events of pops.
    *   **Civilizational Milestones:** Achieving significant breakthroughs (e.g., mastering fire, developing agriculture, forming first laws, creating art) that mark societal progress.
    *   **Knowledge & Tradition:** Passing down skills, stories, and traditions through generations, potentially through elder roles or specific "teaching" interactions.
    *   **End-Game Vision:** A sense of having guided a lineage from primal beginnings to a unique and established place in the world, with a rich history to reflect upon.

---

### 3. 🎮 Gameplay Mechanics

#### 3.1. Pop Entity (`obj_pop`)
*   **Core Attributes:** Health, Hunger, Thirst, Stamina, Mood, Age, Sex.
*   **Needs:** A struct (e.g., `self.needs = { hunger: 50, thirst: 50 }`) defining current levels of various needs. These are initialized on creation and updated by `scr_needs_update`.
*   **Inherited Traits:** Genetic traits affecting attributes, skills, and appearance.
*   **Skills:** Learned abilities (e.g., Foraging proficiency, Crafting skill, Combat effectiveness) improved through practice, influenced by traits.
*   **Needs System:** Pops will attempt to fulfill their needs based on priority. The `scr_needs_update` script manages the decay of needs (e.g., hunger, thirst) and can trigger state changes (e.g., to "Foraging" if hunger is low) based on thresholds. Values are typically 0-100. Failure to meet needs leads to negative mood, health penalties, or death.
*   **Life Cycle:** Birth, childhood (learning phase, dependent), adulthood (productive), old age (potential wisdom/teaching roles, declining physical ability), death (natural causes, starvation, predation, etc.). Records of dead pops are maintained for lineage tracking.
*   **Current State:** Pops now use `current_state_id` (numeric) and `current_state_name` (string) driven by definitions in `global.GameData.pop_states` (loaded from `pop_states.json`).

#### 3.2. Finite State Machine (Pop Behavior)
*(Expanding on the existing FSM, now data-driven by `pop_states.json`)*

Pops operate on a state machine defined in `global.GameData.pop_states`. Each state profile in the JSON can define parameters for behavior. Common states include:

| State          | Description & Triggers                                                                                                                               | Key Actions & Outcomes                                                                                               |
| :------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------- |
| **Idle**       | Default state (ID typically 0). Pop is not actively pursuing a goal. Triggers after other states complete or if no pressing needs/commands.        | Scans environment, minor stamina/mood recovery. May transition to Wandering or other states based on internal logic or needs. |
| **Wandering**  | If idle for too long or specific "curiosity" trait. Picks 1–N random waypoints (within 50–150 px radius) and hops between them, then idles.           | Explores immediate vicinity, chance to discover new resources or points of interest.                                   |
| **Satisfying Need** | Triggered when a need (hunger, thirst, rest) drops below a threshold.                                                                               | Seeks nearest valid source (e.g., food in inventory/storage, water source, shelter for rest). Consumes/uses resource. |
| **Foraging**   | Triggered by low hunger (via `scr_needs_update`) or direct command. Pop seeks and gathers food resources.                                            | Moves to a food source, gathers items (e.g., berries) into inventory. State continues until need is met or interrupted. |
| **Hunting**    | Player designates prey or pop (with "Hunter" role/trait) identifies opportunity. Requires appropriate tools/skills for success.                   | Stalks, attacks prey. Risk of injury. Successful hunt yields food (meat, hide).                                       |
| **Crafting**   | Player assigns crafting task at a station, or pop autonomously crafts needed items (tools, shelter components). Requires resources.                 | Consumes input resources, produces item, gains skill.                                                                |
| **Building**   | Player places a construction blueprint. Pop with "Builder" role/task brings resources and works on it.                                             | Consumes resources, incrementally builds structure.                                                                   |
| **Socializing**| Triggered by social need or proximity to other pops. Involves simple interactions (gestures, vocalizations, later complex language).                | Builds/strains relationships, affects mood, shares information (e.g., food location).                                 |
| **Learning/Teaching** | Child pops observe adults. Later, dedicated "Teacher" roles or elders pass on skills or knowledge.                                         | Increases skills of learner pops. Essential for cultural transmission.                                               |
| **Fleeing**    | Triggered by imminent danger (predator, environmental hazard) and low bravery/combat ability.                                                      | Moves rapidly away from threat source.                                                                               |
| **Fighting**   | Triggered by predation, territorial defense, or inter-tribal conflict.                                                                               | Engages target in combat. Outcome based on stats, tools, numbers.                                                    |
| **Commanded**  | Player-assigned move to a point or interaction with a specific target. Pops navigate directly. Speed = 2 (base).                                  | Overrides autonomous behavior for direct player control.                                                               |
| **Waiting**    | Static pause until new command. Triggered via 'C' key by player, or if pop has no valid actions/needs.                                             | Conserves energy. Player can use this to manage specific pops.                                                       |
| **Hauling**    | Pop has resources in inventory and a designated stockpile/storage exists. Moves resources from inventory/ground to storage.                        | Keeps settlement organized, makes resources available for others.                                                      |

*   **Separation:** `scr_separate_pops()` each step to push apart overlapping pops by 1 px.
*   **Depth Sorting:** `depth = -y` for all pops, resource nodes, and relevant objects.
*   **AI Roles/Priorities:** As society evolves, players can assign roles (Gatherer, Hunter, Crafter, Builder, Thinker/Researcher, Caregiver, Elder). Roles influence which tasks and states pops prioritize autonomously, potentially by modifying state transition logic or available actions within a state.

#### 3.3. Dynamic Portrait & Customization
*   **Features:** Hair color, hairstyle, hair length, beard style/presence, eye color, mouth shape, nose shape, skin tone, body build (influenced by strength/constitution traits).
*   **Portrait UI:** Layered sprites compose a unique icon in Pop Info Panel. Sprites reflect current age (child, adult, elder).
*   **Randomization & Inheritance:** New pops inherit a mix of parental visual traits with slight mutation chances. Certain genetic traits (e.g., "Robust Build," "Fiery Red Hair") directly influence specific visual features.
*   **Player Influence:** Players can name pops. No direct cosmetic surgery, but selective breeding (encouraging pops with desired traits to pair) can indirectly shape lineage appearance over generations.

#### 3.4. Resource & Inventory System
*   **Pop Inventory:** Each pop has a personal `ds_map { item_id → qty }` with limited capacity (can be upgraded via traits or items like baskets/bags).
*   **Communal Storage:** Players can designate stockpile zones or build storage structures (e.g., granary, woodshed). Pops will haul resources to these areas.
*   **Resource Categories:**
    *   **Food:** Berries, roots, meat, fish, eggs, cooked meals (later). Spoilage over time if not preserved.
    *   **Materials:** Wood, stone, flint, hides, fibers, bone, clay.
    *   **Tools:** Stone axe, spear, fishing rod, hammer, etc. (varying quality/durability).
    *   **Crafted Goods:** Baskets, pottery, clothing, shelter components.
*   **API:** `scr_inventory_init()`, `scr_inventory_add/remove()`, `scr_inventory_draw()`, `scr_inventory_clear()`, `scr_inventory_transfer_to_storage()`, `scr_inventory_check_capacity()`.
*   **UI:** Sidebar for selected pop's inventory. Separate UI for viewing communal storage.

#### 3.5. Crafting & Technology System
*(Now partially implemented with JSON-defined recipes)*
*   **Discovery Points (DP) / Research:** (Conceptual) Certain actions or "Eureka!" moments generate DP, spent to unlock new crafting recipes.
*   **Crafting Stations:** (Conceptual) Some recipes may require specific stations.
*   **Recipe List & Discovery (`recipes.json`):**
    *   Recipes are defined in `datafiles/recipes.json` and loaded into `global.GameData.recipes`.
    *   Each recipe specifies `ingredients` (item IDs and quantities) and a `result` (item ID and quantity).
    *   **Example `recipes.json` entry:**
        ```json
        "wooden_pickaxe": {
            "ingredients": { "wood": 3, "stone": 2 },
            "result": { "id": "wooden_pickaxe", "count": 1 }
        }
        ```
    *   The `scr_crafting_functions` script provides:
        *   `can_craft_recipe(_pop_instance, _recipe_id)`: Checks if the pop has the ingredients.
        *   `perform_craft_recipe(_pop_instance, _recipe_id)`: Consumes ingredients and adds the result (assumes inventory functions like `inventory_add_item` exist).
        *   `get_craftable_recipes_for_pop(_pop_instance)`: Lists available recipes.
*   **Tool Tiers & Quality:** (Conceptual)
*   **Crafting Process & Failure:** (Conceptual, `perform_craft_recipe` is basic for now)

#### 3.6. Environment & Interactions
*   **Biomes:** Multiple distinct biomes (Savannah, Temperate Forest, Lush Jungle, Arid Desert, Cold Taiga, Coastal regions, Wetlands, Mountainous areas). Each with unique:
    *   Flora & Fauna (resource nodes, animals, predators, medicinal plants, poisonous plants).
        *   **Flora Examples:**
            *   `obj_redBerryBush`, `obj_fruitTree` (seasonal fruits), `obj_rootPatch` (tubers), `obj_medicinalHerb` (specific types for different ailments), `obj_poisonousPlant` (avoid or use for traps/poisoned weapons later), `obj_fiberPlant` (for ropes, basic weaving), `obj_hardwoodTree`, `obj_softwoodTree`.
        *   **Fauna Examples:**
            *   Passive: Deer (meat, hide), Rabbits (meat, fur), Fish (schools in water bodies).
            *   Neutral: Boar (can become aggressive if approached/attacked).
            *   Aggressive: Wolves (pack hunters), Bears (solitary, powerful), Giant Spiders (jungle/caves), Crocodiles (wetlands/rivers).
            *   **Animal Behavior:** Herding, migration patterns (seasonal), predator stalking, animals having needs (hunger, thirst, sleep), specific diets. Some animals might alter the environment (e.g., beavers building dams, changing water flow).
    *   Temperature ranges and weather patterns (rain, snow, heatwaves, storms, fog, dust storms).
    *   Specific challenges (e.g., water scarcity in desert, extreme cold in taiga, disease hotspots in jungles, difficult terrain in mountains).
*   **Resource Nodes:**
    *   `obj_redBerryBush`: Holds X berries, Y regrowth delay, Z regen rate. Visual stages of growth/depletion.
    *   Other nodes: Fruit trees, root patches, flint deposits, clay pits, fishing spots, salt licks, mineral veins (copper, tin later). Finite but can regrow/replenish over time, influenced by seasons and weather. Some nodes might require specific tools to harvest effectively.
*   **Physics Sway:** Spring-damper model for bushes, small trees when pops interact or due to wind.
*   **Dynamic Elements:**
    *   **Day/Night Cycle:** Affects pop behavior (e.g., sleep), predator activity, visibility, temperature. Some plants might only be harvestable at certain times (e.g., night-blooming flowers for special potions).
    *   **Seasons:** Impacts resource availability, temperature, animal migration, plant growth cycles, water source levels (freezing in winter). Requires planning (e.g., stockpiling food for winter, creating warmer clothing).
    *   **Weather Events:** Rain (can fill water sources, douse fires, make ground muddy slowing movement), storms (can damage structures, scare pops, knock down trees), droughts (deplete water, kill plants), blizzards (extreme cold, low visibility, heavy snow accumulation).
    *   **Environmental Hazards:**
        *   Quicksand/Mud Pits (in wetlands or after heavy rain).
        *   Rockfalls/Landslides (mountainous areas, especially after rain or tremors).
        *   Forest Fires (can be started by lightning, spread based on wind and fuel).
        *   Floods (near rivers after heavy rain/snowmelt).
        *   Disease Clouds/Areas (stagnant water in jungles, near diseased carcasses – can cause sickness if pops pass through).
*   **Exploration & Map:** World can be a large, persistent map, or procedurally generated regions. Fog of War reveals explored areas. Discovering new areas can yield unique resources, ancient ruins/points of interest, new animal species, or isolated pop groups.
    *   **Points of Interest (POIs):**
        *   Ancient Shrines: Might offer a one-time DP boost, a unique piece of lore, or a rare resource.
        *   Abandoned Campsites: Could contain remnants of tools, old fire pits, or clues about previous inhabitants.
        *   Unique Natural Formations: Scenic spots that could provide a mood boost or inspiration for "Art" later.
        *   Resource Hotspots: Areas with an unusually high concentration of a valuable resource.

#### 3.7. Controls & Camera
*   **Selection:** Left-click single pop/object. Left-drag box-select multiple pops.
*   **Commands:** Right-click ground = move. Right-click resource/enemy/blueprint = context-sensitive action (forage, hunt, build).
*   **Camera:** WASD/Arrow keys to pan. Mouse wheel zooms (clamped 0.5x - 2x). Implemented in `obj_controller`.
*   **Time Controls:** Pause, Play, Fast Forward (e.g., 2x, 4x speed).
*   **Hotkeys:** For common actions (e.g., 'C' for Wait, 'B' for Build Menu, 'I' for Inventory).

#### 3.8. Combat System (Primitive)
*   **Initiation:** Triggered by hunting, predator attacks, or inter-tribal aggression (later game).
*   **Mechanics:** Pops auto-attack when in range. Factors:
    *   **Stats:** Strength (damage), Agility (dodge chance), Constitution (health).
    *   **Tools/Weapons:** Damage output, attack speed, range (e.g., spears vs. clubs).
    *   **Traits:** "Brave" pops less likely to flee, "Skirmisher" better at hit-and-run.
    *   **Numbers:** Group tactics (ganging up) are effective.
*   **Outcome:** Injury (can slow or disable), retreat, death. Successful hunts yield resources.
*   **No direct player micro-management of attacks beyond initial targeting.** Focus is on preparation (tools, traits) and strategic positioning.

#### 3.9. Social & Tribal Dynamics
*   **Relationships:** Pops develop simple relationships (kin, friend, rival, mate/partner) based on interactions, shared experiences, proximity, and traits (e.g., "Empathetic" pops form bonds easier, "Grumpy" pops make rivals easily). Visible in Pop Info Panel with a simple indicator (e.g., heart for mate, green icon for friend, red for rival).
    *   **Relationship Modifiers:** Positive interactions (sharing food, grooming, working together successfully, healing) improve relationships. Negative interactions (fighting over resources, failed joint tasks, one pop accidentally injuring another) damage them.
    *   **Kinship:** Automatically recognized (parent, child, sibling). Stronger initial positive bias. Incest taboo might emerge naturally or be a societal rule later.
*   **Mood System:** Influenced by needs fulfillment, social interactions, environment (e.g., comfort, weather, beauty of surroundings), success/failure in tasks, health, and significant life events. Affects efficiency, likelihood of positive/negative social interactions, and susceptibility to stress/illness.
    *   **Mood Levels:** Very Happy, Happy, Content, Neutral, Unhappy, Miserable.
    *   **Mood Effects:**
        *   Happy pops work faster, learn quicker, more likely to perform positive social actions.
        *   Unhappy pops work slower, higher chance of task failure, more irritable, might refuse tasks or start fights.
        *   Miserable pops might stop working altogether, become reclusive, or even attempt to leave the tribe.
*   **Social Events (Simple & Emergent, some can become Rituals):**
    *   Birth of a new pop (mood boost for family/tribe, potential small celebration).
    *   Death of a pop (mood debuff, potential for "funeral" gathering, affects relationships of the deceased).
    *   Sharing food (especially rare/prized food).
    *   Simple disputes over resources (can escalate if not managed, or resolved by a dominant pop or later, a leader).
    *   Formation of "pairs" for procreation (can be a small event, other pops might "acknowledge" it).
    *   Successful group hunt/major build completion (tribe-wide mood boost, cohesion increase).
    *   Discovery of a new important resource/area.
    *   Storytelling sessions (by elders or skilled pops, boosts mood and cohesion, can transfer "knowledge" or reinforce traditions).
*   **Leadership:**
    *   Pops with high "Leadership" traits (Charisma, Wisdom, Strength – depending on tribe's values) and positive social standing may emerge as informal leaders.
    *   **Emergence:** Achieved through successful actions (leading hunts, resolving disputes, making key discoveries) and social influence.
    *   **Effects:** Other pops are more likely to follow their "suggestions" (if player can issue them via the leader), work more efficiently near them, or rally around them in danger.
    *   **Formalization (Later):** Tribes might develop a "Chief" or "Elder Council" role, potentially through consensus, a challenge, or inheritance. This role could unlock specific tribe-wide abilities or decisions.
*   **Tribal Cohesion:** A tribe-wide metric influenced by average mood, resource availability, successful group activities, leadership stability, shared traditions, and external threats.
    *   **High Cohesion:** Pops work well together, share more readily, lower chance of internal conflict. Unlocks communal projects or efforts more easily.
    *   **Low Cohesion:** Increased infighting, resource hoarding, pops ignoring tasks, potential for factions to form or pops to leave/split off.
*   **Traditions & Customs (New Sub-section):**
    *   As tribes evolve, they can develop unique traditions based on significant past events, environmental factors, or influential leaders.
    *   **Formation:** Can emerge organically (e.g., if a specific food is eaten before every hunt and hunts are successful, it becomes a "pre-hunt meal" tradition) or be established by leaders/influential pops.
    *   **Examples:**
        *   Dietary preferences/taboos (e.g., "Our tribe never eats [animal type] because of [past event]")
        *   Specific ways of building shelters.
        *   Coming-of-age rituals for young pops.
        *   Seasonal festivals (e.g., harvest celebration, winter solstice).
        *   Preferred artistic motifs (seen in clothing, shelter decoration later).
    *   **Mechanical Effects:** Following traditions can boost mood and cohesion. Breaking them can cause social friction or individual mood debuffs. Traditions can also influence task choices or unlock unique interactions.
    *   **Evolution:** Traditions can change or fade over time, especially with new leadership or major environmental shifts.

#### 3.10. Genetic System & Evolution
*   **Evolution Points (EP):** Earned by the tribe as a whole through:
    *   Successfully raising offspring to adulthood.
    *   Surviving significant challenges (e.g., harsh winters, predator attacks).
    *   Making major discoveries (linked to DP, but EP is for genetic advancement).
    *   Achieving certain "generational milestones" (e.g., tribe reaches X population, survives Y years).
*   **Genetic Trait Tree:**
    *   A visual interface where players spend EP to unlock or enhance traits for their lineage.
    *   Traits are categorized (e.g., Physical, Mental, Social, Environmental Adaptation).
    *   Unlocking a trait makes it available to appear in newborn pops through inheritance. Enhancing a trait might increase its potency or chance of appearing.
*   **Trait Types:**
    *   **Physical:** Strength, Agility, Constitution (health), Speed, Eyesight (affects foraging/hunting range), Cold/Heat Resistance, Disease Resistance, Specific Visuals (hair/eye color if linked to a functional trait).
    *   **Mental:** Intelligence (affects learning speed, DP generation, problem-solving), Wisdom (affects decision-making, leadership), Creativity (Eureka chance, art/craft quality), Perception (spotting resources/dangers).
    *   **Social:** Empathy (better relationships), Charisma (leadership, persuasion), Aggression, Cooperativeness, Bravery.
    *   **Hidden/Recessive:** Traits that might not express unless conditions are met or two copies are inherited.
*   **Inheritance Mechanics:**
    *   When two pops reproduce, offspring inherit a mix of traits from parents.
    *   Some traits might be dominant, others recessive.
    *   Chance of random mutation: A new, un-unlocked (minor) trait appearing, or a slight variation of an existing one. This allows for organic discovery beyond the EP tree.
    *   Player cannot directly choose traits for newborns but can influence the gene pool by encouraging pops with desired (unlocked) traits to pair.
*   **Visual Impact:** Many genetic traits should have a visible impact on pop sprites (e.g., "Robust Build" makes pops larger, "Nimble" makes them slimmer, specific fur/hair color traits).
*   **"Meta-Genetics" (Divergence Age):**
    *   Focus shifts from unlocking basic traits to more complex genetic concepts.
    *   Examples: Selectively breeding for specific trait combinations, influencing mutation rates (perhaps through rituals or rare resources), or even unlocking "archetypal" lineage paths (e.g., a lineage that becomes exceptionally good in cold environments vs. one that excels in intellect).
*   **Trade-offs:** Some powerful traits might come with downsides or require more food/resources, creating interesting evolutionary choices.

---

### 4. 🎨 Art & Presentation

*   **Sprites:** 64x64 px grid for pops and key objects. Pops have 4-direction walk, idle, work (generic foraging/crafting), and combat animations. Visual distinction for age (child, adult, elder) and key genetic traits (build, unique hair/skin colors if evolved).
*   **Environment Art:** Richly detailed pixel art tilesets for different biomes. Dynamic elements like swaying foliage, weather effects (rain particles, snow accumulation).
*   **Bush Art & Resources:** Clear visual indication of resource type and depletion state.
*   **UI Aesthetic:** Tribal bones, cave paintings, and natural materials (wood, stone, hide) motif. Distinct header colors and icons for UI sections. Aim for clarity and minimal screen clutter.
*   **Visual Feedback:**
    *   Icons above pops for critical needs (hunger, thirst, injury).
    *   Progress bars for crafting, building, foraging.
    *   Visual effects for combat (simple impact sprites), healing.
    *   Subtle animations for pop emotions (e.g., slumped posture for sadness, energetic for joy) during idle or social states.

---

### 5. 🔊 Sound Design & Music

*   **Overall Musical Style:** Atmospheric, organic, and evolving.
    *   **Primitive Age:** Sparse, percussive, using natural-sounding instruments (drums, flutes, ambient textures).
    *   **Tribal Age & Beyond:** Music becomes more complex, introducing melodies and reflecting the tribe's "culture" (e.g., more rhythmic for a warlike tribe, more melodic for a contemplative one).
    *   **Dynamic Music:** Intensity can shift based on game events (e.g., danger, discovery, seasonal change).
*   **Sound Effects (SFX):**
    *   **Pop Actions:** Footsteps (varying by terrain), tool use (chopping, hammering, digging), eating, drinking.
    *   **Vocalizations:** Simple grunts, calls, expressions of effort, pain, joy. Evolves with the language system (see 7.5) into more distinct proto-words.
    *   **Environment:** Ambient sounds for biomes (wind, birds, insects, water), weather (rain, thunder), animal calls (passive and aggressive).
    *   **UI Interactions:** Subtle, satisfying clicks, hovers, and notifications. Non-intrusive.
    *   **Combat:** Weapon impacts, effort sounds, animal roars/snarls.
*   **Goal:** Create an immersive soundscape that enhances the feeling of a living, breathing world and provides crucial gameplay feedback.

---

### 6. 📊 UI Layout & UX Flow

#### 6.1. Main Game Screen
*   **World View:** Dominates the screen.
*   **Top Bar (Minimal):** Current date/time, season, overall tribe mood/alert status, current EP/DP.
*   **Sidebar Inventory (GUI space):** Top-right, fixed. Bone-border. "Inventory" header. Shows selected pop's personal inventory OR selected storage's contents. Clear item icons and counts. Filter/sort options.
*   **Pop Info Panel (Contextual):** Appears top-left when a single pop is selected. Collapsible sections:
    *   **Portrait & Name (Sex, Age):** Dynamic portrait, editable name.
    *   **Needs:** Bars for Hunger, Thirst, Stamina, Health, Mood.
    *   **Attributes & Skills:** Key stats (Strength, Agility, etc.) and skill levels.
    *   **Traits:** List of genetic and learned traits with tooltips.
    *   **Relationships:** List of family and significant others.
    *   **Inventory:** (Can be a tab here or primary in the right sidebar).
    *   **Current State/Task:** What the pop is currently doing. Option to manually assign tasks/priorities.
*   **Build/Command Menu (Contextual):** Appears near cursor or bottom of screen when relevant (e.g., after selecting pops and right-clicking, or pressing 'B' for build). Icon-based.
*   **Event Notifications:** Pop-ups or ticker messages for significant events (births, deaths, discoveries, attacks). Clickable to jump to location/relevant UI.
*   **Game Speed & Pause Controls:** Bottom-center or corner.
*   **Overhead Text (Minimal):** Pop-specific state ("Foraging," "Fighting") and berry/resource count during collection. Only for actively working/selected pops to reduce clutter.
*   **Tooltips:** Extensive use of tooltips on hover for UI elements, traits, resources, etc., to explain mechanics.

#### 6.2. UX Flow
*   **Onboarding/Tutorial:** A guided start for the first tribe, introducing core concepts step-by-step (selecting pops, issuing commands, foraging, basic needs, building first shelter/fire). Integrated into early gameplay rather than a separate mode. "Advisor" pop-ups or highlighted UI elements.
*   **Information Accessibility:** Player should be able to find information easily. E.g., clicking a resource shows its properties; clicking a need shows ways to satisfy it.
*   **Clear Feedback:** Player actions should have immediate and understandable visual/audio feedback.
*   **Task Management:** Easy ways to see what pops are doing, assign tasks, set priorities (e.g., via a tribe-wide job priority screen later).

---

### 7. 📈 Progression Flow & Eras

1.  **Dawn Age (Tutorial/Early Game):**
    *   **Focus:** Basic survival, understanding core needs.
    *   **Mechanics:** Foraging, drinking water, seeking shelter (natural caves), avoiding basic predators. Very simple tool use (e.g., using a sharp rock found on the ground).
    *   **Key Discoveries:** Controlled Fire, "Carrying" (basic inventory concept), Simple Vocalizations.
    *   **Social:** Small family group (2-5 pops). Recognizing kin.
    *   **Goal:** Establish a stable food/water source and a safe resting spot. Successfully raise first offspring.

2.  **Primitive Age (Early-Mid Game):**
    *   **Focus:** Tool crafting, group hunting, building rudimentary shelters.
    *   **Mechanics:** `obj_pop` FSM becomes more active. Crafting basic stone tools (choppers, crude spears). Hunting small, less dangerous game. Building lean-tos or simple huts. Basic food preservation (drying).
    *   **Key Discoveries:** Stone Tool Crafting, Basic Hunting Techniques, Shelter Construction, Simple Containers (gourds, woven grasses).
    *   **Social:** Small tribe (5-15 pops). Emergence of basic roles (best forager, brave hunter). Simple rituals around fire/successful hunts.
    *   **Goal:** Create a self-sufficient settlement with basic defenses and consistent food production. Unlock first tier of Genetic Traits.

3.  **Tribal Age (Mid Game):**
    *   **Focus:** Role specialization, communal structures, early culture, tribal expansion.
    *   **Mechanics:** More complex crafting (better tools, basic clothing from hides). Building communal huts, dedicated crafting areas, basic defenses (palisades). Trapping. Development of "oral traditions" (knowledge passed down). Beginning of tribe branching.
    *   **Key Discoveries:** Advanced Stone/Bone Tools, Tanning Hides, Basic Pottery, Fishing, Simple Herbalism, "Storytelling" (increases cohesion, knowledge transfer).
    *   **Social:** Medium tribe (15-30 pops). Defined roles (Hunter, Gatherer, Crafter, Builder, Elder/Storyteller). More complex social interactions, early forms of leadership. First inter-tribe encounters (potentially hostile or cautious).
    *   **Goal:** Establish a thriving village, explore surrounding territories, potentially form a second specialized settlement. Unlock significant Genetic Traits and begin the Procedural Language system.

4.  **Divergence Age (Late Game):**
    *   **Focus:** Environmental adaptation, distinct cultural identities, early "meta-genetics."
    *   **Mechanics:** Multiple tribes with unique adaptations based on biome and player choices. Rudimentary agriculture/animal tending experiments. More complex structures. Inter-tribe trade, diplomacy, or conflict.
    *   **Key Discoveries:** Domestication (plants/animals - very basic), Weaving, Simple Metallurgy (native copper if available), Boat Building (for coastal/riverine tribes), Formalized Rituals/Beliefs.
    *   **Social:** Larger tribes/confederations (30-50+ pops per group). Distinct cultures with unique languages, art styles (reflected in UI/decorations), and social norms. Formalized leadership structures.
    *   **Goal:** Guide multiple lineages to flourish, master their environments, and develop rich, distinct cultures. Significant progress in the meta-genetic tree.

5.  **Ancestral Legacy (End Game / Continuous Play):**
    *   **Focus:** Managing a web of interconnected lineages, shaping a grand meta-genetic tree, achieving long-term civilizational goals.
    *   **Mechanics:** Inter-tribe alliances, federations, or empires. Advanced "research" driven by specialized "Thinker" pops. Creation of lasting monuments or records. Deep interaction with the Procedural Language system (translation, influencing dialects).
    *   **Key Discoveries:** Rudimentary Writing/Record Keeping, Complex Social Organization (laws, councils), Early Philosophy/Ethics, Star Gazing/Calendars.
    *   **Social:** Complex inter-societal dynamics. Potential for lasting peace or grand conflicts. Legacy of specific ancestors becomes prominent.
    *   **Goal:** Achieve a set of long-term "Legacy Aspirations" (e.g., "Unite the Valleys," "Master the Frozen Wastes," "Achieve Linguistic Harmony," "Unlock the Human Potential" - maxing out key branches of the genetic tree). The game can continue in a sandbox mode after achieving primary goals.

---

### 8. 🌟 Key Feature Deep Dive: Procedural Language System

*(This is a significant USP and deserves elaboration)*

**Concept:** Tribes don't just grunt; they invent and evolve their own unique languages over time, offering a deep, engaging meta-game of linguistic discovery and influence for the player.

**Mechanics:**
1.  **Unlock:** The "Language Spark" is a mid-to-late Tribal Age discovery, possibly triggered by high collective "Mental" traits, specific social interactions, or a "Thinker" pop having a breakthrough.
2.  **Core Vocabulary Generation:**
    *   Initially, languages start with a small set of "proto-words" for very common concepts: `MEAT`, `BERRY`, `WATER`, `FIRE`, `DANGER`, `POP`, `YES`, `NO`.
    *   These words are generated based on a phonetic palette unique to the tribe's founding conditions or a random seed (e.g., Tribe A: "Gro," "Nak," "See"; Tribe B: "Zil," "Voo," "Fep").
3.  **Language Growth & Evolution:**
    *   **New Concepts:** As new items, actions, or social constructs appear (e.g., `SPEAR`, `HUT`, `FRIEND`, `LEADER`), the tribe needs words for them. New words can be formed by:
        *   Combining existing morphemes (e.g., `WATER-PLACE` for "River").
        *   Modifying existing words (phonetic drift).
        *   Spontaneous invention (rare).
    *   **Grammar:** Rudimentary grammar evolves:
        *   Word order (Subject-Object-Verb, etc.).
        *   Simple plurals, tense markers (initially very basic or contextual).
    *   **Linguistic Drift:** If tribes split, their languages will diverge over generations, creating distinct dialects and eventually mutually unintelligible languages.
4.  **Player Interaction & UI:**
    *   **Eavesdropping:** Player can "listen in" on pop conversations (represented by speech bubbles with their tribal words). Initially, it's gibberish.
    *   **Language Dictionary UI:** Unlocked with the Language Spark.
        *   Lists known words from various tribes.
        *   Player can attempt to translate words based on context (e.g., if a pop says "Roja!" and picks up a berry, player can hypothesize "Roja = Berry"). Correct hypotheses are confirmed over time through repeated contextual use.
        *   Shows etymology (how words evolved).
        *   Allows player to assign their own English (or chosen language) translations for easier understanding.
    *   **Linguist & Teacher Roles:** Pops with high "Mental" traits can become Linguists (faster at "inventing" or "standardizing" new words) or Teachers (spread common vocabulary and grammar within the tribe, slowing unwanted drift).
    *   **School Building / Scribe's Hut (Later Ages):** A physical place that boosts language research, standardizes vocabulary, and potentially leads to rudimentary writing.
5.  **Gameplay Impact:**
    *   **Inter-Tribe Communication:** Trade and diplomacy with tribes speaking different languages will be challenging. Requires learning their language (sending pops to "study," or by observing and using the Dictionary). Translators become valuable.
    *   **Knowledge Transfer:** Complex concepts are hard to pass on without developed language.
    *   **Cultural Identity:** Language becomes a strong marker of a tribe's identity.
    *   **Puzzles & Discovery:** Deciphering ancient texts or messages from isolated tribes could be late-game challenges/quests.
    *   **Influence:** Player might be able to subtly influence language (e.g., by popularizing certain player-named terms if they map them in the dictionary early).
6.  **Inspiration:** No Man's Sky alien languages, real-world linguistic evolution, interactive fiction language puzzles.

---

### 9. 🛠️ Dev Roadmap (High-Level Reiteration)

The development of "Lineage: Ancestral Legacies" will progress through several key milestones, each building upon the last to create an increasingly complex and engaging experience. A core design philosophy is to ensure that player choices in genetic evolution and technological discovery lead to tangible differences in how their lineages adapt and thrive, making each playthrough feel unique.

1.  **Core Prototype (Milestone 1): Primal Beginnings**
    *   **Pop FSM:** Implement core states: Idle, Wander, Commanded, and a very basic Forage (e.g., walk to bush, resource appears in inventory). Pops are essentially reactive, driven by immediate, simple needs.
    *   **Movement & Selection:** Basic point-and-click movement, single pop selection, rudimentary camera control.
    *   **UI:** Minimalist Pop Info Panel showing only essential stats (e.g., hunger). No complex inventory UI yet.
    *   **Resources & Environment:** Single resource type (e.g., "Generic Berries") from one type of interactable object (`obj_redBerryBush`). No resource depletion or regrowth initially.
    *   **Genetics/Tech:** None. Pops are homogenous beyond perhaps a random name.
    *   **Goal:** Prove the fundamental pop behavior loop and player's ability to interact with a pop in a simple environment. Establish the engine's capability to handle basic AI and object interaction. This stage represents the most primitive state of the hominids.

2.  **Alpha (Milestone 2): First Sparks of Adaptation**
    *   **Expanded FSM:** Introduce "Satisfy Need" (pops autonomously seek food/water), basic "Hauling" (moving items to a designated spot), and a very simple "Crafting" (e.g., combine 2 stones to make 1 "Sharp Stone" tool without a station).
    *   **Inventory & Resources:** Implement the full inventory API (`scr_inventory_...`) and a basic UI. Introduce 2-3 distinct resources (e.g., Berries, Sticks, Sharp Stones). Resource nodes now have finite quantities and simple depletion visuals.
    *   **Needs System:** Basic hunger, thirst, and a rudimentary rest need (e.g., pop stops and "sleeps" on the ground). Visual indicators for critical needs.
    *   **Crafting System:** Introduce 1-2 simple tool recipes (e.g., Sharp Stone, Basic Stick Club). No crafting stations yet; crafting is done from inventory. Tools provide minor bonuses (e.g., Sharp Stone slightly speeds up berry gathering).
    *   **Health & Combat:** Basic health attribute. Introduce a simple predator (e.g., a slow wolf) that can damage pops. Pops can "fight" back with basic attacks, influenced by whether they have a "Stick Club." Death implemented.
    *   **Genetic Tree & EP:** Implement the core UI for the Genetic Tree. Introduce 2-3 very basic, impactful traits (e.g., "Slightly Faster," "Slightly Stronger," "Better Forager"). Pops earn EP from simple survival tasks (e.g., gathering X food, surviving a day). Basic inheritance model where offspring have a chance to get parental traits.
        *   **Branching Impact:** Even these early traits will start to differentiate pop capabilities. A lineage that invests in "Better Forager" might expand faster due to food surplus, while one with "Slightly Stronger" might fend off the simple predator more easily.
    *   **Goal:** Achieve a playable survival loop. Pops can die, reproduce (simply), and the player can make initial, meaningful choices in the genetic tree that offer slight variations in gameplay. The hominids are now showing the first signs of tool use and heritable advantages.

3.  **Beta (Milestone 3): The Tribal Emergence**
    *   **Full FSM & Roles:** Implement all core FSM states including more complex Socializing, Learning/Teaching, and Building. Introduce basic AI Roles (Gatherer, Hunter, Builder) that players can assign, influencing autonomous task prioritization.
    *   **Progression & Biomes:** Gameplay should support progression through Dawn and Primitive Ages. Introduce 2-3 distinct biomes with unique resources, challenges, and visual styles. This will test the adaptability of different genetic builds.
    *   **Genetic Tree Expansion:** Expand the Genetic Tree with more diverse and impactful traits, including some with visual manifestations (e.g., "Thick Fur" for cold resistance, "Keen Eyes" for better spotting range). Introduce trade-offs (e.g., a trait for higher strength might also mean higher food consumption).
        *   **Branching Impact:** Genetic choices become more strategic. A tribe in a cold biome might prioritize "Thick Fur" and "Efficient Metabolism," while a tribe in a predator-heavy area might focus on "Agility" and "Pack Tactics" (a social/mental trait).
    *   **Crafting & Technology:** More complex crafting recipes, requiring specific resources from different biomes. Introduce basic crafting stations (e.g., Work Stump, Campfire). Unlock "Discovery Points" (DP) system for researching new recipes and simple structures (e.g., Lean-to Shelter, Basic Storage Pit).
        *   **Branching Impact:** Technological choices (what to research with DP) combined with available resources and genetic predispositions will lead to different tribal specializations. A tribe with good "Stone-knapping" genetics might excel at tool production, while another with "Herbalist" traits might focus on discovering medicinal concoctions.
    *   **Social Dynamics & Language:** Implement basic relationships (kin, simple friend/rival), mood system, and tribal cohesion. Introduce the initial Procedural Language System: vocabulary generation for core concepts, basic dictionary UI. Pops begin to use their unique proto-language.
        *   **Branching Impact:** Social structure and language will start to form unique cultural identities. A tribe that evolves "Cooperative" traits and develops language for "Teamwork" might excel at group hunts or large construction projects.
    *   **Tribe Splitting:** Basic mechanics for tribe splitting due to low cohesion or resource pressure, allowing for different player-guided lineages to diverge and adapt to new environments or social structures.
    *   **Sound & Music:** Implement core sound design and dynamic music reflecting the current age and situation.
    *   **Goal:** A feature-complete core game loop representing the Primitive and early Tribal ages. Player choices in genetics, technology, and social development should lead to visibly different and viable tribal paths. Playthroughs should feel distinct based on environmental challenges and player-guided evolution.

4.  **Release Candidate (Milestone 4): Divergent Legacies**
    *   **Full Progression:** Support progression through the Divergence Age, where choices made in earlier ages lead to significant differentiation.
    *   **Advanced Systems:**
        *   **Genetics:** Implement "Meta-Genetics" allowing for more profound lineage shaping and specialization.
        *   **Technology:** Complex crafting chains, specialized crafting stations, early metallurgy (if applicable), rudimentary agriculture/animal tending experiments.
        *   **Social:** Formalized leadership, complex social interactions, distinct cultural identities (reflected in art, rituals, language dialects). Advanced Language System features (grammar, linguistic drift, translation challenges).
        *   **Inter-Tribe Dynamics:** Basic mechanics for trade, diplomacy, or conflict with other (potentially AI-controlled or player-splintered) tribes who have also undergone their own unique development.
    *   **Polishing:** Full UI/UX polish, all art and animations complete, comprehensive soundscape.
    *   **Tutorial & Onboarding:** Implement a full, integrated tutorial guiding players through the initial stages and core mechanics.
    *   **Balancing & Optimization:** Extensive playtesting for balance across different evolutionary paths and biomes. Performance optimization for larger populations and world sizes.
        *   **Branching Impact:** This is where the "unique playthrough" promise fully matures. A lineage that focused on intellect and peaceful adaptation might develop advanced crafting and trade networks. Another that prioritized physical prowess and aggression might become dominant warriors, relying on raiding or hunting megafauna. A third might have specialized in extreme environmental survival, thriving where others cannot. The combination of genetic traits, discovered technologies, cultural norms (from traditions), and linguistic evolution should result in deeply distinct tribes with unique strengths, weaknesses, and stories.
    *   **Goal:** A polished, stable, and engaging game ready for release. The systems for genetic and technological branching should be robust, ensuring high replayability as players explore different evolutionary strategies and create unique ancestral legacies.

5.  **Post-Release: Expanding the Ancestry**
    *   **Support & Bug Fixes:** Ongoing maintenance and community support.
    *   **Content Updates:** Potential for new biomes, traits, animal species, late-game technologies, and world events to further deepen the existing branching paths.
    *   **System Expansions:** Deeper mechanics for features like religion/spirituality, advanced diplomacy, or even rudimentary forms of governance, each offering new avenues for unique societal development.
    *   **Major Expansions (if successful):** Could introduce entirely new Ages (e.g., "The Age of Sail," "The Iron Age") or significantly different gameplay modes (e.g., focusing on a single pop's life in detail, or a more abstract "civilization manager" mode for established empires). The core principle of genetic and cultural branching would continue to be a driving force.

---

### 10. 💰 Monetization Philosophy & Business Considerations

*   **Model:** Premium one-time purchase on PC platforms (e.g., Steam, GOG).
*   **Pricing:** To be determined based on final content scope and market comparables (aim for indie premium, e.g., $20-$30).
*   **No Microtransactions:** Core gameplay will not be compromised by microtransactions. No pay-to-win or pay-for-convenience.
*   **DLC/Expansions:** If the game is successful, substantial post-launch expansions that add significant new gameplay systems, ages, or content could be offered as paid DLC.
*   **Value Proposition:** Focus on delivering a deep, engaging, and complete experience that justifies the purchase price and fosters positive word-of-mouth.
*   **Marketing Hooks:** Evolutionary storytelling, genetic customization, unique procedural language system, emergent narratives, high replayability. Target relevant streamers and communities.

---

### 11. 💡 Future Enhancements (Post-Core Release / Expansions)

*(Many from your original GDD are excellent candidates for future expansions if not integrated earlier)*

*   **Quests & Scripted Events:** Dynamic challenges, moral dilemmas, unique encounters.
*   **Advanced AI Roles & Society:** Deeper specialization (Shaman, Artist, Scholar, Diplomat) with unique behavior trees and societal impact.
*   **Beliefs & Religion System:** Evolving rituals into organized belief systems with mechanical effects (e.g., specific bonuses, social cohesion/schism).
*   **Advanced Diplomacy & Trade:** Complex negotiations, trade routes, resource dependencies between tribes.
*   **Vehicles/Mounts (Late Game):** Rudimentary rafts, animal riding.
*   **Procedural Story Log:** A "Chronicles" feature that logs major events, births, deaths, discoveries for each lineage, creating a readable history.
*   **Modding Support:** Could significantly extend the game's life and community engagement.
*   **Cooperative Multiplayer:** Players each guide a different tribe in a shared world.

---

This expanded GDD provides a much more detailed blueprint. Remember, this is still a high-level strategic document. Each of these sections could be broken down further into many specific design documents for individual features.

Scotty, this is an ambitious and incredibly cool game concept! Focusing on the core loop and USPs first, then iteratively building out the complexity, will be key. I'm excited to see how "Lineage: Ancestral Legacies" evolves! Let me know your thoughts and where you'd like to dive deeper next.

---

### 7. 🌍 World & Lore
*(This section would detail the broader world, its history, overarching mysteries, and other elements that provide context to the tribe's journey. For now, we are focusing on a specific lore-related system.)*

#### 7.5. Procedural Language Evolution & Player Interaction
*(This is a highly ambitious feature aiming to create a unique linguistic fingerprint for each tribe, deeply tied to their experiences and environment. It adds a significant layer to the "Legacy" and "Branch" core pillars.)*

**Core Philosophy:**
The language of a tribe is a living record of its history, environment, and collective understanding. It should evolve organically, reflecting discoveries, significant events, and the unique challenges faced. The goal is for players to witness and subtly influence the birth and growth of a unique dialect for their people.

**Foundational Elements:**
*   **Phoneme Inventory (The Building Blocks):**
    *   The system starts with a predefined set of universal phonemes (basic sounds like /k/, /a/, /m/, /sh/). This list could be stored in an external file (e.g., `phonemes.json`).
    *   **Environmental Seeding (Initial Flavor):** At the start of a new game, the tribe's initial environment could influence the *probability* of certain phonemes being favored or disfavored in early word generation.
        *   *Example:* A tribe starting in a dense jungle with many soft rustling sounds might initially favor softer consonants and more vowel sounds, while a tribe in harsh, windy mountains might favor more guttural or sharp consonant sounds. This is a subtle flavor, not a hard restriction.
    *   This initial phoneme set is the raw material from which all words will be constructed.

*   **Concept Triggers (The Need for a Word):**
    *   New words are generated when the tribe encounters or conceptualizes something new that requires a label. This can be triggered by:
        *   Discovering a new resource (e.g., "Red Berry," "Flint Stone," "River Fish").
        *   Identifying a new animal (e.g., "Deer," "Wolf," "Cave Bear").
        *   Crafting a new tool (e.g., "Stone Axe," "Wooden Spear").
        *   Experiencing a significant event (e.g., "Fire," "Storm," "Attack").
        *   Developing a social concept (e.g., "Friend," "Leader," "Danger," "Family Unit").
        *   An abstract idea or observation (e.g. "Big", "Small", "Fast", "Hot", "Cold" - often linked to "Thinker" pop insights).
    *   These concepts can be predefined in categories within an external file (e.g., `concepts.json`) which might also include rules for how words for these concepts could be formed or what they might relate to.

**Word Generation Mechanics:**
*   **Proto-Word Formation:**
    *   When a new concept needs a word, the system combines phonemes from the available inventory according to generative rules (e.g., consonant-vowel-consonant structure, avoiding unpronounceable clusters initially).
    *   *Example:* For "Red Berry," the system might generate "Kama," "Piko," or "Solu." The first generated word becomes the accepted term.
    *   These words are initially simple and directly tied to concrete experiences.

*   **External Files & Templates for Structure:**
    *   `phonemes.json`: `{"consonants": ["p", "t", "k", "b", "d", "g", "m", "n", "s", "sh", "h", "l", "r", "w", "y"], "vowels": ["a", "e", "i", "o", "u"]}` (example, could be more nuanced).
    *   `word_structure_rules.json`: Could define basic syllable structures (CV, CVC, VC) and rules for combining them. E.g., `{"allow_consonant_clusters": false, "max_syllables_early": 2}`.
    *   `concept_lexicon.csv` (or JSON): This file would map internal game concepts (e.g., `item_berry_red`, `animal_deer`, `action_hunt`) to their current procedurally generated word. It starts empty and gets populated.
        *   `concept_id,generated_word,player_notes,decipher_status`
        *   `item_berry_red,kama,"Seems to mean berry or red food",confirmed`

*   **Uniqueness per Playthrough:**
    *   The specific phonemes chosen for a new word will have a degree of randomness.
    *   The order in which concepts are encountered and named will differ.
    *   Later evolutionary changes (see below) will further diverge languages.
    *   This ensures that each playthrough develops a distinct vocabulary.

**Language Evolution Over Time:**
*(This is where the language becomes more complex and nuanced, reflecting the tribe's cognitive and cultural growth. These changes would happen gradually over many generations or be tied to specific "Mental" trait advancements or "Thinker" pop Eurekas.)*

*   **Sound Shifts (Phonetic Drift):**
    *   Over long periods, common phonemes might subtly change their pronunciation.
    *   *Example:* An original /p/ sound might evolve into a /b/ sound in many words. "Piko" (berry) might become "Biko."
    *   This can be implemented as a low probability event affecting a random phoneme across the lexicon.

*   **Word Combination & Compounding:**
    *   As the tribe's understanding grows, they might combine existing simple words to describe more complex or nuanced concepts.
    *   *Example:* If "Kama" = "Berry" and "Zul" = "Water," then "Kamazul" might become the word for a juicy fruit found near water.
    *   This allows for an expanding vocabulary without generating entirely new roots for every single concept.

*   **Semantic Drift & Specialization:**
    *   The meaning of words can broaden, narrow, or shift over time.
    *   *Example:* An early word for "Sharp Stone" might later specialize to mean "Cutting Tool" specifically, while a new word is generated for "Blunt Stone" (hammer). Or, a word for "Danger" might later become specifically "Predator Danger."

*   **Emergence of Basic Grammar (Affixation, Pluralization, Tense):**
    *   **Affixes:** Common concepts might evolve into prefixes or suffixes.
        *   *Example:* If "Gor" means "Big," then "Gor-Kama" could mean "Big Berry." "Gor-" becomes a prefix.
    *   **Pluralization:** A common way to denote multiple items might emerge (e.g., repeating a vowel, adding a specific suffix like "-im" or "-ot").
    *   **Tense:** Simple markers for past/present/future actions could develop, perhaps from words meaning "before," "now," "after."
    *   These grammatical elements would be very rudimentary initially and could be linked to specific "Mental" trait thresholds or "Eureka" moments from pops with high linguistic aptitude.

**Player Interaction & Deciphering:**
*   **Initial Obscurity:**
    *   When the game starts, the player does not understand the tribe's utterances. Pop-up speech bubbles might show the generated words, perhaps with accompanying icons or animations to give contextual clues.
    *   *Example:* A pop points to a berry bush, an icon of a berry appears, and the speech bubble says "Kama!"

*   **Deciphering Process:**
    *   **Contextual Observation:** The primary way the player (and their "Thinker" pops) learn the language. If pops consistently say "Kama" when interacting with berries, the player can infer the meaning.
    *   **"Thinker" Pop Role:** Pops with high "Mental" traits, "Curiosity," or a specific "Linguist" or "Pattern Seeker" trait could accelerate the deciphering process. They might have a small chance per day to "figure out" a word they've heard frequently, adding it to the known lexicon.
    *   **Lexicon UI:** A dedicated UI screen where the player can see a list of encountered words, their suspected meanings, and their confirmation status (e.g., "Suspected: Berry," "Confirmed: Berry").

*   **Confirmation:** After enough contextual observations or a "Thinker" insight, a word's meaning is "confirmed" and will show its translation in tooltips or UI elements.

**Player Influence & Editing (Mid-to-Late Game Mechanic):**
*(This allows players to feel more connected to their tribe's culture and to smooth out any awkward procedurally generated terms.)*

*   **The "Lorekeeper's Hut" or "School of Words" (Example Structure):**
    *   Unlocking a specific building or societal role (e.g., "Elder Council," "Chief Linguist") grants access to the "Language Editing" interface.

*   **Language Editing Interface:**
    *   Players can view the full list of confirmed words and their generated forms.
    *   For any given concept (e.g., "Berry," "Hunt," "Friend"), the player can:
        *   **Suggest a New Word:** Type in their own preferred word (e.g., rename "Kama" to "Redfruit").
        *   **Suggest a Phonetic Respelling:** If the generated word is hard to pronounce or remember, suggest a new combination of existing phonemes.
    *   **Adoption Process:** Player suggestions are not instantly adopted.
        *   The suggestion might need to be "taught" by pops with high social influence or in a "teaching" role.
        *   There could be a tribe "acceptance" chance influenced by cohesion, the leader's traits, or how radical the change is.
        *   It might take time for the new word to propagate and replace the old one. Some older pops might even continue to use the old term for a while.

*   **Indirect Influence:**
    *   **Naming:** When players name pops, locations, or perhaps even significant tools, these names (or parts of them) could subtly influence the lexicon. The system might try to deconstruct these names into phonemes and incorporate them into future word generations if they fit the tribe's phonetic patterns.
    *   **Repeated Actions:** If the player consistently directs the tribe towards a specific activity (e.g., hunting a particular animal), words related to that activity might become more "developed" with more synonyms or related grammatical forms due to frequent use and conceptual importance.

This system, while complex to implement, would offer a truly unique and emergent narrative layer, making each tribe's development feel distinct and deeply tied to their world.

---

### Appendix A: JSON Data Structures

*(This new appendix will detail the format of key JSON files)*

#### A.1. `pop_states.json`

Defines the available behavioral states for pops. Loaded into `global.GameData.pop_states`.

*   **Structure:** A root JSON object where each key is a state name (e.g., "Idle", "Foraging").
*   **State Profile Fields:**
    *   `id` (Number): A unique numeric identifier for the state. Used in `obj_pop.current_state_id`.
    *   `name` (String): The human-readable name of the state. Used in `obj_pop.current_state_name`.
    *   *(Future fields: `animation_sprite_prefix`, `allowed_transitions`, `interrupt_priority`)*

*   **Example:**
    ```json
    {
        "Idle": { "id": 0, "name": "Idle" },
        "Foraging": { "id": 1, "name": "Foraging" },
        "Resting": { "id": 2, "name": "Resting" }
    }
    ```

#### A.2. `recipes.json`

Defines crafting recipes. Loaded into `global.GameData.recipes`.

*   **Structure:** A root JSON object where each key is a unique recipe identifier string (e.g., "wooden_pickaxe").
*   **Recipe Profile Fields:**
    *   `ingredients` (Object): A sub-object where each key is an `item_id` (string, from `item_data.json`) and the value is the quantity (number) required.
    *   `result` (Object): A sub-object describing the output.
        *   `id` (String): The `item_id` of the crafted item.
        *   `count` (Number): The quantity produced.
    *   `description` (String, Optional): A brief description of the recipe.
    *   *(Future fields: `crafting_time_seconds`, `required_skill_id`, `required_station_tag`)*

*   **Example:**
    ```json
    {
        "wooden_pickaxe": {
            "ingredients": { "wood": 3, "stone": 2 },
            "result": { "id": "wooden_pickaxe", "count": 1 },
            "description": "A basic pickaxe for light mining."
        },
        "berry_pie": {
            "ingredients": { "berries": 5, "water": 1 },
            "result": { "id": "berry_pie", "count": 1 },
            "description": "A simple, nourishing pie."
        }
    }
    ```

#### A.3. `entity_data.json` (Conceptual for Needs)

While not explicitly detailed for needs yet, `entity_data.json` (loaded into `global.GameData.entities`) would be the place to define base need parameters if they vary by entity type.

*   **Conceptual Fields within an entity profile (e.g., `global.GameData.entities.pop_human`):**
    *   `base_needs`: (Object)
        *   `hunger_max`: 100
        *   `hunger_decay_rate`: 0.01 (units per tick)
        *   `thirst_max`: 100
        *   `thirst_decay_rate`: 0.02
    *   `initial_needs_range`: (Object)
        *   `hunger_min_start`: 40
        *   `hunger_max_start`: 80

*(This section would be expanded as the Needs system becomes more deeply integrated with entity definitions.)*
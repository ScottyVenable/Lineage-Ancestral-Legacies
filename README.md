# Lineage: Ancestral Legacies

**Project Status:** In-Development (Alpha v0.1 - June 7, 2025)

This repository contains the Unity project for *Lineage: Ancestral Legacies*, an evolutionary RTS and colony simulation strategy game. Players guide a tribe of early hominids, shaping their physical traits, mental capacities, cultural practices, and lasting legacy across generations in a procedurally-influenced world.

## Game Overview

*Lineage: Ancestral Legacies* invites you to shepherd a tribe of early hominids from their first fragile steps towards survival to becoming a complex, evolving society. You are the guiding spirit of their lineage, making critical decisions that shape their physical traits, mental capacities, cultural practices, and lasting legacy across generations. Manage resources, navigate perilous environments, unlock genetic potential, witness emergent social dynamics, and forge unique ancestral stories in a vibrant, procedurally-influenced world. The game draws inspiration from titles like *RimWorld*, *Dwarf Fortress*, and *Crusader Kings*, focusing on deep simulation and emergent storytelling.

The game is set in the world of **Lumina**, a realm shaped by cosmic energies, ancient Elder Races (Sylvans, Lithids, Aeravine, Abyssal Kith), and a cataclysmic event known as "The Shattering." Players begin in "The Awakening" era, guiding one of the Younger Races as they navigate a world filled with remnants of lost civilizations and lingering magic. The lore encompasses a rich history, from the First Light and the Star-Souls (primordial deities) to the potential for player-driven prophecies to unfold.

### Key Features
*   **Deep Evolutionary Trait System:** Guide your tribe's genetic destiny through a modular tree (Physical, Mental, Emotional/Social traits), impacting abilities and appearance. Traits are inherited, can mutate, and allow for adaptation to diverse challenges.
*   **Emergent Generational Storytelling:** Each pop is a unique individual with inherited traits, developing personalities, relationships (kinship, friendships, rivalries), and life stories. Witness unscripted narratives unfold across detailed family trees.
*   **Procedural Language Evolution:** Tribes develop unique languages from phonetic palettes, influenced by their environment and discoveries. Decipher and influence linguistic development, impacting inter-tribal communication and cultural identity.
*   **Branching Tribal Legacies:** Guide your tribe to split and diverge, adapting uniquely to different biomes and challenges, creating a tapestry of interconnected (or rival) cultures with distinct traits, technologies, and traditions.
*   **Colony Simulation & Resource Management:** Manage fundamental needs (hunger, thirst, shelter, safety). Gather resources, craft tools, build structures, and defend against threats in a top-down RTS framework.
*   **Individual Pop Simulation:** Pops have attributes, needs, skills, a lifecycle, and operate on a Finite State Machine driving autonomous behavior (Idle, Foraging, Hunting, Crafting, Socializing, etc.).
*   **Dynamic Environment & Biomes:** Explore diverse biomes (Savannah, Forest, Jungle, Desert, Taiga, Coastal, etc.) with unique flora, fauna, and challenges. Experience dynamic day/night cycles, seasons, and weather events.
*   **Rich World Lore:** Uncover the history of Lumina, the Elder Races, the Star-Souls, and the impact of "The Shattering." Discover lost technologies and lingering magic that shape the world.

## Core Technologies
*   **Engine:** Unity (version specified in `ProjectSettings/ProjectVersion.txt`)
*   **Language:** C#
*   **Data Management:** ScriptableObjects for core game data (see `Documents/01_Design/Systems/Unified_GameData_System_Design.md`)

## Repository Layout

- **Assets/** – Game assets, C# scripts (organized into `LineageScripts` (Core), `LineageBehavior` (Gameplay Logic), `Assembly-CSharp` (UI/General), and editor-specific assemblies like `LineageScripts.Editor`), and game data (ScriptableObjects under `Assets/Data/`).
- **Documents/** – Design documents, research notes, and guides.
- **Packages/** – Package manifests for Unity.
- **ProjectSettings/** – Unity project configuration.
- **CONTRIBUTING.md** – Guidelines for contributing to the project.

### Documents Overview

The repository contains extensive planning material under `Documents/`:

- **01_Design** – Core game design documents (GDD - `Core/GDD.md`), system designs (e.g., `Systems/Unified_GameData_System_Design.md`), lore details, concept art, and feature specifications.
- **02_Tasks** – Pending work items, roadmaps, and optimization plans.
- **03_Research** – Reference material, articles, and inspiration used during development.
- **04_Guides** – How-to guides, setup instructions, and onboarding documentation for developers.
- **07_Legal** – Licensing notes and legal documentation (currently placeholder).
- **08_Git** – Version-control notes, branching strategies, and merge descriptions (currently placeholder).

See `Documents/README.md` (if it exists, otherwise this section implies its potential) for more details on the document structure.

## Getting Started

1.  **Clone the repository:** `git clone https://github.com/ScottyVenable/Lineage-Ancestral-Legacies.git` (Ensure this is the correct URL or update as needed).
2.  **Install Unity:** Ensure you have the Unity version specified in `ProjectSettings/ProjectVersion.txt` installed via Unity Hub.
3.  **Open the Project:** Add and open the cloned repository folder as a project in Unity Hub.
4.  **Review Guides:** Check `Documents/04_Guides` for any specific setup, configuration, or workflow tips.
5.  **Load Main Scene:** Open the primary development scene (e.g., `Assets/Scenes/MainGame.unity` - verify actual scene name and path).

## Development Roadmap (High-Level)

This project follows an iterative development approach. Key milestones are based on the detailed GDD (`Documents/01_Design/Core/GDD.md`):

*   **Milestone 1: Core Prototype (Completed)**
    *   Focus: Basic pop FSM, movement, selection, minimal UI, single resource type.
    *   Goal: Prove fundamental pop behavior and interaction in a simple environment.
*   **Milestone 2: Alpha - First Sparks of Adaptation (Current Stage - v0.1)**
    *   Focus: Expanded FSM (needs, hauling, basic crafting), inventory system, multiple resources, basic needs system, simple combat, initial Genetic Tree & EP, basic inheritance.
    *   Goal: Achieve a playable survival loop. Pops can die, reproduce, and initial genetic choices offer gameplay variation. This stage represents the "Dawn Age" and early "Primitive Age."
*   **Milestone 3: Beta - The Tribal Emergence**
    *   Focus: Full FSM & AI Roles, progression through "Primitive Age" and into "Tribal Age," multiple biomes, expanded Genetic Tree with visual traits, complex crafting & tech trees (DP system), basic social dynamics (relationships, mood,

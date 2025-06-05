# AI Prompt
Enhanced AI Image Generator Prompt: Graph-Based Studio Tool Concept Art
Core Request:
"Digital painting, concept art for a sophisticated graph-based editor UI, designed as a Studio Tool within a game development environment like Unity. This tool is for creating complex game systems such as AI Behaviors, Quest Lines, or Dialogue Trees for a game titled 'Lineages: Ancestral Legacies.' The visual style should feel integrated, clean, modern, professional, and intuitive."

Overall Layout & Window Title (Reference: Three-Pane Design):

The entire view should be a single software window.

Display a window title example like: "Lineages AI Behavior Editor - (BehaviorAssetName.asset)" or "Lineages Quest Designer - (QuestName.asset)".

1. Main Graph Canvas (Center, Largest Area):

Appearance: Visualize a large, open canvas area with a subtle grid background (e.g., light gray lines on a darker gray, or a "technical blueprint" aesthetic). The canvas should appear pannable and zoomable.

Content - Nodes:

Populate the canvas with several interconnected rectangular nodes.

Each node must have a clearly visible Title Bar (e.g., "Wander Action," "Is Enemy Nearby?," "Dialogue:Greeting").

Nodes should feature small, distinct Input Ports (circular or square connection points) on their left or top sides, and Output Ports on their right or bottom sides.

Show color-coding or distinct visual styles for different node types (e.g., Condition nodes could be green, Action nodes blue, Dialogue nodes yellow).

Optionally, include a small icon or brief text summary on some nodes.

Content - Edges (Connections):

Illustrate smooth Bezier curve lines connecting output ports of some nodes to input ports of others.

These lines should have clear directional arrows indicating the flow of logic or data.

Some edges could vary in color or thickness, perhaps to represent a "True" vs. "False" path from a conditional node.

Interaction Cues (Subtle):

Perhaps hint at a searchable context menu having been right-clicked on an empty canvas space (e.g., a faint menu outline with "Add Node..." visible).

Show one node clearly selected (e.g., highlighted border).

2. Inspector/Properties Panel (Right Side):

Appearance: A clearly defined, dockable panel on the right side, stylistically similar to Unity's Inspector.

Content: This panel should be populated with UI elements representing the properties of the currently selected node from the graph.

Show a variety of standard UI controls: clearly delineated text fields, numerical input fields (some perhaps with sliders), dropdown menus (enum selection), and object selection fields (for assigning assets like Sprites, AudioClips, or other ScriptableObjects).

For example, if a "Dialogue Line" node is notionally selected, the inspector might show fields labeled "Character," "Dialogue Text" (as a multi-line text area), "Voice Clip," "Facial Expression."

If an "AI Attack Action" node is selected, it might show "Attack Animation," "Damage," "Range."

The labels for these fields should be suggested but not necessarily perfectly legible; the focus is on the types of controls.

3. Asset/Toolbar Panel (Optional - Top or Slim Left Side):

Appearance: A smaller, more compact panel, possibly with tabs or subtle section dividers.

Content (Include a few of these clearly):

An ObjectField at the top showing the name of the currently loaded graph asset (e.g., "MyQuestGraph.asset").

Distinct "Save" and "Load" icon buttons.

Perhaps a "Validation" button (e.g., with a checkmark icon).

A Search Bar for finding nodes or filtering a node palette.

(Optional) A small, illustrative Node Palette area showing a few icons or names of available node types that could be dragged onto the canvas.

(Optional, if space allows and clarity is maintained) A very compact Mini-Map of the graph.

Overall Feel & Style Details:

Integrated with Game Engine Aesthetic: Should look like it belongs within a professional game engine editor (e.g., Unity's dark theme is a good reference, but light theme compatibility is also a plus if implied).

Clean, Readable, Uncluttered: The primary focus must be on the graph. UI elements should have good contrast, legible (even if stylized) font choices, and adequate spacing. Avoid visual noise.

Responsive & Informative Cues: Imply that the interface is interactive. Visual cues for selected elements, connection validity (e.g., a slightly glowing port when dragging an edge), and possibly tooltips (suggested by a faint question mark icon near a complex field).

Professional Digital Painting: High-quality rendering, good lighting that enhances UI clarity, attention to detail in UI elements.

Aspect Ratio:

Prefer a widescreen aspect ratio, like 16:9 or 16:10, to mimic a typical monitor display for software.

Negative Prompts (Things to AVOID):

blurry, messy, cluttered, confusing, pixelated, hand-drawn sketch, overly simplistic, childish, dark and unreadable, neon overload, too sci-fi, abstract, non-functional looking, disorganised
# Graph-Based Studio Tool: Window Layout & UI Concept

This document outlines the conceptual layout and user interface elements for an advanced graph-based Studio Tool within the Unity Editor. The tool is designed for creating AI Behaviors, Quest Lines, Dialogue Trees, or Loot Tables for **Lineages: Ancestral Legacies**.

---

## Window Title

- **Examples:**
    - `Lineages AI Behavior Editor - (BehaviorAssetName.asset)`
    - `Lineages Quest Designer - (QuestName.asset)`
    - *(Title adapts based on specialization and loaded asset)*

---

## Overall Layout: Three-Pane Design

A typical node editor layout, divided into:

### 1. Main Graph Canvas (Center, Largest Area)

- **Appearance:**  
    Large, open area with a subtle grid background (e.g., light gray lines on a darker gray or "blueprint" style).  
    - Pannable (click and drag)
    - Zoomable (mouse wheel)

- **Content:**  
    - **Nodes:**  
        - Rectangular or uniquely shaped blocks for each type.
        - **Title Bar:** Displays node type or user-given name (e.g., `Wander Action`, `Is Enemy Nearby?`, `Dialogue: Greeting`).
        - **Input Ports:** Small connection points (left/top) for data/execution flow in.
        - **Output Ports:** Connection points (right/bottom) for data/execution flow out.
        - **Icon/Summary:** (Optional) Small icon or summary of key settings.
        - **Color Coding:** Visual styles based on node type (e.g., Condition: green, Action: blue, Dialogue: yellow).
    - **Edges (Connections):**  
        - Smooth lines (Bezier curves) connecting output to input ports.
        - Arrows indicate flow direction.
        - Color/thickness may change based on state (e.g., "True" vs. "False" paths).

- **Interaction:**  
    - **Add Nodes:** Right-click canvas → searchable context menu (e.g., `Add Node → Action → Move To`).
    - **Select Nodes/Edges:** Click to select, Shift-click or drag to multi-select.
    - **Move Nodes:** Drag selected nodes.
    - **Create Edges:** Drag from one port to another.
    - **Delete:** Select and press `Delete` or use context menu.

---

### 2. Inspector/Properties Panel (Right Side)

- **Appearance:**  
    Dockable panel, similar to Unity's Inspector.

- **Content:**  
    - Shows editable properties for the selected node (or edge).
    - Uses a `GenericEditorUIDrawer` to reflect on the node's data object and auto-generate fields:
        - Strings, numbers, enums
        - Unity Object references (e.g., Sprites, AudioClips, ScriptableObjects)
    - **Examples:**
        - *Dialogue Line Node:*  
            - Character Speaking  
            - Dialogue Text (Text Area)  
            - Associated Voice Clip (Object Field)  
            - Facial Expression (Enum)
        - *AI Attack Action Node:*  
            - Attack Animation (Animation Clip)  
            - Damage Value (Float)  
            - Attack Range (Float)

- **Interaction:**  
    - Standard Unity UI controls (text fields, sliders, dropdowns, object pickers)
    - Immediate updates to data and node visuals
    - Undo support

---

### 3. Asset/Toolbar Panel (Top or Left Side, Optional)

- **Appearance:**  
    Smaller, possibly tabbed or collapsible panel.

- **Content:**  
    - **Current Asset Field:** Assign the `GraphAssetData` ScriptableObject (e.g., `MyEpicQuest.asset`)
    - **Buttons:**  
        - Create New
        - Load Existing
        - Save/Load
        - Validation (e.g., "Are all dialogue choices linked?")
    - **Mini-Map:** (Optional) Zoomed-out overview for large graphs.
    - **Node Palette:** (Alternative to context menu) Drag node types onto canvas.
    - **Search Bar:** Find/filter nodes or palette items.

- **Interaction:**  
    - Button clicks, drag-and-drop from palette

---

## Overall Feel & Style

- **Integrated:** Native Unity editor window, respects Unity UI conventions (dark/light theme, standard controls)
- **Clean & Readable:** Focus on the graph, uncluttered UI, good fonts and spacing
- **Responsive:** Smooth pan/zoom, immediate property editing
- **Informative:**  
    - Clear visual cues for node types and connection validity
    - Tooltips on hover for nodes/ports

---

> This kind of visual editor is essential for building deeply interconnected and dynamic systems, enabling powerful design and visualization of complex logic without getting lost in code or endless inspector lists.

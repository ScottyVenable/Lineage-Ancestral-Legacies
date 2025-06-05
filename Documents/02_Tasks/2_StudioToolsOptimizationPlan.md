# StudioTools: Finalization, Optimization & Automation Plan

This plan details the steps to complete and optimize your StudioTools suite for Unity, focusing on robustness, maintainability, and dynamic UI/tool integration that adapts to changes in `Database.cs`.

---

## Phase 1: Core Data Structure & Persistence Strategy

**Goal:** Establish solid data structures and efficient game data persistence.

- [ ] **Finalize `Database.cs` Data Types**  
    - Review all structs/classes in `Database.cs`.  
    - Convert large/complex data holders (e.g., `Entity`, `Item`, `Quest`, `NPC`) to classes.  
    - Use structs only for small, value-like types (e.g., `Health`, `StatModifiers`, `EntitySize`).  
    - *Why:* Ensures performance, correct state management, and ScriptableObject compatibility.

- [ ] **Implement Namespace Consistency**  
    - Ensure all scripts use logical, consistent namespaces (e.g.,  
        - `Lineage.Ancestral.Legacies.Data` for `Database.cs`  
        - `Lineage.Ancestral.Legacies.Editor.Tools` for editor tools)  
    - *Why:* Prevents naming conflicts and improves organization.

- [ ] **Choose and Implement Data Persistence Strategy**  
    - **Recommended:**  
        - For each core data class (e.g., `Entity`, `Item`, `Skill`), create a ScriptableObject wrapper (e.g., `EntitySO : ScriptableObject { public Entity data; }`).  
        - Add `[CreateAssetMenu]` attributes.  
        - Update `GameData.cs` (or a manager) to load assets from project folders (e.g., `Resources.LoadAll<EntitySO>("Data/Entities")`).  
        - *Why:* Unity-native data management, version control, Inspector-friendly workflow.
    - **Alternative:**  
        - Ensure robust JSON/XML serialization for static lists in `GameData.cs`.

---

## Phase 2: Generic Tool Infrastructure, Reflection-Based UI & Tool Simplification

**Goal:** Enable dynamic, maintainable editor tools with minimal manual UI coding.

- [ ] **Create `BaseStudioEditorWindow : EditorWindow`**  
    - Develop a base class for tool windows.  
    - Include shared styles, helpers, error display, Undo/Redo setup.  
    - *Why:* Reduces duplication, centralizes logic.

- [ ] **Design Tools for Focused Responsibilities**  
    - Review tool windows (e.g., `EntityCreatorWindow`, `DatabaseEditorWindow`).  
    - Split large tools into smaller, focused editors if needed.  
    - *Why:* Simpler tools are easier to develop, debug, and maintain.

- [ ] **Develop Reflection-Based UI Drawer (`GenericEditorUIDrawer.cs`)**  
    - Static methods use reflection to draw fields/properties dynamically.  
    - Support for collections (`List<T>`, arrays), with add/remove for simple types.  
    - Handle complex lists and dictionaries as feasible.  
    - *Why:* New fields in data classes appear automatically in editors.

- [ ] **Implement Custom Attribute Support**  
    - Define attributes (e.g., `[InspectorReadOnly]`, `[InspectorTooltip]`, `[InspectorHeader]`, `[InspectorTextArea]`, `[InspectorHide]`).  
    - *Why:* Fine-grained UI control without hardcoding layout.

---

## Phase 3: Implement/Refactor Individual Studio Tools

**Goal:** Update tools to use new persistence and reflection-based UI.

For each tool:

- [ ] Inherit from `BaseStudioEditorWindow`
- [ ] Integrate ScriptableObject persistence
- [ ] Use `GenericEditorUIDrawer` for core UI
- [ ] Add tool-specific UI/logic as needed
- [ ] Implement robust error handling & feedback
- [ ] Integrate `Undo.RecordObject()`
- [ ] Test thoroughly

---

## Phase 4: Menu Integration, Global Utilities & New Data Class Handling

**Goal:** Streamline tool access, data management, and support for new data types.

- [ ] **Dynamic Menu Item Generation (`StudioMenuItems.cs`)**  
    - Use reflection to find all `EditorWindow` (or base) classes with `[StudioToolMenuItem]` attribute.  
    - Build "Tools/Lineages/..." menu structure dynamically.  
    - *Why:* New tools appear in the menu automatically.

- [ ] **(Optional) Generic Data Editor Tool**  
    - Create `GenericDataEditorWindow.cs` with an `ObjectField` for any ScriptableObject.  
    - Use `GenericEditorUIDrawer` to edit fields.  
    - *Why:* Instantly edit new data types without custom tools.

- [ ] **Enhance `DataValidatorWindow.cs`**  
    - Scan ScriptableObject folders, add validation rules, improve error navigation.

- [ ] **Refine `BulkDataToolWindow.cs`**  
    - Improve selection/filtering, ensure robust Undo for bulk operations.

---

## Phase 5: Testing, Refinement & Documentation

**Goal:** Ensure system stability, usability, and clarity.

- [ ] **Comprehensive System-Wide Testing**  
    - Test all tools and workflows together.  
    - Add a new field to a data class—verify editors update automatically.  
    - Create a new ScriptableObject data class—edit with Generic Data Editor.  
    - Add a new tool window—verify menu integration.

- [ ] **Test Tool Performance with Large Datasets**  
    - Especially for tools listing many ScriptableObjects.  
    - Optimize as needed (pagination, efficient asset loading, reflection).

- [ ] Usability review

- [ ] Write basic documentation

---

By focusing on smart infrastructure (reflection drawer, base window, dynamic menus, generic editor), you minimize boilerplate and manual updates. This framework makes StudioTools scalable and maintainable, letting you and AI focus on unique tool logic rather than repetitive UI/menu code.

---

## References


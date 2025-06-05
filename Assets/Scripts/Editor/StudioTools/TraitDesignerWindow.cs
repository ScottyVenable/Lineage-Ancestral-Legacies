using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Comprehensive trait creation and editing tool for the Lineage game.
    /// Provides visual design of trait properties, modifiers, requirements, and inheritance patterns.
    /// </summary>
    public class TraitDesignerWindow : EditorWindow
    {
        #region Window State
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private readonly string[] tabs = { "Basic Info", "Modifiers", "Requirements", "Inheritance", "Preview" };
        #endregion

        #region Trait Data
        private Trait currentTrait;
        private bool isEditingExisting = false;
        private Trait.ID editingTraitID;
        
        // Basic Info
        private string traitName = "";
        private string description = "";
        private string category = "";
        private List<string> tags = new List<string>();
        private string newTag = "";
        
        // Modifiers
        private StatModifiers modifiers = new StatModifiers();
        private bool showStatModifiers = true;
        
        // Requirements
        private List<Trait.ID> requiredTraits = new List<Trait.ID>();
        private Skill.ID requiredSkill = Skill.ID.Combat;
        private List<Item.ID> requiredItems = new List<Item.ID>();
        private Stat requiredStat = new Stat(Stat.ID.Strength, "Default");
        private bool hasSkillRequirement = false;
        private bool hasStatRequirement = false;
        
        // Inheritance & Genetics
        private bool isInheritable = false;
        private float inheritanceChance = 0.5f;
        private bool isDominant = false;
        private List<Trait.ID> conflictingTraits = new List<Trait.ID>();
        private List<Trait.ID> synergyTraits = new List<Trait.ID>();
        
        // Validation
        private List<string> validationErrors = new List<string>();
        #endregion

        #region GUI Styles
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle errorStyle;
        private bool stylesInitialized = false;
        #endregion

        [MenuItem("Lineage Studio/Content Creation/Trait Designer", priority = 22)]
        public static void ShowWindow()
        {
            TraitDesignerWindow window = GetWindow<TraitDesignerWindow>("Trait Designer");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnEnable()
        {
            InitializeNewTrait();
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };

            errorStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { textColor = Color.red }
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitializeStyles();

            EditorGUILayout.BeginVertical();
            {
                DrawHeader();
                DrawToolbar();
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    DrawTabContent();
                }
                EditorGUILayout.EndScrollView();
                
                DrawFooter();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal("toolbar");
            {
                GUILayout.Label("Trait Designer", headerStyle);
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Load Existing", EditorStyles.toolbarButton))
                {
                    ShowTraitSelectionMenu();
                }
                
                if (GUILayout.Button("New Trait", EditorStyles.toolbarButton))
                {
                    InitializeNewTrait();
                }
                
                GUI.enabled = !string.IsNullOrEmpty(traitName);
                if (GUILayout.Button(isEditingExisting ? "Update" : "Create", EditorStyles.toolbarButton))
                {
                    CreateOrUpdateTrait();
                }
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);
        }

        private void DrawTabContent()
        {
            switch (selectedTab)
            {
                case 0: DrawBasicInfoTab(); break;
                case 1: DrawModifiersTab(); break;
                case 2: DrawRequirementsTab(); break;
                case 3: DrawInheritanceTab(); break;
                case 4: DrawPreviewTab(); break;
            }
        }

        private void DrawBasicInfoTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Basic Information", headerStyle);
                
                traitName = EditorGUILayout.TextField("Trait Name", traitName);
                
                EditorGUILayout.LabelField("Description");
                description = EditorGUILayout.TextArea(description, GUILayout.Height(60));
                
                category = EditorGUILayout.TextField("Category", category);
                
                DrawCategoryPresets();
                
                EditorGUILayout.Space();
                DrawTagsSection();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawCategoryPresets()
        {
            EditorGUILayout.LabelField("Category Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Combat")) category = "Combat";
                if (GUILayout.Button("Social")) category = "Social";
                if (GUILayout.Button("Crafting")) category = "Crafting";
                if (GUILayout.Button("Mental")) category = "Mental";
                if (GUILayout.Button("Physical")) category = "Physical";
                if (GUILayout.Button("Magical")) category = "Magical";
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTagsSection()
        {
            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
            
            // Add new tag
            EditorGUILayout.BeginHorizontal();
            {
                newTag = EditorGUILayout.TextField("New Tag", newTag);
                if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newTag))
                {
                    if (!tags.Contains(newTag))
                    {
                        tags.Add(newTag);
                        newTag = "";
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Display existing tags
            for (int i = 0; i < tags.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(tags[i]);
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        tags.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            // Tag presets
            EditorGUILayout.LabelField("Tag Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Positive")) AddTagIfNotExists("Positive");
                if (GUILayout.Button("Negative")) AddTagIfNotExists("Negative");
                if (GUILayout.Button("Rare")) AddTagIfNotExists("Rare");
                if (GUILayout.Button("Common")) AddTagIfNotExists("Common");
                if (GUILayout.Button("Inherited")) AddTagIfNotExists("Inherited");
                if (GUILayout.Button("Acquired")) AddTagIfNotExists("Acquired");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawModifiersTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Stat Modifiers", headerStyle);
                
                showStatModifiers = EditorGUILayout.Foldout(showStatModifiers, "Stat Modifiers");
                if (showStatModifiers)
                {
                    DrawStatModifiersSection();
                }
                
                EditorGUILayout.Space();
                DrawModifierPresets();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawStatModifiersSection()
        {
            EditorGUI.indentLevel++;
            
            // Core stats
            EditorGUILayout.LabelField("Core Stats", EditorStyles.boldLabel);
            modifiers.strength = EditorGUILayout.FloatField("Strength", modifiers.strength);
            modifiers.defense = EditorGUILayout.FloatField("Defense", modifiers.defense);
            modifiers.speed = EditorGUILayout.FloatField("Speed", modifiers.speed);
            modifiers.intelligence = EditorGUILayout.FloatField("Intelligence", modifiers.intelligence);
            modifiers.charisma = EditorGUILayout.FloatField("Charisma", modifiers.charisma);
            modifiers.luck = EditorGUILayout.FloatField("Luck", modifiers.luck);
            
            EditorGUILayout.Space();
            
            // Secondary stats
            EditorGUILayout.LabelField("Secondary Stats", EditorStyles.boldLabel);
            modifiers.health = EditorGUILayout.FloatField("Health", modifiers.health);
            modifiers.mana = EditorGUILayout.FloatField("Mana", modifiers.mana);
            modifiers.stamina = EditorGUILayout.FloatField("Stamina", modifiers.stamina);
            modifiers.criticalChance = EditorGUILayout.Slider("Critical Chance", modifiers.criticalChance, -1f, 1f);
            modifiers.experienceMultiplier = EditorGUILayout.Slider("Experience Multiplier", modifiers.experienceMultiplier, 0.1f, 3f);
            
            EditorGUI.indentLevel--;
        }

        private void DrawModifierPresets()
        {
            EditorGUILayout.LabelField("Modifier Presets", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Combat Specialist"))
                {
                    modifiers.strength += 5;
                    modifiers.defense += 3;
                    modifiers.criticalChance += 0.1f;
                }
                if (GUILayout.Button("Scholar"))
                {
                    modifiers.intelligence += 8;
                    modifiers.mana += 20;
                    modifiers.experienceMultiplier += 0.2f;
                }
                if (GUILayout.Button("Diplomat"))
                {
                    modifiers.charisma += 6;
                    modifiers.intelligence += 2;
                    modifiers.luck += 1;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Athlete"))
                {
                    modifiers.speed += 4;
                    modifiers.stamina += 15;
                    modifiers.health += 10;
                }
                if (GUILayout.Button("Lucky"))
                {
                    modifiers.luck += 5;
                    modifiers.criticalChance += 0.05f;
                }
                if (GUILayout.Button("Reset All"))
                {
                    modifiers = new StatModifiers();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRequirementsTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Requirements", headerStyle);
                
                // Required Traits
                EditorGUILayout.LabelField("Required Traits", EditorStyles.boldLabel);
                DrawTraitsList(requiredTraits, "No required traits");
                DrawTraitSelectionButtons(requiredTraits, "Add Required Trait");
                
                EditorGUILayout.Space();
                
                // Required Skill
                hasSkillRequirement = EditorGUILayout.Toggle("Has Skill Requirement", hasSkillRequirement);
                if (hasSkillRequirement)
                {
                    requiredSkill = (Skill.ID)EditorGUILayout.EnumPopup("Required Skill", requiredSkill);
                }
                
                EditorGUILayout.Space();
                
                // Required Stat
                hasStatRequirement = EditorGUILayout.Toggle("Has Stat Requirement", hasStatRequirement);
                if (hasStatRequirement)
                {
                    DrawStatRequirement();
                }
                
                EditorGUILayout.Space();
                
                // Required Items
                EditorGUILayout.LabelField("Required Items", EditorStyles.boldLabel);
                DrawItemsList();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawInheritanceTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Inheritance & Genetics", headerStyle);
                
                isInheritable = EditorGUILayout.Toggle("Is Inheritable", isInheritable);
                
                if (isInheritable)
                {
                    inheritanceChance = EditorGUILayout.Slider("Inheritance Chance", inheritanceChance, 0f, 1f);
                    isDominant = EditorGUILayout.Toggle("Is Dominant Gene", isDominant);
                    
                    EditorGUILayout.Space();
                    
                    // Conflicting traits
                    EditorGUILayout.LabelField("Conflicting Traits", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("Traits that cannot coexist with this one", MessageType.Info);
                    DrawTraitsList(conflictingTraits, "No conflicting traits");
                    DrawTraitSelectionButtons(conflictingTraits, "Add Conflicting Trait");
                    
                    EditorGUILayout.Space();
                    
                    // Synergy traits
                    EditorGUILayout.LabelField("Synergy Traits", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("Traits that provide bonuses when combined with this one", MessageType.Info);
                    DrawTraitsList(synergyTraits, "No synergy traits");
                    DrawTraitSelectionButtons(synergyTraits, "Add Synergy Trait");
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Trait Preview", headerStyle);
                
                ValidateTrait();
                
                if (validationErrors.Count > 0)
                {
                    EditorGUILayout.LabelField("Validation Errors:", EditorStyles.boldLabel);
                    foreach (var error in validationErrors)
                    {
                        EditorGUILayout.HelpBox(error, MessageType.Error);
                    }
                    EditorGUILayout.Space();
                }
                
                DrawTraitPreview();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTraitPreview()
        {
            EditorGUILayout.LabelField("Generated Trait Data", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField($"Name: {traitName}");
                EditorGUILayout.LabelField($"Category: {category}");
                EditorGUILayout.LabelField("Description:");
                EditorGUILayout.TextArea(description, EditorStyles.wordWrappedLabel);
                
                if (tags.Count > 0)
                {
                    EditorGUILayout.LabelField($"Tags: {string.Join(", ", tags)}");
                }
                
                // Show modifiers
                if (HasAnyModifiers())
                {
                    EditorGUILayout.LabelField("Stat Modifiers:", EditorStyles.boldLabel);
                    DrawModifierPreview();
                }
                
                // Show requirements
                if (requiredTraits.Count > 0 || hasSkillRequirement || hasStatRequirement || requiredItems.Count > 0)
                {
                    EditorGUILayout.LabelField("Requirements:", EditorStyles.boldLabel);
                    DrawRequirementsPreview();
                }
                
                // Show inheritance info
                if (isInheritable)
                {
                    EditorGUILayout.LabelField("Inheritance:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"  Chance: {inheritanceChance:P0}");
                    EditorGUILayout.LabelField($"  Dominance: {(isDominant ? "Dominant" : "Recessive")}");
                }
            }
            EditorGUILayout.EndVertical();
        }

        #region Helper Methods
        private void InitializeNewTrait()
        {
            isEditingExisting = false;
            traitName = "";
            description = "";
            category = "";
            tags.Clear();
            modifiers = new StatModifiers();
            requiredTraits.Clear();
            requiredSkill = Skill.ID.Combat;
            requiredItems.Clear();
            requiredStat = new Stat(Stat.ID.Strength, "Default");
            hasSkillRequirement = false;
            hasStatRequirement = false;
            isInheritable = false;
            inheritanceChance = 0.5f;
            isDominant = false;
            conflictingTraits.Clear();
            synergyTraits.Clear();
            selectedTab = 0;
        }

        private void ShowTraitSelectionMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            var traitIDs = System.Enum.GetValues(typeof(Trait.ID)).Cast<Trait.ID>();
            foreach (var traitID in traitIDs)
            {
                var trait = GameData.GetTraitByID(traitID);
                menu.AddItem(new GUIContent($"{trait.category}/{trait.traitName}"), false, () => LoadExistingTrait(traitID));
            }
            
            menu.ShowAsContext();
        }

        private void LoadExistingTrait(Trait.ID traitID)
        {
            var trait = GameData.GetTraitByID(traitID);
            
            isEditingExisting = true;
            editingTraitID = traitID;
            traitName = trait.traitName;
            description = trait.description;
            category = trait.category;
            tags = new List<string>(trait.tags);
            modifiers = trait.modifiers;
            requiredTraits = new List<Trait.ID>(trait.requiredTraits.Select(t => t.traitID));
            requiredSkill = trait.requiredSkill;
            requiredItems = new List<Item.ID>(trait.requiredItems);
            requiredStat = trait.requiredStat;
            hasSkillRequirement = requiredSkill != Skill.ID.Combat;
            hasStatRequirement = requiredStat.statID != Stat.ID.Strength;
            
            // Note: Inheritance data would need to be stored in extended trait structure
            isInheritable = tags.Contains("Inherited");
            inheritanceChance = 0.5f; // Default
            isDominant = false; // Default
            
            selectedTab = 0;
        }

        private void CreateOrUpdateTrait()
        {
            if (!ValidateTraitForCreation())
            {
                EditorUtility.DisplayDialog("Validation Error", "Please fix validation errors before creating the trait.", "OK");
                return;
            }

            // In a real implementation, this would save to the database
            var newTrait = ConstructTrait();
            
            if (isEditingExisting)
            {
                Debug.Log($"Updated trait: {newTrait.traitName}");
                EditorUtility.DisplayDialog("Success", $"Trait '{newTrait.traitName}' has been updated!", "OK");
            }
            else
            {
                Debug.Log($"Created new trait: {newTrait.traitName}");
                EditorUtility.DisplayDialog("Success", $"Trait '{newTrait.traitName}' has been created!", "OK");
            }
        }

        private Trait ConstructTrait()
        {
            // Get next available ID (in real implementation, this would be managed by the database)
            var nextID = isEditingExisting ? editingTraitID : (Trait.ID)42; // Placeholder
            
            var trait = new Trait(nextID, traitName, description, category);
            
            // Apply modifiers, requirements, etc.
            // Note: This would require the Trait struct to have setters or a more complex constructor
            
            return trait;
        }

        private void AddTagIfNotExists(string tag)
        {
            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        private void DrawTraitsList(List<Trait.ID> traitList, string emptyMessage)
        {
            if (traitList.Count == 0)
            {
                EditorGUILayout.LabelField(emptyMessage, EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            for (int i = 0; i < traitList.Count; i++)
            {
                var trait = GameData.GetTraitByID(traitList[i]);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(trait.traitName);
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        traitList.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawTraitSelectionButtons(List<Trait.ID> targetList, string buttonText)
        {
            if (GUILayout.Button(buttonText))
            {
                ShowTraitSelectionMenu(targetList);
            }
        }

        private void ShowTraitSelectionMenu(List<Trait.ID> targetList)
        {
            GenericMenu menu = new GenericMenu();
            
            var traitIDs = System.Enum.GetValues(typeof(Trait.ID)).Cast<Trait.ID>();
            foreach (var traitID in traitIDs)
            {
                if (!targetList.Contains(traitID))
                {
                    var trait = GameData.GetTraitByID(traitID);
                    menu.AddItem(new GUIContent($"{trait.category}/{trait.traitName}"), false, () => targetList.Add(traitID));
                }
            }
            
            menu.ShowAsContext();
        }

        private void DrawStatRequirement()
        {
            var statID = (Stat.ID)EditorGUILayout.EnumPopup("Required Stat", requiredStat.statID);
            var minValue = EditorGUILayout.FloatField("Minimum Value", float.Parse(requiredStat.value));
            requiredStat = new Stat(statID, minValue.ToString());
        }

        private void DrawItemsList()
        {
            if (requiredItems.Count == 0)
            {
                EditorGUILayout.LabelField("No required items", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                for (int i = 0; i < requiredItems.Count; i++)
                {
                    var item = GameData.GetItemByID(requiredItems[i]);
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(item.itemName);
                        if (GUILayout.Button("×", GUILayout.Width(20)))
                        {
                            requiredItems.RemoveAt(i);
                            i--;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            if (GUILayout.Button("Add Required Item"))
            {
                ShowItemSelectionMenu();
            }
        }

        private void ShowItemSelectionMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            var itemIDs = System.Enum.GetValues(typeof(Item.ID)).Cast<Item.ID>();
            foreach (var itemID in itemIDs)
            {
                if (!requiredItems.Contains(itemID))
                {
                    var item = GameData.GetItemByID(itemID);
                    menu.AddItem(new GUIContent($"{item.itemType}/{item.itemName}"), false, () => requiredItems.Add(itemID));
                }
            }
            
            menu.ShowAsContext();
        }

        private void ValidateTrait()
        {
            validationErrors.Clear();
            
            if (string.IsNullOrEmpty(traitName))
                validationErrors.Add("Trait name is required");
            
            if (string.IsNullOrEmpty(description))
                validationErrors.Add("Description is required");
            
            if (string.IsNullOrEmpty(category))
                validationErrors.Add("Category is required");
        }

        private bool ValidateTraitForCreation()
        {
            ValidateTrait();
            return validationErrors.Count == 0;
        }

        private bool HasAnyModifiers()
        {
            return modifiers.strength != 0 || modifiers.defense != 0 || modifiers.speed != 0 ||
                   modifiers.intelligence != 0 || modifiers.charisma != 0 || modifiers.luck != 0 ||
                   modifiers.health != 0 || modifiers.mana != 0 || modifiers.stamina != 0 ||
                   modifiers.criticalChance != 0 || modifiers.experienceMultiplier != 1;
        }

        private void DrawModifierPreview()
        {
            EditorGUI.indentLevel++;
            if (modifiers.strength != 0) EditorGUILayout.LabelField($"Strength: {modifiers.strength:+0;-0}");
            if (modifiers.defense != 0) EditorGUILayout.LabelField($"Defense: {modifiers.defense:+0;-0}");
            if (modifiers.speed != 0) EditorGUILayout.LabelField($"Speed: {modifiers.speed:+0;-0}");
            if (modifiers.intelligence != 0) EditorGUILayout.LabelField($"Intelligence: {modifiers.intelligence:+0;-0}");
            if (modifiers.charisma != 0) EditorGUILayout.LabelField($"Charisma: {modifiers.charisma:+0;-0}");
            if (modifiers.luck != 0) EditorGUILayout.LabelField($"Luck: {modifiers.luck:+0;-0}");
            if (modifiers.health != 0) EditorGUILayout.LabelField($"Health: {modifiers.health:+0;-0}");
            if (modifiers.mana != 0) EditorGUILayout.LabelField($"Mana: {modifiers.mana:+0;-0}");
            if (modifiers.stamina != 0) EditorGUILayout.LabelField($"Stamina: {modifiers.stamina:+0;-0}");
            if (modifiers.criticalChance != 0) EditorGUILayout.LabelField($"Critical Chance: {modifiers.criticalChance:+0%;-0%}");
            if (modifiers.experienceMultiplier != 1) EditorGUILayout.LabelField($"Experience Multiplier: ×{modifiers.experienceMultiplier:F1}");
            EditorGUI.indentLevel--;
        }

        private void DrawRequirementsPreview()
        {
            EditorGUI.indentLevel++;
            if (requiredTraits.Count > 0)
            {
                var traitNames = requiredTraits.Select(id => GameData.GetTraitByID(id).traitName);
                EditorGUILayout.LabelField($"Required Traits: {string.Join(", ", traitNames)}");
            }
            if (hasSkillRequirement)
                EditorGUILayout.LabelField($"Required Skill: {requiredSkill}");
            if (hasStatRequirement)
                EditorGUILayout.LabelField($"Required Stat: {requiredStat.statID} ≥ {requiredStat.value}");
            if (requiredItems.Count > 0)
            {
                var itemNames = requiredItems.Select(id => GameData.GetItemByID(id).itemName);
                EditorGUILayout.LabelField($"Required Items: {string.Join(", ", itemNames)}");
            }
            EditorGUI.indentLevel--;
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal("toolbar");
            {
                GUILayout.Label($"Current Tab: {tabs[selectedTab]}");
                GUILayout.FlexibleSpace();
                
                if (validationErrors.Count > 0)
                {
                    GUILayout.Label($"⚠ {validationErrors.Count} error(s)", errorStyle);
                }
                else
                {
                    GUILayout.Label("✓ Ready", EditorStyles.miniLabel);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}

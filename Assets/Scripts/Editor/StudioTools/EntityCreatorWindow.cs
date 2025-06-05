// filepath: c:\Users\scott\OneDrive\Desktop\Lineage\Lineage Ancestral Legacies (Unity)\Assets\Scripts\Editor\StudioTools\EntityCreatorWindow.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Database;
using static Lineage.Ancestral.Legacies.Database.Entity;
using DatabaseEntity = Lineage.Ancestral.Legacies.Database.Entity;
using ComponentEntity = Lineage.Ancestral.Legacies.Entities.Entity;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Advanced entity creation tool that integrates with the database system.
    /// Provides visual creation, stat balancing, trait assignment, and prefab generation.
    /// </summary>
    public class EntityCreatorWindow : EditorWindow
    {
        private static EntityCreatorWindow window;

        [Header("Entity Configuration")]
        private string entityName = "New Entity";
        private ID entityID = ID.Pop;
        private List<Lineage.Ancestral.Legacies.Entities.EntityType> entityTypes = new List<Lineage.Ancestral.Legacies.Entities.EntityType>();
        private EntitySize entitySize = EntitySize.Medium;
        private AggressionType aggressionType = AggressionType.Neutral;

        [Header("Base Stats")]
        private Health healthStat = new Health(100f, 100f);
        private Stat manaStat = new Stat(Stat.ID.Mana, "Mana", 50f, 0f, 100f);
        private Stat attackStat = new Stat(Stat.ID.Attack, "Attack", 10f, 0f, 50f);
        private Stat defenseStat = new Stat(Stat.ID.Defense, "Defense", 10f, 0f, 50f);
        private Stat speedStat = new Stat(Stat.ID.Speed, "Speed", 8f, 0f, 20f);
        private Stat thirstStat = new Stat(Stat.ID.Thirst, "Thirst", 100f, 0f, 100f);
        private Stat hungerStat = new Stat(Stat.ID.Hunger, "Hunger", 100f, 0f, 100f);
        private Stat energyStat = new Stat(Stat.ID.Energy, "Energy", 100f, 0f, 100f);

        [Header("Advanced Stats")]
        private Stat strengthStat = new Stat(Stat.ID.Strength, "Strength", 10f, 0f, 30f);
        private Stat agilityStat = new Stat(Stat.ID.Agility, "Agility", 10f, 0f, 30f);
        private Stat intelligenceStat = new Stat(Stat.ID.Intelligence, "Intelligence", 10f, 0f, 30f);
        private Stat charismaStat = new Stat(Stat.ID.Charisma, "Charisma", 10f, 0f, 30f);
        private Stat luckStat = new Stat(Stat.ID.Luck, "Luck", 5f, 0f, 20f);

        [Header("Traits & Tags")]
        private List<Trait.ID> selectedTraits = new List<Trait.ID>();
        private List<string> entityTags = new List<string>();
        private List<string> availableTraits = new List<string>();

        [Header("Entity Type Configuration")]
        private bool showEntityTypeSettings = true;
        private bool canCraft = false;
        private bool canSocialize = false;
        private bool hasNeedsDecay = true;
        private bool canReproduce = false;
        private bool canAge = true;

        [Header("Prefab Generation")]
        private bool generatePrefab = true;
        private bool generateEntityTypeData = true;
        private string prefabSavePath = "Assets/Prefabs/Entities/";
        private string entityTypeDataPath = "Assets/Data/EntityTypes/";

        [Header("UI State")]
        private Vector2 scrollPosition = Vector2.zero;
        private int selectedTab = 0;
        private string[] tabNames = { "Basic Info", "Stats", "Traits", "Advanced", "Generation" };
        private bool useStatPresets = false;
        private string selectedStatPreset = "Balanced";
        private string[] statPresets = { "Balanced", "Tank", "DPS", "Support", "Fast", "Magic User" };

        [Header("Preview")]
        private bool showPreview = true;
        private float totalStatPoints = 0f;
        private float recommendedStatPoints = 100f;
        private Color statBalanceColor = Color.green;

        public static void ShowWindow()
        {
            window = GetWindow<EntityCreatorWindow>("Entity Creator");
            window.minSize = new Vector2(900, 700);
            window.titleContent = new GUIContent("Entity Creator", "Advanced Entity Creation Tool");
        }

        private void OnEnable()
        {
            RefreshAvailableTraits();
            CalculateStatPoints();
        }

        private void OnGUI()
        {
            DrawHeader();
            
            EditorGUILayout.BeginHorizontal();
            
            // Main content area
            EditorGUILayout.BeginVertical(GUILayout.Width(600));
            
            DrawTabSelection();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawSelectedTab();
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();

            // Preview panel
            if (showPreview)
            {
                GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));
                DrawPreviewPanel();
            }
            
            EditorGUILayout.EndHorizontal();

            DrawFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Lineage Entity Creator", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Creating: {entityName}", EditorStyles.label);
            
            GUILayout.FlexibleSpace();
            
            showPreview = EditorGUILayout.Toggle("Preview", showPreview, GUILayout.Width(70));
            
            if (GUILayout.Button("Load Template", GUILayout.Width(100)))
            {
                LoadEntityTemplate();
            }
            
            if (GUILayout.Button("Save Template", GUILayout.Width(100)))
            {
                SaveEntityTemplate();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawTabSelection()
        {
            EditorGUILayout.BeginHorizontal();
            
            for (int i = 0; i < tabNames.Length; i++)
            {
                bool isSelected = (selectedTab == i);
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                
                if (GUILayout.Button(tabNames[i], GUILayout.Height(30)))
                {
                    selectedTab = i;
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
        }

        private void DrawSelectedTab()
        {
            switch (selectedTab)
            {
                case 0: DrawBasicInfoTab(); break;
                case 1: DrawStatsTab(); break;
                case 2: DrawTraitsTab(); break;
                case 3: DrawAdvancedTab(); break;
                case 4: DrawGenerationTab(); break;
            }
        }

        private void DrawBasicInfoTab()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Basic Entity Information", EditorStyles.boldLabel);
            
            GUILayout.Label("Entity Name:");
            entityName = EditorGUILayout.TextField(entityName);
            
            GUILayout.Label("Entity ID:");
            entityID = (ID)EditorGUILayout.EnumPopup(entityID);
            
            GUILayout.Label("Entity Size:");
            entitySize = (EntitySize)EditorGUILayout.EnumPopup("Size", entitySize);
            
            GUILayout.Label("Aggression Type:");
            aggressionType = (AggressionType)EditorGUILayout.EnumPopup(aggressionType);
            
            GUILayout.Space(10);
            
            GUILayout.Label("Entity Types:", EditorStyles.boldLabel);
            DrawEntityTypesSelection();
            
            GUILayout.Space(10);
            
            GUILayout.Label("Basic Tags:", EditorStyles.boldLabel);
            DrawBasicTagsEditor();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawStatsTab()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Entity Statistics", EditorStyles.boldLabel);
            
            // Stat preset selection
            EditorGUILayout.BeginHorizontal();
            
            useStatPresets = EditorGUILayout.Toggle("Use Preset:", useStatPresets, GUILayout.Width(80));
            
            if (useStatPresets)
            {
                int newPreset = EditorGUILayout.Popup(
                    System.Array.IndexOf(statPresets, selectedStatPreset), 
                    statPresets);
                if (newPreset >= 0 && newPreset < statPresets.Length)
                {
                    string newPresetName = statPresets[newPreset];
                    if (newPresetName != selectedStatPreset)
                    {
                        selectedStatPreset = newPresetName;
                        ApplyStatPreset(selectedStatPreset);
                    }
                }
            }
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Balance Stats", GUILayout.Width(100)))
            {
                BalanceAllStats();
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Core Stats
            GUILayout.Label("Core Statistics:", EditorStyles.boldLabel);
            healthStat = DrawHealthEditor("Health", healthStat, 50f, 200f);
            manaStat = DrawStatEditor("Mana", manaStat, 0f, 200f);
            attackStat = DrawStatEditor("Attack", attackStat, 1f, 50f);
            defenseStat = DrawStatEditor("Defense", defenseStat, 1f, 50f);
            speedStat = DrawStatEditor("Speed", speedStat, 1f, 30f);
            
            GUILayout.Space(10);
            
            // Needs Stats
            GUILayout.Label("Needs Statistics:", EditorStyles.boldLabel);
            thirstStat = DrawStatEditor("Thirst", thirstStat, 0f, 100f);
            hungerStat = DrawStatEditor("Hunger", hungerStat, 0f, 100f);
            energyStat = DrawStatEditor("Energy", energyStat, 0f, 100f);
            
            GUILayout.Space(10);
            
            // Ability Stats
            GUILayout.Label("Ability Statistics:", EditorStyles.boldLabel);
            strengthStat = DrawStatEditor("Strength", strengthStat, 1f, 30f);
            agilityStat = DrawStatEditor("Agility", agilityStat, 1f, 30f);
            intelligenceStat = DrawStatEditor("Intelligence", intelligenceStat, 1f, 30f);
            charismaStat = DrawStatEditor("Charisma", charismaStat, 1f, 30f);
            luckStat = DrawStatEditor("Luck", luckStat, 1f, 20f);
            
            CalculateStatPoints();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawTraitsTab()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Traits & Characteristics", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Refresh Traits", GUILayout.Width(100)))
            {
                RefreshAvailableTraits();
            }
            
            if (GUILayout.Button("Random Traits", GUILayout.Width(100)))
            {
                AssignRandomTraits();
            }
            
            GUILayout.FlexibleSpace();
            
            GUILayout.Label("Selected: {selectedTraits.Count}");
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Available traits
            GUILayout.Label("Available Traits:", EditorStyles.boldLabel);
            DrawTraitSelection();
            
            GUILayout.Space(10);
            
            // Selected traits
            GUILayout.Label("Selected Traits:", EditorStyles.boldLabel);
            DrawSelectedTraits();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedTab()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Advanced Configuration", EditorStyles.boldLabel);
            
            showEntityTypeSettings = EditorGUILayout.Foldout(showEntityTypeSettings, "Entity Type Settings");
            if (showEntityTypeSettings)
            {
                EditorGUI.indentLevel++;
                canCraft = EditorGUILayout.Toggle("Can Craft", canCraft);
                canSocialize = EditorGUILayout.Toggle("Can Socialize", canSocialize);
                hasNeedsDecay = EditorGUILayout.Toggle("Has Needs Decay", hasNeedsDecay);
                canReproduce = EditorGUILayout.Toggle("Can Reproduce", canReproduce);
                canAge = EditorGUILayout.Toggle("Can Age", canAge);
                EditorGUI.indentLevel--;
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("Advanced Tags:", EditorStyles.boldLabel);
            DrawAdvancedTagsEditor();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawGenerationTab()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Generation Settings", EditorStyles.boldLabel);
            
            generatePrefab = EditorGUILayout.Toggle("Generate Prefab", generatePrefab);
            if (generatePrefab)
            {
                EditorGUI.indentLevel++;
                prefabSavePath = EditorGUILayout.TextField("Prefab Path:", prefabSavePath);
                EditorGUI.indentLevel--;
            }
            
            GUILayout.Space(5);
            
            generateEntityTypeData = EditorGUILayout.Toggle("Generate EntityType Data", generateEntityTypeData);
            if (generateEntityTypeData)
            {
                EditorGUI.indentLevel++;
                entityTypeDataPath = EditorGUILayout.TextField("Data Path:", entityTypeDataPath);
                EditorGUI.indentLevel--;
            }
            
            GUILayout.Space(20);
            
            DrawEntityValidation();
            
            GUILayout.Space(20);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Entity", GUILayout.Height(40)))
            {
                CreateEntity();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Add to Database", GUILayout.Height(40)))
            {
                AddToDatabase();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(280));
            
            GUILayout.Label("Entity Preview", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Basic Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Name:", entityName);
            EditorGUILayout.LabelField("ID:", entityID.ToString());
            EditorGUILayout.LabelField("Size:", entitySize.ToString());
            EditorGUILayout.LabelField("Types:", string.Join(", ", entityTypes.Select(t => t.ToString())));
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Stat Balance", EditorStyles.boldLabel);
            
            GUI.color = statBalanceColor;
            EditorGUILayout.LabelField("Total Points:", totalStatPoints.ToString("F1"));
            EditorGUILayout.LabelField("Recommended:", recommendedStatPoints.ToString("F1"));
            GUI.color = Color.white;
            
            float percentage = (totalStatPoints / recommendedStatPoints) * 100f;
            EditorGUILayout.LabelField("Balance:", "percentage:F1%");
            
            Rect progressRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(progressRect, percentage / 100f, "percentage:F1%");
            
            EditorGUILayout.EndVertical();
            
            if (selectedTraits.Count > 0)
            {
                EditorGUILayout.BeginVertical("box");
                
                GUILayout.Label("Selected Traits", EditorStyles.boldLabel);
                foreach (var traitID in selectedTraits)
                {
                    var trait = GameData.GetTraitByID(traitID);
                    EditorGUILayout.LabelField(" " + trait.traitName, EditorStyles.wordWrappedMiniLabel);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Validation", EditorStyles.boldLabel);
            DrawQuickValidation();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }

        // Helper methods would continue here...
        private void DrawEntityTypesSelection()
        {
            UnityEngine.Debug.Log("DrawEntityTypesSelection placeholder");
        }

        private void DrawBasicTagsEditor()
        {
            UnityEngine.Debug.Log("DrawBasicTagsEditor placeholder");
        }

        private void DrawAdvancedTagsEditor()
        {
            UnityEngine.Debug.Log("DrawAdvancedTagsEditor placeholder");
        }

        private Stat DrawStatEditor(string label, Stat stat, float minValue, float maxValue)
        {
            return stat;
        }

        private Health DrawHealthEditor(string label, Health health, float minValue, float maxValue)
        {
            return health;
        }

        private void DrawTraitSelection()
        {
            UnityEngine.Debug.Log("DrawTraitSelection placeholder");
        }

        private void DrawSelectedTraits()
        {
            UnityEngine.Debug.Log("DrawSelectedTraits placeholder");
        }

        private void DrawEntityValidation()
        {
            UnityEngine.Debug.Log("DrawEntityValidation placeholder");
        }

        private void DrawQuickValidation()
        {
            UnityEngine.Debug.Log("DrawQuickValidation placeholder");
        }

        private void DrawFooter()
        {
            UnityEngine.Debug.Log("DrawFooter placeholder");
        }

        private void RefreshAvailableTraits()
        {
            UnityEngine.Debug.Log("RefreshAvailableTraits placeholder");
        }

        private void CalculateStatPoints()
        {
            totalStatPoints = 100f;
        }

        private void ApplyStatPreset(string presetName)
        {
            UnityEngine.Debug.Log("ApplyStatPreset: {presetName}");
        }

        private void BalanceAllStats()
        {
            UnityEngine.Debug.Log("BalanceAllStats placeholder");
        }

        private void AssignRandomTraits()
        {
            UnityEngine.Debug.Log("AssignRandomTraits placeholder");
        }

        private void CreateEntity()
        {
            UnityEngine.Debug.Log("CreateEntity placeholder");
        }

        private void AddToDatabase()
        {
            UnityEngine.Debug.Log("AddToDatabase placeholder");
        }

        private void LoadEntityTemplate()
        {
            UnityEngine.Debug.Log("LoadEntityTemplate placeholder");
        }

        private void SaveEntityTemplate()
        {
            UnityEngine.Debug.Log("SaveEntityTemplate placeholder");
        }
    }
}

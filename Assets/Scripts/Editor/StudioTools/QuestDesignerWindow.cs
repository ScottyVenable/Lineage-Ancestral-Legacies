using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Comprehensive quest creation and editing tool for the Lineage game.
    /// Provides visual design of quests, objectives, rewards, and branching narratives.
    /// </summary>
    public class QuestDesignerWindow : EditorWindow
    {
        #region Window State
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private readonly string[] tabs = { "Basic Info", "Objectives", "Rewards", "Requirements", "Structure", "Preview" };
        #endregion

        #region Quest Data
        private Quest currentQuest;
        private bool isEditingExisting = false;
        private Quest.ID editingQuestID;
        
        // Basic Info
        private string questName = "";
        private string description = "";
        private Quest.Type questType = Quest.Type.GatherResources;
        private Quest.Status status = Quest.Status.NotStarted;
        private int experienceReward = 100;
        private int questCompletionPercentage = 0;
        
        // Objectives
        private List<ObjectiveData> objectives = new List<ObjectiveData>();
        private Vector2 objectivesScrollPosition;
        
        // Rewards
        private List<Item.ID> itemRewards = new List<Item.ID>();
        private List<RewardData> customRewards = new List<RewardData>();
        
        // Requirements
        private int minimumLevel = 1;
        private List<Quest.ID> prerequisiteQuests = new List<Quest.ID>();
        private List<Trait.ID> requiredTraits = new List<Trait.ID>();
        private List<Item.ID> requiredItems = new List<Item.ID>();
        
        // Quest Structure
        private bool isMainQuest = false;
        private bool isRepeatable = false;
        private bool hasTimeLimit = false;
        private float timeLimitHours = 24f;
        private List<Quest.ID> followUpQuests = new List<Quest.ID>();
        
        // Validation
        private List<string> validationErrors = new List<string>();
        #endregion

        #region Nested Data Structures
        [System.Serializable]
        private class ObjectiveData
        {
            public string name = "";
            public string description = "";
            public Objective.ID type = Objective.ID.Collect;
            public Objective.Difficulty difficulty = Objective.Difficulty.Easy;
            public bool isCompleted = false;
            public bool isOptional = false;
            public int targetCount = 1;
            public int currentCount = 0;
            public List<Item.ID> targetItems = new List<Item.ID>();
            public List<NPC.ID> targetNPCs = new List<NPC.ID>();
            public string locationHint = "";
            public List<string> tags = new List<string>();
            public int experienceReward = 25;
            public List<Item.ID> objectiveRewards = new List<Item.ID>();
        }

        [System.Serializable]
        private class RewardData
        {
            public string name = "";
            public string description = "";
            public RewardType type = RewardType.Experience;
            public int amount = 0;
            public Item.ID itemID = Item.ID.GoldCoin;
        }

        private enum RewardType
        {
            Experience,
            Currency,
            Item,
            Skill,
            Trait,
            Reputation
        }
        #endregion

        #region GUI Styles
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle errorStyle;
        private GUIStyle objectiveStyle;
        private bool stylesInitialized = false;
        #endregion

        [MenuItem("Lineage Studio/Content Creation/Quest Designer", priority = 23)]
        public static void ShowWindow()
        {
            QuestDesignerWindow window = GetWindow<QuestDesignerWindow>("Quest Designer");
            window.minSize = new Vector2(900, 700);
            window.Show();
        }

        private void OnEnable()
        {
            InitializeNewQuest();
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

            objectiveStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset(2, 2, 2, 2)
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
                GUILayout.Label("Quest Designer", headerStyle);
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Load Existing", EditorStyles.toolbarButton))
                {
                    ShowQuestSelectionMenu();
                }
                
                if (GUILayout.Button("New Quest", EditorStyles.toolbarButton))
                {
                    InitializeNewQuest();
                }
                
                GUI.enabled = !string.IsNullOrEmpty(questName) && objectives.Count > 0;
                if (GUILayout.Button(isEditingExisting ? "Update" : "Create", EditorStyles.toolbarButton))
                {
                    CreateOrUpdateQuest();
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
                case 1: DrawObjectivesTab(); break;
                case 2: DrawRewardsTab(); break;
                case 3: DrawRequirementsTab(); break;
                case 4: DrawStructureTab(); break;
                case 5: DrawPreviewTab(); break;
            }
        }

        private void DrawBasicInfoTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Basic Information", headerStyle);
                
                questName = EditorGUILayout.TextField("Quest Name", questName);
                
                EditorGUILayout.LabelField("Description");
                description = EditorGUILayout.TextArea(description, GUILayout.Height(80));
                
                questType = (Quest.Type)EditorGUILayout.EnumPopup("Quest Type", questType);
                status = (Quest.Status)EditorGUILayout.EnumPopup("Status", status);
                
                EditorGUILayout.Space();
                
                experienceReward = EditorGUILayout.IntField("Base Experience Reward", experienceReward);
                questCompletionPercentage = EditorGUILayout.IntSlider("Completion Percentage", questCompletionPercentage, 0, 100);
                
                EditorGUILayout.Space();
                DrawQuestTypePresets();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawQuestTypePresets()
        {
            EditorGUILayout.LabelField("Quest Type Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {                if (GUILayout.Button("Main Story"))
                {
                    questType = Quest.Type.Investigation;
                    isMainQuest = true;
                    experienceReward = 500;
                }                if (GUILayout.Button("Side Quest"))
                {
                    questType = Quest.Type.SocialInteraction;
                    isMainQuest = false;
                    experienceReward = 200;
                }
                if (GUILayout.Button("Gathering"))
                {
                    questType = Quest.Type.GatherResources;
                    experienceReward = 100;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            {                if (GUILayout.Button("Combat"))
                {
                    questType = Quest.Type.KillTarget;
                    experienceReward = 300;
                }                if (GUILayout.Button("Exploration"))
                {
                    questType = Quest.Type.ExploreArea;
                    experienceReward = 150;
                }
                if (GUILayout.Button("Social"))
                {
                    questType = Quest.Type.SocialInteraction;
                    experienceReward = 250;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawObjectivesTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Objectives", headerStyle);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add Objective", GUILayout.Width(100)))
                    {
                        objectives.Add(new ObjectiveData());
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                objectivesScrollPosition = EditorGUILayout.BeginScrollView(objectivesScrollPosition, GUILayout.Height(400));
                {
                    for (int i = 0; i < objectives.Count; i++)
                    {
                        DrawObjectiveEditor(objectives[i], i);
                    }
                }
                EditorGUILayout.EndScrollView();
                
                DrawObjectivePresets();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawObjectiveEditor(ObjectiveData objective, int index)
        {
            EditorGUILayout.BeginVertical(objectiveStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    objective.name = EditorGUILayout.TextField($"Objective {index + 1} Name", objective.name);
                    
                    objective.isOptional = EditorGUILayout.Toggle("Optional", objective.isOptional, GUILayout.Width(80));
                    
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        objectives.RemoveAt(index);
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                objective.description = EditorGUILayout.TextField("Description", objective.description);
                
                EditorGUILayout.BeginHorizontal();
                {
                    objective.type = (Objective.ID)EditorGUILayout.EnumPopup("Type", objective.type, GUILayout.Width(200));
                    objective.difficulty = (Objective.Difficulty)EditorGUILayout.EnumPopup("Difficulty", objective.difficulty, GUILayout.Width(150));
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                {
                    objective.targetCount = EditorGUILayout.IntField("Target Count", objective.targetCount, GUILayout.Width(200));
                    objective.currentCount = EditorGUILayout.IntField("Current Count", objective.currentCount, GUILayout.Width(200));
                }
                EditorGUILayout.EndHorizontal();
                
                // Objective-specific fields
                switch (objective.type)
                {
                    case Objective.ID.Collect:
                    case Objective.ID.Craft:
                        DrawItemTargets(objective);
                        break;
                    case Objective.ID.TalkToNPC:
                    case Objective.ID.Interact:
                        DrawNPCTargets(objective);
                        break;
                    case Objective.ID.Explore:
                        objective.locationHint = EditorGUILayout.TextField("Location Hint", objective.locationHint);
                        break;
                }
                
                EditorGUILayout.BeginHorizontal();
                {
                    objective.experienceReward = EditorGUILayout.IntField("Experience Reward", objective.experienceReward, GUILayout.Width(200));
                    if (GUILayout.Button("Edit Rewards", GUILayout.Width(100)))
                    {
                        // Could open a sub-window for detailed reward editing
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawItemTargets(ObjectiveData objective)
        {
            EditorGUILayout.LabelField("Target Items:", EditorStyles.boldLabel);
            
            for (int i = 0; i < objective.targetItems.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var item = GameData.GetItemByID(objective.targetItems[i]);
                    EditorGUILayout.LabelField(item.itemName, GUILayout.Width(150));
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        objective.targetItems.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Target Item"))
            {
                ShowItemSelectionMenu(objective.targetItems);
            }
        }

        private void DrawNPCTargets(ObjectiveData objective)
        {
            EditorGUILayout.LabelField("Target NPCs:", EditorStyles.boldLabel);
            
            for (int i = 0; i < objective.targetNPCs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var npc = GameData.GetNPCByName($"NPC_{objective.targetNPCs[i]}");
                    EditorGUILayout.LabelField(npc.npcName, GUILayout.Width(150));
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        objective.targetNPCs.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Target NPC"))
            {
                ShowNPCSelectionMenu(objective.targetNPCs);
            }
        }

        private void DrawObjectivePresets()
        {
            EditorGUILayout.LabelField("Objective Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Collect Items"))
                {
                    var obj = new ObjectiveData
                    {
                        name = "Collect Items",
                        type = Objective.ID.Collect,
                        targetCount = 5,
                        description = "Collect the required items"
                    };
                    objectives.Add(obj);
                }
                if (GUILayout.Button("Talk to NPC"))
                {
                    var obj = new ObjectiveData
                    {
                        name = "Speak with Character",
                        type = Objective.ID.TalkToNPC,
                        targetCount = 1,
                        description = "Have a conversation with the target NPC"
                    };
                    objectives.Add(obj);
                }
                if (GUILayout.Button("Explore Area"))
                {
                    var obj = new ObjectiveData
                    {
                        name = "Explore Location",
                        type = Objective.ID.Explore,
                        targetCount = 1,
                        description = "Discover and explore the target location"
                    };
                    objectives.Add(obj);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRewardsTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Quest Rewards", headerStyle);
                
                // Item Rewards
                EditorGUILayout.LabelField("Item Rewards", EditorStyles.boldLabel);
                DrawItemRewardsList();
                
                EditorGUILayout.Space();
                
                // Custom Rewards
                EditorGUILayout.LabelField("Custom Rewards", EditorStyles.boldLabel);
                DrawCustomRewardsList();
                
                EditorGUILayout.Space();
                DrawRewardPresets();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawItemRewardsList()
        {
            for (int i = 0; i < itemRewards.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var item = GameData.GetItemByID(itemRewards[i]);
                    EditorGUILayout.LabelField(item.itemName, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"Type: {item.itemType}", GUILayout.Width(150));
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        itemRewards.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Item Reward"))
            {
                ShowItemSelectionMenu(itemRewards);
            }
        }

        private void DrawCustomRewardsList()
        {
            for (int i = 0; i < customRewards.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        customRewards[i].name = EditorGUILayout.TextField("Name", customRewards[i].name);
                        if (GUILayout.Button("×", GUILayout.Width(20)))
                        {
                            customRewards.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    customRewards[i].type = (RewardType)EditorGUILayout.EnumPopup("Type", customRewards[i].type);
                    customRewards[i].amount = EditorGUILayout.IntField("Amount", customRewards[i].amount);
                    
                    if (customRewards[i].type == RewardType.Item)
                    {
                        customRewards[i].itemID = (Item.ID)EditorGUILayout.EnumPopup("Item", customRewards[i].itemID);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            
            if (GUILayout.Button("Add Custom Reward"))
            {
                customRewards.Add(new RewardData());
            }
        }

        private void DrawRewardPresets()
        {
            EditorGUILayout.LabelField("Reward Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Basic Loot"))
                {
                    itemRewards.Add(Item.ID.GoldCoin);
                    customRewards.Add(new RewardData { name = "Gold", type = RewardType.Currency, amount = 50 });
                }
                if (GUILayout.Button("Rare Equipment"))
                {
                    itemRewards.Add(Item.ID.SteelAxe);
                    itemRewards.Add(Item.ID.LeatherArmor);
                }
                if (GUILayout.Button("Experience Boost"))
                {
                    experienceReward += 200;
                    customRewards.Add(new RewardData { name = "Bonus XP", type = RewardType.Experience, amount = 100 });
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRequirementsTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Quest Requirements", headerStyle);
                
                minimumLevel = EditorGUILayout.IntField("Minimum Level", minimumLevel);
                
                EditorGUILayout.Space();
                
                // Prerequisite Quests
                EditorGUILayout.LabelField("Prerequisite Quests", EditorStyles.boldLabel);
                DrawQuestPrerequisites();
                
                EditorGUILayout.Space();
                
                // Required Traits
                EditorGUILayout.LabelField("Required Traits", EditorStyles.boldLabel);
                DrawTraitRequirements();
                
                EditorGUILayout.Space();
                
                // Required Items
                EditorGUILayout.LabelField("Required Items", EditorStyles.boldLabel);
                DrawItemRequirements();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawQuestPrerequisites()
        {
            for (int i = 0; i < prerequisiteQuests.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var quest = GameData.GetQuestByID(prerequisiteQuests[i]);
                    EditorGUILayout.LabelField(quest.questName, GUILayout.Width(200));
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        prerequisiteQuests.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Prerequisite Quest"))
            {
                ShowQuestSelectionMenu(prerequisiteQuests);
            }
        }

        private void DrawTraitRequirements()
        {
            for (int i = 0; i < requiredTraits.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var trait = GameData.GetTraitByID(requiredTraits[i]);
                    EditorGUILayout.LabelField(trait.traitName, GUILayout.Width(200));
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        requiredTraits.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Required Trait"))
            {
                ShowTraitSelectionMenu(requiredTraits);
            }
        }

        private void DrawItemRequirements()
        {
            for (int i = 0; i < requiredItems.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var item = GameData.GetItemByID(requiredItems[i]);
                    EditorGUILayout.LabelField(item.itemName, GUILayout.Width(200));
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        requiredItems.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Required Item"))
            {
                ShowItemSelectionMenu(requiredItems);
            }
        }

        private void DrawStructureTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Quest Structure", headerStyle);
                
                isMainQuest = EditorGUILayout.Toggle("Is Main Quest", isMainQuest);
                isRepeatable = EditorGUILayout.Toggle("Is Repeatable", isRepeatable);
                
                hasTimeLimit = EditorGUILayout.Toggle("Has Time Limit", hasTimeLimit);
                if (hasTimeLimit)
                {
                    timeLimitHours = EditorGUILayout.FloatField("Time Limit (Hours)", timeLimitHours);
                }
                
                EditorGUILayout.Space();
                
                // Follow-up Quests
                EditorGUILayout.LabelField("Follow-up Quests", EditorStyles.boldLabel);
                DrawFollowUpQuests();
                
                EditorGUILayout.Space();
                DrawStructurePresets();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFollowUpQuests()
        {
            for (int i = 0; i < followUpQuests.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var quest = GameData.GetQuestByID(followUpQuests[i]);
                    EditorGUILayout.LabelField(quest.questName, GUILayout.Width(200));
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        followUpQuests.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Follow-up Quest"))
            {
                ShowQuestSelectionMenu(followUpQuests);
            }
        }

        private void DrawStructurePresets()
        {
            EditorGUILayout.LabelField("Structure Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Main Story Quest"))
                {
                    isMainQuest = true;
                    isRepeatable = false;
                    hasTimeLimit = false;
                    questType = Quest.Type.Investigation;
                }
                if (GUILayout.Button("Daily Quest"))
                {
                    isMainQuest = false;
                    isRepeatable = true;
                    hasTimeLimit = true;
                    timeLimitHours = 24f;
                }
                if (GUILayout.Button("Timed Challenge"))
                {
                    isMainQuest = false;
                    isRepeatable = true;
                    hasTimeLimit = true;
                    timeLimitHours = 1f;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPreviewTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Quest Preview", headerStyle);
                
                ValidateQuest();
                
                if (validationErrors.Count > 0)
                {
                    EditorGUILayout.LabelField("Validation Errors:", EditorStyles.boldLabel);
                    foreach (var error in validationErrors)
                    {
                        EditorGUILayout.HelpBox(error, MessageType.Error);
                    }
                    EditorGUILayout.Space();
                }
                
                DrawQuestPreview();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawQuestPreview()
        {
            EditorGUILayout.LabelField("Generated Quest Data", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField($"Name: {questName}");
                EditorGUILayout.LabelField($"Type: {questType}");
                EditorGUILayout.LabelField($"Status: {status}");
                EditorGUILayout.LabelField($"Experience Reward: {experienceReward}");
                
                EditorGUILayout.LabelField("Description:");
                EditorGUILayout.TextArea(description, EditorStyles.wordWrappedLabel);
                
                // Objectives Preview
                if (objectives.Count > 0)
                {
                    EditorGUILayout.LabelField($"Objectives ({objectives.Count}):", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    foreach (var obj in objectives)
                    {
                        string optional = obj.isOptional ? " (Optional)" : "";
                        EditorGUILayout.LabelField($"• {obj.name} - {obj.type}{optional}");
                    }
                    EditorGUI.indentLevel--;
                }
                
                // Rewards Preview
                if (itemRewards.Count > 0 || customRewards.Count > 0)
                {
                    EditorGUILayout.LabelField("Rewards:", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    foreach (var itemID in itemRewards)
                    {
                        var item = GameData.GetItemByID(itemID);
                        EditorGUILayout.LabelField($"• {item.itemName}");
                    }
                    foreach (var reward in customRewards)
                    {
                        EditorGUILayout.LabelField($"• {reward.name}: {reward.amount} {reward.type}");
                    }
                    EditorGUI.indentLevel--;
                }
                
                // Structure info
                if (isMainQuest || isRepeatable || hasTimeLimit)
                {
                    EditorGUILayout.LabelField("Structure:", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    if (isMainQuest) EditorGUILayout.LabelField("• Main Quest");
                    if (isRepeatable) EditorGUILayout.LabelField("• Repeatable");
                    if (hasTimeLimit) EditorGUILayout.LabelField($"• Time Limit: {timeLimitHours} hours");
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();
        }

        #region Helper Methods
        private void InitializeNewQuest()
        {
            isEditingExisting = false;
            questName = "";
            description = "";
            questType = Quest.Type.GatherResources;
            status = Quest.Status.NotStarted;
            experienceReward = 100;
            questCompletionPercentage = 0;
            objectives.Clear();
            itemRewards.Clear();
            customRewards.Clear();
            minimumLevel = 1;
            prerequisiteQuests.Clear();
            requiredTraits.Clear();
            requiredItems.Clear();
            isMainQuest = false;
            isRepeatable = false;
            hasTimeLimit = false;
            timeLimitHours = 24f;
            followUpQuests.Clear();
            selectedTab = 0;
        }

        private void ShowQuestSelectionMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            var questIDs = System.Enum.GetValues(typeof(Quest.ID)).Cast<Quest.ID>();
            foreach (var questID in questIDs)
            {
                var quest = GameData.GetQuestByID(questID);
                menu.AddItem(new GUIContent($"{quest.questType}/{quest.questName}"), false, () => LoadExistingQuest(questID));
            }
            
            menu.ShowAsContext();
        }

        private void ShowQuestSelectionMenu(List<Quest.ID> targetList)
        {
            GenericMenu menu = new GenericMenu();
            
            var questIDs = System.Enum.GetValues(typeof(Quest.ID)).Cast<Quest.ID>();
            foreach (var questID in questIDs)
            {
                if (!targetList.Contains(questID))
                {
                    var quest = GameData.GetQuestByID(questID);
                    menu.AddItem(new GUIContent($"{quest.questType}/{quest.questName}"), false, () => targetList.Add(questID));
                }
            }
            
            menu.ShowAsContext();
        }

        private void ShowItemSelectionMenu(List<Item.ID> targetList)
        {
            GenericMenu menu = new GenericMenu();
            
            var itemIDs = System.Enum.GetValues(typeof(Item.ID)).Cast<Item.ID>();
            foreach (var itemID in itemIDs)
            {
                if (!targetList.Contains(itemID))
                {
                    var item = GameData.GetItemByID(itemID);
                    menu.AddItem(new GUIContent($"{item.itemType}/{item.itemName}"), false, () => targetList.Add(itemID));
                }
            }
            
            menu.ShowAsContext();
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

        private void ShowNPCSelectionMenu(List<NPC.ID> targetList)
        {
            GenericMenu menu = new GenericMenu();
            
            var npcIDs = System.Enum.GetValues(typeof(NPC.ID)).Cast<NPC.ID>();
            foreach (var npcID in npcIDs)
            {
                if (!targetList.Contains(npcID))
                {
                    var npc = GameData.GetNPCByName($"NPC_{npcID}");
                    menu.AddItem(new GUIContent($"{npc.archetype}/{npc.npcName}"), false, () => targetList.Add(npcID));
                }
            }
            
            menu.ShowAsContext();
        }

        private void LoadExistingQuest(Quest.ID questID)
        {
            var quest = GameData.GetQuestByID(questID);
            
            isEditingExisting = true;
            editingQuestID = questID;
            questName = quest.questName;
            description = quest.description;
            questType = quest.questType;
            status = quest.status;
            experienceReward = quest.experienceReward;
            questCompletionPercentage = quest.questCompletionPercentage;
            
            // Convert objectives
            objectives.Clear();
            foreach (var obj in quest.objectives)
            {
                var objData = new ObjectiveData
                {
                    name = obj.objectiveName,
                    description = obj.description,
                    type = obj.objectiveID,
                    isCompleted = obj.isCompleted,
                    experienceReward = 25 // Default, would need to extract from obj
                };
                objectives.Add(objData);
            }
              // Convert rewards
            itemRewards.Clear();
            foreach (var item in quest.rewards)
            {
                itemRewards.Add((Item.ID)item.itemID);
            }
            
            selectedTab = 0;
        }

        private void CreateOrUpdateQuest()
        {
            if (!ValidateQuestForCreation())
            {
                EditorUtility.DisplayDialog("Validation Error", "Please fix validation errors before creating the quest.", "OK");
                return;
            }

            // In a real implementation, this would save to the database
            var newQuest = ConstructQuest();
            
            if (isEditingExisting)
            {
                UnityEngine.Debug.Log($"Updated quest: {newQuest.questName}");
                EditorUtility.DisplayDialog("Success", $"Quest '{newQuest.questName}' has been updated!", "OK");
            }
            else
            {
                UnityEngine.Debug.Log($"Created new quest: {newQuest.questName}");
                EditorUtility.DisplayDialog("Success", $"Quest '{newQuest.questName}' has been created!", "OK");
            }
        }

        private Quest ConstructQuest()
        {
            // Get next available ID (in real implementation, this would be managed by the database)
            var nextID = isEditingExisting ? editingQuestID : (Quest.ID)42; // Placeholder
            
            var quest = new Quest(nextID, questName, description);
            
            // Note: This would require the Quest struct to have setters or a more complex constructor
            // to properly set all the configured properties
            
            return quest;
        }

        private void ValidateQuest()
        {
            validationErrors.Clear();
            
            if (string.IsNullOrEmpty(questName))
                validationErrors.Add("Quest name is required");
            
            if (string.IsNullOrEmpty(description))
                validationErrors.Add("Description is required");
            
            if (objectives.Count == 0)
                validationErrors.Add("At least one objective is required");
            
            foreach (var obj in objectives)
            {
                if (string.IsNullOrEmpty(obj.name))
                    validationErrors.Add($"Objective name is required for all objectives");
                
                if (obj.targetCount <= 0)
                    validationErrors.Add($"Target count must be greater than 0 for objective: {obj.name}");
            }
            
            if (experienceReward < 0)
                validationErrors.Add("Experience reward cannot be negative");
        }

        private bool ValidateQuestForCreation()
        {
            ValidateQuest();
            return validationErrors.Count == 0;
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal("toolbar");
            {
                GUILayout.Label($"Current Tab: {tabs[selectedTab]}");
                GUILayout.FlexibleSpace();
                
                GUILayout.Label($"Objectives: {objectives.Count}");
                GUILayout.Label($"Rewards: {itemRewards.Count + customRewards.Count}");
                
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

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Database;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Comprehensive NPC creation and editing tool for the Lineage game.
    /// Provides visual design of NPC properties, relationships, dialogue, and quest assignments.
    /// </summary>
    public class NPCEditorWindow : EditorWindow
    {
        #region Window State
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private readonly string[] tabs = { "Basic Info", "Entity Data", "Relationships", "Dialogue", "Quests", "Preview" };
        #endregion

        #region NPC Data
        private NPC currentNPC;
        private bool isEditingExisting = false;
        private string editingNPCName = "";
        
        // Basic Info
        private string npcName = "";
        private NPC.Archetype archetype = NPC.Archetype.Trader;
        private string description = "";
        private List<string> tags = new List<string>();
        
        // Entity Data
        private Entity entityData;
        private string entityName = "";
        private Entity.ID entityType = Entity.ID.Pop;
        private bool createNewEntity = true;
        private string existingEntityName = "";
        
        // Relationships
        private Dictionary<string, int> relationships = new Dictionary<string, int>();
        private string newRelationshipName = "";
        private int newRelationshipValue = 0;
        private Vector2 relationshipsScrollPosition;
        
        // Dialogue
        private List<string> dialogueKeys = new List<string>();
        private string newDialogueKey = "";
        private Vector2 dialogueScrollPosition;
        
        // Quests
        private List<Quest.ID> availableQuests = new List<Quest.ID>();
        private Vector2 questsScrollPosition;
        
        // Behavior Settings
        private bool isHostile = false;
        private bool isInteractable = true;
        private bool isTrader = false;
        private bool providesServices = false;
        private List<string> services = new List<string>();
        
        // Validation
        private List<string> validationErrors = new List<string>();
        #endregion

        #region GUI Styles
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle errorStyle;
        private GUIStyle relationshipStyle;
        private bool stylesInitialized = false;
        #endregion

        [MenuItem("Lineage Studio/Content Creation/NPC Editor", priority = 24)]
        public static void ShowWindow()
        {
            NPCEditorWindow window = GetWindow<NPCEditorWindow>("NPC Editor");
            window.minSize = new Vector2(800, 650);
            window.Show();
        }

        private void OnEnable()
        {
            InitializeNewNPC();
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

            relationshipStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(5, 5, 5, 5),
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
                GUILayout.Label("NPC Editor", headerStyle);
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Load Existing", EditorStyles.toolbarButton))
                {
                    ShowNPCSelectionMenu();
                }
                
                if (GUILayout.Button("New NPC", EditorStyles.toolbarButton))
                {
                    InitializeNewNPC();
                }
                
                GUI.enabled = !string.IsNullOrEmpty(npcName);
                if (GUILayout.Button(isEditingExisting ? "Update" : "Create", EditorStyles.toolbarButton))
                {
                    CreateOrUpdateNPC();
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
                case 1: DrawEntityDataTab(); break;
                case 2: DrawRelationshipsTab(); break;
                case 3: DrawDialogueTab(); break;
                case 4: DrawQuestsTab(); break;
                case 5: DrawPreviewTab(); break;
            }
        }

        private void DrawBasicInfoTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Basic Information", headerStyle);
                
                npcName = EditorGUILayout.TextField("NPC Name", npcName);
                archetype = (NPC.Archetype)EditorGUILayout.EnumPopup("Archetype", archetype);
                
                EditorGUILayout.LabelField("Description");
                description = EditorGUILayout.TextArea(description, GUILayout.Height(60));
                
                EditorGUILayout.Space();
                DrawArchetypePresets();
                
                EditorGUILayout.Space();
                DrawBehaviorSettings();
                
                EditorGUILayout.Space();
                DrawTagsSection();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawArchetypePresets()
        {
            EditorGUILayout.LabelField("Archetype Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Trader"))
                {
                    archetype = NPC.Archetype.Trader;
                    isTrader = true;
                    providesServices = true;
                    services.Clear();
                    services.AddRange(new[] { "Buy Items", "Sell Items", "Trade" });
                }
                if (GUILayout.Button("Warrior"))
                {
                    archetype = NPC.Archetype.Warrior;
                    isTrader = false;
                    providesServices = true;
                    services.Clear();
                    services.AddRange(new[] { "Training", "Combat Tips" });
                }
                if (GUILayout.Button("Scholar"))
                {
                    archetype = NPC.Archetype.Scholar;
                    isTrader = false;
                    providesServices = true;
                    services.Clear();
                    services.AddRange(new[] { "Research", "Information", "Lore" });
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Healer"))
                {
                    archetype = NPC.Archetype.Healer;
                    isTrader = false;
                    providesServices = true;
                    services.Clear();
                    services.AddRange(new[] { "Healing", "Restore Health", "Remove Debuffs" });
                }
                if (GUILayout.Button("Guide"))
                {
                    archetype = NPC.Archetype.Guide;
                    isTrader = false;
                    providesServices = true;
                    services.Clear();
                    services.AddRange(new[] { "Directions", "Map Information", "Local Knowledge" });
                }
                if (GUILayout.Button("Hermit"))
                {
                    archetype = NPC.Archetype.Hermit;
                    isTrader = false;
                    providesServices = false;
                    services.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBehaviorSettings()
        {
            EditorGUILayout.LabelField("Behavior Settings", EditorStyles.boldLabel);
            
            isHostile = EditorGUILayout.Toggle("Is Hostile", isHostile);
            isInteractable = EditorGUILayout.Toggle("Is Interactable", isInteractable);
            isTrader = EditorGUILayout.Toggle("Is Trader", isTrader);
            providesServices = EditorGUILayout.Toggle("Provides Services", providesServices);
            
            if (providesServices)
            {
                DrawServicesSection();
            }
        }

        private void DrawServicesSection()
        {
            EditorGUILayout.LabelField("Services Provided", EditorStyles.boldLabel);
            
            for (int i = 0; i < services.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    services[i] = EditorGUILayout.TextField(services[i]);
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        services.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Service"))
            {
                services.Add("New Service");
            }
        }

        private void DrawTagsSection()
        {
            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
            
            // Add new tag
            EditorGUILayout.BeginHorizontal();
            {
                var newTag = EditorGUILayout.TextField("New Tag", "");
                if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newTag))
                {
                    if (!tags.Contains(newTag))
                    {
                        tags.Add(newTag);
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
                if (GUILayout.Button("Important")) AddTagIfNotExists("Important");
                if (GUILayout.Button("Friendly")) AddTagIfNotExists("Friendly");
                if (GUILayout.Button("Unique")) AddTagIfNotExists("Unique");
                if (GUILayout.Button("Quest Giver")) AddTagIfNotExists("Quest Giver");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntityDataTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Entity Data", headerStyle);
                
                createNewEntity = EditorGUILayout.Toggle("Create New Entity", createNewEntity);
                
                if (createNewEntity)
                {
                    EditorGUILayout.LabelField("New Entity Settings", EditorStyles.boldLabel);
                    entityName = EditorGUILayout.TextField("Entity Name", entityName);
                    entityType = (Entity.ID)EditorGUILayout.EnumPopup("Entity Type", entityType);
                    
                    if (GUILayout.Button("Auto-Fill from NPC Name"))
                    {
                        entityName = npcName;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Existing Entity", EditorStyles.boldLabel);
                    existingEntityName = EditorGUILayout.TextField("Entity Name", existingEntityName);
                    
                    if (GUILayout.Button("Search Entities"))
                    {
                        ShowEntitySelectionMenu();
                    }
                }
                
                EditorGUILayout.Space();
                DrawEntityPreview();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawEntityPreview()
        {
            EditorGUILayout.LabelField("Entity Preview", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                if (createNewEntity)
                {
                    EditorGUILayout.LabelField($"Name: {entityName}");
                    EditorGUILayout.LabelField($"Type: {entityType}");
                    EditorGUILayout.LabelField("Status: Will be created");
                }
                else
                {
                    if (!string.IsNullOrEmpty(existingEntityName))
                    {
                        var entities = GameData.GetEntitiesByName(existingEntityName);
                        if (entities.Count > 0)
                        {
                            var entity = entities[0];
                            EditorGUILayout.LabelField($"Name: {entity.entityName}");
                            EditorGUILayout.LabelField($"Type: {entity.entityID}");
                            EditorGUILayout.LabelField("Status: Using existing");
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No entity found");
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No entity selected");
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawRelationshipsTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("NPC Relationships", headerStyle);
                
                // Add new relationship
                EditorGUILayout.LabelField("Add New Relationship", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                {
                    newRelationshipName = EditorGUILayout.TextField("NPC Name", newRelationshipName);
                    newRelationshipValue = EditorGUILayout.IntSlider("Relationship", newRelationshipValue, -100, 100);
                    
                    if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newRelationshipName))
                    {
                        if (!relationships.ContainsKey(newRelationshipName))
                        {
                            relationships[newRelationshipName] = newRelationshipValue;
                            newRelationshipName = "";
                            newRelationshipValue = 0;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // Display existing relationships
                EditorGUILayout.LabelField("Current Relationships", EditorStyles.boldLabel);
                relationshipsScrollPosition = EditorGUILayout.BeginScrollView(relationshipsScrollPosition, GUILayout.Height(200));
                {
                    var relationshipKeys = relationships.Keys.ToList();
                    for (int i = 0; i < relationshipKeys.Count; i++)
                    {
                        var key = relationshipKeys[i];
                        EditorGUILayout.BeginVertical(relationshipStyle);
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(key, GUILayout.Width(150));
                                
                                int newValue = EditorGUILayout.IntSlider(relationships[key], -100, 100);
                                relationships[key] = newValue;
                                
                                string relationshipDesc = GetRelationshipDescription(newValue);
                                EditorGUILayout.LabelField(relationshipDesc, GUILayout.Width(80));
                                
                                if (GUILayout.Button("×", GUILayout.Width(20)))
                                {
                                    relationships.Remove(key);
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                EditorGUILayout.EndScrollView();
                
                DrawRelationshipPresets();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawRelationshipPresets()
        {
            EditorGUILayout.LabelField("Relationship Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add Ally (+75)"))
                {
                    if (!string.IsNullOrEmpty(newRelationshipName))
                    {
                        relationships[newRelationshipName] = 75;
                        newRelationshipName = "";
                    }
                }
                if (GUILayout.Button("Add Friend (+50)"))
                {
                    if (!string.IsNullOrEmpty(newRelationshipName))
                    {
                        relationships[newRelationshipName] = 50;
                        newRelationshipName = "";
                    }
                }
                if (GUILayout.Button("Add Enemy (-50)"))
                {
                    if (!string.IsNullOrEmpty(newRelationshipName))
                    {
                        relationships[newRelationshipName] = -50;
                        newRelationshipName = "";
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDialogueTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Dialogue System", headerStyle);
                
                // Add new dialogue key
                EditorGUILayout.BeginHorizontal();
                {
                    newDialogueKey = EditorGUILayout.TextField("New Dialogue Key", newDialogueKey);
                    if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newDialogueKey))
                    {
                        if (!dialogueKeys.Contains(newDialogueKey))
                        {
                            dialogueKeys.Add(newDialogueKey);
                            newDialogueKey = "";
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // Display dialogue keys
                EditorGUILayout.LabelField("Dialogue Keys", EditorStyles.boldLabel);
                dialogueScrollPosition = EditorGUILayout.BeginScrollView(dialogueScrollPosition, GUILayout.Height(200));
                {
                    for (int i = 0; i < dialogueKeys.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            dialogueKeys[i] = EditorGUILayout.TextField(dialogueKeys[i]);
                            if (GUILayout.Button("×", GUILayout.Width(20)))
                            {
                                dialogueKeys.RemoveAt(i);
                                i--;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
                
                DrawDialoguePresets();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawDialoguePresets()
        {
            EditorGUILayout.LabelField("Dialogue Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Basic Greeting"))
                {
                    AddDialogueKeyIfNotExists("greeting_basic");
                    AddDialogueKeyIfNotExists("greeting_friendly");
                    AddDialogueKeyIfNotExists("goodbye_basic");
                }
                if (GUILayout.Button("Trader Dialogue"))
                {
                    AddDialogueKeyIfNotExists("shop_welcome");
                    AddDialogueKeyIfNotExists("shop_browse");
                    AddDialogueKeyIfNotExists("shop_purchase");
                    AddDialogueKeyIfNotExists("shop_goodbye");
                }
                if (GUILayout.Button("Quest Giver"))
                {
                    AddDialogueKeyIfNotExists("quest_available");
                    AddDialogueKeyIfNotExists("quest_inprogress");
                    AddDialogueKeyIfNotExists("quest_complete");
                    AddDialogueKeyIfNotExists("quest_reward");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawQuestsTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("Available Quests", headerStyle);
                
                // Display available quests
                questsScrollPosition = EditorGUILayout.BeginScrollView(questsScrollPosition, GUILayout.Height(300));
                {
                    for (int i = 0; i < availableQuests.Count; i++)
                    {
                        var quest = GameData.GetQuestByID(availableQuests[i]);
                        EditorGUILayout.BeginHorizontal("box");
                        {
                            EditorGUILayout.LabelField(quest.questName, GUILayout.Width(200));
                            EditorGUILayout.LabelField(quest.questType.ToString(), GUILayout.Width(100));
                            EditorGUILayout.LabelField($"XP: {quest.experienceReward}", GUILayout.Width(80));
                            
                            if (GUILayout.Button("×", GUILayout.Width(20)))
                            {
                                availableQuests.RemoveAt(i);
                                i--;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
                
                if (GUILayout.Button("Add Quest"))
                {
                    ShowQuestSelectionMenu();
                }
                
                DrawQuestPresets();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawQuestPresets()
        {
            EditorGUILayout.LabelField("Quest Assignment Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Starter Quests"))
                {
                    // Add some basic starter quests (would need to exist in database)
                    EditorUtility.DisplayDialog("Info", "Would assign starter quests if they exist in database", "OK");
                }
                if (GUILayout.Button("Trade Quests"))
                {
                    EditorUtility.DisplayDialog("Info", "Would assign trade-related quests if they exist in database", "OK");
                }
                if (GUILayout.Button("Clear All"))
                {
                    if (EditorUtility.DisplayDialog("Confirm", "Remove all quests from this NPC?", "Yes", "No"))
                    {
                        availableQuests.Clear();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPreviewTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            {
                EditorGUILayout.LabelField("NPC Preview", headerStyle);
                
                ValidateNPC();
                
                if (validationErrors.Count > 0)
                {
                    EditorGUILayout.LabelField("Validation Errors:", EditorStyles.boldLabel);
                    foreach (var error in validationErrors)
                    {
                        EditorGUILayout.HelpBox(error, MessageType.Error);
                    }
                    EditorGUILayout.Space();
                }
                
                DrawNPCPreview();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawNPCPreview()
        {
            EditorGUILayout.LabelField("Generated NPC Data", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField($"Name: {npcName}");
                EditorGUILayout.LabelField($"Archetype: {archetype}");
                
                if (!string.IsNullOrEmpty(description))
                {
                    EditorGUILayout.LabelField("Description:");
                    EditorGUILayout.TextArea(description, EditorStyles.wordWrappedLabel);
                }
                
                // Behavior summary
                List<string> behaviors = new List<string>();
                if (isHostile) behaviors.Add("Hostile");
                if (isInteractable) behaviors.Add("Interactable");
                if (isTrader) behaviors.Add("Trader");
                if (providesServices) behaviors.Add("Service Provider");
                
                if (behaviors.Count > 0)
                {
                    EditorGUILayout.LabelField($"Behaviors: {string.Join(", ", behaviors)}");
                }
                
                // Services
                if (services.Count > 0)
                {
                    EditorGUILayout.LabelField($"Services: {string.Join(", ", services)}");
                }
                
                // Relationships summary
                if (relationships.Count > 0)
                {
                    EditorGUILayout.LabelField($"Relationships: {relationships.Count}");
                    EditorGUI.indentLevel++;
                    foreach (var rel in relationships)
                    {
                        EditorGUILayout.LabelField($"• {rel.Key}: {GetRelationshipDescription(rel.Value)} ({rel.Value})");
                    }
                    EditorGUI.indentLevel--;
                }
                
                // Dialogue summary
                if (dialogueKeys.Count > 0)
                {
                    EditorGUILayout.LabelField($"Dialogue Keys: {dialogueKeys.Count}");
                }
                
                // Quests summary
                if (availableQuests.Count > 0)
                {
                    EditorGUILayout.LabelField($"Available Quests: {availableQuests.Count}");
                    EditorGUI.indentLevel++;
                    foreach (var questID in availableQuests)
                    {
                        var quest = GameData.GetQuestByID(questID);
                        EditorGUILayout.LabelField($"• {quest.questName} ({quest.questType})");
                    }
                    EditorGUI.indentLevel--;
                }
                
                // Tags
                if (tags.Count > 0)
                {
                    EditorGUILayout.LabelField($"Tags: {string.Join(", ", tags)}");
                }
            }
            EditorGUILayout.EndVertical();
        }

        #region Helper Methods
        private void InitializeNewNPC()
        {
            isEditingExisting = false;
            npcName = "";
            archetype = NPC.Archetype.Trader;
            description = "";
            tags.Clear();
            entityName = "";
            entityType = Entity.ID.Pop;
            createNewEntity = true;
            existingEntityName = "";
            relationships.Clear();
            newRelationshipName = "";
            newRelationshipValue = 0;
            dialogueKeys.Clear();
            newDialogueKey = "";
            availableQuests.Clear();
            isHostile = false;
            isInteractable = true;
            isTrader = false;
            providesServices = false;
            services.Clear();
            selectedTab = 0;
        }

        private void ShowNPCSelectionMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            foreach (var npc in GameData.npcDatabase)
            {
                menu.AddItem(new GUIContent($"{npc.archetype}/{npc.npcName}"), false, () => LoadExistingNPC(npc.npcName));
            }
            
            menu.ShowAsContext();
        }

        private void ShowEntitySelectionMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            foreach (var entity in GameData.entityDatabase)
            {
                menu.AddItem(new GUIContent($"{entity.entityID}/{entity.entityName}"), false, () => 
                {
                    existingEntityName = entity.entityName;
                });
            }
            
            menu.ShowAsContext();
        }

        private void ShowQuestSelectionMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            var questIDs = System.Enum.GetValues(typeof(Quest.ID)).Cast<Quest.ID>();
            foreach (var questID in questIDs)
            {
                if (!availableQuests.Contains(questID))
                {
                    var quest = GameData.GetQuestByID(questID);
                    menu.AddItem(new GUIContent($"{quest.questType}/{quest.questName}"), false, () => availableQuests.Add(questID));
                }
            }
            
            menu.ShowAsContext();
        }

        private void LoadExistingNPC(string npcName)
        {
            var npc = GameData.GetNPCByName(npcName);
            
            isEditingExisting = true;
            editingNPCName = npcName;
            this.npcName = npc.npcName;
            archetype = npc.archetype;
            // description would need to be stored separately or in extended NPC structure
            description = ""; // Default
            
            entityData = npc.entityData;
            entityName = npc.entityData.entityName;
            entityType = (Entity.ID)npc.entityData.entityID;
            createNewEntity = false;
            existingEntityName = npc.entityData.entityName;
            
            relationships = new Dictionary<string, int>(npc.relationships);
            dialogueKeys = new List<string>(npc.dialogueKeys);
            
            availableQuests.Clear();
            foreach (var quest in npc.availableQuests)
            {
                availableQuests.Add(quest.questID);
            }
            
            // Set archetype-based defaults
            switch (archetype)
            {
                case NPC.Archetype.Trader:
                    isTrader = true;
                    providesServices = true;
                    break;
                case NPC.Archetype.Healer:
                    providesServices = true;
                    break;
                default:
                    isTrader = false;
                    providesServices = false;
                    break;
            }
            
            selectedTab = 0;
        }

        private void CreateOrUpdateNPC()
        {
            if (!ValidateNPCForCreation())
            {
                EditorUtility.DisplayDialog("Validation Error", "Please fix validation errors before creating the NPC.", "OK");
                return;
            }

            // In a real implementation, this would save to the database
            var newNPC = ConstructNPC();
            
            if (isEditingExisting)
            {
                Log.Info($"Updated NPC: {newNPC.npcName}", Log.LogCategory.Systems);
                EditorUtility.DisplayDialog("Success", $"NPC '{newNPC.npcName}' has been updated!", "OK");
            }
            else
            {
                Log.Info($"Created new NPC: {newNPC.npcName}", Log.LogCategory.Systems);
                EditorUtility.DisplayDialog("Success", $"NPC '{newNPC.npcName}' has been created!", "OK");
            }
        }        private NPC ConstructNPC()
        {
            // Create or get entity
            Entity entity;
            if (createNewEntity)
            {
                entity = new Entity(entityName, entityType);
            }
            else
            {
                var entities = GameData.GetEntitiesByName(existingEntityName);
                entity = entities.Count > 0 ? entities[0] : default(Entity);
            }
            
            var npc = new NPC(npcName, archetype, entity);
            
            // Set dialogue keys
            npc.dialogueKeys = new List<string>(dialogueKeys);
            
            // Set relationships
            npc.relationships = new Dictionary<string, int>(relationships);
            
            // Convert available quest IDs to Quest objects
            npc.availableQuests = new List<Quest>();
            foreach (var questID in availableQuests)
            {
                var quest = GameData.GetQuestByID(questID);
                npc.availableQuests.Add(quest);
            }
            
            return npc;
        }

        private void AddTagIfNotExists(string tag)
        {
            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        private void AddDialogueKeyIfNotExists(string key)
        {
            if (!dialogueKeys.Contains(key))
            {
                dialogueKeys.Add(key);
            }
        }

        private string GetRelationshipDescription(int value)
        {
            if (value >= 75) return "Ally";
            if (value >= 50) return "Friend";
            if (value >= 25) return "Friendly";
            if (value >= -25) return "Neutral";
            if (value >= -50) return "Unfriendly";
            if (value >= -75) return "Enemy";
            return "Hostile";
        }

        private void ValidateNPC()
        {
            validationErrors.Clear();
            
            if (string.IsNullOrEmpty(npcName))
                validationErrors.Add("NPC name is required");
            
            if (createNewEntity && string.IsNullOrEmpty(entityName))
                validationErrors.Add("Entity name is required when creating new entity");
            
            if (!createNewEntity && string.IsNullOrEmpty(existingEntityName))
                validationErrors.Add("Existing entity must be selected");
            
            if (isTrader && archetype != NPC.Archetype.Trader)
                validationErrors.Add("Trader behavior should match Trader archetype");
        }

        private bool ValidateNPCForCreation()
        {
            ValidateNPC();
            return validationErrors.Count == 0;
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal("toolbar");
            {
                GUILayout.Label($"Current Tab: {tabs[selectedTab]}");
                GUILayout.FlexibleSpace();
                
                GUILayout.Label($"Relationships: {relationships.Count}");
                GUILayout.Label($"Dialogue: {dialogueKeys.Count}");
                GUILayout.Label($"Quests: {availableQuests.Count}");
                
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

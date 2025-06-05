using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Comprehensive database editor and visualizer for all game data structures.
    /// Provides visual editing, search, filtering, and relationship management.
    /// </summary>
    public class DatabaseEditorWindow : EditorWindow
    {
        private static DatabaseEditorWindow window;
        
        [Header("Database Selection")]
        private int selectedDatabaseIndex = 0;
        private string[] databaseNames = new string[]
        {
            "Entities", "Items", "Traits", "Quests", "NPCs", "Lore Entries", 
            "Skills", "Buffs", "Objectives", "Stats", "Genetics", "Journal"
        };

        [Header("UI State")]
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 detailScrollPosition = Vector2.zero;
        private string searchFilter = "";
        private bool showFilters = true;
        private bool showRelationships = false;
        private bool showStatistics = true;

        [Header("Selection & Editing")]
        private int selectedItemIndex = -1;
        private object selectedItem = null;
        private bool isEditing = false;
        private bool isDirty = false;

        [Header("Advanced Filters")]
        private string categoryFilter = "";
        private string tagFilter = "";
        private float minStatValue = 0f;
        private float maxStatValue = 100f;
        private bool showOnlyModified = false;

        [Header("Visual Settings")]
        private bool useCompactView = false;
        private bool showIDs = true;
        private bool showTags = true;
        private Color highlightColor = Color.yellow;

        public static void ShowWindow()
        {
            window = GetWindow<DatabaseEditorWindow>("Database Editor");
            window.minSize = new Vector2(1000, 600);
            window.titleContent = new GUIContent("Database Editor", "Lineage Database Editor & Visualizer");
        }

        private void OnEnable()
        {
            // Initialize window state
            RefreshDatabaseCounts();
        }

        private void OnGUI()
        {
            DrawHeader();
            
            EditorGUILayout.BeginHorizontal();
            {
                // Left panel - Database list and filters
                DrawLeftPanel();
                
                // Separator
                GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));
                
                // Right panel - Item details and editing
                DrawRightPanel();
            }
            EditorGUILayout.EndHorizontal();

            DrawFooter();
            
            if (isDirty)
            {
                Repaint();
                isDirty = false;
            }
        }

        #region Header

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Lineage Database Editor & Visualizer", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                {
                    // Database selection
                    GUILayout.Label("Database:", GUILayout.Width(60));
                    int newSelection = EditorGUILayout.Popup(selectedDatabaseIndex, databaseNames);
                    if (newSelection != selectedDatabaseIndex)
                    {
                        selectedDatabaseIndex = newSelection;
                        selectedItemIndex = -1;
                        selectedItem = null;
                        RefreshCurrentDatabase();
                    }

                    GUILayout.FlexibleSpace();

                    // Quick actions
                    if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                    {
                        RefreshCurrentDatabase();
                    }

                    if (GUILayout.Button("Add New", GUILayout.Width(70)))
                    {
                        CreateNewItem();
                    }

                    if (GUILayout.Button("Save All", GUILayout.Width(70)))
                    {
                        SaveAllChanges();
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Search and filters
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Search:", GUILayout.Width(50));
                    string newSearch = EditorGUILayout.TextField(searchFilter);
                    if (newSearch != searchFilter)
                    {
                        searchFilter = newSearch;
                        isDirty = true;
                    }

                    showFilters = EditorGUILayout.Toggle("Filters", showFilters, GUILayout.Width(60));
                    showStatistics = EditorGUILayout.Toggle("Stats", showStatistics, GUILayout.Width(50));
                }
                EditorGUILayout.EndHorizontal();

                if (showFilters)
                {
                    DrawAdvancedFilters();
                }

                if (showStatistics)
                {
                    DrawDatabaseStatistics();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedFilters()
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Advanced Filters", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Category:", GUILayout.Width(60));
                    categoryFilter = EditorGUILayout.TextField(categoryFilter, GUILayout.Width(120));
                    
                    GUILayout.Label("Tags:", GUILayout.Width(40));
                    tagFilter = EditorGUILayout.TextField(tagFilter, GUILayout.Width(120));
                    
                    showOnlyModified = EditorGUILayout.Toggle("Modified Only", showOnlyModified);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Stat Range:", GUILayout.Width(70));
                    minStatValue = EditorGUILayout.FloatField(minStatValue, GUILayout.Width(60));
                    GUILayout.Label("to", GUILayout.Width(20));
                    maxStatValue = EditorGUILayout.FloatField(maxStatValue, GUILayout.Width(60));
                    
                    if (GUILayout.Button("Clear Filters", GUILayout.Width(80)))
                    {
                        ClearAllFilters();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawDatabaseStatistics()
        {
            EditorGUILayout.BeginHorizontal("box");
            {
                var counts = GameData.GetDatabaseCounts();
                var currentDatabase = databaseNames[selectedDatabaseIndex];
                
                if (counts.ContainsKey(currentDatabase))
                {
                    GUILayout.Label($"Total Items: {counts[currentDatabase]}", EditorStyles.miniLabel);
                }

                GUILayout.FlexibleSpace();
                
                if (selectedItem != null)
                {
                    GUILayout.Label($"Selected: {GetItemDisplayName(selectedItem)}", EditorStyles.miniLabel);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Left Panel

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(400));
            {
                GUILayout.Label($"{databaseNames[selectedDatabaseIndex]} Database", EditorStyles.boldLabel);
                
                // View options
                EditorGUILayout.BeginHorizontal();
                {
                    useCompactView = EditorGUILayout.Toggle("Compact", useCompactView, GUILayout.Width(70));
                    showIDs = EditorGUILayout.Toggle("IDs", showIDs, GUILayout.Width(40));
                    showTags = EditorGUILayout.Toggle("Tags", showTags, GUILayout.Width(50));
                }
                EditorGUILayout.EndHorizontal();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    DrawDatabaseItemsList();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawDatabaseItemsList()
        {
            var items = GetCurrentDatabaseItems();
            var filteredItems = ApplyFilters(items);

            for (int i = 0; i < filteredItems.Count; i++)
            {
                var item = filteredItems[i];
                bool isSelected = (selectedItem != null && selectedItem.Equals(item));
                
                GUI.backgroundColor = isSelected ? highlightColor : Color.white;
                
                EditorGUILayout.BeginVertical("box");
                {
                    if (useCompactView)
                    {
                        DrawCompactItemView(item, i);
                    }
                    else
                    {
                        DrawDetailedItemView(item, i);
                    }
                }
                EditorGUILayout.EndVertical();
                
                GUI.backgroundColor = Color.white;

                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && 
                    Event.current.type == EventType.MouseDown)
                {
                    selectedItemIndex = i;
                    selectedItem = item;
                    isDirty = true;
                    Event.current.Use();
                }
            }
        }

        private void DrawCompactItemView(object item, int index)
        {
            EditorGUILayout.BeginHorizontal();
            {
                string displayName = GetItemDisplayName(item);
                GUILayout.Label(displayName, EditorStyles.label);
                
                GUILayout.FlexibleSpace();
                
                if (showIDs)
                {
                    string id = GetItemID(item);
                    GUILayout.Label(id, EditorStyles.miniLabel, GUILayout.Width(60));
                }

                if (GUILayout.Button("Edit", GUILayout.Width(40)))
                {
                    selectedItem = item;
                    selectedItemIndex = index;
                    isEditing = true;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDetailedItemView(object item, int index)
        {
            string displayName = GetItemDisplayName(item);
            string description = GetItemDescription(item);
            var tags = GetItemTags(item);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    GUILayout.Label(displayName, EditorStyles.boldLabel);
                    
                    if (!string.IsNullOrEmpty(description))
                    {
                        GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);
                    }

                    if (showTags && tags != null && tags.Count > 0)
                    {
                        string tagString = string.Join(", ", tags);
                        GUILayout.Label($"Tags: {tagString}", EditorStyles.miniLabel);
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUILayout.Width(60));
                {
                    if (showIDs)
                    {
                        string id = GetItemID(item);
                        GUILayout.Label(id, EditorStyles.centeredGreyMiniLabel);
                    }

                    if (GUILayout.Button("Edit"))
                    {
                        selectedItem = item;
                        selectedItemIndex = index;
                        isEditing = true;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Right Panel

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();
            {
                if (selectedItem != null)
                {
                    GUILayout.Label("Item Details & Editor", EditorStyles.boldLabel);
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        isEditing = EditorGUILayout.Toggle("Edit Mode", isEditing);
                        
                        GUILayout.FlexibleSpace();
                        
                        if (GUILayout.Button("Duplicate", GUILayout.Width(70)))
                        {
                            DuplicateSelectedItem();
                        }
                        
                        if (GUILayout.Button("Delete", GUILayout.Width(60)))
                        {
                            DeleteSelectedItem();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    detailScrollPosition = EditorGUILayout.BeginScrollView(detailScrollPosition);
                    {
                        DrawSelectedItemEditor();
                    }
                    EditorGUILayout.EndScrollView();

                    if (showRelationships)
                    {
                        DrawItemRelationships();
                    }
                }
                else
                {
                    GUILayout.Label("Select an item to view details", EditorStyles.centeredGreyMiniLabel);
                    
                    EditorGUILayout.BeginVertical("box");
                    {
                        GUILayout.Label("Quick Actions", EditorStyles.boldLabel);
                        
                        if (GUILayout.Button("Create New " + databaseNames[selectedDatabaseIndex].TrimEnd('s')))
                        {
                            CreateNewItem();
                        }
                        
                        if (GUILayout.Button("Import from CSV"))
                        {
                            ImportFromCSV();
                        }
                        
                        if (GUILayout.Button("Export to CSV"))
                        {
                            ExportToCSV();
                        }
                        
                        GUILayout.Space(10);
                        
                        showRelationships = EditorGUILayout.Toggle("Show Relationships", showRelationships);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedItemEditor()
        {
            if (selectedItem == null) return;

            switch (selectedDatabaseIndex)
            {
                case 0: DrawEntityEditor((Entity)selectedItem); break;
                case 1: DrawItemEditor((Item)selectedItem); break;
                case 2: DrawTraitEditor((Trait)selectedItem); break;
                case 3: DrawQuestEditor((Quest)selectedItem); break;
                case 4: DrawNPCEditor((NPC)selectedItem); break;
                case 5: DrawLoreEntryEditor((LoreEntry)selectedItem); break;
                case 6: DrawSkillEditor((Skill)selectedItem); break;
                case 7: DrawBuffEditor((Buff)selectedItem); break;
                case 8: DrawObjectiveEditor((Objective)selectedItem); break;
                case 9: DrawStatEditor((Stat)selectedItem); break;
                case 10: DrawGeneticsEditor((Genetics)selectedItem); break;
                case 11: DrawJournalEntryEditor((JournalEntry)selectedItem); break;
            }
        }

        #endregion

        #region Item Editors

        private void DrawEntityEditor(Entity entity)
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Entity Properties", EditorStyles.boldLabel);

                if (isEditing)
                {
                    // Edit mode - show editable fields
                    GUILayout.Label("Name:");
                    entity.entityName = EditorGUILayout.TextField(entity.entityName);
                    
                    GUILayout.Label("Health:");
                    entity.health = DrawStatField(entity.health);
                    
                    GUILayout.Label("Mana:");
                    entity.mana = DrawStatField(entity.mana);
                    
                    GUILayout.Label("Attack:");
                    entity.attack = DrawStatField(entity.attack);
                    
                    GUILayout.Label("Defense:");
                    entity.defense = DrawStatField(entity.defense);
                    
                    GUILayout.Label("Speed:");
                    entity.speed = DrawStatField(entity.speed);
                    
                    GUILayout.Label("Tags:");
                    DrawTagsEditor(entity.tags);
                }
                else
                {
                    // View mode - show read-only info
                    EditorGUILayout.LabelField("Name:", entity.entityName);
                    EditorGUILayout.LabelField("ID:", entity.entityID.ToString());
                    EditorGUILayout.LabelField("Health:", $"{entity.health.current}/{entity.health.max}");
                    EditorGUILayout.LabelField("Mana:", $"{entity.mana.currentValue}/{entity.mana.maxValue}");
                    EditorGUILayout.LabelField("Attack:", entity.attack.currentValue.ToString());
                    EditorGUILayout.LabelField("Defense:", entity.defense.currentValue.ToString());
                    EditorGUILayout.LabelField("Speed:", entity.speed.currentValue.ToString());

                    if (entity.tags != null && entity.tags.Count > 0)
                    {
                        EditorGUILayout.LabelField("Tags:", string.Join(", ", entity.tags));
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawItemEditor(Item item)
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Item Properties", EditorStyles.boldLabel);

                if (isEditing)
                {
                    GUILayout.Label("Name:");
                    item.itemName = EditorGUILayout.TextField(item.itemName);
                    
                    GUILayout.Label("Type:");
                    item.itemType = (Item.ItemType)EditorGUILayout.EnumPopup(item.itemType);
                    
                    GUILayout.Label("Weight:");
                    item.weight = EditorGUILayout.FloatField(item.weight);
                    
                    GUILayout.Label("Value:");
                    item.value = EditorGUILayout.IntField(item.value);
                    
                    GUILayout.Label("Rarity:");
                    item.itemRarity = (Item.ItemRarity)EditorGUILayout.EnumPopup(item.itemRarity);
                    
                    GUILayout.Label("Quality:");
                    item.itemQuality = (Item.ItemQuality)EditorGUILayout.EnumPopup(item.itemQuality);
                    
                    GUILayout.Label("Tags:");
                    DrawTagsEditor(item.tags);
                }
                else
                {
                    EditorGUILayout.LabelField("Name:", item.itemName);
                    EditorGUILayout.LabelField("ID:", item.itemID.ToString());
                    EditorGUILayout.LabelField("Type:", item.itemType.ToString());
                    EditorGUILayout.LabelField("Weight:", item.weight.ToString("F2"));
                    EditorGUILayout.LabelField("Value:", item.value.ToString());
                    EditorGUILayout.LabelField("Rarity:", item.itemRarity.ToString());
                    EditorGUILayout.LabelField("Quality:", item.itemQuality.ToString());
                    
                    if (item.tags != null && item.tags.Count > 0)
                    {
                        EditorGUILayout.LabelField("Tags:", string.Join(", ", item.tags));
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTraitEditor(Trait trait)
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Trait Properties", EditorStyles.boldLabel);

                if (isEditing)
                {
                    GUILayout.Label("Name:");
                    var newTraitName = EditorGUILayout.TextField(trait.traitName);
                    
                    GUILayout.Label("Description:");
                    var newDescription = EditorGUILayout.TextArea(trait.description, GUILayout.Height(60));
                    
                    GUILayout.Label("Category:");
                    var newCategory = EditorGUILayout.TextField(trait.category);
                    
                    GUILayout.Label("Tags:");
                    DrawTagsEditor(trait.tags);

                    // Create new trait with updated values if changes were made
                    if (newTraitName != trait.traitName || newDescription != trait.description || newCategory != trait.category)
                    {
                        var updatedTrait = new Trait(
                            name: newTraitName,
                            id: trait.traitID,
                            description: newDescription,
                            category: newCategory
                        );
                        selectedItem = updatedTrait;
                        isDirty = true;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Name:", trait.traitName);
                    EditorGUILayout.LabelField("ID:", trait.traitID.ToString());
                    EditorGUILayout.LabelField("Category:", trait.category);
                    
                    if (!string.IsNullOrEmpty(trait.description))
                    {
                        GUILayout.Label("Description:");
                        EditorGUILayout.TextArea(trait.description, EditorStyles.wordWrappedLabel, GUILayout.Height(60));
                    }
                    
                    if (trait.tags != null && trait.tags.Count > 0)
                    {
                        EditorGUILayout.LabelField("Tags:", string.Join(", ", trait.tags));
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawQuestEditor(Quest quest)
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Quest Properties", EditorStyles.boldLabel);

                if (isEditing)
                {
                    GUILayout.Label("Name:");
                    quest.questName = EditorGUILayout.TextField(quest.questName);
                    
                    GUILayout.Label("Description:");
                    quest.description = EditorGUILayout.TextArea(quest.description, GUILayout.Height(60));
                    
                    GUILayout.Label("Type:");
                    quest.questType = (Quest.Type)EditorGUILayout.EnumPopup(quest.questType);
                    
                    GUILayout.Label("Status:");
                    quest.status = (Quest.Status)EditorGUILayout.EnumPopup(quest.status);
                    
                    GUILayout.Label("Experience Reward:");
                    quest.experienceReward = EditorGUILayout.IntField(quest.experienceReward);
                }
                else
                {
                    EditorGUILayout.LabelField("Name:", quest.questName);
                    EditorGUILayout.LabelField("ID:", quest.questID.ToString());
                    EditorGUILayout.LabelField("Type:", quest.questType.ToString());
                    EditorGUILayout.LabelField("Status:", quest.status.ToString());
                    EditorGUILayout.LabelField("Experience Reward:", quest.experienceReward.ToString());
                    
                    if (!string.IsNullOrEmpty(quest.description))
                    {
                        GUILayout.Label("Description:");
                        EditorGUILayout.TextArea(quest.description, EditorStyles.wordWrappedLabel, GUILayout.Height(60));
                    }
                    
                    if (quest.objectives != null && quest.objectives.Count > 0)
                    {
                        EditorGUILayout.LabelField("Objectives:", quest.objectives.Count.ToString());
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawNPCEditor(NPC npc)
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("NPC Properties", EditorStyles.boldLabel);

                if (isEditing)
                {
                    GUILayout.Label("Name:");
                    npc.npcName = EditorGUILayout.TextField(npc.npcName);
                    
                    GUILayout.Label("Archetype:");
                    npc.archetype = (NPC.Archetype)EditorGUILayout.EnumPopup(npc.archetype);
                }
                else
                {
                    EditorGUILayout.LabelField("Name:", npc.npcName);
                    EditorGUILayout.LabelField("Archetype:", npc.archetype.ToString());
                    
                    if (npc.availableQuests != null && npc.availableQuests.Count > 0)
                    {
                        EditorGUILayout.LabelField("Available Quests:", npc.availableQuests.Count.ToString());
                    }
                    
                    if (npc.dialogueKeys != null && npc.dialogueKeys.Count > 0)
                    {
                        EditorGUILayout.LabelField("Dialogue Keys:", npc.dialogueKeys.Count.ToString());
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawLoreEntryEditor(LoreEntry lore)
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Lore Entry Properties", EditorStyles.boldLabel);

                if (isEditing)
                {
                    GUILayout.Label("Title:");
                    lore.title = EditorGUILayout.TextField(lore.title);
                    
                    GUILayout.Label("Category:");
                    lore.category = (LoreEntry.LegacyCategory)EditorGUILayout.EnumPopup(lore.category);
                    
                    GUILayout.Label("Content:");
                    lore.content = EditorGUILayout.TextArea(lore.content, GUILayout.Height(100));
                    
                    lore.isDiscovered = EditorGUILayout.Toggle("Is Discovered:", lore.isDiscovered);
                }
                else
                {
                    EditorGUILayout.LabelField("Title:", lore.title);
                    EditorGUILayout.LabelField("Category:", lore.category.ToString());
                    EditorGUILayout.LabelField("Discovered:", lore.isDiscovered ? "Yes" : "No");
                    
                    if (!string.IsNullOrEmpty(lore.content))
                    {
                        GUILayout.Label("Content:");
                        EditorGUILayout.TextArea(lore.content, EditorStyles.wordWrappedLabel, GUILayout.Height(100));
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        // Placeholder implementations for other editors
        private void DrawSkillEditor(Skill skill) { /* Implementation for Skill editing */ }
        private void DrawBuffEditor(Buff buff) { /* Implementation for Buff editing */ }
        private void DrawObjectiveEditor(Objective objective) { /* Implementation for Objective editing */ }
        private void DrawStatEditor(Stat stat) { /* Implementation for Stat editing */ }
        private void DrawGeneticsEditor(Genetics genetics) { /* Implementation for Genetics editing */ }
        private void DrawJournalEntryEditor(JournalEntry journal) { /* Implementation for Journal editing */ }

        #endregion

        #region Helper Methods

        private Health DrawStatField(Health health)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Current:", GUILayout.Width(60));
                health.current = EditorGUILayout.FloatField(health.current, GUILayout.Width(80));
                GUILayout.Label("Max:", GUILayout.Width(35));
                health.max = EditorGUILayout.FloatField(health.max, GUILayout.Width(80));
            }
            EditorGUILayout.EndHorizontal();
            return health;
        }

        private Stat DrawStatField(Stat stat)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Current:", GUILayout.Width(60));
                stat.currentValue = EditorGUILayout.FloatField(stat.currentValue, GUILayout.Width(80));
                GUILayout.Label("Min:", GUILayout.Width(35));
                stat.minValue = EditorGUILayout.FloatField(stat.minValue, GUILayout.Width(60));
                GUILayout.Label("Max:", GUILayout.Width(35));
                stat.maxValue = EditorGUILayout.FloatField(stat.maxValue, GUILayout.Width(80));
            }
            EditorGUILayout.EndHorizontal();
            return stat;
        }

        private void DrawTagsEditor(List<string> tags)
        {
            if (tags == null) tags = new List<string>();
            
            for (int i = 0; i < tags.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    tags[i] = EditorGUILayout.TextField(tags[i]);
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        tags.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Tag"))
            {
                tags.Add("");
            }
        }

        private void DrawItemRelationships()
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Relationships", EditorStyles.boldLabel);
                // Implementation for showing item relationships
                GUILayout.Label("Related items and dependencies will be shown here.", EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal("box");
            {
                GUILayout.Label($"Database: {databaseNames[selectedDatabaseIndex]}", EditorStyles.miniLabel);
                
                GUILayout.FlexibleSpace();
                
                if (isDirty)
                {
                    GUILayout.Label("Changes pending...", EditorStyles.miniLabel);
                }
                
                GUILayout.Label($"Unity {Application.unityVersion}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Database Operations

        private List<object> GetCurrentDatabaseItems()
        {
            switch (selectedDatabaseIndex)
            {
                case 0: return GameData.entityDatabase.Cast<object>().ToList();
                case 1: return GameData.itemDatabase.Cast<object>().ToList();
                case 2: return GameData.traitDatabase.Cast<object>().ToList();
                case 3: return GameData.questDatabase.Cast<object>().ToList();
                case 4: return GameData.npcDatabase.Cast<object>().ToList();
                case 5: return GameData.loreDatabase.Cast<object>().ToList();
                case 6: return GameData.skillDatabase.Cast<object>().ToList();
                case 7: return GameData.buffDatabase.Cast<object>().ToList();
                case 8: return GameData.objectiveDatabase.Cast<object>().ToList();
                case 9: return GameData.statDatabase.Cast<object>().ToList();
                case 10: return GameData.geneticsDatabase.Cast<object>().ToList();
                case 11: return GameData.journalDatabase.Cast<object>().ToList();
                default: return new List<object>();
            }
        }

        private List<object> ApplyFilters(List<object> items)
        {
            var filtered = items;

            if (!string.IsNullOrEmpty(searchFilter))
            {
                filtered = filtered.Where(item => 
                    GetItemDisplayName(item).ToLower().Contains(searchFilter.ToLower()) ||
                    GetItemDescription(item).ToLower().Contains(searchFilter.ToLower())
                ).ToList();
            }

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                filtered = filtered.Where(item => 
                    GetItemCategory(item).ToLower().Contains(categoryFilter.ToLower())
                ).ToList();
            }

            if (!string.IsNullOrEmpty(tagFilter))
            {
                filtered = filtered.Where(item => 
                {
                    var tags = GetItemTags(item);
                    return tags != null && tags.Any(tag => tag.ToLower().Contains(tagFilter.ToLower()));
                }).ToList();
            }

            return filtered;
        }

        private string GetItemDisplayName(object item)
        {
            switch (item)
            {
                case Entity entity: return entity.entityName;
                case Item gameItem: return gameItem.itemName;
                case Trait trait: return trait.traitName;
                case Quest quest: return quest.questName;
                case NPC npc: return npc.npcName;
                case LoreEntry lore: return lore.title;
                case Skill skill: return skill.skillName.ToString();
                case Buff buff: return buff.buffName;
                case Objective objective: return objective.objectiveName;
                case Stat stat: return stat.statName;
      //          case Genetics genetics: return genetics.geneName.ToString(); //todo: We'll need to add this field to the Genetics class in the Database.
                case JournalEntry journal: return journal.title;
                default: return "Unknown Item";
            }
        }

        private string GetItemDescription(object item)
        {
            switch (item)
            {
                case Entity entity: return $"ID: {entity.entityID}";
                case Item gameItem: return $"Type: {gameItem.itemType}";
                case Trait trait: return trait.description;
                case Quest quest: return quest.description;
                case NPC npc: return $"Archetype: {npc.archetype}";
                case LoreEntry lore: return lore.content;
 //               case Skill skill: return skill.skillDescription; //todo: we'll need to add this field to the Skill class in the Database.
                case Buff buff: return buff.buffDescription;
                case Objective objective: return objective.description;
                case Stat stat: return stat.statDescription;
 //               case Genetics genetics: return genetics.description;
                case JournalEntry journal: return journal.content;
                default: return "";
            }
        }

        private string GetItemID(object item)
        {
            switch (item)
            {
                case Entity entity: return entity.entityID.ToString();
                case Item gameItem: return gameItem.itemID.ToString();
                case Trait trait: return trait.traitID.ToString();
                case Quest quest: return quest.questID.ToString();
                case NPC npc: return npc.npcName; // NPCs use name as ID
                case LoreEntry lore: return lore.title; // Lore uses title as ID
                case Skill skill: return skill.skillID.ToString();
                case Buff buff: return buff.buffID.ToString();
                case Objective objective: return objective.objectiveID.ToString();
                case Stat stat: return stat.statID.ToString();
            //  case Genetics genetics: return genetics.geneID.ToString(); // todo: We'll need to add this field to the Genetics class in the Database.
                case JournalEntry journal: return journal.title; // Journal uses title as ID
                default: return "Unknown";
            }
        }

        private List<string> GetItemTags(object item)
        {
            switch (item)
            {
                case Entity entity: return entity.tags;
                case Item gameItem: return gameItem.tags;
                case Trait trait: return trait.tags;
                case Skill skill: return skill.tags;
                case Buff buff: return buff.tags;
                case Objective objective: return objective.tags;
                default: return new List<string>();
            }
        }

        private string GetItemCategory(object item)
        {
            switch (item)
            {
                case Trait trait: return trait.category;
                case Quest quest: return quest.questType.ToString();
                case NPC npc: return npc.archetype.ToString();
                case LoreEntry lore: return lore.category.ToString();
                case Item gameItem: return gameItem.itemType.ToString();
                default: return "";
            }
        }

        private void RefreshDatabaseCounts()
        {
            // Refresh the database counts for display
            isDirty = true;
        }

        private void RefreshCurrentDatabase()
        {
            // Refresh the current database view
            isDirty = true;
        }

        private void CreateNewItem()
        {
            // Implementation for creating new items
            Lineage.Ancestral.Legacies.Debug.Log.Info($"Creating new {databaseNames[selectedDatabaseIndex]} item", Ancestral.Legacies.Debug.Log.LogCategory.Systems);
        }

        private void DuplicateSelectedItem()
        {
            // Implementation for duplicating items
            Lineage.Ancestral.Legacies.Debug.Log.Info("Duplicating selected item", Ancestral.Legacies.Debug.Log.LogCategory.Systems);
        }

        private void DeleteSelectedItem()
        {
            if (EditorUtility.DisplayDialog("Delete Item", 
                $"Are you sure you want to delete {GetItemDisplayName(selectedItem)}?", 
                "Delete", "Cancel"))
            {
                // Implementation for deleting items
                Lineage.Ancestral.Legacies.Debug.Log.Info("Deleting selected item", Ancestral.Legacies.Debug.Log.LogCategory.Systems);
                selectedItem = null;
                selectedItemIndex = -1;
                isDirty = true;
            }
        }

        private void SaveAllChanges()
        {
            // Implementation for saving all changes
            Lineage.Ancestral.Legacies.Debug.Log.Info("Saving all database changes", Ancestral.Legacies.Debug.Log.LogCategory.Systems);
        }

        private void ClearAllFilters()
        {
            searchFilter = "";
            categoryFilter = "";
            tagFilter = "";
            minStatValue = 0f;
            maxStatValue = 100f;
            showOnlyModified = false;
            isDirty = true;
        }

        private void ImportFromCSV()
        {
            // Implementation for CSV import
            Lineage.Ancestral.Legacies.Debug.Log.Info("Importing from CSV", Ancestral.Legacies.Debug.Log.LogCategory.Systems);
        }

        private void ExportToCSV()
        {
            // Implementation for CSV export
            Lineage.Ancestral.Legacies.Debug.Log.Info("Exporting to CSV", Ancestral.Legacies.Debug.Log.LogCategory.Systems);
        }

        #endregion
    }
}

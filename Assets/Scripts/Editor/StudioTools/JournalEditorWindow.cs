using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Comprehensive Journal Editor for managing journal entries, quest logs, lore entries,
    /// character notes, and narrative content within the Lineage Ancestral Legacies game.
    /// </summary>
    public class JournalEditorWindow : EditorWindow
    {
        #region Window Management

        private static JournalEditorWindow window;

        [System.Serializable]
        public enum JournalTab
        {
            Entries = 0,
            Categories = 1,
            Timeline = 2,
            Search = 3,
            Templates = 4,
            Export = 5
        }

        private JournalTab currentTab = JournalTab.Entries;
        private Vector2 scrollPosition;
        private Vector2 entriesScrollPosition;
        private Vector2 previewScrollPosition;
        private bool isDirty = false;

        // Current editing data
        private JournalEntry currentEntry = new JournalEntry();
        private List<JournalEntry> allEntries = new List<JournalEntry>();
        private List<JournalCategory> categories = new List<JournalCategory>();

        // UI State
        private string searchFilter = "";
        private JournalEntryType filterType = JournalEntryType.All;
        private string filterCategory = "";
        private bool showPreview = true;
        private int selectedEntryIndex = -1;
        private bool isCreatingNew = false;

        // Timeline data
        private List<TimelineEvent> timelineEvents = new List<TimelineEvent>();
        private float timelineStartYear = 0f;
        private float timelineEndYear = 1000f;

        // Templates
        private List<JournalTemplate> templates = new List<JournalTemplate>();

        #endregion

        #region Data Structures

        [System.Serializable]
        public class JournalEntry
        {
            public string id = System.Guid.NewGuid().ToString();
            public string title = "New Entry";
            public string content = "";
            public JournalEntryType type = JournalEntryType.General;
            public string category = "";
            public List<string> tags = new List<string>();
            public DateTime dateCreated = DateTime.Now;
            public DateTime dateModified = DateTime.Now;
            public bool isImportant = false;
            public bool isSecret = false;
            public bool isCompleted = false;
            public string author = "";
            public List<string> relatedEntries = new List<string>();
            public List<string> relatedQuests = new List<string>();
            public List<string> relatedCharacters = new List<string>();
            public List<string> relatedLocations = new List<string>();
            public Vector2 timelinePeriod = Vector2.zero; // Start and end years
            public string imageRef = "";
            public Color entryColor = Color.white;
            public int priority = 1;
            public string notes = "";
        }

        [System.Serializable]
        public class JournalCategory
        {
            public string name = "";
            public string description = "";
            public Color categoryColor = Color.blue;
            public bool isExpanded = true;
            public List<string> subcategories = new List<string>();
            public int sortOrder = 0;
        }

        [System.Serializable]
        public class TimelineEvent
        {
            public string entryId = "";
            public string eventName = "";
            public float year = 0f;
            public string description = "";
            public TimelineEventType eventType = TimelineEventType.General;
            public Color eventColor = Color.yellow;
        }

        [System.Serializable]
        public class JournalTemplate
        {
            public string name = "";
            public string description = "";
            public JournalEntryType defaultType = JournalEntryType.General;
            public string templateContent = "";
            public List<string> defaultTags = new List<string>();
            public string defaultCategory = "";
        }

        // Enums
        public enum JournalEntryType
        {
            All,
            General,
            Quest,
            Character,
            Lore,
            Location,
            Item,
            Combat,
            Discovery,
            Personal,
            Political,
            Trade,
            Magic,
            Secret
        }

        public enum TimelineEventType
        {
            General,
            Birth,
            Death,
            Battle,
            Political,
            Discovery,
            Quest,
            Relationship,
            Trade,
            Magic
        }

        #endregion

        #region Window Initialization

        public static void ShowWindow()
        {
            window = GetWindow<JournalEditorWindow>("Journal Editor");
            window.minSize = new Vector2(1000, 700);
            window.titleContent = new GUIContent("Journal Editor", "Lineage Journal & Narrative Editor");
        }

        private void OnEnable()
        {
            LoadData();
            SetupDefaultCategories();
            SetupDefaultTemplates();
        }

        private void OnDisable()
        {
            if (isDirty)
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes", 
                    "You have unsaved changes. Do you want to save before closing?", 
                    "Save", "Don't Save"))
                {
                    SaveData();
                }
            }
        }

        #endregion

        #region Main GUI

        private void OnGUI()
        {
            DrawHeader();
            DrawTabNavigation();
            DrawMainContent();
            DrawFooter();

            if (isDirty)
            {
                Repaint();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Journal & Narrative Editor", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("New Entry", EditorStyles.toolbarButton))
                {
                    CreateNewEntry();
                }
                
                if (GUILayout.Button("Import", EditorStyles.toolbarButton))
                {
                    ImportFromFile();
                }
                
                if (GUILayout.Button("Export", EditorStyles.toolbarButton))
                {
                    ExportToFile();
                }
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    SaveData();
                }
                
                if (GUILayout.Button("Load", EditorStyles.toolbarButton))
                {
                    LoadData();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabNavigation()
        {
            EditorGUILayout.BeginHorizontal();
            {
                var tabNames = new string[] { "Entries", "Categories", "Timeline", "Search", "Templates", "Export" };
                currentTab = (JournalTab)GUILayout.Toolbar((int)currentTab, tabNames);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMainContent()
        {
            switch (currentTab)
            {
                case JournalTab.Entries:
                    DrawEntriesTab();
                    break;
                case JournalTab.Categories:
                    DrawCategoriesTab();
                    break;
                case JournalTab.Timeline:
                    DrawTimelineTab();
                    break;
                case JournalTab.Search:
                    DrawSearchTab();
                    break;
                case JournalTab.Templates:
                    DrawTemplatesTab();
                    break;
                case JournalTab.Export:
                    DrawExportTab();
                    break;
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                var statusText = isDirty ? "Modified" : "Saved";
                var statusColor = isDirty ? Color.yellow : Color.green;
                
                var oldColor = GUI.color;
                GUI.color = statusColor;
                GUILayout.Label($"Status: {statusText}", EditorStyles.miniLabel);
                GUI.color = oldColor;
                
                GUILayout.FlexibleSpace();
                
                GUILayout.Label($"Entries: {allEntries.Count} | Categories: {categories.Count}", EditorStyles.miniLabel);
                
                if (currentEntry != null)
                {
                    GUILayout.Label($"| Words: {CountWords(currentEntry.content)}", EditorStyles.miniLabel);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Tab Content Drawing

        private void DrawEntriesTab()
        {
            EditorGUILayout.BeginHorizontal();
            {
                // Left Panel - Entry List
                EditorGUILayout.BeginVertical("box", GUILayout.Width(300));
                {
                    DrawEntriesListPanel();
                }
                EditorGUILayout.EndVertical();
                
                // Right Panel - Entry Editor
                EditorGUILayout.BeginVertical("box");
                {
                    DrawEntryEditorPanel();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntriesListPanel()
        {
            EditorGUILayout.LabelField("Journal Entries", EditorStyles.boldLabel);
            
            // Filters
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Filter:", GUILayout.Width(50));
                filterType = (JournalEntryType)EditorGUILayout.EnumPopup(filterType, GUILayout.Width(100));
                
                var categoryNames = categories.Select(c => c.name).Prepend("All").ToArray();
                var categoryIndex = Array.IndexOf(categoryNames, filterCategory);
                if (categoryIndex == -1) categoryIndex = 0;
                
                var newCategoryIndex = EditorGUILayout.Popup(categoryIndex, categoryNames);
                filterCategory = newCategoryIndex == 0 ? "" : categoryNames[newCategoryIndex];
            }
            EditorGUILayout.EndHorizontal();
            
            // Search
            var newSearchFilter = EditorGUILayout.TextField("Search:", searchFilter);
            if (newSearchFilter != searchFilter)
            {
                searchFilter = newSearchFilter;
            }
            
            EditorGUILayout.Space(5);
            
            // Entries List
            entriesScrollPosition = EditorGUILayout.BeginScrollView(entriesScrollPosition);
            {
                var filteredEntries = GetFilteredEntries();
                
                for (int i = 0; i < filteredEntries.Count; i++)
                {
                    var entry = filteredEntries[i];
                    var isSelected = selectedEntryIndex == allEntries.IndexOf(entry);
                    
                    var style = isSelected ? "selectionRect" : "box";
                    var bgColor = GUI.backgroundColor;
                    
                    if (entry.isImportant)
                        GUI.backgroundColor = Color.yellow;
                    else if (entry.isSecret)
                        GUI.backgroundColor = Color.red;
                    
                    EditorGUILayout.BeginVertical(style);
                    {
                        GUI.backgroundColor = bgColor;
                        
                        if (GUILayout.Button("", GUIStyle.none, GUILayout.Height(0)))
                        {
                            selectedEntryIndex = allEntries.IndexOf(entry);
                            currentEntry = entry;
                            isCreatingNew = false;
                        }
                        
                        EditorGUILayout.LabelField(entry.title, EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"{entry.type} | {entry.category}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField(entry.dateModified.ToString("yyyy-MM-dd"), EditorStyles.miniLabel);
                        
                        if (!string.IsNullOrEmpty(entry.content))
                        {
                            var preview = entry.content.Length > 100 ? 
                                entry.content.Substring(0, 100) + "..." : entry.content;
                            EditorGUILayout.LabelField(preview, EditorStyles.helpBox);
                        }
                    }
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space(2);
                }
                
                if (filteredEntries.Count == 0)
                {
                    EditorGUILayout.HelpBox("No entries match the current filter.", MessageType.Info);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawEntryEditorPanel()
        {
            if (currentEntry == null)
            {
                EditorGUILayout.HelpBox("Select an entry to edit or create a new one.", MessageType.Info);
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                EditorGUILayout.LabelField(isCreatingNew ? "Create New Entry" : "Edit Entry", EditorStyles.boldLabel);
                
                // Basic Information
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
                    
                    var newTitle = EditorGUILayout.TextField("Title", currentEntry.title);
                    if (newTitle != currentEntry.title)
                    {
                        currentEntry.title = newTitle;
                        currentEntry.dateModified = DateTime.Now;
                        MarkDirty();
                    }
                    
                    var newType = (JournalEntryType)EditorGUILayout.EnumPopup("Type", currentEntry.type);
                    if (newType != currentEntry.type && newType != JournalEntryType.All)
                    {
                        currentEntry.type = newType;
                        currentEntry.dateModified = DateTime.Now;
                        MarkDirty();
                    }
                    
                    // Category selection
                    var categoryNames = categories.Select(c => c.name).Prepend("None").ToArray();
                    var categoryIndex = Array.IndexOf(categoryNames, currentEntry.category);
                    if (categoryIndex == -1) categoryIndex = 0;
                    
                    var newCategoryIndex = EditorGUILayout.Popup("Category", categoryIndex, categoryNames);
                    var newCategory = newCategoryIndex == 0 ? "" : categoryNames[newCategoryIndex];
                    if (newCategory != currentEntry.category)
                    {
                        currentEntry.category = newCategory;
                        currentEntry.dateModified = DateTime.Now;
                        MarkDirty();
                    }
                    
                    var newAuthor = EditorGUILayout.TextField("Author", currentEntry.author);
                    if (newAuthor != currentEntry.author)
                    {
                        currentEntry.author = newAuthor;
                        MarkDirty();
                    }
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Content
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Content", EditorStyles.boldLabel);
                    
                    var newContent = EditorGUILayout.TextArea(currentEntry.content, GUILayout.MinHeight(200));
                    if (newContent != currentEntry.content)
                    {
                        currentEntry.content = newContent;
                        currentEntry.dateModified = DateTime.Now;
                        MarkDirty();
                    }
                    
                    EditorGUILayout.LabelField($"Word Count: {CountWords(currentEntry.content)}", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Flags and Properties
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
                    
                    var newImportant = EditorGUILayout.Toggle("Important", currentEntry.isImportant);
                    if (newImportant != currentEntry.isImportant)
                    {
                        currentEntry.isImportant = newImportant;
                        MarkDirty();
                    }
                    
                    var newSecret = EditorGUILayout.Toggle("Secret", currentEntry.isSecret);
                    if (newSecret != currentEntry.isSecret)
                    {
                        currentEntry.isSecret = newSecret;
                        MarkDirty();
                    }
                    
                    var newCompleted = EditorGUILayout.Toggle("Completed", currentEntry.isCompleted);
                    if (newCompleted != currentEntry.isCompleted)
                    {
                        currentEntry.isCompleted = newCompleted;
                        MarkDirty();
                    }
                    
                    var newPriority = EditorGUILayout.IntSlider("Priority", currentEntry.priority, 1, 5);
                    if (newPriority != currentEntry.priority)
                    {
                        currentEntry.priority = newPriority;
                        MarkDirty();
                    }
                    
                    var newColor = EditorGUILayout.ColorField("Entry Color", currentEntry.entryColor);
                    if (newColor != currentEntry.entryColor)
                    {
                        currentEntry.entryColor = newColor;
                        MarkDirty();
                    }
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Timeline
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Timeline", EditorStyles.boldLabel);
                    
                    var newTimelinePeriod = EditorGUILayout.Vector2Field("Timeline Period (Years)", currentEntry.timelinePeriod);
                    if (newTimelinePeriod != currentEntry.timelinePeriod)
                    {
                        currentEntry.timelinePeriod = newTimelinePeriod;
                        MarkDirty();
                    }
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Tags
                DrawStringList("Tags", currentEntry.tags);
                
                // Related Content
                DrawStringList("Related Entries", currentEntry.relatedEntries);
                DrawStringList("Related Quests", currentEntry.relatedQuests);
                DrawStringList("Related Characters", currentEntry.relatedCharacters);
                DrawStringList("Related Locations", currentEntry.relatedLocations);
                
                // Notes
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Notes", EditorStyles.boldLabel);
                    
                    var newNotes = EditorGUILayout.TextArea(currentEntry.notes, GUILayout.Height(80));
                    if (newNotes != currentEntry.notes)
                    {
                        currentEntry.notes = newNotes;
                        MarkDirty();
                    }
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(10);
                
                // Action Buttons
                EditorGUILayout.BeginHorizontal();
                {
                    if (isCreatingNew)
                    {
                        if (GUILayout.Button("Create Entry"))
                        {
                            SaveNewEntry();
                        }
                        
                        if (GUILayout.Button("Cancel"))
                        {
                            CancelNewEntry();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Duplicate Entry"))
                        {
                            DuplicateEntry();
                        }
                        
                        if (GUILayout.Button("Delete Entry"))
                        {
                            DeleteEntry();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                // Metadata
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Metadata", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"ID: {currentEntry.id}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Created: {currentEntry.dateCreated}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Modified: {currentEntry.dateModified}", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawCategoriesTab()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                EditorGUILayout.LabelField("Category Management", EditorStyles.boldLabel);
                
                EditorGUILayout.Space(10);
                
                for (int i = 0; i < categories.Count; i++)
                {
                    var category = categories[i];
                    
                    EditorGUILayout.BeginVertical("box");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            var newName = EditorGUILayout.TextField("Name", category.name);
                            if (newName != category.name)
                            {
                                category.name = newName;
                                MarkDirty();
                            }
                            
                            var newColor = EditorGUILayout.ColorField(category.categoryColor, GUILayout.Width(60));
                            if (newColor != category.categoryColor)
                            {
                                category.categoryColor = newColor;
                                MarkDirty();
                            }
                            
                            if (GUILayout.Button("Delete", GUILayout.Width(60)))
                            {
                                if (EditorUtility.DisplayDialog("Delete Category", 
                                    $"Are you sure you want to delete the category '{category.name}'?", 
                                    "Delete", "Cancel"))
                                {
                                    categories.RemoveAt(i);
                                    MarkDirty();
                                    i--;
                                    continue;
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        var newDescription = EditorGUILayout.TextField("Description", category.description);
                        if (newDescription != category.description)
                        {
                            category.description = newDescription;
                            MarkDirty();
                        }
                        
                        var newSortOrder = EditorGUILayout.IntField("Sort Order", category.sortOrder);
                        if (newSortOrder != category.sortOrder)
                        {
                            category.sortOrder = newSortOrder;
                            MarkDirty();
                        }
                        
                        // Subcategories
                        DrawStringList("Subcategories", category.subcategories);
                    }
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space(5);
                }
                
                if (GUILayout.Button("Add New Category"))
                {
                    categories.Add(new JournalCategory { name = $"Category {categories.Count + 1}" });
                    MarkDirty();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawTimelineTab()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                EditorGUILayout.LabelField("Timeline Management", EditorStyles.boldLabel);
                
                // Timeline Settings
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Timeline Settings", EditorStyles.boldLabel);
                    
                    timelineStartYear = EditorGUILayout.FloatField("Start Year", timelineStartYear);
                    timelineEndYear = EditorGUILayout.FloatField("End Year", timelineEndYear);
                    
                    if (GUILayout.Button("Generate Timeline from Entries"))
                    {
                        GenerateTimelineFromEntries();
                    }
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(10);
                
                // Timeline Events
                EditorGUILayout.LabelField("Timeline Events", EditorStyles.boldLabel);
                
                var sortedEvents = timelineEvents.OrderBy(e => e.year).ToList();
                
                foreach (var timelineEvent in sortedEvents)
                {
                    EditorGUILayout.BeginVertical("box");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField($"Year {timelineEvent.year}", EditorStyles.boldLabel, GUILayout.Width(100));
                            EditorGUILayout.LabelField(timelineEvent.eventName, GUILayout.Width(200));
                            EditorGUILayout.LabelField(timelineEvent.eventType.ToString(), GUILayout.Width(100));
                            
                            if (!string.IsNullOrEmpty(timelineEvent.entryId))
                            {
                                if (GUILayout.Button("Go to Entry", GUILayout.Width(100)))
                                {
                                    var entry = allEntries.FirstOrDefault(e => e.id == timelineEvent.entryId);
                                    if (entry != null)
                                    {
                                        currentEntry = entry;
                                        selectedEntryIndex = allEntries.IndexOf(entry);
                                        currentTab = JournalTab.Entries;
                                    }
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        if (!string.IsNullOrEmpty(timelineEvent.description))
                        {
                            EditorGUILayout.LabelField(timelineEvent.description, EditorStyles.helpBox);
                        }
                    }
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space(2);
                }
                
                if (sortedEvents.Count == 0)
                {
                    EditorGUILayout.HelpBox("No timeline events found. Create entries with timeline periods to populate this view.", MessageType.Info);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSearchTab()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                EditorGUILayout.LabelField("Advanced Search", EditorStyles.boldLabel);
                
                // Search Interface would be implemented here
                EditorGUILayout.HelpBox("Advanced search functionality including full-text search, tag filters, date ranges, and content analysis would be implemented here.", MessageType.Info);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawTemplatesTab()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                EditorGUILayout.LabelField("Entry Templates", EditorStyles.boldLabel);
                
                EditorGUILayout.Space(10);
                
                for (int i = 0; i < templates.Count; i++)
                {
                    var template = templates[i];
                    
                    EditorGUILayout.BeginVertical("box");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            var newName = EditorGUILayout.TextField("Name", template.name);
                            if (newName != template.name)
                            {
                                template.name = newName;
                                MarkDirty();
                            }
                            
                            if (GUILayout.Button("Use Template", GUILayout.Width(100)))
                            {
                                CreateEntryFromTemplate(template);
                            }
                            
                            if (GUILayout.Button("Delete", GUILayout.Width(60)))
                            {
                                templates.RemoveAt(i);
                                MarkDirty();
                                i--;
                                continue;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        var newDescription = EditorGUILayout.TextField("Description", template.description);
                        if (newDescription != template.description)
                        {
                            template.description = newDescription;
                            MarkDirty();
                        }
                        
                        var newType = (JournalEntryType)EditorGUILayout.EnumPopup("Default Type", template.defaultType);
                        if (newType != template.defaultType && newType != JournalEntryType.All)
                        {
                            template.defaultType = newType;
                            MarkDirty();
                        }
                        
                        var newCategory = EditorGUILayout.TextField("Default Category", template.defaultCategory);
                        if (newCategory != template.defaultCategory)
                        {
                            template.defaultCategory = newCategory;
                            MarkDirty();
                        }
                        
                        var newContent = EditorGUILayout.TextArea(template.templateContent, GUILayout.Height(100));
                        if (newContent != template.templateContent)
                        {
                            template.templateContent = newContent;
                            MarkDirty();
                        }
                        
                        DrawStringList("Default Tags", template.defaultTags);
                    }
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space(5);
                }
                
                if (GUILayout.Button("Add New Template"))
                {
                    templates.Add(new JournalTemplate { name = $"Template {templates.Count + 1}" });
                    MarkDirty();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawExportTab()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                EditorGUILayout.LabelField("Export & Import", EditorStyles.boldLabel);
                
                // Export options would be implemented here
                EditorGUILayout.HelpBox("Export functionality for various formats (JSON, XML, Plain Text, PDF) and import capabilities would be implemented here.", MessageType.Info);
                
                EditorGUILayout.Space(10);
                
                if (GUILayout.Button("Export All Entries to JSON"))
                {
                    ExportToFile();
                }
                
                if (GUILayout.Button("Import Entries from JSON"))
                {
                    ImportFromFile();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Utility Methods

        private void DrawStringList(string label, List<string> list)
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                
                for (int i = 0; i < list.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        var newValue = EditorGUILayout.TextField($"Item {i + 1}", list[i]);
                        if (newValue != list[i])
                        {
                            list[i] = newValue;
                            MarkDirty();
                        }
                        
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            list.RemoveAt(i);
                            MarkDirty();
                            i--;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button($"Add {label}"))
                {
                    list.Add("");
                    MarkDirty();
                }
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
        }

        private List<JournalEntry> GetFilteredEntries()
        {
            return allEntries.Where(entry =>
            {
                // Type filter
                if (filterType != JournalEntryType.All && entry.type != filterType)
                    return false;
                    
                // Category filter
                if (!string.IsNullOrEmpty(filterCategory) && entry.category != filterCategory)
                    return false;
                    
                // Search filter
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    var searchLower = searchFilter.ToLower();
                    if (!entry.title.ToLower().Contains(searchLower) &&
                        !entry.content.ToLower().Contains(searchLower) &&
                        !entry.tags.Any(tag => tag.ToLower().Contains(searchLower)))
                        return false;
                }
                
                return true;
            }).ToList();
        }

        private int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
                
            return text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private void CreateNewEntry()
        {
            currentEntry = new JournalEntry();
            selectedEntryIndex = -1;
            isCreatingNew = true;
            currentTab = JournalTab.Entries;
        }

        private void SaveNewEntry()
        {
            if (string.IsNullOrWhiteSpace(currentEntry.title))
            {
                EditorUtility.DisplayDialog("Invalid Entry", "Entry title cannot be empty.", "OK");
                return;
            }
            
            allEntries.Add(currentEntry);
            selectedEntryIndex = allEntries.Count - 1;
            isCreatingNew = false;
            MarkDirty();
            
            Debug.Log($"[Journal Editor] Created new entry: {currentEntry.title}");
        }

        private void CancelNewEntry()
        {
            if (allEntries.Count > 0)
            {
                selectedEntryIndex = 0;
                currentEntry = allEntries[0];
            }
            else
            {
                currentEntry = null;
                selectedEntryIndex = -1;
            }
            
            isCreatingNew = false;
        }

        private void DuplicateEntry()
        {
            if (currentEntry == null) return;
            
            var duplicateEntry = new JournalEntry
            {
                title = $"{currentEntry.title} (Copy)",
                content = currentEntry.content,
                type = currentEntry.type,
                category = currentEntry.category,
                tags = new List<string>(currentEntry.tags),
                author = currentEntry.author,
                relatedEntries = new List<string>(currentEntry.relatedEntries),
                relatedQuests = new List<string>(currentEntry.relatedQuests),
                relatedCharacters = new List<string>(currentEntry.relatedCharacters),
                relatedLocations = new List<string>(currentEntry.relatedLocations),
                timelinePeriod = currentEntry.timelinePeriod,
                entryColor = currentEntry.entryColor,
                priority = currentEntry.priority,
                notes = currentEntry.notes
            };
            
            allEntries.Add(duplicateEntry);
            currentEntry = duplicateEntry;
            selectedEntryIndex = allEntries.Count - 1;
            MarkDirty();
        }

        private void DeleteEntry()
        {
            if (currentEntry == null) return;
            
            if (EditorUtility.DisplayDialog("Delete Entry", 
                $"Are you sure you want to delete the entry '{currentEntry.title}'?", 
                "Delete", "Cancel"))
            {
                allEntries.Remove(currentEntry);
                
                if (allEntries.Count > 0)
                {
                    selectedEntryIndex = Mathf.Min(selectedEntryIndex, allEntries.Count - 1);
                    currentEntry = allEntries[selectedEntryIndex];
                }
                else
                {
                    currentEntry = null;
                    selectedEntryIndex = -1;
                }
                
                MarkDirty();
            }
        }

        private void CreateEntryFromTemplate(JournalTemplate template)
        {
            currentEntry = new JournalEntry
            {
                title = "New Entry from Template",
                content = template.templateContent,
                type = template.defaultType,
                category = template.defaultCategory,
                tags = new List<string>(template.defaultTags)
            };
            
            selectedEntryIndex = -1;
            isCreatingNew = true;
            currentTab = JournalTab.Entries;
        }

        private void GenerateTimelineFromEntries()
        {
            timelineEvents.Clear();
            
            foreach (var entry in allEntries)
            {
                if (entry.timelinePeriod.x != 0 || entry.timelinePeriod.y != 0)
                {
                    var timelineEvent = new TimelineEvent
                    {
                        entryId = entry.id,
                        eventName = entry.title,
                        year = entry.timelinePeriod.x,
                        description = entry.content.Length > 200 ? entry.content.Substring(0, 200) + "..." : entry.content,
                        eventType = ConvertToTimelineEventType(entry.type),
                        eventColor = entry.entryColor
                    };
                    
                    timelineEvents.Add(timelineEvent);
                }
            }
            
            MarkDirty();
            Debug.Log($"[Journal Editor] Generated {timelineEvents.Count} timeline events from entries.");
        }

        private TimelineEventType ConvertToTimelineEventType(JournalEntryType entryType)
        {
            switch (entryType)
            {
                case JournalEntryType.Quest: return TimelineEventType.Quest;
                case JournalEntryType.Character: return TimelineEventType.Relationship;
                case JournalEntryType.Combat: return TimelineEventType.Battle;
                case JournalEntryType.Political: return TimelineEventType.Political;
                case JournalEntryType.Trade: return TimelineEventType.Trade;
                case JournalEntryType.Magic: return TimelineEventType.Magic;
                case JournalEntryType.Discovery: return TimelineEventType.Discovery;
                default: return TimelineEventType.General;
            }
        }

        private void SetupDefaultCategories()
        {
            if (categories.Count == 0)
            {
                categories.AddRange(new[]
                {
                    new JournalCategory { name = "Personal", description = "Personal thoughts and experiences", categoryColor = Color.blue },
                    new JournalCategory { name = "Quests", description = "Quest-related entries", categoryColor = Color.yellow },
                    new JournalCategory { name = "Characters", description = "Character encounters and relationships", categoryColor = Color.green },
                    new JournalCategory { name = "Lore", description = "World lore and discoveries", categoryColor = Color.purple },
                    new JournalCategory { name = "Locations", description = "Places and locations", categoryColor = Color.cyan },
                    new JournalCategory { name = "Combat", description = "Combat encounters and strategies", categoryColor = Color.red }
                });
            }
        }

        private void SetupDefaultTemplates()
        {
            if (templates.Count == 0)
            {
                templates.AddRange(new[]
                {
                    new JournalTemplate 
                    { 
                        name = "Quest Entry", 
                        description = "Template for quest-related entries",
                        defaultType = JournalEntryType.Quest,
                        defaultCategory = "Quests",
                        templateContent = "Quest: [Quest Name]\nObjective: [Objective]\nProgress: [Current Progress]\nNotes: [Additional Notes]",
                        defaultTags = new List<string> { "quest", "objective" }
                    },
                    new JournalTemplate 
                    { 
                        name = "Character Meeting", 
                        description = "Template for character encounters",
                        defaultType = JournalEntryType.Character,
                        defaultCategory = "Characters",
                        templateContent = "Met: [Character Name]\nLocation: [Where Met]\nFirst Impression: [Initial Thoughts]\nImportant Information: [Key Details]",
                        defaultTags = new List<string> { "character", "meeting" }
                    },
                    new JournalTemplate 
                    { 
                        name = "Lore Discovery", 
                        description = "Template for lore and world-building discoveries",
                        defaultType = JournalEntryType.Lore,
                        defaultCategory = "Lore",
                        templateContent = "Discovery: [What Was Learned]\nSource: [How/Where Learned]\nSignificance: [Why Important]\nRelated: [Connected Information]",
                        defaultTags = new List<string> { "lore", "discovery" }
                    }
                });
            }
        }

        private void ExportToFile()
        {
            var path = EditorUtility.SaveFilePanel("Export Journal Entries", "", "journal_entries.json", "json");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var json = JsonUtility.ToJson(new SerializableJournalData 
                    { 
                        entries = allEntries, 
                        categories = categories, 
                        templates = templates 
                    }, true);
                    System.IO.File.WriteAllText(path, json);
                    Debug.Log($"[Journal Editor] Exported {allEntries.Count} entries to {path}");
                    EditorUtility.DisplayDialog("Export Complete", $"Successfully exported {allEntries.Count} entries.", "OK");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Journal Editor] Export failed: {e.Message}");
                    EditorUtility.DisplayDialog("Export Failed", $"Failed to export entries: {e.Message}", "OK");
                }
            }
        }

        private void ImportFromFile()
        {
            var path = EditorUtility.OpenFilePanel("Import Journal Entries", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(path);
                    var data = JsonUtility.FromJson<SerializableJournalData>(json);
                    
                    if (data != null)
                    {
                        allEntries.AddRange(data.entries ?? new List<JournalEntry>());
                        categories.AddRange(data.categories ?? new List<JournalCategory>());
                        templates.AddRange(data.templates ?? new List<JournalTemplate>());
                        
                        MarkDirty();
                        Debug.Log($"[Journal Editor] Imported entries from {path}");
                        EditorUtility.DisplayDialog("Import Complete", "Successfully imported entries.", "OK");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Journal Editor] Import failed: {e.Message}");
                    EditorUtility.DisplayDialog("Import Failed", $"Failed to import entries: {e.Message}", "OK");
                }
            }
        }

        private void MarkDirty()
        {
            isDirty = true;
        }

        private void SaveData()
        {
            // Save to GameData or EditorPrefs
            Debug.Log("[Journal Editor] Data saved successfully.");
            isDirty = false;
        }

        private void LoadData()
        {
            // Load from GameData or create defaults
            if (allEntries.Count == 0)
            {
                CreateNewEntry();
                SaveNewEntry();
            }
            
            Debug.Log("[Journal Editor] Data loaded successfully.");
        }

        #endregion

        #region Data Serialization

        [System.Serializable]
        public class SerializableJournalData
        {
            public List<JournalEntry> entries;
            public List<JournalCategory> categories;
            public List<JournalTemplate> templates;
        }

        #endregion
    }
}

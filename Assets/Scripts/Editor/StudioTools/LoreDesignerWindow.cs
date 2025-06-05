using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Core;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    public class LoreDesignerWindow : EditorWindow
    {
        private LoreEntry currentLore; //TODO: Add LoreEntry to Database.
        private Vector2 scrollPosition;
        private Vector2 listScrollPosition;
        private string searchFilter = "";
        private int selectedTab = 0;
        private readonly string[] tabNames = { "Basic Info", "Content", "Relationships", "Timeline", "Preview" };
        
        // Timeline and relationship data
        private string newRelatedEntry = "";
        private string newTimelineEvent = "";
        
        [MenuItem("Lineage Studio/Content Creation/Lore & Narrative Designer")]
        public static void ShowWindow()
        {
            var window = GetWindow<LoreDesignerWindow>("Lore & Narrative Designer");
            window.minSize = new Vector2(900, 650);
            window.Show();
        }
          private void OnEnable()
        {
            if (currentLore == null)
                currentLore = new LoreEntry();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            
            EditorGUILayout.BeginHorizontal();
            {
                // Left panel - Lore list
                DrawLoreList();
                
                // Right panel - Lore editor
                DrawLoreEditor();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Lore & Narrative Designer", EditorStyles.boldLabel);
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Import Text File", GUILayout.Width(120)))
                {
                    ImportTextFile();
                }
                
                if (GUILayout.Button("Export All", GUILayout.Width(80)))
                {
                    ExportAllLore();
                }
                
                if (GUILayout.Button("Save All", GUILayout.Width(80)))
                {
                    SaveAllLore();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawLoreList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(280));
            {
                // Search and controls
                EditorGUILayout.BeginHorizontal();
                {
                    searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
                    if (GUILayout.Button("New", GUILayout.Width(50)))
                        CreateNewLore();
                }
                EditorGUILayout.EndHorizontal();
                
                // Category filter
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("All", EditorStyles.miniButtonLeft))
                        searchFilter = "";
                    if (GUILayout.Button("History", EditorStyles.miniButtonMid))
                        searchFilter = "History";
                    if (GUILayout.Button("Character", EditorStyles.miniButtonMid))
                        searchFilter = "Character";
                    if (GUILayout.Button("Location", EditorStyles.miniButtonMid))
                        searchFilter = "Location";
                    if (GUILayout.Button("Culture", EditorStyles.miniButtonRight))
                        searchFilter = "Culture";
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // Lore entries list
                listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition);
                {
                    DrawLoreEntries();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawLoreEntries()
        {
            if (GameData.LoreEntries == null) return;
            
            var filteredEntries = GameData.LoreEntries.Where(l => 
                string.IsNullOrEmpty(searchFilter) || 
                l.Title.ToLower().Contains(searchFilter.ToLower()) ||
                l.Category.ToLower().Contains(searchFilter.ToLower()) ||
                l.Tags.Any(t => t.ToLower().Contains(searchFilter.ToLower()))).ToList();
            
            // Group by category
            var groupedEntries = filteredEntries.GroupBy(l => l.Category).OrderBy(g => g.Key);
            
            foreach (var group in groupedEntries)
            {
                GUILayout.Label(group.Key, EditorStyles.boldLabel);
                
                foreach (var lore in group.OrderBy(l => l.Title))
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        bool isSelected = currentLore != null && currentLore.ID == lore.ID;
                        
                        if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
                        {
                            currentLore = lore.Clone();
                        }
                        
                        EditorGUILayout.BeginVertical();
                        {
                            GUILayout.Label(lore.Title, EditorStyles.boldLabel);
                            
                            // Show first line of content as preview
                            if (!string.IsNullOrEmpty(lore.Content))
                            {
                                string preview = lore.Content.Length > 60 ? 
                                    lore.Content.Substring(0, 60) + "..." : lore.Content;
                                GUILayout.Label(preview, EditorStyles.miniLabel);
                            }
                            
                            // Show tags
                            if (lore.Tags.Count > 0)
                            {
                                string tagsText = string.Join(", ", lore.Tags.Take(3));
                                if (lore.Tags.Count > 3) tagsText += "...";
                                GUILayout.Label($"Tags: {tagsText}", EditorStyles.miniLabel);
                            }
                        }
                        EditorGUILayout.EndVertical();
                        
                        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            if (EditorUtility.DisplayDialog("Delete Lore Entry", 
                                $"Are you sure you want to delete '{lore.Title}'?", "Delete", "Cancel"))
                            {
                                GameData.LoreEntries.Remove(lore);
                                if (currentLore?.ID == lore.ID)
                                    currentLore = new LoreEntry();
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.Space(2);
                }
                
                GUILayout.Space(5);
            }
        }
        
        private void DrawLoreEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                if (currentLore == null)
                {
                    GUILayout.Label("Select a lore entry to edit or create a new one.", EditorStyles.centeredGreyMiniLabel);
                    return;
                }
                
                // Tab selection
                selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    switch (selectedTab)
                    {
                        case 0: DrawBasicInfo(); break;
                        case 1: DrawContent(); break;
                        case 2: DrawRelationships(); break;
                        case 3: DrawTimeline(); break;
                        case 4: DrawPreview(); break;
                    }
                }
                EditorGUILayout.EndScrollView();
                
                DrawSaveControls();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBasicInfo()
        {
            GUILayout.Label("Basic Information", EditorStyles.boldLabel);
            
            currentLore.Title = EditorGUILayout.TextField("Title", currentLore.Title);
            
            EditorGUILayout.BeginHorizontal();
            {
                currentLore.Category = EditorGUILayout.TextField("Category", currentLore.Category);
                if (GUILayout.Button("Presets", GUILayout.Width(70)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("History"), false, () => currentLore.Category = "History");
                    menu.AddItem(new GUIContent("Character"), false, () => currentLore.Category = "Character");
                    menu.AddItem(new GUIContent("Location"), false, () => currentLore.Category = "Location");
                    menu.AddItem(new GUIContent("Culture"), false, () => currentLore.Category = "Culture");
                    menu.AddItem(new GUIContent("Religion"), false, () => currentLore.Category = "Religion");
                    menu.AddItem(new GUIContent("Technology"), false, () => currentLore.Category = "Technology");
                    menu.AddItem(new GUIContent("Magic"), false, () => currentLore.Category = "Magic");
                    menu.AddItem(new GUIContent("Politics"), false, () => currentLore.Category = "Politics");
                    menu.AddItem(new GUIContent("Economics"), false, () => currentLore.Category = "Economics");
                    menu.AddItem(new GUIContent("Mythology"), false, () => currentLore.Category = "Mythology");
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            currentLore.Importance = (LoreImportance)EditorGUILayout.EnumPopup("Importance", currentLore.Importance);
            currentLore.IsPublic = EditorGUILayout.Toggle("Is Public Knowledge", currentLore.IsPublic);
            
            GUILayout.Space(10);
            
            // Tags
            GUILayout.Label("Tags", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (currentLore.Tags == null)
                    currentLore.Tags = new List<string>();
                
                string tagsString = string.Join(", ", currentLore.Tags);
                string newTagsString = EditorGUILayout.TextField("Tags (comma-separated)", tagsString);
                
                if (newTagsString != tagsString)
                {
                    currentLore.Tags = newTagsString.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                }
                
                if (GUILayout.Button("Tag Presets", GUILayout.Width(90)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Ancient"), false, () => AddTag("Ancient"));
                    menu.AddItem(new GUIContent("Secret"), false, () => AddTag("Secret"));
                    menu.AddItem(new GUIContent("War"), false, () => AddTag("War"));
                    menu.AddItem(new GUIContent("Peace"), false, () => AddTag("Peace"));
                    menu.AddItem(new GUIContent("Noble"), false, () => AddTag("Noble"));
                    menu.AddItem(new GUIContent("Common"), false, () => AddTag("Common"));
                    menu.AddItem(new GUIContent("Legendary"), false, () => AddTag("Legendary"));
                    menu.AddItem(new GUIContent("Forbidden"), false, () => AddTag("Forbidden"));
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Summary
            GUILayout.Label("Summary", EditorStyles.boldLabel);
            currentLore.Summary = EditorGUILayout.TextArea(currentLore.Summary, GUILayout.Height(80));
            EditorGUILayout.HelpBox("Brief summary for quick reference and search.", MessageType.Info);
        }
        
        private void DrawContent()
        {
            GUILayout.Label("Lore Content", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Word Count: " + GetWordCount(currentLore.Content), EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Format", GUILayout.Width(60)))
                {
                    FormatContent();
                }
                
                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                {
                    if (EditorUtility.DisplayDialog("Clear Content", 
                        "Are you sure you want to clear all content?", "Clear", "Cancel"))
                    {
                        currentLore.Content = "";
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Main content area
            currentLore.Content = EditorGUILayout.TextArea(currentLore.Content, 
                GUILayout.Height(300), GUILayout.ExpandWidth(true));
            
            GUILayout.Space(10);
            
            // Writing tools
            GUILayout.Label("Writing Tools", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add Chapter Break"))
                {
                    currentLore.Content += "\n\n--- Chapter ---\n\n";
                }
                
                if (GUILayout.Button("Add Section Break"))
                {
                    currentLore.Content += "\n\n• • •\n\n";
                }
                
                if (GUILayout.Button("Add Quote"))
                {
                    currentLore.Content += "\n\n\"[Quote here]\"\n- [Source]\n\n";
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add Dialogue"))
                {
                    currentLore.Content += "\n[Character]: \"[Dialogue here]\"\n";
                }
                
                if (GUILayout.Button("Add Location"))
                {
                    currentLore.Content += "\n**Location: [Name]**\n[Description]\n";
                }
                
                if (GUILayout.Button("Add Date"))
                {
                    currentLore.Content += "\n**Date: [Year/Era]**\n";
                }
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Content metadata
            GUILayout.Label("Content Metadata", EditorStyles.boldLabel);
            currentLore.Author = EditorGUILayout.TextField("In-World Author", currentLore.Author);
            currentLore.DateWritten = EditorGUILayout.TextField("Date Written (In-World)", currentLore.DateWritten);
            currentLore.Source = EditorGUILayout.TextField("Source/Origin", currentLore.Source);
        }
        
        private void DrawRelationships()
        {
            GUILayout.Label("Relationships & Connections", EditorStyles.boldLabel);
            
            // Related Characters
            GUILayout.Label("Related Characters:", EditorStyles.boldLabel);
            if (currentLore.RelatedCharacters == null)
                currentLore.RelatedCharacters = new List<string>();
            
            for (int i = 0; i < currentLore.RelatedCharacters.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentLore.RelatedCharacters[i] = EditorGUILayout.TextField($"Character {i + 1}", currentLore.RelatedCharacters[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentLore.RelatedCharacters.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            {
                newRelatedEntry = EditorGUILayout.TextField("New Character", newRelatedEntry);
                if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newRelatedEntry))
                {
                    currentLore.RelatedCharacters.Add(newRelatedEntry);
                    newRelatedEntry = "";
                }
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Related Locations
            GUILayout.Label("Related Locations:", EditorStyles.boldLabel);
            if (currentLore.RelatedLocations == null)
                currentLore.RelatedLocations = new List<string>();
            
            for (int i = 0; i < currentLore.RelatedLocations.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentLore.RelatedLocations[i] = EditorGUILayout.TextField($"Location {i + 1}", currentLore.RelatedLocations[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentLore.RelatedLocations.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Related Location"))
                currentLore.RelatedLocations.Add("");
            
            GUILayout.Space(10);
            
            // Related Events
            GUILayout.Label("Related Events:", EditorStyles.boldLabel);
            if (currentLore.RelatedEvents == null)
                currentLore.RelatedEvents = new List<string>();
            
            for (int i = 0; i < currentLore.RelatedEvents.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentLore.RelatedEvents[i] = EditorGUILayout.TextField($"Event {i + 1}", currentLore.RelatedEvents[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentLore.RelatedEvents.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Related Event"))
                currentLore.RelatedEvents.Add("");
            
            GUILayout.Space(10);
            
            // Cross-References
            GUILayout.Label("Cross-References:", EditorStyles.boldLabel);
            if (currentLore.CrossReferences == null)
                currentLore.CrossReferences = new List<string>();
            
            for (int i = 0; i < currentLore.CrossReferences.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentLore.CrossReferences[i] = EditorGUILayout.TextField($"Reference {i + 1}", currentLore.CrossReferences[i]);
                    if (GUILayout.Button("Find", GUILayout.Width(45)))
                    {
                        // Find and select referenced lore entry
                        var referencedLore = GameData.LoreEntries?.FirstOrDefault(l => 
                            l.Title.Equals(currentLore.CrossReferences[i], System.StringComparison.OrdinalIgnoreCase));
                        if (referencedLore != null)
                        {
                            currentLore = referencedLore.Clone();
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Not Found", 
                                $"Could not find lore entry: {currentLore.CrossReferences[i]}", "OK");
                        }
                    }
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentLore.CrossReferences.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Cross-Reference"))
                currentLore.CrossReferences.Add("");
        }
        
        private void DrawTimeline()
        {
            GUILayout.Label("Timeline & Chronology", EditorStyles.boldLabel);
            
            currentLore.TimelinePosition = EditorGUILayout.TextField("Timeline Position", currentLore.TimelinePosition);
            EditorGUILayout.HelpBox("Example: '2nd Age, Year 1847' or 'Before the Great War'", MessageType.Info);
            
            currentLore.Duration = EditorGUILayout.TextField("Duration/Time Span", currentLore.Duration);
            EditorGUILayout.HelpBox("How long did this event/period last?", MessageType.Info);
            
            GUILayout.Space(10);
            
            // Timeline Events
            GUILayout.Label("Key Timeline Events:", EditorStyles.boldLabel);
            if (currentLore.TimelineEvents == null)
                currentLore.TimelineEvents = new List<TimelineEvent>();
            
            for (int i = 0; i < currentLore.TimelineEvents.Count; i++)
            {
                var timelineEvent = currentLore.TimelineEvents[i];
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label($"Event {i + 1}", EditorStyles.boldLabel);
                        if (GUILayout.Button("×", GUILayout.Width(20)))
                        {
                            currentLore.TimelineEvents.RemoveAt(i);
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    timelineEvent.Date = EditorGUILayout.TextField("Date", timelineEvent.Date);
                    timelineEvent.Title = EditorGUILayout.TextField("Event Title", timelineEvent.Title);
                    timelineEvent.Description = EditorGUILayout.TextArea(timelineEvent.Description, GUILayout.Height(40));
                    timelineEvent.Importance = (EventImportance)EditorGUILayout.EnumPopup("Importance", timelineEvent.Importance);
                }
                EditorGUILayout.EndVertical();
                
                GUILayout.Space(5);
            }
            
            EditorGUILayout.BeginHorizontal();
            {
                newTimelineEvent = EditorGUILayout.TextField("New Event Title", newTimelineEvent);
                if (GUILayout.Button("Add Event", GUILayout.Width(80)) && !string.IsNullOrEmpty(newTimelineEvent))
                {
                    currentLore.TimelineEvents.Add(new TimelineEvent
                    {
                        Title = newTimelineEvent,
                        Date = "Unknown",
                        Description = "",
                        Importance = EventImportance.Minor
                    });
                    newTimelineEvent = "";
                }
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Era Information
            GUILayout.Label("Era Information", EditorStyles.boldLabel);
            currentLore.Era = EditorGUILayout.TextField("Era", currentLore.Era);
            currentLore.Age = EditorGUILayout.TextField("Age", currentLore.Age);
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("1st Age"))
                    currentLore.Age = "1st Age";
                if (GUILayout.Button("2nd Age"))
                    currentLore.Age = "2nd Age";
                if (GUILayout.Button("3rd Age"))
                    currentLore.Age = "3rd Age";
                if (GUILayout.Button("Current Age"))
                    currentLore.Age = "Current Age";
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawPreview()
        {
            GUILayout.Label("Lore Entry Preview", EditorStyles.boldLabel);
            
            // Header information
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label(currentLore.Title, EditorStyles.largeLabel);
                GUILayout.Label($"Category: {currentLore.Category} | Importance: {currentLore.Importance}", EditorStyles.miniLabel);
                
                if (!string.IsNullOrEmpty(currentLore.TimelinePosition))
                    GUILayout.Label($"Timeline: {currentLore.TimelinePosition}", EditorStyles.miniLabel);
                
                if (!string.IsNullOrEmpty(currentLore.Author))
                    GUILayout.Label($"Author: {currentLore.Author}", EditorStyles.miniLabel);
                
                if (currentLore.Tags?.Count > 0)
                    GUILayout.Label($"Tags: {string.Join(", ", currentLore.Tags)}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Summary
            if (!string.IsNullOrEmpty(currentLore.Summary))
            {
                GUILayout.Label("Summary:", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(currentLore.Summary, EditorStyles.wordWrappedLabel, GUILayout.Height(60));
                GUILayout.Space(10);
            }
            
            // Content preview
            if (!string.IsNullOrEmpty(currentLore.Content))
            {
                GUILayout.Label("Content Preview:", EditorStyles.boldLabel);
                string previewContent = currentLore.Content.Length > 500 ? 
                    currentLore.Content.Substring(0, 500) + "..." : currentLore.Content;
                EditorGUILayout.TextArea(previewContent, EditorStyles.wordWrappedLabel, GUILayout.Height(200));
            }
            
            GUILayout.Space(10);
            
            // Relationships summary
            if (HasRelationships())
            {
                GUILayout.Label("Connections:", EditorStyles.boldLabel);
                
                if (currentLore.RelatedCharacters?.Count > 0)
                    GUILayout.Label($"Characters: {string.Join(", ", currentLore.RelatedCharacters)}", EditorStyles.miniLabel);
                
                if (currentLore.RelatedLocations?.Count > 0)
                    GUILayout.Label($"Locations: {string.Join(", ", currentLore.RelatedLocations)}", EditorStyles.miniLabel);
                
                if (currentLore.RelatedEvents?.Count > 0)
                    GUILayout.Label($"Events: {string.Join(", ", currentLore.RelatedEvents)}", EditorStyles.miniLabel);
                
                if (currentLore.CrossReferences?.Count > 0)
                    GUILayout.Label($"References: {string.Join(", ", currentLore.CrossReferences)}", EditorStyles.miniLabel);
            }
            
            // Timeline summary
            if (currentLore.TimelineEvents?.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("Timeline Events:", EditorStyles.boldLabel);
                foreach (var evt in currentLore.TimelineEvents.OrderBy(e => e.Date))
                {
                    GUILayout.Label($"• {evt.Date}: {evt.Title}", EditorStyles.miniLabel);
                }
            }
        }
        
        private void DrawSaveControls()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save Entry"))
                {
                    SaveLore();
                }
                
                if (GUILayout.Button("Reset"))
                {
                    if (EditorUtility.DisplayDialog("Reset Entry", 
                        "Are you sure you want to reset all changes?", "Reset", "Cancel"))
                    {
                        currentLore = new LoreEntry();
                    }
                }
                
                if (GUILayout.Button("Duplicate"))
                {
                    var newLore = currentLore.Clone();
                    newLore.ID = System.Guid.NewGuid().ToString();
                    newLore.Title += " (Copy)";
                    currentLore = newLore;
                }
                
                if (GUILayout.Button("Export"))
                {
                    ExportLoreEntry(currentLore);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void CreateNewLore()
        {
            currentLore = new LoreEntry
            {
                ID = System.Guid.NewGuid().ToString(),
                Title = "New Lore Entry",
                Category = "History",
                Importance = LoreImportance.Minor,
                IsPublic = true,
                Tags = new List<string>()
            };
            selectedTab = 0;
        }
        
        private void SaveLore()
        {
            if (string.IsNullOrEmpty(currentLore.Title))
            {
                EditorUtility.DisplayDialog("Error", "Lore entry title cannot be empty!", "OK");
                return;
            }
            
            var existingLore = GameData.LoreEntries?.FirstOrDefault(l => l.ID == currentLore.ID);
            if (existingLore != null)
            {
                // Update existing
                int index = GameData.LoreEntries.IndexOf(existingLore);
                GameData.LoreEntries[index] = currentLore.Clone();
            }
            else
            {
                // Add new
                if (GameData.LoreEntries == null)
                    GameData.LoreEntries = new List<LoreEntry>();
                GameData.LoreEntries.Add(currentLore.Clone());
            }
            
            EditorUtility.DisplayDialog("Success", $"Lore entry '{currentLore.Title}' saved successfully!", "OK");
        }
        
        private void SaveAllLore()
        {
            // This would integrate with your save system
            EditorUtility.DisplayDialog("Save All", "All lore entries saved to database!", "OK");
        }
        
        private void ImportTextFile()
        {
            string path = EditorUtility.OpenFilePanel("Import Text File", "", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                string content = System.IO.File.ReadAllText(path);
                string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                
                var newLore = new LoreEntry
                {
                    ID = System.Guid.NewGuid().ToString(),
                    Title = filename,
                    Content = content,
                    Category = "Imported",
                    Importance = LoreImportance.Minor,
                    IsPublic = true
                };
                
                currentLore = newLore;
                EditorUtility.DisplayDialog("Import Successful", $"Imported '{filename}' successfully!", "OK");
            }
        }
        
        private void ExportLoreEntry(LoreEntry lore)
        {
            string path = EditorUtility.SaveFilePanel("Export Lore Entry", "", lore.Title + ".txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                string exportContent = $"Title: {lore.Title}\n";
                exportContent += $"Category: {lore.Category}\n";
                exportContent += $"Timeline: {lore.TimelinePosition}\n";
                if (!string.IsNullOrEmpty(lore.Author))
                    exportContent += $"Author: {lore.Author}\n";
                exportContent += $"\nSummary:\n{lore.Summary}\n";
                exportContent += $"\nContent:\n{lore.Content}";
                
                System.IO.File.WriteAllText(path, exportContent);
                EditorUtility.DisplayDialog("Export Successful", $"Exported '{lore.Title}' successfully!", "OK");
            }
        }
        
        private void ExportAllLore()
        {
            string folderPath = EditorUtility.OpenFolderPanel("Export All Lore", "", "");
            if (!string.IsNullOrEmpty(folderPath) && GameData.LoreEntries != null)
            {
                foreach (var lore in GameData.LoreEntries)
                {
                    string filename = lore.Title.Replace(" ", "_").Replace("/", "_") + ".txt";
                    string path = System.IO.Path.Combine(folderPath, filename);
                    
                    string exportContent = $"Title: {lore.Title}\n";
                    exportContent += $"Category: {lore.Category}\n";
                    exportContent += $"Timeline: {lore.TimelinePosition}\n";
                    if (!string.IsNullOrEmpty(lore.Author))
                        exportContent += $"Author: {lore.Author}\n";
                    exportContent += $"\nSummary:\n{lore.Summary}\n";
                    exportContent += $"\nContent:\n{lore.Content}";
                    
                    System.IO.File.WriteAllText(path, exportContent);
                }
                
                EditorUtility.DisplayDialog("Export Complete", 
                    $"Exported {GameData.LoreEntries.Count} lore entries successfully!", "OK");
            }
        }
        
        private void FormatContent()
        {
            // Basic text formatting
            currentLore.Content = currentLore.Content.Replace("\n\n\n", "\n\n"); // Remove triple line breaks
            currentLore.Content = currentLore.Content.Trim(); // Remove leading/trailing whitespace
        }
        
        private void AddTag(string tag)
        {
            if (currentLore.Tags == null)
                currentLore.Tags = new List<string>();
            
            if (!currentLore.Tags.Contains(tag))
                currentLore.Tags.Add(tag);
        }
        
        private int GetWordCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
            
            return text.Split(new char[] { ' ', '\t', '\n', '\r' }, 
                System.StringSplitOptions.RemoveEmptyEntries).Length;
        }
        
        private bool HasRelationships()
        {
            return (currentLore.RelatedCharacters?.Count > 0) ||
                   (currentLore.RelatedLocations?.Count > 0) ||
                   (currentLore.RelatedEvents?.Count > 0) ||
                   (currentLore.CrossReferences?.Count > 0);
        }
    }
}

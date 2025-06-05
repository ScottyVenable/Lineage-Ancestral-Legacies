using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Database Inspector window for detailed analysis and inspection of game database contents.
    /// Provides advanced search, filtering, relationship visualization, and data integrity checks.
    /// </summary>
    public class DatabaseInspectorWindow : EditorWindow
    {
        #region Window Management

        private static DatabaseInspectorWindow window;

        [MenuItem("Lineage/Studio/Database/Database Inspector")]
        public static void ShowWindow()
        {
            window = GetWindow<DatabaseInspectorWindow>("Database Inspector");
            window.minSize = new Vector2(900, 600);
            window.Show();
        }

        #endregion

        #region Private Fields

        private Vector2 scrollPosition;
        private int selectedTabIndex = 0;
        private readonly string[] tabNames = { "Overview", "Search", "Relationships", "Statistics", "Integrity", "Performance" };

        // Overview Tab
        private bool[] databaseExpanded = new bool[20];
        private Dictionary<string, int> databaseCounts = new Dictionary<string, int>();

        // Search Tab
        private string searchQuery = "";
        private SearchType searchType = SearchType.Global;
        private SearchScope searchScope = SearchScope.All;
        private List<SearchResult> searchResults = new List<SearchResult>();
        private Vector2 searchScrollPosition;
        private bool caseSensitive = false;
        private bool useRegex = false;

        // Relationships Tab
        private object selectedEntity;
        private Vector2 relationshipScrollPosition;
        private List<RelationshipInfo> relationships = new List<RelationshipInfo>();
        private bool showIncomingRefs = true;
        private bool showOutgoingRefs = true;

        // Statistics Tab
        private Vector2 statsScrollPosition;
        private Dictionary<string, DatabaseStatistics> databaseStats = new Dictionary<string, DatabaseStatistics>();

        // Integrity Tab
        private Vector2 integrityScrollPosition;
        private List<IntegrityIssue> integrityIssues = new List<IntegrityIssue>();
        private bool autoCheckIntegrity = true;

        // Performance Tab
        private Vector2 performanceScrollPosition;
        private Dictionary<string, PerformanceMetrics> performanceMetrics = new Dictionary<string, PerformanceMetrics>();

        #endregion

        #region Data Structures

        private enum SearchType
        {
            Global,
            ByName,
            ByProperty,
            ByContent
        }

        private enum SearchScope
        {
            All,
            Entities,
            Items,
            Traits,
            Quests,
            NPCs,
            Skills,
            Buffs,
            Objectives,
            Stats,
            Genetics,
            Journal,
            Settlements,
            Populations
        }

        [System.Serializable]
        public class SearchResult
        {
            public object entity;
            public string databaseType;
            public string entityName;
            public string propertyName;
            public string matchingContent;
            public float relevanceScore;
        }

        [System.Serializable]
        public class RelationshipInfo
        {
            public object fromEntity;
            public object toEntity;
            public string relationshipType;
            public string propertyPath;
            public bool isIncoming;
        }

        [System.Serializable]
        public class DatabaseStatistics
        {
            public int totalEntries;
            public int avgPropertiesPerEntry;
            public Dictionary<string, int> propertyUsage;
            public List<string> duplicateEntries;
            public Dictionary<string, object> extremeValues;
        }

        [System.Serializable]
        public class IntegrityIssue
        {
            public SeverityLevel severity;
            public string issueType;
            public string description;
            public object relatedEntity;
            public string suggestedFix;
        }

        public enum SeverityLevel
        {
            Info,
            Warning,
            Error,
            Critical
        }

        [System.Serializable]
        public class PerformanceMetrics
        {
            public float searchTime;
            public int memoryUsage;
            public int indexSize;
            public Dictionary<string, float> queryTimes;
        }

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            RefreshDatabaseCounts();
            if (autoCheckIntegrity)
            {
                CheckDatabaseIntegrity();
            }
            CalculateStatistics();
        }

        private void OnGUI()
        {
            DrawHeader();
            
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (selectedTabIndex)
            {
                case 0: DrawOverviewTab(); break;
                case 1: DrawSearchTab(); break;
                case 2: DrawRelationshipsTab(); break;
                case 3: DrawStatisticsTab(); break;
                case 4: DrawIntegrityTab(); break;
                case 5: DrawPerformanceTab(); break;
            }
            
            EditorGUILayout.EndScrollView();
            
            DrawFooter();
        }

        #endregion

        #region GUI Drawing Methods

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Database Inspector", EditorStyles.largeLabel);
                GUILayout.FlexibleSpace();
            }
            
            EditorGUILayout.Space();
        }

        private void DrawOverviewTab()
        {
            EditorGUILayout.LabelField("Database Overview", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh All", GUILayout.Width(100)))
                {
                    RefreshDatabaseCounts();
                }
                
                GUILayout.FlexibleSpace();
                
                EditorGUILayout.LabelField($"Total Databases: {databaseCounts.Count}");
            }

            EditorGUILayout.Space();

            DrawDatabaseOverview("Entities", GameData.entityDatabase.Count, 0);
            DrawDatabaseOverview("Items", GameData.itemDatabase.Count, 1);
            DrawDatabaseOverview("Traits", GameData.traitDatabase.Count, 2);
            DrawDatabaseOverview("Quests", GameData.questDatabase.Count, 3);
            DrawDatabaseOverview("NPCs", GameData.npcDatabase.Count, 4);
            DrawDatabaseOverview("Skills", GameData.skillDatabase.Count, 5);
            DrawDatabaseOverview("Buffs", GameData.buffDatabase.Count, 6);
            DrawDatabaseOverview("Objectives", GameData.objectiveDatabase.Count, 7);
            DrawDatabaseOverview("Stats", GameData.statDatabase.Count, 8);
            DrawDatabaseOverview("Genetics", GameData.geneticsDatabase.Count, 9);
            DrawDatabaseOverview("Journal", GameData.journalDatabase.Count, 10);

            EditorGUILayout.Space();
            
            if (GUILayout.Button("Export Database Summary"))
            {
                ExportDatabaseSummary();
            }
        }

        private void DrawDatabaseOverview(string name, int count, int index)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                databaseExpanded[index] = EditorGUILayout.Foldout(databaseExpanded[index], $"{name} Database");
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"{count} entries", GUILayout.Width(80));
                
                if (GUILayout.Button("View", GUILayout.Width(50)))
                {
                    OpenDatabaseEditor(name);
                }
            }

            if (databaseExpanded[index] && count > 0)
            {
                EditorGUI.indentLevel++;
                
                var sampleEntries = GetSampleEntries(name, 3);
                foreach (var entry in sampleEntries)
                {
                    EditorGUILayout.LabelField($"• {GetEntityDisplayName(entry)}");
                }
                
                if (count > 3)
                {
                    EditorGUILayout.LabelField($"... and {count - 3} more entries");
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        private void DrawSearchTab()
        {
            EditorGUILayout.LabelField("Advanced Search", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Search Query:", GUILayout.Width(100));
                searchQuery = EditorGUILayout.TextField(searchQuery);
                
                if (GUILayout.Button("Search", GUILayout.Width(80)))
                {
                    PerformSearch();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                searchType = (SearchType)EditorGUILayout.EnumPopup("Search Type:", searchType, GUILayout.Width(250));
                searchScope = (SearchScope)EditorGUILayout.EnumPopup("Scope:", searchScope, GUILayout.Width(200));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                caseSensitive = EditorGUILayout.Toggle("Case Sensitive", caseSensitive, GUILayout.Width(150));
                useRegex = EditorGUILayout.Toggle("Use Regex", useRegex, GUILayout.Width(150));
            }

            EditorGUILayout.Space();

            if (searchResults.Count > 0)
            {
                EditorGUILayout.LabelField($"Search Results ({searchResults.Count} found):", EditorStyles.boldLabel);
                
                searchScrollPosition = EditorGUILayout.BeginScrollView(searchScrollPosition, GUILayout.Height(300));
                
                foreach (var result in searchResults.OrderByDescending(r => r.relevanceScore))
                {
                    DrawSearchResult(result);
                }
                
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawSearchResult(SearchResult result)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(result.entityName, EditorStyles.boldLabel, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"[{result.databaseType}]", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Score: {result.relevanceScore:F2}", GUILayout.Width(80));
                    
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        selectedEntity = result.entity;
                        selectedTabIndex = 2; // Switch to relationships tab
                    }
                }
                
                if (!string.IsNullOrEmpty(result.propertyName))
                {
                    EditorGUILayout.LabelField($"Property: {result.propertyName}", EditorStyles.miniLabel);
                }
                
                if (!string.IsNullOrEmpty(result.matchingContent))
                {
                    EditorGUILayout.LabelField($"Match: {result.matchingContent}", EditorStyles.miniLabel);
                }
            }
        }

        private void DrawRelationshipsTab()
        {
            EditorGUILayout.LabelField("Entity Relationships", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Selected Entity:", GUILayout.Width(120));
                
                if (selectedEntity != null)
                {
                    EditorGUILayout.LabelField(GetEntityDisplayName(selectedEntity));
                    
                    if (GUILayout.Button("Clear", GUILayout.Width(50)))
                    {
                        selectedEntity = null;
                        relationships.Clear();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("None selected");
                }
            }

            if (selectedEntity != null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    showIncomingRefs = EditorGUILayout.Toggle("Show Incoming", showIncomingRefs, GUILayout.Width(150));
                    showOutgoingRefs = EditorGUILayout.Toggle("Show Outgoing", showOutgoingRefs, GUILayout.Width(150));
                    
                    if (GUILayout.Button("Analyze Relationships", GUILayout.Width(150)))
                    {
                        AnalyzeRelationships();
                    }
                }

                EditorGUILayout.Space();

                if (relationships.Count > 0)
                {
                    relationshipScrollPosition = EditorGUILayout.BeginScrollView(relationshipScrollPosition, GUILayout.Height(350));
                    
                    foreach (var rel in relationships)
                    {
                        if ((rel.isIncoming && showIncomingRefs) || (!rel.isIncoming && showOutgoingRefs))
                        {
                            DrawRelationship(rel);
                        }
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
                else
                {
                    EditorGUILayout.HelpBox("No relationships found for this entity.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select an entity from the Search tab to analyze its relationships.", MessageType.Info);
            }
        }

        private void DrawRelationship(RelationshipInfo rel)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    string arrow = rel.isIncoming ? "← " : "→ ";
                    EditorGUILayout.LabelField(arrow + rel.relationshipType, EditorStyles.boldLabel, GUILayout.Width(150));
                    
                    string otherEntity = rel.isIncoming ? GetEntityDisplayName(rel.fromEntity) : GetEntityDisplayName(rel.toEntity);
                    EditorGUILayout.LabelField(otherEntity);
                    
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        selectedEntity = rel.isIncoming ? rel.fromEntity : rel.toEntity;
                        AnalyzeRelationships();
                    }
                }
                
                EditorGUILayout.LabelField($"Via: {rel.propertyPath}", EditorStyles.miniLabel);
            }
        }

        private void DrawStatisticsTab()
        {
            EditorGUILayout.LabelField("Database Statistics", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Recalculate Statistics", GUILayout.Width(200)))
            {
                CalculateStatistics();
            }

            EditorGUILayout.Space();

            statsScrollPosition = EditorGUILayout.BeginScrollView(statsScrollPosition);

            foreach (var kvp in databaseStats)
            {
                DrawDatabaseStatistics(kvp.Key, kvp.Value);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawDatabaseStatistics(string databaseName, DatabaseStatistics stats)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(databaseName, EditorStyles.boldLabel);
                
                EditorGUILayout.LabelField($"Total Entries: {stats.totalEntries}");
                EditorGUILayout.LabelField($"Avg Properties per Entry: {stats.avgPropertiesPerEntry}");
                
                if (stats.duplicateEntries.Count > 0)
                {
                    EditorGUILayout.LabelField($"Duplicate Entries: {stats.duplicateEntries.Count}", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.Space();
            }
        }

        private void DrawIntegrityTab()
        {
            EditorGUILayout.LabelField("Database Integrity", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                autoCheckIntegrity = EditorGUILayout.Toggle("Auto Check", autoCheckIntegrity, GUILayout.Width(100));
                
                if (GUILayout.Button("Check Now", GUILayout.Width(100)))
                {
                    CheckDatabaseIntegrity();
                }
                
                if (GUILayout.Button("Fix All", GUILayout.Width(100)))
                {
                    FixIntegrityIssues();
                }
            }

            EditorGUILayout.Space();

            if (integrityIssues.Count > 0)
            {
                EditorGUILayout.LabelField($"Issues Found: {integrityIssues.Count}", EditorStyles.boldLabel);
                
                integrityScrollPosition = EditorGUILayout.BeginScrollView(integrityScrollPosition, GUILayout.Height(350));
                
                foreach (var issue in integrityIssues.OrderByDescending(i => i.severity))
                {
                    DrawIntegrityIssue(issue);
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("No integrity issues found.", MessageType.Info);
            }
        }

        private void DrawIntegrityIssue(IntegrityIssue issue)
        {
            Color originalColor = GUI.color;
            
            switch (issue.severity)
            {
                case SeverityLevel.Critical: GUI.color = Color.red; break;
                case SeverityLevel.Error: GUI.color = new Color(1f, 0.5f, 0f); break;
                case SeverityLevel.Warning: GUI.color = Color.yellow; break;
                case SeverityLevel.Info: GUI.color = Color.cyan; break;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUI.color = originalColor;
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"[{issue.severity}] {issue.issueType}", EditorStyles.boldLabel);
                    
                    if (!string.IsNullOrEmpty(issue.suggestedFix) && GUILayout.Button("Fix", GUILayout.Width(50)))
                    {
                        ApplyIntegrityFix(issue);
                    }
                }
                
                EditorGUILayout.LabelField(issue.description, EditorStyles.wordWrappedLabel);
                
                if (issue.relatedEntity != null)
                {
                    EditorGUILayout.LabelField($"Entity: {GetEntityDisplayName(issue.relatedEntity)}", EditorStyles.miniLabel);
                }
            }
        }

        private void DrawPerformanceTab()
        {
            EditorGUILayout.LabelField("Performance Metrics", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Measure Performance", GUILayout.Width(150)))
            {
                MeasurePerformance();
            }

            EditorGUILayout.Space();

            performanceScrollPosition = EditorGUILayout.BeginScrollView(performanceScrollPosition);

            foreach (var kvp in performanceMetrics)
            {
                DrawPerformanceMetrics(kvp.Key, kvp.Value);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawPerformanceMetrics(string databaseName, PerformanceMetrics metrics)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(databaseName, EditorStyles.boldLabel);
                
                EditorGUILayout.LabelField($"Search Time: {metrics.searchTime:F3}ms");
                EditorGUILayout.LabelField($"Memory Usage: {metrics.memoryUsage} bytes");
                EditorGUILayout.LabelField($"Index Size: {metrics.indexSize}");
                
                EditorGUILayout.Space();
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh All Data"))
                {
                    RefreshAllData();
                }
                
                GUILayout.FlexibleSpace();
                
                EditorGUILayout.LabelField($"Last Updated: {System.DateTime.Now:HH:mm:ss}");
            }
        }

        #endregion

        #region Core Functionality

        private void RefreshDatabaseCounts()
        {
            databaseCounts.Clear();
            databaseCounts["Entities"] = GameData.entityDatabase.Count;
            databaseCounts["Items"] = GameData.itemDatabase.Count;
            databaseCounts["Traits"] = GameData.traitDatabase.Count;
            databaseCounts["Quests"] = GameData.questDatabase.Count;
            databaseCounts["NPCs"] = GameData.npcDatabase.Count;
            databaseCounts["Skills"] = GameData.skillDatabase.Count;
            databaseCounts["Buffs"] = GameData.buffDatabase.Count;
            databaseCounts["Objectives"] = GameData.objectiveDatabase.Count;
            databaseCounts["Stats"] = GameData.statDatabase.Count;
            databaseCounts["Genetics"] = GameData.geneticsDatabase.Count;
            databaseCounts["Journal"] = GameData.journalDatabase.Count;
        }

        private List<object> GetSampleEntries(string databaseName, int maxCount)
        {
            var entries = new List<object>();
            
            switch (databaseName)
            {
                case "Entities": entries = GameData.entityDatabase.Cast<object>().Take(maxCount).ToList(); break;
                case "Items": entries = GameData.itemDatabase.Cast<object>().Take(maxCount).ToList(); break;
                case "Traits": entries = GameData.traitDatabase.Cast<object>().Take(maxCount).ToList(); break;
                case "Quests": entries = GameData.questDatabase.Cast<object>().Take(maxCount).ToList(); break;
                case "NPCs": entries = GameData.npcDatabase.Cast<object>().Take(maxCount).ToList(); break;
                case "Skills": entries = GameData.skillDatabase.Cast<object>().Take(maxCount).ToList(); break;
                case "Buffs": entries = GameData.buffDatabase.Cast<object>().Take(maxCount).ToList(); break;
                case "Objectives": entries = GameData.objectiveDatabase.Cast<object>().Take(maxCount).ToList(); break;
                case "Stats": entries = GameData.statDatabase.Cast<object>().Take(maxCount).ToList(); break;
                case "Genetics": entries = GameData.geneticsDatabase.Cast<object>().Take(maxCount).ToList(); break;
                case "Journal": entries = GameData.journalDatabase.Cast<object>().Take(maxCount).ToList(); break;
            }
            
            return entries;
        }

        private string GetEntityDisplayName(object entity)
        {
            switch (entity)
            {
                case Entity e: return e.entityName;
                case Item i: return i.itemName;
                case Trait t: return t.traitName;
                case Quest q: return q.questName;
                case NPC n: return n.npcName;
                case Skill s: return s.skillName.ToString();
                case Buff b: return b.buffName;
                case Stat st: return st.statName;
                default: return entity?.ToString() ?? "Unknown";
            }
        }

        private void PerformSearch()
        {
            searchResults.Clear();
            
            if (string.IsNullOrWhiteSpace(searchQuery))
                return;
                
            var allEntities = GetAllEntitiesForScope();
            
            foreach (var entity in allEntities)
            {
                var result = SearchEntity(entity);
                if (result != null)
                {
                    searchResults.Add(result);
                }
            }
        }

        private List<object> GetAllEntitiesForScope()
        {
            var entities = new List<object>();
            
            if (searchScope == SearchScope.All || searchScope == SearchScope.Entities)
                entities.AddRange(GameData.entityDatabase.Cast<object>());
            if (searchScope == SearchScope.All || searchScope == SearchScope.Items)
                entities.AddRange(GameData.itemDatabase.Cast<object>());
            if (searchScope == SearchScope.All || searchScope == SearchScope.Traits)
                entities.AddRange(GameData.traitDatabase.Cast<object>());
            if (searchScope == SearchScope.All || searchScope == SearchScope.Quests)
                entities.AddRange(GameData.questDatabase.Cast<object>());
            if (searchScope == SearchScope.All || searchScope == SearchScope.NPCs)
                entities.AddRange(GameData.npcDatabase.Cast<object>());
            if (searchScope == SearchScope.All || searchScope == SearchScope.Skills)
                entities.AddRange(GameData.skillDatabase.Cast<object>());
            if (searchScope == SearchScope.All || searchScope == SearchScope.Buffs)
                entities.AddRange(GameData.buffDatabase.Cast<object>());
            if (searchScope == SearchScope.All || searchScope == SearchScope.Objectives)
                entities.AddRange(GameData.objectiveDatabase.Cast<object>());
            if (searchScope == SearchScope.All || searchScope == SearchScope.Stats)
                entities.AddRange(GameData.statDatabase.Cast<object>());
            if (searchScope == SearchScope.All || searchScope == SearchScope.Genetics)
                entities.AddRange(GameData.geneticsDatabase.Cast<object>());
            if (searchScope == SearchScope.All || searchScope == SearchScope.Journal)
                entities.AddRange(GameData.journalDatabase.Cast<object>());
                
            return entities;
        }

        private SearchResult SearchEntity(object entity)
        {
            bool matches = false;
            string matchingContent = "";
            string propertyName = "";
            string entityName = GetEntityDisplayName(entity);
            
            switch (searchType)
            {
                case SearchType.Global:
                    matches = SearchInName(entityName) || SearchInAllProperties(entity, out matchingContent, out propertyName);
                    break;
                case SearchType.ByName:
                    matches = SearchInName(entityName);
                    matchingContent = entityName;
                    break;
                case SearchType.ByProperty:
                    matches = SearchInSpecificProperty(entity, out matchingContent, out propertyName);
                    break;
                case SearchType.ByContent:
                    matches = SearchInContent(entity, out matchingContent);
                    break;
            }
            
            if (matches)
            {
                return new SearchResult
                {
                    entity = entity,
                    databaseType = GetDatabaseType(entity),
                    entityName = entityName,
                    propertyName = propertyName,
                    matchingContent = matchingContent,
                    relevanceScore = CalculateRelevanceScore(entityName, matchingContent)
                };
            }
            
            return null;
        }

        private bool SearchInAllProperties(object entity, out string matchingContent, out string propertyName)
        {
            matchingContent = "";
            propertyName = "";
            
            if (entity == null)
                return false;
                
            Type type = entity.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(entity);
                    if (value != null)
                    {
                        string stringValue = value.ToString();
                        if (MatchesQuery(stringValue))
                        {
                            matchingContent = stringValue;
                            propertyName = prop.Name;
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    // Skip properties that throw exceptions when accessed
                }
            }
            
            return false;
        }

        private bool SearchInName(string entityName)
        {
            return !string.IsNullOrEmpty(entityName) && MatchesQuery(entityName);
        }

        private bool SearchInSpecificProperty(object entity, out string matchingContent, out string propertyName)
        {
            matchingContent = "";
            propertyName = "";
            
            if (entity == null)
                return false;
                
            // In a real implementation, you might want to show a dropdown to select which property to search
            // For simplicity, let's search commonly used properties like "name", "description", etc.
            string[] commonProperties = { "name", "description", "title", "content", "text", "value" };
            
            Type type = entity.GetType();
            
            foreach (var commonProp in commonProperties)
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                   .Where(p => p.Name.ToLower().Contains(commonProp));
                
                foreach (var prop in properties)
                {
                    try
                    {
                        var value = prop.GetValue(entity);
                        if (value != null)
                        {
                            string stringValue = value.ToString();
                            if (MatchesQuery(stringValue))
                            {
                                matchingContent = stringValue;
                                propertyName = prop.Name;
                                return true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Skip properties that throw exceptions when accessed
                    }
                }
            }
            
            return false;
        }

        private bool SearchInContent(object entity, out string matchingContent)
        {
            matchingContent = "";
            
            if (entity == null)
                return false;
                
            // Convert entity to JSON or string representation to search through all its content
            string entityContent = JsonUtility.ToJson(entity);
            
            if (MatchesQuery(entityContent))
            {
                matchingContent = entityContent.Substring(0, Math.Min(50, entityContent.Length)) + "...";
                return true;
            }
            
            return false;
        }

        private bool MatchesQuery(string text)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchQuery))
                return false;
                
            StringComparison comparison = caseSensitive ? 
                StringComparison.Ordinal : 
                StringComparison.OrdinalIgnoreCase;
            
            if (useRegex)
            {
                try
                {
                    var regex = new System.Text.RegularExpressions.Regex(
                        searchQuery, 
                        caseSensitive ? System.Text.RegularExpressions.RegexOptions.None : System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );
                    return regex.IsMatch(text);
                }
                catch
                {
                    // If regex pattern is invalid, fall back to simple string comparison
                    return text.IndexOf(searchQuery, comparison) >= 0;
                }
            }
            else
            {
                return text.IndexOf(searchQuery, comparison) >= 0;
            }
        }

        private float CalculateRelevanceScore(string entityName, string matchingContent)
        {
            float score = 1.0f;
            
            // Boost score if the match is in the entity name
            if (entityName.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                score += 2.0f;
                
                // Even higher score for exact name match
                if (string.Equals(entityName, searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score += 3.0f;
                }
            }
            
            // Add score based on matching content
            if (!string.IsNullOrEmpty(matchingContent))
            {
                int index = matchingContent.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    // Higher score if match is at the beginning of a field
                    if (index == 0)
                    {
                        score += 1.0f;
                    }
                    
                    // Add score for multiple matches
                    int matchCount = 0;
                    int startIndex = 0;
                    while ((startIndex = matchingContent.IndexOf(searchQuery, startIndex, StringComparison.OrdinalIgnoreCase)) >= 0)
                    {
                        matchCount++;
                        startIndex += searchQuery.Length;
                    }
                    
                    score += matchCount * 0.2f;
                }
            }
            
            return score;
        }        private string GetDatabaseType(object entity)
        {
            switch (entity)
            {
                case Entity: return "Entity";
                case Item: return "Item";
                case Trait: return "Trait";
                case Quest: return "Quest";
                case NPC: return "NPC";
                case Skill: return "Skill";
                case Buff: return "Buff";
                case Stat: return "Stat";
                case JournalEntry: return "Journal";
                case Genetics: return "Genetics";
                case Objective: return "Objective";
                default: return "Unknown";
            }
        }

        private void AnalyzeRelationships()
        {
            relationships.Clear();
            
            if (selectedEntity == null)
                return;
                
            // Find incoming relationships (where other entities reference this one)
            if (showIncomingRefs)
            {
                var allEntities = GetAllEntitiesForScope();
                foreach (var entity in allEntities)
                {
                    if (entity == selectedEntity)
                        continue;
                        
                    FindRelationships(entity, selectedEntity, true);
                }
            }
            
            // Find outgoing relationships (where this entity references others)
            if (showOutgoingRefs)
            {
                var allEntities = GetAllEntitiesForScope();
                foreach (var entity in allEntities)
                {
                    if (entity == selectedEntity)
                        continue;
                        
                    FindRelationships(selectedEntity, entity, false);
                }
            }
        }
        
        private void FindRelationships(object fromEntity, object toEntity, bool isIncoming)
        {
            if (fromEntity == null || toEntity == null)
                return;
                
            Type fromType = fromEntity.GetType();
            string toEntityName = GetEntityDisplayName(toEntity);
            
            // Check reference properties
            foreach (var prop in fromType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    // Skip complex types that might cause stack overflows
                    if (prop.PropertyType == typeof(UnityEngine.Object))
                        continue;
                        
                    var value = prop.GetValue(fromEntity);
                    
                    // Check if property directly references the target entity
                    if (value == toEntity)
                    {
                        relationships.Add(new RelationshipInfo
                        {
                            fromEntity = fromEntity,
                            toEntity = toEntity,
                            relationshipType = "Direct Reference",
                            propertyPath = prop.Name,
                            isIncoming = isIncoming
                        });
                        continue;
                    }
                    
                    // Check if it's a string property containing the entity's name
                    if (value is string strValue && toEntityName != null)
                    {
                        if (strValue.Contains(toEntityName))
                        {
                            relationships.Add(new RelationshipInfo
                            {
                                fromEntity = fromEntity,
                                toEntity = toEntity,
                                relationshipType = "Name Reference",
                                propertyPath = prop.Name,
                                isIncoming = isIncoming
                            });
                            continue;
                        }
                    }
                    
                    // Check if it's a collection containing the entity
                    if (value is IEnumerable<object> collection)
                    {
                        if (collection.Contains(toEntity))
                        {
                            relationships.Add(new RelationshipInfo
                            {
                                fromEntity = fromEntity,
                                toEntity = toEntity,
                                relationshipType = "Collection Reference",
                                propertyPath = prop.Name,
                                isIncoming = isIncoming
                            });
                            continue;
                        }
                    }
                }
                catch (Exception)
                {
                    // Skip properties that throw exceptions when accessed
                }
            }
        }

        private void CalculateStatistics()
        {
            databaseStats.Clear();
            
            // Calculate statistics for each database type
            CalculateDatabaseStatistics("Entities", GameData.entityDatabase.Cast<object>().ToList());
            CalculateDatabaseStatistics("Items", GameData.itemDatabase.Cast<object>().ToList());
            CalculateDatabaseStatistics("Traits", GameData.traitDatabase.Cast<object>().ToList());
            CalculateDatabaseStatistics("Quests", GameData.questDatabase.Cast<object>().ToList());
            CalculateDatabaseStatistics("NPCs", GameData.npcDatabase.Cast<object>().ToList());
            CalculateDatabaseStatistics("Skills", GameData.skillDatabase.Cast<object>().ToList());
            CalculateDatabaseStatistics("Buffs", GameData.buffDatabase.Cast<object>().ToList());
            CalculateDatabaseStatistics("Objectives", GameData.objectiveDatabase.Cast<object>().ToList());
            CalculateDatabaseStatistics("Stats", GameData.statDatabase.Cast<object>().ToList());
            CalculateDatabaseStatistics("Genetics", GameData.geneticsDatabase.Cast<object>().ToList());
            CalculateDatabaseStatistics("Journal", GameData.journalDatabase.Cast<object>().ToList());
        }

        private void CalculateDatabaseStatistics(string databaseName, List<object> entities)
        {
            var stats = new DatabaseStatistics();
            
            if (entities.Count > 0)
            {
                stats.totalEntries = entities.Count;
                
                // Calculate average properties per entry
                int totalProperties = 0;
                foreach (var entity in entities)
                {
                    if (entity != null)
                    {
                        var type = entity.GetType();
                        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        totalProperties += properties.Length;
                    }
                }
                stats.avgPropertiesPerEntry = totalProperties / Math.Max(1, entities.Count);
                
                // Track property usage
                stats.propertyUsage = new Dictionary<string, int>();
                foreach (var entity in entities)
                {
                    if (entity != null)
                    {
                        var type = entity.GetType();
                        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var prop in properties)
                        {
                            string propName = prop.Name;
                            if (!stats.propertyUsage.ContainsKey(propName))
                            {
                                stats.propertyUsage[propName] = 0;
                            }
                            stats.propertyUsage[propName]++;
                        }
                    }
                }
                
                // Check for duplicates based on names
                stats.duplicateEntries = new List<string>();
                var nameSet = new HashSet<string>();
                foreach (var entity in entities)
                {
                    string name = GetEntityDisplayName(entity);
                    if (!string.IsNullOrEmpty(name))
                    {
                        if (nameSet.Contains(name))
                        {
                            stats.duplicateEntries.Add(name);
                        }
                        else
                        {
                            nameSet.Add(name);
                        }
                    }
                }
                
                // Track extreme values
                stats.extremeValues = new Dictionary<string, object>();
                // Example: find max/min values for common number properties
                foreach (var entity in entities)
                {
                    if (entity != null)
                    {
                        var type = entity.GetType();
                        
                        // Look for some common properties like level, value, etc.
                        string[] commonNumericProps = { "level", "value", "health", "power", "count", "amount" };
                        
                        foreach (var propName in commonNumericProps)
                        {
                            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.Name.ToLower().Contains(propName));
                            
                            foreach (var prop in properties)
                            {
                                try
                                {
                                    var value = prop.GetValue(entity);
                                    if (value is float || value is int || value is double)
                                    {
                                        // Convert to double for comparison
                                        double numValue = Convert.ToDouble(value);
                                        
                                        string maxPropKey = $"max_{prop.Name}";
                                        string minPropKey = $"min_{prop.Name}";
                                        
                                        // Update max value
                                        if (!stats.extremeValues.ContainsKey(maxPropKey) || 
                                            numValue > Convert.ToDouble(stats.extremeValues[maxPropKey]))
                                        {
                                            stats.extremeValues[maxPropKey] = value;
                                        }
                                        
                                        // Update min value
                                        if (!stats.extremeValues.ContainsKey(minPropKey) || 
                                            numValue < Convert.ToDouble(stats.extremeValues[minPropKey]))
                                        {
                                            stats.extremeValues[minPropKey] = value;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    // Skip properties that throw exceptions when accessed
                                }
                            }
                        }
                    }
                }
            }
            
            databaseStats[databaseName] = stats;
        }

        private void CheckDatabaseIntegrity()
        {
            integrityIssues.Clear();
            
            CheckForMissingReferences();
            CheckForDuplicateEntries();
            CheckForInvalidData();
            CheckForCircularReferences();
        }

        private void CheckForMissingReferences()
        {
            // Example: Check if quests reference valid objectives
            foreach (var quest in GameData.questDatabase)
            {
                if (quest.objectives != null)
                {
                    foreach (var objective in quest.objectives)
                    {
                        if (GameData.objectiveDatabase.Find(o => o.objectiveID == objective.objectiveID).objectiveID == 0)
                        {
                            integrityIssues.Add(new IntegrityIssue
                            {
                                severity = SeverityLevel.Warning,
                                issueType = "Missing Objective Reference",
                                description = $"Quest '{quest.questName}' references an objective with ID {objective.objectiveID} that doesn't exist in the database.",
                                relatedEntity = quest,
                                suggestedFix = "Remove the invalid objective reference or create the missing objective."
                            });
                        }
                    }
                }
            }
            
            // Check if items reference valid items in their requiredItems list
            foreach (var trait in GameData.traitDatabase)
            {
                if (trait.requiredItems != null)
                {
                    foreach (var itemID in trait.requiredItems)
                    {
                        if (GameData.GetItemByID(itemID).itemID == 0)
                        {
                            integrityIssues.Add(new IntegrityIssue
                            {
                                severity = SeverityLevel.Warning,
                                issueType = "Missing Item Reference",
                                description = $"Trait '{trait.traitName}' requires an item with ID {itemID} that doesn't exist in the database.",
                                relatedEntity = trait,
                                suggestedFix = "Remove the invalid item reference or create the missing item."
                            });
                        }
                    }
                }
            }
            
            // More reference checks could be added here
        }

        private void CheckForDuplicateEntries()
        {
            // Check for duplicate entity names
            var entityNames = new Dictionary<string, List<Entity>>();
            foreach (var entity in GameData.entityDatabase)
            {
                if (!string.IsNullOrEmpty(entity.entityName))
                {
                    if (!entityNames.ContainsKey(entity.entityName))
                    {
                        entityNames[entity.entityName] = new List<Entity>();
                    }
                    entityNames[entity.entityName].Add(entity);
                }
            }
            
            foreach (var kvp in entityNames)
            {
                if (kvp.Value.Count > 1)
                {
                    integrityIssues.Add(new IntegrityIssue
                    {
                        severity = SeverityLevel.Warning,
                        issueType = "Duplicate Entity Name",
                        description = $"There are {kvp.Value.Count} entities with the name '{kvp.Key}'.",
                        relatedEntity = kvp.Value[0],
                        suggestedFix = "Rename the entities to ensure unique names."
                    });
                }
            }
            
            // Check for duplicate item names
            var itemNames = new Dictionary<string, List<Item>>();
            foreach (var item in GameData.itemDatabase)
            {
                if (!string.IsNullOrEmpty(item.itemName))
                {
                    if (!itemNames.ContainsKey(item.itemName))
                    {
                        itemNames[item.itemName] = new List<Item>();
                    }
                    itemNames[item.itemName].Add(item);
                }
            }
            
            foreach (var kvp in itemNames)
            {
                if (kvp.Value.Count > 1)
                {
                    integrityIssues.Add(new IntegrityIssue
                    {
                        severity = SeverityLevel.Warning,
                        issueType = "Duplicate Item Name",
                        description = $"There are {kvp.Value.Count} items with the name '{kvp.Key}'.",
                        relatedEntity = kvp.Value[0],
                        suggestedFix = "Rename the items to ensure unique names."
                    });
                }
            }
            
            // More duplicate checks could be added here
        }

        private void CheckForInvalidData()
        {
            // Check for entities with empty names
            foreach (var entity in GameData.entityDatabase)
            {
                if (string.IsNullOrWhiteSpace(entity.entityName))
                {
                    integrityIssues.Add(new IntegrityIssue
                    {
                        severity = SeverityLevel.Error,
                        issueType = "Invalid Entity Name",
                        description = $"Entity with ID {entity.entityID} has an empty name.",
                        relatedEntity = entity,
                        suggestedFix = "Set a valid name for the entity."
                    });
                }
            }
            
            // Check for entities with invalid stat values
            foreach (var entity in GameData.entityDatabase)
            {                // Check for valid health - this depends on your Health class structure
                // This is just a placeholder check
                var healthProp = entity.GetType().GetField("health");
                if (healthProp != null)
                {
                    var health = healthProp.GetValue(entity);
                    if (health != null && health.ToString().Contains("-"))
                    {
                        integrityIssues.Add(new IntegrityIssue
                        {
                            severity = SeverityLevel.Warning,
                            issueType = "Invalid Health Value",
                            description = $"Entity '{entity.entityName}' may have a negative health value.",
                            relatedEntity = entity,
                            suggestedFix = "Set a non-negative health value."
                        });
                    }
                }
            }
            
            // Check for items with invalid values
            foreach (var item in GameData.itemDatabase)
            {
                if (item.value < 0)
                {
                    integrityIssues.Add(new IntegrityIssue
                    {
                        severity = SeverityLevel.Info,
                        issueType = "Negative Item Value",
                        description = $"Item '{item.itemName}' has a negative value: {item.value}.",
                        relatedEntity = item,
                        suggestedFix = "Set a non-negative value for the item."
                    });
                }
                
                if (item.weight < 0)
                {
                    integrityIssues.Add(new IntegrityIssue
                    {
                        severity = SeverityLevel.Info,
                        issueType = "Negative Item Weight",
                        description = $"Item '{item.itemName}' has a negative weight: {item.weight}.",
                        relatedEntity = item,
                        suggestedFix = "Set a non-negative weight for the item."
                    });
                }
            }
            
            // More invalid data checks could be added here
        }

        private void CheckForCircularReferences()
        {
            // Example: Check for circular dependencies in traits requiring other traits
            foreach (var trait in GameData.traitDatabase)
            {
                if (trait.requiredTraits != null && trait.requiredTraits.Count > 0)
                {
                    CheckTraitCircularDependency(trait, new HashSet<Trait.ID>(), new List<string>());
                }
            }
        }
        
        private void CheckTraitCircularDependency(Trait trait, HashSet<Trait.ID> visitedTraits, List<string> path)
        {
            if (visitedTraits.Contains(trait.traitID))
            {
                // Circular dependency detected
                path.Add(trait.traitName);
                string pathString = string.Join(" -> ", path);
                
                integrityIssues.Add(new IntegrityIssue
                {
                    severity = SeverityLevel.Error,
                    issueType = "Circular Trait Dependency",
                    description = $"Circular dependency detected in trait requirements: {pathString}.",
                    relatedEntity = trait,
                    suggestedFix = "Break the circular dependency by removing one of the trait requirements."
                });
                return;
            }
            
            visitedTraits.Add(trait.traitID);
            path.Add(trait.traitName);
            
            foreach (var requiredTrait in trait.requiredTraits)
            {
                CheckTraitCircularDependency(requiredTrait, new HashSet<Trait.ID>(visitedTraits), new List<string>(path));
            }
        }

        private void FixIntegrityIssues()
        {
            List<IntegrityIssue> issuesFixed = new List<IntegrityIssue>();
            
            foreach (var issue in integrityIssues)
            {
                if (!string.IsNullOrEmpty(issue.suggestedFix))
                {                    bool isFixed = ApplyIntegrityFix(issue);
                    if (isFixed)
                    {
                        issuesFixed.Add(issue);
                    }
                }
            }
            
            // Remove fixed issues
            foreach (var issue in issuesFixed)
            {
                integrityIssues.Remove(issue);
            }
            
            if (issuesFixed.Count > 0)
            {
                EditorUtility.DisplayDialog("Issues Fixed", $"Fixed {issuesFixed.Count} integrity issues.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Issues Fixed", "No issues could be automatically fixed.", "OK");
            }
        }

        private bool ApplyIntegrityFix(IntegrityIssue issue)
        {
            // Apply fixes based on issue type
            switch (issue.issueType)
            {
                case "Invalid Entity Name":
                    if (issue.relatedEntity is Entity entity)
                    {
                        // Generate a name based on ID
                        var entityType = entity.GetType();
                        var nameField = entityType.GetField("entityName");
                        if (nameField != null)
                        {
                            nameField.SetValue(entity, $"Entity_{entity.entityID}");
                            return true;
                        }
                    }
                    break;
                    
                case "Negative Item Value":
                    if (issue.relatedEntity is Item item && item.value < 0)
                    {
                        // Use reflection to set the value to 0
                        var itemType = item.GetType();
                        var valueField = itemType.GetField("value");
                        if (valueField != null)
                        {
                            valueField.SetValue(item, 0);
                            return true;
                        }
                    }
                    break;
                    
                case "Negative Item Weight":
                    if (issue.relatedEntity is Item weightItem && weightItem.weight < 0)
                    {
                        // Use reflection to set the weight to 0
                        var itemType = weightItem.GetType();
                        var weightField = itemType.GetField("weight");
                        if (weightField != null)
                        {
                            weightField.SetValue(weightItem, 0f);
                            return true;
                        }
                    }
                    break;
            }
            
            // No fix was applied
            return false;
        }

        private void MeasurePerformance()
        {
            performanceMetrics.Clear();
            
            // Measure search performance
            MeasureSearchPerformance("Entities", GameData.entityDatabase);
            MeasureSearchPerformance("Items", GameData.itemDatabase);
            MeasureSearchPerformance("Traits", GameData.traitDatabase);
            MeasureSearchPerformance("Quests", GameData.questDatabase);
            MeasureSearchPerformance("NPCs", GameData.npcDatabase);
            MeasureSearchPerformance("Skills", GameData.skillDatabase);
            MeasureSearchPerformance("Buffs", GameData.buffDatabase);
            MeasureSearchPerformance("Objectives", GameData.objectiveDatabase);
        }
        
        private void MeasureSearchPerformance<T>(string databaseName, List<T> database)
        {
            if (database == null || database.Count == 0)
                return;
                
            var metrics = new PerformanceMetrics
            {
                queryTimes = new Dictionary<string, float>()
            };
            
            // Measure the time it takes to search the database
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            
            // Benchmark: full scan
            stopwatch.Restart();
            foreach (var item in database)
            {
                // Do a basic check on each item (simulates a basic search operation)
                var type = item.GetType();
                var properties = type.GetProperties();
                foreach (var prop in properties.Take(5)) // Limit to first 5 props for benchmark
                {
                    try { var val = prop.GetValue(item); } catch { }
                }
            }
            stopwatch.Stop();
            metrics.searchTime = stopwatch.ElapsedMilliseconds;
            metrics.queryTimes["FullScan"] = stopwatch.ElapsedMilliseconds;
            
            // Calculate memory usage (rough estimate)
            int estimatedItemSize = 0;
            T sampleItem = database[0];
            var sampleType = sampleItem.GetType();
            foreach (var prop in sampleType.GetProperties())
            {
                if (prop.PropertyType == typeof(int)) estimatedItemSize += 4;
                else if (prop.PropertyType == typeof(float)) estimatedItemSize += 4;
                else if (prop.PropertyType == typeof(bool)) estimatedItemSize += 1;
                else if (prop.PropertyType == typeof(string)) estimatedItemSize += 40; // Rough estimate for a string
                else estimatedItemSize += 8; // Rough estimate for object reference
            }
            
            metrics.memoryUsage = estimatedItemSize * database.Count;
            
            // Estimate index size (if we were to index every property)
            metrics.indexSize = database.Count * sampleType.GetProperties().Length * 12; // Rough estimate
            
            performanceMetrics[databaseName] = metrics;
        }

        private void OpenDatabaseEditor(string databaseName)
        {
            // Open the database editor focused on the selected database
            DatabaseEditorWindow window = EditorWindow.GetWindow<DatabaseEditorWindow>();
            
            // Set the selected database type - the exact implementation depends on your DatabaseEditorWindow structure
            // This is just a placeholder implementation
            var selectDatabaseMethod = typeof(DatabaseEditorWindow).GetMethod("SelectDatabase", 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
            if (selectDatabaseMethod != null)
            {
                selectDatabaseMethod.Invoke(window, new object[] { databaseName });
            }
            else
            {
                // Fallback if no direct method is available
                window.Show();
                EditorUtility.DisplayDialog("Database Selected", 
                    $"The database editor has been opened. Please select the '{databaseName}' database manually.", "OK");
            }
        }

        private void ExportDatabaseSummary()
        {
            string path = EditorUtility.SaveFilePanel("Export Database Summary", "", "DatabaseSummary.txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                string summary = GenerateDatabaseSummary();
                System.IO.File.WriteAllText(path, summary);
                EditorUtility.RevealInFinder(path);
            }
        }

        private string GenerateDatabaseSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("DATABASE SUMMARY REPORT");
            sb.AppendLine("======================");
            sb.AppendLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            
            // Database Counts
            sb.AppendLine("DATABASE COUNTS");
            sb.AppendLine("--------------");
            foreach (var kvp in databaseCounts)
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value} entries");
            }
            sb.AppendLine();
            
            // Statistics
            if (databaseStats.Count > 0)
            {
                sb.AppendLine("STATISTICS");
                sb.AppendLine("----------");
                foreach (var kvp in databaseStats)
                {
                    sb.AppendLine($"{kvp.Key} Database:");
                    sb.AppendLine($"  - Total Entries: {kvp.Value.totalEntries}");
                    sb.AppendLine($"  - Avg Properties per Entry: {kvp.Value.avgPropertiesPerEntry}");
                    
                    if (kvp.Value.duplicateEntries != null && kvp.Value.duplicateEntries.Count > 0)
                    {
                        sb.AppendLine($"  - Duplicate Entries: {kvp.Value.duplicateEntries.Count}");
                        foreach (var dup in kvp.Value.duplicateEntries.Take(5)) // Show first 5 duplicates at most
                        {
                            sb.AppendLine($"    - {dup}");
                        }
                        if (kvp.Value.duplicateEntries.Count > 5)
                        {
                            sb.AppendLine($"    - ... and {kvp.Value.duplicateEntries.Count - 5} more");
                        }
                    }
                    
                    sb.AppendLine();
                }
            }
            
            // Integrity Issues
            if (integrityIssues.Count > 0)
            {
                sb.AppendLine("INTEGRITY ISSUES");
                sb.AppendLine("----------------");
                
                var issuesBySeverity = integrityIssues.GroupBy(i => i.severity);
                foreach (var group in issuesBySeverity.OrderByDescending(g => g.Key))
                {
                    sb.AppendLine($"{group.Key} Issues: {group.Count()}");
                    
                    foreach (var issue in group.Take(10)) // Show first 10 issues at most
                    {
                        sb.AppendLine($"  - {issue.issueType}: {issue.description}");
                    }
                    
                    if (group.Count() > 10)
                    {
                        sb.AppendLine($"  - ... and {group.Count() - 10} more {group.Key} issues");
                    }
                    
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("INTEGRITY ISSUES");
                sb.AppendLine("----------------");
                sb.AppendLine("No integrity issues found.");
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        private void RefreshAllData()
        {
            // Refresh database counts
            RefreshDatabaseCounts();
            
            // Recalculate statistics
            CalculateStatistics();
            
            // Check integrity if enabled
            if (autoCheckIntegrity)
            {
                CheckDatabaseIntegrity();
            }
            
            // Measure performance metrics
            MeasurePerformance();
        }

        #endregion
    }
}

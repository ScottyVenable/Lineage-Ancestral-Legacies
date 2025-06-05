using UnityEngine;
using UnityEditor;
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
                case Entity e: return e.name;
                case Item i: return i.name;
                case Trait t: return t.name;
                case Quest q: return q.title;
                case NPC n: return n.name;
                case Skill s: return s.name;
                case Buff b: return b.name;
                case QuestObjective o: return o.description;
                case Stat s: return s.name;
                case GeneticTrait g: return g.name;
                case JournalEntry j: return j.title;
                default: return entity?.ToString() ?? "Unknown";
            }
        }

        private void PerformSearch()
        {
            searchResults.Clear();
            
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
            string entityName = GetEntityDisplayName(entity);
            
            bool matches = false;
            string matchingContent = "";
            string propertyName = "";
            
            switch (searchType)
            {
                case SearchType.Global:
                    matches = SearchInAllProperties(entity, out matchingContent, out propertyName);
                    break;
                case SearchType.ByName:
                    matches = SearchInName(entityName);
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
            
            var properties = entity.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    string valueStr = value.ToString();
                    if (MatchesQuery(valueStr))
                    {
                        matchingContent = valueStr;
                        propertyName = prop.Name;
                        return true;
                    }
                }
            }
            
            return false;
        }

        private bool SearchInName(string entityName)
        {
            return MatchesQuery(entityName);
        }

        private bool SearchInSpecificProperty(object entity, out string matchingContent, out string propertyName)
        {
            matchingContent = "";
            propertyName = "";
            
            // This would need to be customized based on specific property searches
            return false;
        }

        private bool SearchInContent(object entity, out string matchingContent)
        {
            matchingContent = "";
            
            // Search in description/content fields
            var contentFields = new[] { "description", "content", "text", "details" };
            var properties = entity.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var prop in properties)
            {
                if (contentFields.Contains(prop.Name.ToLower()))
                {
                    var value = prop.GetValue(entity);
                    if (value != null && MatchesQuery(value.ToString()))
                    {
                        matchingContent = value.ToString();
                        return true;
                    }
                }
            }
            
            return false;
        }

        private bool MatchesQuery(string text)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchQuery))
                return false;
            
            var comparison = caseSensitive ? System.StringComparison.Ordinal : System.StringComparison.OrdinalIgnoreCase;
            
            if (useRegex)
            {
                try
                {
                    var regex = new System.Text.RegularExpressions.Regex(searchQuery, 
                        caseSensitive ? System.Text.RegularExpressions.RegexOptions.None : System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    return regex.IsMatch(text);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return text.IndexOf(searchQuery, comparison) >= 0;
            }
        }

        private float CalculateRelevanceScore(string entityName, string matchingContent)
        {
            float score = 0f;
            
            if (entityName.IndexOf(searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0)
                score += 1.0f;
            
            if (!string.IsNullOrEmpty(matchingContent))
            {
                int index = matchingContent.IndexOf(searchQuery, System.StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    // Earlier matches get higher scores
                    score += 0.5f * (1.0f - (float)index / matchingContent.Length);
                }
            }
            
            return score;
        }

        private string GetDatabaseType(object entity)
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
                case QuestObjective: return "Objective";
                case Stat: return "Stat";
                case GeneticTrait: return "Genetic";
                case JournalEntry: return "Journal";
                default: return "Unknown";
            }
        }

        private void AnalyzeRelationships()
        {
            relationships.Clear();
            
            if (selectedEntity == null) return;
            
            // This would analyze relationships between entities
            // For now, this is a placeholder implementation
            
            // TODO: Implement comprehensive relationship analysis
            // - Find references to this entity in other entities
            // - Find entities this entity references
            // - Build dependency graph
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
            var stats = new DatabaseStatistics
            {
                totalEntries = entities.Count,
                propertyUsage = new Dictionary<string, int>(),
                duplicateEntries = new List<string>(),
                extremeValues = new Dictionary<string, object>()
            };
            
            if (entities.Count > 0)
            {
                // Calculate average properties per entry
                int totalProperties = 0;
                foreach (var entity in entities)
                {
                    var properties = entity.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                    totalProperties += properties.Length;
                }
                stats.avgPropertiesPerEntry = totalProperties / entities.Count;
            }
            
            databaseStats[databaseName] = stats;
        }

        private void CheckDatabaseIntegrity()
        {
            integrityIssues.Clear();
            
            // Check for common integrity issues
            CheckForMissingReferences();
            CheckForDuplicateEntries();
            CheckForInvalidData();
            CheckForCircularReferences();
        }

        private void CheckForMissingReferences()
        {
            // TODO: Implement missing reference checks
        }

        private void CheckForDuplicateEntries()
        {
            // TODO: Implement duplicate entry checks
        }

        private void CheckForInvalidData()
        {
            // TODO: Implement invalid data checks
        }

        private void CheckForCircularReferences()
        {
            // TODO: Implement circular reference checks
        }

        private void FixIntegrityIssues()
        {
            foreach (var issue in integrityIssues)
            {
                ApplyIntegrityFix(issue);
            }
            
            CheckDatabaseIntegrity(); // Re-check after fixes
        }

        private void ApplyIntegrityFix(IntegrityIssue issue)
        {
            // TODO: Implement integrity fix application
        }

        private void MeasurePerformance()
        {
            performanceMetrics.Clear();
            
            // TODO: Implement performance measurement
            // - Measure search times
            // - Calculate memory usage
            // - Analyze query performance
        }

        private void OpenDatabaseEditor(string databaseName)
        {
            DatabaseEditorWindow.ShowWindow();
        }

        private void ExportDatabaseSummary()
        {
            string path = EditorUtility.SaveFilePanel("Export Database Summary", "", "database_summary.txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                var summary = GenerateDatabaseSummary();
                System.IO.File.WriteAllText(path, summary);
                EditorUtility.DisplayDialog("Export Complete", $"Database summary exported to:\n{path}", "OK");
            }
        }

        private string GenerateDatabaseSummary()
        {
            var summary = new System.Text.StringBuilder();
            summary.AppendLine("Database Summary");
            summary.AppendLine("================");
            summary.AppendLine($"Generated: {System.DateTime.Now}");
            summary.AppendLine();
            
            foreach (var kvp in databaseCounts)
            {
                summary.AppendLine($"{kvp.Key}: {kvp.Value} entries");
            }
            
            return summary.ToString();
        }

        private void RefreshAllData()
        {
            RefreshDatabaseCounts();
            CalculateStatistics();
            if (autoCheckIntegrity)
            {
                CheckDatabaseIntegrity();
            }
        }

        #endregion
    }
}

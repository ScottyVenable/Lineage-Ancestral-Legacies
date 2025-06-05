using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Content Statistics and Reporting Window for analyzing game content distribution,
    /// usage patterns, and generating comprehensive reports about database content.
    /// </summary>
    public class ContentStatisticsWindow : EditorWindow
    {
        #region Window Management
        
        [MenuItem("Lineage/Studio/Analysis/Content Statistics Report")]
        public static void ShowWindow()
        {
            var window = GetWindow<ContentStatisticsWindow>("Content Statistics");
            window.minSize = new Vector2(900, 600);
            window.Show();
        }
        
        #endregion
        
        #region UI State
        
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Overview", "Entities", "Traits", "Items", "Relationships", "Reports" };
        private Vector2 _scrollPosition = Vector2.zero;
        
        // Statistics Data
        private ContentStats _contentStats;
        private bool _statsCalculated = false;
        private DateTime _lastCalculated;
        
        // Report Generation
        private StringBuilder _reportBuffer = new StringBuilder();
        private string _exportPath = "";
        private ReportFormat _selectedFormat = ReportFormat.Markdown;
        
        #endregion
        
        #region Data Structures
        
        private enum ReportFormat
        {
            Markdown,
            HTML,
            CSV,
            JSON
        }
        
        private class ContentStats
        {
            public int TotalEntities { get; set; }
            public int TotalTraits { get; set; }
            public int TotalItems { get; set; }
            public int TotalRelationships { get; set; }
            
            public Dictionary<string, int> EntityTypeDistribution { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> TraitCategoryDistribution { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> ItemCategoryDistribution { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> RelationshipTypeDistribution { get; set; } = new Dictionary<string, int>();
            
            public Dictionary<string, float> TraitUsageFrequency { get; set; } = new Dictionary<string, float>();
            public Dictionary<string, float> ItemUsageFrequency { get; set; } = new Dictionary<string, float>();
            
            public List<string> MostUsedTraits { get; set; } = new List<string>();
            public List<string> LeastUsedTraits { get; set; } = new List<string>();
            public List<string> UnusedTraits { get; set; } = new List<string>();
            
            public List<string> OrphanedEntities { get; set; } = new List<string>();
            public List<string> HighlyConnectedEntities { get; set; } = new List<string>();
            
            public float AverageTraitsPerEntity { get; set; }
            public float AverageRelationshipsPerEntity { get; set; }
            public float DatabaseComplexityScore { get; set; }
        }
        
        #endregion
        
        #region Unity Events
        
        private void OnEnable()
        {
            _exportPath = Application.dataPath + "/Reports/";
        }
        
        private void OnGUI()
        {
            DrawHeader();
            DrawTabs();
            
            GUILayout.Space(10);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            switch (_selectedTab)
            {
                case 0: DrawOverviewTab(); break;
                case 1: DrawEntitiesTab(); break;
                case 2: DrawTraitsTab(); break;
                case 3: DrawItemsTab(); break;
                case 4: DrawRelationshipsTab(); break;
                case 5: DrawReportsTab(); break;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        #endregion
        
        #region UI Drawing
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Content Statistics & Reports", EditorStyles.boldLabel);
            GUILayout.Label("Analyze content distribution, usage patterns, and generate comprehensive reports", EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Calculate Statistics", GUILayout.Width(150)))
            {
                CalculateStatistics();
            }
            
            GUI.enabled = _statsCalculated;
            if (GUILayout.Button("Refresh Data", GUILayout.Width(100)))
            {
                CalculateStatistics();
            }
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            
            if (_statsCalculated)
            {
                GUILayout.Label($"Last Updated: {_lastCalculated:HH:mm:ss}", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTabs()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
        }
        
        private void DrawOverviewTab()
        {
            if (!_statsCalculated)
            {
                EditorGUILayout.HelpBox("Click 'Calculate Statistics' to generate content overview.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Database Overview", EditorStyles.boldLabel);
            
            // Summary Statistics
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Content Summary", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            DrawStatCard("Total Entities", _contentStats.TotalEntities.ToString(), Color.cyan);
            DrawStatCard("Total Traits", _contentStats.TotalTraits.ToString(), Color.green);
            DrawStatCard("Total Items", _contentStats.TotalItems.ToString(), Color.yellow);
            DrawStatCard("Total Relationships", _contentStats.TotalRelationships.ToString(), Color.magenta);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Quality Metrics
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Quality Metrics", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            DrawMetricBar("Avg Traits/Entity", _contentStats.AverageTraitsPerEntity, 10f);
            DrawMetricBar("Avg Relations/Entity", _contentStats.AverageRelationshipsPerEntity, 5f);
            DrawMetricBar("Complexity Score", _contentStats.DatabaseComplexityScore, 100f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Distribution Charts
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Content Distribution", EditorStyles.boldLabel);
            
            if (_contentStats.EntityTypeDistribution.Any())
            {
                GUILayout.Label("Entity Types:", EditorStyles.boldLabel);
                DrawDistributionChart(_contentStats.EntityTypeDistribution);
            }
            
            GUILayout.Space(5);
            
            if (_contentStats.TraitCategoryDistribution.Any())
            {
                GUILayout.Label("Trait Categories:", EditorStyles.boldLabel);
                DrawDistributionChart(_contentStats.TraitCategoryDistribution);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawEntitiesTab()
        {
            if (!_statsCalculated)
            {
                EditorGUILayout.HelpBox("Calculate statistics first to view entity analysis.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Entity Analysis", EditorStyles.boldLabel);
            
            // Entity Type Distribution
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Entity Type Distribution", EditorStyles.boldLabel);
            
            foreach (var kvp in _contentStats.EntityTypeDistribution.OrderByDescending(x => x.Value))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(kvp.Key, GUILayout.Width(150));
                
                float percentage = (float)kvp.Value / _contentStats.TotalEntities * 100f;
                DrawProgressBar(percentage / 100f, $"{kvp.Value} ({percentage:F1}%)");
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Entity Connection Analysis
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Connection Analysis", EditorStyles.boldLabel);
            
            if (_contentStats.HighlyConnectedEntities.Any())
            {
                GUILayout.Label("Highly Connected Entities (Hub Entities):", EditorStyles.boldLabel);
                foreach (var entity in _contentStats.HighlyConnectedEntities.Take(10))
                {
                    EditorGUILayout.LabelField("• " + entity, EditorStyles.miniLabel);
                }
            }
            
            GUILayout.Space(5);
            
            if (_contentStats.OrphanedEntities.Any())
            {
                GUILayout.Label("Orphaned Entities (No Relationships):", EditorStyles.boldLabel);
                foreach (var entity in _contentStats.OrphanedEntities.Take(10))
                {
                    EditorGUILayout.LabelField("• " + entity, EditorStyles.miniLabel);
                }
                
                if (_contentStats.OrphanedEntities.Count > 10)
                {
                    EditorGUILayout.LabelField($"... and {_contentStats.OrphanedEntities.Count - 10} more", EditorStyles.miniLabel);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTraitsTab()
        {
            if (!_statsCalculated)
            {
                EditorGUILayout.HelpBox("Calculate statistics first to view trait analysis.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Trait Usage Analysis", EditorStyles.boldLabel);
            
            // Most Used Traits
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Most Popular Traits", EditorStyles.boldLabel);
            
            foreach (var trait in _contentStats.MostUsedTraits.Take(15))
            {
                if (_contentStats.TraitUsageFrequency.TryGetValue(trait, out float frequency))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(trait, GUILayout.Width(200));
                    DrawProgressBar(frequency / 100f, $"{frequency:F1}% usage");
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Least Used Traits
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Underutilized Traits", EditorStyles.boldLabel);
            
            foreach (var trait in _contentStats.LeastUsedTraits.Take(10))
            {
                if (_contentStats.TraitUsageFrequency.TryGetValue(trait, out float frequency))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(trait, GUILayout.Width(200));
                    GUILayout.Label($"{frequency:F1}% usage", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Unused Traits
            if (_contentStats.UnusedTraits.Any())
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label($"Unused Traits ({_contentStats.UnusedTraits.Count})", EditorStyles.boldLabel);
                
                foreach (var trait in _contentStats.UnusedTraits.Take(15))
                {
                    EditorGUILayout.LabelField("• " + trait, EditorStyles.miniLabel);
                }
                
                if (_contentStats.UnusedTraits.Count > 15)
                {
                    EditorGUILayout.LabelField($"... and {_contentStats.UnusedTraits.Count - 15} more", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawItemsTab()
        {
            if (!_statsCalculated)
            {
                EditorGUILayout.HelpBox("Calculate statistics first to view item analysis.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Item Analysis", EditorStyles.boldLabel);
            
            // Item Category Distribution
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Item Category Distribution", EditorStyles.boldLabel);
            
            foreach (var kvp in _contentStats.ItemCategoryDistribution.OrderByDescending(x => x.Value))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(kvp.Key, GUILayout.Width(150));
                
                float percentage = (float)kvp.Value / _contentStats.TotalItems * 100f;
                DrawProgressBar(percentage / 100f, $"{kvp.Value} ({percentage:F1}%)");
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Item Usage Frequency
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Item Usage Patterns", EditorStyles.boldLabel);
            
            var sortedItems = _contentStats.ItemUsageFrequency.OrderByDescending(x => x.Value).Take(20);
            foreach (var kvp in sortedItems)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(kvp.Key, GUILayout.Width(200));
                DrawProgressBar(kvp.Value / 100f, $"{kvp.Value:F1}% usage");
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawRelationshipsTab()
        {
            if (!_statsCalculated)
            {
                EditorGUILayout.HelpBox("Calculate statistics first to view relationship analysis.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Relationship Analysis", EditorStyles.boldLabel);
            
            // Relationship Type Distribution
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Relationship Type Distribution", EditorStyles.boldLabel);
            
            foreach (var kvp in _contentStats.RelationshipTypeDistribution.OrderByDescending(x => x.Value))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(kvp.Key, GUILayout.Width(150));
                
                float percentage = (float)kvp.Value / _contentStats.TotalRelationships * 100f;
                DrawProgressBar(percentage / 100f, $"{kvp.Value} ({percentage:F1}%)");
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Network Analysis
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Network Metrics", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField($"Average Relationships per Entity: {_contentStats.AverageRelationshipsPerEntity:F2}");
            EditorGUILayout.LabelField($"Network Density: {(_contentStats.TotalRelationships / (float)(_contentStats.TotalEntities * (_contentStats.TotalEntities - 1))):P2}");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawReportsTab()
        {
            EditorGUILayout.LabelField("Report Generation", EditorStyles.boldLabel);
            
            // Export Settings
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Export Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Format:", GUILayout.Width(60));
            _selectedFormat = (ReportFormat)EditorGUILayout.EnumPopup(_selectedFormat);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Path:", GUILayout.Width(60));
            _exportPath = EditorGUILayout.TextField(_exportPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.SaveFolderPanel("Select Export Folder", _exportPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    _exportPath = path + "/";
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Report Generation Buttons
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Generate Reports", EditorStyles.boldLabel);
            
            GUI.enabled = _statsCalculated;
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Full Statistics Report"))
            {
                GenerateFullReport();
            }
            
            if (GUILayout.Button("Entity Summary"))
            {
                GenerateEntitySummary();
            }
            
            if (GUILayout.Button("Trait Usage Report"))
            {
                GenerateTraitUsageReport();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Quality Assessment"))
            {
                GenerateQualityReport();
            }
            
            if (GUILayout.Button("Optimization Suggestions"))
            {
                GenerateOptimizationReport();
            }
            
            if (GUILayout.Button("Custom Report Builder"))
            {
                // Open custom report builder
                EditorUtility.DisplayDialog("Custom Reports", "Custom report builder coming soon!", "OK");
            }
            EditorGUILayout.EndHorizontal();
            
            GUI.enabled = true;
            
            EditorGUILayout.EndVertical();
            
            // Report Preview
            if (_reportBuffer.Length > 0)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Report Preview", EditorStyles.boldLabel);
                
                EditorGUILayout.TextArea(_reportBuffer.ToString().Substring(0, Mathf.Min(1000, _reportBuffer.Length)), 
                    GUILayout.Height(200));
                
                if (_reportBuffer.Length > 1000)
                {
                    EditorGUILayout.LabelField("... (truncated for preview)");
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        #endregion
        
        #region Statistics Calculation
        
        private void CalculateStatistics()
        {
            _contentStats = new ContentStats();
              if (GameData.entityDatabase == null)
            {
                EditorUtility.DisplayDialog("Error", "GameData instance not found. Initialize the database first.", "OK");
                return;
            }
            
            try
            {
                CalculateBasicStats();
                CalculateDistributions();
                CalculateUsagePatterns();
                CalculateQualityMetrics();
                CalculateConnectionAnalysis();
                
                _statsCalculated = true;
                _lastCalculated = DateTime.Now;
                
                UnityEngine.Debug.Log("[Content Statistics] Statistics calculated successfully.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Content Statistics] Error calculating statistics: {ex.Message}");
                EditorUtility.DisplayDialog("Error", "Failed to calculate statistics. Check console for details.", "OK");
            }
        }
          private void CalculateBasicStats()
        {
            _contentStats.TotalEntities = GameData.entityDatabase.Count;
            _contentStats.TotalTraits = GameData.traitDatabase.Count;
            _contentStats.TotalItems = GameData.itemDatabase.Count;
            _contentStats.TotalRelationships = 0; // Relationships are stored within NPCs, not as separate database
        }
          private void CalculateDistributions()
        {
            // Entity type distribution (mock implementation)
            var entities = GameData.entityDatabase;
            foreach (var entity in entities)
            {
                string type = "Character"; // Placeholder - determine actual type
                _contentStats.EntityTypeDistribution[type] = _contentStats.EntityTypeDistribution.GetValueOrDefault(type) + 1;
            }
            
            // Trait category distribution
            var traits = GameData.traitDatabase;
            foreach (var trait in traits)
            {
                string category = trait.category ?? "Unknown";
                _contentStats.TraitCategoryDistribution[category] = _contentStats.TraitCategoryDistribution.GetValueOrDefault(category) + 1;
            }
            
            // Item category distribution
            var items = GameData.itemDatabase;
            foreach (var item in items)
            {
                string category = item.itemType.ToString();
                _contentStats.ItemCategoryDistribution[category] = _contentStats.ItemCategoryDistribution.GetValueOrDefault(category) + 1;
            }
        }
          private void CalculateUsagePatterns()
        {
            // Calculate trait usage frequency
            var entities = GameData.entityDatabase;
            var traitUsageCount = new Dictionary<string, int>();
              foreach (var entity in entities)
            {
                // Note: Entity-trait relationships need to be implemented in the database
                // Using tags as a proxy for trait relationships since Entity.traits doesn't exist
                // This is a temporary workaround until proper entity-trait relationships are implemented                if (entity.tags != null)
                {
                    var entityTraits = GameData.traitDatabase.Where(t => 
                        entity.tags.Contains(t.traitName) || entity.tags.Contains(t.traitID.ToString())).ToList();
                    foreach (var trait in entityTraits)
                    {
                        traitUsageCount[trait.traitName] = traitUsageCount.GetValueOrDefault(trait.traitName) + 1;
                    }
                }
            }
            
            foreach (var kvp in traitUsageCount)
            {
                float percentage = (float)kvp.Value / _contentStats.TotalEntities * 100f;
                _contentStats.TraitUsageFrequency[kvp.Key] = percentage;
            }
            
            // Sort traits by usage
            var sortedTraits = _contentStats.TraitUsageFrequency.OrderByDescending(x => x.Value).ToList();
            _contentStats.MostUsedTraits = sortedTraits.Take(20).Select(x => x.Key).ToList();
            _contentStats.LeastUsedTraits = sortedTraits.TakeLast(20).Select(x => x.Key).ToList();
            
            // Find unused traits
            var allTraits = GameData.traitDatabase.Select(t => t.traitName).ToHashSet();
            var usedTraits = traitUsageCount.Keys.ToHashSet();
            _contentStats.UnusedTraits = allTraits.Except(usedTraits).ToList();
        }
          private void CalculateQualityMetrics()
        {
            if (_contentStats.TotalEntities > 0)
            {
                // Calculate average traits per entity
                // Note: Since Entity-Trait relationships are not implemented yet,
                // using a placeholder calculation based on available data
                var entities = GameData.entityDatabase;
                var totalTraitAssignments = entities.Sum(e => e.tags?.Count ?? 0);
                _contentStats.AverageTraitsPerEntity = (float)totalTraitAssignments / _contentStats.TotalEntities;
                
                // Calculate average relationships per entity
                _contentStats.AverageRelationshipsPerEntity = (float)_contentStats.TotalRelationships / _contentStats.TotalEntities;
                
                // Calculate complexity score (normalized composite metric)
                _contentStats.DatabaseComplexityScore = (
                    _contentStats.AverageTraitsPerEntity * 5f +
                    _contentStats.AverageRelationshipsPerEntity * 10f +
                    (_contentStats.TotalEntities / 100f) * 2f
                );
            }
        }
          private void CalculateConnectionAnalysis()
        {
            var entityConnections = new Dictionary<string, int>();
            
            // Note: RelationshipDatabase is not implemented yet
            // Using placeholder data until relationship system is available
            var relationships = new List<object>(); // GameData.relationshipDatabase when available
            
            foreach (var relationship in relationships)
            {
                // This would access relationship properties when available
                // entityConnections[relationship.SourceEntityId] = entityConnections.GetValueOrDefault(relationship.SourceEntityId) + 1;
                // entityConnections[relationship.TargetEntityId] = entityConnections.GetValueOrDefault(relationship.TargetEntityId) + 1;
            }
            
            // Find highly connected entities (top 10%)
            var sortedConnections = entityConnections.OrderByDescending(x => x.Value);
            int topCount = Mathf.Max(1, entityConnections.Count / 10);
            _contentStats.HighlyConnectedEntities = sortedConnections.Take(topCount)
                .Select(x => $"{x.Key} ({x.Value} connections)").ToList();
            
            // Find orphaned entities (no relationships)
            var allEntityIds = GameData.entityDatabase.Select(e => e.entityID.ToString()).ToHashSet();
            var connectedEntityIds = entityConnections.Keys.ToHashSet();
            _contentStats.OrphanedEntities = allEntityIds.Except(connectedEntityIds).ToList();
        }
        
        #endregion
        
        #region Report Generation
        
        private void GenerateFullReport()
        {
            _reportBuffer.Clear();
            _reportBuffer.AppendLine("# Content Statistics Report");
            _reportBuffer.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _reportBuffer.AppendLine();
            
            // Summary
            _reportBuffer.AppendLine("## Summary");
            _reportBuffer.AppendLine($"- Total Entities: {_contentStats.TotalEntities}");
            _reportBuffer.AppendLine($"- Total Traits: {_contentStats.TotalTraits}");
            _reportBuffer.AppendLine($"- Total Items: {_contentStats.TotalItems}");
            _reportBuffer.AppendLine($"- Total Relationships: {_contentStats.TotalRelationships}");
            _reportBuffer.AppendLine();
            
            // Quality metrics
            _reportBuffer.AppendLine("## Quality Metrics");
            _reportBuffer.AppendLine($"- Average Traits per Entity: {_contentStats.AverageTraitsPerEntity:F2}");
            _reportBuffer.AppendLine($"- Average Relationships per Entity: {_contentStats.AverageRelationshipsPerEntity:F2}");
            _reportBuffer.AppendLine($"- Database Complexity Score: {_contentStats.DatabaseComplexityScore:F2}");
            _reportBuffer.AppendLine();
            
            SaveReport("full_statistics_report");
        }
        
        private void GenerateEntitySummary()
        {
            _reportBuffer.Clear();
            _reportBuffer.AppendLine("# Entity Summary Report");
            _reportBuffer.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _reportBuffer.AppendLine();
            
            _reportBuffer.AppendLine("## Entity Distribution");
            foreach (var kvp in _contentStats.EntityTypeDistribution.OrderByDescending(x => x.Value))
            {
                float percentage = (float)kvp.Value / _contentStats.TotalEntities * 100f;
                _reportBuffer.AppendLine($"- {kvp.Key}: {kvp.Value} ({percentage:F1}%)");
            }
            
            SaveReport("entity_summary_report");
        }
        
        private void GenerateTraitUsageReport()
        {
            _reportBuffer.Clear();
            _reportBuffer.AppendLine("# Trait Usage Report");
            _reportBuffer.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _reportBuffer.AppendLine();
            
            _reportBuffer.AppendLine("## Most Used Traits");
            foreach (var trait in _contentStats.MostUsedTraits.Take(15))
            {
                if (_contentStats.TraitUsageFrequency.TryGetValue(trait, out float frequency))
                {
                    _reportBuffer.AppendLine($"- {trait}: {frequency:F1}% usage");
                }
            }
            
            _reportBuffer.AppendLine();
            _reportBuffer.AppendLine("## Unused Traits");
            foreach (var trait in _contentStats.UnusedTraits.Take(20))
            {
                _reportBuffer.AppendLine($"- {trait}");
            }
            
            SaveReport("trait_usage_report");
        }
        
        private void GenerateQualityReport()
        {
            _reportBuffer.Clear();
            _reportBuffer.AppendLine("# Database Quality Assessment");
            _reportBuffer.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _reportBuffer.AppendLine();
            
            _reportBuffer.AppendLine("## Quality Score: " + CalculateQualityScore());
            _reportBuffer.AppendLine();
            
            _reportBuffer.AppendLine("## Issues Found");
            if (_contentStats.UnusedTraits.Count > 0)
            {
                _reportBuffer.AppendLine($"- {_contentStats.UnusedTraits.Count} unused traits detected");
            }
            if (_contentStats.OrphanedEntities.Count > 0)
            {
                _reportBuffer.AppendLine($"- {_contentStats.OrphanedEntities.Count} orphaned entities found");
            }
            
            SaveReport("quality_assessment_report");
        }
        
        private void GenerateOptimizationReport()
        {
            _reportBuffer.Clear();
            _reportBuffer.AppendLine("# Optimization Suggestions");
            _reportBuffer.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _reportBuffer.AppendLine();
            
            _reportBuffer.AppendLine("## Recommended Actions");
            if (_contentStats.UnusedTraits.Count > 5)
            {
                _reportBuffer.AppendLine("- Consider removing or utilizing unused traits");
            }
            if (_contentStats.OrphanedEntities.Count > 0)
            {
                _reportBuffer.AppendLine("- Connect orphaned entities to the relationship network");
            }
            if (_contentStats.AverageTraitsPerEntity < 3)
            {
                _reportBuffer.AppendLine("- Consider adding more traits to entities for richer characterization");
            }
            
            SaveReport("optimization_suggestions");
        }
        
        private void SaveReport(string filename)
        {
            try
            {
                if (!System.IO.Directory.Exists(_exportPath))
                {
                    System.IO.Directory.CreateDirectory(_exportPath);
                }
                
                string extension = _selectedFormat switch
                {
                    ReportFormat.Markdown => ".md",
                    ReportFormat.HTML => ".html",
                    ReportFormat.CSV => ".csv",
                    ReportFormat.JSON => ".json",
                    _ => ".txt"
                };
                
                string fullPath = System.IO.Path.Combine(_exportPath, $"{filename}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}");
                System.IO.File.WriteAllText(fullPath, _reportBuffer.ToString());
                
                UnityEngine.Debug.Log($"[Content Statistics] Report saved to: {fullPath}");
                EditorUtility.DisplayDialog("Report Saved", $"Report saved to:\n{fullPath}", "OK");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Content Statistics] Failed to save report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", "Failed to save report. Check console for details.", "OK");
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private void DrawStatCard(string label, string value, Color color)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(150));
            
            var oldColor = GUI.color;
            GUI.color = color;
            GUILayout.Label(value, EditorStyles.boldLabel);
            GUI.color = oldColor;
            
            GUILayout.Label(label, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }
        
        private void DrawMetricBar(string label, float value, float maxValue)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(150));
            GUILayout.Label(label, EditorStyles.miniLabel);
            
            float normalized = Mathf.Clamp01(value / maxValue);
            DrawProgressBar(normalized, $"{value:F1}");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawProgressBar(float progress, string label)
        {
            Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
            EditorGUI.ProgressBar(rect, progress, label);
        }
        
        private void DrawDistributionChart(Dictionary<string, int> distribution)
        {
            int total = distribution.Values.Sum();
            
            foreach (var kvp in distribution.Take(8))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(kvp.Key, GUILayout.Width(120));
                
                float percentage = (float)kvp.Value / total;
                DrawProgressBar(percentage, $"{kvp.Value}");
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private string CalculateQualityScore()
        {
            float score = 100f;
            
            // Deduct points for issues
            score -= _contentStats.UnusedTraits.Count * 2f;
            score -= _contentStats.OrphanedEntities.Count * 1f;
            
            if (_contentStats.AverageTraitsPerEntity < 3) score -= 10f;
            if (_contentStats.AverageRelationshipsPerEntity < 1) score -= 15f;
            
            score = Mathf.Clamp(score, 0f, 100f);
            
            return score switch
            {
                >= 90f => "Excellent (A+)",
                >= 80f => "Good (B+)",
                >= 70f => "Average (C+)",
                >= 60f => "Below Average (D+)",
                _ => "Needs Improvement (F)"
            };
        }
        
        #endregion
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Progression Analysis Window for analyzing character and game progression systems,
    /// including skill development, trait evolution, and relationship progression over time.
    /// </summary>
    public class ProgressionAnalyzerWindow : EditorWindow
    {
        #region Window Management
        
        [MenuItem("Lineage/Studio/Analysis/Progression Analyzer")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProgressionAnalyzerWindow>("Progression Analyzer");
            window.minSize = new Vector2(1000, 650);
            window.Show();
        }
        
        #endregion
        
        #region UI State
        
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Character Evolution", "Skill Progression", "Trait Development", "Relationships", "Timeline", "Predictions" };
        private Vector2 _scrollPosition = Vector2.zero;
        
        // Analysis Settings
        private string _selectedCharacterId = "";
        private DateTime _analysisStartDate = DateTime.Now.AddYears(-1);
        private DateTime _analysisEndDate = DateTime.Now;
        private int _timeGranularity = 30; // days
        
        // Progression Data
        private ProgressionAnalysis _analysis;
        private bool _analysisCalculated = false;
        private DateTime _lastAnalyzed;
        
        // Visualization
        private bool _showTrendLines = true;
        private bool _showPredictions = true;
        private ProgressionMetric _selectedMetric = ProgressionMetric.SkillPoints;
        
        #endregion
        
        #region Data Structures
        
        private enum ProgressionMetric
        {
            SkillPoints,
            TraitIntensity,
            RelationshipStrength,
            OverallDevelopment,
            SocialConnections,
            Achievements
        }
        
        private class ProgressionAnalysis
        {
            public string CharacterId { get; set; }
            public string CharacterName { get; set; }
            public TimeSpan AnalysisPeriod { get; set; }
            
            public List<ProgressionSnapshot> Snapshots { get; set; } = new List<ProgressionSnapshot>();
            public List<SkillProgression> SkillProgressions { get; set; } = new List<SkillProgression>();
            public List<TraitEvolution> TraitEvolutions { get; set; } = new List<TraitEvolution>();
            public List<RelationshipProgression> RelationshipProgressions { get; set; } = new List<RelationshipProgression>();
            
            public ProgressionTrends Trends { get; set; } = new ProgressionTrends();
            public List<ProgressionMilestone> Milestones { get; set; } = new List<ProgressionMilestone>();
            public ProgressionPredictions Predictions { get; set; } = new ProgressionPredictions();
            
            public float OverallDevelopmentScore { get; set; }
            public string DevelopmentStage { get; set; }
            public List<string> ProgressionInsights { get; set; } = new List<string>();
        }
        
        private class ProgressionSnapshot
        {
            public DateTime Timestamp { get; set; }
            public Dictionary<string, float> SkillLevels { get; set; } = new Dictionary<string, float>();
            public Dictionary<string, float> TraitIntensities { get; set; } = new Dictionary<string, float>();
            public int RelationshipCount { get; set; }
            public float AverageRelationshipStrength { get; set; }
            public int AchievementCount { get; set; }
            public float OverallScore { get; set; }
        }
        
        private class SkillProgression
        {
            public string SkillName { get; set; }
            public List<ProgressionDataPoint> DataPoints { get; set; } = new List<ProgressionDataPoint>();
            public float GrowthRate { get; set; }
            public float CurrentLevel { get; set; }
            public float PredictedLevel { get; set; }
            public ProgressionPattern Pattern { get; set; }
        }
        
        private class TraitEvolution
        {
            public string TraitName { get; set; }
            public List<ProgressionDataPoint> IntensityHistory { get; set; } = new List<ProgressionDataPoint>();
            public float IntensityChange { get; set; }
            public EvolutionDirection Direction { get; set; }
            public string EvolutionDescription { get; set; }
        }
        
        private class RelationshipProgression
        {
            public string RelatedCharacterId { get; set; }
            public string RelatedCharacterName { get; set; }
            public string RelationshipType { get; set; }
            public List<ProgressionDataPoint> StrengthHistory { get; set; } = new List<ProgressionDataPoint>();
            public float StrengthChange { get; set; }
            public RelationshipTrend Trend { get; set; }
        }
        
        private class ProgressionDataPoint
        {
            public DateTime Date { get; set; }
            public float Value { get; set; }
            public string Note { get; set; }
        }
        
        private class ProgressionTrends
        {
            public TrendDirection OverallTrend { get; set; }
            public float GrowthRate { get; set; }
            public float Acceleration { get; set; }
            public Dictionary<string, TrendDirection> SkillTrends { get; set; } = new Dictionary<string, TrendDirection>();
            public Dictionary<string, TrendDirection> TraitTrends { get; set; } = new Dictionary<string, TrendDirection>();
            public TrendDirection SocialTrend { get; set; }
        }
        
        private class ProgressionMilestone
        {
            public DateTime Date { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public MilestoneType Type { get; set; }
            public float Impact { get; set; }
        }
        
        private class ProgressionPredictions
        {
            public DateTime PredictionDate { get; set; }
            public Dictionary<string, float> PredictedSkillLevels { get; set; } = new Dictionary<string, float>();
            public Dictionary<string, float> PredictedTraitIntensities { get; set; } = new Dictionary<string, float>();
            public float ConfidenceLevel { get; set; }
            public List<string> PredictedEvents { get; set; } = new List<string>();
        }
        
        private enum ProgressionPattern
        {
            Linear,
            Exponential,
            Logarithmic,
            Plateau,
            Declining,
            Irregular
        }
        
        private enum EvolutionDirection
        {
            Strengthening,
            Weakening,
            Stable,
            Fluctuating
        }
        
        private enum RelationshipTrend
        {
            Improving,
            Deteriorating,
            Stable,
            Volatile
        }
        
        private enum TrendDirection
        {
            Ascending,
            Descending,
            Stable,
            Volatile
        }
        
        private enum MilestoneType
        {
            SkillMastery,
            TraitEvolution,
            RelationshipChange,
            Achievement,
            LifeEvent
        }
        
        #endregion
        
        #region Unity Events
        
        private void OnGUI()
        {
            DrawHeader();
            DrawTabs();
            
            GUILayout.Space(10);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            switch (_selectedTab)
            {
                case 0: DrawCharacterEvolutionTab(); break;
                case 1: DrawSkillProgressionTab(); break;
                case 2: DrawTraitDevelopmentTab(); break;
                case 3: DrawRelationshipsTab(); break;
                case 4: DrawTimelineTab(); break;
                case 5: DrawPredictionsTab(); break;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        #endregion
        
        #region UI Drawing
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Progression Analyzer", EditorStyles.boldLabel);
            GUILayout.Label("Analyze character development, skill progression, and relationship evolution over time", EditorStyles.helpBox);
            
            // Analysis Settings
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Character:", GUILayout.Width(70));
            _selectedCharacterId = EditorGUILayout.TextField(_selectedCharacterId, GUILayout.Width(200));
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                ShowCharacterSelector();
            }
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Analyze Progression", GUILayout.Width(150)))
            {
                AnalyzeProgression();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Time Range
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Period:", GUILayout.Width(70));
            
            GUILayout.Label("From:", GUILayout.Width(40));
            _analysisStartDate = DrawDateField(_analysisStartDate);
            
            GUILayout.Label("To:", GUILayout.Width(25));
            _analysisEndDate = DrawDateField(_analysisEndDate);
            
            GUILayout.Label("Granularity:", GUILayout.Width(80));
            _timeGranularity = EditorGUILayout.IntSlider(_timeGranularity, 1, 365, GUILayout.Width(150));
            GUILayout.Label("days", GUILayout.Width(30));
            
            EditorGUILayout.EndHorizontal();
            
            if (_analysisCalculated)
            {
                EditorGUILayout.LabelField($"Last Analysis: {_lastAnalyzed:yyyy-MM-dd HH:mm} | Character: {_analysis?.CharacterName ?? "Unknown"}");
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTabs()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
        }
        
        private void DrawCharacterEvolutionTab()
        {
            if (!_analysisCalculated)
            {
                EditorGUILayout.HelpBox("Select a character and click 'Analyze Progression' to view evolution data.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Character Evolution Overview", EditorStyles.boldLabel);
            
            // Overall Development Score
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Development Summary", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Overall Development Score: {_analysis.OverallDevelopmentScore:F2}/10", EditorStyles.boldLabel);
            DrawProgressBar(_analysis.OverallDevelopmentScore / 10f, "");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField($"Development Stage: {_analysis.DevelopmentStage}");
            EditorGUILayout.LabelField($"Analysis Period: {_analysis.AnalysisPeriod.Days} days");
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Development Trends
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Development Trends", EditorStyles.boldLabel);
            
            DrawTrendIndicator("Overall Growth", _analysis.Trends.OverallTrend, _analysis.Trends.GrowthRate);
            DrawTrendIndicator("Social Development", _analysis.Trends.SocialTrend, 0f);
            
            if (_analysis.Trends.SkillTrends.Any())
            {
                GUILayout.Label("Skill Trends:", EditorStyles.miniLabel);
                foreach (var skillTrend in _analysis.Trends.SkillTrends.Take(5))
                {
                    DrawTrendIndicator($"  {skillTrend.Key}", skillTrend.Value, 0f);
                }
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Key Insights
            if (_analysis.ProgressionInsights.Any())
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Key Insights", EditorStyles.boldLabel);
                
                foreach (var insight in _analysis.ProgressionInsights)
                {
                    EditorGUILayout.LabelField($"• {insight}", EditorStyles.wordWrappedLabel);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            // Evolution Chart
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Evolution Timeline", EditorStyles.boldLabel);
            
            if (_analysis.Snapshots.Any())
            {
                DrawEvolutionChart();
            }
            else
            {
                EditorGUILayout.HelpBox("No historical data available for timeline visualization.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSkillProgressionTab()
        {
            if (!_analysisCalculated)
            {
                EditorGUILayout.HelpBox("Analyze progression first to view skill development data.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Skill Progression Analysis", EditorStyles.boldLabel);
            
            if (!_analysis.SkillProgressions.Any())
            {
                EditorGUILayout.HelpBox("No skill progression data available for this character.", MessageType.Info);
                return;
            }
            
            // Skills Overview
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Skills Overview", EditorStyles.boldLabel);
            
            foreach (var skill in _analysis.SkillProgressions.OrderByDescending(s => s.CurrentLevel))
            {
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(skill.SkillName, EditorStyles.boldLabel, GUILayout.Width(150));
                
                GUILayout.Label($"Level: {skill.CurrentLevel:F1}", GUILayout.Width(80));
                GUILayout.Label($"Growth: {GetGrowthRateText(skill.GrowthRate)}", GUILayout.Width(100));
                GUILayout.Label($"Pattern: {skill.Pattern}", GUILayout.Width(100));
                
                EditorGUILayout.EndHorizontal();
                
                // Progress bar
                float normalizedLevel = skill.CurrentLevel / 100f; // Assuming max level 100
                DrawProgressBar(normalizedLevel, $"{skill.CurrentLevel:F1}/100");
                
                // Growth indicator
                if (skill.GrowthRate != 0)
                {
                    string growthIcon = skill.GrowthRate > 0 ? "↗" : "↘";
                    Color growthColor = skill.GrowthRate > 0 ? Color.green : Color.red;
                    
                    var oldColor = GUI.color;
                    GUI.color = growthColor;
                    GUILayout.Label($"{growthIcon} {Mathf.Abs(skill.GrowthRate):F2}/month", EditorStyles.miniLabel);
                    GUI.color = oldColor;
                }
                
                // Mini chart
                if (skill.DataPoints.Count > 1)
                {
                    DrawMiniChart(skill.DataPoints, 100f, 40f);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Skill Development Recommendations
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Development Recommendations", EditorStyles.boldLabel);
            
            var stagnantSkills = _analysis.SkillProgressions.Where(s => s.GrowthRate < 0.1f).ToList();
            if (stagnantSkills.Any())
            {
                GUILayout.Label("Skills needing attention:", EditorStyles.miniLabel);
                foreach (var skill in stagnantSkills.Take(5))
                {
                    EditorGUILayout.LabelField($"• {skill.SkillName} - Consider focused training", EditorStyles.miniLabel);
                }
            }
            
            var rapidGrowthSkills = _analysis.SkillProgressions.Where(s => s.GrowthRate > 2f).ToList();
            if (rapidGrowthSkills.Any())
            {
                GUILayout.Label("Rapidly developing skills:", EditorStyles.miniLabel);
                foreach (var skill in rapidGrowthSkills.Take(3))
                {
                    EditorGUILayout.LabelField($"• {skill.SkillName} - Excellent progress!", EditorStyles.miniLabel);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTraitDevelopmentTab()
        {
            if (!_analysisCalculated)
            {
                EditorGUILayout.HelpBox("Analyze progression first to view trait development data.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Trait Development Analysis", EditorStyles.boldLabel);
            
            if (!_analysis.TraitEvolutions.Any())
            {
                EditorGUILayout.HelpBox("No trait evolution data available for this character.", MessageType.Info);
                return;
            }
            
            foreach (var trait in _analysis.TraitEvolutions.OrderBy(t => t.TraitName))
            {
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(trait.TraitName, EditorStyles.boldLabel, GUILayout.Width(150));
                
                // Direction indicator
                string directionIcon = trait.Direction switch
                {
                    EvolutionDirection.Strengthening => "↗ Strengthening",
                    EvolutionDirection.Weakening => "↘ Weakening",
                    EvolutionDirection.Stable => "→ Stable",
                    EvolutionDirection.Fluctuating => "↕ Fluctuating",
                    _ => "? Unknown"
                };
                
                Color directionColor = trait.Direction switch
                {
                    EvolutionDirection.Strengthening => Color.green,
                    EvolutionDirection.Weakening => Color.red,
                    EvolutionDirection.Stable => Color.yellow,
                    EvolutionDirection.Fluctuating => Color.cyan,
                    _ => Color.white
                };
                
                var oldColor = GUI.color;
                GUI.color = directionColor;
                GUILayout.Label(directionIcon, GUILayout.Width(120));
                GUI.color = oldColor;
                
                GUILayout.Label($"Change: {trait.IntensityChange:+0.0;-0.0;0.0}", GUILayout.Width(80));
                
                EditorGUILayout.EndHorizontal();
                
                if (!string.IsNullOrEmpty(trait.EvolutionDescription))
                {
                    EditorGUILayout.LabelField(trait.EvolutionDescription, EditorStyles.wordWrappedMiniLabel);
                }
                
                // Mini evolution chart
                if (trait.IntensityHistory.Count > 1)
                {
                    DrawMiniChart(trait.IntensityHistory, 10f, 30f);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            // Trait Analysis Summary
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Trait Development Summary", EditorStyles.boldLabel);
            
            var strengtheningTraits = _analysis.TraitEvolutions.Count(t => t.Direction == EvolutionDirection.Strengthening);
            var weakeningTraits = _analysis.TraitEvolutions.Count(t => t.Direction == EvolutionDirection.Weakening);
            var stableTraits = _analysis.TraitEvolutions.Count(t => t.Direction == EvolutionDirection.Stable);
            
            EditorGUILayout.LabelField($"Strengthening Traits: {strengtheningTraits}");
            EditorGUILayout.LabelField($"Weakening Traits: {weakeningTraits}");
            EditorGUILayout.LabelField($"Stable Traits: {stableTraits}");
            
            float stabilityRatio = stableTraits / (float)_analysis.TraitEvolutions.Count;
            EditorGUILayout.LabelField($"Personality Stability: {stabilityRatio:P1}");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawRelationshipsTab()
        {
            if (!_analysisCalculated)
            {
                EditorGUILayout.HelpBox("Analyze progression first to view relationship development data.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Relationship Progression Analysis", EditorStyles.boldLabel);
            
            if (!_analysis.RelationshipProgressions.Any())
            {
                EditorGUILayout.HelpBox("No relationship progression data available for this character.", MessageType.Info);
                return;
            }
            
            // Relationship summary
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Relationship Overview", EditorStyles.boldLabel);
            
            var improvingRelationships = _analysis.RelationshipProgressions.Count(r => r.Trend == RelationshipTrend.Improving);
            var deterioratingRelationships = _analysis.RelationshipProgressions.Count(r => r.Trend == RelationshipTrend.Deteriorating);
            var stableRelationships = _analysis.RelationshipProgressions.Count(r => r.Trend == RelationshipTrend.Stable);
            
            EditorGUILayout.LabelField($"Total Relationships: {_analysis.RelationshipProgressions.Count}");
            EditorGUILayout.LabelField($"Improving: {improvingRelationships}");
            EditorGUILayout.LabelField($"Deteriorating: {deterioratingRelationships}");
            EditorGUILayout.LabelField($"Stable: {stableRelationships}");
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Individual relationships
            foreach (var relationship in _analysis.RelationshipProgressions.OrderByDescending(r => Math.Abs(r.StrengthChange)))
            {
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(relationship.RelatedCharacterName, EditorStyles.boldLabel, GUILayout.Width(150));
                GUILayout.Label(relationship.RelationshipType, GUILayout.Width(100));
                
                // Trend indicator
                string trendIcon = relationship.Trend switch
                {
                    RelationshipTrend.Improving => "↗",
                    RelationshipTrend.Deteriorating => "↘",
                    RelationshipTrend.Stable => "→",
                    RelationshipTrend.Volatile => "↕",
                    _ => "?"
                };
                
                Color trendColor = relationship.Trend switch
                {
                    RelationshipTrend.Improving => Color.green,
                    RelationshipTrend.Deteriorating => Color.red,
                    RelationshipTrend.Stable => Color.yellow,
                    RelationshipTrend.Volatile => Color.cyan,
                    _ => Color.white
                };
                
                var oldColor = GUI.color;
                GUI.color = trendColor;
                GUILayout.Label($"{trendIcon} {relationship.Trend}", GUILayout.Width(100));
                GUI.color = oldColor;
                
                GUILayout.Label($"Change: {relationship.StrengthChange:+0.0;-0.0;0.0}", GUILayout.Width(100));
                
                EditorGUILayout.EndHorizontal();
                
                // Relationship strength history
                if (relationship.StrengthHistory.Count > 1)
                {
                    DrawMiniChart(relationship.StrengthHistory, 10f, 25f);
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawTimelineTab()
        {
            if (!_analysisCalculated)
            {
                EditorGUILayout.HelpBox("Analyze progression first to view timeline data.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Progression Timeline", EditorStyles.boldLabel);
            
            // Timeline controls
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label("Metric:", GUILayout.Width(60));
            _selectedMetric = (ProgressionMetric)EditorGUILayout.EnumPopup(_selectedMetric, GUILayout.Width(150));
            
            _showTrendLines = EditorGUILayout.Toggle("Trend Lines", _showTrendLines);
            
            if (GUILayout.Button("Export Timeline Data"))
            {
                ExportTimelineData();
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Milestones
            if (_analysis.Milestones.Any())
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Key Milestones", EditorStyles.boldLabel);
                
                foreach (var milestone in _analysis.Milestones.OrderBy(m => m.Date))
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    GUILayout.Label(milestone.Date.ToString("yyyy-MM-dd"), GUILayout.Width(80));
                    
                    string typeIcon = milestone.Type switch
                    {
                        MilestoneType.SkillMastery => "🎯",
                        MilestoneType.TraitEvolution => "🔄",
                        MilestoneType.RelationshipChange => "💙",
                        MilestoneType.Achievement => "🏆",
                        MilestoneType.LifeEvent => "📅",
                        _ => "•"
                    };
                    
                    GUILayout.Label(typeIcon, GUILayout.Width(20));
                    GUILayout.Label(milestone.Title, EditorStyles.boldLabel, GUILayout.Width(150));
                    GUILayout.Label(milestone.Description, EditorStyles.wordWrappedLabel);
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            // Main timeline visualization
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label($"Timeline - {_selectedMetric}", EditorStyles.boldLabel);
            
            if (_analysis.Snapshots.Any())
            {
                DrawTimelineChart();
            }
            else
            {
                EditorGUILayout.HelpBox("No snapshot data available for timeline visualization.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPredictionsTab()
        {
            if (!_analysisCalculated)
            {
                EditorGUILayout.HelpBox("Analyze progression first to generate predictions.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Progression Predictions", EditorStyles.boldLabel);
            
            // Prediction settings
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Prediction Settings", EditorStyles.boldLabel);
            
            _showPredictions = EditorGUILayout.Toggle("Show Predictions", _showPredictions);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate New Predictions"))
            {
                GeneratePredictions();
            }
            
            if (GUILayout.Button("Export Predictions"))
            {
                ExportPredictions();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            if (_analysis.Predictions == null)
            {
                EditorGUILayout.HelpBox("No predictions available. Click 'Generate New Predictions' to create forecasts.", MessageType.Info);
                return;
            }
            
            // Prediction confidence
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Prediction Confidence", EditorStyles.boldLabel);
            
            DrawProgressBar(_analysis.Predictions.ConfidenceLevel, $"{_analysis.Predictions.ConfidenceLevel:P1} Confidence");
            
            EditorGUILayout.LabelField($"Prediction Date: {_analysis.Predictions.PredictionDate:yyyy-MM-dd}");
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Skill predictions
            if (_analysis.Predictions.PredictedSkillLevels.Any())
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Predicted Skill Levels", EditorStyles.boldLabel);
                
                foreach (var prediction in _analysis.Predictions.PredictedSkillLevels.OrderByDescending(p => p.Value))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(prediction.Key, GUILayout.Width(150));
                    
                    var currentSkill = _analysis.SkillProgressions.FirstOrDefault(s => s.SkillName == prediction.Key);
                    float currentLevel = currentSkill?.CurrentLevel ?? 0f;
                    float change = prediction.Value - currentLevel;
                    
                    GUILayout.Label($"Current: {currentLevel:F1}", GUILayout.Width(80));
                    GUILayout.Label($"Predicted: {prediction.Value:F1}", GUILayout.Width(100));
                    
                    string changeText = $"{change:+0.0;-0.0;0.0}";
                    Color changeColor = change > 0 ? Color.green : change < 0 ? Color.red : Color.yellow;
                    
                    var oldColor = GUI.color;
                    GUI.color = changeColor;
                    GUILayout.Label(changeText, GUILayout.Width(60));
                    GUI.color = oldColor;
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            // Predicted events
            if (_analysis.Predictions.PredictedEvents.Any())
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Predicted Events", EditorStyles.boldLabel);
                
                foreach (var eventPrediction in _analysis.Predictions.PredictedEvents)
                {
                    EditorGUILayout.LabelField($"• {eventPrediction}", EditorStyles.wordWrappedLabel);
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        #endregion
        
        #region Analysis Methods
        
        private void AnalyzeProgression()
        {
            if (string.IsNullOrEmpty(_selectedCharacterId))
            {
                EditorUtility.DisplayDialog("Error", "Please select a character to analyze.", "OK");
                return;
            }
            
            if (GameData.Instance == null)
            {
                EditorUtility.DisplayDialog("Error", "GameData instance not found. Initialize the database first.", "OK");
                return;
            }
            
            try
            {
                _analysis = new ProgressionAnalysis
                {
                    CharacterId = _selectedCharacterId,
                    CharacterName = GetCharacterName(_selectedCharacterId),
                    AnalysisPeriod = _analysisEndDate - _analysisStartDate
                };
                
                GenerateProgressionSnapshots();
                AnalyzeSkillProgression();
                AnalyzeTraitEvolution();
                AnalyzeRelationshipProgression();
                CalculateProgressionTrends();
                IdentifyMilestones();
                GenerateInsights();
                
                _analysisCalculated = true;
                _lastAnalyzed = DateTime.Now;
                
                UnityEngine.Debug.Log($"[Progression Analyzer] Analysis completed for character: {_analysis.CharacterName}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Progression Analyzer] Error during analysis: {ex.Message}");
                EditorUtility.DisplayDialog("Error", "Failed to analyze progression. Check console for details.", "OK");
            }
        }
        
        private void GenerateProgressionSnapshots()
        {
            // Generate mock snapshots - in real implementation, this would query historical data
            var startDate = _analysisStartDate;
            var endDate = _analysisEndDate;
            var interval = TimeSpan.FromDays(_timeGranularity);
            
            for (var date = startDate; date <= endDate; date += interval)
            {
                var snapshot = new ProgressionSnapshot
                {
                    Timestamp = date,
                    RelationshipCount = UnityEngine.Random.Range(5, 15),
                    AverageRelationshipStrength = UnityEngine.Random.Range(3f, 8f),
                    AchievementCount = UnityEngine.Random.Range(0, 10),
                    OverallScore = UnityEngine.Random.Range(3f, 9f)
                };
                
                // Add some skills
                var skillNames = new[] { "Combat", "Diplomacy", "Crafting", "Leadership", "Magic" };
                foreach (var skillName in skillNames)
                {
                    snapshot.SkillLevels[skillName] = UnityEngine.Random.Range(10f, 90f);
                }
                
                // Add some traits
                var traitNames = new[] { "Courage", "Intelligence", "Charisma", "Wisdom", "Strength" };
                foreach (var traitName in traitNames)
                {
                    snapshot.TraitIntensities[traitName] = UnityEngine.Random.Range(1f, 10f);
                }
                
                _analysis.Snapshots.Add(snapshot);
            }
        }
        
        private void AnalyzeSkillProgression()
        {
            var skillNames = new[] { "Combat", "Diplomacy", "Crafting", "Leadership", "Magic" };
            
            foreach (var skillName in skillNames)
            {
                var progression = new SkillProgression
                {
                    SkillName = skillName,
                    CurrentLevel = UnityEngine.Random.Range(20f, 80f),
                    GrowthRate = UnityEngine.Random.Range(-0.5f, 3f),
                    Pattern = (ProgressionPattern)UnityEngine.Random.Range(0, 6)
                };
                
                // Generate data points from snapshots
                foreach (var snapshot in _analysis.Snapshots)
                {
                    if (snapshot.SkillLevels.TryGetValue(skillName, out float level))
                    {
                        progression.DataPoints.Add(new ProgressionDataPoint
                        {
                            Date = snapshot.Timestamp,
                            Value = level
                        });
                    }
                }
                
                progression.PredictedLevel = progression.CurrentLevel + progression.GrowthRate * 3f; // 3 months ahead
                
                _analysis.SkillProgressions.Add(progression);
            }
        }
        
        private void AnalyzeTraitEvolution()
        {
            var traitNames = new[] { "Courage", "Intelligence", "Charisma", "Wisdom", "Strength" };
            
            foreach (var traitName in traitNames)
            {
                var evolution = new TraitEvolution
                {
                    TraitName = traitName,
                    IntensityChange = UnityEngine.Random.Range(-2f, 2f),
                    Direction = (EvolutionDirection)UnityEngine.Random.Range(0, 4)
                };
                
                // Generate intensity history
                foreach (var snapshot in _analysis.Snapshots)
                {
                    if (snapshot.TraitIntensities.TryGetValue(traitName, out float intensity))
                    {
                        evolution.IntensityHistory.Add(new ProgressionDataPoint
                        {
                            Date = snapshot.Timestamp,
                            Value = intensity
                        });
                    }
                }
                
                evolution.EvolutionDescription = GenerateTraitEvolutionDescription(evolution);
                
                _analysis.TraitEvolutions.Add(evolution);
            }
        }
        
        private void AnalyzeRelationshipProgression()
        {
            var relationshipTypes = new[] { "Friend", "Family", "Rival", "Mentor", "Student" };
            var characterNames = new[] { "Alice", "Bob", "Charlie", "Diana", "Edward" };
            
            for (int i = 0; i < 5; i++)
            {
                var progression = new RelationshipProgression
                {
                    RelatedCharacterId = $"char_{i}",
                    RelatedCharacterName = characterNames[i],
                    RelationshipType = relationshipTypes[i],
                    StrengthChange = UnityEngine.Random.Range(-3f, 3f),
                    Trend = (RelationshipTrend)UnityEngine.Random.Range(0, 4)
                };
                
                // Generate strength history
                foreach (var snapshot in _analysis.Snapshots)
                {
                    progression.StrengthHistory.Add(new ProgressionDataPoint
                    {
                        Date = snapshot.Timestamp,
                        Value = UnityEngine.Random.Range(1f, 10f)
                    });
                }
                
                _analysis.RelationshipProgressions.Add(progression);
            }
        }
        
        private void CalculateProgressionTrends()
        {
            _analysis.Trends = new ProgressionTrends
            {
                OverallTrend = (TrendDirection)UnityEngine.Random.Range(0, 4),
                GrowthRate = UnityEngine.Random.Range(-1f, 3f),
                Acceleration = UnityEngine.Random.Range(-0.5f, 0.5f),
                SocialTrend = (TrendDirection)UnityEngine.Random.Range(0, 4)
            };
            
            foreach (var skill in _analysis.SkillProgressions)
            {
                _analysis.Trends.SkillTrends[skill.SkillName] = skill.GrowthRate > 0 ? TrendDirection.Ascending : 
                                                                skill.GrowthRate < 0 ? TrendDirection.Descending : TrendDirection.Stable;
            }
            
            foreach (var trait in _analysis.TraitEvolutions)
            {
                _analysis.Trends.TraitTrends[trait.TraitName] = trait.Direction switch
                {
                    EvolutionDirection.Strengthening => TrendDirection.Ascending,
                    EvolutionDirection.Weakening => TrendDirection.Descending,
                    EvolutionDirection.Stable => TrendDirection.Stable,
                    _ => TrendDirection.Volatile
                };
            }
        }
        
        private void IdentifyMilestones()
        {
            // Generate some mock milestones
            var milestoneEvents = new[]
            {
                ("Mastered Combat Skills", MilestoneType.SkillMastery),
                ("Personality Shift Detected", MilestoneType.TraitEvolution),
                ("New Friendship Formed", MilestoneType.RelationshipChange),
                ("Achievement Unlocked", MilestoneType.Achievement),
                ("Major Life Event", MilestoneType.LifeEvent)
            };
            
            foreach (var (title, type) in milestoneEvents)
            {
                if (UnityEngine.Random.value > 0.3f) // 70% chance for each milestone
                {
                    _analysis.Milestones.Add(new ProgressionMilestone
                    {
                        Date = _analysisStartDate.AddDays(UnityEngine.Random.Range(0, (int)_analysis.AnalysisPeriod.TotalDays)),
                        Title = title,
                        Description = $"Detailed description of {title.ToLower()}",
                        Type = type,
                        Impact = UnityEngine.Random.Range(0.1f, 1.0f)
                    });
                }
            }
        }
        
        private void GenerateInsights()
        {
            _analysis.ProgressionInsights.Clear();
            
            // Overall development assessment
            _analysis.OverallDevelopmentScore = UnityEngine.Random.Range(4f, 9f);
            _analysis.DevelopmentStage = _analysis.OverallDevelopmentScore switch
            {
                >= 8f => "Advanced Development",
                >= 6f => "Steady Growth",
                >= 4f => "Early Development",
                _ => "Needs Attention"
            };
            
            // Generate insights based on analysis
            if (_analysis.Trends.OverallTrend == TrendDirection.Ascending)
            {
                _analysis.ProgressionInsights.Add("Character shows consistent positive development across multiple areas.");
            }
            
            var rapidSkills = _analysis.SkillProgressions.Where(s => s.GrowthRate > 2f).ToList();
            if (rapidSkills.Any())
            {
                _analysis.ProgressionInsights.Add($"Exceptional growth in {string.Join(", ", rapidSkills.Select(s => s.SkillName))}.");
            }
            
            var stagnantSkills = _analysis.SkillProgressions.Where(s => s.GrowthRate < 0.1f).ToList();
            if (stagnantSkills.Any())
            {
                _analysis.ProgressionInsights.Add($"Consider focusing on {string.Join(", ", stagnantSkills.Select(s => s.SkillName))} development.");
            }
            
            var improvingRelationships = _analysis.RelationshipProgressions.Count(r => r.Trend == RelationshipTrend.Improving);
            if (improvingRelationships > _analysis.RelationshipProgressions.Count * 0.6f)
            {
                _analysis.ProgressionInsights.Add("Strong social development with improving relationships across the board.");
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private DateTime DrawDateField(DateTime date)
        {
            string dateString = date.ToString("yyyy-MM-dd");
            string newDateString = EditorGUILayout.TextField(dateString, GUILayout.Width(100));
            
            if (DateTime.TryParse(newDateString, out DateTime newDate))
            {
                return newDate;
            }
            return date;
        }
        
        private void DrawProgressBar(float progress, string label)
        {
            Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
            EditorGUI.ProgressBar(rect, progress, label);
        }
        
        private void DrawTrendIndicator(string label, TrendDirection trend, float value)
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label(label, GUILayout.Width(150));
            
            string trendIcon = trend switch
            {
                TrendDirection.Ascending => "↗",
                TrendDirection.Descending => "↘",
                TrendDirection.Stable => "→",
                TrendDirection.Volatile => "↕",
                _ => "?"
            };
            
            Color trendColor = trend switch
            {
                TrendDirection.Ascending => Color.green,
                TrendDirection.Descending => Color.red,
                TrendDirection.Stable => Color.yellow,
                TrendDirection.Volatile => Color.cyan,
                _ => Color.white
            };
            
            var oldColor = GUI.color;
            GUI.color = trendColor;
            GUILayout.Label($"{trendIcon} {trend}", GUILayout.Width(100));
            GUI.color = oldColor;
            
            if (value != 0)
            {
                GUILayout.Label($"({value:+0.0;-0.0;0.0}/month)", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawMiniChart(List<ProgressionDataPoint> dataPoints, float maxValue, float height)
        {
            if (dataPoints.Count < 2) return;
            
            Rect chartRect = GUILayoutUtility.GetRect(200, height);
            EditorGUI.DrawRect(chartRect, new Color(0.1f, 0.1f, 0.1f, 0.5f));
            
            // Find min/max values for scaling
            float minValue = dataPoints.Min(p => p.Value);
            float maxActualValue = dataPoints.Max(p => p.Value);
            float range = maxActualValue - minValue;
            
            if (range < 0.01f) range = 1f; // Avoid division by zero
            
            Vector2 prevPoint = Vector2.zero;
            
            for (int i = 0; i < dataPoints.Count; i++)
            {
                float x = chartRect.x + (i / (float)(dataPoints.Count - 1)) * chartRect.width;
                float normalizedValue = (dataPoints[i].Value - minValue) / range;
                float y = chartRect.yMax - normalizedValue * chartRect.height;
                
                Vector2 currentPoint = new Vector2(x, y);
                
                if (i > 0)
                {
                    DrawSimpleLine(prevPoint, currentPoint, Color.cyan, 1f);
                }
                
                prevPoint = currentPoint;
            }
        }
        
        private void DrawEvolutionChart()
        {
            // Simplified evolution chart visualization
            Rect chartRect = GUILayoutUtility.GetRect(400, 150);
            EditorGUI.DrawRect(chartRect, new Color(0.1f, 0.1f, 0.1f, 0.3f));
            
            GUILayout.BeginArea(chartRect);
            GUILayout.Label("Evolution Chart", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label($"Data points: {_analysis.Snapshots.Count}", EditorStyles.centeredGreyMiniLabel);
            GUILayout.EndArea();
        }
        
        private void DrawTimelineChart()
        {
            // Simplified timeline chart visualization
            Rect chartRect = GUILayoutUtility.GetRect(500, 200);
            EditorGUI.DrawRect(chartRect, new Color(0.1f, 0.1f, 0.1f, 0.3f));
            
            GUILayout.BeginArea(chartRect);
            GUILayout.Label($"Timeline: {_selectedMetric}", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label($"Period: {_analysisStartDate:yyyy-MM-dd} to {_analysisEndDate:yyyy-MM-dd}", EditorStyles.centeredGreyMiniLabel);
            GUILayout.EndArea();
        }
        
        private void DrawSimpleLine(Vector2 start, Vector2 end, Color color, float width)
        {
            // Simple line drawing implementation
            Vector2 direction = (end - start).normalized;
            float distance = Vector2.Distance(start, end);
            
            var oldColor = GUI.color;
            GUI.color = color;
            
            Rect lineRect = new Rect(start.x, start.y - width * 0.5f, distance, width);
            
            // Rotate the rect to match the line direction (simplified)
            EditorGUI.DrawRect(lineRect, color);
            
            GUI.color = oldColor;
        }
        
        private string GetGrowthRateText(float growthRate)
        {
            return growthRate switch
            {
                > 2f => "Rapid",
                > 1f => "Fast",
                > 0.5f => "Moderate",
                > 0f => "Slow",
                0f => "Stable",
                _ => "Declining"
            };
        }
        
        private string GetCharacterName(string characterId)
        {
            // In real implementation, this would query the database
            return $"Character_{characterId}";
        }
        
        private string GenerateTraitEvolutionDescription(TraitEvolution evolution)
        {
            return evolution.Direction switch
            {
                EvolutionDirection.Strengthening => $"{evolution.TraitName} is becoming more pronounced over time.",
                EvolutionDirection.Weakening => $"{evolution.TraitName} is diminishing in intensity.",
                EvolutionDirection.Stable => $"{evolution.TraitName} remains consistent.",
                EvolutionDirection.Fluctuating => $"{evolution.TraitName} shows irregular patterns.",
                _ => "Evolution pattern unclear."
            };
        }
        
        #endregion
        
        #region Export and Prediction Methods
        
        private void ShowCharacterSelector()
        {
            // In real implementation, this would show a character selection window
            _selectedCharacterId = "sample_character_id";
        }
        
        private void GeneratePredictions()
        {
            if (_analysis == null) return;
            
            _analysis.Predictions = new ProgressionPredictions
            {
                PredictionDate = DateTime.Now.AddMonths(3),
                ConfidenceLevel = UnityEngine.Random.Range(0.6f, 0.9f)
            };
            
            // Generate skill predictions
            foreach (var skill in _analysis.SkillProgressions)
            {
                _analysis.Predictions.PredictedSkillLevels[skill.SkillName] = 
                    skill.CurrentLevel + skill.GrowthRate * 3f + UnityEngine.Random.Range(-5f, 5f);
            }
            
            // Generate trait predictions
            foreach (var trait in _analysis.TraitEvolutions)
            {
                float currentIntensity = trait.IntensityHistory.LastOrDefault()?.Value ?? 5f;
                _analysis.Predictions.PredictedTraitIntensities[trait.TraitName] = 
                    currentIntensity + trait.IntensityChange * 0.5f + UnityEngine.Random.Range(-1f, 1f);
            }
            
            // Generate event predictions
            _analysis.Predictions.PredictedEvents.AddRange(new[]
            {
                "Likely to achieve skill milestone in Combat",
                "Personality shift predicted in Charisma trait",
                "New relationship development expected",
                "Achievement unlock probability: High"
            });
        }
        
        private void ExportTimelineData()
        {
            EditorUtility.DisplayDialog("Export", "Timeline data export functionality coming soon!", "OK");
        }
        
        private void ExportPredictions()
        {
            EditorUtility.DisplayDialog("Export", "Predictions export functionality coming soon!", "OK");
        }
        
        #endregion
    }
}

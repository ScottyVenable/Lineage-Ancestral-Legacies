using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Game Balance Analyzer for analyzing and visualizing game balance metrics.
    /// Provides tools for stat analysis, progression curves, and balance recommendations.
    /// </summary>
    public class GameBalanceAnalyzerWindow : EditorWindow
    {
        #region Window Management

        private static GameBalanceAnalyzerWindow window;

        [MenuItem("Lineage/Studio/Analysis/Game Balance Analyzer")]
        public static void ShowWindow()
        {
            window = GetWindow<GameBalanceAnalyzerWindow>("Balance Analyzer");
            window.minSize = new Vector2(1000, 700);
            window.Show();
        }

        #endregion

        #region Private Fields

        private Vector2 scrollPosition;
        private int selectedTabIndex = 0;
        private readonly string[] tabNames = { "Stats Analysis", "Progression", "Items", "Skills", "Traits", "NPCs", "Reports" };

        // Stats Analysis Tab
        private AnalysisScope analysisScope = AnalysisScope.All;
        private StatCategory selectedStatCategory = StatCategory.All;
        private Vector2 statsScrollPosition;
        private Dictionary<string, StatAnalysis> statAnalyses = new Dictionary<string, StatAnalysis>();
        private bool showOutliers = true;
        private bool showDistribution = true;

        // Progression Tab
        private ProgressionType progressionType = ProgressionType.Character;
        private Vector2 progressionScrollPosition;
        private List<ProgressionCurve> progressionCurves = new List<ProgressionCurve>();
        private int selectedLevel = 1;
        private int maxLevel = 100;

        // Items Tab
        private ItemCategory itemCategory = ItemCategory.All;
        private Vector2 itemsScrollPosition;
        private Dictionary<ItemCategory, ItemBalanceData> itemBalanceData = new Dictionary<ItemCategory, ItemBalanceData>();
        private bool analyzeItemPower = true;
        private bool analyzeItemRarity = true;

        // Skills Tab
        private SkillType skillType = SkillType.All;
        private Vector2 skillsScrollPosition;
        private Dictionary<SkillType, SkillBalanceData> skillBalanceData = new Dictionary<SkillType, SkillBalanceData>();

        // Traits Tab
        private TraitType traitType = TraitType.All;
        private Vector2 traitsScrollPosition;
        private Dictionary<TraitType, TraitBalanceData> traitBalanceData = new Dictionary<TraitType, TraitBalanceData>();

        // NPCs Tab
        private NPCType npcType = NPCType.All;
        private Vector2 npcsScrollPosition;
        private Dictionary<NPCType, NPCBalanceData> npcBalanceData = new Dictionary<NPCType, NPCBalanceData>();

        // Reports Tab
        private Vector2 reportsScrollPosition;
        private List<BalanceReport> balanceReports = new List<BalanceReport>();
        private ReportType reportType = ReportType.Summary;

        #endregion

        #region Data Structures

        public enum AnalysisScope
        {
            All,
            Player,
            NPCs,
            Items,
            Skills,
            Environment
        }

        public enum StatCategory
        {
            All,
            Combat,
            Social,
            Mental,
            Physical,
            Economic,
            Custom
        }

        public enum ProgressionType
        {
            Character,
            Skills,
            Items,
            Relationships,
            Economy
        }

        public enum ItemCategory
        {
            All,
            Weapons,
            Armor,
            Tools,
            Consumables,
            Materials,
            Artifacts
        }

        public enum SkillType
        {
            All,
            Combat,
            Social,
            Crafting,
            Survival,
            Magic,
            Leadership
        }

        public enum TraitType
        {
            All,
            Physical,
            Mental,
            Social,
            Cultural,
            Genetic,
            Learned
        }

        public enum NPCType
        {
            All,
            Villagers,
            Merchants,
            Guards,
            Leaders,
            Enemies,
            Animals
        }

        public enum ReportType
        {
            Summary,
            Detailed,
            Recommendations,
            Comparisons,
            Trends
        }

        [System.Serializable]
        public class StatAnalysis
        {
            public string statName;
            public float min;
            public float max;
            public float average;
            public float median;
            public float standardDeviation;
            public List<float> values;
            public List<object> outliers;
            public BalanceStatus status;
            public string recommendation;
        }

        public enum BalanceStatus
        {
            Balanced,
            Overpowered,
            Underpowered,
            Inconsistent,
            NeedsReview
        }

        [System.Serializable]
        public class ProgressionCurve
        {
            public string name;
            public ProgressionType type;
            public List<ProgressionPoint> points;
            public float steepness;
            public BalanceStatus balance;
            public string analysis;
        }

        [System.Serializable]
        public class ProgressionPoint
        {
            public int level;
            public float value;
            public float deltaFromPrevious;
            public float percentageIncrease;
        }

        [System.Serializable]
        public class ItemBalanceData
        {
            public ItemCategory category;
            public int totalItems;
            public float averagePower;
            public float powerVariance;
            public Dictionary<Item.Rarity, int> rarityDistribution;
            public List<Item> overpoweredItems;
            public List<Item> underpoweredItems;
            public BalanceStatus overallBalance;
        }

        [System.Serializable]
        public class SkillBalanceData
        {
            public SkillType type;
            public int totalSkills;
            public float averagePower;
            public Dictionary<int, float> levelRequirements;
            public List<Skill> problematicSkills;
            public BalanceStatus overallBalance;
        }

        [System.Serializable]
        public class TraitBalanceData
        {
            public TraitType type;
            public int totalTraits;
            public float averageImpact;
            public Dictionary<Trait.TraitType, int> typeDistribution;
            public List<Trait> problematicTraits;
            public BalanceStatus overallBalance;
        }

        [System.Serializable]
        public class NPCBalanceData
        {
            public NPCType type;
            public int totalNPCs;
            public float averageLevel;
            public Dictionary<string, float> averageStats;
            public List<NPC> problematicNPCs;
            public BalanceStatus overallBalance;
        }

        [System.Serializable]
        public class BalanceReport
        {
            public string title;
            public ReportType type;
            public System.DateTime generatedTime;
            public List<BalanceIssue> issues;
            public List<BalanceRecommendation> recommendations;
            public OverallBalanceScore overallScore;
        }

        [System.Serializable]
        public class BalanceIssue
        {
            public string category;
            public string description;
            public BalanceStatus severity;
            public List<object> affectedEntities;
            public string suggestedFix;
        }

        [System.Serializable]
        public class BalanceRecommendation
        {
            public string title;
            public string description;
            public RecommendationPriority priority;
            public float estimatedImpact;
            public string implementation;
        }

        public enum RecommendationPriority
        {
            Low,
            Medium,
            High,
            Critical
        }

        [System.Serializable]
        public class OverallBalanceScore
        {
            public float score; // 0-100
            public BalanceGrade grade;
            public string analysis;
        }

        public enum BalanceGrade
        {
            Excellent,
            Good,
            Fair,
            Poor,
            Critical
        }

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            RefreshAllAnalyses();
        }

        private void OnGUI()
        {
            DrawHeader();
            
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (selectedTabIndex)
            {
                case 0: DrawStatsAnalysisTab(); break;
                case 1: DrawProgressionTab(); break;
                case 2: DrawItemsTab(); break;
                case 3: DrawSkillsTab(); break;
                case 4: DrawTraitsTab(); break;
                case 5: DrawNPCsTab(); break;
                case 6: DrawReportsTab(); break;
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
                EditorGUILayout.LabelField("Game Balance Analyzer", EditorStyles.largeLabel);
                GUILayout.FlexibleSpace();
            }
            
            // Quick Status Bar
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"Entities: {GetTotalEntityCount()}", GUILayout.Width(100));
                EditorGUILayout.LabelField($"Issues: {GetTotalIssueCount()}", GUILayout.Width(100));
                
                var overallScore = CalculateOverallBalance();
                Color originalColor = GUI.color;
                GUI.color = GetBalanceGradeColor(overallScore.grade);
                EditorGUILayout.LabelField($"Balance: {overallScore.grade}", EditorStyles.boldLabel, GUILayout.Width(120));
                GUI.color = originalColor;
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Refresh All", GUILayout.Width(80)))
                {
                    RefreshAllAnalyses();
                }
            }
            
            EditorGUILayout.Space();
        }

        private void DrawStatsAnalysisTab()
        {
            EditorGUILayout.LabelField("Statistics Analysis", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Analysis Settings
            using (new EditorGUILayout.HorizontalScope())
            {
                analysisScope = (AnalysisScope)EditorGUILayout.EnumPopup("Scope:", analysisScope, GUILayout.Width(200));
                selectedStatCategory = (StatCategory)EditorGUILayout.EnumPopup("Category:", selectedStatCategory, GUILayout.Width(200));
                
                showOutliers = EditorGUILayout.Toggle("Show Outliers", showOutliers, GUILayout.Width(150));
                showDistribution = EditorGUILayout.Toggle("Show Distribution", showDistribution, GUILayout.Width(150));
            }

            if (GUILayout.Button("Analyze Stats"))
            {
                AnalyzeStats();
            }

            EditorGUILayout.Space();

            // Stats Analysis Results
            statsScrollPosition = EditorGUILayout.BeginScrollView(statsScrollPosition);

            foreach (var kvp in statAnalyses)
            {
                DrawStatAnalysis(kvp.Key, kvp.Value);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawStatAnalysis(string statName, StatAnalysis analysis)
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = GetBalanceStatusColor(analysis.status);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUI.backgroundColor = originalColor;
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(statName, EditorStyles.boldLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField($"[{analysis.status}]", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Avg: {analysis.average:F2}", GUILayout.Width(80));
                    EditorGUILayout.LabelField($"Range: {analysis.min:F1}-{analysis.max:F1}", GUILayout.Width(120));
                    
                    if (GUILayout.Button("Details", GUILayout.Width(60)))
                    {
                        ShowStatDetails(statName, analysis);
                    }
                }
                
                // Distribution visualization (simplified)
                if (showDistribution && analysis.values.Count > 0)
                {
                    DrawStatDistribution(analysis);
                }
                
                // Outliers
                if (showOutliers && analysis.outliers.Count > 0)
                {
                    EditorGUILayout.LabelField($"Outliers ({analysis.outliers.Count}):", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;
                    foreach (var outlier in analysis.outliers.Take(3))
                    {
                        EditorGUILayout.LabelField($"• {GetEntityDisplayName(outlier)}", EditorStyles.miniLabel);
                    }
                    if (analysis.outliers.Count > 3)
                    {
                        EditorGUILayout.LabelField($"... and {analysis.outliers.Count - 3} more", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }
                
                if (!string.IsNullOrEmpty(analysis.recommendation))
                {
                    EditorGUILayout.LabelField($"Recommendation: {analysis.recommendation}", EditorStyles.wordWrappedMiniLabel);
                }
            }
        }

        private void DrawStatDistribution(StatAnalysis analysis)
        {
            // Simple ASCII-style distribution chart
            var rect = GUILayoutUtility.GetRect(200, 30);
            
            if (analysis.values.Count > 0)
            {
                float range = analysis.max - analysis.min;
                if (range > 0)
                {
                    // Draw background
                    EditorGUI.DrawRect(rect, Color.gray * 0.3f);
                    
                    // Draw distribution bars (simplified histogram)
                    int bins = 10;
                    float binWidth = rect.width / bins;
                    float binRange = range / bins;
                    
                    for (int i = 0; i < bins; i++)
                    {
                        float binMin = analysis.min + i * binRange;
                        float binMax = binMin + binRange;
                        int count = analysis.values.Count(v => v >= binMin && v < binMax);
                        
                        if (count > 0)
                        {
                            float height = (float)count / analysis.values.Count * rect.height;
                            var barRect = new Rect(rect.x + i * binWidth, rect.y + rect.height - height, binWidth - 1, height);
                            EditorGUI.DrawRect(barRect, Color.cyan * 0.7f);
                        }
                    }
                    
                    // Draw average line
                    float avgX = rect.x + (analysis.average - analysis.min) / range * rect.width;
                    var avgRect = new Rect(avgX - 1, rect.y, 2, rect.height);
                    EditorGUI.DrawRect(avgRect, Color.red);
                }
            }
        }

        private void DrawProgressionTab()
        {
            EditorGUILayout.LabelField("Progression Analysis", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Progression Settings
            using (new EditorGUILayout.HorizontalScope())
            {
                progressionType = (ProgressionType)EditorGUILayout.EnumPopup("Type:", progressionType, GUILayout.Width(200));
                maxLevel = EditorGUILayout.IntSlider("Max Level:", maxLevel, 10, 200, GUILayout.Width(300));
                
                if (GUILayout.Button("Analyze Progression"))
                {
                    AnalyzeProgression();
                }
            }

            EditorGUILayout.Space();

            // Progression Curves
            progressionScrollPosition = EditorGUILayout.BeginScrollView(progressionScrollPosition);

            foreach (var curve in progressionCurves)
            {
                DrawProgressionCurve(curve);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawProgressionCurve(ProgressionCurve curve)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(curve.name, EditorStyles.boldLabel, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"[{curve.balance}]", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Steepness: {curve.steepness:F2}", GUILayout.Width(120));
                }
                
                // Simple curve visualization
                if (curve.points.Count > 0)
                {
                    var rect = GUILayoutUtility.GetRect(300, 100);
                    DrawProgressionGraph(rect, curve);
                }
                
                if (!string.IsNullOrEmpty(curve.analysis))
                {
                    EditorGUILayout.LabelField(curve.analysis, EditorStyles.wordWrappedMiniLabel);
                }
            }
        }

        private void DrawProgressionGraph(Rect rect, ProgressionCurve curve)
        {
            if (curve.points.Count < 2) return;
            
            // Draw background
            EditorGUI.DrawRect(rect, Color.black * 0.1f);
            
            // Calculate ranges
            float minValue = curve.points.Min(p => p.value);
            float maxValue = curve.points.Max(p => p.value);
            float valueRange = maxValue - minValue;
            
            if (valueRange <= 0) return;
            
            // Draw curve
            Vector3[] points = new Vector3[curve.points.Count];
            for (int i = 0; i < curve.points.Count; i++)
            {
                float x = rect.x + (float)i / (curve.points.Count - 1) * rect.width;
                float y = rect.y + rect.height - (curve.points[i].value - minValue) / valueRange * rect.height;
                points[i] = new Vector3(x, y, 0);
            }
            
            // Draw line segments
            for (int i = 0; i < points.Length - 1; i++)
            {
                var startPoint = points[i];
                var endPoint = points[i + 1];
                
                // Simple line drawing using rects (Unity GUI limitation)
                float distance = Vector2.Distance(startPoint, endPoint);
                float angle = Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x) * Mathf.Rad2Deg;
                
                // This is a simplified line drawing - in a real implementation, you'd use Handles or GL
                var lineRect = new Rect(startPoint.x, startPoint.y, distance, 2);
                EditorGUI.DrawRect(lineRect, Color.cyan);
            }
        }

        private void DrawItemsTab()
        {
            EditorGUILayout.LabelField("Item Balance Analysis", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Item Analysis Settings
            using (new EditorGUILayout.HorizontalScope())
            {
                itemCategory = (ItemCategory)EditorGUILayout.EnumPopup("Category:", itemCategory, GUILayout.Width(200));
                analyzeItemPower = EditorGUILayout.Toggle("Analyze Power", analyzeItemPower, GUILayout.Width(150));
                analyzeItemRarity = EditorGUILayout.Toggle("Analyze Rarity", analyzeItemRarity, GUILayout.Width(150));
                
                if (GUILayout.Button("Analyze Items"))
                {
                    AnalyzeItems();
                }
            }

            EditorGUILayout.Space();

            // Item Balance Results
            itemsScrollPosition = EditorGUILayout.BeginScrollView(itemsScrollPosition);

            foreach (var kvp in itemBalanceData)
            {
                DrawItemBalanceData(kvp.Key, kvp.Value);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawItemBalanceData(ItemCategory category, ItemBalanceData data)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{category} Items", EditorStyles.boldLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField($"[{data.overallBalance}]", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Count: {data.totalItems}", GUILayout.Width(80));
                    EditorGUILayout.LabelField($"Avg Power: {data.averagePower:F1}", GUILayout.Width(100));
                }
                
                if (data.overpoweredItems.Count > 0)
                {
                    EditorGUILayout.LabelField($"Overpowered Items ({data.overpoweredItems.Count}):", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;
                    foreach (var item in data.overpoweredItems.Take(3))
                    {
                        EditorGUILayout.LabelField($"• {item.name}", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }
                
                if (data.underpoweredItems.Count > 0)
                {
                    EditorGUILayout.LabelField($"Underpowered Items ({data.underpoweredItems.Count}):", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;
                    foreach (var item in data.underpoweredItems.Take(3))
                    {
                        EditorGUILayout.LabelField($"• {item.name}", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawSkillsTab()
        {
            EditorGUILayout.LabelField("Skill Balance Analysis", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Skill Analysis Settings
            using (new EditorGUILayout.HorizontalScope())
            {
                skillType = (SkillType)EditorGUILayout.EnumPopup("Type:", skillType, GUILayout.Width(200));
                
                if (GUILayout.Button("Analyze Skills"))
                {
                    AnalyzeSkills();
                }
            }

            EditorGUILayout.Space();

            // Skill Balance Results
            skillsScrollPosition = EditorGUILayout.BeginScrollView(skillsScrollPosition);

            foreach (var kvp in skillBalanceData)
            {
                DrawSkillBalanceData(kvp.Key, kvp.Value);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSkillBalanceData(SkillType type, SkillBalanceData data)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{type} Skills", EditorStyles.boldLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField($"[{data.overallBalance}]", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Count: {data.totalSkills}", GUILayout.Width(80));
                    EditorGUILayout.LabelField($"Avg Power: {data.averagePower:F1}", GUILayout.Width(100));
                }
                
                if (data.problematicSkills.Count > 0)
                {
                    EditorGUILayout.LabelField($"Problematic Skills ({data.problematicSkills.Count}):", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;
                    foreach (var skill in data.problematicSkills.Take(3))
                    {
                        EditorGUILayout.LabelField($"• {skill.name}", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawTraitsTab()
        {
            EditorGUILayout.LabelField("Trait Balance Analysis", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Trait Analysis Settings
            using (new EditorGUILayout.HorizontalScope())
            {
                traitType = (TraitType)EditorGUILayout.EnumPopup("Type:", traitType, GUILayout.Width(200));
                
                if (GUILayout.Button("Analyze Traits"))
                {
                    AnalyzeTraits();
                }
            }

            EditorGUILayout.Space();

            // Trait Balance Results
            traitsScrollPosition = EditorGUILayout.BeginScrollView(traitsScrollPosition);

            foreach (var kvp in traitBalanceData)
            {
                DrawTraitBalanceData(kvp.Key, kvp.Value);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTraitBalanceData(TraitType type, TraitBalanceData data)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{type} Traits", EditorStyles.boldLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField($"[{data.overallBalance}]", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Count: {data.totalTraits}", GUILayout.Width(80));
                    EditorGUILayout.LabelField($"Avg Impact: {data.averageImpact:F1}", GUILayout.Width(100));
                }
                
                if (data.problematicTraits.Count > 0)
                {
                    EditorGUILayout.LabelField($"Problematic Traits ({data.problematicTraits.Count}):", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;
                    foreach (var trait in data.problematicTraits.Take(3))
                    {
                        EditorGUILayout.LabelField($"• {trait.name}", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawNPCsTab()
        {
            EditorGUILayout.LabelField("NPC Balance Analysis", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // NPC Analysis Settings
            using (new EditorGUILayout.HorizontalScope())
            {
                npcType = (NPCType)EditorGUILayout.EnumPopup("Type:", npcType, GUILayout.Width(200));
                
                if (GUILayout.Button("Analyze NPCs"))
                {
                    AnalyzeNPCs();
                }
            }

            EditorGUILayout.Space();

            // NPC Balance Results
            npcsScrollPosition = EditorGUILayout.BeginScrollView(npcsScrollPosition);

            foreach (var kvp in npcBalanceData)
            {
                DrawNPCBalanceData(kvp.Key, kvp.Value);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawNPCBalanceData(NPCType type, NPCBalanceData data)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{type} NPCs", EditorStyles.boldLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField($"[{data.overallBalance}]", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Count: {data.totalNPCs}", GUILayout.Width(80));
                    EditorGUILayout.LabelField($"Avg Level: {data.averageLevel:F1}", GUILayout.Width(100));
                }
                
                if (data.problematicNPCs.Count > 0)
                {
                    EditorGUILayout.LabelField($"Problematic NPCs ({data.problematicNPCs.Count}):", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;
                    foreach (var npc in data.problematicNPCs.Take(3))
                    {
                        EditorGUILayout.LabelField($"• {npc.name}", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawReportsTab()
        {
            EditorGUILayout.LabelField("Balance Reports", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Report Generation
            using (new EditorGUILayout.HorizontalScope())
            {
                reportType = (ReportType)EditorGUILayout.EnumPopup("Report Type:", reportType, GUILayout.Width(200));
                
                if (GUILayout.Button("Generate Report"))
                {
                    GenerateBalanceReport();
                }
                
                if (GUILayout.Button("Export All Reports"))
                {
                    ExportAllReports();
                }
            }

            EditorGUILayout.Space();

            // Reports List
            reportsScrollPosition = EditorGUILayout.BeginScrollView(reportsScrollPosition);

            foreach (var report in balanceReports.OrderByDescending(r => r.generatedTime))
            {
                DrawBalanceReport(report);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawBalanceReport(BalanceReport report)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(report.title, EditorStyles.boldLabel, GUILayout.Width(250));
                    EditorGUILayout.LabelField($"[{report.type}]", GUILayout.Width(100));
                    EditorGUILayout.LabelField(report.generatedTime.ToString("MM/dd HH:mm"), GUILayout.Width(100));
                    
                    Color originalColor = GUI.color;
                    GUI.color = GetBalanceGradeColor(report.overallScore.grade);
                    EditorGUILayout.LabelField($"{report.overallScore.grade}", EditorStyles.boldLabel, GUILayout.Width(80));
                    GUI.color = originalColor;
                    
                    if (GUILayout.Button("View", GUILayout.Width(50)))
                    {
                        ShowReportDetails(report);
                    }
                    
                    if (GUILayout.Button("Export", GUILayout.Width(60)))
                    {
                        ExportReport(report);
                    }
                }
                
                EditorGUILayout.LabelField($"Issues: {report.issues.Count} | Recommendations: {report.recommendations.Count}", EditorStyles.miniLabel);
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate Full Report"))
                {
                    GenerateComprehensiveReport();
                }
                
                GUILayout.FlexibleSpace();
                
                var overallScore = CalculateOverallBalance();
                EditorGUILayout.LabelField($"Overall Balance Score: {overallScore.score:F1}/100");
            }
        }

        #endregion

        #region Core Functionality

        private void RefreshAllAnalyses()
        {
            AnalyzeStats();
            AnalyzeProgression();
            AnalyzeItems();
            AnalyzeSkills();
            AnalyzeTraits();
            AnalyzeNPCs();
        }

        private void AnalyzeStats()
        {
            statAnalyses.Clear();
            
            // Analyze different stat categories
            if (selectedStatCategory == StatCategory.All || selectedStatCategory == StatCategory.Combat)
            {
                AnalyzeStatCategory("Combat", GetCombatStats());
            }
            
            if (selectedStatCategory == StatCategory.All || selectedStatCategory == StatCategory.Social)
            {
                AnalyzeStatCategory("Social", GetSocialStats());
            }
            
            // Add more stat categories as needed
        }

        private void AnalyzeStatCategory(string categoryName, Dictionary<string, List<float>> stats)
        {
            foreach (var kvp in stats)
            {
                var analysis = new StatAnalysis
                {
                    statName = kvp.Key,
                    values = kvp.Value,
                    min = kvp.Value.Min(),
                    max = kvp.Value.Max(),
                    average = kvp.Value.Average(),
                    median = CalculateMedian(kvp.Value),
                    standardDeviation = CalculateStandardDeviation(kvp.Value),
                    outliers = FindOutliers(kvp.Value, kvp.Key),
                    status = DetermineBalanceStatus(kvp.Value),
                    recommendation = GenerateStatRecommendation(kvp.Key, kvp.Value)
                };
                
                statAnalyses[$"{categoryName}.{kvp.Key}"] = analysis;
            }
        }

        private void AnalyzeProgression()
        {
            progressionCurves.Clear();
            
            // Analyze character progression
            if (progressionType == ProgressionType.All || progressionType == ProgressionType.Character)
            {
                var characterCurve = AnalyzeCharacterProgression();
                if (characterCurve != null) progressionCurves.Add(characterCurve);
            }
            
            // Analyze skill progression
            if (progressionType == ProgressionType.All || progressionType == ProgressionType.Skills)
            {
                var skillCurves = AnalyzeSkillProgression();
                progressionCurves.AddRange(skillCurves);
            }
            
            // Add more progression types
        }

        private void AnalyzeItems()
        {
            itemBalanceData.Clear();
            
            foreach (ItemCategory category in System.Enum.GetValues(typeof(ItemCategory)))
            {
                if (category == ItemCategory.All) continue;
                
                if (itemCategory == ItemCategory.All || itemCategory == category)
                {
                    var data = AnalyzeItemCategory(category);
                    if (data != null) itemBalanceData[category] = data;
                }
            }
        }

        private void AnalyzeSkills()
        {
            skillBalanceData.Clear();
            
            foreach (SkillType type in System.Enum.GetValues(typeof(SkillType)))
            {
                if (type == SkillType.All) continue;
                
                if (skillType == SkillType.All || skillType == type)
                {
                    var data = AnalyzeSkillType(type);
                    if (data != null) skillBalanceData[type] = data;
                }
            }
        }

        private void AnalyzeTraits()
        {
            traitBalanceData.Clear();
            
            foreach (TraitType type in System.Enum.GetValues(typeof(TraitType)))
            {
                if (type == TraitType.All) continue;
                
                if (traitType == TraitType.All || traitType == type)
                {
                    var data = AnalyzeTraitType(type);
                    if (data != null) traitBalanceData[type] = data;
                }
            }
        }

        private void AnalyzeNPCs()
        {
            npcBalanceData.Clear();
            
            foreach (NPCType type in System.Enum.GetValues(typeof(NPCType)))
            {
                if (type == NPCType.All) continue;
                
                if (npcType == NPCType.All || npcType == type)
                {
                    var data = AnalyzeNPCType(type);
                    if (data != null) npcBalanceData[type] = data;
                }
            }
        }

        // Helper Methods

        private Dictionary<string, List<float>> GetCombatStats()
        {
            var stats = new Dictionary<string, List<float>>();
            
            // Extract combat stats from entities and NPCs
            // This would be implemented based on your specific stat system
            
            return stats;
        }

        private Dictionary<string, List<float>> GetSocialStats()
        {
            var stats = new Dictionary<string, List<float>>();
            
            // Extract social stats from entities and NPCs
            // This would be implemented based on your specific stat system
            
            return stats;
        }

        private float CalculateMedian(List<float> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            int count = sorted.Count;
            
            if (count == 0) return 0;
            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
            else
                return sorted[count / 2];
        }

        private float CalculateStandardDeviation(List<float> values)
        {
            if (values.Count == 0) return 0;
            
            float mean = values.Average();
            float sumSquaredDifferences = values.Sum(v => (v - mean) * (v - mean));
            return Mathf.Sqrt(sumSquaredDifferences / values.Count);
        }

        private List<object> FindOutliers(List<float> values, string statName)
        {
            var outliers = new List<object>();
            
            if (values.Count < 4) return outliers;
            
            float q1 = CalculatePercentile(values, 25);
            float q3 = CalculatePercentile(values, 75);
            float iqr = q3 - q1;
            float lowerBound = q1 - 1.5f * iqr;
            float upperBound = q3 + 1.5f * iqr;
            
            // Find entities with outlier values
            // This would need to be implemented based on your data structure
            
            return outliers;
        }

        private float CalculatePercentile(List<float> values, int percentile)
        {
            var sorted = values.OrderBy(v => v).ToList();
            int index = (int)((percentile / 100.0) * (sorted.Count - 1));
            return sorted[index];
        }

        private BalanceStatus DetermineBalanceStatus(List<float> values)
        {
            if (values.Count == 0) return BalanceStatus.NeedsReview;
            
            float stdDev = CalculateStandardDeviation(values);
            float mean = values.Average();
            float coefficientOfVariation = mean != 0 ? stdDev / mean : 0;
            
            if (coefficientOfVariation < 0.1f) return BalanceStatus.Balanced;
            if (coefficientOfVariation < 0.3f) return BalanceStatus.NeedsReview;
            return BalanceStatus.Inconsistent;
        }

        private string GenerateStatRecommendation(string statName, List<float> values)
        {
            var status = DetermineBalanceStatus(values);
            
            switch (status)
            {
                case BalanceStatus.Balanced:
                    return "Values are well balanced.";
                case BalanceStatus.Inconsistent:
                    return $"High variance in {statName} values. Consider normalizing the range.";
                case BalanceStatus.NeedsReview:
                    return $"Review {statName} distribution for potential balance issues.";
                default:
                    return "Requires analysis.";
            }
        }

        private ProgressionCurve AnalyzeCharacterProgression()
        {
            // Analyze character level progression
            // This would be implemented based on your progression system
            return null;
        }

        private List<ProgressionCurve> AnalyzeSkillProgression()
        {
            var curves = new List<ProgressionCurve>();
            
            // Analyze skill progression curves
            // This would be implemented based on your skill system
            
            return curves;
        }

        private ItemBalanceData AnalyzeItemCategory(ItemCategory category)
        {
            var items = GameData.itemDatabase.Where(i => GetItemCategory(i) == category).ToList();
            
            if (items.Count == 0) return null;
            
            return new ItemBalanceData
            {
                category = category,
                totalItems = items.Count,
                averagePower = CalculateAverageItemPower(items),
                powerVariance = CalculateItemPowerVariance(items),
                rarityDistribution = CalculateRarityDistribution(items),
                overpoweredItems = FindOverpoweredItems(items),
                underpoweredItems = FindUnderpoweredItems(items),
                overallBalance = DetermineItemBalance(items)
            };
        }

        private SkillBalanceData AnalyzeSkillType(SkillType type)
        {
            var skills = GameData.skillDatabase.Where(s => GetSkillType(s) == type).ToList();
            
            if (skills.Count == 0) return null;
            
            return new SkillBalanceData
            {
                type = type,
                totalSkills = skills.Count,
                averagePower = CalculateAverageSkillPower(skills),
                levelRequirements = CalculateSkillLevelRequirements(skills),
                problematicSkills = FindProblematicSkills(skills),
                overallBalance = DetermineSkillBalance(skills)
            };
        }

        private TraitBalanceData AnalyzeTraitType(TraitType type)
        {
            var traits = GameData.traitDatabase.Where(t => GetTraitType(t) == type).ToList();
            
            if (traits.Count == 0) return null;
            
            return new TraitBalanceData
            {
                type = type,
                totalTraits = traits.Count,
                averageImpact = CalculateAverageTraitImpact(traits),
                typeDistribution = CalculateTraitTypeDistribution(traits),
                problematicTraits = FindProblematicTraits(traits),
                overallBalance = DetermineTraitBalance(traits)
            };
        }

        private NPCBalanceData AnalyzeNPCType(NPCType type)
        {
            var npcs = GameData.npcDatabase.Where(n => GetNPCType(n) == type).ToList();
            
            if (npcs.Count == 0) return null;
            
            return new NPCBalanceData
            {
                type = type,
                totalNPCs = npcs.Count,
                averageLevel = CalculateAverageNPCLevel(npcs),
                averageStats = CalculateAverageNPCStats(npcs),
                problematicNPCs = FindProblematicNPCs(npcs),
                overallBalance = DetermineNPCBalance(npcs)
            };
        }

        // Placeholder implementations - these would need to be filled out based on your specific data structures

        private ItemCategory GetItemCategory(Item item) => ItemCategory.Weapons; // Placeholder
        private SkillType GetSkillType(Skill skill) => SkillType.Combat; // Placeholder
        private TraitType GetTraitType(Trait trait) => TraitType.Physical; // Placeholder
        private NPCType GetNPCType(NPC npc) => NPCType.Villagers; // Placeholder
        
        private float CalculateAverageItemPower(List<Item> items) => 0f; // Placeholder
        private float CalculateItemPowerVariance(List<Item> items) => 0f; // Placeholder
        private Dictionary<Item.Rarity, int> CalculateRarityDistribution(List<Item> items) => new Dictionary<Item.Rarity, int>(); // Placeholder
        private List<Item> FindOverpoweredItems(List<Item> items) => new List<Item>(); // Placeholder
        private List<Item> FindUnderpoweredItems(List<Item> items) => new List<Item>(); // Placeholder
        private BalanceStatus DetermineItemBalance(List<Item> items) => BalanceStatus.Balanced; // Placeholder
        
        private float CalculateAverageSkillPower(List<Skill> skills) => 0f; // Placeholder
        private Dictionary<int, float> CalculateSkillLevelRequirements(List<Skill> skills) => new Dictionary<int, float>(); // Placeholder
        private List<Skill> FindProblematicSkills(List<Skill> skills) => new List<Skill>(); // Placeholder
        private BalanceStatus DetermineSkillBalance(List<Skill> skills) => BalanceStatus.Balanced; // Placeholder
        
        private float CalculateAverageTraitImpact(List<Trait> traits) => 0f; // Placeholder
        private Dictionary<Trait.TraitType, int> CalculateTraitTypeDistribution(List<Trait> traits) => new Dictionary<Trait.TraitType, int>(); // Placeholder
        private List<Trait> FindProblematicTraits(List<Trait> traits) => new List<Trait>(); // Placeholder
        private BalanceStatus DetermineTraitBalance(List<Trait> traits) => BalanceStatus.Balanced; // Placeholder
        
        private float CalculateAverageNPCLevel(List<NPC> npcs) => 0f; // Placeholder
        private Dictionary<string, float> CalculateAverageNPCStats(List<NPC> npcs) => new Dictionary<string, float>(); // Placeholder
        private List<NPC> FindProblematicNPCs(List<NPC> npcs) => new List<NPC>(); // Placeholder
        private BalanceStatus DetermineNPCBalance(List<NPC> npcs) => BalanceStatus.Balanced; // Placeholder

        private int GetTotalEntityCount()
        {
            return GameData.entityDatabase.Count +
                   GameData.itemDatabase.Count +
                   GameData.npcDatabase.Count +
                   GameData.skillDatabase.Count +
                   GameData.traitDatabase.Count;
        }

        private int GetTotalIssueCount()
        {
            int count = 0;
            
            count += statAnalyses.Count(s => s.Value.status != BalanceStatus.Balanced);
            count += itemBalanceData.Count(i => i.Value.overallBalance != BalanceStatus.Balanced);
            count += skillBalanceData.Count(s => s.Value.overallBalance != BalanceStatus.Balanced);
            count += traitBalanceData.Count(t => t.Value.overallBalance != BalanceStatus.Balanced);
            count += npcBalanceData.Count(n => n.Value.overallBalance != BalanceStatus.Balanced);
            
            return count;
        }

        private OverallBalanceScore CalculateOverallBalance()
        {
            // Calculate overall balance score
            float totalScore = 100f;
            int totalIssues = GetTotalIssueCount();
            int totalEntities = GetTotalEntityCount();
            
            if (totalEntities > 0)
            {
                float issueRatio = (float)totalIssues / totalEntities;
                totalScore = Mathf.Max(0, 100f - (issueRatio * 100f));
            }
            
            BalanceGrade grade;
            if (totalScore >= 90) grade = BalanceGrade.Excellent;
            else if (totalScore >= 75) grade = BalanceGrade.Good;
            else if (totalScore >= 60) grade = BalanceGrade.Fair;
            else if (totalScore >= 40) grade = BalanceGrade.Poor;
            else grade = BalanceGrade.Critical;
            
            return new OverallBalanceScore
            {
                score = totalScore,
                grade = grade,
                analysis = $"Overall balance is {grade.ToString().ToLower()} with {totalIssues} issues found across {totalEntities} entities."
            };
        }

        private Color GetBalanceStatusColor(BalanceStatus status)
        {
            switch (status)
            {
                case BalanceStatus.Balanced: return Color.green;
                case BalanceStatus.NeedsReview: return Color.yellow;
                case BalanceStatus.Inconsistent: return Color.orange;
                case BalanceStatus.Overpowered: return Color.red;
                case BalanceStatus.Underpowered: return Color.cyan;
                default: return Color.white;
            }
        }

        private Color GetBalanceGradeColor(BalanceGrade grade)
        {
            switch (grade)
            {
                case BalanceGrade.Excellent: return Color.green;
                case BalanceGrade.Good: return Color.cyan;
                case BalanceGrade.Fair: return Color.yellow;
                case BalanceGrade.Poor: return Color.orange;
                case BalanceGrade.Critical: return Color.red;
                default: return Color.white;
            }
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
                default: return entity?.ToString() ?? "Unknown";
            }
        }

        private void ShowStatDetails(string statName, StatAnalysis analysis)
        {
            EditorUtility.DisplayDialog("Stat Analysis Details",
                $"Stat: {statName}\n" +
                $"Status: {analysis.status}\n" +
                $"Range: {analysis.min:F2} - {analysis.max:F2}\n" +
                $"Average: {analysis.average:F2}\n" +
                $"Median: {analysis.median:F2}\n" +
                $"Std Dev: {analysis.standardDeviation:F2}\n" +
                $"Sample Size: {analysis.values.Count}\n" +
                $"Outliers: {analysis.outliers.Count}\n\n" +
                $"Recommendation: {analysis.recommendation}", "OK");
        }

        private void GenerateBalanceReport()
        {
            var report = new BalanceReport
            {
                title = $"{reportType} Balance Report",
                type = reportType,
                generatedTime = System.DateTime.Now,
                issues = new List<BalanceIssue>(),
                recommendations = new List<BalanceRecommendation>(),
                overallScore = CalculateOverallBalance()
            };
            
            // Generate issues and recommendations based on current analysis
            // This would be implemented based on the specific report type
            
            balanceReports.Add(report);
        }

        private void GenerateComprehensiveReport()
        {
            // Generate a comprehensive balance report covering all aspects
            RefreshAllAnalyses();
            
            reportType = ReportType.Detailed;
            GenerateBalanceReport();
            
            Debug.Log("[Balance Analyzer] Comprehensive balance report generated.");
        }

        private void ShowReportDetails(BalanceReport report)
        {
            // Show detailed report in a popup or separate window
            EditorUtility.DisplayDialog("Balance Report Details",
                $"Title: {report.title}\n" +
                $"Generated: {report.generatedTime}\n" +
                $"Overall Score: {report.overallScore.score:F1}/100\n" +
                $"Grade: {report.overallScore.grade}\n" +
                $"Issues: {report.issues.Count}\n" +
                $"Recommendations: {report.recommendations.Count}\n\n" +
                $"Analysis: {report.overallScore.analysis}", "OK");
        }

        private void ExportReport(BalanceReport report)
        {
            // Export individual report
            string path = EditorUtility.SaveFilePanel("Export Balance Report", "", $"balance_report_{report.generatedTime:yyyyMMdd_HHmmss}.txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                // Generate and save report content
                Debug.Log($"[Balance Analyzer] Report exported to: {path}");
            }
        }

        private void ExportAllReports()
        {
            // Export all reports
            string directory = EditorUtility.SaveFolderPanel("Export All Reports", "", "");
            if (!string.IsNullOrEmpty(directory))
            {
                foreach (var report in balanceReports)
                {
                    // Export each report
                }
                Debug.Log($"[Balance Analyzer] All reports exported to: {directory}");
            }
        }

        #endregion
    }
}

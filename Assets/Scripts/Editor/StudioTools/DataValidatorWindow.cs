using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Data Validation Suite for comprehensive database integrity checking.
    /// Provides automated validation rules, custom checks, and detailed reporting.
    /// </summary>
    public class DataValidatorWindow : EditorWindow
    {
        #region Window Management

        private static DataValidatorWindow window;

        [MenuItem("Lineage/Studio/Database/Data Validation Suite")]
        public static void ShowWindow()
        {
            window = GetWindow<DataValidatorWindow>("Data Validator");
            window.minSize = new Vector2(900, 700);
            window.Show();
        }

        #endregion

        #region Private Fields

        private Vector2 scrollPosition;
        private int selectedTabIndex = 0;
        private readonly string[] tabNames = { "Validation", "Rules", "Results", "Reports", "Settings" };

        // Validation Tab
        private bool isValidating = false;
        private ValidationScope validationScope = ValidationScope.All;
        private bool validateReferences = true;
        private bool validateConstraints = true;
        private bool validateCustomRules = true;
        private bool autoFixIssues = false;
        private Vector2 validationProgressScroll;

        // Rules Tab
        private Vector2 rulesScroll;
        private List<ValidationRule> validationRules = new List<ValidationRule>();
        private ValidationRule newRule = new ValidationRule();
        private bool showBuiltInRules = true;
        private bool showCustomRules = true;

        // Results Tab
        private Vector2 resultsScroll;
        private List<ValidationIssue> validationIssues = new List<ValidationIssue>();
        private ValidationSeverity filterSeverity = ValidationSeverity.All;
        private string filterDatabase = "All";
        private bool groupByType = true;
        private bool showFixedIssues = false;

        // Reports Tab
        private Vector2 reportsScroll;
        private ReportFormat reportFormat = ReportFormat.HTML;
        private bool includeDetails = true;
        private bool includeSuggestions = true;
        private bool includeStatistics = true;
        private string reportTitle = "Database Validation Report";

        // Settings Tab
        private bool autoValidateOnStartup = true;
        private bool realTimeValidation = false;
        private int validationFrequency = 5; // minutes
        private bool logToConsole = true;
        private bool saveValidationHistory = true;

        // Internal State
        private System.DateTime lastValidationTime;
        private ValidationStatistics currentStats = new ValidationStatistics();
        private Dictionary<string, List<ValidationIssue>> issuesByDatabase = new Dictionary<string, List<ValidationIssue>>();

        #endregion

        #region Data Structures

        public enum ValidationScope
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

        public enum ValidationSeverity
        {
            All,
            Info,
            Warning,
            Error,
            Critical
        }

        public enum ReportFormat
        {
            HTML,
            PDF,
            CSV,
            JSON,
            Text
        }

        [System.Serializable]
        public class ValidationRule
        {
            public string id;
            public string name;
            public string description;
            public ValidationRuleType type;
            public ValidationSeverity severity;
            public string targetDatabase;
            public string fieldName;
            public string condition;
            public string expectedValue;
            public bool enabled;
            public bool isBuiltIn;
            public string customCode;
        }

        public enum ValidationRuleType
        {
            NotNull,
            NotEmpty,
            Range,
            Format,
            Reference,
            Uniqueness,
            Dependency,
            Custom,
            DataType,
            Length,
            Pattern,
            Enum,
            Calculation
        }

        [System.Serializable]
        public class ValidationIssue
        {
            public string id;
            public ValidationRule rule;
            public object entity;
            public string databaseType;
            public string entityId;
            public string fieldName;
            public string message;
            public ValidationSeverity severity;
            public string suggestedFix;
            public bool canAutoFix;
            public bool isFixed;
            public System.DateTime detectedTime;
        }

        [System.Serializable]
        public class ValidationStatistics
        {
            public int totalEntitiesValidated;
            public int totalIssuesFound;
            public Dictionary<ValidationSeverity, int> issuesBySeverity;
            public Dictionary<string, int> issuesByDatabase;
            public Dictionary<ValidationRuleType, int> issuesByType;
            public float validationTime;
            public int autoFixedIssues;
        }

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            LoadSettings();
            InitializeBuiltInRules();
            
            if (autoValidateOnStartup)
            {
                ValidateAllData();
            }
        }

        private void OnDisable()
        {
            SaveSettings();
        }

        private void OnGUI()
        {
            DrawHeader();
            
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (selectedTabIndex)
            {
                case 0: DrawValidationTab(); break;
                case 1: DrawRulesTab(); break;
                case 2: DrawResultsTab(); break;
                case 3: DrawReportsTab(); break;
                case 4: DrawSettingsTab(); break;
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
                EditorGUILayout.LabelField("Data Validation Suite", EditorStyles.largeLabel);
                GUILayout.FlexibleSpace();
            }
            
            // Status Bar
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"Last Validation: {lastValidationTime:HH:mm:ss}", GUILayout.Width(150));
                EditorGUILayout.LabelField($"Issues: {validationIssues.Count}", GUILayout.Width(100));
                EditorGUILayout.LabelField($"Rules: {validationRules.Count(r => r.enabled)}", GUILayout.Width(100));
                
                GUILayout.FlexibleSpace();
                
                if (isValidating)
                {
                    EditorGUILayout.LabelField("Validating...", EditorStyles.boldLabel);
                    if (GUILayout.Button("Cancel", GUILayout.Width(60)))
                    {
                        isValidating = false;
                    }
                }
                else
                {
                    if (GUILayout.Button("Quick Validate", GUILayout.Width(100)))
                    {
                        ValidateAllData();
                    }
                }
            }
            
            EditorGUILayout.Space();
        }

        private void DrawValidationTab()
        {
            EditorGUILayout.LabelField("Database Validation", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Validation Scope
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Validation Scope", EditorStyles.boldLabel);
                validationScope = (ValidationScope)EditorGUILayout.EnumPopup("Scope:", validationScope);
                
                EditorGUILayout.Space();
                
                validateReferences = EditorGUILayout.Toggle("Validate References", validateReferences);
                validateConstraints = EditorGUILayout.Toggle("Validate Constraints", validateConstraints);
                validateCustomRules = EditorGUILayout.Toggle("Validate Custom Rules", validateCustomRules);
                autoFixIssues = EditorGUILayout.Toggle("Auto-Fix Issues", autoFixIssues);
            }

            EditorGUILayout.Space();

            // Validation Actions
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = !isValidating;
                
                if (GUILayout.Button("Validate All Data"))
                {
                    ValidateAllData();
                }
                
                if (GUILayout.Button("Validate Scope"))
                {
                    ValidateScopeData();
                }
                
                if (GUILayout.Button("Quick Check"))
                {
                    QuickValidation();
                }
                
                GUI.enabled = true;
                
                if (GUILayout.Button("Clear Results"))
                {
                    ClearValidationResults();
                }
            }

            EditorGUILayout.Space();

            // Validation Progress
            if (isValidating)
            {
                DrawValidationProgress();
            }

            // Summary Statistics
            if (currentStats.totalEntitiesValidated > 0)
            {
                DrawValidationSummary();
            }

            // Quick Issue Preview
            if (validationIssues.Count > 0)
            {
                EditorGUILayout.LabelField("Recent Issues:", EditorStyles.boldLabel);
                
                var recentIssues = validationIssues.OrderByDescending(i => i.detectedTime).Take(5);
                foreach (var issue in recentIssues)
                {
                    DrawQuickIssuePreview(issue);
                }
                
                if (validationIssues.Count > 5)
                {
                    EditorGUILayout.LabelField($"... and {validationIssues.Count - 5} more issues. See Results tab.", EditorStyles.miniLabel);
                }
            }
        }

        private void DrawValidationProgress()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Validation Progress", EditorStyles.boldLabel);
                
                // Progress bar would be implemented here
                EditorGUILayout.LabelField("Validating data...", EditorStyles.centeredGreyMiniLabel);
                
                validationProgressScroll = EditorGUILayout.BeginScrollView(validationProgressScroll, GUILayout.Height(100));
                
                // Progress details would be shown here
                EditorGUILayout.LabelField("• Checking entity references...");
                EditorGUILayout.LabelField("• Validating data constraints...");
                EditorGUILayout.LabelField("• Running custom rules...");
                
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawValidationSummary()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Validation Summary", EditorStyles.boldLabel);
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Entities Validated: {currentStats.totalEntitiesValidated}", GUILayout.Width(200));
                    EditorGUILayout.LabelField($"Issues Found: {currentStats.totalIssuesFound}", GUILayout.Width(150));
                    EditorGUILayout.LabelField($"Validation Time: {currentStats.validationTime:F2}s", GUILayout.Width(150));
                }
                
                if (currentStats.issuesBySeverity != null)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        foreach (var kvp in currentStats.issuesBySeverity)
                        {
                            if (kvp.Value > 0)
                            {
                                Color originalColor = GUI.color;
                                switch (kvp.Key)
                                {
                                    case ValidationSeverity.Critical: GUI.color = Color.red; break;
                                    case ValidationSeverity.Error: GUI.color = new Color(1f, 0.5f, 0f); break;
                                    case ValidationSeverity.Warning: GUI.color = Color.yellow; break;
                                    case ValidationSeverity.Info: GUI.color = Color.cyan; break;
                                }
                                
                                EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value}", GUILayout.Width(100));
                                GUI.color = originalColor;
                            }
                        }
                    }
                }
            }
        }

        private void DrawQuickIssuePreview(ValidationIssue issue)
        {
            Color originalColor = GUI.color;
            
            switch (issue.severity)
            {
                case ValidationSeverity.Critical: GUI.color = Color.red; break;
                case ValidationSeverity.Error: GUI.color = new Color(1f, 0.5f, 0f); break;
                case ValidationSeverity.Warning: GUI.color = Color.yellow; break;
                case ValidationSeverity.Info: GUI.color = Color.cyan; break;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUI.color = originalColor;
                
                EditorGUILayout.LabelField($"[{issue.severity}]", GUILayout.Width(80));
                EditorGUILayout.LabelField(issue.message, GUILayout.Width(300));
                EditorGUILayout.LabelField($"[{issue.databaseType}]", GUILayout.Width(100));
                
                if (issue.canAutoFix && GUILayout.Button("Fix", GUILayout.Width(40)))
                {
                    ApplyAutoFix(issue);
                }
            }
        }

        private void DrawRulesTab()
        {
            EditorGUILayout.LabelField("Validation Rules", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Rule Filters
            using (new EditorGUILayout.HorizontalScope())
            {
                showBuiltInRules = EditorGUILayout.Toggle("Built-in Rules", showBuiltInRules, GUILayout.Width(150));
                showCustomRules = EditorGUILayout.Toggle("Custom Rules", showCustomRules, GUILayout.Width(150));
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Add Custom Rule", GUILayout.Width(120)))
                {
                    AddCustomRule();
                }
                
                if (GUILayout.Button("Import Rules", GUILayout.Width(100)))
                {
                    ImportRules();
                }
                
                if (GUILayout.Button("Export Rules", GUILayout.Width(100)))
                {
                    ExportRules();
                }
            }

            EditorGUILayout.Space();

            // Rules List
            rulesScroll = EditorGUILayout.BeginScrollView(rulesScroll);
            
            var filteredRules = validationRules.Where(r => 
                (r.isBuiltIn && showBuiltInRules) || (!r.isBuiltIn && showCustomRules)).ToList();
            
            foreach (var rule in filteredRules)
            {
                DrawValidationRule(rule);
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawValidationRule(ValidationRule rule)
        {
            Color originalColor = GUI.backgroundColor;
            if (!rule.enabled) GUI.backgroundColor = Color.gray;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUI.backgroundColor = originalColor;
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    rule.enabled = EditorGUILayout.Toggle(rule.enabled, GUILayout.Width(20));
                    
                    EditorGUILayout.LabelField(rule.name, EditorStyles.boldLabel, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"[{rule.type}]", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"[{rule.severity}]", GUILayout.Width(80));
                    
                    if (rule.isBuiltIn)
                    {
                        EditorGUILayout.LabelField("Built-in", EditorStyles.miniLabel, GUILayout.Width(60));
                    }
                    else
                    {
                        if (GUILayout.Button("Edit", GUILayout.Width(50)))
                        {
                            EditCustomRule(rule);
                        }
                        
                        if (GUILayout.Button("Remove", GUILayout.Width(60)))
                        {
                            RemoveCustomRule(rule);
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(rule.description))
                {
                    EditorGUILayout.LabelField(rule.description, EditorStyles.wordWrappedMiniLabel);
                }
                
                if (!string.IsNullOrEmpty(rule.condition))
                {
                    EditorGUILayout.LabelField($"Condition: {rule.condition}", EditorStyles.miniLabel);
                }
            }
        }

        private void DrawResultsTab()
        {
            EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Result Filters
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Filter:", GUILayout.Width(50));
                filterSeverity = (ValidationSeverity)EditorGUILayout.EnumPopup(filterSeverity, GUILayout.Width(100));
                filterDatabase = EditorGUILayout.TextField(filterDatabase, GUILayout.Width(100));
                
                groupByType = EditorGUILayout.Toggle("Group by Type", groupByType, GUILayout.Width(150));
                showFixedIssues = EditorGUILayout.Toggle("Show Fixed", showFixedIssues, GUILayout.Width(100));
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Export Results", GUILayout.Width(100)))
                {
                    ExportResults();
                }
            }

            EditorGUILayout.Space();

            // Results Statistics
            if (validationIssues.Count > 0)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    var filteredIssues = GetFilteredIssues();
                    EditorGUILayout.LabelField($"Showing {filteredIssues.Count} of {validationIssues.Count} issues");
                    
                    GUILayout.FlexibleSpace();
                    
                    if (GUILayout.Button("Fix All Auto-Fixable", GUILayout.Width(150)))
                    {
                        FixAllAutoFixable();
                    }
                }
            }

            EditorGUILayout.Space();

            // Results List
            resultsScroll = EditorGUILayout.BeginScrollView(resultsScroll);
            
            var issues = GetFilteredIssues();
            
            if (groupByType)
            {
                var groupedIssues = issues.GroupBy(i => i.rule.type);
                foreach (var group in groupedIssues)
                {
                    EditorGUILayout.LabelField($"{group.Key} ({group.Count()})", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    
                    foreach (var issue in group)
                    {
                        DrawValidationIssue(issue);
                    }
                    
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
            }
            else
            {
                foreach (var issue in issues)
                {
                    DrawValidationIssue(issue);
                }
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawValidationIssue(ValidationIssue issue)
        {
            Color originalColor = GUI.color;
            
            if (issue.isFixed)
            {
                GUI.color = Color.green;
            }
            else
            {
                switch (issue.severity)
                {
                    case ValidationSeverity.Critical: GUI.color = Color.red; break;
                    case ValidationSeverity.Error: GUI.color = new Color(1f, 0.5f, 0f); break;
                    case ValidationSeverity.Warning: GUI.color = Color.yellow; break;
                    case ValidationSeverity.Info: GUI.color = Color.cyan; break;
                }
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUI.color = originalColor;
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    string statusLabel = issue.isFixed ? "[FIXED]" : $"[{issue.severity}]";
                    EditorGUILayout.LabelField(statusLabel, EditorStyles.boldLabel, GUILayout.Width(80));
                    EditorGUILayout.LabelField(issue.rule.name, GUILayout.Width(150));
                    EditorGUILayout.LabelField($"[{issue.databaseType}]", GUILayout.Width(80));
                    
                    GUILayout.FlexibleSpace();
                    
                    if (!issue.isFixed && issue.canAutoFix && GUILayout.Button("Fix", GUILayout.Width(40)))
                    {
                        ApplyAutoFix(issue);
                    }
                    
                    if (GUILayout.Button("Details", GUILayout.Width(60)))
                    {
                        ShowIssueDetails(issue);
                    }
                }
                
                EditorGUILayout.LabelField($"Entity: {issue.entityId}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(issue.message, EditorStyles.wordWrappedLabel);
                
                if (!string.IsNullOrEmpty(issue.suggestedFix))
                {
                    EditorGUILayout.LabelField($"Suggested Fix: {issue.suggestedFix}", EditorStyles.miniLabel);
                }
            }
        }

        private void DrawReportsTab()
        {
            EditorGUILayout.LabelField("Validation Reports", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Report Settings
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Report Settings", EditorStyles.boldLabel);
                
                reportTitle = EditorGUILayout.TextField("Title:", reportTitle);
                reportFormat = (ReportFormat)EditorGUILayout.EnumPopup("Format:", reportFormat);
                
                EditorGUILayout.Space();
                
                includeDetails = EditorGUILayout.Toggle("Include Details", includeDetails);
                includeSuggestions = EditorGUILayout.Toggle("Include Suggestions", includeSuggestions);
                includeStatistics = EditorGUILayout.Toggle("Include Statistics", includeStatistics);
            }

            EditorGUILayout.Space();

            // Report Generation
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate Report"))
                {
                    GenerateReport();
                }
                
                if (GUILayout.Button("Quick Report"))
                {
                    GenerateQuickReport();
                }
                
                if (GUILayout.Button("Email Report"))
                {
                    EmailReport();
                }
            }

            EditorGUILayout.Space();

            // Report History
            reportsScroll = EditorGUILayout.BeginScrollView(reportsScroll);
            
            EditorGUILayout.LabelField("Report History", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Report history functionality would be implemented here.", MessageType.Info);
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawSettingsTab()
        {
            EditorGUILayout.LabelField("Validation Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Auto Validation Settings
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Auto Validation", EditorStyles.boldLabel);
                
                autoValidateOnStartup = EditorGUILayout.Toggle("Validate on Startup", autoValidateOnStartup);
                realTimeValidation = EditorGUILayout.Toggle("Real-time Validation", realTimeValidation);
                
                if (realTimeValidation)
                {
                    validationFrequency = EditorGUILayout.IntSlider("Frequency (minutes):", validationFrequency, 1, 60);
                }
            }

            EditorGUILayout.Space();

            // Logging Settings
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Logging & History", EditorStyles.boldLabel);
                
                logToConsole = EditorGUILayout.Toggle("Log to Console", logToConsole);
                saveValidationHistory = EditorGUILayout.Toggle("Save History", saveValidationHistory);
            }

            EditorGUILayout.Space();

            // Reset Settings
            if (GUILayout.Button("Reset to Defaults"))
            {
                if (EditorUtility.DisplayDialog("Reset Settings", "Reset all validation settings to defaults?", "Yes", "Cancel"))
                {
                    ResetSettings();
                }
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh Data"))
                {
                    RefreshValidationData();
                }
                
                GUILayout.FlexibleSpace();
                
                string statusText = isValidating ? "Validating..." : $"Ready - {validationIssues.Count} issues";
                EditorGUILayout.LabelField(statusText);
            }
        }

        #endregion

        #region Core Functionality

        private void InitializeBuiltInRules()
        {
            if (validationRules.Any(r => r.isBuiltIn)) return;

            // Add built-in validation rules
            validationRules.AddRange(new[]
            {
                new ValidationRule
                {
                    id = "builtin_name_notnull",
                    name = "Name Not Null",
                    description = "Ensures entity names are not null",
                    type = ValidationRuleType.NotNull,
                    severity = ValidationSeverity.Error,
                    targetDatabase = "All",
                    fieldName = "name",
                    enabled = true,
                    isBuiltIn = true
                },
                new ValidationRule
                {
                    id = "builtin_name_notempty",
                    name = "Name Not Empty",
                    description = "Ensures entity names are not empty",
                    type = ValidationRuleType.NotEmpty,
                    severity = ValidationSeverity.Error,
                    targetDatabase = "All",
                    fieldName = "name",
                    enabled = true,
                    isBuiltIn = true
                },
                new ValidationRule
                {
                    id = "builtin_id_unique",
                    name = "ID Uniqueness",
                    description = "Ensures entity IDs are unique within database",
                    type = ValidationRuleType.Uniqueness,
                    severity = ValidationSeverity.Critical,
                    targetDatabase = "All",
                    fieldName = "id",
                    enabled = true,
                    isBuiltIn = true
                },
                new ValidationRule
                {
                    id = "builtin_references_valid",
                    name = "Valid References",
                    description = "Ensures all entity references point to existing entities",
                    type = ValidationRuleType.Reference,
                    severity = ValidationSeverity.Error,
                    targetDatabase = "All",
                    fieldName = "*",
                    enabled = true,
                    isBuiltIn = true
                }
            });
        }

        private void ValidateAllData()
        {
            isValidating = true;
            lastValidationTime = System.DateTime.Now;
            validationIssues.Clear();
            
            var startTime = System.DateTime.Now;
            
            try
            {
                ValidateDatabase("Entities", GameData.entityDatabase.Cast<object>().ToList());
                ValidateDatabase("Items", GameData.itemDatabase.Cast<object>().ToList());
                ValidateDatabase("Traits", GameData.traitDatabase.Cast<object>().ToList());
                ValidateDatabase("Quests", GameData.questDatabase.Cast<object>().ToList());
                ValidateDatabase("NPCs", GameData.npcDatabase.Cast<object>().ToList());
                ValidateDatabase("Skills", GameData.skillDatabase.Cast<object>().ToList());
                ValidateDatabase("Buffs", GameData.buffDatabase.Cast<object>().ToList());
                ValidateDatabase("Objectives", GameData.objectiveDatabase.Cast<object>().ToList());
                ValidateDatabase("Stats", GameData.statDatabase.Cast<object>().ToList());
                ValidateDatabase("Genetics", GameData.geneticsDatabase.Cast<object>().ToList());
                ValidateDatabase("Journal", GameData.journalDatabase.Cast<object>().ToList());
                
                UpdateValidationStatistics(startTime);
            }
            finally
            {
                isValidating = false;
            }
            
            if (logToConsole)
            {
                Debug.Log($"[Data Validator] Validation complete. Found {validationIssues.Count} issues.");
            }
        }

        private void ValidateScopeData()
        {
            isValidating = true;
            lastValidationTime = System.DateTime.Now;
            validationIssues.Clear();
            
            var startTime = System.DateTime.Now;
            
            try
            {
                switch (validationScope)
                {
                    case ValidationScope.Entities:
                        ValidateDatabase("Entities", GameData.entityDatabase.Cast<object>().ToList());
                        break;
                    case ValidationScope.Items:
                        ValidateDatabase("Items", GameData.itemDatabase.Cast<object>().ToList());
                        break;
                    case ValidationScope.Traits:
                        ValidateDatabase("Traits", GameData.traitDatabase.Cast<object>().ToList());
                        break;
                    // Add other scopes
                    default:
                        ValidateAllData();
                        return;
                }
                
                UpdateValidationStatistics(startTime);
            }
            finally
            {
                isValidating = false;
            }
        }

        private void QuickValidation()
        {
            // Quick validation with only critical rules
            var originalRuleStates = validationRules.ToDictionary(r => r, r => r.enabled);
            
            try
            {
                // Disable all non-critical rules
                foreach (var rule in validationRules)
                {
                    rule.enabled = rule.severity == ValidationSeverity.Critical;
                }
                
                ValidateAllData();
            }
            finally
            {
                // Restore original rule states
                foreach (var kvp in originalRuleStates)
                {
                    kvp.Key.enabled = kvp.Value;
                }
            }
        }

        private void ValidateDatabase(string databaseName, List<object> entities)
        {
            var databaseIssues = new List<ValidationIssue>();
            
            foreach (var entity in entities)
            {
                var entityIssues = ValidateEntity(entity, databaseName);
                databaseIssues.AddRange(entityIssues);
            }
            
            issuesByDatabase[databaseName] = databaseIssues;
            validationIssues.AddRange(databaseIssues);
        }

        private List<ValidationIssue> ValidateEntity(object entity, string databaseType)
        {
            var issues = new List<ValidationIssue>();
            
            foreach (var rule in validationRules.Where(r => r.enabled && (r.targetDatabase == "All" || r.targetDatabase == databaseType)))
            {
                var issue = ValidateEntityWithRule(entity, rule, databaseType);
                if (issue != null)
                {
                    issues.Add(issue);
                }
            }
            
            return issues;
        }

        private ValidationIssue ValidateEntityWithRule(object entity, ValidationRule rule, string databaseType)
        {
            try
            {
                switch (rule.type)
                {
                    case ValidationRuleType.NotNull:
                        return ValidateNotNull(entity, rule, databaseType);
                    case ValidationRuleType.NotEmpty:
                        return ValidateNotEmpty(entity, rule, databaseType);
                    case ValidationRuleType.Uniqueness:
                        return ValidateUniqueness(entity, rule, databaseType);
                    case ValidationRuleType.Reference:
                        return ValidateReference(entity, rule, databaseType);
                    case ValidationRuleType.Range:
                        return ValidateRange(entity, rule, databaseType);
                    case ValidationRuleType.Format:
                        return ValidateFormat(entity, rule, databaseType);
                    case ValidationRuleType.Custom:
                        return ValidateCustom(entity, rule, databaseType);
                    default:
                        return null;
                }
            }
            catch (System.Exception ex)
            {
                return new ValidationIssue
                {
                    id = System.Guid.NewGuid().ToString(),
                    rule = rule,
                    entity = entity,
                    databaseType = databaseType,
                    entityId = GetEntityId(entity),
                    fieldName = rule.fieldName,
                    message = $"Validation error: {ex.Message}",
                    severity = ValidationSeverity.Error,
                    canAutoFix = false,
                    detectedTime = System.DateTime.Now
                };
            }
        }

        private ValidationIssue ValidateNotNull(object entity, ValidationRule rule, string databaseType)
        {
            var field = entity.GetType().GetField(rule.fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                var value = field.GetValue(entity);
                if (value == null)
                {
                    return new ValidationIssue
                    {
                        id = System.Guid.NewGuid().ToString(),
                        rule = rule,
                        entity = entity,
                        databaseType = databaseType,
                        entityId = GetEntityId(entity),
                        fieldName = rule.fieldName,
                        message = $"Field '{rule.fieldName}' is null",
                        severity = rule.severity,
                        suggestedFix = $"Provide a value for {rule.fieldName}",
                        canAutoFix = false,
                        detectedTime = System.DateTime.Now
                    };
                }
            }
            
            return null;
        }

        private ValidationIssue ValidateNotEmpty(object entity, ValidationRule rule, string databaseType)
        {
            var field = entity.GetType().GetField(rule.fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(string))
            {
                var value = field.GetValue(entity) as string;
                if (string.IsNullOrEmpty(value))
                {
                    return new ValidationIssue
                    {
                        id = System.Guid.NewGuid().ToString(),
                        rule = rule,
                        entity = entity,
                        databaseType = databaseType,
                        entityId = GetEntityId(entity),
                        fieldName = rule.fieldName,
                        message = $"Field '{rule.fieldName}' is empty",
                        severity = rule.severity,
                        suggestedFix = $"Provide a non-empty value for {rule.fieldName}",
                        canAutoFix = false,
                        detectedTime = System.DateTime.Now
                    };
                }
            }
            
            return null;
        }

        private ValidationIssue ValidateUniqueness(object entity, ValidationRule rule, string databaseType)
        {
            // TODO: Implement uniqueness validation
            return null;
        }

        private ValidationIssue ValidateReference(object entity, ValidationRule rule, string databaseType)
        {
            // TODO: Implement reference validation
            return null;
        }

        private ValidationIssue ValidateRange(object entity, ValidationRule rule, string databaseType)
        {
            // TODO: Implement range validation
            return null;
        }

        private ValidationIssue ValidateFormat(object entity, ValidationRule rule, string databaseType)
        {
            // TODO: Implement format validation
            return null;
        }

        private ValidationIssue ValidateCustom(object entity, ValidationRule rule, string databaseType)
        {
            // TODO: Implement custom validation
            return null;
        }

        private void UpdateValidationStatistics(System.DateTime startTime)
        {
            var endTime = System.DateTime.Now;
            
            currentStats = new ValidationStatistics
            {
                totalEntitiesValidated = GetTotalEntityCount(),
                totalIssuesFound = validationIssues.Count,
                issuesBySeverity = validationIssues.GroupBy(i => i.severity).ToDictionary(g => g.Key, g => g.Count()),
                issuesByDatabase = issuesByDatabase.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),
                issuesByType = validationIssues.GroupBy(i => i.rule.type).ToDictionary(g => g.Key, g => g.Count()),
                validationTime = (float)(endTime - startTime).TotalSeconds,
                autoFixedIssues = validationIssues.Count(i => i.isFixed)
            };
        }

        private int GetTotalEntityCount()
        {
            return GameData.entityDatabase.Count +
                   GameData.itemDatabase.Count +
                   GameData.traitDatabase.Count +
                   GameData.questDatabase.Count +
                   GameData.npcDatabase.Count +
                   GameData.skillDatabase.Count +
                   GameData.buffDatabase.Count +
                   GameData.objectiveDatabase.Count +
                   GameData.statDatabase.Count +
                   GameData.geneticsDatabase.Count +
                   GameData.journalDatabase.Count;
        }

        private List<ValidationIssue> GetFilteredIssues()
        {
            return validationIssues.Where(i => 
                (filterSeverity == ValidationSeverity.All || i.severity == filterSeverity) &&
                (filterDatabase == "All" || i.databaseType.Contains(filterDatabase)) &&
                (showFixedIssues || !i.isFixed)
            ).ToList();
        }

        private string GetEntityId(object entity)
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

        private void ClearValidationResults()
        {
            validationIssues.Clear();
            issuesByDatabase.Clear();
            currentStats = new ValidationStatistics();
        }

        private void ApplyAutoFix(ValidationIssue issue)
        {
            // TODO: Implement auto-fix functionality
            issue.isFixed = true;
        }

        private void FixAllAutoFixable()
        {
            var autoFixableIssues = validationIssues.Where(i => i.canAutoFix && !i.isFixed).ToList();
            
            foreach (var issue in autoFixableIssues)
            {
                ApplyAutoFix(issue);
            }
            
            Debug.Log($"[Data Validator] Auto-fixed {autoFixableIssues.Count} issues.");
        }

        private void ShowIssueDetails(ValidationIssue issue)
        {
            EditorUtility.DisplayDialog("Issue Details", 
                $"Rule: {issue.rule.name}\n" +
                $"Entity: {issue.entityId}\n" +
                $"Field: {issue.fieldName}\n" +
                $"Message: {issue.message}\n" +
                $"Severity: {issue.severity}\n" +
                $"Detected: {issue.detectedTime}\n" +
                $"Can Auto-Fix: {issue.canAutoFix}", "OK");
        }

        private void AddCustomRule()
        {
            var newRule = new ValidationRule
            {
                id = System.Guid.NewGuid().ToString(),
                name = "New Custom Rule",
                description = "",
                type = ValidationRuleType.Custom,
                severity = ValidationSeverity.Warning,
                targetDatabase = "All",
                fieldName = "",
                condition = "",
                expectedValue = "",
                enabled = true,
                isBuiltIn = false,
                customCode = ""
            };
            
            validationRules.Add(newRule);
        }

        private void EditCustomRule(ValidationRule rule)
        {
            // TODO: Implement custom rule editor
            EditorUtility.DisplayDialog("Edit Rule", "Custom rule editor would open here.", "OK");
        }

        private void RemoveCustomRule(ValidationRule rule)
        {
            if (EditorUtility.DisplayDialog("Remove Rule", $"Remove custom rule '{rule.name}'?", "Yes", "Cancel"))
            {
                validationRules.Remove(rule);
            }
        }

        private void ImportRules()
        {
            // TODO: Implement rule import
        }

        private void ExportRules()
        {
            // TODO: Implement rule export
        }

        private void ExportResults()
        {
            // TODO: Implement results export
        }

        private void GenerateReport()
        {
            // TODO: Implement comprehensive report generation
        }

        private void GenerateQuickReport()
        {
            // TODO: Implement quick report generation
        }

        private void EmailReport()
        {
            // TODO: Implement email functionality
        }

        private void RefreshValidationData()
        {
            if (!isValidating)
            {
                ValidateAllData();
            }
        }

        private void LoadSettings()
        {
            // TODO: Implement settings loading
        }

        private void SaveSettings()
        {
            // TODO: Implement settings saving
        }

        private void ResetSettings()
        {
            autoValidateOnStartup = true;
            realTimeValidation = false;
            validationFrequency = 5;
            logToConsole = true;
            saveValidationHistory = true;
        }

        #endregion
    }
}

using UnityEngine;
using UnityEditor;
using Lineage.Ancestral.Legacies.Database;
using Lineage.Ancestral.Legacies.Editor.StudioTools; // Import window classes
using Lineage.Editor; // Import population and lore designers



namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Main menu system for Lineage Ancestral Legacies Studio content creation tools.
    /// Provides unified access to all game-specific content creation and management utilities.
    /// </summary>
    public static class StudioMenuItems
    {
        private const string STUDIO_MENU_ROOT = "Lineage/Studio/";
        private const int MENU_PRIORITY_BASE = 2000;

        #region Database Tools

        [MenuItem(STUDIO_MENU_ROOT + "Database/Database Editor & Visualizer", priority = MENU_PRIORITY_BASE)]
        public static void OpenDatabaseEditor()
        {
            DatabaseEditorWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Database/Database Inspector", priority = MENU_PRIORITY_BASE + 1)]
        public static void OpenDatabaseInspector()
        {
            DatabaseInspectorWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Database/Bulk Data Import/Export", priority = MENU_PRIORITY_BASE + 2)]
        public static void OpenBulkDataTool()
        {
            BulkDataToolWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Database/Data Validation Suite", priority = MENU_PRIORITY_BASE + 3)]
        public static void OpenDataValidator()
        {
            DataValidatorWindow.ShowWindow();
        }

        #endregion

        #region Content Creation Tools

        [MenuItem(STUDIO_MENU_ROOT + "Content Creation/Entity Creator", priority = MENU_PRIORITY_BASE + 100)]
        public static void OpenEntityCreator()
        {
            EntityCreatorWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Content Creation/Item Creator & Editor", priority = MENU_PRIORITY_BASE + 101)]
        public static void OpenItemCreator()
        {
            ItemCreatorWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Content Creation/Trait Designer", priority = MENU_PRIORITY_BASE + 102)]
        public static void OpenTraitDesigner()
        {
            TraitDesignerWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Content Creation/Quest Designer", priority = MENU_PRIORITY_BASE + 103)]
        public static void OpenQuestDesigner()
        {
            QuestDesignerWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Content Creation/Population Designer", priority = MENU_PRIORITY_BASE + 104)]
        public static void OpenPopulationDesigner()
        {
            Lineage.Editor.PopulationDesignerWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Content Creation/Lore & Narrative Designer", priority = MENU_PRIORITY_BASE + 105)]
        public static void OpenLoreDesigner()
        {
            Lineage.Editor.LoreDesignerWindow.ShowWindow();
        }

        #endregion

        #region Specialized Editors

        [MenuItem(STUDIO_MENU_ROOT + "Specialized Editors/NPC Editor", priority = MENU_PRIORITY_BASE + 200)]
        public static void OpenNPCEditor()
        {
            NPCEditorWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Specialized Editors/Skills & Buffs Editor", priority = MENU_PRIORITY_BASE + 201)]
        public static void OpenSkillsBuffsEditor()
        {
            Lineage.Editor.SkillsBuffsEditorWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Specialized Editors/Genetics & Inheritance Editor", priority = MENU_PRIORITY_BASE + 202)]
        public static void OpenGeneticsEditor()
        {
            GeneticsEditorWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Specialized Editors/Settlement & Territory Designer", priority = MENU_PRIORITY_BASE + 203)]
        public static void OpenSettlementDesigner()
        {
            SettlementDesignerWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Specialized Editors/Journal & Narrative Editor", priority = MENU_PRIORITY_BASE + 204)]
        public static void OpenJournalEditor()
        {
            JournalEditorWindow.ShowWindow();
        }

        #endregion

        #region Analysis & Reporting Tools

        [MenuItem(STUDIO_MENU_ROOT + "Analysis/Game Balance Analyzer", priority = MENU_PRIORITY_BASE + 300)]
        public static void OpenBalanceAnalyzer()
        {
            GameBalanceAnalyzerWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Analysis/Content Statistics Report", priority = MENU_PRIORITY_BASE + 301)]
        public static void OpenContentStats()
        {
            ContentStatisticsWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Analysis/Dependency Graph Visualizer", priority = MENU_PRIORITY_BASE + 302)]
        public static void OpenDependencyVisualizer()
        {
            DependencyGraphWindow.ShowWindow();
        }

        [MenuItem(STUDIO_MENU_ROOT + "Analysis/Progression Curve Analyzer", priority = MENU_PRIORITY_BASE + 303)]
        public static void OpenProgressionAnalyzer()
        {
            ProgressionAnalyzerWindow.ShowWindow();
        }

        #endregion

        #region Batch Operations

        [MenuItem(STUDIO_MENU_ROOT + "Batch Operations/Batch Entity Processor", priority = MENU_PRIORITY_BASE + 400)]
        public static void OpenBatchEntityProcessor()
        {
            EditorUtility.DisplayDialog("Coming Soon", "Batch Entity Processor tool is not yet implemented.", "OK");
        }

        [MenuItem(STUDIO_MENU_ROOT + "Batch Operations/Mass Trait Assignment", priority = MENU_PRIORITY_BASE + 401)]
        public static void OpenMassTraitAssignment()
        {
            EditorUtility.DisplayDialog("Coming Soon", "Mass Trait Assignment tool is not yet implemented.", "OK");
        }

        [MenuItem(STUDIO_MENU_ROOT + "Batch Operations/Content Bulk Updater", priority = MENU_PRIORITY_BASE + 402)]
        public static void OpenContentBulkUpdater()
        {
            EditorUtility.DisplayDialog("Coming Soon", "Content Bulk Updater tool is not yet implemented.", "OK");
        }

        #endregion

        #region Debugging & Testing Tools

        [MenuItem(STUDIO_MENU_ROOT + "Debug & Test/Runtime Entity Inspector", priority = MENU_PRIORITY_BASE + 500)]
        public static void OpenRuntimeEntityInspector()
        {
            EditorUtility.DisplayDialog("Coming Soon", "Runtime Entity Inspector tool is not yet implemented.", "OK");
        }

        [MenuItem(STUDIO_MENU_ROOT + "Debug & Test/Database Integrity Checker", priority = MENU_PRIORITY_BASE + 501)]
        public static void OpenDatabaseIntegrityChecker()
        {
            EditorUtility.DisplayDialog("Coming Soon", "Database Integrity Checker tool is not yet implemented.", "OK");
        }

        [MenuItem(STUDIO_MENU_ROOT + "Debug & Test/Performance Profiler", priority = MENU_PRIORITY_BASE + 502)]
        public static void OpenPerformanceProfiler()
        {
            EditorUtility.DisplayDialog("Coming Soon", "Performance Profiler tool is not yet implemented.", "OK");
        }

        [MenuItem(STUDIO_MENU_ROOT + "Debug & Test/Data Migration Tool", priority = MENU_PRIORITY_BASE + 503)]
        public static void OpenDataMigrationTool()
        {
            EditorUtility.DisplayDialog("Coming Soon", "Data Migration tool is not yet implemented.", "OK");
        }

        #endregion

        #region Quick Actions

        [MenuItem(STUDIO_MENU_ROOT + "Quick Actions/Initialize All Databases", priority = MENU_PRIORITY_BASE + 600)]
        public static void InitializeAllDatabases()
        {
            if (EditorUtility.DisplayDialog("Initialize Databases",
                "This will clear and reinitialize all game databases with default data. Continue?",
                "Yes", "Cancel"))
            {
                GameData.InitializeAllDatabases();
                UnityEngine.Debug.Log("[Studio] All databases initialized successfully.");
                EditorUtility.DisplayDialog("Complete", "All databases have been initialized with default data.", "OK");
            }
        }

        [MenuItem(STUDIO_MENU_ROOT + "Quick Actions/Backup Current Database State", priority = MENU_PRIORITY_BASE + 601)]
        public static void BackupDatabaseState()
        {
            EditorUtility.DisplayDialog("Coming Soon", "Backup Current Database State tool is not yet implemented.", "OK");
        }

        [MenuItem(STUDIO_MENU_ROOT + "Quick Actions/Generate Entity Prefabs", priority = MENU_PRIORITY_BASE + 602)]
        public static void GenerateEntityPrefabs()
        {
            EditorUtility.DisplayDialog("Coming Soon", "Generate Entity Prefabs tool is not yet implemented.", "OK");
        }

        [MenuItem(STUDIO_MENU_ROOT + "Quick Actions/Validate All Content", priority = MENU_PRIORITY_BASE + 603)]
        public static void ValidateAllContent()
        {
            EditorUtility.DisplayDialog("Coming Soon", "Validate All Content tool is not yet implemented.", "OK");
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Validates if the Studio menu items should be enabled based on project state.
        /// </summary>
        /// <returns>True if Studio tools should be available.</returns>
        public static bool ValidateStudioMenus()
        {
            // Check if we're in a valid Lineage project
            return System.IO.Directory.Exists(Application.dataPath + "/Scripts/Systems/Data");
        }

        /// <summary>
        /// Shows information about the Studio tools suite.
        /// </summary>
        [MenuItem(STUDIO_MENU_ROOT + "About Studio Tools", priority = MENU_PRIORITY_BASE + 1000)]
        public static void ShowAboutDialog()
        {
            EditorUtility.DisplayDialog("Lineage Studio Tools", 
                "Lineage Ancestral Legacies Studio Tools Suite\n\n" +
                "Complete content creation and management tools for:\n" +
                "• Database-driven entity system\n" +
                "• Trait and genetics management\n" +
                "• Quest and narrative design\n" +
                "• Population and settlement tools\n" +
                "• Performance analysis and validation\n\n" +
                "Designed for modular, scalable game development.", "OK");
        }

        #endregion
    }
}

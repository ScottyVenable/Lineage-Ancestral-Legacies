using UnityEngine;
using UnityEditor;

namespace Lineage.Core.Editor.StudioTools
{
    /// <summary>
    /// Common base class for StudioTools editor windows. Provides
    /// shared styles, helper utilities and standard undo handling.
    /// </summary>
    public class BaseStudioEditorWindow : EditorWindow
    {
        protected GUIStyle HeaderStyle { get; private set; }
        protected GUIStyle SubHeaderStyle { get; private set; }
        protected GUIStyle ErrorStyle { get; private set; }
        private bool stylesInitialized;

        protected virtual void OnEnable()
        {
            InitializeStyles();
        }

        /// <summary>
        /// Initialize basic GUI styles used across tools.
        /// </summary>
        protected virtual void InitializeStyles()
        {
            if (stylesInitialized) return;

            HeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14
            };

            SubHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12
            };

            ErrorStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.red },
                fontStyle = FontStyle.Bold
            };

            stylesInitialized = true;
        }

        /// <summary>
        /// Records an undo step and marks the target object as dirty.
        /// </summary>
        protected void MarkDirty(Object target, string description = "StudioTool Change")
        {
            if (target == null) return;
            Undo.RecordObject(target, description);
            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// Convenience method for drawing status/info messages.
        /// </summary>
        protected void DrawMessage(string message, MessageType type = MessageType.Info)
        {
            EditorGUILayout.HelpBox(message, type);
        }
    }
}

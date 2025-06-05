using UnityEditor;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.Editor.StudioTools.Core
{
    /// <summary>
    /// Base class for all Studio tool windows providing shared styles and helpers.
    /// </summary>
    public class BaseStudioEditorWindow : EditorWindow
    {
        protected GUIStyle headerStyle;
        protected GUIStyle subHeaderStyle;
        protected GUIStyle errorStyle;
        private bool stylesInitialized;

        protected string statusMessage;
        protected MessageType statusType = MessageType.Info;

        /// <summary>
        /// Initialize common GUI styles if not already created.
        /// </summary>
        protected virtual void InitializeStyles()
        {
            if (stylesInitialized)
                return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                normal = { textColor = Color.white }
            };

            subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
            };

            errorStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.red },
                fontStyle = FontStyle.Bold
            };

            stylesInitialized = true;
        }

        /// <summary>
        /// Display a status message in the window.
        /// </summary>
        protected void ShowStatus(string message, MessageType type = MessageType.Info)
        {
            statusMessage = message;
            statusType = type;
            Repaint();
        }

        /// <summary>
        /// Draw the status bar if a message is set.
        /// </summary>
        protected void DrawStatusBar()
        {
            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.HelpBox(statusMessage, statusType);
            }
        }

        /// <summary>
        /// Helper for recording undo operations on an object.
        /// </summary>
        protected void RecordUndo(Object obj, string actionName)
        {
            if (obj != null)
            {
                Undo.RecordObject(obj, actionName);
                EditorUtility.SetDirty(obj);
            }
        }
    }
}

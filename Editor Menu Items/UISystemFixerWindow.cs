using UnityEngine;
using UnityEditor;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Editor
{
    /// <summary>
    /// Editor window to fix UI system issues
    /// </summary>
    public class UISystemFixerWindow : EditorWindow
    {
        [MenuItem("Lineage/Debug/Fix UI System")]
        public static void ShowWindow()
        {
            GetWindow<UISystemFixerWindow>("UI System Fixer");
        }
        
        [MenuItem("Lineage/Debug/Quick Fix UI Issues")]
        public static void QuickFix()
        {
            // Create temporary fixer object
            GameObject tempGO = new GameObject("TempUIFixer");
            Debug.UISystemFixer fixer = tempGO.AddComponent<Debug.UISystemFixer>();
            
            fixer.FixUISystem();
            
            DestroyImmediate(tempGO);
            
            Log.Info("[UI System Fixer] Quick fix completed! Check console for details.", Log.LogCategory.Systems);
        }
        
        private void OnGUI()
        {
            GUILayout.Label("UI System Fixer", EditorStyles.boldLabel);
            GUILayout.Space(10);
              GUILayout.Label("This tool fixes common UI system issues, particularly:");
            GUILayout.Label("• MissingReferenceException with TextMeshProUGUI");
            GUILayout.Label("• Missing EventSystem or Input Modules");
            GUILayout.Label("• Legacy Input System vs New Input System");
            GUILayout.Label("• Destroyed UI component references");
            GUILayout.Label("• Canvas configuration issues");
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Run Full UI System Fix", GUILayout.Height(30)))
            {
                QuickFix();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Migrate to New Input System"))
            {
                GameObject tempGO = new GameObject("TempUIFixer");
                Debug.UISystemFixer fixer = tempGO.AddComponent<Debug.UISystemFixer>();
                fixer.MigrateToNewInputSystem();
                DestroyImmediate(tempGO);
                Log.Info("[UI System Fixer] Input System migration completed!", Log.LogCategory.Systems);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Force Refresh All UI"))
            {
                Canvas.ForceUpdateCanvases();
                Log.Info("[UI System Fixer] UI refresh completed!", Log.LogCategory.Systems);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Remove Null Components"))
            {
                GameObject tempGO = new GameObject("TempUIFixer");
                Debug.UISystemFixer fixer = tempGO.AddComponent<Debug.UISystemFixer>();
                fixer.RemoveNullComponents();
                DestroyImmediate(tempGO);
            }
            
            GUILayout.Space(20);
              EditorGUILayout.HelpBox(
                "If buttons are still not working after running the fix:\n" +
                "1. Check that your scene has an active EventSystem\n" +
                "2. Verify Canvas has GraphicRaycaster component\n" +
                "3. Make sure buttons have valid targetGraphic assigned\n" +
                "4. Check that no CanvasGroup is blocking raycasts\n" +
                "5. Ensure using InputSystemUIInputModule for New Input System",
                MessageType.Info
            );
        }
    }
}

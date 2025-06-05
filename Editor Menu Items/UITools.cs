using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Lineage.Editor
{
    /// <summary>
    /// Extended UI development and debugging tools
    /// </summary>
    public static class UITools
    {
        [MenuItem("Lineage/UI Tools/Fix UI System Issues", false, 400)]
        public static void FixUISystemIssues()
        {
            // Create temporary fixer object
            GameObject tempGO = new GameObject("TempUIFixer");
            var fixer = tempGO.AddComponent<Lineage.Debug.UISystemFixer>();
            
            fixer.FixUISystem();
            
            DestroyImmediate(tempGO);
            Debug.Log("[UI Tools] UI System fix completed!");
        }
        
        [MenuItem("Lineage/UI Tools/Auto-Assign Button References", false, 420)]
        public static void AutoAssignButtonReferences()
        {
            Button[] buttons = Object.FindObjectsOfType<Button>();
            int assignedCount = 0;
            
            foreach (Button button in buttons)
            {
                if (button.targetGraphic == null)
                {
                    Image image = button.GetComponent<Image>();
                    if (image != null)
                    {
                        button.targetGraphic = image;
                        assignedCount++;
                        EditorUtility.SetDirty(button);
                    }
                }
            }
            
            Debug.Log($"[UI Tools] Auto-assigned targetGraphic to {assignedCount} buttons");
        }
        
        [MenuItem("Lineage/UI Tools/Find UI Performance Issues", false, 440)]
        public static void FindUIPerformanceIssues()
        {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
            
            Debug.Log("=== UI Performance Analysis ===");
            
            foreach (Canvas canvas in canvases)
            {
                // Check for multiple canvases
                if (canvases.Length > 3)
                {
                    Debug.LogWarning($"[UI Performance] {canvases.Length} canvases found - consider consolidating");
                }
                
                // Check for missing GraphicRaycaster
                if (canvas.GetComponent<GraphicRaycaster>() == null && canvas.renderMode != RenderMode.WorldSpace)
                {
                    Debug.LogWarning($"[UI Performance] Canvas '{canvas.name}' missing GraphicRaycaster", canvas);
                }
                
                // Check for expensive UI elements
                Text[] texts = canvas.GetComponentsInChildren<Text>();
                if (texts.Length > 0)
                {
                    Debug.LogWarning($"[UI Performance] Canvas '{canvas.name}' using legacy Text components ({texts.Length} found) - consider TextMeshPro", canvas);
                }
                
                // Check for many UI elements
                Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>();
                if (graphics.Length > 100)
                {
                    Debug.LogWarning($"[UI Performance] Canvas '{canvas.name}' has {graphics.Length} UI elements - consider splitting", canvas);
                }
            }
        }
        
        [MenuItem("Lineage/UI Tools/Convert Text to TextMeshPro", false, 460)]
        public static void ConvertTextToTMP()
        {
            if (!EditorUtility.DisplayDialog("Convert to TextMeshPro", 
                "This will convert all legacy Text components to TextMeshPro. Continue?", "Yes", "Cancel"))
                return;
            
            Text[] legacyTexts = Object.FindObjectsOfType<Text>();
            int convertedCount = 0;
            
            foreach (Text text in legacyTexts)
            {
                GameObject textObject = text.gameObject;
                string textContent = text.text;
                Font font = text.font;
                int fontSize = text.fontSize;
                Color color = text.color;
                TextAnchor alignment = text.alignment;
                
                DestroyImmediate(text);
                
                TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
                tmpText.text = textContent;
                tmpText.fontSize = fontSize;
                tmpText.color = color;
                
                // Convert alignment
                switch (alignment)
                {
                    case TextAnchor.UpperLeft: tmpText.alignment = TextAlignmentOptions.TopLeft; break;
                    case TextAnchor.UpperCenter: tmpText.alignment = TextAlignmentOptions.Top; break;
                    case TextAnchor.UpperRight: tmpText.alignment = TextAlignmentOptions.TopRight; break;
                    case TextAnchor.MiddleLeft: tmpText.alignment = TextAlignmentOptions.Left; break;
                    case TextAnchor.MiddleCenter: tmpText.alignment = TextAlignmentOptions.Center; break;
                    case TextAnchor.MiddleRight: tmpText.alignment = TextAlignmentOptions.Right; break;
                    case TextAnchor.LowerLeft: tmpText.alignment = TextAlignmentOptions.BottomLeft; break;
                    case TextAnchor.LowerCenter: tmpText.alignment = TextAlignmentOptions.Bottom; break;
                    case TextAnchor.LowerRight: tmpText.alignment = TextAlignmentOptions.BottomRight; break;
                }
                
                convertedCount++;
                EditorUtility.SetDirty(textObject);
            }
            
            Debug.Log($"[UI Tools] Converted {convertedCount} Text components to TextMeshPro");
        }
        
        [MenuItem("Lineage/UI Tools/Optimize UI Anchors", false, 480)]
        public static void OptimizeUIAnchors()
        {
            RectTransform[] rectTransforms = Object.FindObjectsOfType<RectTransform>();
            int optimizedCount = 0;
            
            foreach (RectTransform rect in rectTransforms)
            {
                // Auto-set anchors based on position
                Canvas parentCanvas = rect.GetComponentInParent<Canvas>();
                if (parentCanvas == null) continue;
                
                RectTransform parentRect = rect.parent as RectTransform;
                if (parentRect == null) continue;
                
                Vector2 anchorMin = rect.anchorMin;
                Vector2 anchorMax = rect.anchorMax;
                
                // If anchors are default (0.5, 0.5), try to optimize
                if (Vector2.Distance(anchorMin, Vector2.one * 0.5f) < 0.01f && 
                    Vector2.Distance(anchorMax, Vector2.one * 0.5f) < 0.01f)
                {
                    Rect parentSize = parentRect.rect;
                    Vector2 normalizedPos = new Vector2(
                        (rect.anchoredPosition.x + parentSize.width * 0.5f) / parentSize.width,
                        (rect.anchoredPosition.y + parentSize.height * 0.5f) / parentSize.height
                    );
                    
                    rect.anchorMin = normalizedPos;
                    rect.anchorMax = normalizedPos;
                    optimizedCount++;
                    EditorUtility.SetDirty(rect);
                }
            }
            
            Debug.Log($"[UI Tools] Optimized anchors for {optimizedCount} UI elements");
        }
        
        [MenuItem("Lineage/UI Tools/Generate UI Report", false, 500)]
        public static void GenerateUIReport()
        {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
            Button[] buttons = Object.FindObjectsOfType<Button>();
            Image[] images = Object.FindObjectsOfType<Image>();
            Text[] legacyTexts = Object.FindObjectsOfType<Text>();
            TextMeshProUGUI[] tmpTexts = Object.FindObjectsOfType<TextMeshProUGUI>();
            
            Debug.Log("=== UI System Report ===");
            Debug.Log($"Canvases: {canvases.Length}");
            Debug.Log($"Buttons: {buttons.Length}");
            Debug.Log($"Images: {images.Length}");
            Debug.Log($"Legacy Text: {legacyTexts.Length}");
            Debug.Log($"TextMeshPro: {tmpTexts.Length}");
            
            if (legacyTexts.Length > 0)
            {
                Debug.LogWarning($"Consider converting {legacyTexts.Length} legacy Text components to TextMeshPro");
            }
            
            // Check for buttons without target graphics
            int buttonsWithoutTarget = buttons.Count(b => b.targetGraphic == null);
            if (buttonsWithoutTarget > 0)
            {
                Debug.LogWarning($"{buttonsWithoutTarget} buttons missing targetGraphic");
            }
        }
    }
}

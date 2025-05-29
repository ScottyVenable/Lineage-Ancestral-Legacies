using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lineage.Ancestral.Legacies.UI
{
    /// <summary>
    /// Handles the visual selection box UI for drag selection.
    /// </summary>
    public class SelectionBoxUI : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Image selectionImage;
        
        [SerializeField] private Color boxColor = new Color(0.2f, 0.6f, 1f, 0.3f);
        [SerializeField] private Color borderColor = new Color(0.2f, 0.6f, 1f, 0.8f);
        [SerializeField] private float borderThickness = 2f;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            
            // Add image component if not exists
            selectionImage = GetComponent<Image>();
            if (selectionImage == null)
            {
                selectionImage = gameObject.AddComponent<Image>();
                selectionImage.color = boxColor;
            }
            
            // Add border (optional)
            CreateBorder();
            
            // Initially hide
            gameObject.SetActive(false);
        }
        
        private void CreateBorder()
        {
            // Create border as child objects on all four sides
            CreateBorderEdge("TopBorder", new Vector2(0, 1), Vector2.one, new Vector2(1, 0));
            CreateBorderEdge("BottomBorder", new Vector2(0, 0), Vector2.right, new Vector2(1, 0));
            CreateBorderEdge("LeftBorder", new Vector2(0, 0), Vector2.up, new Vector2(0, 1));
            CreateBorderEdge("RightBorder", new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 1));
        }
        
        private void CreateBorderEdge(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            GameObject edge = new GameObject(name);
            edge.transform.SetParent(transform, false);
            
            RectTransform rt = edge.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            
            // Set size
            if (name == "TopBorder" || name == "BottomBorder")
            {
                rt.sizeDelta = new Vector2(0, borderThickness);
            }
            else
            {
                rt.sizeDelta = new Vector2(borderThickness, 0);
            }
            
            // Add image
            Image img = edge.AddComponent<Image>();
            img.color = borderColor;
        }
    }
}

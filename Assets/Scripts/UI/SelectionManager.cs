using UnityEngine;
using System.Collections.Generic;
using Lineage.Ancestral.Legacies.Entities;

namespace Lineage.Ancestral.Legacies.UI
{
    public class SelectionManager : MonoBehaviour
    {
        public static SelectionManager Instance { get; private set; }

        [SerializeField] private Material selectionBoxMaterial;
        [SerializeField] private LayerMask selectableLayer;

        private Vector3 startPos;
        private bool isDragging = false;
        private List<Pop> selectedPops = new List<Pop>();

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            HandleInput();
            
            if (isDragging)
            {
                DrawSelectionBox();
            }
        }

        private void HandleInput()
        {
            // Start selection
            if (Input.GetMouseButtonDown(0))
            {
                startPos = Input.mousePosition;
                isDragging = true;
                
                // Clear previous selection on new click
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                {
                    ClearSelection();
                }
            }
            
            // End selection
            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
                SelectObjectsInBox();
            }
        }

        private void DrawSelectionBox()
        {
            // This needs to be called during OnGUI but we'll add rendering with GL here
            if (Event.current.type == EventType.Repaint)
            {
                Vector3 currentPos = Input.mousePosition;
                
                // Set up GL for drawing
                GL.PushMatrix();
                selectionBoxMaterial.SetPass(0);
                GL.LoadOrtho();
                
                // Convert screen positions to GL coordinates (0,0 is bottom left in GL)
                float screenHeight = Screen.height;
                float startX = startPos.x / Screen.width;
                float startY = 1.0f - (startPos.y / screenHeight);
                float endX = currentPos.x / Screen.width;
                float endY = 1.0f - (currentPos.y / screenHeight);
                
                // Draw the selection box
                GL.Begin(GL.QUADS);
                GL.Color(new Color(0.5f, 0.5f, 1f, 0.3f)); // Light blue with transparency
                GL.Vertex3(startX, startY, 0);
                GL.Vertex3(endX, startY, 0);
                GL.Vertex3(endX, endY, 0);
                GL.Vertex3(startX, endY, 0);
                GL.End();
                
                // Draw the border
                GL.Begin(GL.LINES);
                GL.Color(new Color(0.5f, 0.5f, 1f, 1f)); // Solid light blue
                GL.Vertex3(startX, startY, 0);
                GL.Vertex3(endX, startY, 0);
                
                GL.Vertex3(endX, startY, 0);
                GL.Vertex3(endX, endY, 0);
                
                GL.Vertex3(endX, endY, 0);
                GL.Vertex3(startX, endY, 0);
                
                GL.Vertex3(startX, endY, 0);
                GL.Vertex3(startX, startY, 0);
                GL.End();
                
                GL.PopMatrix();
            }
        }
        
        private void OnGUI()
        {
            if (isDragging)
            {
                // Calculate the rect between start position and current mouse position
                Rect rect = GetScreenRect(startPos, Input.mousePosition);
                DrawScreenRect(rect, new Color(0.5f, 0.5f, 1f, 0.3f)); // Light blue with transparency
                DrawScreenRectBorder(rect, 2, new Color(0.5f, 0.5f, 1f)); // Solid light blue border
            }
        }

        private void SelectObjectsInBox()
        {
            Rect selectionRect = GetScreenRect(startPos, Input.mousePosition);
            
            // Find all Pops in the scene
            Pop[] allPops = FindObjectsOfType<Pop>();
            
            foreach (Pop pop in allPops)
            {
                // Convert pop's world position to screen position
                Vector3 screenPos = Camera.main.WorldToScreenPoint(pop.transform.position);
                
                // Check if inside selection rect
                if (selectionRect.Contains(new Vector2(screenPos.x, Screen.height - screenPos.y)))
                {
                    SelectPop(pop);
                }
            }
        }
        
        public void SelectPop(Pop pop)
        {
            if (!selectedPops.Contains(pop))
            {
                selectedPops.Add(pop);
                // Call selection callback on pop if needed
                pop.OnSelected();
            }
        }
        
        public void DeselectPop(Pop pop)
        {
            if (selectedPops.Contains(pop))
            {
                selectedPops.Remove(pop);
                // Call deselection callback on pop if needed
                pop.OnDeselected();
            }
        }
        
        public void ClearSelection()
        {
            foreach (Pop pop in selectedPops)
            {
                pop.OnDeselected();
            }
            selectedPops.Clear();
        }
        
        // Helper functions for drawing the selection box
        private Rect GetScreenRect(Vector3 screenPos1, Vector3 screenPos2)
        {
            // Create a rect from the points
            screenPos1.y = Screen.height - screenPos1.y;
            screenPos2.y = Screen.height - screenPos2.y;
            
            Vector3 min = Vector3.Min(screenPos1, screenPos2);
            Vector3 max = Vector3.Max(screenPos1, screenPos2);
            
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        private void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            // Draw top
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            // Draw left
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            // Draw right
            DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
            // Draw bottom
            DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        }
        
        // Get currently selected pops
        public List<Pop> GetSelectedPops()
        {
            return selectedPops;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Managers;

namespace Lineage.Ancestral.Legacies.UI
{
    /// <summary>
    /// Debug panel that shows information about selected Pops.
    /// </summary>
    public class PopSelectionDebugPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelContainer;
        [SerializeField] private TextMeshProUGUI selectionCountText;
        [SerializeField] private TextMeshProUGUI selectionInfoText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject popInfoItemPrefab;
        
        [Header("Settings")]
        [SerializeField] private bool showOnSelection = true;
        [SerializeField] private bool showDetailedInfo = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
        
        private List<GameObject> infoItems = new List<GameObject>();
        private bool isPanelVisible = false;
        
        private void Start()
        {
            // Subscribe to selection events
            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.OnSelectionChanged += OnSelectionChanged;
            }
            
            // Initialize UI
            if (panelContainer != null)
            {
                SetPanelVisible(false);
            }
            
            // Create info item prefab if not assigned
            if (popInfoItemPrefab == null)
            {
                CreateInfoItemPrefab();
            }
        }
        
        private void Update()
        {
            // Toggle panel with key
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }
            
            if (SelectionManager.Instance == null) return;
        
            List<GameObject> selectedPops = SelectionManager.Instance.GetSelectedPops();
        
            // Update selection count
            if (selectionCountText != null)
            {
                selectionCountText.text = $"Selected Units: {selectedPops.Count}";
            }
        
            // Update selection details
            if (selectionInfoText != null)
            {
                if (selectedPops.Count == 0)
                {
                    selectionInfoText.text = "No units selected";
                }
                else if (selectedPops.Count == 1)
                {
                    // Show detailed information for single selection
                    GameObject pop = selectedPops[0];
                    PopController controller = pop.GetComponent<PopController>();
                    
                    selectionInfoText.text = $"Name: {pop.name}\n" +
                        $"Position: {pop.transform.position}\n";
                        
                    // Add other component information as needed
                }
                else
                {
                    // Show summary for multi-selection
                    selectionInfoText.text = "Multiple units selected";
                }
            }
        }
        
        private void OnDestroy()
        {
            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.OnSelectionChanged -= OnSelectionChanged;
            }
        }
        
        private void OnSelectionChanged()
        {
            if (showOnSelection && SelectionManager.Instance != null)
            {
                var selectedPops = SelectionManager.Instance.GetSelectedPops();
                bool hasSelection = selectedPops.Count > 0;
                
                if (hasSelection != isPanelVisible)
                {
                    SetPanelVisible(hasSelection);
                }
                
                if (hasSelection)
                {
                    UpdateSelectionInfo(selectedPops);
                }
            }
        }
        
        private void TogglePanel()
        {
            SetPanelVisible(!isPanelVisible);
            
            if (isPanelVisible && SelectionManager.Instance != null)
            {
                UpdateSelectionInfo(SelectionManager.Instance.GetSelectedPops());
            }
        }
        
        private void SetPanelVisible(bool visible)
        {
            isPanelVisible = visible;
            if (panelContainer != null)
            {
                panelContainer.SetActive(visible);
            }
        }
        
        private void UpdateSelectionInfo(List<PopController> selectedPops)
        {
            if (selectedPops == null) return;
            
            // Update selection count
            if (selectionCountText != null)
            {
                selectionCountText.text = $"Selected: {selectedPops.Count} Pop{(selectedPops.Count != 1 ? "s" : "")}";
            }
            
            // Clear existing info items
            ClearInfoItems();
            
            if (!showDetailedInfo || contentParent == null || popInfoItemPrefab == null)
            {
                // Simple text display
                if (selectionInfoText != null)
                {
                    string info = "";
                    foreach (var popController in selectedPops)
                    {
                        var pop = popController.GetPop();
                        if (pop != null)
                        {
                            info += $"{pop.name}: {popController.GetCurrentStateName()}\n";
                            info += $"  Health: {pop.health:F1}, Hunger: {pop.hunger:F1}, Thirst: {pop.thirst:F1}\n\n";
                        }
                    }
                    selectionInfoText.text = info;
                }
                return;
            }
            
            // Detailed info items
            foreach (var popController in selectedPops)
            {
                CreateInfoItem(popController);
            }
        }
        
        private void CreateInfoItem(PopController popController)
        {
            if (popInfoItemPrefab == null || contentParent == null) return;
            
            GameObject infoItem = Instantiate(popInfoItemPrefab, contentParent);
            infoItems.Add(infoItem);
            
            // Get the PopInfoItem component (we'll create this too)
            var popInfoItem = infoItem.GetComponent<PopInfoItem>();
            if (popInfoItem == null)
            {
                popInfoItem = infoItem.AddComponent<PopInfoItem>();
            }
            
            popInfoItem.SetPopController(popController);
        }
        
        private void ClearInfoItems()
        {
            foreach (var item in infoItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            infoItems.Clear();
        }
        
        private void CreateInfoItemPrefab()
        {
            // Create a simple prefab programmatically
            GameObject prefab = new GameObject("PopInfoItem");
            
            // Add layout components
            var layoutElement = prefab.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60f;
            
            // Add background
            var background = prefab.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Add text component
            GameObject textObj = new GameObject("InfoText");
            textObj.transform.SetParent(prefab.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            var text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 12;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            
            popInfoItemPrefab = prefab;
        }
    }
    
    /// <summary>
    /// Component for individual Pop info items in the debug panel.
    /// </summary>
    public class PopInfoItem : MonoBehaviour
    {
        private PopController popController;
        private Text infoText;
        
        private void Awake()
        {
            infoText = GetComponentInChildren<Text>();
        }
        
        private void Update()
        {
            if (popController != null && infoText != null)
            {
                UpdateInfo();
            }
        }
        
        public void SetPopController(PopController controller)
        {
            popController = controller;
            UpdateInfo();
        }
        
        private void UpdateInfo()
        {
            if (popController == null || infoText == null) return;
            
            var pop = popController.GetPop();
            if (pop == null)
            {
                infoText.text = "Invalid Pop";
                return;
            }
            
            string info = $"{pop.name}\n";
            info += $"State: {popController.GetCurrentStateName()}\n";
            info += $"Health: {pop.health:F1} | Hunger: {pop.hunger:F1} | Thirst: {pop.thirst:F1}";
            
            infoText.text = info;
        }
        
        /// <summary>
        /// Called when this info item is clicked (for focus/selection).
        /// </summary>
        public void OnClick()
        {
            if (popController != null)
            {
                // Focus camera on this pop
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.FocusOnTransform(popController.transform);
                }
            }
        }
    }
}

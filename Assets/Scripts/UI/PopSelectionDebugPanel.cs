using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Lineage.Entities;
using Lineage.Managers;

namespace Lineage.UI
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

        // Input System
        private InputAction toggleAction;

        private readonly List<GameObject> infoItems = new();
        private bool isPanelVisible;

        private void Start()
        {
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

            // Setup input actions
            SetupInputActions();
        }

        private void SetupInputActions()
        {
            // Create input action for toggle key
            toggleAction = new InputAction("TogglePopSelectionPanel", InputActionType.Button);
            toggleAction.AddBinding($"<Keyboard>/{toggleKey.ToString().ToLower()}");
            toggleAction.performed += OnTogglePerformed;
            toggleAction.Enable();
        }

        private void OnTogglePerformed(InputAction.CallbackContext context)
        {
            TogglePanel();
        }

        private void OnDestroy()
        {
            // Clean up input action
            if (toggleAction != null)
            {
                toggleAction.performed -= OnTogglePerformed;
                toggleAction.Disable();
                toggleAction.Dispose();
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
                    var popControllers = new List<PopController>();
                    foreach (var popGameObject in selectedPops)
                    {
                        if (popGameObject.TryGetComponent<PopController>(out var controller))
                        {
                            popControllers.Add(controller);
                        }
                    }
                    UpdateSelectionInfo(popControllers);
                }
            }
        }

        private void TogglePanel()
        {
            SetPanelVisible(!isPanelVisible);

            if (isPanelVisible && SelectionManager.Instance != null)
            {
                var selectedPops = SelectionManager.Instance.GetSelectedPops();
                var popControllers = new List<PopController>();
                foreach (var popGameObject in selectedPops)
                {
                    if (popGameObject.TryGetComponent<PopController>(out var controller))
                    {
                        popControllers.Add(controller);
                    }
                }
                UpdateSelectionInfo(popControllers);
            }
        }

        private void SetPanelVisible(bool visible)
        {
            if (panelContainer != null)
            {
                panelContainer.SetActive(visible);
                isPanelVisible = visible;
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
                            info += $"{pop.name}\n";
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
            GameObject infoItem = Instantiate(popInfoItemPrefab, contentParent);
            infoItems.Add(infoItem);

            // Get the PopInfoItem component
            if (!infoItem.TryGetComponent<PopInfoItem>(out var popInfoItem))
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
            GameObject prefab = new("PopInfoItem");

            // Add layout components
            var layoutElement = prefab.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60f;

            var background = prefab.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Add text component
            GameObject textObj = new("InfoText");
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
        }        private void Update()
        {
            // Update less frequently to improve performance
            if (popController != null && infoText != null && Time.unscaledTime - lastUpdateTime > 0.1f)
            {
                UpdateInfo();
                lastUpdateTime = Time.unscaledTime;
            }
        }

        private float lastUpdateTime;

        public void SetPopController(PopController controller)
        {
            popController = controller;
            UpdateInfo();
        }        private void UpdateInfo()
        {
            if (popController == null || infoText == null) return;

            var pop = popController.GetPop();
            if (pop == null)
            {
                if (infoText.text != "Invalid Pop")
                    infoText.text = "Invalid Pop";
                return;
            }

            string newInfo = $"{pop.name}\n";
            newInfo += $"Health: {pop.health:F1} | Hunger: {pop.hunger:F1} | Thirst: {pop.thirst:F1}";

            // Only update text if it actually changed (prevents unnecessary UI rebuilds)
            if (infoText.text != newInfo)
                infoText.text = newInfo;
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

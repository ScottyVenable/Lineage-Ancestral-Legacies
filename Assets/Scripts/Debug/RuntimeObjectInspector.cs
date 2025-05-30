using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Lineage.Ancestral.Legacies.Debug
{
    /// <summary>
    /// Runtime Object Inspector - Allows inspection and editing of GameObject properties at runtime
    /// Click on objects in the scene to inspect them, view components, and edit simple property types
    /// </summary>
    public class RuntimeObjectInspector : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas inspectorCanvas;
        [SerializeField] private GameObject inspectorPanel;
        [SerializeField] private TextMeshProUGUI objectNameText;
        [SerializeField] private Transform componentContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private ScrollRect scrollRect;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject componentHeaderPrefab;
        [SerializeField] private GameObject propertyFieldPrefab;
        [SerializeField] private GameObject propertyLabelPrefab;
        
        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        [SerializeField] private LayerMask selectableLayers = -1;
        [SerializeField] private float raycastDistance = 100f;
        
        // State
        private GameObject selectedObject;
        private Camera playerCamera;
        private bool isInspectorActive = false;
        private List<GameObject> uiElements = new List<GameObject>();
        private Dictionary<string, object> propertyCache = new Dictionary<string, object>();
        
        // Reflection cache for performance
        private Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();
        private Dictionary<Type, PropertyInfo[]> propertyInfoCache = new Dictionary<Type, PropertyInfo[]>();
        
        private void Start()
        {
            InitializeInspector();
            SetupEventHandlers();
            
            // Find player camera if not assigned
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null)
                    playerCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        private void InitializeInspector()
        {
            // Create inspector UI if not exists
            if (inspectorCanvas == null)
            {
                CreateInspectorUI();
            }
            
            // Hide inspector initially
            if (inspectorPanel != null)
                inspectorPanel.SetActive(false);
        }
        
        private void CreateInspectorUI()
        {
            // Create canvas
            GameObject canvasGO = new GameObject("Runtime Inspector Canvas");
            inspectorCanvas = canvasGO.AddComponent<Canvas>();
            inspectorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            inspectorCanvas.sortingOrder = 1000;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create main panel
            GameObject panelGO = new GameObject("Inspector Panel");
            panelGO.transform.SetParent(inspectorCanvas.transform, false);
            inspectorPanel = panelGO;
            
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.6f, 0.1f);
            panelRect.anchorMax = new Vector2(0.95f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            CreateInspectorContent(panelGO);
        }
        
        private void CreateInspectorContent(GameObject parent)
        {
            // Header
            GameObject headerGO = new GameObject("Header");
            headerGO.transform.SetParent(parent.transform, false);
            
            RectTransform headerRect = headerGO.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.9f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;
            
            objectNameText = headerGO.AddComponent<TextMeshProUGUI>();
            objectNameText.text = "No Object Selected";
            objectNameText.fontSize = 18;
            objectNameText.fontStyle = FontStyles.Bold;
            objectNameText.color = Color.white;
            objectNameText.alignment = TextAlignmentOptions.Center;
            
            // Buttons
            CreateButtons(parent);
            
            // Scroll view
            CreateScrollView(parent);
        }
        
        private void CreateButtons(GameObject parent)
        {
            GameObject buttonContainer = new GameObject("Button Container");
            buttonContainer.transform.SetParent(parent.transform, false);
            
            RectTransform buttonRect = buttonContainer.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 0.85f);
            buttonRect.anchorMax = new Vector2(1, 0.9f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            HorizontalLayoutGroup layout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;
            
            // Close button
            closeButton = CreateButton(buttonContainer, "Close", CloseInspector);
            
            // Refresh button
            refreshButton = CreateButton(buttonContainer, "Refresh", RefreshInspector);
        }
        
        private Button CreateButton(GameObject parent, string text, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonGO = new GameObject(text + " Button");
            buttonGO.transform.SetParent(parent.transform, false);
            
            Button button = buttonGO.AddComponent<Button>();
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 14;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            button.onClick.AddListener(action);
            
            return button;
        }
        
        private void CreateScrollView(GameObject parent)
        {
            GameObject scrollViewGO = new GameObject("Scroll View");
            scrollViewGO.transform.SetParent(parent.transform, false);
            
            RectTransform scrollRect = scrollViewGO.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 0.85f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            this.scrollRect = scrollViewGO.AddComponent<ScrollRect>();
            
            // Viewport
            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollViewGO.transform, false);
            
            RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            
            Image viewportImage = viewportGO.AddComponent<Image>();
            viewportImage.color = new Color(0.05f, 0.05f, 0.05f, 1f);
            
            Mask viewportMask = viewportGO.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            
            // Content
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            componentContainer = contentGO.transform;
            
            RectTransform contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            
            VerticalLayoutGroup contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 5;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            
            ContentSizeFitter contentFitter = contentGO.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Configure scroll rect
            this.scrollRect.content = contentRect;
            this.scrollRect.viewport = viewportRect;
            this.scrollRect.horizontal = false;
            this.scrollRect.vertical = true;
        }        private void SetupEventHandlers()
        {
            // Register debug commands
            var console = FindFirstObjectByType<DebugConsoleManager>();
            if (console != null)
            {
                console.RegisterCommand("inspect", "Inspect object by name", "inspect [object_name]", 
                    (args, data) => { 
                        InspectObjectByName(args.ToArray()); 
                        return "Inspect command executed"; 
                    });
                console.RegisterCommand("inspector.toggle", "Toggle runtime object inspector", "inspector.toggle", 
                    (args, data) => { 
                        ToggleInspector(); 
                        return "Inspector toggled"; 
                    });
            }
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            // Toggle inspector
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleInspector();
            }
            
            // Object selection via mouse click
            if (!isInspectorActive) return;
            
            if (Input.GetMouseButtonDown(0) && playerCamera != null)
            {
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, selectableLayers))
                {
                    SelectObject(hit.collider.gameObject);
                }
                else if (Physics2D.Raycast(ray.origin, ray.direction, raycastDistance, selectableLayers))
                {
                    RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction, raycastDistance, selectableLayers);
                    if (hit2D.collider != null)
                    {
                        SelectObject(hit2D.collider.gameObject);
                    }
                }
            }
        }
        
        public void ToggleInspector()
        {
            isInspectorActive = !isInspectorActive;
            if (inspectorPanel != null)
                inspectorPanel.SetActive(isInspectorActive);
            
            AdvancedLogger.Log(LogLevel.Info, LogCategory.General, $"Runtime Object Inspector {(isInspectorActive ? "Enabled" : "Disabled")}");
        }
        
        public void SelectObject(GameObject obj)
        {
            selectedObject = obj;
            RefreshInspector();
        }
        
        public void RefreshInspector()
        {
            if (selectedObject == null) return;
            
            ClearInspectorContent();
            DisplayObjectInfo();
        }
        
        private void ClearInspectorContent()
        {
            foreach (GameObject element in uiElements)
            {
                if (element != null)
                    DestroyImmediate(element);
            }
            uiElements.Clear();
            propertyCache.Clear();
        }
        
        private void DisplayObjectInfo()
        {
            if (selectedObject == null) return;
            
            objectNameText.text = $"Inspecting: {selectedObject.name}";
            
            // Get all components
            Component[] components = selectedObject.GetComponents<Component>();
            
            foreach (Component component in components)
            {
                if (component != null)
                {
                    CreateComponentSection(component);
                }
            }
        }
        
        private void CreateComponentSection(Component component)
        {
            Type componentType = component.GetType();
            
            // Component header
            GameObject headerGO = CreateComponentHeader(componentType.Name);
            uiElements.Add(headerGO);
            
            // Get fields and properties
            FieldInfo[] fields = GetCachedFields(componentType);
            PropertyInfo[] properties = GetCachedProperties(componentType);
            
            // Display public fields
            foreach (FieldInfo field in fields)
            {
                if (field.IsPublic && !field.IsStatic)
                {
                    CreatePropertyField(component, field.Name, field.FieldType, field.GetValue(component), 
                        (value) => field.SetValue(component, value));
                }
            }
            
            // Display public properties
            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && property.GetMethod.IsPublic && !property.GetMethod.IsStatic)
                {
                    try
                    {
                        object value = property.GetValue(component);
                        Action<object> setter = null;
                        
                        if (property.CanWrite && property.SetMethod.IsPublic)
                        {
                            setter = (newValue) => property.SetValue(component, newValue);
                        }
                        
                        CreatePropertyField(component, property.Name, property.PropertyType, value, setter);
                    }
                    catch (Exception e)
                    {
                        AdvancedLogger.Log(LogLevel.Warning, LogCategory.General, $"Failed to read property {property.Name}: {e.Message}");
                    }
                }
            }
        }
        
        private GameObject CreateComponentHeader(string componentName)
        {
            GameObject headerGO = new GameObject($"Component Header: {componentName}");
            headerGO.transform.SetParent(componentContainer, false);
            
            Image headerImage = headerGO.AddComponent<Image>();
            headerImage.color = new Color(0.2f, 0.4f, 0.6f, 1f);
            
            RectTransform headerRect = headerGO.GetComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 30);
            
            GameObject textGO = new GameObject("Header Text");
            textGO.transform.SetParent(headerGO.transform, false);
            
            TextMeshProUGUI headerText = textGO.AddComponent<TextMeshProUGUI>();
            headerText.text = componentName;
            headerText.fontSize = 16;
            headerText.fontStyle = FontStyles.Bold;
            headerText.color = Color.white;
            headerText.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);
            
            return headerGO;
        }
        
        private void CreatePropertyField(Component component, string propertyName, Type propertyType, object value, Action<object> setter)
        {
            GameObject fieldGO = new GameObject($"Property: {propertyName}");
            fieldGO.transform.SetParent(componentContainer, false);
            
            RectTransform fieldRect = fieldGO.AddComponent<RectTransform>();
            fieldRect.sizeDelta = new Vector2(0, 25);
            
            HorizontalLayoutGroup layout = fieldGO.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            
            // Property name label
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(fieldGO.transform, false);
            
            TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = $"{propertyName}:";
            labelText.fontSize = 12;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Left;
            
            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(150, 25);
            
            // Property value field
            CreateValueField(fieldGO, propertyType, value, setter);
            
            uiElements.Add(fieldGO);
        }
        
        private void CreateValueField(GameObject parent, Type valueType, object value, Action<object> setter)
        {
            if (IsSimpleType(valueType))
            {
                CreateEditableField(parent, valueType, value, setter);
            }
            else
            {
                CreateReadOnlyField(parent, value);
            }
        }
        
        private void CreateEditableField(GameObject parent, Type valueType, object value, Action<object> setter)
        {
            GameObject inputGO = new GameObject("Input Field");
            inputGO.transform.SetParent(parent.transform, false);
            
            Image inputImage = inputGO.AddComponent<Image>();
            inputImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            RectTransform inputRect = inputGO.GetComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(200, 25);
            
            TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();
            inputField.text = value?.ToString() ?? "null";
            
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(inputGO.transform, false);
            
            TextMeshProUGUI inputText = textGO.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 12;
            inputText.color = Color.white;
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);
            
            inputField.textComponent = inputText;
            
            if (setter != null)
            {
                inputField.onEndEdit.AddListener((newValue) => {
                    try
                    {
                        object convertedValue = ConvertStringToType(newValue, valueType);
                        setter(convertedValue);
                        AdvancedLogger.Log(LogLevel.Info, LogCategory.General, $"Property updated: {newValue}");
                    }
                    catch (Exception e)
                    {
                        AdvancedLogger.Log(LogLevel.Error, LogCategory.General, $"Failed to convert value: {e.Message}");
                        inputField.text = value?.ToString() ?? "null"; // Revert to original value
                    }
                });
            }
            else
            {
                inputField.interactable = false;
                inputImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            }
        }
        
        private void CreateReadOnlyField(GameObject parent, object value)
        {
            GameObject labelGO = new GameObject("Value Label");
            labelGO.transform.SetParent(parent.transform, false);
            
            TextMeshProUGUI valueText = labelGO.AddComponent<TextMeshProUGUI>();
            valueText.text = value?.ToString() ?? "null";
            valueText.fontSize = 12;
            valueText.color = Color.gray;
            valueText.alignment = TextAlignmentOptions.Left;
            
            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(200, 25);
        }
        
        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || 
                   type == typeof(string) || 
                   type == typeof(Vector2) || 
                   type == typeof(Vector3) || 
                   type == typeof(Color) || 
                   type.IsEnum;
        }
        
        private object ConvertStringToType(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;
            
            if (targetType == typeof(bool))
                return bool.Parse(value);
            
            if (targetType == typeof(int))
                return int.Parse(value);
            
            if (targetType == typeof(float))
                return float.Parse(value);
            
            if (targetType == typeof(double))
                return double.Parse(value);
            
            if (targetType.IsEnum)
                return Enum.Parse(targetType, value);
            
            if (targetType == typeof(Vector2))
            {
                string[] parts = value.Split(',');
                return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            }
            
            if (targetType == typeof(Vector3))
            {
                string[] parts = value.Split(',');
                return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            }
            
            if (targetType == typeof(Color))
            {
                string[] parts = value.Split(',');
                return new Color(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), 
                    parts.Length > 3 ? float.Parse(parts[3]) : 1f);
            }
            
            return Convert.ChangeType(value, targetType);
        }
        
        private FieldInfo[] GetCachedFields(Type type)
        {
            if (!fieldCache.ContainsKey(type))
            {
                fieldCache[type] = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            }
            return fieldCache[type];
        }
        
        private PropertyInfo[] GetCachedProperties(Type type)
        {
            if (!propertyInfoCache.ContainsKey(type))
            {
                propertyInfoCache[type] = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            }
            return propertyInfoCache[type];
        }
        
        // Console command handlers
        private void InspectObjectByName(string[] args)
        {
            if (args.Length < 1)
            {
                AdvancedLogger.Log(LogLevel.Warning, LogCategory.General, "Usage: inspect <object_name>");
                return;
            }
            
            string objectName = args[0];
            GameObject targetObject = GameObject.Find(objectName);
            
            if (targetObject != null)
            {
                SelectObject(targetObject);
                if (!isInspectorActive)
                    ToggleInspector();
                AdvancedLogger.Log(LogLevel.Info, LogCategory.General, $"Now inspecting: {objectName}");
            }
            else
            {
                AdvancedLogger.Log(LogLevel.Warning, LogCategory.General, $"Object not found: {objectName}");
            }
        }
        
        private void CloseInspector()
        {
            isInspectorActive = false;
            if (inspectorPanel != null)
                inspectorPanel.SetActive(false);
        }
        
        private void OnDestroy()
        {
            // Clean up UI elements
            ClearInspectorContent();
        }
    }
}
#endif

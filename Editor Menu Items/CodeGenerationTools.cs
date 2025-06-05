using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Lineage.Editor
{
    /// <summary>
    /// Advanced code generation and scaffolding tools for rapid development
    /// </summary>
    public static class CodeGenerationTools
    {
        [MenuItem("Lineage/Code Generation/Create MVC Pattern", false, 1100)]
        public static void CreateMVCPattern()
        {
            string baseName = EditorUtility.DisplayDialog("Create MVC Pattern", "Enter base name (e.g., 'Player', 'Inventory'):", "Create", "Cancel") ?
                EditorUtility.DisplayDialogComplex("MVC Generator", "Enter the base name:", "Create", "Cancel", "") == 0 ?
                GetUserInput("Enter base name (e.g., 'Player', 'Inventory'):") : null : null;
            
            if (string.IsNullOrEmpty(baseName)) return;
            
            CreateMVCFiles(baseName);
        }
        
        [MenuItem("Lineage/Code Generation/Create Singleton Manager", false, 1101)]
        public static void CreateSingletonManager()
        {
            string managerName = GetUserInput("Enter manager name (e.g., 'GameManager', 'AudioManager'):");
            if (string.IsNullOrEmpty(managerName)) return;
            
            CreateSingletonManagerFile(managerName);
        }
        
        [MenuItem("Lineage/Code Generation/Create ScriptableObject Data", false, 1102)]
        public static void CreateScriptableObjectData()
        {
            string dataName = GetUserInput("Enter data class name (e.g., 'WeaponData', 'CharacterData'):");
            if (string.IsNullOrEmpty(dataName)) return;
            
            CreateScriptableObjectFile(dataName);
        }
        
        [MenuItem("Lineage/Code Generation/Create Component System", false, 1120)]
        public static void CreateComponentSystem()
        {
            string systemName = GetUserInput("Enter system name (e.g., 'Movement', 'Combat', 'Inventory'):");
            if (string.IsNullOrEmpty(systemName)) return;
            
            CreateComponentSystemFiles(systemName);
        }
        
        [MenuItem("Lineage/Code Generation/Create Event System", false, 1140)]
        public static void CreateEventSystem()
        {
            string eventName = GetUserInput("Enter event name (e.g., 'PlayerDied', 'ItemCollected'):");
            if (string.IsNullOrEmpty(eventName)) return;
            
            CreateEventSystemFiles(eventName);
        }
        
        [MenuItem("Lineage/Code Generation/Create State Machine", false, 1160)]
        public static void CreateStateMachine()
        {
            string stateMachineName = GetUserInput("Enter state machine name (e.g., 'Player', 'Enemy', 'Game'):");
            if (string.IsNullOrEmpty(stateMachineName)) return;
            
            CreateStateMachineFiles(stateMachineName);
        }
        
        [MenuItem("Lineage/Code Generation/Generate Interface Implementation", false, 1180)]
        public static void GenerateInterfaceImplementation()
        {
            // Find all interfaces in the project
            string[] scriptFiles = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            List<string> interfaces = new List<string>();
            
            foreach (string file in scriptFiles)
            {
                string content = File.ReadAllText(file);
                if (content.Contains("interface I"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName.StartsWith("I") && char.IsUpper(fileName[1]))
                    {
                        interfaces.Add(fileName);
                    }
                }
            }
            
            if (interfaces.Count == 0)
            {
                Debug.LogWarning("[Code Generation] No interfaces found in Assets/Scripts");
                return;
            }
            
            // Show selection dialog
            GenericMenu menu = new GenericMenu();
            foreach (string interfaceName in interfaces)
            {
                menu.AddItem(new GUIContent(interfaceName), false, () => GenerateImplementationForInterface(interfaceName));
            }
            menu.ShowAsContext();
        }
        
        private static void CreateMVCFiles(string baseName)
        {
            // Model
            string modelContent = GenerateModelClass(baseName);
            CreateScriptFile($"{baseName}Model", "Assets/Scripts/Models", modelContent);
            
            // View
            string viewContent = GenerateViewClass(baseName);
            CreateScriptFile($"{baseName}View", "Assets/Scripts/Views", viewContent);
            
            // Controller
            string controllerContent = GenerateControllerClass(baseName);
            CreateScriptFile($"{baseName}Controller", "Assets/Scripts/Controllers", controllerContent);
            
            Debug.Log($"[Code Generation] Created MVC pattern for {baseName}");
        }
        
        private static void CreateSingletonManagerFile(string managerName)
        {
            string content = $@"using UnityEngine;

namespace Lineage.Core
{{
    /// <summary>
    /// Singleton manager for {managerName.Replace("Manager", "")} functionality
    /// </summary>
    public class {managerName} : MonoBehaviour
    {{
        private static {managerName} _instance;
        public static {managerName} Instance
        {{
            get
            {{
                if (_instance == null)
                {{
                    _instance = FindObjectOfType<{managerName}>();
                    if (_instance == null)
                    {{
                        GameObject go = new GameObject(""{managerName}"");
                        _instance = go.AddComponent<{managerName}>();
                        DontDestroyOnLoad(go);
                    }}
                }}
                return _instance;
            }}
        }}
        
        [Header(""{managerName} Settings"")]
        [SerializeField] private bool _initializeOnAwake = true;
        
        private bool _isInitialized = false;
        
        private void Awake()
        {{
            if (_instance == null)
            {{
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (_initializeOnAwake)
                {{
                    Initialize();
                }}
            }}
            else if (_instance != this)
            {{
                Destroy(gameObject);
            }}
        }}
        
        public void Initialize()
        {{
            if (_isInitialized) return;
            
            // TODO: Initialize {managerName.Replace("Manager", "")} systems
            OnInitialize();
            
            _isInitialized = true;
            Debug.Log($""[{{nameof({managerName})}}] Initialized successfully"");
        }}
        
        protected virtual void OnInitialize()
        {{
            // Override in derived classes for custom initialization
        }}
        
        private void OnDestroy()
        {{
            if (_instance == this)
            {{
                OnShutdown();
                _instance = null;
            }}
        }}
        
        protected virtual void OnShutdown()
        {{
            // Override in derived classes for cleanup
        }}
        
        // TODO: Add {managerName.Replace("Manager", "")} specific methods here
    }}
}}";
            
            CreateScriptFile(managerName, "Assets/Scripts/Managers", content);
            Debug.Log($"[Code Generation] Created singleton manager: {managerName}");
        }
        
        private static void CreateScriptableObjectFile(string dataName)
        {
            string content = $@"using UnityEngine;

namespace Lineage.Data
{{
    /// <summary>
    /// ScriptableObject data container for {dataName.Replace("Data", "")} configuration
    /// </summary>
    [CreateAssetMenu(fileName = ""New{dataName}"", menuName = ""Lineage/Data/{dataName}"", order = 1)]
    public class {dataName} : ScriptableObject
    {{
        [Header(""{dataName.Replace("Data", "")} Configuration"")]
        [SerializeField] private string _displayName = """";
        [SerializeField] private string _description = """";
        [SerializeField] private Sprite _icon;
        
        [Header(""Numeric Properties"")]
        [SerializeField] private float _value = 1.0f;
        [SerializeField] private int _level = 1;
        
        // Properties
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public float Value => _value;
        public int Level => _level;
        
        // TODO: Add specific properties for {dataName.Replace("Data", "")}
        
        private void OnValidate()
        {{
            // Validate data constraints
            _level = Mathf.Max(1, _level);
            _value = Mathf.Max(0, _value);
        }}
        
        public override string ToString()
        {{
            return $""{{_displayName}} (Level {{_level}})"";
        }}
    }}
}}";
            
            CreateScriptFile(dataName, "Assets/Scripts/Data", content);
            Debug.Log($"[Code Generation] Created ScriptableObject: {dataName}");
        }
        
        private static void CreateComponentSystemFiles(string systemName)
        {
            // Interface
            string interfaceContent = $@"using UnityEngine;

namespace Lineage.Core
{{
    /// <summary>
    /// Interface for {systemName} system components
    /// </summary>
    public interface I{systemName}System
    {{
        bool IsActive {{ get; set; }}
        void Initialize();
        void UpdateSystem();
        void Shutdown();
    }}
}}";
            
            // Component
            string componentContent = $@"using UnityEngine;

namespace Lineage.Behavior
{{
    /// <summary>
    /// {systemName} system component
    /// </summary>
    public class {systemName}Component : MonoBehaviour, Lineage.Core.I{systemName}System
    {{
        [Header(""{systemName} Settings"")]
        [SerializeField] private bool _isActive = true;
        [SerializeField] private bool _updateAutomatically = true;
        
        public bool IsActive {{ get => _isActive; set => _isActive = value; }}
        
        private bool _isInitialized = false;
        
        private void Awake()
        {{
            Initialize();
        }}
        
        private void Update()
        {{
            if (_updateAutomatically && _isActive && _isInitialized)
            {{
                UpdateSystem();
            }}
        }}
        
        public virtual void Initialize()
        {{
            if (_isInitialized) return;
            
            // TODO: Initialize {systemName} component
            OnInitialize();
            
            _isInitialized = true;
        }}
        
        public virtual void UpdateSystem()
        {{
            // TODO: Update {systemName} logic
            OnUpdate();
        }}
        
        public virtual void Shutdown()
        {{
            // TODO: Cleanup {systemName} component
            OnShutdown();
            _isInitialized = false;
        }}
        
        protected virtual void OnInitialize() {{ }}
        protected virtual void OnUpdate() {{ }}
        protected virtual void OnShutdown() {{ }}
        
        private void OnDestroy()
        {{
            if (_isInitialized)
            {{
                Shutdown();
            }}
        }}
    }}
}}";
            
            CreateScriptFile($"I{systemName}System", "Assets/Scripts/Interfaces", interfaceContent);
            CreateScriptFile($"{systemName}Component", "Assets/Scripts/Components", componentContent);
            
            Debug.Log($"[Code Generation] Created component system: {systemName}");
        }
        
        private static string GenerateModelClass(string baseName)
        {
            return $@"using UnityEngine;
using System;

namespace Lineage.Models
{{
    /// <summary>
    /// Data model for {baseName}
    /// </summary>
    [Serializable]
    public class {baseName}Model
    {{
        [Header(""{baseName} Data"")]
        [SerializeField] private string _id = System.Guid.NewGuid().ToString();
        [SerializeField] private string _name = """";
        [SerializeField] private bool _isActive = true;
        
        // Events
        public event Action<{baseName}Model> OnDataChanged;
        
        // Properties
        public string Id => _id;
        public string Name 
        {{ 
            get => _name; 
            set 
            {{ 
                if (_name != value)
                {{
                    _name = value;
                    NotifyDataChanged();
                }}
            }}
        }}
        
        public bool IsActive 
        {{ 
            get => _isActive; 
            set 
            {{ 
                if (_isActive != value)
                {{
                    _isActive = value;
                    NotifyDataChanged();
                }}
            }}
        }}
        
        // TODO: Add specific data fields for {baseName}
        
        private void NotifyDataChanged()
        {{
            OnDataChanged?.Invoke(this);
        }}
        
        public override string ToString()
        {{
            return $""{{_name}} ({{_id}})"";
        }}
    }}
}}";
        }
        
        private static string GenerateViewClass(string baseName)
        {
            return $@"using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lineage.Views
{{
    /// <summary>
    /// View component for {baseName}
    /// </summary>
    public class {baseName}View : MonoBehaviour
    {{
        [Header(""{baseName} UI References"")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private Image _statusIcon;
        
        private Lineage.Models.{baseName}Model _model;
        
        public void Initialize(Lineage.Models.{baseName}Model model)
        {{
            _model = model;
            _model.OnDataChanged += OnModelDataChanged;
            
            if (_actionButton != null)
            {{
                _actionButton.onClick.AddListener(OnActionButtonClicked);
            }}
            
            UpdateView();
        }}
        
        private void OnModelDataChanged(Lineage.Models.{baseName}Model model)
        {{
            UpdateView();
        }}
        
        private void UpdateView()
        {{
            if (_model == null) return;
            
            if (_nameText != null)
            {{
                _nameText.text = _model.Name;
            }}
            
            if (_statusIcon != null)
            {{
                _statusIcon.color = _model.IsActive ? Color.green : Color.gray;
            }}
            
            // TODO: Update additional UI elements
        }}
        
        private void OnActionButtonClicked()
        {{
            // TODO: Handle button click
            Debug.Log($""Action button clicked for {{_model?.Name}}"");
        }}
        
        private void OnDestroy()
        {{
            if (_model != null)
            {{
                _model.OnDataChanged -= OnModelDataChanged;
            }}
            
            if (_actionButton != null)
            {{
                _actionButton.onClick.RemoveListener(OnActionButtonClicked);
            }}
        }}
    }}
}}";
        }
        
        private static string GenerateControllerClass(string baseName)
        {
            return $@"using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Controllers
{{
    /// <summary>
    /// Controller for {baseName} logic and coordination
    /// </summary>
    public class {baseName}Controller : MonoBehaviour
    {{
        [Header(""{baseName} Controller Settings"")]
        [SerializeField] private Lineage.Views.{baseName}View _viewPrefab;
        [SerializeField] private Transform _viewContainer;
        
        private List<Lineage.Models.{baseName}Model> _models = new List<Lineage.Models.{baseName}Model>();
        private List<Lineage.Views.{baseName}View> _views = new List<Lineage.Views.{baseName}View>();
        
        private void Start()
        {{
            Initialize();
        }}
        
        public void Initialize()
        {{
            // TODO: Initialize {baseName} controller
            CreateDefault{baseName}();
        }}
        
        public void Add{baseName}(Lineage.Models.{baseName}Model model)
        {{
            if (model == null || _models.Contains(model)) return;
            
            _models.Add(model);
            CreateViewForModel(model);
        }}
        
        public void Remove{baseName}(Lineage.Models.{baseName}Model model)
        {{
            if (model == null || !_models.Contains(model)) return;
            
            int index = _models.IndexOf(model);
            _models.RemoveAt(index);
            
            if (index < _views.Count)
            {{
                Destroy(_views[index].gameObject);
                _views.RemoveAt(index);
            }}
        }}
        
        private void CreateViewForModel(Lineage.Models.{baseName}Model model)
        {{
            if (_viewPrefab == null || _viewContainer == null) return;
            
            Lineage.Views.{baseName}View view = Instantiate(_viewPrefab, _viewContainer);
            view.Initialize(model);
            _views.Add(view);
        }}
        
        private void CreateDefault{baseName}()
        {{
            var defaultModel = new Lineage.Models.{baseName}Model();
            defaultModel.Name = ""Default {baseName}"";
            Add{baseName}(defaultModel);
        }}
        
        public List<Lineage.Models.{baseName}Model> GetAll{baseName}s()
        {{
            return new List<Lineage.Models.{baseName}Model>(_models);
        }}
    }}
}}";
        }
        
        private static void CreateStateMachineFiles(string stateMachineName)
        {
            // State interface
            string stateInterfaceContent = $@"namespace Lineage.StateMachines
{{
    /// <summary>
    /// Interface for {stateMachineName} states
    /// </summary>
    public interface I{stateMachineName}State
    {{
        void Enter({stateMachineName}StateMachine stateMachine);
        void Update({stateMachineName}StateMachine stateMachine);
        void Exit({stateMachineName}StateMachine stateMachine);
    }}
}}";
            
            // State machine
            string stateMachineContent = $@"using UnityEngine;
using System.Collections.Generic;

namespace Lineage.StateMachines
{{
    /// <summary>
    /// State machine for {stateMachineName}
    /// </summary>
    public class {stateMachineName}StateMachine : MonoBehaviour
    {{
        [Header(""{stateMachineName} State Machine"")]
        [SerializeField] private bool _debugMode = false;
        
        private I{stateMachineName}State _currentState;
        private Dictionary<System.Type, I{stateMachineName}State> _states = new Dictionary<System.Type, I{stateMachineName}State>();
        
        public I{stateMachineName}State CurrentState => _currentState;
        
        private void Start()
        {{
            InitializeStates();
        }}
        
        private void Update()
        {{
            _currentState?.Update(this);
        }}
        
        protected virtual void InitializeStates()
        {{
            // TODO: Register states
            // RegisterState(new Idle{stateMachineName}State());
            // ChangeState<Idle{stateMachineName}State>();
        }}
        
        public void RegisterState<T>(T state) where T : I{stateMachineName}State
        {{
            _states[typeof(T)] = state;
        }}
        
        public void ChangeState<T>() where T : I{stateMachineName}State
        {{
            if (_states.TryGetValue(typeof(T), out I{stateMachineName}State newState))
            {{
                if (_debugMode)
                {{
                    Debug.Log($""[{{nameof({stateMachineName}StateMachine)}}] Changing state to {{typeof(T).Name}}"");
                }}
                
                _currentState?.Exit(this);
                _currentState = newState;
                _currentState.Enter(this);
            }}
            else
            {{
                Debug.LogError($""[{{nameof({stateMachineName}StateMachine)}}] State {{typeof(T).Name}} not registered!"");
            }}
        }}
        
        public bool IsInState<T>() where T : I{stateMachineName}State
        {{
            return _currentState is T;
        }}
    }}
}}";
            
            CreateScriptFile($"I{stateMachineName}State", "Assets/Scripts/StateMachines", stateInterfaceContent);
            CreateScriptFile($"{stateMachineName}StateMachine", "Assets/Scripts/StateMachines", stateMachineContent);
            
            Debug.Log($"[Code Generation] Created state machine: {stateMachineName}");
        }
        
        private static void CreateEventSystemFiles(string eventName)
        {
            // Event data
            string eventDataContent = $@"using System;

namespace Lineage.Events
{{
    /// <summary>
    /// Event data for {eventName}
    /// </summary>
    [Serializable]
    public class {eventName}EventData
    {{
        public float timestamp;
        // TODO: Add specific event data fields
        
        public {eventName}EventData()
        {{
            timestamp = UnityEngine.Time.time;
        }}
    }}
}}";
            
            // Event handler
            string eventHandlerContent = $@"using UnityEngine;
using UnityEngine.Events;

namespace Lineage.Events
{{
    /// <summary>
    /// Event handler for {eventName} events
    /// </summary>
    public class {eventName}EventHandler : MonoBehaviour
    {{
        [Header(""{eventName} Events"")]
        [SerializeField] private UnityEvent<{eventName}EventData> _on{eventName};
        
        public UnityEvent<{eventName}EventData> On{eventName} => _on{eventName};
        
        private void OnEnable()
        {{
            EventManager.Subscribe<{eventName}EventData>(Handle{eventName});
        }}
        
        private void OnDisable()
        {{
            EventManager.Unsubscribe<{eventName}EventData>(Handle{eventName});
        }}
        
        private void Handle{eventName}({eventName}EventData eventData)
        {{
            _on{eventName}?.Invoke(eventData);
        }}
        
        public void Trigger{eventName}()
        {{
            var eventData = new {eventName}EventData();
            EventManager.TriggerEvent(eventData);
        }}
    }}
}}";
            
            CreateScriptFile($"{eventName}EventData", "Assets/Scripts/Events", eventDataContent);
            CreateScriptFile($"{eventName}EventHandler", "Assets/Scripts/Events", eventHandlerContent);
            
            Debug.Log($"[Code Generation] Created event system: {eventName}");
        }
        
        private static void GenerateImplementationForInterface(string interfaceName)
        {
            string implementationName = interfaceName.Substring(1) + "Implementation"; // Remove 'I' prefix
            
            string content = $@"using UnityEngine;

namespace Lineage.Implementations
{{
    /// <summary>
    /// Implementation of {interfaceName}
    /// </summary>
    public class {implementationName} : MonoBehaviour, Lineage.Core.{interfaceName}
    {{
        [Header(""{implementationName} Settings"")]
        [SerializeField] private bool _isEnabled = true;
        
        // TODO: Implement {interfaceName} interface methods
        
        private void Start()
        {{
            Initialize();
        }}
        
        private void Initialize()
        {{
            // TODO: Initialize implementation
        }}
    }}
}}";
            
            CreateScriptFile(implementationName, "Assets/Scripts/Implementations", content);
            Debug.Log($"[Code Generation] Created implementation: {implementationName}");
        }
        
        private static void CreateScriptFile(string fileName, string directory, string content)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string filePath = Path.Combine(directory, $"{fileName}.cs");
            File.WriteAllText(filePath, content);
            AssetDatabase.Refresh();
        }
        
        private static string GetUserInput(string message)
        {
            // This is a simplified input method - in a real implementation you'd use a proper dialog
            return EditorInputDialog.Show("Code Generation", message);
        }
    }
    
    /// <summary>
    /// Simple input dialog for editor tools
    /// </summary>
    public class EditorInputDialog : EditorWindow
    {
        private static string _inputText = "";
        private static string _message = "";
        private static System.Action<string> _onConfirm;
        
        public static string Show(string title, string message)
        {
            _inputText = "";
            _message = message;
            
            EditorInputDialog window = GetWindow<EditorInputDialog>(true, title);
            window.minSize = new Vector2(300, 120);
            window.maxSize = new Vector2(300, 120);
            window.ShowModalUtility();
            
            return _inputText;
        }
        
        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label(_message, EditorStyles.label);
            GUILayout.Space(10);
            
            _inputText = EditorGUILayout.TextField(_inputText);
            
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("OK"))
            {
                Close();
            }
            
            if (GUILayout.Button("Cancel"))
            {
                _inputText = "";
                Close();
            }
            
            GUILayout.EndHorizontal();
        }
    }
}

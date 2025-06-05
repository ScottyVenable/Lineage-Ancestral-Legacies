using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

namespace Lineage.Editor
{
    /// <summary>
    /// Development workflow and utility tools
    /// </summary>
    public static class DevelopmentTools
    {
        [MenuItem("Lineage/Development/Open Project Folder", false, 900)]
        public static void OpenProjectFolder()
        {
            EditorUtility.RevealInFinder(Application.dataPath);
        }
        
        [MenuItem("Lineage/Development/Open Persistent Data Path", false, 901)]
        public static void OpenPersistentDataPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
        
        [MenuItem("Lineage/Development/Open Unity Console Log", false, 920)]
        public static void OpenUnityConsoleLog()
        {
            string logPath = "";
            
            #if UNITY_EDITOR_WIN
            string userProfile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            logPath = Path.Combine(userProfile, @"AppData\Local\Unity\Editor\Editor.log");
            #elif UNITY_EDITOR_OSX
            string userHome = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            logPath = Path.Combine(userHome, "Library/Logs/Unity/Editor.log");
            #endif
            
            if (File.Exists(logPath))
            {
                Process.Start(logPath);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[Development] Unity log file not found at: {logPath}");
            }
        }
        
        [MenuItem("Lineage/Development/Copy Project Path", false, 940)]
        public static void CopyProjectPath()
        {
            string projectPath = Application.dataPath.Replace("/Assets", "");
            EditorGUIUtility.systemCopyBuffer = projectPath;
            UnityEngine.Debug.Log($"[Development] Copied project path to clipboard: {projectPath}");
        }
        
        [MenuItem("Lineage/Development/Open Command Prompt Here", false, 960)]
        public static void OpenCommandPrompt()
        {
            string projectPath = Application.dataPath.Replace("/Assets", "");
            
            #if UNITY_EDITOR_WIN
            Process.Start("cmd.exe", $"/k cd /d \"{projectPath}\"");
            #elif UNITY_EDITOR_OSX
            Process.Start("open", $"-a Terminal \"{projectPath}\"");
            #endif
            
            UnityEngine.Debug.Log("[Development] Opened command prompt in project directory");
        }
        
        [MenuItem("Lineage/Development/Open PowerShell Here", false, 961)]
        public static void OpenPowerShell()
        {
            string projectPath = Application.dataPath.Replace("/Assets", "");
            
            #if UNITY_EDITOR_WIN
            Process.Start("powershell.exe", $"-NoExit -Command \"cd '{projectPath}'\"");
            #endif
            
            UnityEngine.Debug.Log("[Development] Opened PowerShell in project directory");
        }
        
        [MenuItem("Lineage/Development/Create Script Template", false, 980)]
        public static void CreateScriptTemplate()
        {
            string templateContent = @"using UnityEngine;

namespace Lineage.Core
{
    /// <summary>
    /// [Description of what this script does]
    /// </summary>
    public class NewScript : MonoBehaviour
    {
        [Header(""Configuration"")]
        [SerializeField] private float _value = 1.0f;
        
        private void Start()
        {
            
        }
        
        private void Update()
        {
            
        }
    }
}";
            
            string fileName = "NewScript.cs";
            string path = EditorUtility.SaveFilePanel("Create Script Template", "Assets/Scripts", fileName, "cs");
            
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, templateContent);
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log($"[Development] Created script template: {path}");
            }
        }
        
        [MenuItem("Lineage/Development/Screenshot Scene View", false, 1000)]
        public static void ScreenshotSceneView()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Camera camera = sceneView.camera;
                RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
                camera.targetTexture = renderTexture;
                camera.Render();
                
                RenderTexture.active = renderTexture;
                Texture2D screenshot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
                screenshot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                screenshot.Apply();
                
                string fileName = $"Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
                string path = Path.Combine(Application.dataPath, "..", "Screenshots", fileName);
                
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, screenshot.EncodeToPNG());
                
                camera.targetTexture = null;
                RenderTexture.active = null;
                DestroyImmediate(renderTexture);
                DestroyImmediate(screenshot);
                
                UnityEngine.Debug.Log($"[Development] Screenshot saved: {path}");
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                UnityEngine.Debug.LogWarning("[Development] No active Scene View found");
            }
        }
        
        [MenuItem("Lineage/Development/Toggle Console Window", false, 1020)]
        public static void ToggleConsoleWindow()
        {
            System.Type consoleType = System.Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
            if (consoleType != null)
            {
                EditorWindow consoleWindow = EditorWindow.GetWindow(consoleType);
                if (consoleWindow != null)
                {
                    consoleWindow.Focus();
                }
            }
        }
        
        [MenuItem("Lineage/Development/Show System Info", false, 1040)]
        public static void ShowSystemInfo()
        {
            UnityEngine.Debug.Log("=== System Information ===");
            UnityEngine.Debug.Log($"Unity Version: {Application.unityVersion}");
            UnityEngine.Debug.Log($"Platform: {Application.platform}");
            UnityEngine.Debug.Log($"Operating System: {SystemInfo.operatingSystem}");
            UnityEngine.Debug.Log($"Processor: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)");
            UnityEngine.Debug.Log($"Memory: {SystemInfo.systemMemorySize} MB");
            UnityEngine.Debug.Log($"Graphics: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} MB)");
            UnityEngine.Debug.Log($"Graphics API: {SystemInfo.graphicsDeviceType}");
            UnityEngine.Debug.Log($"Screen Resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}");
        }
    }
}

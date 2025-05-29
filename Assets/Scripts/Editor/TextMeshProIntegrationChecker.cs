#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[InitializeOnLoad]
public class TextMeshProIntegrationChecker
{
    static TextMeshProIntegrationChecker()
    {
        // Check if TextMeshPro is in the project
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        bool tmpFound = assemblies.Any(a => a.GetName().Name.Contains("TextMeshPro"));
        
        if (tmpFound)
        {
            AddTMProDefine();
            Debug.Log("TextMeshPro found and integrated.");
        }
        else
        {
            RemoveTMProDefine();
            Debug.LogWarning("TextMeshPro not found. UI features will be limited. Install TextMeshPro from Package Manager.");
        }
    }

    static void AddTMProDefine()
    {
        string define = "TMPRO_PRESENT";
        AddDefineSymbol(define);
    }

    static void RemoveTMProDefine()
    {
        string define = "TMPRO_PRESENT";
        RemoveDefineSymbol(define);
    }

    static void AddDefineSymbol(string define)
    {
        var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        string definesString = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        
        if (!definesString.Contains(define))
        {
            if (string.IsNullOrEmpty(definesString))
                definesString = define;
            else
                definesString += ";" + define;
                
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, definesString);
        }
    }

    static void RemoveDefineSymbol(string define)
    {
        var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        string definesString = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        
        if (definesString.Contains(define))
        {
            definesString = definesString
                .Replace(define + ";", "")
                .Replace(";" + define, "")
                .Replace(define, "");
                
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, definesString);
        }
    }
}
#endif

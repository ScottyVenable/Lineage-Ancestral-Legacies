#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[InitializeOnLoad]
public class CinemachineIntegrationChecker
{
    static CinemachineIntegrationChecker()
    {
        // Check if Cinemachine is in the project
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        bool cinemachineFound = assemblies.Any(a => a.GetName().Name.Contains("Cinemachine"));
        
        if (cinemachineFound)
        {
            AddCinemachineDefine();
            Debug.Log("Cinemachine found and integrated.");
        }
        else
        {
            RemoveCinemachineDefine();
            Debug.LogWarning("Cinemachine not found. Some camera features will be limited.");
        }
    }

    static void AddCinemachineDefine()
    {
        string define = "CINEMACHINE_PRESENT";
        AddDefineSymbol(define);
    }

    static void RemoveCinemachineDefine()
    {
        string define = "CINEMACHINE_PRESENT";
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

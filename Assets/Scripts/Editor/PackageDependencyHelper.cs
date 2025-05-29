#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;

public class PackageDependencyHelper : EditorWindow
{
    private static AddRequest addRequest;
    private static ListRequest listRequest;
    private static List<string> requiredPackages = new List<string>
    {
        "com.unity.textmeshpro",
        "com.unity.cinemachine"
    };

    [MenuItem("Lineage/Check Package Dependencies")]
    public static void CheckDependencies()
    {
        listRequest = Client.List();
        EditorApplication.update += OnPackageListProgress;
    }

    private static void OnPackageListProgress()
    {
        if (listRequest.IsCompleted)
        {
            EditorApplication.update -= OnPackageListProgress;
            
            if (listRequest.Status == StatusCode.Success)
            {
                List<string> packageIdsToInstall = new List<string>();
                
                foreach (var reqPackage in requiredPackages)
                {
                    bool found = false;
                    foreach (var package in listRequest.Result)
                    {
                        if (package.name == reqPackage)
                        {
                            found = true;
                            Debug.Log($"Package {reqPackage} is already installed (version: {package.version})");
                            break;
                        }
                    }
                    
                    if (!found)
                    {
                        packageIdsToInstall.Add(reqPackage);
                    }
                }
                
                if (packageIdsToInstall.Count > 0)
                {
                    string packagesToInstall = string.Join("\n- ", packageIdsToInstall);
                    bool installPackages = EditorUtility.DisplayDialog(
                        "Missing Required Packages", 
                        $"The following packages are required but not installed:\n- {packagesToInstall}\n\nWould you like to install them now?",
                        "Install", "Cancel");
                        
                    if (installPackages)
                    {
                        InstallNextPackage(packageIdsToInstall, 0);
                    }
                }
                else
                {
                    Debug.Log("All required packages are installed!");
                    if (EditorUtility.DisplayDialog("Packages OK", "All required packages are installed!", "OK"))
                    {
                        // Do nothing, just acknowledge
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve package list: " + listRequest.Error.message);
            }
        }
    }

    private static void InstallNextPackage(List<string> packages, int index)
    {
        if (index >= packages.Count)
        {
            Debug.Log("All packages installed successfully!");
            if (EditorUtility.DisplayDialog("Installation Complete", "All packages have been installed successfully!", "OK"))
            {
                // After TextMeshPro is installed, prompt to import essentials
                bool importTMPEssentials = EditorUtility.DisplayDialog(
                    "Import TextMeshPro Essentials",
                    "Would you like to import TextMeshPro essential resources?",
                    "Yes", "No");
                
                if (importTMPEssentials)
                {
                    TMP_PackageUtilities.ImportTextMeshProEssentials();
                }
            }
            return;
        }
        
        string packageToInstall = packages[index];
        Debug.Log($"Installing package: {packageToInstall}");
        
        addRequest = Client.Add(packageToInstall);
        EditorApplication.update += () => OnPackageInstallProgress(packages, index);
    }

    private static void OnPackageInstallProgress(List<string> packages, int index)
    {
        if (addRequest.IsCompleted)
        {
            EditorApplication.update -= () => OnPackageInstallProgress(packages, index);
            
            if (addRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Successfully installed: {addRequest.Result.name} ({addRequest.Result.version})");
                InstallNextPackage(packages, index + 1);
            }
            else
            {
                Debug.LogError($"Failed to install {packages[index]}: {addRequest.Error.message}");
            }
        }
    }
}

// Add this class to work with TextMeshPro utilities
public class TMP_PackageUtilities
{
    public static void ImportTextMeshProEssentials()
    {
        // Use reflection to call the internal TMP method since we can't directly reference
        // the TextMeshPro assembly here
        var assembly = System.Reflection.Assembly.Load("Unity.TextMeshPro.Editor");
        if (assembly == null)
        {
            Debug.LogError("Could not load TextMeshPro Editor assembly.");
            return;
        }
        
        var packageImporterType = assembly.GetType("TMPro.TMP_PackageUtilities") ?? 
                                  assembly.GetType("TMPro.EditorUtilities.TMP_PackageUtilities");
        
        if (packageImporterType != null)
        {
            var method = packageImporterType.GetMethod("ImportTextMeshProEssentials", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            
            if (method != null)
            {
                method.Invoke(null, null);
                Debug.Log("TextMeshPro Essential Resources imported.");
            }
            else
            {
                Debug.LogError("Could not find ImportTextMeshProEssentials method.");
                EditorUtility.DisplayDialog("Manual Import Required", 
                    "Please manually import TextMeshPro Essential Resources through Window > TextMeshPro > Import TMP Essential Resources", 
                    "OK");
            }
        }
        else
        {
            Debug.LogError("Could not find TMP_PackageUtilities class.");
            EditorUtility.DisplayDialog("Manual Import Required", 
                "Please manually import TextMeshPro Essential Resources through Window > TextMeshPro > Import TMP Essential Resources", 
                "OK");
        }
    }
}
#endif

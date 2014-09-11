using UnityEngine;
using System.Collections;
using UnityEditor;
    
public class Exporter
{
    [MenuItem("Assets/ExportWithSettings")]
    static void Export () {
		AssetDatabase.ExportPackage("Assets/", "Entire Project.unitypackage", 
		                            ExportPackageOptions.Recurse | 
		                            ExportPackageOptions.IncludeLibraryAssets );
    }
}
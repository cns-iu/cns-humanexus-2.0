using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using PlasticGui.WorkspaceWindow.BrowseRepository;
using System.Dynamic;

// 2025-5-18
public class ManualMenu : EditorWindow
{
    static GameObject dataContainer;
    static string tempTexDirectory = "Assets/TempTextures";

    [MenuItem("Humanexus/Manual Import")]
    static void ImportManual()
    {
        GetWindow<ManualMenu>("Manual Import");
    }

    void OnGUI()
    {
        // check if tempTextures folder is empty
        if (IsDirectoryEmpty(tempTexDirectory))     // TRUE if empty
        {
            GUILayout.Label("TempTextures folder is empty");
        }
        else    // FALSE if not empty
        {
            // can we check to see if this needs to be built?
            if (GUILayout.Button("Build Database"))
            {
                BuildDatabaseFromFolder();
            }

            if (GUILayout.Button("Cleanup"))
            {
                CleanupManual();
            }
        }

    }

    private static void BuildDatabaseFromFolder()
    {
        //thisDatabase = GameObject.Find("Databases").GetComponent<LoadExcel>().itemDatabase;
        GameObject.Find("Databases").GetComponent<LoadExcel>().itemDatabase.Clear();

        // loop through all files in TempTextures folder
        int counter = 0;
        DirectoryInfo tempTexDi = new DirectoryInfo(tempTexDirectory);
        FileInfo[] texs = tempTexDi.GetFiles("*.jpg");
        foreach (FileInfo tex in texs) counter++;       // count files in folder
        string[] texFilesArray = new string[counter];   // set up array

        int j = 0;
        foreach (FileInfo tex in texs)
        {
            texFilesArray[j] = Path.GetFileName(tex.Name);
            GameObject.Find("Databases").GetComponent<LoadExcel>().AddItem(texFilesArray[j], "na", "na");

            Debug.Log(texFilesArray[j]);
        }
        dataContainer = GameObject.Find("Databases");
        dataContainer.GetComponent<DataContainer>().lastImportSet = "<manual import>>";

        Debug.Log("texs in tex = " + counter);
    }


    // exactly same as Cleanup() in ImportMenu.cs, maybe consolidate
    private static void CleanupManual()
    {
        Debug.Log("Cleaning up: GameObjects, TempMaterials, TempTextures");

        // delete all children of vertexcloud - this goes through each icosphere GameObject
        GameObject spheres = GameObject.Find("Spheres");    // parent where clones go
        foreach (Transform child in spheres.transform)
        {
            for (int i = child.childCount; i > 0; --i)
            {
                Object.DestroyImmediate(child.transform.GetChild(0).gameObject);
            }
            //child.GetComponent<SphereInfo>().clones.Clear();        // clear clones list
            child.GetComponent<SphereInfo>().cloneItems.Clear();
        }

        // delete TempMaterials folder and all contents, then creates new empty folder
        List<string> failedPathsMat = new List<string>();
        string[] assetPathsMat = { "Assets/TempMaterials/" };
        AssetDatabase.DeleteAssets(assetPathsMat, failedPathsMat);
        AssetDatabase.Refresh();
        AssetDatabase.CreateFolder("Assets", "TempMaterials");

        // delete TempTextures folder and all contents, then creates new empty folder
        List<string> failedPathsTex = new List<string>();
        string[] assetPathsTex = { "Assets/TempTextures/" };
        AssetDatabase.DeleteAssets(assetPathsTex, failedPathsTex);
        AssetDatabase.Refresh();
        AssetDatabase.CreateFolder("Assets", "TempTextures");

        dataContainer = GameObject.Find("Databases");
        dataContainer.GetComponent<DataContainer>().lastImportSet = "<empty>";
        dataContainer.GetComponent<LoadExcel>().itemDatabase.Clear();
        EditorUtility.SetDirty(dataContainer);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Cleanup done.");
    }



    public bool IsDirectoryEmpty(string path)
    {
        IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
        using (IEnumerator<string> en = items.GetEnumerator())
        {
            return !en.MoveNext();
        }
    }

}

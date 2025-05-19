using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using PlasticGui.WorkspaceWindow.BrowseRepository;
using System.Dynamic;

// 2025-5-14
public class ManualMenu : EditorWindow
{
    static GameObject dataContainer;

    [MenuItem("Humanexus/Manual Import")]
    static void ImportManual()
    {
        GetWindow<ManualMenu>("Manual Import");
    }

    void OnGUI()
    {
        // -- check if TempRTextures is NOT empty
        if (GUILayout.Button("Build Database"))
        {
            BuildDatabaseFromFolder();
        }
    }

    private static void BuildDatabaseFromFolder()
    {
        //thisDatabase = GameObject.Find("Databases").GetComponent<LoadExcel>().itemDatabase;
        GameObject.Find("Databases").GetComponent<LoadExcel>().itemDatabase.Clear();

        // loop through all files in TempTextures folder
        int counter = 0;
        DirectoryInfo tempTexDi = new DirectoryInfo("Assets/TempTextures");
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

}

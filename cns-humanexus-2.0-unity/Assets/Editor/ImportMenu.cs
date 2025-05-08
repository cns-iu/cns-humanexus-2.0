using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using PlasticGui.WorkspaceWindow.BrowseRepository;

public class ImportMenu : EditorWindow
{

    // data from SciptableObject
    static string masterDirectory = "<initialize first>";
    static string lastImportSet = "<initialize first>";

    public string[] csvfilesArray;
    public int index;
    static string selectedCSV;

    static GameObject dataContainer;

    static bool initialized = false;

    [MenuItem("Humanexus/Texture Sets")]
    static void ImportSet()
    {
        GetWindow<ImportMenu>("Texture Sets");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Initialize Set"))
        {
            Initialize();
        }
        GUILayout.Label("Last Import Set: " + lastImportSet);
        GUILayout.Label("from: " + masterDirectory);

        if (initialized)
        {
            index = EditorGUILayout.Popup(index, csvfilesArray);    // popup returns index
            selectedCSV = csvfilesArray[index];
        }




        if (GUILayout.Button("Import Images from master folder"))
        {
            ImportImages();
        }

        if (GUILayout.Button("Cleanup"))
        {
            Cleanup();
        }


    }

    private static void ImportImages()
    {
        Debug.Log("Importing images...");


        dataContainer.GetComponent<DataContainer>().masterDirectory = masterDirectory;
        dataContainer.GetComponent<DataContainer>().lastImportSet = selectedCSV;
        EditorUtility.SetDirty(dataContainer);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }



    public void Cleanup()
    {
        Debug.Log("cleaning up...");
    }

    public void Initialize()
    {
        // initialize from Databases GO

        dataContainer = GameObject.Find("Databases");
        masterDirectory = dataContainer.GetComponent<DataContainer>().masterDirectory;
        lastImportSet = dataContainer.GetComponent<DataContainer>().lastImportSet;

        // setup for popup menu, more complicated than it shoud be!!!!
        // find all available CSV files in Resources folder
        int i = 0;
        DirectoryInfo csvDi = new DirectoryInfo("Assets/Resources");
        FileInfo[] fis = csvDi.GetFiles("*.csv");
        foreach (FileInfo fi in fis) i++;       // count files in folder
        csvfilesArray = new string[i];      // make array of size

        // populate string array for popup
        int j = 0;
        foreach (FileInfo fi in fis)
        {
            csvfilesArray[j] = Path.GetFileNameWithoutExtension(fi.Name);
            if (fi.Name == lastImportSet)
            {
                index = j;  // set index for popup default from ScriptableObject
            }
            j++;
        }
        initialized = true;
    }



}

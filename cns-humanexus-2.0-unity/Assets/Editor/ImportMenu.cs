
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using PlasticGui.WorkspaceWindow.BrowseRepository;
using System.Dynamic;

//  2025-5-13
// adds Texture Sets menu under Humanexus
// - persistent data (last import set & master image directory) are stored on Databases GO
// - persistent data is updated by this script after changes were made
// - Gadgets in Texture Sets window:
//  Initialize Set from Database: reads data from Databases GO to determine what next
//  Info about installed image set in TempFolder & master image directory (source)
//  Popup menu to choose from CSV sets in Resources folder
//  Import Images...: copies images from master image directory to TempTextures as required by selected CSV set
//  Cleanup: deletes TempTextures folder with contents and creates new empty folder

// improvements:
// - better setting and evaluation of gadget display flags

public class ImportMenu : EditorWindow
{
    static string masterDirectory = "<initialize first>";
    static string lastImportSet = "<initialize first>";

    public string[] csvfilesArray;
    static int index;
    static string selectedCSV;
    static GameObject dataContainer;
    static private List<Item> thisDatabase = new List<Item>();

    // button/gadget enable flags
    static bool enableInitialize = true;
    static bool enableImport = false;
    static bool enableCleanup = false;
    static bool enableMenu = false;
    static bool enableInfo = false;

    [MenuItem("Humanexus/Texture Sets")]
    static void ImportSet()
    {
        GetWindow<ImportMenu>("Texture Sets");
        enableInitialize = true;
        enableMenu = false;
        enableImport = false;
        enableInfo = false;
        enableCleanup = false;
    }

    void OnGUI()
    {
        if (enableInitialize)
        {
            if (GUILayout.Button("Initialize Set from Database"))
            {
                int init = Initialize();

                if (init == -1)
                {
                    enableInitialize = false;
                    enableMenu = true;
                    enableImport = true;
                    enableCleanup = false;
                    enableInfo = false;
                    index = 0;
                }
                else
                {
                    enableInitialize = false;
                    enableMenu = false;
                    enableImport = false;
                    enableInfo = true;
                    enableCleanup = true;
                    index = init;
                }
                Debug.Log("initialize result: " + init);
            }
        }

        if (enableMenu)
        {
            index = EditorGUILayout.Popup(index, csvfilesArray);    // popup returns index
            selectedCSV = csvfilesArray[index];
        }

        if (enableInfo)
        {
            GUILayout.Label("Last Import Set: " + lastImportSet);
            GUILayout.Label("from: " + masterDirectory);
        }

        if (enableImport)
        {
            if (GUILayout.Button("Import Images from master folder"))
            {
                System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();   // start timer for import
                st.Start();
   
                ImportImages();
                enableInitialize = false;
                enableMenu = false;
                enableImport = false;
                enableCleanup = true;
                enableInfo = true;

                st.Stop();
                Debug.Log("Duration of Import in milliseconds: " + st.ElapsedMilliseconds);
            }
        }

        if (enableCleanup)
        {
            if (GUILayout.Button("Cleanup"))
            {
                // add safety dialog?
                Cleanup();
                enableInitialize = false;
                enableMenu = true;
                enableImport = true;
                enableCleanup = false;
                enableInfo = false;
                index = 0;
            }
        }
    }

    private static void ImportImages()
    {
        Debug.Log("Importing images from: " + masterDirectory);
        Debug.Log("selectedCSV = " + selectedCSV);

        DirectoryInfo sourceDi = new DirectoryInfo(masterDirectory);
        DirectoryInfo destDi = new DirectoryInfo("Assets/TempTextures");
        GameObject.Find("Databases").GetComponent<LoadExcel>().LoadItemData(selectedCSV);
        thisDatabase = GameObject.Find("Databases").GetComponent<LoadExcel>().itemDatabase;

        foreach (Item i in thisDatabase)
        {
            FileInfo[] fileInfos = sourceDi.GetFiles(i.graphic);
            foreach (FileInfo fileInfo in fileInfos)
            {
                fileInfo.CopyTo(Path.Combine(destDi.ToString(), fileInfo.Name), true);
                Debug.Log("Copying: " + fileInfo.Name);
            }
            //AssetDatabase.Refresh();  // not refreshing here gives a 18% speed boost
        }
        lastImportSet = selectedCSV + ".csv";

        dataContainer.GetComponent<DataContainer>().lastImportSet = lastImportSet;
        EditorUtility.SetDirty(dataContainer);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Import done.");
    }

    // this deletes/removes all assets produced by build process & tempTextures folder
    public void Cleanup()
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

        dataContainer.GetComponent<DataContainer>().lastImportSet = "<empty>";
        dataContainer.GetComponent<LoadExcel>().itemDatabase.Clear();
        EditorUtility.SetDirty(dataContainer);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Cleanup done.");
    }


    // pull lastImportSet from Databases
    // return ID of valid (installed) CSV file
    // return -1 if not found
    public int Initialize()
    {
        dataContainer = GameObject.Find("Databases");
        masterDirectory = dataContainer.GetComponent<DataContainer>().masterDirectory;
        lastImportSet = dataContainer.GetComponent<DataContainer>().lastImportSet;

        Debug.Log(lastImportSet);

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
                return j;
            }
            j++;
        }
        return -1;
    }



}

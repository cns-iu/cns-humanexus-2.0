using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using PlasticGui.WorkspaceWindow.BrowseRepository;

// CustomEditorWindow for Humanexus 2.0
// Rev. 2025-5-4

public class CustomEditorWindow : EditorWindow
{
    static public GameObject icosphere;     // this is the sphere selected in InitSphere
    static public GameObject ball;
    //static private GameObject vertexCloud;
    static private List<Item> thisDatabase = new List<Item>();

    static private Vector3[] vertices;
    static private List<Vector3> verticesDone = new List<Vector3>();    //list collects all unique vertices (icosphere1 has 12)
    //static private List<GameObject> clones = new List<GameObject>();    //list of cloned objects
    static private Mesh mesh;

    public string[] csvfilesArray; // = new string[] { "Cube", "Sphere", "Plane" };
    public int index;
    static string selectedCSV;

    // data from SciptableObject
    public string masterDirectory;
    public string lastImportSet;
    public DataSaver myData;     // scriptable object containing data


    [MenuItem("Tools/Setup Tools")]

    public static void ShowWindow()
    {
        GetWindow<CustomEditorWindow>("Setup Tools");

    }
    void OnGUI()
    {
        // init these... thisDatabase & itemDatabase should be init'ed to selectedCSV
        thisDatabase = GameObject.Find("Databases").GetComponent<LoadExcel>().itemDatabase; // list with subset = objects to create
        ball = GameObject.Find("Cube");                 // object to clone from

        Debug.Log("from Editor Script: " + myData);
        //lastImportSet = myData.lastImport;
        //masterDirectory = myData.sourceDirectory;

        Debug.Log(masterDirectory);
        Debug.Log(lastImportSet);

        GUILayout.Label("Populate Materials & Objects", EditorStyles.boldLabel);
        if (GUILayout.Button("Populate"))
        {
            Populate();
        }

        if (GUILayout.Button("Clean Up"))
        {
            Cleanup();
        }

        // rewrite, better label & descr. & configuration?
        GUILayout.Label("Copy graphics assets from master folder. This is a slow process!", EditorStyles.boldLabel);
        //GUILayout.Label("Last Import Set: " + GameObject.Find("Databases").GetComponent<DataContainer>().DataSaver);
        GUILayout.Label("Last Import Set: " + lastImportSet);
        //GameObject.Find("Databases").GetComponent<LoadExcel>().LoadItemData(selectedCSV);


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

        GUILayout.Label("Initialize Database from CSV file - must make selection!", EditorStyles.boldLabel);

        // show popup menu
        index = EditorGUILayout.Popup(index, csvfilesArray);    // popup returns index
        selectedCSV = csvfilesArray[index];
        GameObject.Find("Databases").GetComponent<LoadExcel>().LoadItemData(selectedCSV);


        if (GUILayout.Button("Import images from Master folder"))
        {
            ImportImages();
        }
    }

    /*  public void Initialize(DataSaver dataSaver)
     {
         sourceDirectory = dataSaver.sourceDirectory;
         lastImport = dataSaver.lastImport;
     }
  */

    // copy each jpg in database.graphic from source to assets/TempTextures
    private static void ImportImages()
    {
        DirectoryInfo sourceDi = new DirectoryInfo("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads/micro_ftu22_crop_200k");
        DirectoryInfo destDi = new DirectoryInfo("Assets/TempTextures");

        Debug.Log("Importing images from: Humanexus 2/Downloads/micro_ftu22_crop_200k");

        foreach (Item i in thisDatabase)
        {
            FileInfo[] fileInfos = sourceDi.GetFiles(i.graphic);
            foreach (FileInfo fileInfo in fileInfos)
            {
                fileInfo.CopyTo(Path.Combine(destDi.ToString(), fileInfo.Name), true);
                Debug.Log("Copying: " + fileInfo.Name);
            }
            AssetDatabase.Refresh();
        }
        GameObject.Find("Databases").GetComponent<LoadExcel>().lastImportSet = selectedCSV; // need this?
        //lastImportSet = 
        Debug.Log("Import done.");
    }



    private static void Populate()
    {
        GameObject materialRepo = GameObject.Find("MaterialRepo");  // parent where new materials are created
        GameObject goMatt = GameObject.Find("Matt");                // template object for material holder GO

        Debug.Log("Creating clones at vertices...");


        InitSphere();       // picks correct icosphere
        MakeVertexList();   // make vertex list, eliminate duplicates
        icosphere.GetComponent<SphereInfo>().clones.Clear();

        GameObject clone;
        int vertexCounter = 0;

        DirectoryInfo dirInfo = new DirectoryInfo("Assets/TempTextures");
        //thisDatabase = GameObject.Find("Databases").GetComponent<LoadExcel>().itemDatabase;

        foreach (Item item in thisDatabase)
        {
            Debug.Log(item.graphic);
            FileInfo[] fileInfos = dirInfo.GetFiles(item.graphic);

            foreach (FileInfo fileInfo in fileInfos)
            {
                Debug.Log(fileInfo.Name);
                string fullPath = fileInfo.FullName.Replace(@"\", "/");
                string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
                Texture2D tex2d = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;

                Material newMaterial = new(Shader.Find("Unlit/Texture"))
                {
                    mainTexture = tex2d
                };

                string savePath = "Assets/TempMaterials/";
                string newAssetName = savePath + tex2d.name + ".mat";

                AssetDatabase.CreateAsset(newMaterial, newAssetName);
                AssetDatabase.SaveAssets();

                clone = Instantiate(ball, verticesDone[vertexCounter], Quaternion.identity);   // make clone
                clone.transform.parent = icosphere.transform;             // into parent (vertexCloud)
                clone.transform.LookAt(icosphere.transform);                // impose tidal lock so clone always faces center
                clone.GetComponent<MeshRenderer>().material = newMaterial;  // assign new material to clone
                clone.name = tex2d.name;                                    // rename clone with name of texture/material
                icosphere.GetComponent<SphereInfo>().clones.Add(clone);

                vertexCounter++;
            }
        }

        Debug.Log("Done creating clones...");
    }

    private static void Cleanup()
    {
        Debug.Log("Cleaning up...");
        // delete all children of materialrepo
        GameObject materialRepo = GameObject.Find("MaterialRepo");  // parent where new materials are created
        for (int i = materialRepo.transform.childCount; i > 0; --i)
            Object.DestroyImmediate(materialRepo.transform.GetChild(0).gameObject);

        // delete all children of vertexcloud
        GameObject spheres = GameObject.Find("Spheres");    // parent where clones go
        foreach (Transform child in spheres.transform)
        {
            for (int i = child.childCount; i > 0; --i)
                Object.DestroyImmediate(child.transform.GetChild(0).gameObject);
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

        Debug.Log("Cleanup done.");
    }


    // find the icosphere that has more vertices than needed
    // disable mesh renderer on all spheres
    private static void InitSphere()
    {
        GameObject spheres = GameObject.Find("Spheres");

        // disable renderer on all spheres
        foreach (Transform child in spheres.transform)
        {
            child.GetComponent<MeshRenderer>().enabled = false;
        }

        // find sphere with more vertices than needed (this is not fool proof - icospheres have to be in ascending order)
        foreach (Transform child in spheres.transform)
        {
            int vCount = child.gameObject.GetComponent<SphereInfo>().vertexCount;

            if (vCount >= thisDatabase.Count)
            {
                icosphere = child.gameObject;
                break;
            }
        }

        Debug.Log("sphere = " + icosphere);
    }


    private static void MakeVertexList()
    {
        mesh = icosphere.GetComponent<MeshFilter>().sharedMesh;
        vertices = mesh.vertices;

        foreach (Vector3 vertex in vertices)
        {
            if (!verticesDone.Contains(vertex))
            {
                verticesDone.Add(vertex);
            }
        }

        Debug.Log("vertices before: " + vertices.Count());
        Debug.Log("Vertices after: " + verticesDone.Count);
    }
}
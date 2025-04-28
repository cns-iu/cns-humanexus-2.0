using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Linq;

// CustomEditorWindow for Humanexus 2.0
// Rev. 2025-4-27

public class CustomEditorWindow : EditorWindow
{
    static public GameObject icosphere;
    static public GameObject ball;
    static private GameObject vertexCloud;
    static private List<Item> thisDatabase = new List<Item>();

    static private Vector3[] vertices;
    static private List<Vector3> verticesDone = new List<Vector3>();    //list collects all unique vertices (icosphere1 has 12)
    //static private List<GameObject> clones = new List<GameObject>();    //list of cloned objects
    static private Mesh mesh;

    // to test pop-up for CSV selection
    public string[] csvfilesArray; // = new string[] { "Cube", "Sphere", "Plane" };
    public int index = 0;


    [MenuItem("Tools/Setup Tools")]
    public static void ShowWindow()
    {
        GetWindow<CustomEditorWindow>("Setup Tools");
    }
    void OnGUI()
    {
        // init these...
        thisDatabase = GameObject.Find("Databases").GetComponent<LoadExcel>().itemDatabase; // list with subset = objects to create
        ball = GameObject.Find("Cube");                 // object to clone from
        vertexCloud = GameObject.Find("VertexCloud");   // parent object to hold all clones (vertex cloud)

        // setup for popup menu
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
            j++;
        }

        GUILayout.Label("Initialize Database from CSV file", EditorStyles.boldLabel);
        // show popup menu
        index = EditorGUILayout.Popup(index, csvfilesArray);    // popup returns index
        string selectedCSV = csvfilesArray[index];
        GameObject.Find("Databases").GetComponent<LoadExcel>().LoadItemData(selectedCSV);

        GUILayout.Label("Populate Materials & Objects", EditorStyles.boldLabel);
        if (GUILayout.Button("Populate"))
        {
            Populate();
        }

        if (GUILayout.Button("Clean Up"))
        {
            Cleanup();
        }

        GUILayout.Label("Testing", EditorStyles.boldLabel);
        if (GUILayout.Button("Test 0 - copy external jpg files to project"))
        {
            Test0();
        }
    }


    // copy each jpg in database.graphic from source (test set 1000) to assets/TempTextures
    private static void Test0()
    {
        // sourceDi should point to "22ftu_micro_organ_metadata new.csv"
        DirectoryInfo sourceDi = new DirectoryInfo("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads/micro_ftu22_crop_200k");
        //DirectoryInfo sourceDi = new DirectoryInfo("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads/With third column/test_set_jpgs_1000");
        DirectoryInfo destDi = new DirectoryInfo("Assets/TempTextures");

        foreach (Item i in thisDatabase)
        {
            FileInfo[] fileInfos = sourceDi.GetFiles(i.graphic);
            foreach (FileInfo fileInfo in fileInfos)
            {
                fileInfo.CopyTo(Path.Combine(destDi.ToString(), fileInfo.Name), true);
            }
            AssetDatabase.Refresh();
        }
    }



    private static void Populate()
    {
        GameObject materialRepo = GameObject.Find("MaterialRepo");  // parent where new materials are created
        GameObject goMatt = GameObject.Find("Matt");                // template object for material holder GO

        Debug.Log("Importing assets...");

        InitSphere();       // picks correct icosphere
        MakeVertexList();   // make vertex list, eliminate duplicates
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
                clone.transform.parent = vertexCloud.transform;             // into parent (vertexCloud)
                clone.transform.LookAt(icosphere.transform);                // impose tidal lock so clone always faces center
                clone.GetComponent<MeshRenderer>().material = newMaterial;  // assign new material to clone
                clone.name = tex2d.name;                                    // rename clone with name of texture/material
                //clones.Add(clone);  

                vertexCounter++;
            }
        }

        Debug.Log("Done importing assets...");
    }
    private static void Cleanup()
    {
        // delete all children of materialrepo
        GameObject materialRepo = GameObject.Find("MaterialRepo");  // parent where new materials are created
        for (int i = materialRepo.transform.childCount; i > 0; --i)
            Object.DestroyImmediate(materialRepo.transform.GetChild(0).gameObject);

        // delete all children of vertexcloud
        GameObject vertexCloud = GameObject.Find("VertexCloud");    // parent where clones go
        for (int i = vertexCloud.transform.childCount; i > 0; --i)
            Object.DestroyImmediate(vertexCloud.transform.GetChild(0).gameObject);

        //------only delete stuff in the folder - not the folder itself
        /* List<string> failedPaths = new List<string>();
        string[] assetPaths = {"Assets/TempMaterials/"};
        
        AssetDatabase.DeleteAssets(assetPaths,failedPaths);
        Debug.Log(failedPaths); */
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
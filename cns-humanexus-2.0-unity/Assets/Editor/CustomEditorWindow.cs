using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Linq;

public class CustomEditorWindow : EditorWindow
{
    static public GameObject icosphere;
    static public GameObject ball;
    static private GameObject vertexCloud;
    static private List<Item> thisDatabase = new List<Item>();

    static private Vector3[] vertices;
    static private List<Vector3> verticesDone = new List<Vector3>();    //list collects all unique vertices (icosphere1 has 12)
    static private Mesh mesh;

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

        GUILayout.Label("Initialize Database", EditorStyles.boldLabel);
        if (GUILayout.Button("Load Items from CSV"))
        {
            GameObject.Find("Databases").GetComponent<LoadExcel>().LoadItemData();
            //--->build list of icospheres
        }

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
        if (GUILayout.Button("Test 1"))
        {
            Test1();
        }
    }

    // adds icosphere with vertices >= database.count 
    private static void Test1()
    {
        InitSphere();       // picks correct icosphere
        MakeVertexList();   // make vertex list, eliminate duplicates

        GameObject clone;
        int vertexCounter = 0;

        foreach (Item item in thisDatabase)
        {
            // clone "ball"
            clone = Instantiate(ball, verticesDone[vertexCounter], Quaternion.identity);   // make clone
            clone.transform.parent = vertexCloud.transform;             // into parent
            clone.transform.LookAt(icosphere.transform);              // impose tidal lock so clone always faces center

            // clone needs material!
            // 
            vertexCounter++;
        }

    }


    private static void Populate()
    {
        GameObject materialRepo = GameObject.Find("MaterialRepo");  // parent where new materials are created
        GameObject goMatt = GameObject.Find("Matt");                // template object for material holder GO

        Debug.Log("Importing assets...");
        GameObject clone;

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

                clone = UnityEngine.Object.Instantiate(goMatt);
                clone.GetComponent<MeshRenderer>().material = newMaterial;
                clone.name = tex2d.name;
                clone.transform.parent = materialRepo.transform;
            }
        }

        Debug.Log("Done importing assets...");
    }
    private static void Cleanup()
    {
        GameObject materialRepo = GameObject.Find("MaterialRepo");  // parent where new materials are created

        // delete all children of materialrepo
        for (int i = materialRepo.transform.childCount; i > 0; --i)
            Object.DestroyImmediate(materialRepo.transform.GetChild(0).gameObject);

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

        // find sphere with more vertices than needed (this is not fool proof)
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
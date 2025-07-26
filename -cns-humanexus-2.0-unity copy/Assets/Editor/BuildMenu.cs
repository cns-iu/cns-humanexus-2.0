using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using PlasticGui.WorkspaceWindow.BrowseRepository;

// 2025-6-11
// - Build From Current Set: builds a vertice cloud from installed set
// - Cleanup: removes created clones, tempMaterials
//

public class BuildMenu : EditorWindow
{
    static public GameObject icosphere;     // this is the sphere selected in InitSphere
    static public GameObject ball;
    static private List<Item> thisDatabase = new List<Item>();

    static private Vector3[] vertices;
    static private List<Vector3> verticesDone = new List<Vector3>();    //list collects all unique vertices (icosphere1 has 12)
    static private Mesh mesh;
    static private int nodeFactor;  // init'ed in InitSphere() used to determine if vertices are skipped for better distro
    static bool vertexDistribution = false;  // would be nice to set somewhere in menu or panel
    static bool showBtn;



    [MenuItem("Humanexus/Cloud Building/1 Build from Current Set")]
    static void Build()
    {
        thisDatabase = GameObject.Find("Databases").GetComponent<LoadExcel>().itemDatabase;
        if (thisDatabase.Count() != 0)
        {
            Debug.Log("Starting to build cloud...");
            Populate();
            Debug.Log("Done building cloud....");
        }
        else
        {
            Debug.LogError("No texture set installed: Manual Import->Build Database");
        }
    }

    [MenuItem("Humanexus/Cloud Building/2 Cloud Cleanup")]
    static void CloudCleanup()
    {
        Cleanup();
    }

    [MenuItem("Humanexus/Cloud Building/3 Vertex Distribution")]
    static void VertexDistro()
    {
        GetWindow<BuildMenu>("Vertex Distribution");
    }

    void OnGUI()
    {
        showBtn = EditorGUILayout.Toggle("Better Vertex Distribution", showBtn);
        if (showBtn)
        { vertexDistribution = true; }
        else { vertexDistribution = false; }
    }


    private static void Populate()
    {
        //GameObject materialRepo = GameObject.Find("MaterialRepo");  // parent where new materials are created
        //GameObject goMatt = GameObject.Find("Matt");                // template object for material holder GO
        GameObject ball = GameObject.Find("Ball");                  // object to clone (it is a cube)

        Debug.Log("Creating clones at vertices...VertexDistribution = " + vertexDistribution);

        InitSphere();       // picks correct icosphere
        MakeVertexList();   // make vertex list, eliminate duplicates
        //icosphere.GetComponent<SphereInfo>().clones.Clear();
        icosphere.GetComponent<SphereInfo>().cloneItems.Clear();    //------------

        GameObject clone;
        CloneItem cloneItem; // = new(clone, verticesDone[vertexCounter]);

        int vertexCounter = 0;

        DirectoryInfo dirInfo = new DirectoryInfo("Assets/TempTextures");
        //thisDatabase = GameObject.Find("Databases").GetComponent<LoadExcel>().itemDatabase;

        foreach (Item item in thisDatabase)
        {
            //Debug.Log(item.graphic);
            FileInfo[] fileInfos = dirInfo.GetFiles(item.graphic);

            foreach (FileInfo fileInfo in fileInfos)
            {
                // create a new material...
                Debug.Log(vertexCounter + ": " + fileInfo.Name);
                string fullPath = fileInfo.FullName.Replace(@"\", "/");
                string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
                Texture2D tex2d = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;

                //Material newMaterial = new(Shader.Find("Unlit/Texture"))
                Material newMaterial = new(Shader.Find("Universal Render Pipeline/Unlit"))
                {
                    mainTexture = tex2d

                };
                // this is to make material opaque-------
                newMaterial.SetFloat("_Surface", 1.0f);
                newMaterial.SetFloat("_Blend", 0.0f);

                Color baseColor = newMaterial.GetColor("_BaseColor");
                baseColor.a = 1.0f; // this value control amount of transparency 0-1f
                newMaterial.SetColor("_BaseColor", baseColor);

                newMaterial.SetFloat("_ReceiveShadows", 0.0f);

                newMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                newMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                newMaterial.SetInt("_ZWrite", 0);
                newMaterial.DisableKeyword("_ALPHATEST_ON");
                newMaterial.EnableKeyword("_ALPHABLEND_ON");
                newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                newMaterial.renderQueue = 3000;
                //--------------------



                string savePath = "Assets/TempMaterials/";
                string newAssetName = savePath + tex2d.name + ".mat";

                AssetDatabase.CreateAsset(newMaterial, newAssetName);
                AssetDatabase.SaveAssets();

                if (vertexDistribution)
                {
                    int skipNode = nodeFactor - 1;
                    while (skipNode != 0)
                    {
                        skipNode--;
                        vertexCounter++;
                    }
                }

                // clone cube at vertex
                clone = Instantiate(ball, verticesDone[vertexCounter], Quaternion.identity);   // make clone
                clone.transform.parent = icosphere.transform;               // into parent (vertexCloud)
                clone.transform.LookAt(icosphere.transform);                // impose tidal lock so clone always faces center
                clone.GetComponent<MeshRenderer>().material = newMaterial;  // assign new material to clone
                clone.GetComponent<MeshRenderer>().enabled = true;          // make visible
                clone.name = tex2d.name;                                    // rename clone with name of texture/material
                clone.GetComponent<CloneInfo>().cloneID = vertexCounter;    // ID of new clone

                // populate clone info; ID, GameObject, Vector3 pos, Vector3 rotation
                cloneItem = new(vertexCounter, clone, verticesDone[vertexCounter], clone.transform.rotation);
                icosphere.GetComponent<SphereInfo>().cloneItems.Add(cloneItem); // this should be on the sphereInfo

                vertexCounter++;
            }
        }
        ResizeCloud(icosphere.GetComponent<SphereInfo>().startDiameter);    // set startsize of cloud from icosphere SphereInfo
        //Debug.Log("start size = " + icosphere.GetComponent<SphereInfo>().startSize);
        Debug.Log("Done creating clones..." + vertexCounter);
    }


    private static void Cleanup()
    {
        Debug.Log("Cleaning up: GameObjects & TempMaterials");

        // delete all children of vertexcloud
        GameObject spheres = GameObject.Find("Spheres");    // parent where clones go
        foreach (Transform child in spheres.transform)
        {
            for (int i = child.childCount; i > 0; --i)
            {
                Object.DestroyImmediate(child.transform.GetChild(0).gameObject);
            }
            child.GetComponent<SphereInfo>().cloneItems.Clear();
        }

        // delete TempMaterials folder and all contents, then creates new empty folder
        List<string> failedPathsMat = new List<string>();
        string[] assetPathsMat = { "Assets/TempMaterials/" };

        AssetDatabase.DeleteAssets(assetPathsMat, failedPathsMat);
        AssetDatabase.Refresh();
        AssetDatabase.CreateFolder("Assets", "TempMaterials");

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

        // override automatic icosphere selection-------------
        //icosphere = GameObject.Find("icosphere 4");

        GameObject dataContainer = GameObject.Find("Databases");                // update info on databases
        dataContainer.GetComponent<DataContainer>().usedIcosphere = icosphere;

        nodeFactor = icosphere.GetComponent<SphereInfo>().vertexCount / thisDatabase.Count();
        Debug.Log("nodeFactor = " + nodeFactor);

        Debug.Log("sphere = " + icosphere);
    }


    private static void MakeVertexList()
    {
        mesh = icosphere.GetComponent<MeshFilter>().sharedMesh;
        vertices = mesh.vertices;

        verticesDone.Clear();
        icosphere.GetComponent<SphereInfo>().verticesDone.Clear();

        foreach (Vector3 vertex in vertices)
        {
            if (!verticesDone.Contains(vertex))
            {
                verticesDone.Add(vertex * 1);     // why we need *1? Normalizing?
                icosphere.GetComponent<SphereInfo>().verticesDone.Add(vertex * 1);  // build reference list on icosphere
            }
        }
        Debug.Log("verticesDone: " + verticesDone.Count());
    }


    // factor: 1=stays the same, <1 shrink, >1 expand
    private static void ResizeCloud(float factor)
    {
        // for resize to work:
        //  cloneItems <list>
        //  add clones created in EditorScript to this list
        //  can access for resizing
        GameObject currentClone;
        Vector3 savedVector;

        foreach (CloneItem ci in icosphere.GetComponent<SphereInfo>().cloneItems)   // get access to clone gameobjects though the list
        {
            Debug.Log("cloneItem = " + ci.CloneObject);
            currentClone = ci.CloneObject;
            savedVector = ci.CloneVector;

            currentClone.transform.localPosition = savedVector * factor;
        }
    }
}

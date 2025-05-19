using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using PlasticGui.WorkspaceWindow.BrowseRepository;

// 2025-5-12
// - Build From Current Set: builds a vertice cloud from installed set
// - Cleanup: removes created clones, tempMaterials
//

public class BuildMenu : MonoBehaviour
{
    static public GameObject icosphere;     // this is the sphere selected in InitSphere
    static public GameObject ball;
    static private List<Item> thisDatabase = new List<Item>();

    static private Vector3[] vertices;
    static private List<Vector3> verticesDone = new List<Vector3>();    //list collects all unique vertices (icosphere1 has 12)
    static private Mesh mesh;


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
            Debug.LogError("No texture set installed");
        }

    }

    [MenuItem("Humanexus/Cloud Building/2 Cloud Cleanup")]
    static void CloudCleanup()
    {
        Debug.Log("cleaning cloud");
        Cleanup();
    }


    private static void Populate()
    {
        //GameObject materialRepo = GameObject.Find("MaterialRepo");  // parent where new materials are created
        //GameObject goMatt = GameObject.Find("Matt");                // template object for material holder GO
        GameObject ball = GameObject.Find("Ball");                  // object to clone (it is a cube)

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
                clone.GetComponent<MeshRenderer>().enabled = true;          // make visible
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

        // delete all children of vertexcloud
        GameObject spheres = GameObject.Find("Spheres");    // parent where clones go
        foreach (Transform child in spheres.transform)
        {
            for (int i = child.childCount; i > 0; --i)
            {
                Object.DestroyImmediate(child.transform.GetChild(0).gameObject);
            }
            child.GetComponent<SphereInfo>().clones.Clear();        // clear clones list
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
        GameObject dataContainer = GameObject.Find("Databases");
        dataContainer.GetComponent<DataContainer>().usedIcosphere = icosphere;

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
    }

}

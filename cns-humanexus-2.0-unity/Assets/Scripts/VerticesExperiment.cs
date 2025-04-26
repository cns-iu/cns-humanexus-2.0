using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class VerticesExperiment : MonoBehaviour
{

    public GameObject icosphere;
    public GameObject ball;
    public float resizeFactor;
    public bool randomFlag;
    public bool halfFlag;
    public GameObject materialRepo;
    public GameObject mainCamera;
    public int vertexCounter;

    Vector3[] vertices;     // all vertices from icosphere are collected to here (many doubles, icosphere1 has 60)
    static private List<Vector3> verticesDone = new List<Vector3>();    //list collects all unique vertices (icosphere1 has 12)
    static private List<GameObject> clones = new List<GameObject>();    //list of cloned objects
    private List<GameObject> repoMaterials = new List<GameObject>(); // list of GO holding materials
    private Mesh mesh;




    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh; // extract mesh from object
        vertices = mesh.vertices;               // enter all vertices of mesh into array
        int itemsProcessed;

        GameObject go;
        foreach (Transform child in materialRepo.transform)
        {
            go = child.gameObject;
            repoMaterials.Add(go);
        }

        Debug.Log("vertices in icosphere object: " + vertices.Length);

        int materialCount = repoMaterials.Count();

        if (randomFlag)
        {
            itemsProcessed = randomBuild(materialCount);
        }
        else
        {
            itemsProcessed = strictBuild(materialCount);
        }

        GetComponent<Renderer>().enabled = false;       // hide icosphere
        ball.GetComponent<Renderer>().enabled = false;  // hide ball

        //GameObject mainCamera = GameObject.Find("Main Camera");

        Debug.Log("Item processed: " + itemsProcessed);
    }

    // builds all vertices in 
    // allow specific numbers of vertices....
    private int randomBuild(int materialCount)
    {
       vertexCounter = 1;
        GameObject clone;
        foreach (Vector3 vertex in vertices)
        {
            if (!verticesDone.Contains(vertex))                 // eliminate multiple vertices at same position
            {
                clone = Instantiate(ball, vertex, Quaternion.identity);   // make clone
                clone.transform.parent = transform;             // into parent
                clone.transform.LookAt(transform);              // impose tidal lock so clone always faces center
                verticesDone.Add(vertex);                       // add vertex to list so we can check for dups
                clones.Add(clone);                              // build list of instantiated game objects

                clone.GetComponent<MeshRenderer>().material = repoMaterials[Random.Range(0, materialCount)].GetComponent<MeshRenderer>().material;

                Debug.Log("(" + vertexCounter + ")" + vertex);
                vertexCounter++;

               /*  if (halfFlag)
                {
                    if ( vertexCounter*2 >= verticesDone.Count)
                    {
                        break;
                    }
                } */
            }
        }
        return vertexCounter;
    }

    // builds ONLY as many vertices as there are materials
    private int strictBuild(int materialCount)
    {
        // int vertexCounter = 1;
        int materialIndex = 0;
        GameObject clone;
        foreach (Vector3 vertex in vertices)
        {
            if (!verticesDone.Contains(vertex))
            {
                clone = Instantiate(ball, vertex, Quaternion.identity);   // make clone
                clone.transform.parent = transform;             // into parent
                clone.transform.LookAt(transform);              // impose tidal lock so clone always faces center
                verticesDone.Add(vertex);                       // add vertex to list so we can check for dups
                clones.Add(clone);                              // build list of instantiated game objects

                clone.GetComponent<MeshRenderer>().material = repoMaterials[materialIndex].GetComponent<MeshRenderer>().material;

                //Debug.Log("(" + vertexCounter + ")" + vertex);
                //vertexCounter++;
                if (materialIndex == materialCount - 1) return materialIndex;
                materialIndex++;

            }
        }
        return materialIndex;
    }


    // Update is called once per frame
    void Update()
    {
        // manual resizing of cloud
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ResizeCloud(1.2f);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ResizeCloud(0.8f);
        }

        // manual camera movement along Z
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            mainCamera.transform.Translate(0, 0, Time.deltaTime * 5);
            //mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            mainCamera.transform.Translate(0, 0, -Time.deltaTime * 5);
        }

        // hide/show icosphere object
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (icosphere.GetComponent<Renderer>().enabled == true)
            {
                icosphere.GetComponent<Renderer>().enabled = false;
            }
            else
            {
                icosphere.GetComponent<Renderer>().enabled = true;
            }
        }

        icosphere.transform.Rotate(0, 10 * Time.deltaTime, 0);      // constant rotation (move to UI?)
        float resizeValue = 1 + (resizeFactor * Time.deltaTime);    // evaluate resize factor

        ResizeCloud(resizeValue);

    }

    void ResizeCloud(float factor)
    {
        foreach (GameObject gameObject in clones)
        {
            gameObject.transform.position = gameObject.transform.position * factor;
        }

    }
}


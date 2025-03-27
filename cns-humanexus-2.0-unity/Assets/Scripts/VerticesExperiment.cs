using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VerticesExperiment : MonoBehaviour
{

    public GameObject icosphere;
    public GameObject ball;
    public float resizeFactor;
    public GameObject materialRepo;

    Vector3[] vertices;     // all vertices from icosphere are collected to here (many doubles, icosphere1 has 60)
    static private List<Vector3> verticesDone = new List<Vector3>();    //list collects all unique vertices (icosphere1 has 12)
    static private List<GameObject> clones = new List<GameObject>();    //list of cloned objects
    private Mesh mesh;


    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh; // extract mesh from object
        vertices = mesh.vertices;               // enter all vertices of mesh into array
        GameObject clone;

        Debug.Log("vertices in icosphere object: " + vertices.Length);

        int materialCount = materialRepo.GetComponent<MeshRenderer>().materials.Count();
        int vertexCounter = 1;
        foreach (Vector3 vertex in vertices)
        {
            if (!verticesDone.Contains(vertex))
            {
                clone = Instantiate(ball, vertex, Quaternion.identity);   // make clone
                clone.transform.parent = transform;             // into parent
                clone.transform.LookAt(transform);              // impose tidal lock so clone always faces center
                verticesDone.Add(vertex);                       // add vertex to list so we can check for dups
                clones.Add(clone);                              // build list of instantiated game objects

                // apply material
                //======does not work on cloned object?
                clone.GetComponent<MeshRenderer>().material = materialRepo.GetComponent<MeshRenderer>().materials[Random.Range(0, materialCount)];

                //m_Renderer.material.SetTexture("_MainTex", m_MainTexture);
                Debug.Log("(" + vertexCounter + ")" + vertex);
                vertexCounter++;
            }
        }
        GetComponent<Renderer>().enabled = false;       // hide icosphere
        ball.GetComponent<Renderer>().enabled = false;  // hide ball
    }


    // Update is called once per frame
    void Update()
    {
        // manual resizing of cloud
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ResizeCloud(1.2f);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ResizeCloud(0.8f);
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


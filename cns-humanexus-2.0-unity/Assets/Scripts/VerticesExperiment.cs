using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VerticesExperiment : MonoBehaviour
{

    public GameObject icosphere;
    public GameObject ball;
    Vector3[] vertices;
    static private List<Vector3> verticesDone = new List<Vector3>();    //list is used to omit multiple similar vertices
    static private List<GameObject> clones = new List<GameObject>();    //all clones are in here
    private Mesh mesh;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        GameObject clone;
        int vertexCounter = 1;
        Debug.Log("vertices in array: " + vertices.Length);

        foreach (Vector3 vertex in vertices)
        {
            if (!verticesDone.Contains(vertex))
            {
                clone = Instantiate(ball, vertex, Quaternion.identity);   // make clone
                clone.transform.parent = icosphere.transform;               // into parent
                verticesDone.Add(vertex);                                   // add vertex to list so we can check for dups
                clones.Add(clone);                                          // keep list of created game objects
                Debug.Log("(" + vertexCounter + ")" + vertex);
                vertexCounter++;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ResizeCloud(1.2f);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ResizeCloud(0.8f);
        }

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
    }

    void ResizeCloud(float factor)
    {
        foreach (GameObject gameObject in clones)
        {
            gameObject.transform.position = gameObject.transform.position * factor;
        }

    }
}


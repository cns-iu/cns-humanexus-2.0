using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 2025-5-13
// - added exposed autoResizeFactor & zoomFactor

public class SphereInfo : MonoBehaviour
{
    public int vertexCount;
    public float autoResizeFactor;
    public float zoomFactor;
    public float resizeFactor;
    public List<GameObject> clones = new List<GameObject>();    // clones list created from Editor script
    private GameObject mainCamera;
    static private List<Item> thisDatabase = new List<Item>();

    void Start()
    {
        // this script is on every icosphere GameObject. Which icosphere is actually used is stored usedIcosphere on Databases
        GameObject dataContainer = GameObject.Find("Databases");
        GameObject icosphere = dataContainer.GetComponent<DataContainer>().usedIcosphere;
        // make sure there is a built set
        if (icosphere.GetComponent<SphereInfo>().clones.Count() == 0)
        {
            Debug.LogError("No Built available. Use Build From Current Set first");
        }

        mainCamera = GameObject.Find("Main Camera");
        zoomFactor = 2.0f;
        resizeFactor = 0.1f;
    }

    void Update()
    {
        // manual resizing of cloud
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ResizeCloud(1 + resizeFactor);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ResizeCloud(1 - resizeFactor);
        }

        // manual camera movement along Z
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            mainCamera.transform.Translate(0, 0, Time.deltaTime * zoomFactor);
            //mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            mainCamera.transform.Translate(0, 0, -Time.deltaTime * zoomFactor);
        }

        // hide/show icosphere object
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GetComponent<Renderer>().enabled == true)
            {
                GetComponent<Renderer>().enabled = false;
            }
            else
            {
                GetComponent<Renderer>().enabled = true;
            }
        }

        transform.Rotate(0, 10 * Time.deltaTime, 0);      // constant rotation (move to UI?)
        float resizeValue = 1 + (autoResizeFactor * Time.deltaTime);    // evaluate resize factor

        ResizeCloud(resizeValue);

    }

    // factor: 1=stays the same, <1 shrink, >1 expand
    void ResizeCloud(float factor)
    {
        // for resize to work:
        //  clones <list>
        //  add clones created in EditorScript to this list
        //  can access for resizing
        foreach (GameObject gameObject in clones)
        {
            gameObject.transform.position = gameObject.transform.position * factor;
        }

    }
}

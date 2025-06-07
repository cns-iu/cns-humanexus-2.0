using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

// 2025-6-6

[Serializable]
public struct CloneItem
{
    //constructor
    public CloneItem(GameObject cloneObject, Vector3 cloneVector)
    {
        this.CloneObject = cloneObject;
        this.CloneVector = cloneVector;
    }
    public GameObject CloneObject;
    public Vector3 CloneVector;
}

public class SphereController : MonoBehaviour
{
    static private GameObject mainCamera;
    static private List<Item> thisDatabase = new();

    public float currentZoom;
    public float sizeMultiplier;
    public float cameraZStart;
    public float zoomFactor;
    public float startSize;
    public List<CloneItem> cloneItems; // = new();
    GameObject icosphere;


    void Start()
    {
        GameObject dataContainer = GameObject.Find("Databases");
        icosphere = dataContainer.GetComponent<DataContainer>().usedIcosphere;
        // make sure there is a built set
        if (icosphere.GetComponent<SphereInfo>().cloneItems.Count() == 0)
        {
            Debug.LogError("No Built available. Use Build From Current Set first");
        }

        mainCamera = GameObject.Find("Main Camera");

        // retrieve info from active icosphere - may not need all of these
        cameraZStart = icosphere.GetComponent<SphereInfo>().cameraZStart;
        zoomFactor = icosphere.GetComponent<SphereInfo>().zoomFactor;
        //sizeMultiplier = icosphere.GetComponent<SphereInfo>().sizeMultiplier;
        startSize = icosphere.GetComponent<SphereInfo>().startSize;
        cloneItems = icosphere.GetComponent<SphereInfo>().cloneItems;   // can we just get a pointer to the list on SphereInfo?

        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, cameraZStart);
        currentZoom = mainCamera.transform.position.z;
        sizeMultiplier = startSize;
        ResizeCloud(sizeMultiplier);
        mainCamera.transform.LookAt(icosphere.transform);
    }

    void Update()
    {
        // manual resizing of cloud
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            sizeMultiplier += 0.1f;
            ResizeCloud(sizeMultiplier);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            sizeMultiplier -= 0.1f;
            ResizeCloud(sizeMultiplier);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCloud();
        }

        // manual camera movement along Z
        if (Input.GetKeyDown(KeyCode.UpArrow))  // zoom in
        {
            mainCamera.transform.Translate(0, 0, zoomFactor);
            mainCamera.transform.LookAt(icosphere.transform);
            currentZoom = mainCamera.transform.position.z;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))    // zoom out
        {
            mainCamera.transform.Translate(0, 0, -zoomFactor);
            mainCamera.transform.LookAt(icosphere.transform);
            currentZoom = mainCamera.transform.position.z;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ResetCamera();
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

        icosphere.transform.Rotate(0, 10 * Time.deltaTime, 0);      // constant rotation (move to UI?)
        //float resizeValue = 1 + (autoResizeFactor * Time.deltaTime);    // evaluate resize factor
        //ResizeCloud(resizeFactor);
    }


    void ResetCloud()
    {
        sizeMultiplier = startSize;
        ResizeCloud(sizeMultiplier);
    }


    // factor: 1=stays the same, <1 shrink, >1 expand
    void ResizeCloud(float factor)
    {
        // for resize to work:
        //  cloneItems <list>
        //  add clones created in EditorScript to this list
        //  can access for resizing
        GameObject currentClone;
        Vector3 savedVector;

        foreach (CloneItem ci in cloneItems)
        {
            Debug.Log("cloneItem = " + ci.CloneObject);
            currentClone = ci.CloneObject;
            savedVector = ci.CloneVector;

            currentClone.transform.localPosition = savedVector * factor;
        }
    }


    void ResetCamera()
    {
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, cameraZStart);
        currentZoom = mainCamera.transform.position.z;
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEditor.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.TextCore.Text;
using System.Collections;

// 2025-6-20

[Serializable]
public struct CloneItem
{
    //constructor
    public CloneItem(int cloneID, GameObject cloneObject, Vector3 cloneVector)
    {
        this.CloneID = cloneID;
        this.CloneObject = cloneObject;
        this.CloneVector = cloneVector;
    }
    public int CloneID;
    public GameObject CloneObject;
    public Vector3 CloneVector;
}

public class SphereController : MonoBehaviour
{
    static private GameObject mainCamera;
    static private List<Item> thisDatabase = new();

    public float currentZoom;
    public float currentSizeMultiplier;
    public float cameraZStart;
    public float zoomFactor;
    public float startSize;
    public List<CloneItem> cloneItems; // = new();
    GameObject icosphere;
    static float currentTransparency = 1.0f;

    private PlayerControls playerControls;

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
        startSize = icosphere.GetComponent<SphereInfo>().startSize;
        cloneItems = icosphere.GetComponent<SphereInfo>().cloneItems;   // can we just get a pointer to the list on SphereInfo?

        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, cameraZStart);
        currentZoom = mainCamera.transform.position.z;
        currentSizeMultiplier = startSize;
        ResizeCloud(currentSizeMultiplier);
        mainCamera.transform.LookAt(icosphere.transform);
    }

    void Awake()
    {
        playerControls = new PlayerControls();
    }

    void Update()
    {
        // try this:
        // all stuff that is now triggered from 'On....' functions ->
        //  move to Update(), test if deltaTime can be used
        // 'On...' functions change values in 'parameter block'
        //  info in this block is evaluated in Update()
        // so, 'parameter block' always has 'soll' parameters
        //  functions called from Update() will try to 'achieve' 'soll' parameters over time!!

        // we can have complete, self-contained On... functions for global, un-timed operations (reset)
        // -> move individual clone ops in a loop in Update() - run each clone as Coroutine?
        icosphere.transform.Rotate(0, 10 * Time.deltaTime, 0);      // constant rotation (move to UI?)
    }

    void OnAlignRotation()
    {
        AlignRotation();
    }

    void OnSphereSizeIncrease()
    {
        currentSizeMultiplier += 0.1f;
        ResizeCloud(currentSizeMultiplier);
    }

    void OnSphereSizeDecrease()
    {
        currentSizeMultiplier -= 0.1f;
        ResizeCloud(currentSizeMultiplier);
    }

    void OnResetCloud()
    {
        ResetCloud();
    }

    void OnZoomIn()
    {
        mainCamera.transform.Translate(0, 0, zoomFactor);
        mainCamera.transform.LookAt(icosphere.transform);
        currentZoom = mainCamera.transform.position.z;
    }

    void OnZoomOut()
    {
        mainCamera.transform.Translate(0, 0, -zoomFactor);
        mainCamera.transform.LookAt(icosphere.transform);
        currentZoom = mainCamera.transform.position.z;
    }

    void OnZoomReset()
    {
        ResetZoom();
    }

    void OnHideAllClones()
    {
        HideAllClones();
    }

    void OnSphereTransparency()
    {
        if (currentTransparency <= 0)
        { currentTransparency = 1; }
        else
        { currentTransparency -= 0.1f; }
        SetTransparency();
    }

    void OnComplexTest()
    {
        currentTransparency = 0f;   // all invisible
        SetTransparency();

        currentTransparency = 0.5f;
        StartCoroutine(CoroutineTransparency());
        StartCoroutine(CouroutineExpand());
        StartCoroutine(CoroutineRotation());
        //CoroutineRotation();
        //StartCoroutine(CoroutineAlignRotation());
    }

    void OnShowIcosphere()
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



    IEnumerator CouroutineExpand()
    {
        foreach (CloneItem ci in cloneItems)
        {
            ci.CloneObject.transform.localPosition = ci.CloneVector * 0.6f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator CoroutineRotation()
    {
        GameObject currentClone;
        foreach (CloneItem ci in cloneItems)
        {
            currentClone = ci.CloneObject;
            currentClone.transform.Rotate(90.0f, 0.0f, 0.0f, Space.World);
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator CoroutineTransparency()
    {
        foreach (CloneItem ci in cloneItems)
        {
            SetTransparencySingle(ci.CloneObject, currentTransparency);
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator CoroutineAlignRotation()
    {
        foreach (CloneItem ci in cloneItems)
        {
            AlignRotationSingle(ci.CloneObject);
            yield return new WaitForSeconds(0.05f);
        }
    }

    void ResetCloud()
    {
        currentSizeMultiplier = startSize;
        currentTransparency = 1;

        ResizeCloud(currentSizeMultiplier);     // position of all clones to default

        foreach (CloneItem ci in cloneItems)
        {
            ci.CloneObject.transform.LookAt(icosphere.transform);       // rotation to default
            SetTransparencySingle(ci.CloneObject, currentTransparency); // transparency to default
        }
    }

    // factor: 1=stays the same, <1 shrink, >1 expand
    void ResizeCloud(float factor)
    {
        // for resize to work:
        //  cloneItems <list>
        //  add clones created in EditorScript to this list
        //  can access for resizing
        foreach (CloneItem ci in cloneItems)
        {
            ci.CloneObject.transform.localPosition = ci.CloneVector * factor;
        }
    }

    void ResetZoom()
    {
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, cameraZStart);
        currentZoom = mainCamera.transform.position.z;
    }

    void HideAllClones()
    {
        foreach (CloneItem ci in cloneItems)
        {
            if (ci.CloneObject.GetComponent<MeshRenderer>().enabled == false)
            { ci.CloneObject.GetComponent<MeshRenderer>().enabled = true; }
            else
            { ci.CloneObject.GetComponent<MeshRenderer>().enabled = false; }
        }
    }

    void SetTransparency()
    {
        foreach (CloneItem ci in cloneItems)
        {
            SetTransparencySingle(ci.CloneObject, currentTransparency);
        }
    }

    // sets gameobject c to transparency t
    void SetTransparencySingle(GameObject c, float t)
    {
        Material currentMat;
        Color baseColor;

        currentMat = c.GetComponent<Renderer>().material;
        baseColor = currentMat.GetColor("_BaseColor");
        baseColor.a = t; // this value control amount of transparency 0-1f
        currentMat.SetColor("_BaseColor", baseColor);
    }

    void AlignRotation()
    {
        foreach (CloneItem ci in cloneItems)
        {
            AlignRotationSingle(ci.CloneObject);
        }
    }

    void AlignRotationSingle(GameObject c)
    {
        c.transform.LookAt(icosphere.transform);       // rotation to default
    }
}

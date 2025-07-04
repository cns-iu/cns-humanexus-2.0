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

// 2025-7-4
// run live kewy controls in Update() loop

// try this:
// all stuff that is now triggered from 'On....' functions ->
//  move to Update(), test if deltaTime can be used
// 'On...' functions change values in 'parameter block'
//  info in this block is evaluated in Update()
// so, 'parameter block' always has 'soll' parameters
//  functions called from Update() will try to 'achieve' 'soll' parameters over time!!

// we can have complete, self-contained On... functions for global, un-timed operations (reset)
// -> move individual clone ops in a loop in Update() - run each clone as Coroutine?


[Serializable]
public struct CloneItem
{
    //constructor
    public CloneItem(int cloneID, GameObject cloneObject, Vector3 cloneVector, Quaternion cloneRotation)
    {
        this.CloneID = cloneID;
        this.CloneObject = cloneObject;
        this.CloneVector = cloneVector;
        this.CloneRotation = cloneRotation;
    }
    public int CloneID;
    public GameObject CloneObject;
    public Vector3 CloneVector;
    public Quaternion CloneRotation;
}

[Serializable]
public struct SequenceItem
{
    public SequenceItem(int sequenceDelta, string sequenceAction, float sequenceParameter1, float sequenceParameter2)
    {
        this.SequenceDelta = sequenceDelta;             // time since last action
        this.SequenceAction = sequenceAction;         // action type
        this.SequenceParamater1 = sequenceParameter1;   // magnitude, how much larger, further away etc. action-dependent
        this.SequenceParamater2 = sequenceParameter2;   // experimental -> speed of action (multiplier for deltaTime())
    }
    public int SequenceDelta;
    public string SequenceAction;
    public float SequenceParamater1;
    public float SequenceParamater2;
}

public class SphereController : MonoBehaviour
{
    static private GameObject mainCamera;
    //static private List<Item> thisDatabase = new();

    public float currentZoom;
    public float requestedZoom;
    public float requestedSizeMultiplier;
    public float cameraZStart;
    public float zoomFactor;
    public float startSize;
    public float currentOpacity = 1.0f;    // start with full opaque
    static float requestedOpacity = 1.0f;
    public float requestedRotation = 0;
    public float currentRotation = 0;
    public GameObject lookHere;    // this is opbject the clones are looking at
    public List<SequenceItem> sequenceItems;
    public List<CloneItem> cloneItems;
    public int currentSequenceItem = 0;
    static int sequenceDeltaTime = 0;
    GameObject icosphere;
    static float currentSizeMultiplier;
    static float sizeMultiplierTime = 1;
    static float zoomTime = 1;
    static float opacityTime = 1;
    static float rotationTime = 1;
    static float rotationFactor;
    public bool complexFlag = false;


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

        // retrieve info from active icosphere as defaults - may not need all of these
        cameraZStart = icosphere.GetComponent<SphereInfo>().cameraZStart;
        zoomFactor = icosphere.GetComponent<SphereInfo>().zoomFactor;
        startSize = icosphere.GetComponent<SphereInfo>().startSize;
        cloneItems = icosphere.GetComponent<SphereInfo>().cloneItems;   // can we just get a pointer to the list on SphereInfo?

        // populate working parameters
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, cameraZStart);
        currentZoom = mainCamera.transform.position.z;
        requestedZoom = currentZoom;
        currentSizeMultiplier = startSize;
        requestedSizeMultiplier = startSize;
        requestedOpacity = currentOpacity;

        // set cloud size and opcacity to defaults
        ResizeCloud(currentSizeMultiplier);
        mainCamera.transform.LookAt(icosphere.transform);

        lookHere = icosphere;   // center of sphere is default look at object

        // populate sequenceItems list
        // timeDelta since previous action, action type, action magnitude, action speed
        sequenceItems.Add(new SequenceItem(50, "sphere_opacity", 0.1f, 0.3f));
        sequenceItems.Add(new SequenceItem(200, "sphere_opacity", 1.0f, 0.3f));
        sequenceItems.Add(new SequenceItem(200, "sphere_size", -0.2f, 0.3f));
        sequenceItems.Add(new SequenceItem(0, "camera_zoom", 0.5f, 0.3f));

        sequenceItems.Add(new SequenceItem(50, "sphere_rotation", 45.0f, 0.5f));
        sequenceItems.Add(new SequenceItem(400, "sphere_opacity", 0.5f, 0.3f));
        sequenceItems.Add(new SequenceItem(200, "sphere_opacity", 0.0f, 0.3f));

        sequenceItems.Add(new SequenceItem(200, "sphere_opacity", 1.0f, 0.3f));
        sequenceItems.Add(new SequenceItem(0, "sphere_size", 0.4f, 0.3f));
        sequenceItems.Add(new SequenceItem(0, "sphere_rotation", -45.0f, 0.5f));
        sequenceItems.Add(new SequenceItem(0, "camera_zoom", -0.5f, 0.3f));

        sequenceItems.Add(new SequenceItem(200, "full_reset", 0.0f, 1.0f));
        sequenceItems.Add(new SequenceItem(10, "stop", 0, 0));

    }


    void Update()
    {
        // works like thermostat==========================================
        // modifies current parameters from requested parameters 
        // modify size, this is continually balancing.....
        if (currentSizeMultiplier < requestedSizeMultiplier)
        { currentSizeMultiplier += Time.deltaTime * sizeMultiplierTime; }
        if (currentSizeMultiplier > requestedSizeMultiplier)
        { currentSizeMultiplier -= Time.deltaTime * sizeMultiplierTime; }

        // modify Opacity==========================================
        /*         if (currentOpacity > requestedOpacity)
                { currentOpacity -= Time.deltaTime * opacityTime; }
                else
                { currentOpacity = requestedOpacity; } */
        if (currentOpacity != requestedOpacity)
        {
            if (currentOpacity > requestedOpacity)
            { currentOpacity -= Time.deltaTime * opacityTime; }
            if (currentOpacity < requestedOpacity)
            { currentOpacity += Time.deltaTime * opacityTime; }
        }

        // modify rotation==========================================
        if (currentRotation != requestedRotation)
        {
            rotationFactor = rotationTime;
            if (currentRotation < requestedRotation)
            { currentRotation += rotationFactor; }
            if (currentRotation > requestedRotation)
            { currentRotation -= rotationFactor; }
        }
        else { rotationFactor = 0; }

        // modify zoom (camera.position.z)==========================================
        if (currentZoom != requestedZoom)
        {
            if (currentZoom < requestedZoom)
            { currentZoom += Time.deltaTime * zoomTime; }
            if (currentZoom > requestedZoom)
            { currentZoom -= Time.deltaTime * zoomTime; }
        }

        // apply requested changes==========================================
        ModifyCloud(currentSizeMultiplier, currentOpacity, currentZoom, rotationFactor);

        //=============
        icosphere.transform.Rotate(0, 10 * Time.deltaTime, 0);      // constant rotation of sphere
    }

    // called from Update(); loops through all clones and applies all modifications
    void ModifyCloud(float s, float o, float c, float r)
    {
        // modify camera.z
        Vector3 zoomVector;

        zoomVector = mainCamera.transform.position;
        zoomVector.z = c;
        mainCamera.transform.position = zoomVector;
        mainCamera.transform.LookAt(icosphere.transform);
        currentZoom = mainCamera.transform.position.z;

        // apply individual clone mods
        foreach (CloneItem ci in cloneItems)
        {
            // translate clone
            ci.CloneObject.transform.localPosition = ci.CloneVector * s;

            // rotate
            if (lookHere != icosphere)
            { ci.CloneObject.transform.LookAt(lookHere.transform); }
            else
            { ci.CloneObject.transform.Rotate(r, 0.0f, 0.0f, Space.Self); }

            // set to requested opacity
            SetOpacitySingle(ci.CloneObject, o);
            //StartCoroutine(CoroutineModifyClone(ci, s, o, r)); // why running in Coroutine()?

        }
    }

    void FixedUpdate()
    {
        if (complexFlag)
        {
            int sTime;
            string sCommand;
            float sParameter1;
            float sParameter2;

            sTime = sequenceItems[currentSequenceItem].SequenceDelta;
            sCommand = sequenceItems[currentSequenceItem].SequenceAction;
            sParameter1 = sequenceItems[currentSequenceItem].SequenceParamater1;
            sParameter2 = sequenceItems[currentSequenceItem].SequenceParamater2;

            // this loop has to be set up above -
            // i.e. only one sequenceItem can be active at any given time
            // we pick up sequenceItem[n] and empy cycle for <time> FixedUpdates before actually executing command

            if (sequenceDeltaTime == sTime)
            {
                // do action
                Debug.Log("current sequence item " + currentSequenceItem + ", " + sCommand);

                switch (sCommand)
                {
                    case "sphere_size":
                        Debug.Log("doing sphere size...");
                        SetSphereSize(sParameter1, sParameter2);
                        break;
                    case "sphere_reset":
                        Debug.Log("doing sphere reset...");
                        SetSphereSizeReset();
                        break;
                    case "sphere_opacity":
                        Debug.Log("doing sphere opacity...");
                        SetSphereOpacity(sParameter1, sParameter2);
                        break;
                    case "sphere_rotation":
                        Debug.Log("doing sphere rotation...");
                        SetSphereRotation(sParameter1, sParameter2);
                        break;
                    case "camera_zoom":
                        Debug.Log("doing camera zoom...");
                        SetCameraZoom(sParameter1, sParameter2);
                        break;
                    case "full_reset":
                        Debug.Log("doing full reset...");
                        OnResetCloud();
                        break;
                    default:    // to catch stop
                        complexFlag = false;
                        currentSequenceItem = -1;   // lame!! just because currentSequenceItem must be 0 after switch!
                        break;
                }
                // end of command
                sequenceDeltaTime = 0;
                currentSequenceItem++;
            }
            else
            {
                sequenceDeltaTime++;
                //Debug.Log("sequence waiting for delta: " + sequenceDeltaTime);
            }
        }

    }





    //=============sequence commands=================================

    // s = add to current vector
    // t = multiplier for deltaTime()
    void SetSphereSize(float s, float t)
    {
        requestedSizeMultiplier = Mathf.Round((requestedSizeMultiplier += s) * 10) * 0.1f;
        sizeMultiplierTime = t;
    }

    void SetSphereSizeReset()
    {
        requestedSizeMultiplier = startSize;
        sizeMultiplierTime = 1;
    }

    // opacity o is absolite amount 0-1
    void SetSphereOpacity(float o, float t)
    {
        requestedOpacity = Mathf.Round((requestedOpacity = o) * 10) * 0.1f;   // round to n.0
        opacityTime = t;
    }

    // r = rotation along z by degrees from current 
    void SetSphereRotation(float r, float t)
    {
        requestedRotation = r;
        /*  if (requestedRotation >= 360f)
         { requestedRotation = 0; } */
        rotationTime = t;
    }

    // z = add to current zoom
    // t = multiplier for deltaTime()
    void SetCameraZoom(float z, float t)
    {
        requestedZoom = Mathf.Round((requestedZoom += z) * 10) * 0.1f;
        zoomTime = t;
    }


    //============keyboard commands=============================
    // flips sequencer flag; when this is TRUE FixedUpdate() plays the sequence
    void OnComplexTest()
    {
        // flip sequence on/off
        complexFlag = !complexFlag;
        // clear current sequence item
        if (complexFlag == false)
        { currentSequenceItem = 0; }
    }

    // <right arrow> increase size of clone cloud
    void OnSphereSizeIncrease()
    {
        requestedSizeMultiplier = Mathf.Round((requestedSizeMultiplier += 0.1f) * 10) * 0.1f;   // round to n.0
    }

    // <left arrow> decrease size of clone cloud
    void OnSphereSizeDecrease()
    {
        requestedSizeMultiplier = Mathf.Round((requestedSizeMultiplier -= 0.1f) * 10) * 0.1f;   // round to n.0
    }

    // <R> reset clone cloud size, opacity, zoom, (cancel any rotation)
    void OnResetCloud()
    {
        requestedSizeMultiplier = startSize;
        sizeMultiplierTime = 1;
        requestedOpacity = 1;
        currentOpacity = 1;
        rotationTime = 1;
        rotationFactor = 0;
        OnZoomReset();
        OnAlignRotation();
    }

    // <down arrow> camera zoom out; position.z
    void OnZoomIn()
    {
        requestedZoom = Mathf.Round((requestedZoom += zoomFactor) * 10) * 0.1f;
    }

    // <up arrow> camera zoom in; position.z
    void OnZoomOut()
    {
        requestedZoom = Mathf.Round((requestedZoom -= zoomFactor) * 10) * 0.1f;
    }

    // <Z> reset camera zoom; instantenous
    void OnZoomReset()
    {
        requestedZoom = cameraZStart;
        currentZoom = requestedZoom;
        zoomTime = 1;
    }

    // <H> hide/show all clones; disable/enable renderer
    void OnHideAllClones()
    {
        //HideAllClones();
        foreach (CloneItem ci in cloneItems)
        {
            if (ci.CloneObject.GetComponent<MeshRenderer>().enabled == false)
            { ci.CloneObject.GetComponent<MeshRenderer>().enabled = true; }
            else
            { ci.CloneObject.GetComponent<MeshRenderer>().enabled = false; }
        }
    }

    // <O> +/- requested opacity
    void OnSphereOpacity()
    {
        if (requestedOpacity <= 0)
        { requestedOpacity = 1; }
        else
        {
            //requestedOpacity -= 0.1f;
            requestedOpacity = Mathf.Round((requestedOpacity -= 0.1f) * 10) * 0.1f;   // round to n.0
        }
    }

    // <space> show/hide used icosphere
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

    // <L> look at ... camera
    void OnLookAt()
    {
        if (lookHere == mainCamera)
        { lookHere = icosphere; }
        else
        { lookHere = mainCamera; }

        AlignRotation();
    }

    // <T> rotate clones on x axis
    void OnRotation()
    {
        requestedRotation += 10.0f;
        if (requestedRotation >= 360f)
        { requestedRotation = 0; }
    }

    void OnAlignRotation()
    {
        AlignRotation();
    }



    //================================================
    // factor: 1=stays the same, <1 shrink, >1 expand
    void ResizeCloud(float factor)
    {
        foreach (CloneItem ci in cloneItems)
        {
            ci.CloneObject.transform.localPosition = ci.CloneVector * factor;
        }
    }

    // sets gameobject c to Opacity o
    void SetOpacitySingle(GameObject c, float o)
    {
        Material currentMat;
        Color baseColor;

        currentMat = c.GetComponent<Renderer>().material;
        baseColor = currentMat.GetColor("_BaseColor");
        baseColor.a = o; // this value control amount of Opacity 0-1f
        currentMat.SetColor("_BaseColor", baseColor);
    }

    void AlignRotation()
    {
        foreach (CloneItem ci in cloneItems)
        {
            AlignRotationSingle(ci.CloneObject);
            currentRotation = 0;
            requestedRotation = 0;
        }
    }

    void AlignRotationSingle(GameObject c)
    {
        c.transform.LookAt(lookHere.transform);       // rotation to default
    }



    //=========================not used


    IEnumerator CoroutineModifySingle(GameObject c, Vector3 cv)
    {
        c.transform.localPosition = cv * currentSizeMultiplier;

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator CouroutineExpand()
    {
        foreach (CloneItem ci in cloneItems)
        {
            ci.CloneObject.transform.localPosition = ci.CloneVector * 0.6f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    /*     IEnumerator CoroutineRotation()
        {
            GameObject currentClone;
            foreach (CloneItem ci in cloneItems)
            {
                currentClone = ci.CloneObject;
                currentClone.transform.Rotate(90.0f, 0.0f, 0.0f, Space.World);
                yield return new WaitForSeconds(0.05f);
            }
        } */

    IEnumerator CoroutineOpacity()
    {
        foreach (CloneItem ci in cloneItems)
        {
            SetOpacitySingle(ci.CloneObject, currentOpacity);
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


    IEnumerator CoroutineModifyClone(CloneItem ci, float s, float o, float r)
    {
        ci.CloneObject.transform.localPosition = ci.CloneVector * s;

        //currentClone = ci.CloneObject;
        ci.CloneObject.transform.Rotate(r, 0.0f, 0.0f, Space.World);

        SetOpacitySingle(ci.CloneObject, o);
        yield return new WaitForSeconds(0.01f);
    }
}

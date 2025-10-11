using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
/* using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEditor.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.TextCore.Text;
using System.Collections; */

// 2025-7-30

// 4. add actions to affect individual clones (use a marker flag on clonmeInfo?)


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
    public SequenceItem(int SequenceTime, string sequenceAction, float sequenceParameter1, int sequenceParameter2)
    {
        this.SequenceTime = SequenceTime;             // time since last action
        this.SequenceAction = sequenceAction;         // action type
        this.SequenceMagnitude = sequenceParameter1;   // magnitude, how much larger, further away etc. action-dependent
        this.SequenceDuration = sequenceParameter2;   // experimental -> speed of action (multiplier for deltaTime())
    }
    public int SequenceTime;
    public string SequenceAction;
    public float SequenceMagnitude;
    public int SequenceDuration;
}

public class SphereController : MonoBehaviour
{
    static private GameObject mainCamera;
    //static private List<Item> thisDatabase = new();
    [Header("Sequencer Parameters")]
    public float cloudDiameterTarget;
    public int cloudDiameterTimer;
    public float cloudDiameterSlice;
    public float cloudItemsOpacityTarget;
    public int cloudItemsOpacityTimer;
    public float cloudItemsOpacitySlice;
    public float cloudItemsRotationTarget;
    public int cloudItemsRotationTimer;
    public float cloudItemsRotationSlice;
    public float cloudRotationSpeedTarget;
    public int cloudRotationSpeedTimer;
    public float cloudRotationSpeedSlice;
    public float cameraZoomTarget;
    public int cameraZoomTimer;
    public float cameraZoomSlice;
    public int lookAtsIndex = 0;

    [Header("Operating Parameters")]
    public float currentZoom;
    public float requestedZoom;
    public float requestedDiameterMultiplier;   // 
    public float currentDiameterMultiplier;     // absolute value
    public float cameraZStart;
    //public float zoomFactor;
    public float startDiameter;
    public float currentOpacity = 1.0f;    // start with full opaque
    static float requestedOpacity = 1.0f;
    public float requestedRotation = 0;
    public float currentRotation = 0;
    public GameObject currentLookHere;    // this is opbject the clones are looking at
    //static GameObject requestedLookHere;
    public List<GameObject> lookAts;
    public List<SequenceItem> sequenceItems;
    public List<CloneItem> cloneItems;
    public int currentSequenceItem = 0;
    static int SequenceTimeTime = 0;
    public float lookAtTimer;
    public float currentCloudRotationSpeed = 0.1f;
    GameObject icosphere;
    static float rotationFactor;
    //static float arrayRotationDefault = 0.1f;
    public bool complexFlag = false;
    static int manualTimer = 20;    // timer used for manual operations
    static float manualAmount = 0.1f;
    static float manualRotationAmount = 10.0f;

    // stopwatch
    System.Diagnostics.Stopwatch st;

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
        //zoomFactor = icosphere.GetComponent<SphereInfo>().zoomFactor;
        startDiameter = icosphere.GetComponent<SphereInfo>().startDiameter;
        cloneItems = icosphere.GetComponent<SphereInfo>().cloneItems;   // can we just get a pointer to the list on SphereInfo?

        // populate working parameters
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, cameraZStart);
        currentZoom = mainCamera.transform.position.z;
        requestedZoom = currentZoom;
        currentDiameterMultiplier = startDiameter;
        requestedDiameterMultiplier = startDiameter;
        requestedOpacity = currentOpacity;

        CloudDiameterInit(startDiameter);

        // add lookAt target gameobjects
        lookAts.Add(icosphere);
        lookAts.Add(mainCamera);
        lookAts.Add(GameObject.Find("LookAtTarget1"));

        currentLookHere = lookAts[lookAtsIndex];
        mainCamera.transform.LookAt(currentLookHere.transform);   // look at item 0 in lookAts list

        PopulateSequence();

        //complexFlag = true;
    }

    void PopulateSequence()
    {
        // populate sequenceItems list============================
        // timeDelta since previous action, action type, action magnitude, action duration

        // add start time of each item to calculate duration of sequence

        // sequence 2
        // -- hide, diameter to 0.5, opacity = 0
        sequenceItems.Add(new SequenceItem(0, "cloud-hide", 0, 0));
        sequenceItems.Add(new SequenceItem(0, "cloud-diameter-init", 0.5f, 0));
        sequenceItems.Add(new SequenceItem(0, "cloud-items-opacity-init", 0.0f, 0));

        sequenceItems.Add(new SequenceItem(0, "timer-start", 0, 0));

        sequenceItems.Add(new SequenceItem(5, "cloud-show", 0, 0));
        sequenceItems.Add(new SequenceItem(5, "cloud-items-opacity", 1.0f, 150));
        sequenceItems.Add(new SequenceItem(0, "cloud-diameter-absolute", 1.8f, 150));


        sequenceItems.Add(new SequenceItem(100, "camera-zoom-absolute", 1.0f, 140));


        sequenceItems.Add(new SequenceItem(140, "timer-stop", 0, 0));

        // end sequence 2

        // sequence 1
        /* sequenceItems.Add(new SequenceItem(0, "start", 0, 0));
        sequenceItems.Add(new SequenceItem(0, "cloud-hide", 0, 0));
        sequenceItems.Add(new SequenceItem(0, "cloud-items-opacity-init", 0.0f, 0));
        sequenceItems.Add(new SequenceItem(0, "cloud-rotation-speed", 0.2f, 0));
        sequenceItems.Add(new SequenceItem(0, "cloud-diameter-init", 0.5f, 0));
        sequenceItems.Add(new SequenceItem(5, "cloud-show", 0, 0));
        sequenceItems.Add(new SequenceItem(10, "cloud-diameter-absolute", 1.8f, 200));
        sequenceItems.Add(new SequenceItem(15, "cloud-items-opacity", 1.0f, 150));

        sequenceItems.Add(new SequenceItem(150, "look-here", 1, 200));
        sequenceItems.Add(new SequenceItem(100, "camera-zoom-absolute", 3.9f, 400));

        sequenceItems.Add(new SequenceItem(200, "stop", 0, 0)); */
        // end sequence 1

        sequenceItems.Add(new SequenceItem(500, "sequence-end", 0, 0));

    }


    void Update()
    {
        // works like thermostat==========================================
        // modifies current parameters from requested parameters

        // modify cloud diameter================
        if (cloudDiameterTimer != 0)
        {
            //Debug.Log("sphereSize_timer: " +     cloudDiameterTimer);
            currentDiameterMultiplier += cloudDiameterSlice;
            cloudDiameterTimer--;

            if (cloudDiameterTimer == 0)
            { currentDiameterMultiplier = cloudDiameterTarget; }
        }

        // modify Opacity==========================================
        if (cloudItemsOpacityTimer != 0)
        {
            currentOpacity += cloudItemsOpacitySlice;
            cloudItemsOpacityTimer--;

            if (cloudItemsOpacityTimer == 0)
            { currentOpacity = cloudItemsOpacityTarget; }
        }

        // modify rotation==========================================
        if (cloudItemsRotationTimer != 0)
        {
            currentRotation += cloudItemsRotationSlice;
            rotationFactor = cloudItemsRotationSlice;
            cloudItemsRotationTimer--;

            if (cloudItemsRotationTimer == 0)
            {
                currentRotation = cloudItemsRotationTarget;
                rotationFactor = 0;
            }
        }

        // evaluate lookHere
        if (currentLookHere != lookAts[lookAtsIndex])
        {
            Debug.Log("new lookAts index: " + lookAtsIndex);
            currentLookHere = lookAts[lookAtsIndex];
        }

        // modify zoom (camera.position.z)==========================================
        if (cameraZoomTimer != 0)
        {
            currentZoom += cameraZoomSlice;
            cameraZoomTimer--;

            if (cameraZoomTimer == 0)
            { currentZoom = cameraZoomTarget; }
        }

        // set rotation speed of icosphere
        if (currentCloudRotationSpeed != cloudRotationSpeedTarget)
        {
            currentCloudRotationSpeed += cloudRotationSpeedSlice;
            cloudRotationSpeedTimer--;

            if (cloudRotationSpeedTimer == 0)
            { currentCloudRotationSpeed = cloudRotationSpeedTarget; }
        }

        // apply requested changes==========================================
        ModifyCloud(currentDiameterMultiplier, currentOpacity, currentZoom, rotationFactor, currentLookHere);

    }

    // called from Update(); loops through all clones and applies all modifications
    void ModifyCloud(float s, float o, float c, float r, GameObject l)
    {
        // modify camera.z
        Vector3 zoomVector;

        zoomVector = mainCamera.transform.position;
        zoomVector.z = c;
        mainCamera.transform.position = zoomVector;
        mainCamera.transform.LookAt(icosphere.transform);
        currentZoom = mainCamera.transform.position.z;

        // rotate the cloud
        icosphere.transform.Rotate(0, currentCloudRotationSpeed, 0);

        // apply individual clone mods
        foreach (CloneItem ci in cloneItems)
        {
            // translate vector multiplication
            ci.CloneObject.transform.localPosition = ci.CloneVector * s;

            // look here alignment
            var lRotation = Quaternion.LookRotation(l.transform.position - ci.CloneObject.transform.position);
            ci.CloneObject.transform.rotation = Quaternion.Slerp(ci.CloneObject.transform.rotation, lRotation, lookAtTimer);

            // rotate by degrees
            ci.CloneObject.transform.Rotate(r, 0.0f, 0.0f, Space.Self);

            // set to requested opacity
            SetOpacitySingle(ci.CloneObject, o);
        }
    }

    // FixedUpdate() processes the SequenceItemss list if complexFlag is ON
    void FixedUpdate()
    {
        if (complexFlag)
        {
            int sTime;
            string sAction;
            float sMagnitude;
            int sDuration;

            // pick this action apart
            sTime = sequenceItems[currentSequenceItem].SequenceTime;
            sAction = sequenceItems[currentSequenceItem].SequenceAction;
            sMagnitude = sequenceItems[currentSequenceItem].SequenceMagnitude;
            sDuration = sequenceItems[currentSequenceItem].SequenceDuration;

            // duration should not be 0 ro prevent division-by-zero
            if (sDuration == 0)
            { sDuration = 1; }

            // i.e. only one sequenceItem can be active at any given time
            // we pick up sequenceItem[n] and empty cycle for <time> FixedUpdates before actually executing command

            if (SequenceTimeTime == sTime)
            {
                // do action
                Debug.Log(currentSequenceItem + ": " + sTime + ", " + sAction + ", " + sMagnitude + ", " + sDuration);

                switch (sAction)
                {
                    case "cloud-diameter-init":
                        CloudDiameterInit(sMagnitude);
                        break;
                    case "cloud-items-opacity-init":
                        CloudItemsOpacityInit(sMagnitude);
                        break;
                    case "cloud-items-rotation-init":
                        CloudItemsRotationInit(sMagnitude);
                        break;
                    case "cloud-rotation-speed-init":
                        CloudRotationSpeedInit(sMagnitude);
                        break;
                    case "cloud-diameter-relative":
                        CloudDiameterDiameterRelative(sMagnitude, sDuration);
                        break;
                    case "cloud-diameter-absolute":
                        CloudDiameterDiameterAbsolute(sMagnitude, sDuration);
                        break;
                    case "cloud-diameter-reset":
                        CloudDiameterDiameterReset();
                        break;
                    case "cloud-items-opacity":
                        CloudItemsOpacity(sMagnitude, sDuration);
                        break;
                    case "cloud-items-rotation":
                        CloudItemsRotation(sMagnitude, sDuration);
                        break;
                    case "cloud-show":
                        CloudShow();
                        break;
                    case "cloud-hide":
                        CloudHide();
                        break;
                    case "cloud-rotation-speed":
                        CloudRotationSpeed(sMagnitude, sDuration);
                        break;

                    case "camera-zoom-init":
                        CameraZoomInit(sMagnitude);
                        break;
                    case "camera-zoom-relative":
                        CameraZoomRelative(sMagnitude, sDuration);
                        break;
                    case "camera-zoom-absolute":
                        CameraZoomAbsolute(sMagnitude, sDuration);
                        break;

                    case "look-here":
                        LookHere((int)sMagnitude, sDuration);    // sMagnitude is float by default but needs int for index here
                        break;

                    case "icosphere-show":
                        IcosphereShow();
                        break;
                    case "icosphere-hide":
                        IcosphereHide();
                        break;

                    case "full-reset":
                        OnFullReset();
                        break;
                    case "timer-start":
                        TimerStart();
                        break;
                    case "timer-stop":
                        TimerStop();
                        break;
                    case "sequence-end":
                        SequenceEnd();
                        break;
                    default:    // to catch stop
                        Debug.Log("+++++++++ illegal action request: " + sAction);
                        complexFlag = false;
                        currentSequenceItem = -1;   // lame!! just because currentSequenceItem must be 0 after switch!
                        break;
                }
                // end of command
                SequenceTimeTime = 0;
                currentSequenceItem++;
            }
            else
            {
                SequenceTimeTime++;
                //Debug.Log("sequence waiting for delta: " + SequenceTimeTime);
            }
        }

    }



    //=============sequence commands=================================

    // no delay sphere size
    void CloudDiameterInit(float s)
    {
        requestedDiameterMultiplier = s;
        currentDiameterMultiplier = s;
    }

    // no delay opacity
    void CloudItemsOpacityInit(float o)
    {
        requestedOpacity = o;
        currentOpacity = o;
    }

    // r = rotation along z by degrees from current 
    void CloudItemsRotationInit(float r)
    {
        AlignRotation(r);
        requestedRotation = r;
        currentRotation = r;
    }

    // set rotation speed of sphere immediately
    void CloudRotationSpeedInit(float s)
    {
        currentCloudRotationSpeed = s;
        cloudRotationSpeedTarget = currentCloudRotationSpeed;
    }

    // s = add to current vector
    // t = multiplier for deltaTime()
    void CloudDiameterDiameterRelative(float s, int t)
    {
        cloudDiameterTarget = currentDiameterMultiplier + s;
        cloudDiameterTimer = t;
        cloudDiameterSlice = s / cloudDiameterTimer;
    }

    // s = absolute size
    // t = multiplier for deltaTime()
    void CloudDiameterDiameterAbsolute(float s, int t)
    {
        cloudDiameterTarget = s;
        cloudDiameterTimer = t;
        cloudDiameterSlice = (cloudDiameterTarget - currentDiameterMultiplier) / cloudDiameterTimer;
    }

    void CloudDiameterDiameterReset()
    {
        requestedDiameterMultiplier = startDiameter;
        currentDiameterMultiplier = startDiameter;
    }

    // opacity o is absolite amount 0-1
    void CloudItemsOpacity(float o, int t)
    {
        cloudItemsOpacityTarget = o;
        cloudItemsOpacityTimer = t;
        cloudItemsOpacitySlice = (cloudItemsOpacityTarget - currentOpacity) / cloudItemsOpacityTimer;
    }

    // r = absolute rotation along z by degrees 
    void CloudItemsRotation(float r, int t)
    {
        cloudItemsRotationTarget = r;
        cloudItemsRotationTimer = t;
        cloudItemsRotationSlice = (cloudItemsRotationTarget - currentRotation) / cloudItemsRotationTimer;
    }

    // show all clones (enable renderer)
    void CloudShow()
    {
        foreach (CloneItem ci in cloneItems)
        {
            ci.CloneObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    // show all clones (enable renderer)
    void CloudHide()
    {
        foreach (CloneItem ci in cloneItems)
        {
            ci.CloneObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    // set rotation speed of sphere
    void CloudRotationSpeed(float s, int t)
    {
        cloudRotationSpeedTarget = s;
        cloudRotationSpeedTimer = t;
        cloudRotationSpeedSlice = (cloudRotationSpeedTarget - currentCloudRotationSpeed) / cloudRotationSpeedTimer;
    }

    // no delay camera zoom
    void CameraZoomInit(float z)
    {
        requestedZoom = z;
        currentZoom = z;
    }

    // z = add to current zoom
    // t = multiplier for deltaTime()
    void CameraZoomRelative(float z, int t)
    {
        cameraZoomTarget = currentZoom + z;
        cameraZoomTimer = t;
        cameraZoomSlice = z / cameraZoomTimer;
    }

    // z = absolute zoom
    // t = multiplier for deltaTime()
    void CameraZoomAbsolute(float z, int t)
    {
        cameraZoomTarget = z;
        cameraZoomTimer = t;
        cameraZoomSlice = (cameraZoomTarget - currentZoom) / cameraZoomTimer;
    }

    // i = index in lookAts list
    void LookHere(int i, int t)
    {
        if (i < lookAts.Count)
        {
            lookAtsIndex = i;
            lookAtTimer = 8.0f / (float)t;
        }
    }

    // show icosphere
    void IcosphereShow()
    {
        icosphere.GetComponent<Renderer>().enabled = true;
    }

    // hide icosphere
    void IcosphereHide()
    {
        icosphere.GetComponent<Renderer>().enabled = false;
    }

    void TimerStart()
    {
        st = new System.Diagnostics.Stopwatch();   // start timer for import
        st.Start();
        Debug.Log("stopwatch timer started");
    }

    void TimerStop()
    {
        st.Stop();
        Debug.Log("stopwatch timer stopped");
        Debug.Log("Duration of sequence in milliseconds: " + st.ElapsedMilliseconds);
    }

    void SequenceEnd()
    {
        complexFlag = false;
        currentSequenceItem = -1;   // lame!! just because currentSequenceItem must be 0 after switch!
        ClearParameterBlock();
        st.Stop();
    }

    void ClearParameterBlock()
    {
        /* sphereSize_requested = 0;
        sphereSize_current = 0;
        sphereSize_target = 0;
        sphereSize_timer = 0;
        sphereSize_slice = 0; */
    }


    //============keyboard commands=============================

    // flips sequencer flag; when this is TRUE FixedUpdate() plays the sequence
    void OnComplexSequence()
    {
        // flip sequence on/off
        complexFlag = !complexFlag;

        // reset current sequence item
        if (complexFlag == false)
        { currentSequenceItem = 0; }
    }

    // <right arrow> increase size of clone cloud
    void OnSphereDiameterIncrease()
    {
        CloudDiameterDiameterRelative(manualAmount, manualTimer);
    }

    // <left arrow> decrease size of clone cloud
    void OnSphereDiameterDecrease()
    {
        CloudDiameterDiameterRelative(-manualAmount, manualTimer);
    }

    // <R> reset clone cloud size, opacity, zoom, (cancel any rotation)
    void OnFullReset()
    {
        // diameter to default
        //requestedDiameterMultiplier = startDiameter;
        //currentDiameterMultiplier = startDiameter;
        CloudDiameterDiameterReset();

        // cloud rotation to default
        //currentCloudRotationSpeed = 0.1f;
        //cloudRotationSpeedTarget = currentCloudRotationSpeed;
        CloudRotationSpeedInit(0.1f);

        // items opacity to 100%
        //requestedOpacity = 1.0f;
        //currentOpacity = 1.0f;
        CloudItemsOpacityInit(1.0f);

        // camera zoom to default
        //OnZoomReset();
        CameraZoomInit(cameraZStart);

        // look at item 0
        LookHere(0, 1);

        // reset items rotation
        CloudItemsRotationInit(0.0f);
    }

    // <down arrow> camera zoom out; position.z
    void OnZoomIn()
    {
        CameraZoomRelative(manualAmount, manualTimer);
    }

    // <up arrow> camera zoom in; position.z
    void OnZoomOut()
    {
        CameraZoomRelative(-manualAmount, manualTimer);
    }

    // <Z> reset camera zoom; instantenous
    void OnZoomReset()
    {
        cameraZoomTarget = cameraZStart;
        cameraZoomTimer = manualTimer;
        cameraZoomSlice = (cameraZoomTarget - currentZoom) / cameraZoomTimer;
    }

    // <H> hide/show all clones; disable/enable renderer
    void OnHideAllClones()
    {
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
        cloudItemsOpacityTarget = Mathf.Round((currentOpacity - manualAmount) * 10) * 0.1f;
        if (cloudItemsOpacityTarget < 0)     // wrap around 
        { cloudItemsOpacityTarget = 1.0f; }

        CloudItemsOpacity(cloudItemsOpacityTarget, manualTimer);
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

    // look at gameobject o on lookAts list
    void OnLookHere0()
    {
        LookHere(0, manualTimer * 10);
    }

    // look at gameobject o on lookAts list
    void OnLookHere1()
    {
        LookHere(1, manualTimer * 10);
    }

    // look at gameobject o on lookAts list
    void OnLookHere2()
    {
        LookHere(2, manualTimer * 10);
    }

    // <T> rotate clones on x axis
    void OnRotation()
    {
        float r;
        r = manualRotationAmount + currentRotation;
        if (r >= 360f)
        { r = 0; }
        CloudItemsRotation(r, manualTimer);
    }

    //================================================

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

    void AlignRotation(float r)
    {
        foreach (CloneItem ci in cloneItems)
        {
            AlignRotationSingle(ci.CloneObject, r);
            currentRotation = r;
            requestedRotation = r;
        }
    }

    void AlignRotationSingle(GameObject c, float r)
    {
        c.transform.LookAt(currentLookHere.transform);       // rotation to default
        c.transform.Rotate(r, 0.0f, 0.0f, Space.Self);
    }

}

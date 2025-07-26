using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

// 2025-7-23
// - most of this code needs to be in one SINGLE script on databases (scenecontroller)
// keep ONLY specific data for THIS icosphere!!!
//
// <arrow l/r> decrease/increase size of cloud
// <arrow up/down> zoom in/out
// <space> show/hide icosphere
// <r> reset size of cloud (set to 1)

public class SphereInfo : MonoBehaviour
{
    public int vertexCount;
    public float cameraZStart; // camera position.z where this icosphere fits on screen
    public float startDiameter;
    public List<Vector3> verticesDone = new();

    [SerializeField]
    public List<CloneItem> cloneItems = new();



}

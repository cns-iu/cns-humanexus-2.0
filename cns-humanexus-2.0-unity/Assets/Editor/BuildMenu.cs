using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using PlasticGui.WorkspaceWindow.BrowseRepository;

public class BuildMenu : MonoBehaviour
{
    [MenuItem("Humanexus/Cloud Building/1 Build from Current Set")]
    static void Build()
    {
        //Debug.Log("building cloud");

    }

    [MenuItem("Humanexus/Cloud Building/2 Cloud Cleanup")]
    static void CloudCleanup()
    {
        Debug.Log("cleaning cloud");
    }
}

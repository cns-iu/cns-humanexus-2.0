using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;

// Populate iterates through Assets/Textures folder
// creates new material for each texture, in Assets/TempMaterials
// clones template gameobject and adds the new material
public class MenuItems
{
    [MenuItem("Tools/1 Populate")]
    private static void Populate()
    {
        GameObject materialRepo = GameObject.Find("MaterialRepo");  // parent where new materials are created
        GameObject goMatt = GameObject.Find("Matt");                // template object for material holder GO

        Debug.Log("Importing assets...");

        if ((goMatt != null) && (materialRepo != null))             // only loop if both GO are present
        {
            GameObject clone;

            DirectoryInfo dirInfo = new DirectoryInfo("Assets/TempTextures");
            FileInfo[] fileInfos = dirInfo.GetFiles("*.jpg");

            foreach (FileInfo fileInfo in fileInfos)
            {
                Debug.Log(fileInfo.Name);
                string fullPath = fileInfo.FullName.Replace(@"\", "/");
                string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
                Texture2D tex2d = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;

                Material newMaterial = new(Shader.Find("Unlit/Texture"))
                {
                    mainTexture = tex2d
                };

                string savePath = "Assets/TempMaterials/";
                string newAssetName = savePath + tex2d.name + ".mat";

                AssetDatabase.CreateAsset(newMaterial, newAssetName);
                AssetDatabase.SaveAssets();

                clone = UnityEngine.Object.Instantiate(goMatt);
                clone.GetComponent<MeshRenderer>().material = newMaterial;
                clone.name = tex2d.name;
                clone.transform.parent = materialRepo.transform;
            }
        }
        Debug.Log("Done importing assets...");
    }

    [MenuItem("Tools/2 Cleanup")]
    private static void Cleanup()
    {
        GameObject materialRepo = GameObject.Find("MaterialRepo");  // parent where new materials are created

        // delete all children of materialrepo
        for (int i = materialRepo.transform.childCount; i > 0; --i)
            Object.DestroyImmediate(materialRepo.transform.GetChild(0).gameObject);

        //------only delete stuff in the folder - not the folder itself
        /* List<string> failedPaths = new List<string>();
        string[] assetPaths = {"Assets/TempMaterials/"};
        
        AssetDatabase.DeleteAssets(assetPaths,failedPaths);
        Debug.Log(failedPaths); */

    }
}



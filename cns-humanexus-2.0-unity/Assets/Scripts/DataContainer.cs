using UnityEngine;

public class DataContainer : MonoBehaviour
{
    public string masterDirectory;
    public string lastImportSet;

    public DataSaver myData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // sourceDirectory = DataSaver.sourceDirectory;
        lastImportSet = myData.lastImport;
        masterDirectory = myData.sourceDirectory;
        Debug.Log("myData = " + myData);

    }

    // Update is called once per frame
    void Update()
    {

    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "NewDataSaver", menuName = "Scriptable Objects/NewDataSaver")]
public class NewDataSaver : ScriptableObject
{
    public string sourceDirectory;
    public string lastImport;
}

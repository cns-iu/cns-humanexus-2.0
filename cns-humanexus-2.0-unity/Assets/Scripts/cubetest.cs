using UnityEngine;

public class cubetest : MonoBehaviour
{

    public GameObject target1;
    public GameObject target2;

    public float rotator = 0.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target1 = GameObject.Find("target1");
        target2 = GameObject.Find("target2");
        transform.LookAt(target1.transform);
    }

    // Update is called once per frame
    void Update()
    {
       


        var rotation = Quaternion.LookRotation(target1.transform.position - transform.position);
        // rotation.x = 0; This is for limiting the rotation to the y axis. I needed this for my project so just
        // rotation.z = 0;                 delete or add the lines you need to have it behave the way you want.
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation,  0.1f);

    }
}

using UnityEngine;
using System.Collections.Generic;

public class SphereInfo : MonoBehaviour
{

    public int vertexCount;
    public GameObject mainCamera;
    public float resizeFactor;
    


    void Start()
    {

    }

    void Update()
    {
        // manual resizing of cloud
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ResizeCloud(1.2f);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ResizeCloud(0.8f);
        }

        // manual camera movement along Z
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            mainCamera.transform.Translate(0, 0, Time.deltaTime * 5);
            //mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            mainCamera.transform.Translate(0, 0, -Time.deltaTime * 5);
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

        transform.Rotate(0, 10 * Time.deltaTime, 0);      // constant rotation (move to UI?)
        float resizeValue = 1 + (resizeFactor * Time.deltaTime);    // evaluate resize factor

        ResizeCloud(resizeValue);

    }

    void ResizeCloud(float factor)
    {
        // for resize to work:
        //  clones <list> on vertexCloud
        //  add clones created in EditorScript to this list
        //  SPhereInfo (this) can access for resizing
        /*foreach (GameObject gameObject in clones)
        {
            gameObject.transform.position = gameObject.transform.position * factor;
        }*/

    }
}

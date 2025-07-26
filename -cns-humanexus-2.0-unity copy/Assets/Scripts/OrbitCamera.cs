using UnityEngine;

/// <summary>
/// A class to enable orbit camera control
/// </summary>
public class CameraOrbit : MonoBehaviour
{
    public Transform target; // The target to orbit around
    public float rotationSpeed = 100.0f; // Speed of rotation
    public float panSpeed = 20.0f; // Speed of panning
    public float zoomSpeed = 10.0f; // Speed of zooming

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target not assigned!");
            return;
        }
    }

    private void Update()
    {
        HandleRotation();
        HandlePan();
        HandleZoom();
    }

    // Rotate the camera with the left mouse button
    private void HandleRotation()
    {
        if (Input.GetMouseButton(0)) // Left mouse button
        {
            float horizontal = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float vertical = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            // Rotate the camera around the target
            transform.RotateAround(target.position, Vector3.up, horizontal); // Rotate horizontally
            transform.RotateAround(target.position, transform.right, -vertical); // Rotate vertically
        }
    }

    // Pan the camera with the right mouse button
    private void HandlePan()
    {
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float moveX = Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            float moveY = Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;

            // Move the camera based on mouse movement
            transform.Translate(new Vector3(-moveX, -moveY, 0), Space.World);
        }
    }

    // Zoom the camera with the mouse wheel
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            GetComponent<Camera>().fieldOfView -= scroll * zoomSpeed;
        }
    }
}

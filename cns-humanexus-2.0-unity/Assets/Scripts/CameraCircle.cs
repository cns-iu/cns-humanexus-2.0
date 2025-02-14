using UnityEngine;
using System;
using System.Drawing;
using System.Collections;

/// <summary>
/// A class to rotate a camera in a circle around a target
/// </summary>
public class CameraCircle : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float radius = 10f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private Transform target;
    private Transform cameraTransform;
    
    

    private void Awake()
    {
        cameraTransform = Camera.main.transform; ;
    }

    private void Start()
    {
        StartCoroutine(MoveInCircle(duration));
    }

    private IEnumerator MoveInCircle(float duration)
    {
        float elapsed = 0f;
        float theta = 0f;
        while (elapsed <= duration)
        {
            theta = (elapsed / duration) * 360f;
            SetCamera(theta);

            elapsed += Time.deltaTime;
            yield return null;
        }

    }

    private void SetCamera(float angle) {

        (float, float) positionOnCircle = GetPointOnCircle(target.position.x, target.position.z, radius, angle);

        cameraTransform.localPosition = new Vector3(positionOnCircle.Item1, 0f, positionOnCircle.Item2);
    }


    private (float, float) GetPointOnCircle(float centerX, float centerY, float radius, float angleInDegrees)
    {
        double angleInRadians = Math.PI * angleInDegrees / 180;
        float x = (float)(centerX + radius * Math.Cos(angleInRadians));
        float z = (float)(centerY + radius * Math.Sin(angleInRadians));
        (float, float) result = (x, z);
        return result ;
    }
}

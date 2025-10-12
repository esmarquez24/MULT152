using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!targetCamera)
        {
            targetCamera = Camera.main;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (targetCamera)
        {
            transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward, targetCamera.transform.rotation * Vector3.up);
        }
    }
}
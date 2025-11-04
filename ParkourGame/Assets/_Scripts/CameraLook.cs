using UnityEngine;

public class FPCameraLook : MonoBehaviour
{
    public Transform playerRoot;  // the Player (capsule)
    public float mouseSensitivity = 150f;
    public float minPitch = -75f;
    public float maxPitch = 75f;
    public float inputDelay = 2f; // seconds to wait before capturing mouse input

    float pitch = 0f;
    float timer = 0f;
    bool inputEnabled = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pitch = transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    void Update()
    {
        // Wait for delay before enabling input
        if (!inputEnabled)
        {
            timer += Time.deltaTime;
            if (timer >= inputDelay)
            {
                inputEnabled = true;
            }
            return;
        }

        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");

        playerRoot.Rotate(Vector3.up, mx * mouseSensitivity * Time.deltaTime);
        pitch = Mathf.Clamp(pitch - my * mouseSensitivity * Time.deltaTime, minPitch, maxPitch);
        transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
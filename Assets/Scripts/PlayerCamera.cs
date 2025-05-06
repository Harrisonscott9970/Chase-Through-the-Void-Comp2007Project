using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float sensx = 100f; // How fast the camera turns left/right
    public float sensy = 100f; // How fast the camera turns up/down

    public Transform orientation; // The direction the player is facing

    float xRotation; // Up/down turning angle
    float yRotation; // Left/right turning angle

    void Start()
    {
        // Hide and lock the mouse in the middle of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse movement
        float mouseX = Input.GetAxisRaw("Mouse X") * sensx * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensy * Time.deltaTime;

        // Add mouse movement to rotation
        yRotation += mouseX; // Turn left/right
        xRotation -= mouseY; // Turn up/down

        // Stop the camera from flipping upside down
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate the camera
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Rotate the player body (left/right only)
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}

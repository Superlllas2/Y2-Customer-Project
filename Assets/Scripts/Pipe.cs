using System;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public float rotationSpeed = 100f;   // Speed of rotation

    private bool isBeingHeld = false;    // Is the pipe currently being held by the player?
    private bool isRotating = false;     // Is the player rotating the pipe?
    private Vector3 initialMousePosition;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // If the player is holding the pipe
        if (isBeingHeld)
        {
            Debug.Log("Hehehe");
            // Check if the player is holding Shift to rotate
            if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift))
            {
                StartRotation();
            }
            else if (isRotating)
            {
                StopRotation();
            }

            if (isRotating)
            {
                RotatePipe();
            }
        }
    }

    void StartRotation()
    {
        isRotating = true;
        Cursor.lockState = CursorLockMode.Locked;  // Lock the cursor
        Cursor.visible = false;                    // Hide the cursor
        initialMousePosition = Input.mousePosition;
    }

    void StopRotation()
    {
        isRotating = false;
        Cursor.lockState = CursorLockMode.None;    // Unlock the cursor
        Cursor.visible = true;                     // Show the cursor
    }

    void RotatePipe()
    {
        // Get mouse delta movement
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Apply rotation based on mouse movement
        // Rotates around Y axis based on horizontal mouse movement, and around X axis based on vertical movement
        transform.Rotate(Vector3.up, -mouseX * rotationSpeed * Time.deltaTime, Space.World);   // Rotate around Y axis
        transform.Rotate(Vector3.right, mouseY * rotationSpeed * Time.deltaTime, Space.World); // Rotate around X axis
    }

    // Call this function when the player grabs or releases the pipe
    public void SetHeldState(bool held)
    {
        isBeingHeld = held;

        if (held)
        {
            rb.isKinematic = true;  // Disable physics while being held
        }
        else
        {
            rb.isKinematic = false;  // Enable physics again when dropped
            StopRotation();          // Ensure rotation stops when pipe is released
        }
    }
}

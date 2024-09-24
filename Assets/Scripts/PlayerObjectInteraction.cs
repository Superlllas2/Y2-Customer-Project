using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float rayDistance = 10f;       // Max distance for interaction
    public float sphereRadius = 0.5f;     // Radius around the crosshair
    public LayerMask interactableLayer;   // Layer for interactable objects
    public float grabSpeed = 10f;         // Speed of object movement toward the hold point
    public float throwForce = 500f;       // Force applied when throwing the object
    public float holdDistance = 2f;       // Distance in front of the player where the object will be held
    public float rotationSpeed = 100f;    // Speed of rotation

    private ObjectOutline lastOutlinedObject = null;
    private Rigidbody grabbedObjectRb = null;  // Reference to the currently grabbed object
    private Camera cam;
    private Pipe grabbedPipe = null;
    private bool isInRotationMode = false;     // Flag to check if the player is in rotation mode

    public MonoBehaviour cameraController;

    void Start()
    {
        cam = Camera.main;  // Reference to the main camera
    }

    void Update()
    {
        // If we're in rotation mode, handle rotation
        if (isInRotationMode)
        {
            RotateObject();  // Handle object rotation based on mouse movement

            // Exit rotation mode when Shift is released
            if (Input.GetKeyUp(KeyCode.R))
            {
                ExitRotationMode();
            }
            return;  // Skip the rest of the update logic if in rotation mode
        }

        // Ray from the center of the screen
        var ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Perform a spherecast
        if (Physics.SphereCast(ray, sphereRadius, out hit, rayDistance, interactableLayer))
        {
            ObjectOutline outlineScript = hit.collider.gameObject.GetComponent<ObjectOutline>();

            // If an object with an outline script is hit
            if (outlineScript)
            {
                // If this is a new object, remove the outline from the previous one
                if (lastOutlinedObject != outlineScript)
                {
                    if (lastOutlinedObject)
                    {
                        lastOutlinedObject.RemoveOutline();  // Remove outline
                    }

                    outlineScript.ApplyOutline();  // Apply outline to the new object
                    lastOutlinedObject = outlineScript;  // Store the new object as the last hit
                }

                // Attempt to grab the object
                TryGrabObject(hit);
            }
        }
        else
        {
            // If we didn't hit any object, remove outline from the last hit object
            if (lastOutlinedObject)
            {
                lastOutlinedObject.RemoveOutline();
                lastOutlinedObject = null;  // Clear the reference after removing the outline
            }
        }

        // Move the grabbed object while the left mouse button is held
        if (grabbedObjectRb)
        {
            if (Input.GetKeyDown(KeyCode.R))  // Enter rotation mode when Shift is pressed
            {
                EnterRotationMode();
            }

            MoveObject();  // Move the grabbed object
            if (Input.GetMouseButtonUp(0))  // Release the object when the left mouse button is released
            {
                ReleaseObject();
            }
        }
    }

    void TryGrabObject(RaycastHit hit)
    {
        if (Input.GetMouseButtonDown(0))  // Check if the left mouse button is pressed
        {
            Rigidbody hitRb = hit.collider.GetComponent<Rigidbody>();
            if (!hitRb.isKinematic)
            {
                Pipe pipeComponent = hit.collider.GetComponent<Pipe>();
                if (hitRb)  // Check if the hit object has a Rigidbody
                {
                    grabbedObjectRb = hitRb;
                    grabbedPipe = pipeComponent;
                    grabbedPipe.SetHeldState(true);
                    grabbedObjectRb.useGravity = false;  // Disable gravity while holding the object
                    grabbedObjectRb.drag = 10;           // Setting drug to better the rotation
                }
            }
            else
            {
                Debug.Log("You have clicked a pipe that has been attached");
                Pipe hitPipe = hit.collider.GetComponent<Pipe>();
                if (!hitPipe) return;
                switch (hitPipe.connectedAxis)
                {
                    case "X":
                        hitPipe.transform.eulerAngles += new Vector3(90, 0, 0);
                        break;
                    case "Y":
                        hitPipe.transform.eulerAngles += new Vector3(0, 90, 0);
                        break;
                    case "Z":
                        hitPipe.transform.eulerAngles += new Vector3(0, 0, 90);
                        break;
                    default:
                        Debug.Log("Invalid axis");
                        break;
                }
            }
        }
    }

    void MoveObject()
    {
        // Ensure the pipe is not snapped before moving it
        if (grabbedPipe && grabbedPipe.isSnapped)
        {
            return;  // Exit the function if the pipe is already snapped
        }
        
        // Ensure the Rigidbody is not kinematic
        if (grabbedObjectRb.isKinematic)
        {
            grabbedObjectRb.isKinematic = false;  // Make sure the object is movable
        }
        
        // The position in front of the camera where the object should come
        Vector3 holdPosition = cam.transform.position + cam.transform.forward * holdDistance;

        // Move the grabbed object towards the position
        Vector3 directionToHoldPoint = (holdPosition - grabbedObjectRb.position);
        grabbedObjectRb.velocity = directionToHoldPoint * grabSpeed;  // Move the object toward the hold position
    }

    void ReleaseObject()
    {
        if (grabbedPipe && grabbedPipe.isSnapped)
        {
            return;  // Exit the function if the pipe is already snapped
        }
        
        grabbedPipe.SetHeldState(false);
        grabbedPipe = null;
        
        grabbedObjectRb.useGravity = true;  // Re-enable gravity when releasing
        grabbedObjectRb.drag = 1;           // Reset the drag value to default
        
        // Apply a forward force in the direction of the camera when releasing the object
        grabbedObjectRb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
        
        grabbedObjectRb = null;  // Reset the reference to the grabbed object
    }

    // Enter Rotation Mode: Disable player/camera movement, hide the cursor, and start rotating
    void EnterRotationMode()
    {
        isInRotationMode = true;

        if (cameraController)
        {
            
            cameraController.enabled = false;      // Disable the camera control script
        }
    }

    // Exit Rotation Mode: Restore player/camera movement, show the cursor
    void ExitRotationMode()
    {
        isInRotationMode = false;

        if (cameraController)
        {
            cameraController.enabled = true;      // Disable the camera control script
        }
    }

    // Rotate the grabbed object based on mouse movement
    void RotateObject()
    {
        // Get mouse delta movement
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Apply rotation based on mouse movement
        grabbedObjectRb.transform.Rotate(Vector3.up, -mouseX * rotationSpeed * Time.deltaTime, Space.World);   // Rotate around Y axis
        grabbedObjectRb.transform.Rotate(Vector3.right, mouseY * rotationSpeed * Time.deltaTime, Space.World); // Rotate around X axis
    }
}

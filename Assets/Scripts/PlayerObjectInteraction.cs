using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float rayDistance = 10f;       // Max distance for interaction
    public float sphereRadius = 0.5f;     // Radius around the crosshair
    public LayerMask interactableLayer;   // Layer for interactable objects
    public float grabSpeed = 10f;         // Speed of object movement toward the hold point
    public float throwForce = 500f;       // Force applied when throwing the object
    public float holdDistance = 2f;       // Distance in front of the player where the object will be held

    private ObjectOutline lastOutlinedObject = null;
    private Rigidbody grabbedObjectRb = null;  // Reference to the currently grabbed object
    private Camera cam;

    void Start()
    {
        cam = Camera.main;  // Reference to the main camera
    }

    void Update()
    {
        // Ray from the center of the screen (crosshair)
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Perform a spherecast
        if (Physics.SphereCast(ray, sphereRadius, out hit, rayDistance, interactableLayer))
        {
            ObjectOutline outlineScript = hit.collider.gameObject.GetComponent<ObjectOutline>();

            // If we hit an object with an outline script
            if (outlineScript)
            {
                // If this is a new object, remove the outline from the previous one
                if (lastOutlinedObject != outlineScript)
                {
                    if (lastOutlinedObject)
                    {
                        lastOutlinedObject.RemoveOutline();  // Remove outline from the last object
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
            if (hitRb)  // Check if the hit object has a Rigidbody
            {
                grabbedObjectRb = hitRb;
                grabbedObjectRb.useGravity = false;  // Disable gravity while holding the object
                grabbedObjectRb.drag = 10;           // Increase drag to smooth out movement
            }
        }
    }

    void MoveObject()
    {
        // Calculate the position in front of the camera where the object should move
        Vector3 holdPosition = cam.transform.position + cam.transform.forward * holdDistance;

        // Move the grabbed object towards this position
        Vector3 directionToHoldPoint = (holdPosition - grabbedObjectRb.position);
        grabbedObjectRb.velocity = directionToHoldPoint * grabSpeed;  // Move the object toward the hold position
    }

    void ReleaseObject()
    {
        grabbedObjectRb.useGravity = true;  // Re-enable gravity when releasing
        grabbedObjectRb.drag = 1;           // Reset the drag value to default

        // Apply a forward force in the direction of the camera when releasing the object
        grabbedObjectRb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
        
        grabbedObjectRb = null;  // Reset the reference to the grabbed object
    }
}

using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Transform startEnd;         // One end of the pipe
    public Transform endEnd;           // The other end of the pipe
    public LayerMask pickable;        // Layer for detecting other pipes
    public float snapDistance = 1f;    // Distance threshold for snapping pipes together

    private bool isBeingHeld = false;  // Is the pipe currently being held by the player?
    private bool isSnapped = false;    // Is the pipe snapped and connected to another object?
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check for pipe connection only if the pipe is being held and not already snapped
        Debug.Log("isBeingHeld: " + isBeingHeld);
        Debug.Log("!isSnapped: " + !isSnapped);
        if (isBeingHeld && !isSnapped)
        {
            CheckForConnection();
            Debug.Log("Getting checked");
        }
    }

    void CheckForConnection()
    {
        // Check for nearby pipes within the snapDistance
        Collider[] nearbyPipes = Physics.OverlapSphere(startEnd.position, snapDistance, pickable);

        foreach (Collider other in nearbyPipes)
        {
            Pipe otherPipe = other.GetComponent<Pipe>();

            if (otherPipe && otherPipe != this)
            {
                // Check if this pipe's startEnd is close enough to the other pipe's ends
                if (Vector3.Distance(startEnd.position, otherPipe.startEnd.position) <= snapDistance)
                {
                    SnapToOtherPipe(otherPipe.startEnd);
                    return;
                }
                else if (Vector3.Distance(startEnd.position, otherPipe.endEnd.position) <= snapDistance)
                {
                    SnapToOtherPipe(otherPipe.endEnd);
                    return;
                }
            }
        }

        // Also check the other end of this pipe
        nearbyPipes = Physics.OverlapSphere(endEnd.position, snapDistance, pickable);

        foreach (Collider other in nearbyPipes)
        {
            Pipe otherPipe = other.GetComponent<Pipe>();

            if (otherPipe && otherPipe)
            {
                // Check if this pipe's endEnd is close enough to the other pipe's ends
                if (Vector3.Distance(endEnd.position, otherPipe.startEnd.position) <= snapDistance)
                {
                    SnapToOtherPipe(otherPipe.startEnd);
                    return;
                }
                else if (Vector3.Distance(endEnd.position, otherPipe.endEnd.position) <= snapDistance)
                {
                    SnapToOtherPipe(otherPipe.endEnd);
                    return;
                }
            }
        }
    }

    void SnapToOtherPipe(Transform targetEnd)
    {
        // Snap this pipe's closest end to the other pipe's end
        transform.position = targetEnd.position - (startEnd.position - transform.position);  // Adjust based on the connection point
        transform.rotation = targetEnd.rotation;

        LockPipe();  // Lock the pipe in place
    }

    void LockPipe()
    {
        isSnapped = true;
        rb.isKinematic = true;  // Disable physics to "lock" the pipe in place
        isBeingHeld = false;     // Pipe is no longer being held by the player
        Debug.Log("Pipe locked in place!");
    }

    public void SetHeldState(bool held)
    {
        isBeingHeld = held;

        if (held)
        {
            rb.isKinematic = false;  // Enable physics while being held
        }
        else if (!isSnapped)
        {
            rb.isKinematic = false;  // Enable physics again when dropped, if not snapped
        }
    }
}

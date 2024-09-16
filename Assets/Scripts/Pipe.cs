using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Transform startEnd;         // One end of the pipe
    public Transform endEnd;           // The other end of the pipe
    public LayerMask pickable;        // Layer for detecting other pipes
    public float snapDistance = 1f;    // Distance threshold for snapping pipes together

    private bool isBeingHeld = false;  // Is the pipe currently being held by the player?
    public bool isSnapped = false;    // Is the pipe snapped and connected to another object?
    private Rigidbody rb;
    
    // The next pipe in the series
    public Pipe nextPipe;
    public bool isConnected = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check for pipe connection only if the pipe is being held and not already snapped
        if (isBeingHeld && !isSnapped)
        {
            CheckForConnection();
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
                    SnapToOtherPipe(otherPipe.startEnd, otherPipe);
                    return;
                }
                else if (Vector3.Distance(startEnd.position, otherPipe.endEnd.position) <= snapDistance)
                {
                    SnapToOtherPipe(otherPipe.endEnd, otherPipe);
                    return;
                }
            }
        }

        // Also check the other end of this pipe
        nearbyPipes = Physics.OverlapSphere(endEnd.position, snapDistance, pickable);

        foreach (Collider other in nearbyPipes)
        {
            Pipe otherPipe = other.GetComponent<Pipe>();

            if (otherPipe && otherPipe != this)
            {
                // Check if this pipe's endEnd is close enough to the other pipe's ends
                if (Vector3.Distance(endEnd.position, otherPipe.startEnd.position) <= snapDistance)
                {
                    SnapToOtherPipe(otherPipe.startEnd, otherPipe);
                    return;
                }
                else if (Vector3.Distance(startEnd.position, otherPipe.endEnd.position) <= snapDistance)
                {
                    SnapToOtherPipe(otherPipe.endEnd, otherPipe);
                    return;
                }
            }
        }
    }

    void SnapToOtherPipe(Transform targetEnd, Pipe otherPipe)
    {
        // If we're connecting startEnd to the target end
        if (Vector3.Distance(startEnd.position, targetEnd.position) <= snapDistance)
        {
            // Move the startEnd of the current pipe to the targetEnd's exact position
            AlignToPoint(startEnd, targetEnd, otherPipe);
        }
        // If we're connecting endEnd to the target end
        else if (Vector3.Distance(endEnd.position, targetEnd.position) <= snapDistance)
        {
            // Move the endEnd of the current pipe to the targetEnd's exact position
            AlignToPoint(endEnd, targetEnd, otherPipe);
        }

        nextPipe = otherPipe;
        isConnected = true;

        LockPipe();  // Lock the pipe in place
        CheckSeriesCompletion();  // Check if the series is complete
    }
    
    void AlignToPoint(Transform heldEnd, Transform targetEnd, Pipe otherPipe)
    {
        // Calculate the offset from the center of the pipe to the held end
        Vector3 offset = heldEnd.position - transform.position;  // Pivot is at the center

        // Move the pipe such that the end aligns with the target
        transform.position = targetEnd.position - offset;

        // Adjust rotation if necessary
        Vector3 heldDirection = (heldEnd == startEnd) ? (endEnd.position - startEnd.position).normalized : (startEnd.position - endEnd.position).normalized;
        Vector3 targetDirection = (targetEnd == otherPipe.startEnd) ? (otherPipe.endEnd.position - otherPipe.startEnd.position).normalized : (otherPipe.startEnd.position - otherPipe.endEnd.position).normalized;

        Quaternion rotationAdjustment = Quaternion.FromToRotation(heldDirection, targetDirection);
        transform.rotation = rotationAdjustment * transform.rotation;
    }




    void LockPipe()
    {
        isSnapped = true;
        rb.isKinematic = true;  // Disable physics to "lock" the pipe in place
        isBeingHeld = false;     // Pipe is no longer being held by the player
        // Debug.Log("Pipe locked in place!");
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
        else
        {
            rb.isKinematic = true;  // Disable physics when snapped to prevent further movement
        }
    }
    
    void CheckSeriesCompletion()
    {
        Pipe currentPipe = this;
        int pipeCount = 1;
        
        while (currentPipe.nextPipe)
        {
            pipeCount++;
            currentPipe = currentPipe.nextPipe;
        }
        
        //TODO: change the hardcoded value
        if (pipeCount >= 4)
        {
            Debug.Log("Connection is successful! Water is at home!");
        }
    }
}

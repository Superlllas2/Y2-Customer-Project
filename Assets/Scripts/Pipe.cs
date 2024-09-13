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
        // Check if we are connecting startEnd or endEnd
        if (Vector3.Distance(startEnd.position, targetEnd.position) <= snapDistance)
        {
            // Align the start of this pipe to the target end
            Vector3 connectionDirection = (targetEnd.position - endEnd.position).normalized;
            transform.position = targetEnd.position - (startEnd.position - transform.position);  // Correct position
            transform.rotation = targetEnd.rotation;
        }
        else if (Vector3.Distance(endEnd.position, targetEnd.position) <= snapDistance)
        {
            // Align the end of this pipe to the target start
            Vector3 connectionDirection = (targetEnd.position - startEnd.position).normalized;
            transform.position = targetEnd.position - (endEnd.position - transform.position);  // Correct position
            transform.rotation = targetEnd.rotation;
        }
        
        
        nextPipe = otherPipe;
        isConnected = true;

        LockPipe();
        // Debug.Log("isSnapped" + isSnapped + "rb.isKinematic" + rb.isKinematic + "isBeingHeld" + isBeingHeld);
        CheckSeriesCompletion();
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

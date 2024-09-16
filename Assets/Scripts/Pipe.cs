using System.Collections;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Transform startEnd;         // One end of the pipe
    public Transform endEnd;           // The other end of the pipe

    public Transform[] ends; //TODO changing here so you can have multiple ends and small code. Nice :)
    
    public LayerMask pickable;         // Layer for detecting other pipes
    public float snapDistance = 1f;    // Distance threshold for snapping pipes together

    private bool isBeingHeld = false;  // Is the pipe currently being held by the player?
    public bool isSnapped = false;     // Is the pipe snapped and connected to another object?
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

    // Check if the held pipe is close enough to connect to another pipe
    void CheckForConnection()
    {
        // Check for nearby pipes within the snapDistance
        Collider[] nearbyPipes = Physics.OverlapSphere(startEnd.position, snapDistance, pickable);

        foreach (Collider other in nearbyPipes)
        {
            Pipe otherPipe = other.GetComponent<Pipe>();

            if (otherPipe is not null && otherPipe != this)
            {
                // Check if this pipe's startEnd is close enough to the other pipe's ends
                if (Vector3.Distance(startEnd.position, otherPipe.startEnd.position) <= snapDistance)
                {
                    Debug.Log("startEnd.position, otherPipe.startEnd.position");
                    AlignAndSnapPipe(otherPipe.startEnd, otherPipe, startEnd);
                    return;
                }
                if (Vector3.Distance(startEnd.position, otherPipe.endEnd.position) <= snapDistance)
                {
                    Debug.Log("startEnd.position, otherPipe.endEnd.position");
                    AlignAndSnapPipe(otherPipe.endEnd, otherPipe, startEnd);
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
                    Debug.Log("endEnd.position, otherPipe.startEnd.position");
                    AlignAndSnapPipe(otherPipe.startEnd, otherPipe, endEnd);
                    return;
                }
                if (Vector3.Distance(endEnd.position, otherPipe.endEnd.position) <= snapDistance)
                {
                    Debug.Log("endEnd.position, otherPipe.endEnd.position");
                    AlignAndSnapPipe(otherPipe.endEnd, otherPipe, endEnd);
                    return;
                }
            }
        }
    }

    // Align and snap the held pipe to the target pipe
    void AlignAndSnapPipe(Transform endPositionOnTheOtherPipe, Pipe otherPipe, Transform endPositionThisPipe)
    {
        // Step 1: Calculate the direction vector of the target pipe
        // Vector3 targetVector = (otherPipe.endEnd.position - otherPipe.startEnd.position).normalized;

        // Step 2: Align the held pipe's center to the target vector
        // AlignPipeToVector(targetVector);
        // Step 3: Snap the held pipe's end to the target pipe's end
        SnapPipeToTarget(endPositionOnTheOtherPipe, otherPipe, endPositionThisPipe);

        // Mark the pipes as connected
        nextPipe = otherPipe;
        isConnected = true;
        LockPipe();  // Lock the pipe in place
        CheckSeriesCompletion();  // Check if the series is complete
    }

    // Align the center of the held pipe along the target vector
    void AlignPipeToVector(Vector3 targetVector)
    {
        // Simply adjust the position and alignment to the target vector, assuming the pivot is already at the center
        transform.rotation = Quaternion.LookRotation(targetVector);
    }

    // Snap the held pipe's end to the target end
    void SnapPipeToTarget(Transform endPositionOnTheOtherPipe, Pipe otherPipe, Transform endPositionThisPipe)
    {
        Vector3 offset = transform.position - endPositionOnTheOtherPipe.position;
        // Debug.DrawLine(endPositionThisPipe.position, endPositionOnTheOtherPipe.position, Color.cyan, 5.0f);
 
        StartCoroutine(RotateAfterOneFrame(offset, otherPipe.transform.rotation, endPositionThisPipe, endPositionOnTheOtherPipe));
    }

    private IEnumerator RotateAfterOneFrame(Vector3 moveTo, Quaternion rotateTo,Transform mine, Transform targetEnd)
    {
        yield return null;
        transform.rotation = rotateTo;
        yield return null;
        var halfDistance = transform.position - mine.position;
        var offset = transform.position - targetEnd.position;
        // Debug.Log($"NEW Offset -> {offset}.");
        transform.position -= (offset + halfDistance);
        yield return null;
        LockPipe();
    }

    // Lock the pipe in place by making it kinematic and disabling further movement
    void LockPipe()
    {
        isSnapped = true;
        rb.isKinematic = true;  // Disable physics to lock the pipe in place
        isBeingHeld = false;     // Pipe is no longer being held by the player
    }

    // Set whether the pipe is being held or not
    public void SetHeldState(bool held)
    {
        isBeingHeld = held;

        if (held)
        {
            rb.isKinematic = false;  // Enable physics while being held
        }
        else if (!isSnapped)
        {
            rb.isKinematic = false;  // Enable physics when dropped, if not snapped
        }
        else
        {
            rb.isKinematic = true;  // Disable physics when snapped to prevent further movement
        }
    }

    // Check if the entire pipe series is connected
    void CheckSeriesCompletion()
    {
        Pipe currentPipe = this;
        int pipeCount = 1;

        while (currentPipe.nextPipe)
        {
            pipeCount++;
            currentPipe = currentPipe.nextPipe;
        }

        if (pipeCount >= 4)  // Adjust the pipe count as needed for your game
        {
            Debug.Log("Connection is successful! All pipes are connected.");
        }
    }
}

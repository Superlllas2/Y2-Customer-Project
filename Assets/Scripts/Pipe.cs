using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Transform startEnd; // One end of the pipe
    public Transform endEnd; // The other end of the pipe

    private PlayerInteraction playerInteraction;

    public LayerMask pickable; // Layer for detecting other pipes
    public float snapDistance = 1f; // Distance threshold for snapping pipes together

    private bool isBeingHeld;
    public bool isSnapped;
    private Rigidbody rb;

    // The next pipe in the series
    public Pipe previousPipe;
    public Pipe nextPipe;
    public bool isConnected;
    public PipeType pipeType;

    private const float MinOutwardSqrMagnitude = 1e-6f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInteraction = FindAnyObjectByType<PlayerInteraction>();
    }

    void Update()
    {
        // Check for pipe connection only if the pipe is being held and not already snapped
        if (isBeingHeld && !isSnapped && playerInteraction && !playerInteraction.isInRotationMode)
        {
            CheckForConnection();
        }
    }

    Transform[] GetEnds()
    {
        return new[] { startEnd, endEnd };
    }

    Vector3 GetOutward(Transform end)
    {
        Vector3 fromRoot = end.position - transform.position;
        if (fromRoot.sqrMagnitude > MinOutwardSqrMagnitude)
        {
            return fromRoot.normalized;
        }

        return end.forward;
    }

    // Check if the held pipe is close enough to connect to another pipe
    void CheckForConnection()
    {
        if (!startEnd || !endEnd)
        {
            return;
        }

        float reachFromCenter = 0f;
        foreach (Transform end in GetEnds())
        {
            reachFromCenter = Mathf.Max(reachFromCenter, Vector3.Distance(transform.position, end.position));
        }

        Collider[] nearby = Physics.OverlapSphere(transform.position, reachFromCenter + snapDistance, pickable);

        float bestDistance = snapDistance;
        Transform bestMyEnd = null;
        Transform bestTheirEnd = null;
        Pipe bestOther = null;

        foreach (Collider other in nearby)
        {
            Pipe otherPipe = other.GetComponent<Pipe>();
            if (!otherPipe || otherPipe == this || !otherPipe.startEnd || !otherPipe.endEnd)
            {
                continue;
            }

            foreach (Transform myEnd in GetEnds())
            {
                foreach (Transform theirEnd in otherPipe.GetEnds())
                {
                    float distance = Vector3.Distance(myEnd.position, theirEnd.position);
                    if (distance <= bestDistance)
                    {
                        bestDistance = distance;
                        bestMyEnd = myEnd;
                        bestTheirEnd = theirEnd;
                        bestOther = otherPipe;
                    }
                }
            }
        }

        if (bestOther != null)
        {
            AlignAndSnapPipe(bestTheirEnd, bestOther, bestMyEnd);
        }
    }

    // Align and snap the held pipe to the target pipe
    private void AlignAndSnapPipe(Transform theirEnd, Pipe otherPipe, Transform myEnd)
    {
        Vector3 myOutward = GetOutward(myEnd);
        Vector3 theirOutward = otherPipe.GetOutward(theirEnd);

        if (myOutward.sqrMagnitude < MinOutwardSqrMagnitude ||
            theirOutward.sqrMagnitude < MinOutwardSqrMagnitude)
        {
            return;
        }

        Vector3 targetOutward = -theirOutward;
        transform.rotation = Quaternion.FromToRotation(myOutward, targetOutward) * transform.rotation;
        transform.position += theirEnd.position - myEnd.position;

        previousPipe = otherPipe;
        otherPipe.nextPipe = this;

        isConnected = true;
        otherPipe.isConnected = true;
        LockPipe();
    }

    // Lock the pipe in place by making it kinematic and disabling further movement
    void LockPipe()
    {
        isSnapped = true;
        rb.isKinematic = true; // Disable physics to lock the pipe in place
        isBeingHeld = false; // Pipe is no longer being held by the player
    }

    // Set whether the pipe is being held or not
    public void SetHeldState(bool held)
    {
        isBeingHeld = held;

        if (held)
        {
            rb.isKinematic = false; // Enable physics while being held
        }
        else if (!isSnapped)
        {
            rb.isKinematic = false; // Enable physics when dropped, if not snapped
        }
        else
        {
            rb.isKinematic = true; // Disable physics when snapped to prevent further movement
        }
    }
}

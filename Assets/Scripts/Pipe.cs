using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Pipe : MonoBehaviour
{
    public Transform startEnd; // One end of the pipe
    public Transform endEnd; // The other end of the pipe

    public Transform[] ends; //TODO changing here so you can have multiple ends and small code. Nice :)

    public LayerMask pickable; // Layer for detecting other pipes
    public float snapDistance = 1f; // Distance threshold for snapping pipes together

    private bool isBeingHeld = false; // Is the pipe currently being held by the player?
    public bool isSnapped = false; // Is the pipe snapped and connected to another object?
    private Rigidbody rb;
    public String connectedAxis = null;
    public Vector3[] connectionDirections;

    // The next pipe in the series
    public Pipe nextPipe;
    public bool isConnected = false;
    public PipeType pipeType;

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
                    // Debug.Log("startEnd.position, otherPipe.startEnd.position");
                    AlignAndSnapPipe(otherPipe.startEnd, otherPipe, startEnd);
                    return;
                }
                // --------
                // if (Vector3.Distance(startEnd.position, otherPipe.endEnd.position) <= snapDistance)
                // {
                //     Debug.Log("startEnd.position, otherPipe.endEnd.position");
                //     AlignAndSnapPipe(otherPipe.endEnd, otherPipe, startEnd);
                //     return;
                // }
                // --------
            }
        }

        // Also check the other end of this pipe
        nearbyPipes = Physics.OverlapSphere(endEnd.position, snapDistance, pickable);

        foreach (Collider other in nearbyPipes)
        {
            Pipe otherPipe = other.GetComponent<Pipe>();

            if (otherPipe && otherPipe != this)
            {
                // --------
                // // Check if this pipe's endEnd is close enough to the other pipe's ends
                // if (Vector3.Distance(endEnd.position, otherPipe.startEnd.position) <= snapDistance)
                // {
                //     Debug.Log("endEnd.position, otherPipe.startEnd.position");
                //     AlignAndSnapPipe(otherPipe.startEnd, otherPipe, endEnd);
                //     return;
                // }
                // --------
                if (Vector3.Distance(endEnd.position, otherPipe.endEnd.position) <= snapDistance)
                {
                    // Debug.Log("endEnd.position, otherPipe.endEnd.position");
                    AlignAndSnapPipe(otherPipe.endEnd, otherPipe, endEnd);
                    return;
                }
            }
        }
    }

    // Align and snap the held pipe to the target pipe
    void AlignAndSnapPipe(Transform endPositionOnTheOtherPipe, Pipe otherPipe, Transform endPositionThisPipe)
    {
        var bestDirection = 0;
        var bestDot = float.MinValue;
        var toOtherPipe = endPositionOnTheOtherPipe.position - transform.position;
        // Debug.Log("Desired connection direction: " + toOtherPipe);
        for (int i = 0; i < connectionDirections.Length; i++)
        {
            var worldDir = transform.TransformDirection(connectionDirections[i]);
            var newDot = Vector3.Dot(worldDir, toOtherPipe);
            // Debug.Log("Direction " + worldDir + " gives dot " + newDot);
            if (newDot < bestDot)
            {
                bestDot = newDot;
                bestDirection = i;
            }
        }

        // Debug.Log("Connection direction for pipe: " + connectionDirections[bestDirection]);

        // hardcoded for now - TODO: Pipe script has method that returns this (world space) direction when a connection point is given
        Vector3 connectionDirection = new Vector3(0, 0, 0);
        // if our best direction is say (1,0,0), then we want to say: transform.right = -connectionDirection;
        // if our best direction is say (0,0,-1), then we want to say: transform.forward = connectionDirection;

        if (connectionDirections[bestDirection].x >= 0.99f)
        {
            transform.right = -connectionDirection;
            connectedAxis = "X";
        }
        else if (connectionDirections[bestDirection].z >= 0.99f)
        {
            transform.forward = -connectionDirection;
            connectedAxis = "Z";
        }
        else if (connectionDirections[bestDirection].z <= -0.99f)
        {
            transform.forward = connectionDirection;
            connectedAxis = "Z";
        }
        else if (connectionDirections[bestDirection].x >= -0.99f)
        {
            transform.right = connectionDirection;
            connectedAxis = "X";
        }
        else if (connectionDirections[bestDirection].y >= 0.99f)
        {
            transform.up = -connectionDirection;
            connectedAxis = "Y";
        }
        else if (connectionDirections[bestDirection].y >= -0.99f)
        {
            transform.up = connectionDirection;
            connectedAxis = "Y";
        }

        // ----------------
        // TODO: translate (after rotating) (should be easy)
        // ----------------
        // Quaternion desired = Quaternion.LookRotation(-connectionDirection);
        // Quaternion current = Quaternion.LookRotation(connectionDirections[bestDirection]);
        // Quaternion fromTo = desired * Quaternion.Inverse(current);
        // float angle; 
        // fromTo.ToAngleAxis(out angle, out Vector3 whatever);
        // Debug.Log("Needed rotation: "+angle);
        // transform.rotation = fromTo * transform.rotation;
        // ----------------

        if (pipeType == PipeType.Bend)
        {
            Vector3 distanceDifference = endPositionOnTheOtherPipe.position - endPositionThisPipe.position;
            transform.position += distanceDifference;
        }
        else if (pipeType == PipeType.Straight)
        {
            if (otherPipe.pipeType == PipeType.Bend)
            {
                SnapPipeToTarget(endPositionOnTheOtherPipe, otherPipe, endPositionThisPipe);
                // var childPivotPoint = endPositionThisPipe.position;
                // transform.position = childPivotPoint;
                // transform.RotateAround(childPivotPoint, Vector3.forward, -90);
                // transform.rotation = Quaternion.Euler(0, 0, 90) * transform.rotation;
                // transform.rotation = Quaternion.identity;
                // transform.rotation *= Quaternion.Euler(0, 0, 0);
                transform.SetParent(endPositionOnTheOtherPipe);
                transform.parent.rotation = Quaternion.Euler(0, 0, -90) * transform.rotation;

                // otherPipe.endEnd.Rotate(); // rotates the PARENT, which acts as a new pivot point
                // transform.SetParent(null);
            }
            else
            {
                SnapPipeToTarget(endPositionOnTheOtherPipe, otherPipe, endPositionThisPipe);
            }
        }

        // Mark the pipes as connected
        nextPipe = otherPipe;
        isConnected = true;
        LockPipe(); // Lock the pipe in place
    }

    // Snap the held pipe's end to the target end
    void SnapPipeToTarget(Transform endPositionOnTheOtherPipe, Pipe otherPipe, Transform endPositionThisPipe)
    {
        Vector3 offset = transform.position - endPositionOnTheOtherPipe.position;
        StartCoroutine(RotateAfterOneFrame(offset, otherPipe.transform.rotation, endPositionThisPipe,
            endPositionOnTheOtherPipe));
    }

    // Credit to: Yvans
    private IEnumerator RotateAfterOneFrame(Vector3 moveTo, Quaternion rotateTo, Transform mine, Transform targetEnd)
    {
        transform.rotation = rotateTo;
        var halfDistance = transform.position - mine.position;
        var offset = transform.position - targetEnd.position;
        transform.position -= (offset + halfDistance);
        yield return null;
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
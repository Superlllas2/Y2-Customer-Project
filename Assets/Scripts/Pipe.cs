using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Transform startEnd; // One end of the pipe
    public Transform endEnd; // The other end of the pipe

    private PlayerInteraction playerInteraction;

    public LayerMask pickable; // Layer for detecting other pipes
    public float snapDistance = 1f; // Distance threshold for snapping pipes together
    public float snappedRotateStepDegrees = 45f;

    private bool isBeingHeld;
    public bool isSnapped;
    private Rigidbody rb;

    // The next pipe in the series
    public Pipe previousPipe;
    public Pipe nextPipe;
    public bool isConnected;
    public PipeType pipeType;

    /// <summary>Set when this pipe is snapped directly onto a pump/water source.</summary>
    public PipeNetworkSource attachedSource;

    /// <summary>The end of this pipe that is locked to the previous pipe / source.</summary>
    public Transform connectedEnd;

    private const float MinOutwardSqrMagnitude = 1e-6f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInteraction = FindAnyObjectByType<PlayerInteraction>();
    }

    void Update()
    {
        // Check for pipe connection only if the pipe is being held and not already snapped
        if (isBeingHeld && !isSnapped)
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

    // Check if the held pipe is close enough to connect to another pipe or a network source
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

        float searchRadius = reachFromCenter + snapDistance;

        float bestPipeDistance = snapDistance;
        Transform bestMyEndForPipe = null;
        Transform bestTheirEnd = null;
        Pipe bestOther = null;

        Collider[] nearbyPipes = Physics.OverlapSphere(transform.position, searchRadius, pickable);
        foreach (Collider other in nearbyPipes)
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
                    if (distance <= bestPipeDistance)
                    {
                        bestPipeDistance = distance;
                        bestMyEndForPipe = myEnd;
                        bestTheirEnd = theirEnd;
                        bestOther = otherPipe;
                    }
                }
            }
        }

        float bestSourceDistance = snapDistance;
        Transform bestMyEndForSource = null;
        PipeNetworkSource bestSource = null;

        // Sources (pumps / water) may not be on the pickable layer — search all colliders.
        Collider[] nearbyAll = Physics.OverlapSphere(transform.position, searchRadius);
        foreach (Collider other in nearbyAll)
        {
            if (!other)
            {
                continue;
            }

            var source = other.GetComponentInParent<PipeNetworkSource>();
            if (!source)
            {
                continue;
            }

            var attachPos = source.GetAttachPosition();
            var radius = source.connectRadius > 0f ? source.connectRadius : snapDistance;
            var maxDist = Mathf.Min(snapDistance, radius);

            foreach (Transform myEnd in GetEnds())
            {
                float distance = Vector3.Distance(myEnd.position, attachPos);
                if (distance <= maxDist && distance <= bestSourceDistance)
                {
                    bestSourceDistance = distance;
                    bestMyEndForSource = myEnd;
                    bestSource = source;
                }
            }
        }

        if (bestSource != null && bestSourceDistance <= bestPipeDistance)
        {
            AlignAndSnapToSource(bestSource, bestMyEndForSource);
        }
        else if (bestOther != null)
        {
            AlignAndSnapPipe(bestTheirEnd, bestOther, bestMyEndForPipe);
        }
    }

    private void AlignAndSnapToSource(PipeNetworkSource source, Transform myEnd)
    {
        if (!source || !myEnd)
        {
            return;
        }

        Vector3 myOutward = GetOutward(myEnd);
        Vector3 theirOutward = source.GetOutward();

        if (myOutward.sqrMagnitude < MinOutwardSqrMagnitude ||
            theirOutward.sqrMagnitude < MinOutwardSqrMagnitude)
        {
            return;
        }

        Vector3 targetOutward = -theirOutward;
        transform.rotation = Quaternion.FromToRotation(myOutward, targetOutward) * transform.rotation;
        transform.position += source.GetAttachPosition() - myEnd.position;

        attachedSource = source;
        previousPipe = null;
        connectedEnd = myEnd;
        isConnected = true;
        LockPipe();
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

        attachedSource = null;
        previousPipe = otherPipe;
        otherPipe.nextPipe = this;
        connectedEnd = myEnd;

        isConnected = true;
        otherPipe.isConnected = true;
        LockPipe();
    }

    /// <summary>
    /// Rotates this pipe (and downstream pipes) by a fixed step around the connection axis.
    /// </summary>
    public void RotateAlongConnection(float degrees)
    {
        if (!isSnapped)
        {
            return;
        }

        if (!TryGetConnectionAxis(out var pivot, out var axis))
        {
            // Last resort: twist around the pipe's primary end axis.
            if (connectedEnd)
            {
                pivot = connectedEnd.position;
                axis = GetOutward(connectedEnd);
            }
            else if (startEnd)
            {
                pivot = startEnd.position;
                axis = GetOutward(startEnd);
            }
            else
            {
                return;
            }

            if (axis.sqrMagnitude < MinOutwardSqrMagnitude)
            {
                return;
            }
        }

        var chain = CollectDownstream();
        foreach (var pipe in chain)
        {
            pipe.transform.RotateAround(pivot, axis, degrees);
        }

        // Kill float drift at the joint.
        if (connectedEnd)
        {
            var correction = pivot - connectedEnd.position;
            if (correction.sqrMagnitude > 1e-10f)
            {
                foreach (var pipe in chain)
                {
                    pipe.transform.position += correction;
                }
            }
        }
    }

    public void RotateAlongConnectionStep()
    {
        RotateAlongConnection(snappedRotateStepDegrees);
    }

    private bool TryGetConnectionAxis(out Vector3 pivot, out Vector3 axis)
    {
        pivot = Vector3.zero;
        axis = Vector3.zero;

        if (connectedEnd)
        {
            pivot = connectedEnd.position;
            axis = GetOutward(connectedEnd);
            if (axis.sqrMagnitude > MinOutwardSqrMagnitude)
            {
                return true;
            }
        }

        if (attachedSource)
        {
            pivot = attachedSource.GetAttachPosition();
            axis = attachedSource.GetOutward();
            return axis.sqrMagnitude > MinOutwardSqrMagnitude;
        }

        if (previousPipe && previousPipe.startEnd && previousPipe.endEnd)
        {
            Transform theirEnd = null;
            float best = float.MaxValue;
            foreach (var end in new[] { previousPipe.startEnd, previousPipe.endEnd })
            {
                if (!end)
                {
                    continue;
                }

                var point = connectedEnd ? connectedEnd.position : transform.position;
                var d = Vector3.Distance(end.position, point);
                if (d < best)
                {
                    best = d;
                    theirEnd = end;
                }
            }

            if (theirEnd)
            {
                pivot = theirEnd.position;
                axis = previousPipe.GetOutward(theirEnd);
                return axis.sqrMagnitude > MinOutwardSqrMagnitude;
            }
        }

        return false;
    }

    private List<Pipe> CollectDownstream()
    {
        var list = new List<Pipe>();
        var current = this;
        var guard = 0;
        while (current && guard++ < 256)
        {
            list.Add(current);
            current = current.nextPipe;
        }

        return list;
    }

    // Lock the pipe in place by making it kinematic and disabling further movement
    void LockPipe()
    {
        isSnapped = true;
        if (rb)
        {
            rb.isKinematic = true;
        }

        isBeingHeld = false;

        // Holding must end immediately so free-rotate / grab cannot keep driving this pipe.
        if (playerInteraction)
        {
            playerInteraction.NotifyPipeSnapped(this);
        }
        else if (PlayerInteraction.Instance)
        {
            PlayerInteraction.Instance.NotifyPipeSnapped(this);
        }
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Pipe : MonoBehaviour
{
    public Transform startEnd;
    public Transform endEnd;

    private PlayerInteraction playerInteraction;

    public LayerMask pickable;
    [Tooltip("Max end-to-end distance to show ghost / allow snap on release.")]
    public float snapDistance = 1.75f;
    public float snappedRotateStepDegrees = 45f;

    [Header("Snap Preview")]
    [Tooltip("Optional override material for the ghost. If empty, a transparent grey HDRP Unlit is created at runtime.")]
    public Material ghostMaterial;
    public Color ghostColor = new Color(0.72f, 0.72f, 0.72f, 0.32f);

    private bool isBeingHeld;
    public bool isSnapped;
    private Rigidbody rb;

    public Pipe previousPipe;
    public Pipe nextPipe;
    public bool isConnected;
    public PipeType pipeType;

    public PipeNetworkSource attachedSource;
    public Transform connectedEnd;

    private Vector3 twistReferenceDir;

    private GameObject ghostInstance;
    private Material runtimeGhostMaterial;
    private bool hasPendingSnap;
    private Vector3 pendingPosition;
    private Quaternion pendingRotation;
    private Transform pendingMyEnd;
    private Pipe pendingOtherPipe;
    private Transform pendingTheirEnd;
    private PipeNetworkSource pendingSource;

    private const float MinOutwardSqrMagnitude = 1e-6f;

    public bool HasPendingSnap => hasPendingSnap && !isSnapped;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInteraction = FindAnyObjectByType<PlayerInteraction>();
    }

    void OnDisable()
    {
        ClearSnapPreview();
    }

    void OnDestroy()
    {
        ClearSnapPreview();
        if (runtimeGhostMaterial)
        {
            Destroy(runtimeGhostMaterial);
        }
    }

    void Update()
    {
        if (isBeingHeld && !isSnapped &&
            (playerInteraction == null || !playerInteraction.isInRotationMode))
        {
            UpdateSnapPreview();
        }
        else if (!isBeingHeld)
        {
            ClearSnapPreview();
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

    void UpdateSnapPreview()
    {
        if (!TryFindSnapCandidate(
                out var myEnd,
                out var otherPipe,
                out var theirEnd,
                out var source,
                out var posePos,
                out var poseRot))
        {
            ClearSnapPreview();
            return;
        }

        hasPendingSnap = true;
        pendingMyEnd = myEnd;
        pendingOtherPipe = otherPipe;
        pendingTheirEnd = theirEnd;
        pendingSource = source;
        pendingPosition = posePos;
        pendingRotation = poseRot;

        ShowGhost(posePos, poseRot);
    }

    /// <summary>
    /// Called on LMB release while holding. Returns true if the pipe locked into a snap.
    /// </summary>
    public bool TryCommitPendingSnap()
    {
        if (!HasPendingSnap)
        {
            ClearSnapPreview();
            return false;
        }

        // Re-validate in case the player moved at the last frame.
        if (!TryFindSnapCandidate(
                out var myEnd,
                out var otherPipe,
                out var theirEnd,
                out var source,
                out var posePos,
                out var poseRot))
        {
            ClearSnapPreview();
            return false;
        }

        ClearSnapPreview();
        transform.SetPositionAndRotation(posePos, poseRot);

        if (source)
        {
            attachedSource = source;
            previousPipe = null;
            connectedEnd = myEnd;
            isConnected = true;
        }
        else if (otherPipe)
        {
            attachedSource = null;
            previousPipe = otherPipe;
            otherPipe.nextPipe = this;
            connectedEnd = myEnd;
            isConnected = true;
            otherPipe.isConnected = true;
        }
        else
        {
            return false;
        }

        LockPipe();
        return true;
    }

    public void ClearSnapPreview()
    {
        hasPendingSnap = false;
        pendingMyEnd = null;
        pendingOtherPipe = null;
        pendingTheirEnd = null;
        pendingSource = null;

        if (ghostInstance)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
        }
    }

    bool TryFindSnapCandidate(
        out Transform myEnd,
        out Pipe otherPipe,
        out Transform theirEnd,
        out PipeNetworkSource source,
        out Vector3 posePos,
        out Quaternion poseRot)
    {
        myEnd = null;
        otherPipe = null;
        theirEnd = null;
        source = null;
        posePos = transform.position;
        poseRot = transform.rotation;

        if (!startEnd || !endEnd)
        {
            return false;
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
            Pipe candidate = other.GetComponentInParent<Pipe>();
            if (!candidate || candidate == this || !candidate.startEnd || !candidate.endEnd)
            {
                continue;
            }

            foreach (Transform candidateMyEnd in GetEnds())
            {
                foreach (Transform candidateTheirEnd in candidate.GetEnds())
                {
                    float distance = Vector3.Distance(candidateMyEnd.position, candidateTheirEnd.position);
                    if (distance <= bestPipeDistance)
                    {
                        bestPipeDistance = distance;
                        bestMyEndForPipe = candidateMyEnd;
                        bestTheirEnd = candidateTheirEnd;
                        bestOther = candidate;
                    }
                }
            }
        }

        float bestSourceDistance = snapDistance;
        Transform bestMyEndForSource = null;
        PipeNetworkSource bestSource = null;

        Collider[] nearbyAll = Physics.OverlapSphere(transform.position, searchRadius);
        foreach (Collider other in nearbyAll)
        {
            if (!other)
            {
                continue;
            }

            var candidateSource = other.GetComponentInParent<PipeNetworkSource>();
            if (!candidateSource)
            {
                continue;
            }

            var attachPos = candidateSource.GetAttachPosition();
            var radius = candidateSource.connectRadius > 0f ? candidateSource.connectRadius : snapDistance;
            var maxDist = Mathf.Min(snapDistance, radius);

            foreach (Transform candidateMyEnd in GetEnds())
            {
                float distance = Vector3.Distance(candidateMyEnd.position, attachPos);
                if (distance <= maxDist && distance <= bestSourceDistance)
                {
                    bestSourceDistance = distance;
                    bestMyEndForSource = candidateMyEnd;
                    bestSource = candidateSource;
                }
            }
        }

        if (bestSource != null && bestSourceDistance <= bestPipeDistance)
        {
            if (!TryComputeSourcePose(bestSource, bestMyEndForSource, out posePos, out poseRot))
            {
                return false;
            }

            myEnd = bestMyEndForSource;
            source = bestSource;
            return true;
        }

        if (bestOther != null)
        {
            if (!TryComputePipePose(bestTheirEnd, bestOther, bestMyEndForPipe, out posePos, out poseRot))
            {
                return false;
            }

            myEnd = bestMyEndForPipe;
            otherPipe = bestOther;
            theirEnd = bestTheirEnd;
            return true;
        }

        return false;
    }

    bool TryComputeSourcePose(
        PipeNetworkSource source,
        Transform myEnd,
        out Vector3 posePos,
        out Quaternion poseRot)
    {
        posePos = transform.position;
        poseRot = transform.rotation;

        if (!source || !myEnd)
        {
            return false;
        }

        Vector3 myOutward = GetOutward(myEnd);
        Vector3 theirOutward = source.GetOutward();
        if (myOutward.sqrMagnitude < MinOutwardSqrMagnitude ||
            theirOutward.sqrMagnitude < MinOutwardSqrMagnitude)
        {
            return false;
        }

        ComputeAlignedPose(myEnd, -theirOutward, myOutward, source.GetAttachPosition(), out posePos, out poseRot);
        return true;
    }

    bool TryComputePipePose(
        Transform theirEnd,
        Pipe otherPipe,
        Transform myEnd,
        out Vector3 posePos,
        out Quaternion poseRot)
    {
        posePos = transform.position;
        poseRot = transform.rotation;

        if (!theirEnd || !otherPipe || !myEnd)
        {
            return false;
        }

        Vector3 myOutward = GetOutward(myEnd);
        Vector3 theirOutward = otherPipe.GetOutward(theirEnd);
        if (myOutward.sqrMagnitude < MinOutwardSqrMagnitude ||
            theirOutward.sqrMagnitude < MinOutwardSqrMagnitude)
        {
            return false;
        }

        ComputeAlignedPose(myEnd, -theirOutward, myOutward, theirEnd.position, out posePos, out poseRot);
        return true;
    }

    void ComputeAlignedPose(
        Transform myEnd,
        Vector3 targetOutward,
        Vector3 myOutward,
        Vector3 attachPos,
        out Vector3 posePos,
        out Quaternion poseRot)
    {
        poseRot = Quaternion.FromToRotation(myOutward, targetOutward) * transform.rotation;
        Vector3 endAfterRot = transform.position +
                              poseRot * Quaternion.Inverse(transform.rotation) * (myEnd.position - transform.position);
        posePos = transform.position + (attachPos - endAfterRot);
    }

    void ShowGhost(Vector3 posePos, Quaternion poseRot)
    {
        if (!ghostInstance)
        {
            ghostInstance = CreateGhostVisual();
        }

        if (!ghostInstance)
        {
            return;
        }

        ghostInstance.transform.SetPositionAndRotation(posePos, poseRot);
        ghostInstance.SetActive(true);
    }

    GameObject CreateGhostVisual()
    {
        var ghost = Instantiate(gameObject);
        ghost.name = name + "_Ghost";

        foreach (var pipe in ghost.GetComponentsInChildren<Pipe>(true))
        {
            Destroy(pipe);
        }

        foreach (var body in ghost.GetComponentsInChildren<Rigidbody>(true))
        {
            Destroy(body);
        }

        foreach (var col in ghost.GetComponentsInChildren<Collider>(true))
        {
            Destroy(col);
        }

        foreach (var outline in ghost.GetComponentsInChildren<ObjectOutline>(true))
        {
            Destroy(outline);
        }

        var mat = ghostMaterial;
        if (!mat)
        {
            if (!runtimeGhostMaterial)
            {
                runtimeGhostMaterial = CreateRuntimeGhostMaterial();
            }

            mat = runtimeGhostMaterial;
        }

        foreach (var renderer in ghost.GetComponentsInChildren<Renderer>(true))
        {
            var mats = new Material[renderer.sharedMaterials.Length];
            for (var i = 0; i < mats.Length; i++)
            {
                mats[i] = mat;
            }

            renderer.sharedMaterials = mats;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        return ghost;
    }

    Material CreateRuntimeGhostMaterial()
    {
        var shader = Shader.Find("HDRP/Unlit");
        if (!shader)
        {
            shader = Shader.Find("Unlit/Color");
        }

        var mat = new Material(shader);
        var color = ghostColor;
        // Keep alpha readable even if inspector alpha was left at 1 on older instances.
        if (color.a >= 0.99f)
        {
            color.a = 0.32f;
        }

        if (mat.HasProperty("_UnlitColor"))
        {
            mat.SetColor("_UnlitColor", color);
        }

        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }

        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", color);
        }

        // Proper HDRP transparent setup (keywords, blend, render queue).
        HDMaterial.SetSurfaceType(mat, transparent: true);
        HDMaterial.SetRenderingPass(mat, HDMaterial.RenderingPass.Default);

        if (mat.HasProperty("_ZWrite"))
        {
            mat.SetFloat("_ZWrite", 0f);
        }

        return mat;
    }

    public void RotateAlongConnection(float degrees)
    {
        if (!isSnapped)
        {
            return;
        }

        if (!TryGetConnectionAxis(out var pivot, out var axis))
        {
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
        if (!isSnapped || !TryGetConnectionAxis(out var pivot, out var axis))
        {
            return;
        }

        axis.Normalize();
        EnsureTwistReference(axis);

        var marker = Vector3.ProjectOnPlane(GetTwistMarkerDirection(), axis);
        if (marker.sqrMagnitude < MinOutwardSqrMagnitude)
        {
            RotateAlongConnection(snappedRotateStepDegrees);
            return;
        }

        marker.Normalize();
        var angle = Mathf.Repeat(Vector3.SignedAngle(twistReferenceDir, marker, axis), 360f);
        var step = snappedRotateStepDegrees > 0f ? snappedRotateStepDegrees : 45f;
        var next = Mathf.Floor(angle / step) * step + step;
        if (next >= 360f)
        {
            next -= 360f;
        }

        var delta = Mathf.Repeat(next - angle, 360f);
        if (delta < 0.01f)
        {
            delta = step;
        }

        RotateAlongConnection(delta);
    }

    private void CaptureTwistReference()
    {
        if (!TryGetConnectionAxis(out _, out var axis))
        {
            twistReferenceDir = Vector3.zero;
            return;
        }

        SetWorldTwistReference(axis.normalized);
    }

    private void EnsureTwistReference(Vector3 axis)
    {
        if (twistReferenceDir.sqrMagnitude > MinOutwardSqrMagnitude)
        {
            var projected = Vector3.ProjectOnPlane(twistReferenceDir, axis);
            if (projected.sqrMagnitude > MinOutwardSqrMagnitude)
            {
                twistReferenceDir = projected.normalized;
                return;
            }
        }

        SetWorldTwistReference(axis);
    }

    private void SetWorldTwistReference(Vector3 axis)
    {
        var reference = Vector3.ProjectOnPlane(Vector3.up, axis);
        if (reference.sqrMagnitude < MinOutwardSqrMagnitude)
        {
            reference = Vector3.ProjectOnPlane(Vector3.right, axis);
        }

        twistReferenceDir = reference.normalized;
    }

    private Vector3 GetTwistMarkerDirection()
    {
        Transform freeEnd = null;
        if (connectedEnd == startEnd)
        {
            freeEnd = endEnd;
        }
        else if (connectedEnd == endEnd)
        {
            freeEnd = startEnd;
        }
        else if (endEnd)
        {
            freeEnd = endEnd;
        }
        else
        {
            freeEnd = startEnd;
        }

        if (freeEnd)
        {
            return freeEnd.position - transform.position;
        }

        return transform.up;
    }

    public void ClearTwistReference()
    {
        twistReferenceDir = Vector3.zero;
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
            Transform closestEnd = null;
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
                    closestEnd = end;
                }
            }

            if (closestEnd)
            {
                pivot = closestEnd.position;
                axis = previousPipe.GetOutward(closestEnd);
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

    void LockPipe()
    {
        isSnapped = true;
        if (rb)
        {
            rb.isKinematic = true;
        }

        isBeingHeld = false;
        ClearSnapPreview();
        CaptureTwistReference();

        if (playerInteraction)
        {
            playerInteraction.NotifyPipeSnapped(this);
        }
        else if (PlayerInteraction.Instance)
        {
            PlayerInteraction.Instance.NotifyPipeSnapped(this);
        }
    }

    public void SetHeldState(bool held)
    {
        isBeingHeld = held;

        if (!held)
        {
            ClearSnapPreview();
        }

        if (held)
        {
            rb.isKinematic = false;
        }
        else if (!isSnapped)
        {
            rb.isKinematic = false;
        }
        else
        {
            rb.isKinematic = true;
        }
    }
}

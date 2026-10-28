using UnityEngine;

/// <summary>
/// Marks water / pumps as valid supply sources for the pipe network.
/// Place on water resources and pump instances.
/// </summary>
public class PipeNetworkSource : MonoBehaviour
{
    [Tooltip("Optional outlet transform pipes snap to. Falls back to this transform.")]
    public Transform connectPoint;

    [Tooltip("How close a pipe end must be to count as connected to this source.")]
    public float connectRadius = 1.5f;

    public Transform AttachTransform => connectPoint ? connectPoint : transform;

    public Vector3 GetAttachPosition()
    {
        return AttachTransform.position;
    }

    public Vector3 GetOutward()
    {
        var attach = AttachTransform;
        var forward = attach.forward;
        if (forward.sqrMagnitude > 1e-6f)
        {
            return forward.normalized;
        }

        return transform.forward.sqrMagnitude > 1e-6f ? transform.forward.normalized : Vector3.forward;
    }

    /// <summary>
    /// Picks a child named Outlet / Connect / PipeAttach when no connectPoint is assigned.
    /// </summary>
    public void TryAutoAssignConnectPoint()
    {
        if (connectPoint)
        {
            return;
        }

        foreach (var name in new[] { "Outlet", "Connect", "PipeAttach", "SnapPoint" })
        {
            var child = transform.Find(name);
            if (child)
            {
                connectPoint = child;
                return;
            }
        }
    }
}

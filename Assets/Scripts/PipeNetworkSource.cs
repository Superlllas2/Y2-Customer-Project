using UnityEngine;

/// <summary>
/// Marks water / pumps as valid supply sources for the pipe network.
/// Place on water resources and pump instances.
/// </summary>
public class PipeNetworkSource : MonoBehaviour
{
    [Tooltip("How close a pipe end must be to count as connected to this source.")]
    public float connectRadius = 1.5f;
}

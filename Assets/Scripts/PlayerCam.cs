using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Header("Look")]
    [Tooltip("Horizontal look sensitivity (Inspector). Lower = slower.")]
    public float senseX = 15f;
    [Tooltip("Vertical look sensitivity (Inspector). Lower = slower.")]
    public float senseY = 15f;

    [Tooltip("Extra scale for Input System mouse delta (pixels).")]
    public float lookScale = 0.01f;

    public Transform orientation;

    private float xRotation;
    private float yRotation;

    private void Start()
    {
        if (GameInput.Instance == null || !GameInput.Instance.IsControlsOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (GameInput.Instance != null && GameInput.Instance.IsControlsOpen)
        {
            return;
        }

        var look = GameInput.Instance != null ? GameInput.Instance.LookDelta : Vector2.zero;
        var mouseX = look.x * senseX * lookScale;
        var mouseY = look.y * senseY * lookScale;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}

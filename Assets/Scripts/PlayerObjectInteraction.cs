using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance { get; private set; }

    public float rayDistance = 10f;
    public float sphereRadius = 0.5f;
    public LayerMask interactableLayer;
    public float grabSpeed = 10f;
    public float throwForce = 500f;
    public float holdDistance = 2f;

    private ObjectOutline lastOutlinedObject;
    private Rigidbody grabbedObjectRb;
    private Camera cam;
    private Pipe grabbedPipe;
    private Pipe lookedSnappedPipe;

    public PlayerMovement PlayerMovement;
    public MonoBehaviour cameraController;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        cam = Camera.main;
    }

    void OnDisable()
    {
        SetLookedSnappedPipe(null);
    }

    void Update()
    {
        if (GameInput.Instance != null && GameInput.Instance.IsControlsOpen)
        {
            SetLookedSnappedPipe(null);
            return;
        }

        var input = GameInput.Instance;

        if (!cam)
        {
            cam = Camera.main;
            if (!cam)
            {
                return;
            }
        }

        // If the held pipe snapped underneath us, drop the grab immediately.
        if (grabbedPipe && grabbedPipe.isSnapped)
        {
            ClearGrab(keepPipeLocked: true);
        }

        var ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        Pipe snappedUnderCrosshair = null;

        if (Physics.SphereCast(ray, sphereRadius, out var hit, rayDistance, interactableLayer))
        {
            var outlineScript = hit.collider.GetComponentInParent<ObjectOutline>();
            if (outlineScript)
            {
                if (lastOutlinedObject != outlineScript)
                {
                    if (lastOutlinedObject)
                    {
                        lastOutlinedObject.RemoveOutline();
                    }

                    outlineScript.ApplyOutline();
                    lastOutlinedObject = outlineScript;
                }

                TryGrabObject(hit);
            }

            var hitPipe = hit.collider.GetComponentInParent<Pipe>();
            if (hitPipe && hitPipe.isSnapped && grabbedObjectRb == null)
            {
                snappedUnderCrosshair = hitPipe;
            }
        }
        else if (lastOutlinedObject)
        {
            lastOutlinedObject.RemoveOutline();
            lastOutlinedObject = null;
        }

        SetLookedSnappedPipe(snappedUnderCrosshair);

        if (lookedSnappedPipe && input != null && input.RotatePressed)
        {
            lookedSnappedPipe.RotateAlongConnectionStep();
        }

        if (grabbedObjectRb)
        {
            MoveObject();
            if (input != null && input.InteractReleased)
            {
                ReleaseObject();
            }
        }
    }

    /// <summary>Called from Pipe when it locks into a snap while still "held".</summary>
    public void NotifyPipeSnapped(Pipe pipe)
    {
        if (grabbedPipe == pipe || (grabbedObjectRb && pipe && grabbedObjectRb.gameObject == pipe.gameObject))
        {
            ClearGrab(keepPipeLocked: true);
        }
    }

    void ClearGrab(bool keepPipeLocked)
    {
        if (grabbedPipe && !keepPipeLocked)
        {
            grabbedPipe.SetHeldState(false);
        }

        grabbedPipe = null;
        grabbedObjectRb = null;
    }

    void SetLookedSnappedPipe(Pipe pipe)
    {
        if (lookedSnappedPipe == pipe)
        {
            return;
        }

        lookedSnappedPipe = pipe;
        ControlsPanelController.Instance?.SetRotatePromptVisible(pipe != null);
    }

    void TryGrabObject(RaycastHit hit)
    {
        if (GameInput.Instance == null || !GameInput.Instance.InteractPressed)
        {
            return;
        }

        var hitRb = hit.collider.GetComponentInParent<Rigidbody>();
        if (!hitRb)
        {
            return;
        }

        var pipeComponent = hit.collider.GetComponentInParent<Pipe>();

        if (!hitRb.isKinematic)
        {
            if (!pipeComponent)
            {
                return;
            }

            grabbedObjectRb = hitRb;
            grabbedPipe = pipeComponent;
            grabbedPipe.SetHeldState(true);
            grabbedObjectRb.useGravity = false;
            grabbedObjectRb.linearDamping = 10f;
            return;
        }

        // Kinematic snapped pipe: disconnect the chain.
        if (pipeComponent)
        {
            DisconnectPipesUsingBFS(pipeComponent);
        }
    }

    void MoveObject()
    {
        if (!grabbedObjectRb || (grabbedPipe && grabbedPipe.isSnapped))
        {
            return;
        }

        if (grabbedObjectRb.isKinematic)
        {
            grabbedObjectRb.isKinematic = false;
        }

        var holdPosition = cam.transform.position + cam.transform.forward * holdDistance;
        var directionToHoldPoint = holdPosition - grabbedObjectRb.position;
        grabbedObjectRb.linearVelocity = directionToHoldPoint * grabSpeed;
    }

    void ReleaseObject()
    {
        if (!grabbedObjectRb)
        {
            return;
        }

        if (grabbedPipe && grabbedPipe.isSnapped)
        {
            ClearGrab(keepPipeLocked: true);
            return;
        }

        if (grabbedPipe)
        {
            grabbedPipe.SetHeldState(false);
        }

        grabbedObjectRb.useGravity = true;
        grabbedObjectRb.linearDamping = 1f;
        grabbedObjectRb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);

        grabbedPipe = null;
        grabbedObjectRb = null;
    }

    private void DisconnectPipesUsingBFS(Pipe initialPipe)
    {
        PipeNetwork.DisconnectFrom(initialPipe);
        SetLookedSnappedPipe(null);
    }
}

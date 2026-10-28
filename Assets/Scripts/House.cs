using UnityEngine;

public class House : MonoBehaviour
{
    private IconAboveObject iconAboveObject;
    private Pipe touchingPipe;

    void Start()
    {
        iconAboveObject = GetComponent<IconAboveObject>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        var pipe = collision.GetComponentInParent<Pipe>();
        if (pipe && pipe.isSnapped)
        {
            touchingPipe = pipe;
            RefreshSupplyState();
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        var pipe = collision.GetComponentInParent<Pipe>();
        if (pipe && pipe.isSnapped)
        {
            touchingPipe = pipe;
            RefreshSupplyState();
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        var pipe = collision.GetComponentInParent<Pipe>();
        if (pipe && pipe == touchingPipe)
        {
            touchingPipe = null;
            RefreshSupplyState();
        }
    }

    private void RefreshSupplyState()
    {
        if (!iconAboveObject || iconAboveObject.wasHouseWatered || iconAboveObject.wasHouseDead)
        {
            return;
        }

        var supplied = touchingPipe
                       && touchingPipe.isSnapped
                       && PipeNetwork.HasPathToSource(touchingPipe);

        iconAboveObject.SetConnectionState(supplied);
    }
}

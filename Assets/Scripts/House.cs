using UnityEngine;

public class House : MonoBehaviour
{
    private Renderer houseRenderer;
    private IconAboveObject iconAboveObject;
    
    void Start()
    {
        iconAboveObject = GetComponent<IconAboveObject>();
        houseRenderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        var pipe = collision.gameObject.GetComponent<Pipe>();

        if (pipe && pipe.isSnapped)
        {
            iconAboveObject.isConnected = true;
        }
    }
}
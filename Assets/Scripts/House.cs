using UnityEngine;

public class House : MonoBehaviour
{
    private Renderer houseRenderer;
    public bool isConnected;
    
    void Start()
    {
        houseRenderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        var pipe = collision.gameObject.GetComponent<Pipe>();

        if (pipe && pipe.isSnapped)
        {
            isConnected = true;
        }
    }
}
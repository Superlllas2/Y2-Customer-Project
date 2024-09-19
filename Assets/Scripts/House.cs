using UnityEngine;

public class HouseColorChange : MonoBehaviour
{
    // Reference to the Renderer component
    private Renderer houseRenderer;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Renderer component of the house
        houseRenderer = GetComponent<Renderer>();
    }

    // Detect collisions
    private void OnTriggerEnter(Collider collision)
    {
        // Check if the object that collided is a Pipe
        Pipe pipe = collision.gameObject.GetComponent<Pipe>();

        // If the object is a Pipe and isSnapped is true
        if (pipe != null && pipe.isSnapped)
        {
            Debug.Log("Works");
            // Change the color of the house to blue
            houseRenderer.material.color = Color.blue;
        }
    }
}
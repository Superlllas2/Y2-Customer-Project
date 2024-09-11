using UnityEngine;

public class ObjectOutline : MonoBehaviour
{
    public Material outlineMaterial;  // The material with the outline shader
    public Material defaultMaterial;  // The default material of the object

    private Renderer objectRenderer;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectRenderer.material = defaultMaterial;  // Set to default material initially
    }

    public void ApplyOutline()
    {
        objectRenderer.material = outlineMaterial;  // Apply the outline material
    }

    public void RemoveOutline()
    {
        objectRenderer.material = defaultMaterial;  // Revert to the default material
    }
}
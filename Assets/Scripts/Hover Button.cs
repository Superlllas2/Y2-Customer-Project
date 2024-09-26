using UnityEngine;

public class HoverButton : MonoBehaviour
{
    public float hoverScaleMultiplier = 1.2f; // The scale multiplier when hovering
    public float scaleSpeed = 5f; // Speed of scaling up and down

    private Vector3 originalScale; // Store the original scale
    private Vector3 targetScale; // The scale we are transitioning towards
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    private void OnMouseEnter()
    {
        Debug.Log("MouseHovering");
        targetScale = originalScale * hoverScaleMultiplier;
        isHovered = true;
    }

    private void OnMouseExit()
    {
        Debug.Log("MouseNotHovering");
        targetScale = originalScale;
        isHovered = false;
    }
}
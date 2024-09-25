using UnityEngine;

public class IconAboveObject : MonoBehaviour
{
    public Transform iconPrefab;
    public float iconHeight = 2f; 
    public float rotationSpeed = 30f;
    
    private Transform iconInstance;

    void Start()
    {
        iconInstance = Instantiate(iconPrefab, transform.position + Vector3.up * iconHeight, Quaternion.identity);
        iconInstance.SetParent(transform);

        iconInstance.LookAt(Camera.main.transform);
    }

    void Update()
    {
        if (iconInstance)
        {
            iconInstance.position = transform.position + Vector3.up * iconHeight;
            iconInstance.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
using UnityEngine;

public class IconAboveObject : MonoBehaviour
{
    [Header("Prefab Settings")]
    public Transform iconPrefab; // The default icon prefab
    public Transform secondaryPrefab; // The second state icon prefab (optional)
    
    [Header("Icon Settings")]
    public float iconHeight = 2f; // Height above the object
    public float rotationSpeed = 30f; // Rotation speed of the icon
    public float timer = 60f; // Timer for changing states

    [Header("State Settings")]
    public bool isSecondStateNeeded; // Toggle for enabling the second state
    public bool hideWhenConnected; // Hide the icon if the object gets connected (for use with objects like 'House')

    private Transform iconInstance;
    private float currentTimer;
    private bool isConnected = false; // Flag for connection state (can be set externally)

    private void Start()
    {
        // Initialize the icon
        SpawnIcon(iconPrefab);
        currentTimer = timer;
    }

    private void Update()
    {
        // Rotate and position the icon above the object
        if (iconInstance)
        {
            iconInstance.position = transform.position + Vector3.up * iconHeight;
            iconInstance.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Optionally hide the icon if the object is connected (for scenarios like 'House')
            if (hideWhenConnected && isConnected)
            {
                iconInstance.gameObject.SetActive(false);
            }

            // Handle the state change if the second state is needed
            if (isSecondStateNeeded)
            {
                currentTimer -= Time.deltaTime;
                if (currentTimer <= 0f)
                {
                    ChangeIcon();
                    currentTimer = timer; // Reset the timer
                }
            }
        }
    }

    private void SpawnIcon(Transform prefab)
    {
        if (iconInstance)
        {
            Destroy(iconInstance.gameObject); // Clean up any previous icon
        }

        // Instantiate and set the icon prefab
        iconInstance = Instantiate(prefab, transform.position + Vector3.up * iconHeight, Quaternion.identity);
        iconInstance.SetParent(transform);
        // iconInstance.LookAt(Camera.main.transform); // Make the icon face the camera
    }

    private void ChangeIcon()
    {
        // Spawn the secondary icon if the first state has ended
        SpawnIcon(secondaryPrefab);
    }

    // Optional method to update connection status externally
    public void SetConnectionState(bool connected)
    {
        isConnected = connected;
    }
}

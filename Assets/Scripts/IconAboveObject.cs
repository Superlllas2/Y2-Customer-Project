using UnityEngine;

public class IconAboveObject : MonoBehaviour
{
    [Header("Prefab Settings")]
    public Transform iconPrefab; // The default icon prefab
    public Transform secondaryPrefab; // The second state icon prefab (optional)
    public Transform thirdPrefab; // The second state icon prefab (optional)
    
    [Header("Icon Settings")]
    public float iconHeight = 2f; // Height above the object
    public float rotationSpeed = 30f; // Rotation speed of the icon
    public float timer = 60f; // Timer for changing states

    [Header("State Settings")]
    public bool isSecondStateNeeded; // Toggle for enabling the second state
    public bool hideWhenConnected; // Hide the icon if the object gets connected (for use with objects like 'House')

    private Transform iconInstance;
    private float currentTimer;
    public bool isConnected = false; // Flag for connection state (can be set externally)
    private bool wasHouseDead = false;
    private bool wasHouseWatered = false;

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
            if (isConnected && !wasHouseWatered)
            {
                if (currentTimer > 0f)
                {
                    ChangeIcon(secondaryPrefab);
                    wasHouseWatered = true;
                }
            }

            // Handle the state change if the second state is needed
            if (isSecondStateNeeded && !wasHouseWatered)
            {
                currentTimer -= Time.deltaTime;
                if (currentTimer <= 0f)
                {
                    ChangeIcon(thirdPrefab);
                    wasHouseDead = true;
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

    private void ChangeIcon(Transform prefab)
    {
        if (!wasHouseDead)
        {
            Debug.Log(iconInstance.name);

            // Check if the current icon is already the thirdPrefab, and if so, don't change it
            if (iconInstance && iconInstance.name == thirdPrefab.name)
            {
                return; // Exit the method, as we don't want to change the icon further
            }

            // Spawn the new icon if the current one is not the thirdPrefab
            SpawnIcon(prefab);
        }

    }

    // Optional method to update connection status externally
    public void SetConnectionState(bool connected)
    {
        isConnected = connected;
    }
}

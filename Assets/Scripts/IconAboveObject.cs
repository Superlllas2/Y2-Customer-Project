using UnityEngine;
using UnityEngine.Serialization;

public class IconAboveObject : MonoBehaviour
{
    public Transform needWaterPrefab;
    public Transform houseIsDead;
    public float iconHeight = 2f; 
    public float rotationSpeed = 30f;
    public House house;
    public float timer = 60f;
    private Transform iconInstance;
    private float currentTimer;

    void Start()
    {
        house = GetComponentInParent<House>();
        iconInstance = Instantiate(needWaterPrefab, transform.position + Vector3.up * iconHeight, Quaternion.identity);
        iconInstance.SetParent(transform);
        iconInstance.LookAt(Camera.main.transform);
        currentTimer = timer;
    }

    void Update()
    {
        if (iconInstance)
        {
            iconInstance.position = transform.position + Vector3.up * iconHeight;
            iconInstance.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            Debug.Log(house);
            Debug.Log(house.isConnected);
            if (house && house.isConnected)
            {
                Debug.Log("we get here");
                iconInstance.gameObject.SetActive(false);
            }
            
            currentTimer -= Time.deltaTime;
            if (currentTimer <= 0f)
            {
                ChangeIcon();
                currentTimer = timer;
            }
        }
    }
    
    void ChangeIcon()
    {
        if (iconInstance)
        {
            Destroy(iconInstance.gameObject);
        }

        iconInstance = Instantiate(houseIsDead, transform.position + Vector3.up * iconHeight, Quaternion.identity);
        iconInstance.SetParent(transform);
        iconInstance.LookAt(Camera.main.transform); // Make the new icon look at the camera
        Debug.Log("Icon changed!");
    }
}
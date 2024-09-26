using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//Some of this Code was made by the designers (pls ignore it)
// CREDIT TO: Luis (+500 social points)

public class WaterDetector : MonoBehaviour
{
    public Transform[] waters; // Array of water objects
    // public AudioSource beepSound;
    public float detectionRange = 10f;
    public float minBeepInterval = 0.1f;
    public float maxBeepInterval = 2f;
    public bool HoldingDetector = false;
    public GameObject Water_Detector;

    private float beepTimer = 0f;
    private Transform closestWater; // The closest water source

    private void Start()
    {

        closestWater = null; // Start with no closest water source
        // if (Water_Detector == null)
        //
        //     Water_Detector = transform.Find("Water_Detector").gameObject; 

        if (Water_Detector == null)
        {
            Debug.Log("The Water_Detector reference is missing or not found in the hierarchy.");
        }
    }

    private void Update()
    {
        closestWater = FindClosestWater();

        if (closestWater)
        { 
     
            var distanceToWater = Vector3.Distance(transform.position, closestWater.position);

                // Debug.Log("Closest water source is at a distance of: " + distanceToWater);

                if (distanceToWater <= detectionRange && HoldingDetector == true)
                {
                    float beepInterval = Mathf.Lerp(minBeepInterval, maxBeepInterval, distanceToWater / detectionRange);

                    beepTimer += Time.deltaTime;
                    if (beepTimer >= beepInterval)
                    {
                        Beep();
                        beepTimer = 0f;
                }
            }
            else
            {
                // beepSound.Stop();
            }
        }
        else
        {
            // beepSound.Stop();
        }


        if (Input.GetKeyDown(KeyCode.E) && HoldingDetector == true)  
        {
            HoldingDetector = false;
        }
        else if (Input.GetKeyDown(KeyCode.E) && HoldingDetector == false)  
        {
            HoldingDetector = true;
        }

        if (HoldingDetector == false)
        {
            if(Water_Detector) Water_Detector.SetActive(HoldingDetector);
        }
        else if (HoldingDetector == true)
        {
            if(Water_Detector) Water_Detector.SetActive(HoldingDetector);
        }
    }

    private Transform FindClosestWater()
    {
        Transform nearest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform water in waters)
        {
            float distance = Vector3.Distance(transform.position, water.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearest = water;
            }
        }

        return nearest;
    }

    private void Beep()
    {
        // if (!beepSound.isPlaying)
        // {
        //     beepSound.Play();
        // }
        Debug.Log("Getting closer to water");
    }
}

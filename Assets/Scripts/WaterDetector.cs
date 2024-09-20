using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Some of this Code was made by the designers (pls ignore it)
// CREDIT TO: Luis (+500 social points)


public class WaterDetector : MonoBehaviour
{
    public Transform water;
    public AudioSource beepSound;
    public float detectionRange = 10f;
    public float minBeepInterval = 0.1f; 
    public float maxBeepInterval = 2f; 

    private float beepTimer = 0f;
    
    private void Start()
    {
        
    }

    private void Update()
    {
        var distanceToWater = Vector3.Distance(transform.position, water.position);

        if (distanceToWater <= detectionRange)
        {
            var beepInterval = Mathf.Lerp(minBeepInterval, maxBeepInterval, distanceToWater / detectionRange);

            beepTimer += Time.deltaTime;
            if (beepTimer >= beepInterval)
            {
                Beep();
                beepTimer = 0f;
            }
        }
        else
        {
            beepSound.Stop();
        }
    }
    private void Beep()
    {
        if (!beepSound.isPlaying)
        {
            beepSound.Play();
        }
        Debug.Log("Getting closer to water");
    }
}
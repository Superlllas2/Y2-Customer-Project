using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Pump : MonoBehaviour
{
    public GameObject ground;
    public GameObject pump;
    private Vector3 playerOrientation;
    private Transform childTransform;
    
    private void Update()
    {
        childTransform = transform.Find("Orientation");
        if (childTransform)
        { 
            playerOrientation = childTransform.position;
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
        {
            PlacePump();
        }
    }
    
    

    private void PlacePump()
    {
        var spawnPosition = playerOrientation + (childTransform.forward * 3);
        Instantiate(pump, new Vector3(spawnPosition.x, ground.transform.position.y + 0.9f, spawnPosition.z), childTransform.rotation * Quaternion.Euler(0, 90, 0));
    }
}
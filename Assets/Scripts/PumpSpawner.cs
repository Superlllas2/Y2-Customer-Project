using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using UnityEngine;

public class Pump : MonoBehaviour
{
    public GameObject ground;
    public GameObject pump;
    private Vector3 playerOrientation;
    private Transform childTransform;
    public int numPumpForSpawn = 4;
    private int placedPumps = 0;
    private Renderer objectRenderer;
    private float halfSizeY;

    private void Start()
    {
        objectRenderer = pump.GetComponent<Renderer>();
        
        if (objectRenderer != null)
        {
            halfSizeY = objectRenderer.bounds.size.y/2;
            Debug.Log("Y size of the object: " + halfSizeY);
        }
    }

    private void Update()
    {
        childTransform = transform.Find("Orientation");
        if (childTransform)
        { 
            playerOrientation = childTransform.position;
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (placedPumps >= numPumpForSpawn)
            {
                return;
            }
            PlacePump();
            placedPumps++;
        }
    }

    private void PlacePump()
    {
        var spawnPosition = playerOrientation + (childTransform.forward * 3);
        Instantiate(pump, new Vector3(spawnPosition.x, ground.transform.position.y + halfSizeY - 0.2f, spawnPosition.z), childTransform.rotation);
    }
}
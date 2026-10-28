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
        }
    }

    private void Update()
    {
        childTransform = transform.Find("Orientation");
        if (childTransform)
        { 
            playerOrientation = childTransform.position;
        }
        
        if (GameInput.Instance != null && GameInput.Instance.SpawnPumpPressed)
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
        var instance = Instantiate(
            pump,
            new Vector3(spawnPosition.x, ground.transform.position.y + halfSizeY - 0.2f, spawnPosition.z),
            childTransform.rotation);

        if (instance)
        {
            var source = instance.GetComponent<PipeNetworkSource>();
            if (!source)
            {
                source = instance.AddComponent<PipeNetworkSource>();
            }

            source.connectRadius = 2f;
            source.TryAutoAssignConnectPoint();
        }
    }
}
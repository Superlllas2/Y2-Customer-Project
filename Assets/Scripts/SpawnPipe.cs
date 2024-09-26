using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnPipe : MonoBehaviour
{
    public PipeType pipeType;
    public GameObject straightPipe;
    public GameObject cornerPipe;
    public GameObject sanitiser;

    private string targetTag = "PipeSpawn";
    private IconAboveObject iconAboveObject;
    public Player player;
    private Transform playerOrientation;

    private void Start()
    {
        iconAboveObject = FindObjectOfType<IconAboveObject>();
        playerOrientation = player.transform.Find("Orientation");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider && hit.collider.gameObject == gameObject && hit.collider.CompareTag(targetTag))
                {
                    SpawnPrefab();
                }
            }
        }
    }

    private void SpawnPrefab()
    {
        switch(pipeType)
        {
            case PipeType.Straight:
                Instantiate(straightPipe, playerOrientation.transform.position + new Vector3(0, 0, 0.0005f), Quaternion.identity);
                break;
            case PipeType.Bend:
                Instantiate(cornerPipe, playerOrientation.transform.position + new Vector3(0, 0, 0.0005f), Quaternion.identity);
                break;
            case PipeType.Sanitizer:
                Instantiate(sanitiser, playerOrientation.transform.position + new Vector3(0, 0, 0.0005f), Quaternion.identity);
                break;
        }

    }
}
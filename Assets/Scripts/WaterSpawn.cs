using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaterSpawn : MonoBehaviour
{
    public float spawnRadius = 10f; // the map size
    public GameObject[] waterPrefabs; // Array of prefabs to reposition
    public Transform villageLocation;
    public int numPrefabsToSpawn = 4; // number of prefabs to reposition
    public float minDistanceToVillage = 20f; // minimum distance from village to spawn
    public float minDistanceToEachOther = 12f;

    private Transform lastCreated = null; // Last water prefab placed on the map
    private GameObject[] spawnedPrefabs;

    private void Start()
    {
        spawnedPrefabs = new GameObject[numPrefabsToSpawn];
        
        for (var i = 0; i < numPrefabsToSpawn; i++)
        {
            spawnedPrefabs[i] = RepositionPrefab(waterPrefabs[i]);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (spawnedPrefabs != null)
            {
                foreach (var prefab in spawnedPrefabs)
                {
                    prefab.SetActive(false); // Deactivate the previously placed prefabs
                }
            }

            spawnedPrefabs = new GameObject[numPrefabsToSpawn];
            for (var i = 0; i < numPrefabsToSpawn; i++)
            {
                spawnedPrefabs[i] = RepositionPrefab(waterPrefabs[i]);
            }
        }
    }

    private GameObject RepositionPrefab(GameObject prefab)
    {
        Vector3 spawnPosition = Vector3.zero;
        bool validPosition = false;
        for (int attempts = 0; attempts < 1000; attempts++)
        {
            var randomPosInCircle = Random.insideUnitCircle * spawnRadius;
            spawnPosition = new Vector3(randomPosInCircle.x, 0, randomPosInCircle.y);
            bool farFromVillage = Vector3.Distance(spawnPosition, villageLocation.position) >= minDistanceToVillage;
            bool farFromLastCreated = lastCreated == null || Vector3.Distance(spawnPosition, lastCreated.position) >= minDistanceToEachOther;
            if (farFromVillage && farFromLastCreated)
            {
                validPosition = true;
                break;
            }
        }

        if (!validPosition)
        {
            Debug.LogWarning("Could not find a valid spawn position within the allowed attempts.");
            return null;
        }
        prefab.transform.position = spawnPosition;
        prefab.SetActive(true);
        
        lastCreated = prefab.transform;

        return prefab;
    }
}

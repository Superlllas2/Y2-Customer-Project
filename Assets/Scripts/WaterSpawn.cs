using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaterSpawn : MonoBehaviour
{
    public float spawnRadius = 10f; // the map size
    public GameObject waterPrefab;
    public Transform villageLocation;
    public int numPrefabsToSpawn = 5; // number of prefabs to spawn
    public float minDistanceToVillage = 20f; // minimum distance from village to spawn
    public float minDistanceToEachOther = 12f;

    private Transform lastCreated = null;
    private GameObject[] spawnedPrefabs;

    private void Start()
    {
        spawnedPrefabs = new GameObject[numPrefabsToSpawn];
        for (var i = 0; i < numPrefabsToSpawn; i++)
        {
            spawnedPrefabs[i] = SpawnPrefab();
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
                    Destroy(prefab);
                }
            }

            spawnedPrefabs = new GameObject[numPrefabsToSpawn];
            for (var i = 0; i < numPrefabsToSpawn; i++)
            {
                spawnedPrefabs[i] = SpawnPrefab();
            }
        }
    }

    private GameObject SpawnPrefab()
    {
        var spawnPosition = Vector3.zero;
        var validPosition = false;

        for (var attempts = 0; attempts < 1000; attempts++)
        {
            var randomPosInCircle = Random.insideUnitCircle * spawnRadius;
            spawnPosition = new Vector3(randomPosInCircle.x, 0, randomPosInCircle.y);
            
            var farFromVillage = Vector3.Distance(spawnPosition, villageLocation.position) >= minDistanceToVillage;
            
            var farFromLastCreated = !lastCreated ||
                                      Vector3.Distance(spawnPosition, lastCreated.position) >= minDistanceToEachOther;

            if (farFromVillage && farFromLastCreated)
            {
                validPosition = true;
                break; // Exit the loop once a valid position is found
            }
        }

        if (!validPosition)
        {
            Debug.LogWarning("Could not find a valid spawn position within the allowed attempts.");
            return null;
        }

        var newWaterPrefab = Instantiate(waterPrefab, spawnPosition, Quaternion.identity);
        // Debug.Log("Distance between water and village: " + (Vector3.Distance(spawnPosition, villageLocation.position)) +
        //           " < " + minDistanceToVillage + " " +
        //           (Vector3.Distance(spawnPosition, villageLocation.position) < minDistanceToVillage));

        lastCreated = newWaterPrefab.transform;

        return newWaterPrefab;
    }
}
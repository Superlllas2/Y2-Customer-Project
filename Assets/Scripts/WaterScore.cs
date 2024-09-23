using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterScore : MonoBehaviour
{
    private float score;
    public GameObject waterSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            var pos1 = new Vector2(other.transform.position.x, other.transform.position.z);
            var pos2 = new Vector2(transform.position.x, transform.position.z);
            var playerScript = FindObjectOfType<Player>();
            if (playerScript)
            {
                playerScript.IncrementPlayerScore(CalculateScore(Vector2.Distance(pos1, pos2)));
            }
        }
    }

    private float CalculateScore(float distance)
    {
        var waterRadius = waterSource.transform.localScale.x / 2;
        return Mathf.Clamp(100 * (1 - (distance / waterRadius)), 0, 100);
    }
}
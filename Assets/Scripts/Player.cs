using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float playerScore = 0;

    private void Start()
    {
        UIManager.Instance.UpdateScore((int)playerScore);
    }

    public void IncrementPlayerScore(float scoreIncrement)
    {
        playerScore += scoreIncrement;
        UIManager.Instance.UpdateScore((int)playerScore);
        Debug.Log("Player Score incremented by: " + scoreIncrement + " | Total Score: " + playerScore);
    }
}
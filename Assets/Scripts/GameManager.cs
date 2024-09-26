using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public IconAboveObject iconAboveObject;
    public Player player;
    public AudioManager audioManager;

    private void Update()
    {
        if (iconAboveObject.isConnected)
        {
            if (iconAboveObject.wasHouseWatered)
            {
                StartCoroutine(WaitAndLoadScene(5f));
                player.playerScore *= 2;
            } else if (iconAboveObject.wasHouseDead)
            {
                StartCoroutine(WaitAndLoadScene(5f));
                player.playerScore = 0;
            }
        }
    }

    // Coroutine that waits for a given time and then loads the scene
    private IEnumerator WaitAndLoadScene(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("Main_Menu");
    }
}


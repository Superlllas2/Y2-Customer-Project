using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public IconAboveObject iconAboveObject;
    public Player player;
    public AudioManager audioManager;
    public float returnToMenuDelay = 5f;

    private bool gameEnded;

    private void Update()
    {
        if (gameEnded || iconAboveObject == null)
        {
            return;
        }

        // Win: house was supplied before the timer ran out
        if (iconAboveObject.wasHouseWatered)
        {
            EndGame(won: true);
            return;
        }

        // Lose: timer expired without a successful supply (does not require isConnected)
        if (iconAboveObject.wasHouseDead)
        {
            EndGame(won: false);
        }
    }

    private void EndGame(bool won)
    {
        gameEnded = true;

        if (player != null)
        {
            if (won)
            {
                player.playerScore *= 2f;
            }
            else
            {
                player.playerScore = 0f;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScore((int)player.playerScore);
            }
        }

        StartCoroutine(WaitAndLoadScene(returnToMenuDelay));
    }

    private IEnumerator WaitAndLoadScene(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("Main_Menu");
    }
}

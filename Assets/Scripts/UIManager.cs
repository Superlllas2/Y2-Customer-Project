using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    public IconAboveObject iconAboveObject;

    private int lastDisplayedTimer = int.MinValue;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!timerText || !iconAboveObject)
        {
            return;
        }

        var secondsLeft = (int)iconAboveObject.currentTimer;
        if (secondsLeft == lastDisplayedTimer)
        {
            return;
        }

        lastDisplayedTimer = secondsLeft;
        timerText.SetText("Time left: {0}", secondsLeft);
    }

    public void UpdateScore(int score)
    {
        if (scoreText)
        {
            scoreText.SetText("Score: {0}", score);
        }
    }
}

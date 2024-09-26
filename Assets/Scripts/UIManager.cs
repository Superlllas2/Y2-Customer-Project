using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    public IconAboveObject iconAboveObject;
    
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
        timerText.text = "Time left: " + (int)iconAboveObject.currentTimer;
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score;
    }
}

using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float timeLimit = 120f;
    public float maxScore = 1000f;

    [Header("Game State")]
    public bool hasKey = false;
    public bool isVaultOpen = false;
    public bool isGameOver = false;
    public bool playerWon = false;
    
    private float currentTime;
    private float playerScore;

    public event Action OnKeyCollected;
    public event Action<bool, string> OnGameOver;
    public event Action<float> OnTimeUpdated;
    
    public static GameManager Instance { get; private set; }

    AudioManager audioManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (!isGameOver)
        {
            UpdateTimer();
            CheckTimeLimit();
        }
    }

    private void InitializeGame()
    {
        currentTime = 0f;
        hasKey = false;
        isVaultOpen = false;
        isGameOver = false;
        playerWon = false;
    }

    private void UpdateTimer()
    {
        currentTime += Time.deltaTime;
        OnTimeUpdated?.Invoke(currentTime);
    }

    private void CheckTimeLimit()
    {
        if (currentTime >= timeLimit && !playerWon)
        {
            GameOver(false);
        }
    }

    public void CollectKey()
    {
        hasKey = true;
        OnKeyCollected?.Invoke();
        audioManager.PlaySFX(audioManager.key);
    }

    public void OpenVault()
    {
        if (hasKey)
        {
            isVaultOpen = true;
            GameOver(true);
        }
    }

    public void GameOver(bool won, string customLossMessage = "")
    {
        if (isGameOver) return;
        
        isGameOver = true;
        playerWon = won;

        if (won)
        {
            float remainingTimePercentage = (timeLimit - currentTime) / timeLimit;
            playerScore = Mathf.Round(maxScore * remainingTimePercentage);
            playerScore = Mathf.Max(playerScore, 100);
            audioManager.PlaySFX(audioManager.victory);
        }
        else
        {
            playerScore = 0;
            audioManager.PlaySFX(audioManager.defeat);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player").transform.parent.gameObject;
        MovementController playerMovementController = player.GetComponent<MovementController>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        playerMovementController.DisableMovement();
        playerRb.simulated = false;

        OnGameOver?.Invoke(won, customLossMessage);
    }

    public float GetScore()
    {
        return playerScore;
    }

    public string GetFormattedTime()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
        return string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
    }

    public float GetRemainingTime()
    {
        return timeLimit - currentTime;
    }
}
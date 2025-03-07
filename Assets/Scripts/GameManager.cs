using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("UI References")]
    public TMP_Text timerText;
    public TMP_Text keyStatusText;
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public TMP_Text scoreText;
    public Button restartButton;

    // Private variables
    private float currentTime;
    private float playerScore;

    // Singleton
    public static GameManager Instance { get; private set; }

    private void Awake() {
        // Singleton Setup
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameOver) {
            UpdateTimer();
            CheckTimeLimit();
        }
    }

    private void InitializeGame() {
        // Init game state
        currentTime = 0f;
        hasKey = false;
        isVaultOpen = false;
        isGameOver = false;
        playerWon = false;

        // Init UI
        if (keyStatusText != null) {
            keyStatusText.text = "Find the key!";
            keyStatusText.color = Color.black;
        }

        if (gameOverPanel != null) {
            gameOverPanel.SetActive(false);
        }

        // Setup restart button
        if (restartButton != null) {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    private void UpdateTimer() {
        currentTime += Time.deltaTime;

        if(timerText != null) {
            // Format as minutes:seconds
            TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
            timerText.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);

            // Change color when time is running out (last 30 seconds)
            if (timeLimit - currentTime <= 30f) {
                timerText.color = Color.red;
            }
        }
    }

    private void CheckTimeLimit() {
        if (currentTime >= timeLimit && !playerWon) {
            GameOver(false);
        }
    }

    public void CollectKey() {
        hasKey = true;

        if (keyStatusText != null) {
            keyStatusText.text = "Key found! Now open the vault!";
            keyStatusText.color = Color.yellow;
        }
    }

    public void OpenVault() {
        if (hasKey) {
            isVaultOpen = true;
            GameOver(true);
        }
    }

    public void GameOver(bool won) {
        isGameOver = true;
        playerWon = won;

        if (won) {
            // Calculate time based on remaining time
            float remainingTimePercentage = (timeLimit - currentTime) / timeLimit;
            playerScore = Mathf.Round(maxScore * remainingTimePercentage);

            // Ensure minimum score if won
            playerScore = Mathf.Max(playerScore, 100);
        } else {
            playerScore = 0;
        }

        ShowGameOverScreen();
    }

    private void ShowGameOverScreen() {
        if (gameOverPanel != null) {
            gameOverPanel.SetActive(true);

            if (gameOverText != null) {
                if (playerWon) {
                    gameOverText.text = "VAULT CRACKED!";
                    gameOverText.color = Color.yellow;
                } else {
                    gameOverText.text = "TIME'S UP!";
                    gameOverText.color = Color.red;
                }
            }
        }

        if (scoreText != null) {
            scoreText.text = playerWon ? $"Score: {playerScore}" : "FAILED";
        }
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        InitializeGame();
    }
}

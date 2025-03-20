using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text timerText;
    public TMP_Text keyStatusText;
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public TMP_Text scoreText;
    public Button restartButton;
    
    [Header("UI Animation")]
    public float pulseDuration = 0.5f;
    public float pulseScale = 1.2f;
    
    private GameManager gameManager;
    
    void Awake()
    {
        // Setup restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
    }
    
    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("UIManager couldn't find GameManager!");
            return;
        }
        
        // Initialize UI state
        UpdateKeyStatus(false);
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Subscribe to GameManager events
        gameManager.OnKeyCollected += OnKeyCollected;
        gameManager.OnGameOver += OnGameOver;
        gameManager.OnTimeUpdated += OnTimeUpdated;
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (gameManager != null)
        {
            gameManager.OnKeyCollected -= OnKeyCollected;
            gameManager.OnGameOver -= OnGameOver;
            gameManager.OnTimeUpdated -= OnTimeUpdated;
        }
        
        // Remove button listeners
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }
    }
    
    private void OnKeyCollected()
    {
        UpdateKeyStatus(true);
        if (keyStatusText != null)
        {
            PulseText(keyStatusText);
        }
    }
    
    private void OnTimeUpdated(float currentTime)
    {
        if (timerText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
            timerText.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
            
            // Change color when time is running out (last 30 seconds)
            if (gameManager.GetRemainingTime() <= 30f)
            {
                timerText.color = Color.red;
            }
        }
    }
    
    private void OnGameOver(bool playerWon, string customLossMessage)
    {
        ShowGameOverScreen(playerWon, customLossMessage);
    }
    
    private void UpdateKeyStatus(bool hasKey)
    {
        if (keyStatusText != null)
        {
            keyStatusText.text = hasKey ? "Key found! Now open the vault!" : "Find the key!";
            keyStatusText.color = hasKey ? Color.yellow : Color.black;
        }
    }
    
    private void ShowGameOverScreen(bool playerWon, string customLossMessage = "")
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverText != null)
            {
                if (playerWon)
                {
                    gameOverText.text = "EXAM STOLEN!";
                    gameOverText.color = Color.yellow;
                }
                else
                {
                    gameOverText.text = string.IsNullOrEmpty(customLossMessage) ? "TIME'S UP!" : customLossMessage;
                    gameOverText.color = Color.red;
                }
            }
            
            if (scoreText != null)
            {
                scoreText.text = playerWon ? $"Score: {gameManager.GetScore()}" : "FAILED";
            }
        }
    }
    
    public void PulseText(TMP_Text textElement)
    {
        if (textElement != null)
        {
            StartCoroutine(PulseCoroutine(textElement.transform));
        }
    }
    
    private System.Collections.IEnumerator PulseCoroutine(Transform target)
    {
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * pulseScale;
        
        // Scale up
        float t = 0f;
        while (t < pulseDuration / 2)
        {
            t += Time.deltaTime;
            float normalizedTime = t / (pulseDuration / 2);
            target.localScale = Vector3.Lerp(originalScale, targetScale, normalizedTime);
            yield return null;
        }
        
        // Scale down
        t = 0f;
        while (t < pulseDuration / 2)
        {
            t += Time.deltaTime;
            float normalizedTime = t / (pulseDuration / 2);
            target.localScale = Vector3.Lerp(targetScale, originalScale, normalizedTime);
            yield return null;
        }
        
        // Ensure scale is reset to original
        target.localScale = originalScale;
    }
    
    private void OnRestartButtonClicked()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }
}
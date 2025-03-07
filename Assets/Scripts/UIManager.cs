using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Ensure GameManager has references to UI elements
        if (GameManager.Instance != null)
        {
            GameManager.Instance.timerText = timerText;
            GameManager.Instance.keyStatusText = keyStatusText;
            GameManager.Instance.gameOverPanel = gameOverPanel;
            GameManager.Instance.gameOverText = gameOverText;
            GameManager.Instance.scoreText = scoreText;
            GameManager.Instance.restartButton = restartButton;
        }
    }
    
    public void PulseText(TMP_Text textElement)
    {
        if (textElement != null)
        {
            // Simple pulsing animation with a coroutine
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
}
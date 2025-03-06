using UnityEngine;

public class Vault : MonoBehaviour
{
    [Header("Settings")]
    public bool requiresKey = true;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    
    [Header("Feedback")]
    public GameObject interactionPrompt;
    public GameObject lockEffect;
    public GameObject unlockEffect;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private bool playerInRange = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null && lockedSprite != null)
        {
            spriteRenderer.sprite = lockedSprite;
        }
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        if (lockEffect != null)
        {
            lockEffect.SetActive(true);
        }
        
        if (unlockEffect != null)
        {
            unlockEffect.SetActive(false);
        }
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenVault();
        }
    }
    
    private void TryOpenVault()
    {
        if (GameManager.Instance != null)
        {
            if (!requiresKey || GameManager.Instance.hasKey)
            {
                OpenVault();
            }
            else
            {
                // Play a "locked" feedback sound/animation
                PlayLockedFeedback();
            }
        }
    }
    
    private void OpenVault()
    {
        // Change sprite
        if (spriteRenderer != null && unlockedSprite != null)
        {
            spriteRenderer.sprite = unlockedSprite;
        }
        
        // Show unlock effect
        if (lockEffect != null)
        {
            lockEffect.SetActive(false);
        }
        
        if (unlockEffect != null)
        {
            unlockEffect.SetActive(true);
        }
        
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OpenVault();
        }
        
        // Disable further interaction
        enabled = false;
    }
    
    private void PlayLockedFeedback()
    {
        // Add here later if possible
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
}
using System.Collections;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer plateRenderer; // Reference to the main sprite renderer
    [SerializeField] private Color defaultColor; // Default color of the plate
    [SerializeField] private Color correctColor = Color.white; // Color when correctly pressed
    [SerializeField] private Color incorrectColor = Color.black; // Color when incorrectly pressed
    [SerializeField] private float feedbackDuration = 0.5f; // How long the feedback color shows
    [SerializeField] private float pressedYOffset = -0.1f; // How far the plate moves down when pressed
    
    private SequencePuzzleManager puzzleManager;
    private SequencePuzzleManager.PressurePlateColor plateColor;
    private bool isPressed = false;
    private Vector3 originalPosition;
    private Coroutine feedbackCoroutine;
    
    private void Awake()
    {
        originalPosition = transform.position;
        
        if (plateRenderer == null)
        {
            plateRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (plateRenderer != null)
        {
            plateRenderer.color = defaultColor;
        }
    }
    
    public void Initialize(SequencePuzzleManager.PressurePlateColor color, SequencePuzzleManager manager)
    {
        plateColor = color;
        puzzleManager = manager;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it's the player triggering the plate
        if (other.CompareTag("Player") && !isPressed)
        {
            isPressed = true;
            // Visually press down the plate
            transform.position = new Vector3(transform.position.x, originalPosition.y + pressedYOffset, transform.position.z);
            // Inform the puzzle manager
            puzzleManager.PlatePressed(plateColor);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // Return the plate to original position when player leaves
        if (other.CompareTag("Player") && isPressed)
        {
            isPressed = false;
            transform.position = originalPosition;
        }
    }
    
    public void ResetPlatePosition()
    {
        isPressed = false;
        transform.position = originalPosition;
    }
    
    public void ShowFeedback(bool isCorrect)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(FeedbackCoroutine(isCorrect));
    }
    
    private IEnumerator FeedbackCoroutine(bool isCorrect)
    {
        if (plateRenderer != null)
        {
            // Change the plate color based on correctness
            plateRenderer.color = isCorrect ? correctColor : incorrectColor;
            
            yield return new WaitForSeconds(feedbackDuration);
            
            // Return to default color
            plateRenderer.color = defaultColor;
        }
    }
}
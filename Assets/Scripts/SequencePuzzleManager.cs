using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SequencePuzzleManager : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [SerializeField] private List<PressurePlateColor> correctSequence = new List<PressurePlateColor>(); // Configure your sequence here
    [SerializeField] private float timeoutDuration = 5f; // Time allowed between correct inputs
    
    [Header("References")]
    [SerializeField] private PressurePlate greenPlate;
    [SerializeField] private PressurePlate yellowPlate;
    [SerializeField] private PressurePlate redPlate;
    [SerializeField] private Slider progressSlider;
    
    [Header("Vending Machine")]
    [SerializeField] private SpriteRenderer vendingMachineRenderer;
    [SerializeField] private Sprite closedVendingMachineSprite;
    [SerializeField] private Sprite openVendingMachineSprite;
    [SerializeField] private GameObject bubbleGumObject;
    
    private List<PressurePlateColor> currentSequence = new List<PressurePlateColor>();
    private Coroutine timeoutCoroutine;
    private bool puzzleSolved = false;
    
    public enum PressurePlateColor
    {
        Green,
        Yellow, 
        Red
    }
    
    private void Start()
    {
        // Initialize pressure plates
        greenPlate.Initialize(PressurePlateColor.Green, this);
        yellowPlate.Initialize(PressurePlateColor.Yellow, this);
        redPlate.Initialize(PressurePlateColor.Red, this);
        
        // Initialize progress slider
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            
            // Set initial color of the fill rect
            UpdateProgressSliderColor(0f);
        }
        
        // Initialize vending machine to closed state
        if (vendingMachineRenderer != null && closedVendingMachineSprite != null)
        {
            vendingMachineRenderer.sprite = closedVendingMachineSprite;
        }
        
        // Make sure bubble gum is initially inactive
        if (bubbleGumObject != null)
        {
            bubbleGumObject.SetActive(false);
        }
    }
    
    public void PlatePressed(PressurePlateColor color)
    {
        // If puzzle is already solved, don't respond to plate presses
        if (puzzleSolved)
            return;
            
        // Check if this is the first plate of a new sequence
        if (currentSequence.Count == 0)
        {
            // Start the timeout timer
            if (timeoutCoroutine != null)
                StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = StartCoroutine(SequenceTimeoutCoroutine());
        }
        
        // Add the pressed color to current sequence
        currentSequence.Add(color);
        
        // Check if the current input is correct
        int currentIndex = currentSequence.Count - 1;
        if (currentIndex < correctSequence.Count && correctSequence[currentIndex] == color)
        {
            // Correct input
            GetPlateByColor(color).ShowFeedback(true);
            
            // Update progress slider
            float progress = (float)currentSequence.Count / correctSequence.Count;
            UpdateProgressSlider(progress);
            
            // Check if the sequence is complete
            if (currentSequence.Count == correctSequence.Count)
            {
                // Success! Open the vending machine
                StopAllCoroutines(); // Stop the timeout
                StartCoroutine(PuzzleCompleteCoroutine());
            }
            else
            {
                // Restart the timeout for the next input
                if (timeoutCoroutine != null)
                    StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = StartCoroutine(SequenceTimeoutCoroutine());
            }
        }
        else
        {
            // Incorrect input
            GetPlateByColor(color).ShowFeedback(false);
            ResetSequence();
        }
    }
    
    private void ResetSequence()
    {
        currentSequence.Clear();
        UpdateProgressSlider(0);
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
    }
    
    private IEnumerator SequenceTimeoutCoroutine()
    {
        yield return new WaitForSeconds(timeoutDuration);
        ResetSequence();
    }
    
    private IEnumerator PuzzleCompleteCoroutine()
    {
        // Mark puzzle as solved to prevent further interactions
        puzzleSolved = true;
        
        // Ensure slider is exactly at 100%
        UpdateProgressSlider(1.0f);
        
        yield return new WaitForSeconds(0.2f); // Short delay before opening
        
        // Open the vending machine by changing its sprite
        if (vendingMachineRenderer != null && openVendingMachineSprite != null)
        {
            vendingMachineRenderer.sprite = openVendingMachineSprite;
        }
        
        yield return new WaitForSeconds(0.1f); // Short delay before revealing the bubble gum
        
        // Activate the bubble gum object
        if (bubbleGumObject != null)
        {
            bubbleGumObject.SetActive(true);
        }
        
        // Reset all pressure plates visually (but sequence stays completed)
        greenPlate.ResetPlatePosition();
        yellowPlate.ResetPlatePosition();
        redPlate.ResetPlatePosition();
    }
    
    private void UpdateProgressSlider(float value)
    {
        if (progressSlider != null)
        {
            progressSlider.value = value;
            UpdateProgressSliderColor(value);
        }
    }
    
    private void UpdateProgressSliderColor(float value)
    {
        // Get the fill rect image from the slider and change its color
        Image fillImage = progressSlider.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(Color.gray, Color.green, value);
        }
    }
    
    private PressurePlate GetPlateByColor(PressurePlateColor color)
    {
        switch(color)
        {
            case PressurePlateColor.Green: return greenPlate;
            case PressurePlateColor.Yellow: return yellowPlate;
            case PressurePlateColor.Red: return redPlate;
            default: return null;
        }
    }
}
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
    [SerializeField] private GameObject bubbleGumObject;
    
    [Header("Bubble Gum Animation")]
    [SerializeField] private float inflationScale = 1.3f; // How much bigger the bubble gum gets
    [SerializeField] private float inflationDuration = 0.5f; // How long it takes to inflate
    [SerializeField] private float deflationDuration = 0.3f; // How long it takes to deflate
    [SerializeField] private int pulseCount = 2; // How many times to pulse

    private List<PressurePlateColor> currentSequence = new List<PressurePlateColor>();
    private Coroutine timeoutCoroutine;
    private bool puzzleSolved = false;
    private Vector3 originalBubbleGumScale;

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
            UpdateProgressSliderColor(0f);
        }

        // Ensure bubble gum is initially inactive
        if (bubbleGumObject != null)
        {
            originalBubbleGumScale = bubbleGumObject.transform.localScale;
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
                // Success! Activate the bubble gum
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

        yield return new WaitForSeconds(0.2f); // Short delay

        // Activate the bubble gum object
        if (bubbleGumObject != null)
        {
            bubbleGumObject.SetActive(true);
            
            // Start the inflation/deflation animation
            StartCoroutine(AnimateBubbleGum());
        }

        // Reset all pressure plates visually (but sequence stays completed)
        greenPlate.ResetPlatePosition();
        yellowPlate.ResetPlatePosition();
        redPlate.ResetPlatePosition();
    }
    
    private IEnumerator AnimateBubbleGum()
    {
        // Make sure the bubble gum starts at its original scale
        bubbleGumObject.transform.localScale = originalBubbleGumScale;
        
        // Calculate the inflated scale
        Vector3 inflatedScale = originalBubbleGumScale * inflationScale;
        
        // Perform the pulsing animation multiple times
        for (int i = 0; i < pulseCount; i++)
        {
            // Inflate the bubble gum
            float inflationTimer = 0f;
            while (inflationTimer < inflationDuration)
            {
                inflationTimer += Time.deltaTime;
                float progress = inflationTimer / inflationDuration;
                bubbleGumObject.transform.localScale = Vector3.Lerp(originalBubbleGumScale, inflatedScale, progress);
                yield return null;
            }
            
            // Ensure it reaches exactly the target scale
            bubbleGumObject.transform.localScale = inflatedScale;
            
            // Deflate the bubble gum
            float deflationTimer = 0f;
            while (deflationTimer < deflationDuration)
            {
                deflationTimer += Time.deltaTime;
                float progress = deflationTimer / deflationDuration;
                bubbleGumObject.transform.localScale = Vector3.Lerp(inflatedScale, originalBubbleGumScale, progress);
                yield return null;
            }
            
            // Ensure it returns exactly to the original scale
            bubbleGumObject.transform.localScale = originalBubbleGumScale;
            
            // Add a small pause between pulses if not the last pulse
            if (i < pulseCount - 1)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
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
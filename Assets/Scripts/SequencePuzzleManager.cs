using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SequencePuzzleManager : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [SerializeField] private List<PressurePlateColor> correctSequence = new List<PressurePlateColor>();
    [SerializeField] private float timeoutDuration = 5f;

    [Header("References")]
    [SerializeField] private PressurePlate greenPlate;
    [SerializeField] private PressurePlate yellowPlate;
    [SerializeField] private PressurePlate redPlate;
    [SerializeField] private GameObject objectToOpen;
    [SerializeField] private Image progressBar;

    private List<PressurePlateColor> currentSequence = new List<PressurePlateColor>();
    public Coroutine timeoutCoroutine;

    public enum PressurePlateColor
    {
        Green,
        Yellow,
        Red
    }

    void Start()
    {
        greenPlate.Initialize(PressurePlateColor.Green,this);
        yellowPlate.Initialize(PressurePlateColor.Yellow, this);
        redPlate.Initialize(PressurePlateColor.Red, this);

        UpdateProgressBar(0);

        if (objectToOpen != null) {
            objectToOpen.SetActive(false);
        }
    }

    public void PlatePressed(PressurePlateColor color) {
        if (currentSequence.Count == 0) {
            if (timeoutCoroutine != null)
                StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = StartCoroutine(SequenceTimeoutCoroutine());
        }

        currentSequence.Add(color);

        int currentIndex = currentSequence.Count - 1;
        if (currentIndex < correctSequence.Count && correctSequence[currentIndex] == color) {
            GetPlateByColor(color).ShowFeedback(true);

            float progress = (float) currentSequence.Count / correctSequence.Count;
            UpdateProgressBar(progress);

            if (currentSequence.Count == correctSequence.Count) {
                StopAllCoroutines();
                StartCoroutine(PuzzleCompleteCoroutine());
            } else {
                if (timeoutCoroutine != null)
                    StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = StartCoroutine(SequenceTimeoutCoroutine());
            }
        } else {
            GetPlateByColor(color).ShowFeedback(false);
            ResetSequence();
        }
    }

    private void ResetSequence() {
        currentSequence.Clear();
        UpdateProgressBar(0);
        if (timeoutCoroutine != null) {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
    }

    private IEnumerator SequenceTimeoutCoroutine() {
        yield return new WaitForSeconds(timeoutDuration);
        ResetSequence();
    }

    private IEnumerator PuzzleCompleteCoroutine() {
        yield return new WaitForSeconds(1f);
        if (objectToOpen != null) {
            objectToOpen.SetActive(true);
        }
    }

    private void UpdateProgressBar(float fillAmount) {
        if (progressBar != null) {
            progressBar.fillAmount = fillAmount;
            progressBar.color = Color.Lerp(Color.gray, Color.green, fillAmount);
        }
    }

    private PressurePlate GetPlateByColor(PressurePlateColor color) {
        switch(color) {
            case PressurePlateColor.Green: return greenPlate;
            case PressurePlateColor.Yellow: return yellowPlate;
            case PressurePlateColor.Red: return redPlate;
            default: return null;
        }
    }
}

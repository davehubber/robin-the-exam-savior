using System.Collections;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private SpriteRenderer outlineRenderer;
    [SerializeField] private float feedbackDuration = 0.5f;
    [SerializeField] private float pressedYOffset = -0.1f;

    private SequencePuzzleManager puzzleManager;
    private SequencePuzzleManager.PressurePlateColor plateColor;
    private bool isPressed = false;
    private Vector3 originalPosition;
    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        originalPosition = transform.position;

        if(outlineRenderer != null) {
            outlineRenderer.enabled = false;
        }
    }

    public void Initialize(SequencePuzzleManager.PressurePlateColor color, SequencePuzzleManager manager) {
        plateColor = color;
        puzzleManager = manager;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player") && !isPressed) {
            isPressed = true;

            transform.position = new Vector3(transform.position.x, originalPosition.y + pressedYOffset, transform.position.z);

            puzzleManager.PlatePressed(plateColor);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player") && isPressed) {
            isPressed = false;
            transform.position = originalPosition;
        }
    }

    public void ShowFeedback(bool isCorrect) {
        if(feedbackCoroutine != null) {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(FeedbackCoroutine(isCorrect));
    }

    private IEnumerator FeedbackCoroutine(bool isCorrect) {
        if (outlineRenderer != null) {
            outlineRenderer.enabled = true;
            outlineRenderer.color = isCorrect ? Color.green : Color.red;

            yield return new WaitForSeconds(feedbackDuration);

            outlineRenderer.enabled = false;
        }
    }
}

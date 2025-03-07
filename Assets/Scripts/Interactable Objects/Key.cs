using UnityEngine;

public class Key : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 50f;
    public float hoverAmplitude = 0.2f;
    public float hoverFrequency = 1f;

    // Visual feedback
    public GameObject visualEffect;

    // Private variables
    private Vector3 startPosition;
    private float timeOffset;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);

        if (visualEffect != null) {
            visualEffect.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update() {
        // Rotate the key
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // Make the key hover
        float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * hoverFrequency) * hoverAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            // Notify GameManager that key was collected
            if (GameManager.Instance != null) {
                GameManager.Instance.CollectKey();

                // Play collection sound or effect

                // Destroy the key
                Destroy(gameObject);
            }
        }
    }
}

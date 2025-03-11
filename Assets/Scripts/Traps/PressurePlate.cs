using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Pressure Plate Settings")]
    // The mass threshold required to activate the plate.
    public float activationMassThreshold = 1f;
    // Reference to the door that this plate unlocks.
    public DoorController doorToUnlock;

    private float currentMass = 0f;
    private bool isActivated = false;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Set initial color (translucent yellow) if not already set in the editor.
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 0f, 0.5f);
        }
    }

    // When an object enters the trigger, add its mass.
    private void OnTriggerEnter2D(Collider2D other)
    {
        print("Hey");
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            currentMass += rb.mass;
            print(currentMass);
            CheckActivation();
        }
    }

    // When an object leaves the trigger, subtract its mass.
    private void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            currentMass -= rb.mass;
        }
    }

    // Check if the total mass meets or exceeds the threshold.
    private void CheckActivation()
    {
        if (!isActivated && currentMass >= activationMassThreshold)
        {
            ActivatePlate();
        }
    }

    private void ActivatePlate()
    {
        isActivated = true;
        // Change the pressure plate color to green to indicate activation.
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green;
        }
        // Call the door's unlock method.
        if (doorToUnlock != null)
        {
            doorToUnlock.UnlockDoor();
        }
        Debug.Log("Pressure plate activated!");
    }
}

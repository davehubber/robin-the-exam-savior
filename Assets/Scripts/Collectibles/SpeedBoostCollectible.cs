using UnityEngine;

public class SpeedBoostCollectible : MonoBehaviour
{
    public float speedMultiplier = 2f;
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {   
            MovementController playerMovement = other.GetComponentInParent<MovementController>();
            if (playerMovement != null)
            {
                playerMovement.ActivateSpeedBoost(speedMultiplier, duration);
            }
            gameObject.SetActive(false);
        }
    }
}
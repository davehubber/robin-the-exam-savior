using UnityEngine;

public class LaserTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.GameOver(false, "You were hit by a laser!");
        }
    }
}

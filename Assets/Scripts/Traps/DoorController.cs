using UnityEngine;

public class DoorController : MonoBehaviour
{
    public void UnlockDoor()
    {
        // Optionally, play an animation or sound before destruction.
        Destroy(gameObject);
    }
}

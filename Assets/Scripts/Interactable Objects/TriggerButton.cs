using UnityEngine;

public class TriggerButton : MonoBehaviour
{
    [SerializeField]
    private GameObject blockade;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (blockade != null)
        {
            Destroy(blockade);
        }
        else
        {
            Debug.LogWarning("Blockade reference is not set on the TriggerButton script.");
        }
    }
}

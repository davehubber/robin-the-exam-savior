using UnityEngine;

public class CircuitPuzzleTrigger : MonoBehaviour
{
    [SerializeField] private CircuitPuzzleManager puzzleManager;
    [SerializeField] private string playerTag = "Player";
    
    private Camera mainCamera;
    private void Start()
    {
        if (puzzleManager == null)
        {
            Debug.LogError("No CircuitPuzzleManager assigned to CircuitPuzzleTrigger!");
        }
        
        mainCamera = Camera.main;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            puzzleManager.SetPuzzleActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            puzzleManager.SetPuzzleActive(false);
        }
    }
    
    private void Update()
    {
        // Handle clicks outside of connections
        if (Input.GetMouseButtonDown(0))
        {
            // Check if we clicked on a connection or outside
            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            
            // If we didn't hit anything or hit something that's not a connection
            if (hit.collider == null || hit.collider.GetComponent<CircuitConnection>() == null)
            {
                puzzleManager.OnClickedOutside();
            }
        }
    }
}
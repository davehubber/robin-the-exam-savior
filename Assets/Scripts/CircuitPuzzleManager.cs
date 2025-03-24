using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for Light2D

public class CircuitPuzzleManager : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private CircuitConnection[] connections;
    [SerializeField] private int[] correctSequence;
    
    [Header("Lines")]
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private Color defaultLineColor = Color.white;
    [SerializeField] private Color completedLineColor = Color.yellow;
    
    [Header("Settings")]
    [SerializeField] private float timeoutDuration = 5f;
    [SerializeField] private Transform objectToRotate;
    [SerializeField] private float rotationAmount = -90f;
    [SerializeField] private float rotationDuration = 1f;
    
    [Header("2D Light Settings")]
    [SerializeField] private Light2D puzzleLight2D; // Assign your inactive Light2D here
    
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private int currentConnectionIndex = 0;
    private bool isPuzzleActive = false;
    private bool isPuzzleCompleted = false;
    private Coroutine timeoutCoroutine;
    
    private void Start()
    {
        if (connections.Length == 0)
        {
            Debug.LogError("No connections assigned to Circuit Puzzle Manager!");
            return;
        }
        
        // Initialize connections
        for (int i = 0; i < connections.Length; i++)
        {
            connections[i].Initialize(this, i);
            connections[i].SetActive(false);
        }
        
        // Create line renderers between sequential connections
        CreateLineRenderers();
        
        // Ensure puzzle is inactive at start
        SetPuzzleActive(false);
    }
    
    private void CreateLineRenderers()
    {
        if (correctSequence.Length < 2)
        {
            Debug.LogWarning("Sequence needs at least two connections to create lines!");
            return;
        }
        
        // Create a line renderer for each segment in the sequence
        for (int i = 0; i < correctSequence.Length - 1; i++)
        {
            if (correctSequence[i] >= connections.Length || correctSequence[i + 1] >= connections.Length)
            {
                Debug.LogError("Sequence index out of range!");
                continue;
            }
            
            LineRenderer lineRenderer = Instantiate(linePrefab, transform);
            lineRenderer.positionCount = 2;
            
            // Set positions
            Vector3 startPos = connections[correctSequence[i]].transform.position;
            Vector3 endPos = connections[correctSequence[i + 1]].transform.position;
            
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            
            // Set default color
            lineRenderer.startColor = defaultLineColor;
            lineRenderer.endColor = defaultLineColor;
            
            lineRenderers.Add(lineRenderer);
        }
    }
    
    public void SetPuzzleActive(bool active)
    {
        isPuzzleActive = active;
        
        if (isPuzzleCompleted)
            return;
            
        // Activate/deactivate connections
        foreach (CircuitConnection connection in connections)
        {
            connection.SetActive(active);
        }
        
        // Show/hide line renderers
        foreach (LineRenderer line in lineRenderers)
        {
            line.gameObject.SetActive(active);
        }
        
        // Show/hide cursor
        Cursor.visible = active;
        
        if (active)
        {
            // Reset the puzzle state
            ResetPuzzle();
        }
        else
        {
            // Cancel timeout if it's running
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }
        }
    }
    
    public void ConnectionClicked(CircuitConnection connection)
    {
        if (!isPuzzleActive || isPuzzleCompleted)
            return;
            
        // Check if the correct connection was clicked
        if (connection.connectionIndex == correctSequence[currentConnectionIndex])
        {
            // Mark this connection as correct
            connection.SetCorrect();
            
            // Update line if this isn't the first connection
            if (currentConnectionIndex > 0)
            {
                // The line index is one less than the current connection index
                int lineIndex = currentConnectionIndex - 1;
                if (lineIndex < lineRenderers.Count)
                {
                    lineRenderers[lineIndex].startColor = completedLineColor;
                    lineRenderers[lineIndex].endColor = completedLineColor;
                }
            }
            
            // Cancel previous timeout and start a new one
            if (timeoutCoroutine != null)
                StopCoroutine(timeoutCoroutine);
                
            timeoutCoroutine = StartCoroutine(TimeoutCoroutine());
            
            // Increment connection index
            currentConnectionIndex++;
            
            // Check if puzzle is completed
            if (currentConnectionIndex >= correctSequence.Length)
            {
                CompletePuzzle();
            }
        }
        else
        {
            // Flash incorrect and reset puzzle
            connection.FlashIncorrect(() => ResetPuzzle());
        }
    }
    
    public void OnClickedOutside()
    {
        if (!isPuzzleActive || isPuzzleCompleted)
            return;
            
        ResetPuzzle();
    }
    
    private void ResetPuzzle()
    {
        currentConnectionIndex = 0;
        
        // Reset all connections
        foreach (CircuitConnection connection in connections)
        {
            connection.ResetColor();
        }
        
        // Reset all lines
        foreach (LineRenderer line in lineRenderers)
        {
            line.startColor = defaultLineColor;
            line.endColor = defaultLineColor;
        }
        
        // Cancel timeout if it's running
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
    }
    
    private void CompletePuzzle()
    {
        isPuzzleCompleted = true;
        
        // Cancel timeout
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
        
        // Rotate the target object
        if (objectToRotate != null)
        {
            LeanTween.rotateZ(objectToRotate.gameObject, objectToRotate.eulerAngles.z + rotationAmount, rotationDuration)
                .setEase(LeanTweenType.easeOutQuad);
        }
        
        // Activate the assigned Light2D if it exists
        if (puzzleLight2D != null)
        {
            puzzleLight2D.gameObject.SetActive(true);
        }
    }
    
    private IEnumerator TimeoutCoroutine()
    {
        yield return new WaitForSeconds(timeoutDuration);
        
        // Timeout occurred, reset the puzzle
        ResetPuzzle();
    }
}

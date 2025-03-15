using UnityEngine;

public class Throwable : MonoBehaviour
{
    private bool isLaunched = false;
    private Vector2 launchVelocity;
    [SerializeField] private float customGravity = 9.81f;

    public void Launch(Vector2 direction, float force)
    {
        isLaunched = true;
        launchVelocity = direction.normalized * force;
        transform.SetParent(null);
    }

    private void Update()
    {
        if (isLaunched)
        {
            transform.position += (Vector3)(launchVelocity * Time.deltaTime);
            launchVelocity += Vector2.down * customGravity * Time.deltaTime;
        }
    }
}

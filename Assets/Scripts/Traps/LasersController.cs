using UnityEngine;

public class LasersController : MonoBehaviour
{
    public GameObject laserTop;
    public GameObject laserBottom;
    
    public float switchInterval = 3.0f; // Time in seconds before switching

    private bool isTopActive = false;
    private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        laserTop = transform.Find("LaserTop").gameObject;
        laserBottom = transform.Find("LaserBottom").gameObject;

        SwitchLaser(isTopActive);
        timer = switchInterval;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            isTopActive = !isTopActive;
            SwitchLaser(isTopActive);
            timer = switchInterval;
        }   
    }

    void SwitchLaser(bool isTopActive)
    {
        laserTop.SetActive(isTopActive);
        laserBottom.SetActive(!isTopActive);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("I'm dead ;_;");
            GameManager.Instance.GameOver(false, "You were hit by a laser!");
        }
    }
}

using UnityEngine;
using System.Collections;

public class SlidingDoor: MonoBehaviour
{
    [SerializeField] private Vector3 movemetoffset = new Vector3(0f, 0f, -5f); // How far the door should move when opening

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Vector3 targetPosition;

    private bool isOpen = false;
    private float currentSpeed = 5f;

    void Start()
    {
        
        closedPosition = transform.position;                // Save the door's starting position
        openPosition = closedPosition + movemetoffset;     // Calculate open position
        targetPosition = closedPosition;                    // Start with the door closed
        StartCoroutine(RandomToggle());                     // Start the loop that opens/closes it randomly
    }

    void Update()
    {
        // Move the door smoothly towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * currentSpeed);
    }
    // Keep switching the door open/closed randomly
    IEnumerator RandomToggle()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 2f)); // Wait between 0.5 and 2 seconds
            currentSpeed = Random.Range(5f, 10f); // Pick a new radom speed each time
            isOpen = !isOpen;
            targetPosition = isOpen ? openPosition : closedPosition;
        }
    }
}

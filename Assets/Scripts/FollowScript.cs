using System.Collections;
using UnityEngine;

public class FollowScript : MonoBehaviour
{
    // Follow Settings
    public Transform player; // The object the npc follows
    public Vector3 offset; // How far to stay behind
    public float followSpeed = 5f; // How fast it follows
    public float lossDistance = 2f; // If it gets this close game over
    public float accelerationFactor = 2f; // Makes it move faster when too far away

    // Visuals and Sound
    public Renderer objectRenderer; // Lets us change the npc eye color
    public Color safeColor = Color.green; // Color when far from player
    public Color dangerColor = Color.red; // Color when close to player
    public AudioSource chaseAudio; // Sound when chasing
    public Camera mainCamera; // Used for shaking the screen

    // Game Over Stuff
    public GameObject gameOverPanel;

    // Delay Before Starting
    public float startDelay = 3f; // Wait time before following starts

    private float delayTimer = 0f;
    private bool hasStartedFollowing = false;
    private bool isShaking = false;
    private bool gameOverTriggered = false;

    private void Start()
    {
        // Set the color to green at the start
        if (objectRenderer != null)
        {
            objectRenderer.material.color = safeColor;
        }

        // Hide the game over screen at the beginning
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Stop if game is over
        if (gameOverTriggered) return;

        // Wait for the delay before starting to follow
        if (!hasStartedFollowing)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= startDelay)
            {
                hasStartedFollowing = true;
            }
            else
            {
                return; // Dont do anything until the delay is done
            }
        }

        // If theres no player to follow it stops
        if (player == null) return;

        // Find out how far it is from the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If its too far move closer
        if (distanceToPlayer > lossDistance)
        {
            Vector3 targetPosition = player.position + offset;
            float dynamicSpeed = followSpeed + (1f / Mathf.Max(0.1f, distanceToPlayer)) * accelerationFactor;
            transform.position = Vector3.Lerp(transform.position, targetPosition, dynamicSpeed * Time.deltaTime);
        }

        // Changes its color depending on how close it is - Green to red
        if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.Lerp(safeColor, dangerColor, Mathf.InverseLerp(5f, lossDistance, distanceToPlayer));
        }

        // Change the sound volume based on distance
        if (chaseAudio != null)
        {
            chaseAudio.volume = Mathf.Lerp(0.1f, 1f, Mathf.InverseLerp(5f, lossDistance, distanceToPlayer));
            if (!chaseAudio.isPlaying) chaseAudio.Play();
        }

        // Shake the camera if close
        if (distanceToPlayer <= lossDistance + 1f && mainCamera != null && !isShaking)
        {
            StartCoroutine(ShakeCamera());
        }

        // If it gets too close end the game
        if (distanceToPlayer <= lossDistance)
        {
            TriggerGameOver();
        }

        // Turns to  alweays face the player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f; // doest tilt up or down
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            lookRotation *= Quaternion.Euler(0f, 90f, 0f); // Help it turn sideways
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    // Makes the camera shake
    private IEnumerator ShakeCamera()
    {
        isShaking = true;
        Vector3 originalPos = mainCamera.transform.localPosition;
        float duration = 0.6f;
        float maxMagnitude = Mathf.Lerp(0.1f, 0.5f, 1f - Mathf.InverseLerp(lossDistance, 0f, Vector3.Distance(transform.position, player.position)));
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float magnitude = maxMagnitude * Mathf.Sin(progress * Mathf.PI);
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
        isShaking = false;
    }

    // Ends the game
    private void TriggerGameOver()
    {
        gameOverTriggered = true;
        Debug.Log("Game Over!");

        if (chaseAudio != null)
            chaseAudio.Stop();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f; // Pause the game
    }
}

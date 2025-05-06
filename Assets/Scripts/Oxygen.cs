using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class Oxygen : MonoBehaviour
{
    // Oxygen Settings
    public int MaximumOxygen = 100; // Max amount of oxygen the player has
    private int oxygen; // Current oxygen

    // UI for oxyge
    public Slider OxygenSlider; // Slider that shows oxygen level
    public TextMeshProUGUI oxygenText; // Text showing current oxygen
    public Image oxygenFillImage; 
    public GameObject GameOverPanel; // Panel displayed when oxygen runs out

    // UI Effects
    public Color refillColor = Color.green; // Color shown when refilling oxygen
    public Color lowOxygenColor = Color.yellow; // Color when oxygen is low

    public AnimationCurve refillFlashCurve; // Curve used for refill flash animation
    public AnimationCurve refillCurve; // Curve for refilling oxygen animation
    public AnimationCurve lossCurve; // Curve for oxygen loss animation

    public GameObject refillEffectPrefab;

    private RectTransform sliderTransform;
    private Vector3 originalPosition; // Original slider position for shake effect
    private Color originalColor; 

    private float lerpTimer;
    private float lerpDuration = 0.5f;
    private float previousOxygen;
    private bool isPulsing = false;

    private Coroutine oxygenDrainCoroutine; // Coroutine for draining oxygen over time
    private bool isDraining = true; // Controls whether oxygen is currently draining
    private bool isInOxygenZone = false; // Whether player is in a zone that stops drain

    void Start()
    {
        // Initialize oxygen
        oxygen = MaximumOxygen;
        OxygenSlider.maxValue = MaximumOxygen;
        OxygenSlider.value = MaximumOxygen;

        // data for transform and original position
        sliderTransform = OxygenSlider.GetComponent<RectTransform>();
        originalPosition = sliderTransform.localPosition;

        if (oxygenFillImage != null)
            originalColor = oxygenFillImage.color;

        if (GameOverPanel != null)
            GameOverPanel.SetActive(false); // Hide Game Over panel at start

        // Starts oxygen draining 
        oxygenDrainCoroutine = StartCoroutine(DrainOxygen());
    }

    void Update()
    {
        AnimateOxygenBar(); // Animate slider smooth effects
        oxygenText.text = $"{oxygen}/{MaximumOxygen}"; // Update text display

        
        if (Input.GetKeyDown(KeyCode.M))
        {
            LoseOxygen(50);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            RefillOxygen(10);
        }
    }

    /// Drains oxygen every second if not in an oxygen zone.
    IEnumerator DrainOxygen()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (isDraining && !isInOxygenZone)
            {
                LoseOxygen(1);
            }
        }
    }

    public void PauseDrain() => isDraining = false; // Temporarily stop draining
    public void ResumeDrain() => isDraining = true; // Resume draining

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OxygenZone"))
        {
            isInOxygenZone = true;
            PauseDrain();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OxygenZone"))
        {
            isInOxygenZone = false;
            ResumeDrain();
        }
    }

    /// Reduces oxygen and handles game over when depleted.
    public void LoseOxygen(int amount)
    {
        oxygen -= amount;
        oxygen = Mathf.Clamp(oxygen, 0, MaximumOxygen);
        OxygenSlider.value = oxygen;
        oxygenText.text = $"{oxygen}/{MaximumOxygen}";
        previousOxygen = oxygen;

        StartCoroutine(ShakeSlider()); // Visual feedback

        if (oxygen == 0)
        {
            if (oxygenDrainCoroutine != null)
                StopCoroutine(oxygenDrainCoroutine);
            ShowGameOver(); // Trigger game over
        }
    }

    /// Refills oxygen by a given amount and plays visual effects.
    public void RefillOxygen(int amount)
    {
        oxygen += amount;
        oxygen = Mathf.Clamp(oxygen, 0, MaximumOxygen);
        StartCoroutine(RefillFlashEffect());
    }

    /// Smoothly animates the slider to the current oxygen level.
    void AnimateOxygenBar()
    {
        if (previousOxygen != oxygen)
        {
            lerpTimer = 0;
            previousOxygen = oxygen;
        }

        if (lerpTimer < lerpDuration)
        {
            lerpTimer += Time.deltaTime;
            float progress = lerpTimer / lerpDuration;
            float curveValue = (oxygen > OxygenSlider.value) ? refillCurve.Evaluate(progress) : lossCurve.Evaluate(progress);
            OxygenSlider.value = Mathf.Lerp(OxygenSlider.value, oxygen, curveValue);
        }

        // Trigger pulse effect when oxygen is low
        if (oxygen <= MaximumOxygen * 0.25f)
        {
            if (!isPulsing)
                StartCoroutine(PulseEffect());
        }
        else
        {
            isPulsing = false;
            oxygenFillImage.color = originalColor;
        }
    }

    /// Shakes the oxygen slider when taking damage.
    IEnumerator ShakeSlider()
    {
        float shakeDuration = 0.3f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float intensity = Mathf.Lerp(1f, 10f, 1 - (oxygen / (float)MaximumOxygen));
            float offsetX = Random.Range(-intensity, intensity);
            float offsetY = Random.Range(-intensity, intensity);
            sliderTransform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);
            yield return null;
        }

        sliderTransform.localPosition = originalPosition;
    }

    /// Flashes the oxygen bar color during refill.
    IEnumerator RefillFlashEffect()
    {
        float duration = refillFlashCurve.keys[refillFlashCurve.length - 1].time;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = refillFlashCurve.Evaluate(elapsed);
            Color blendedColor = Color.white * (1 - alpha) + refillColor * alpha;
            oxygenFillImage.color = blendedColor;
            yield return null;
        }

        oxygenFillImage.color = originalColor;
    }

    /// Pulses the oxygen bar when oxygen is low.
    IEnumerator PulseEffect()
    {
        isPulsing = true;
        float pulseSpeed = 1f;
        float elapsed = 0f;

        while (oxygen <= MaximumOxygen * 0.25f)
        {
            elapsed += Time.deltaTime * pulseSpeed;
            float alpha = Mathf.PingPong(elapsed, 1);
            oxygenFillImage.color = Color.Lerp(originalColor, lowOxygenColor, alpha);
            yield return null;
        }

        oxygenFillImage.color = originalColor;
        isPulsing = false;
    }

    /// Displays the game over panel and pauses the game.
    void ShowGameOver()
    {
        Debug.Log("Game Over triggered");
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameOverPanel is not working");
        }

        // Pause game time
        Time.timeScale = 0f;

        // Unlock and show the mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// Reloads the current scene to restart the game.
    public void RestartGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

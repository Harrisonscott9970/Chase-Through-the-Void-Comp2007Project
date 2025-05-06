using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // UI text displaying the timer
    private float elapsedTime; // Tracks how much time has passed
    private bool isRunning = true; // Controls whether the timer is counting

    void Update()
    {
        if (!isRunning) return; // If timer's stopped, skip the rest

        elapsedTime += Time.deltaTime; // Add time since last frame

        // Convert total time into minutes, seconds, and milliseconds
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 1000) % 1000);

        // The UI text updates with the format
        timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
    // timer stops
    public void StopTimer()
    {
        isRunning = false;
    }
    // timer starts
    public void StartTimer()
    {
        isRunning = true;
        elapsedTime = 0;
    }
    // Returns the time recorded so far (Used for keeping score but never got around to it)
    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}

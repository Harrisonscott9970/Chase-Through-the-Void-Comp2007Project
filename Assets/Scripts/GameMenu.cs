using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject gameOverPanel;
    public GameObject pauseMenu;

    private bool isPaused = false;

    void Update()
    {
        // Can only pause when the player hits escape and isn't in another panel
        if (Input.GetKeyDown(KeyCode.Escape) && !winPanel.activeSelf && !gameOverPanel.activeSelf)
        {
            TogglePause();
        }
    }

    public void ShowWinPanel()
    {
        winPanel.SetActive(true);
        pauseMenu.SetActive(false);
        Time.timeScale = 0f;

        isPaused = false;
        EnableMouse(); // Ensure mouse is visible when win panel is active
    }

    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        EnableMouse(); // Show mouse during game over panel
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;
        LockMouse(); // Lock the mouse when restarting the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload current level
        Debug.Log("Game Reset");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        EnableMouse(); // Show mouse when going to the main menu
        SceneManager.LoadScene("MainMenu"); // Loads the main menu scene
    }

    public void QuitGame()
    {
        Application.Quit(); // Exits the game
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        LockMouse(); // Lock the mouse when resuming the game
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused) // Let player use pause menu
        {
            EnableMouse(); // Show mouse during pause
        }
        else
        {
            LockMouse(); // Lock mouse when unpaused
        }
    }

    private void LockMouse()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void EnableMouse()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject settingMenuPanel;

    [Header("Optional UI Elements")]
    public GameObject healthBar;
    public GameObject manaBar;
    public GameObject ScoreText;
    public GameObject FPSDisplay;

    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        settingMenuPanel.SetActive(false);

        // bật lại map khi resume (nếu có)
        if (healthBar != null)
            healthBar.SetActive(true);
        if (manaBar != null)
            manaBar.SetActive(true);
        if (ScoreText != null)
            ScoreText.SetActive(true);
        if (FPSDisplay != null)
            FPSDisplay.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuPanel.SetActive(true);

        // tắt map khi pause (nếu có)
        if (healthBar != null)
            healthBar.SetActive(false);
        if (manaBar != null)
            manaBar.SetActive(false);
        if (ScoreText != null)
            ScoreText.SetActive(false);
        if (FPSDisplay != null)
            FPSDisplay.SetActive(false);


        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OpenSettings()
    {
        settingMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        settingMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu Scene");
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject settingMenuPanel;

    [Header("Optional UI Elements")]
    public GameObject mapCanvas;
    public GameObject ScoreText1;
    public GameObject ScoreText2;
    public GameObject BoostText1;
    public GameObject BoostText2;
    public GameObject time;
    public GameObject goal;


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
        if (mapCanvas != null)
            mapCanvas.SetActive(true);
        if (ScoreText1 != null)
            ScoreText1.SetActive(true);
        if (ScoreText2 != null)
            ScoreText2.SetActive(true);
        if (BoostText1 != null)
            BoostText1.SetActive(true);
        if (BoostText2 != null)
            BoostText2.SetActive(true);
        if (time != null)
            time.SetActive(true);
        if (goal != null)
            goal.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuPanel.SetActive(true);

        // tắt map khi pause (nếu có)
        if (mapCanvas != null)
            mapCanvas.SetActive(false);
        if (ScoreText1 != null)
            ScoreText1.SetActive(false);
        if (ScoreText2 != null)
            ScoreText2.SetActive(false);
        if (BoostText1 != null)
            BoostText1.SetActive(false);
        if (BoostText2 != null)
            BoostText2.SetActive(false);
        if (time != null)
            time.SetActive(false);
        if (goal != null)
            goal.SetActive(false);


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

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public enum GameState
{
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Settings")]
    public GameObject player;
    public PlayerStats playerStats;
    public Transform currentCheckpoint;

    [Header("UI References")]
    public GameObject pauseMenuUI;
    public GameObject gameOverUI;
    public PlayerTextUI playerTextUI;

    [Header("Audio")]
    public AudioSource bgm;
    public AudioSource deathSFX;

    public GameState currentState = GameState.Playing;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Time.timeScale = 1f;
        if (playerStats == null)
            playerStats = player.GetComponent<PlayerStats>();

        if (playerTextUI != null)
        {
            playerTextUI.UpdateHP(playerStats.currentHP);
            playerTextUI.UpdateMana(playerStats.mana);
        }

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    private void Update()
    {
        if (currentState == GameState.GameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Paused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void OnPlayerDeath()
    {
        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        if (deathSFX != null) deathSFX.Play();
        if (bgm != null) bgm.Stop();
        if (gameOverUI != null) gameOverUI.SetActive(true);
    }

    public void RespawnPlayer()
    {
        if (currentCheckpoint != null)
        {
            player.transform.position = currentCheckpoint.position;
            playerStats.currentHP = 100;
            playerStats.mana = 50;
            playerTextUI.UpdateHP(playerStats.currentHP);
            playerTextUI.UpdateMana(playerStats.mana);
        }

        gameOverUI.SetActive(false);
        Time.timeScale = 1f;
        currentState = GameState.Playing;
        if (bgm != null) bgm.Play();
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
        Debug.Log($"Checkpoint set at: {checkpoint.position}");
    }

    public void PauseGame()
    {
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }

    public void ResumeGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private bool inputEnabled = true;

public bool IsEnableInput()
{
    return inputEnabled;
}

public void SetEnableInput(bool enable)
{
    inputEnabled = enable;
}

}

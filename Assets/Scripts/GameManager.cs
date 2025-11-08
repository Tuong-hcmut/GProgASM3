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
    public TMP_Text finalScoreText;
    public TextMeshProUGUI resultText;

    [Header("Optional UI Elements")]
    public GameObject healthBar;
    public GameObject manaBar;
    public GameObject ScoreText;
    public GameObject FPSDisplay;
    public GameObject pauseMenuUInoworking;

    [Header("Audio")]
    public AudioSource bgm;
    public AudioClip deathSFX;
    public AudioClip victorySFX;
    public AudioSource eventAudio;

    public GameState currentState = GameState.Playing;
    private bool eventAudioPlayed = false;

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
        if (eventAudioPlayed) return; 
        eventAudioPlayed = true;
        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        if (deathSFX != null) AudioSource.PlayClipAtPoint(deathSFX, player.transform.position);
        if (bgm != null && bgm.isPlaying) bgm.Stop();

        if (eventAudio != null && deathSFX != null)
        {
            if (!eventAudio.isPlaying)
            {
                eventAudio.clip = deathSFX;
                eventAudio.Play();
            }
        }
        if (gameOverUI != null) gameOverUI.SetActive(true);
        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {playerStats.score}";
        if (resultText != null)
            resultText.text = "YOU DIED!";
        if (healthBar != null)
            healthBar.SetActive(false);
        if (manaBar != null)
            manaBar.SetActive(false);
        if (ScoreText != null)
            ScoreText.SetActive(false);
        if (FPSDisplay != null)
            FPSDisplay.SetActive(false);
        if (pauseMenuUInoworking != null)
            pauseMenuUInoworking.SetActive(false);
    }
    public void OnPlayerWin()
    {
        if (eventAudioPlayed) return; 
        eventAudioPlayed = true;
        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        if (bgm != null && bgm.isPlaying) bgm.Stop();

        
        if (eventAudio != null && victorySFX != null)
        {
            if (!eventAudio.isPlaying)
            {
                eventAudio.clip = victorySFX;
                eventAudio.Play();
            }
        }
        if (gameOverUI != null) gameOverUI.SetActive(true);

        if (resultText != null)
            resultText.text = "YOU WON!";
        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {playerStats.score}";
        if (healthBar != null)
            healthBar.SetActive(false);
        if (manaBar != null)
            manaBar.SetActive(false);
        if (ScoreText != null)
            ScoreText.SetActive(false);
        if (FPSDisplay != null)
            FPSDisplay.SetActive(false);
        if (pauseMenuUInoworking != null)
            pauseMenuUInoworking.SetActive(false);
    }


    public void RespawnPlayer()
    {
        eventAudioPlayed = false;

        if (currentCheckpoint != null)
        {
            player.transform.position = currentCheckpoint.position;
            playerStats.currentHP = 100;
            playerStats.mana = 50;
            playerTextUI.UpdateHP(playerStats.currentHP);
            playerTextUI.UpdateMana(playerStats.mana);
            Debug.Log($"Player respawned at checkpoint: {currentCheckpoint.position}");
        }
        else
        {
            Debug.LogWarning("No checkpoint set! Respawning at default position.");
            player.transform.position = Vector3.zero;
        }

        gameOverUI.SetActive(false);
        Time.timeScale = 1f;
        currentState = GameState.Playing;
        if (eventAudio != null && eventAudio.isPlaying)
        eventAudio.Stop();

        
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

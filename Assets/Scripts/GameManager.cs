using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public enum GameState
{
    Playing,
    GoalScored,
    Paused,
    GameOver,
    ThrowIn
}

public enum GameMode
{
    SinglePlayer,   // Player 1 vs AI
    TwoPlayerTeam   // Player 1 + Player 2 (team)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState currentState;

    [Header("Game Mode")]
    public GameMode gameMode = GameMode.SinglePlayer;

    [Header("Players")]
    public GameObject player1;
    public GameObject player2;

    private Key tabKey = Key.Tab;
    private Key enterKey = Key.Enter;

    [Header("Ball")]
    public GameObject ball;

    [Header("Indicators")]
    [SerializeField] private DirectionIndicator player1Indicator;
    [SerializeField] private DirectionIndicator player2Indicator;

    [Header("Spawn Positions (enter manually)")]
    public Vector3 player1StartPos;
    public Vector3 player2StartPos;
    public Vector3 ballStartPos;

    [Header("Throw-in Settings")]
    public GameObject throwInPlayer;   // Xe được đá biên
    public GameObject waitingPlayer;   // Xe phải chờ
    public bool ballInPlay = true;     // Cờ check bóng đã đá vào sân chưa

    [Header("Game Settings")]
    public int player1Score = 0;
    public int player2Score = 0;
    public int scoreToWin = 5;
    public float matchTime = 180f; // 3 minutes

    private float timer;
    private HUDManager hud;

    [Header("UI GameOver Popup")]
    public GameObject gameOverPopup;
    public TextMeshPro resultText;
    public TextMeshPro scoreText;
    public GameObject playAgain;
    public GameObject mainMenu;
    [Header("Optional UI Elements")]
    public GameObject mapCanvas;
    public GameObject ScoreText1;
    public GameObject ScoreText2;
    public GameObject BoostText1;
    public GameObject BoostText2;
    public GameObject time;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        hud = FindFirstObjectByType<HUDManager>();

        PlayerController2D p1Ctrl = player1.GetComponent<PlayerController2D>();
        PlayerController2D p2Ctrl = player2.GetComponent<PlayerController2D>();
        AIController p1AI = player1.GetComponent<AIController>();
        AIController p2AI = player2.GetComponent<AIController>();

        // --- Configure by game mode ---
        if (gameMode == GameMode.SinglePlayer)
        {
            // Player 1 controlled by human
            player1.tag = "Player1";
            if (p1Ctrl != null)
            {
                p1Ctrl.enabled = true;
                p1Ctrl.useArrowKeys = false; // WASD
                p1Ctrl.controlledByPlayer = 1;
            }
            if (p1AI != null) p1AI.enabled = false;

            // Player 2 is AI opponent
            player2.tag = "AI2";
            if (p2Ctrl != null)
            {
                p2Ctrl.enabled = false;
                p2Ctrl.controlledByPlayer = 0;
            }
            if (p2AI != null) p2AI.enabled = true;

            // Hide P2 indicator (AI doesn’t need it)
            if (player2Indicator != null)
                player2Indicator.gameObject.SetActive(false);
        }
        else
        {
            // Both players human-controlled
            player1.tag = "Player1";
            if (p1Ctrl != null)
            {
                p1Ctrl.enabled = true;
                p1Ctrl.useArrowKeys = false; // WASD
                p1Ctrl.controlledByPlayer = 1;
            }
            player2.tag = "Player2";
            if (p2Ctrl != null)
            {
                p2Ctrl.enabled = true;
                p2Ctrl.useArrowKeys = true;  // Arrow keys
                p2Ctrl.controlledByPlayer = 2;
            }

            if (p1AI != null) p1AI.enabled = false;
            if (p2AI != null) p2AI.enabled = false;

            // Enable both indicators
            if (player1Indicator != null) player1Indicator.gameObject.SetActive(true);
            if (player2Indicator != null) player2Indicator.gameObject.SetActive(true);
        }

        if (playAgain != null)
            playAgain.GetComponent<Button>().onClick.AddListener(OnPlayAgain);
        if (mainMenu != null)
            mainMenu.GetComponent<Button>().onClick.AddListener(OnMainMenu);

        if (gameOverPopup != null)
            gameOverPopup.SetActive(false);
        // Đặt xe và bóng về vị trí ban đầu khi game bắt đầu
        ResetPositions();
        currentState = GameState.Playing;
        timer = matchTime;

        if (hud != null)
            hud.UpdateHUD();
    }

    private void Update()
    {
        if (currentState != GameState.Playing) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Swap car (if applicable)
        if (keyboard[tabKey].wasPressedThisFrame)
            TrySwapCar(ref player1, false);

        if (gameMode == GameMode.TwoPlayerTeam && keyboard[enterKey].wasPressedThisFrame)
            TrySwapCar(ref player2, true);
        if (currentState == GameState.Playing)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0f;
                SetState(GameState.GameOver);
            }
        }
    }

    private void TrySwapCar(ref GameObject currentPlayerRef, bool isPlayer2)
    {
        if (currentPlayerRef == null)
        {
            Debug.LogWarning("Current player reference is null.");
            return;
        }

        // Determine which AI group to target
        string aiTag = isPlayer2 ? "AI2" : "AI1";
        string playerTag = isPlayer2 ? "Player2" : "Player1";

        GameObject[] aiCars = GameObject.FindGameObjectsWithTag(aiTag);
        if (aiCars.Length == 0)
        {
            Debug.Log($"No {aiTag} cars found to swap with.");
            return;
        }

        // --- Find the AI car closest to the ball ---
        GameObject closestAI = null;
        float closestDistance = float.MaxValue;
        Vector3 ballPos = ball.transform.position;

        foreach (GameObject aiCar in aiCars)
        {
            float dist = Vector3.Distance(aiCar.transform.position, ballPos);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestAI = aiCar;
            }
        }

        if (closestAI == null)
        {
            Debug.LogWarning($"No valid {aiTag} car found near the ball.");
            return;
        }

        GameObject newCar = closestAI;
        GameObject oldCar = currentPlayerRef;

        Debug.Log($"Swapping {playerTag} from {oldCar.name} → {newCar.name} (closest to ball, {closestDistance:F2} units)");

        var newPlayerCtrl = newCar.GetComponent<PlayerController2D>();
        var newAICtrl = newCar.GetComponent<AIController>();
        var oldPlayerCtrl = oldCar.GetComponent<PlayerController2D>();
        var oldAICtrl = oldCar.GetComponent<AIController>();

        if (newPlayerCtrl == null)
        {
            Debug.LogError($"Car {newCar.name} has no PlayerController2D.");
            return;
        }

        // --- old car → AI role ---
        oldCar.tag = aiTag;
        if (oldPlayerCtrl != null)
        {
            oldPlayerCtrl.enabled = false;
            oldPlayerCtrl.controlledByPlayer = 0;
        }
        if (oldAICtrl != null) oldAICtrl.enabled = true;

        // --- new car → player role ---
        newCar.tag = playerTag;
        newPlayerCtrl.enabled = true;
        newPlayerCtrl.useArrowKeys = isPlayer2;
        newPlayerCtrl.controlledByPlayer = isPlayer2 ? 2 : 1;
        if (newAICtrl != null) newAICtrl.enabled = false;

        // --- Update reference in GameManager ---
        currentPlayerRef = newCar;
        if (isPlayer2) player2 = newCar;
        else player1 = newCar;

        // --- Update indicator target ---
        DirectionIndicator indicator = isPlayer2 ? player2Indicator : player1Indicator;
        if (indicator != null)
        {
            indicator.SetTarget(newCar.transform);
            indicator.gameObject.SetActive(true);
            Debug.Log($"Indicator for Player {(isPlayer2 ? 2 : 1)} updated to {newCar.name}");
        }

        // --- Trigger HUD refresh ---
        if (hud != null)
            hud.UpdateHUD();

        // Extra feedback
        Debug.Log($"Player {(isPlayer2 ? 2 : 1)} now controls {newCar.name}");
    }

    public void AddScore(int playerNumber)
    {
        if (playerNumber == 1) player1Score++;
        else if (playerNumber == 2) player2Score++;

        if (hud != null) hud.UpdateHUD();

        if (player1Score >= scoreToWin || player2Score >= scoreToWin)
            SetState(GameState.GameOver);
        else
            SetState(GameState.GoalScored);
    }

    public float GetTimeRemaining()
    {
        return timer;
    }

    public void SetState(GameState newState)
    {
        currentState = newState;

        PlayerController2D p1Ctrl = player1.GetComponent<PlayerController2D>();
        PlayerController2D p2Ctrl = player2.GetComponent<PlayerController2D>();
        AIController p1AI = player1.GetComponent<AIController>();
        AIController p2AI = player2.GetComponent<AIController>();

        switch (currentState)
        {
            case GameState.Playing:
                if (gameMode == GameMode.SinglePlayer)
                {
                    if (p1Ctrl != null) p1Ctrl.enabled = true;
                    if (p2AI != null) p2AI.enabled = true;
                }
                else
                {
                    if (p1Ctrl != null) p1Ctrl.enabled = true;
                    if (p2Ctrl != null) p2Ctrl.enabled = true;
                }
                break;

            case GameState.GoalScored:
                ResetPositions();
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Debug.Log("Game Over!");
                ShowGameOverPopup();
                break;
        }
    }

    private void ShowGameOverPopup()
    {
        if (gameOverPopup == null) return;
        Time.timeScale = 0f;
        gameOverPopup.SetActive(true);
        if (scoreText != null)
            scoreText.text = player1Score + " - " + player2Score;
        if (resultText != null)
        {
            if (player1Score > player2Score)
                resultText.text = "Player 1 won!";
            else if (player2Score > player1Score)
            {
                if (player2.CompareTag("AI"))
                {
                    resultText.text = "AI won!";
                }
                else if (player2.CompareTag("Player2"))
                {
                    resultText.text = "Player2 won!";
                }
            }
        }
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

    }
    public void OnPlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu Scene");
    }
    public void ResetPositions()
    {
        player1.transform.position = player1StartPos;
        player1.transform.rotation = Quaternion.identity;
        ResetRigidbody(player1);

        player2.transform.position = player2StartPos;
        player2.transform.rotation = Quaternion.identity;
        ResetRigidbody(player2);

        ball.transform.position = ballStartPos;
        ball.transform.rotation = Quaternion.identity;
        ResetRigidbody(ball);

        currentState = GameState.Playing;
    }

    private void ResetRigidbody(GameObject obj)
    {
        Rigidbody2D rb2d = obj.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            return;
        }
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
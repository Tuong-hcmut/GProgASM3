using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TMP_Text player1ScoreText;        // user-defined
    [SerializeField] private TMP_Text player2ScoreText;        // user-defined

    [Header("Timer UI")]
    [SerializeField] private TMP_Text timerText;               // user-defined

    [Header("Boost UI")]
    [SerializeField] private TMP_Text player1BoostText;        // user-defined
    [SerializeField] private TMP_Text player2BoostText;        // user-defined

    private void Update()
    {
        // Only the timer updates continuously
        if (GameManager.Instance == null) return;

        // float timeLeft = GameManager.Instance.GetTimeRemaining();
        // int minutes = Mathf.FloorToInt(timeLeft / 60f);
        // int seconds = Mathf.FloorToInt(timeLeft % 60f);
        // timerText.text = $"{minutes:00}:{seconds:00}";
    }

    // Called manually by GameManager, PlayerController2D, etc.
    public void UpdateHUD()
    {
        if (GameManager.Instance == null) return;

        // --- Scores ---
        // if (player1ScoreText != null)
        //     player1ScoreText.text = GameManager.Instance.player1Score.ToString();
        // if (player2ScoreText != null)
        //     player2ScoreText.text = GameManager.Instance.player2Score.ToString();

        // --- Boost availability ---
        // UpdateBoostStatus(1, GameManager.Instance.player1);
        // UpdateBoostStatus(2, GameManager.Instance.player2);
    }

    private void UpdateBoostStatus(int playerIndex, GameObject playerObj)
    {
        if (playerObj == null) return;

        var controller = playerObj.GetComponent<PlayerController2D>();
        if (controller == null) return;

        TMP_Text targetText = (playerIndex == 1) ? player1BoostText : player2BoostText;

        if (targetText != null)
        {
            if (controller.canBoost)
                targetText.text = "Boost: READY";
            else
                targetText.text = "Boost: EMPTY";
        }
    }
}

using TMPro;
using UnityEngine;

public class PlayerTextUI : MonoBehaviour
{
    [Header("UI Text Components")]
    public TMP_Text hpText;
    public TMP_Text manaText;
    public TMP_Text scoreText;
    public void UpdateHP(int hp)
    {
        if (hpText != null)
            hpText.text = $"HP: {hp}";
    }

    public void UpdateMana(int mana)
    {
        if (manaText != null)
            manaText.text = $"Mana: {mana}";
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
}

using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int currentHP = 100;
    public int mana = 50;
    public int score = 0;

    public PlayerTextUI textUI;

    void Start()
    {
        if (textUI != null)
        {
            textUI.UpdateHP(currentHP);
            textUI.UpdateMana(mana);
            textUI.UpdateScore(score);
        }
    }

    void Update()
    {
        if (textUI != null)
        {
            textUI.UpdateHP(currentHP);
            textUI.UpdateMana(mana);
            textUI.UpdateScore(score);
        }
    }

    // Gọi hàm này mỗi khi người chơi tăng điểm
    public void AddScore(int amount)
    {
        score += amount;
        if (textUI != null)
            textUI.UpdateScore(score);
    }
}

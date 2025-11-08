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

        // Nếu máu <= 0, gọi GameOver
        if (transform.position.y < -30f) 
        {
            currentHP = 0;
            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerDeath();
        }

        if (currentHP <= 0)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerDeath();
        }
    }

    // Gọi khi tăng điểm
    public void AddScore(int amount)
    {
        score += amount;
        if (textUI != null)
            textUI.UpdateScore(score);
    }

    // Gọi khi nhận damage
    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

        if (textUI != null)
            textUI.UpdateHP(currentHP);

        // Nếu chết
        if (currentHP <= 0 && GameManager.Instance != null)
            GameManager.Instance.OnPlayerDeath();
    }
}

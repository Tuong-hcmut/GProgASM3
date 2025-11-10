using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int currentHP;
    public int mana = 50;
    public int score = 0;
    protected PlayerAttack character;
    public PlayerTextUI textUI;

    void Start()
    {
        character = FindFirstObjectByType<PlayerAttack>();
        if (textUI != null)
        {
            textUI.UpdateHP(currentHP);
            textUI.UpdateMana(mana);
            textUI.UpdateScore(score);
        }
    }

    void Update()
    {
        currentHP = character.GetCurrentHealth();
        if (textUI != null)
        {
            textUI.UpdateHP(currentHP);
            textUI.UpdateMana(mana);
            textUI.UpdateScore(score);
        }

        // Nếu máu <= 0, gọi GameOver
        if (transform.position.y < -30f)
        {
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
        character.Hurt(amount);
    }
}

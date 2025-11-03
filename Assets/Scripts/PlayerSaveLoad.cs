using UnityEngine;

public class PlayerSaveLoad : MonoBehaviour
{
    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        if (stats == null)
            Debug.LogWarning("PlayerStats not found on Player!");
    }

    public void ApplySave(SaveData sd)
    {
        if (sd == null || stats == null) return;

        // Di chuyển player về vị trí lưu
        transform.position = sd.GetPosition();

        // Áp dụng chỉ số
        stats.currentHP = sd.playerHP;
        stats.mana = sd.playerMana;
        stats.score = sd.playerScore;

        // Cập nhật UI
        if (stats.textUI != null)
        {
            stats.textUI.UpdateHP(stats.currentHP);
            stats.textUI.UpdateMana(stats.mana);
            stats.textUI.UpdateScore(stats.score);
        }

        Debug.Log($"Loaded Save → Pos: {sd.GetPosition()} | HP: {stats.currentHP} | Mana: {stats.mana} | Score: {stats.score}");
    }
}

using UnityEngine;

public class PlayerSaveLoad : MonoBehaviour
{
    public void ApplyLoadedSave(SaveData sd)
    {
        if (sd == null) return;

        // 1. Tìm player
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("ApplyLoadedSave: Không tìm thấy Player (tag 'Player')!");
            return;
        }

        // 2. LUÔN LUÔN set vị trí
        // Lưu ý: Nếu player dùng CharacterController, bạn có thể cần tắt nó đi
        // rồi bật lại sau khi set position, nhưng cứ thử cách này trước.
        player.transform.position = sd.GetPosition();
        Debug.Log($"[Load] Đã set vị trí Player tới {sd.GetPosition()}");

        // 3. LUÔN LUÔN tìm PlayerStats và áp dụng chỉ số
        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.currentHP = sd.playerHP;
            stats.mana = sd.playerMana;
            stats.score = sd.playerScore;
            Debug.Log($"[Load] Đã áp dụng chỉ số: HP={sd.playerHP}, Mana={sd.playerMana}, Score={sd.playerScore}");
        }
        else
        {
            Debug.LogWarning("ApplyLoadedSave: Không tìm thấy PlayerStats trên Player!");
        }
    }
}

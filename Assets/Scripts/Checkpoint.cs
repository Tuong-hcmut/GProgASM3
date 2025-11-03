using UnityEngine;

public class Checkpoint : MonoBehaviour
{   
    // Thêm dòng này
    public SaveNotificationUI saveNotificationUI;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var stats = collision.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // Lưu game
                SaveManager.Instance.AutoSave(
                    transform.position,
                    stats.currentHP,
                    stats.mana,
                    stats.score
                );

                Debug.Log("Checkpoint AutoSaved!");

                // Hiển thị thông báo "Game Saved"
                if (saveNotificationUI != null)
                    saveNotificationUI.ShowNotification("Game Saved");
                else
                    Debug.LogWarning("SaveNotificationUI chưa được gán trong Checkpoint!");
            }
            else
            {
                Debug.LogWarning("PlayerStats not found on Player!");
            }
        }
    }
}

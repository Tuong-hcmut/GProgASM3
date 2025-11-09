using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Options")]
    public bool isActivated = false;
    public SaveNotificationUI saveNotificationUI;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        var stats = collision.GetComponent<PlayerStats>();
        if (stats == null)
        {
            Debug.LogWarning("Checkpoint: PlayerStats not found on Player!");
            return;
        }

        
        if (!isActivated)
        {
            isActivated = true;

           
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCheckpoint(transform);
                Debug.Log($"Checkpoint activated at position: {transform.position}");
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is NULL!");
            }

            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.AutoSave(
                    collision.transform.position,
                    stats.currentHP,
                    stats.mana,
                    stats.score
                );
                Debug.Log("Checkpoint AutoSaved!");
            }
            else
            {
                Debug.LogWarning("SaveManager.Instance is NULL!");
            }

            
            if (saveNotificationUI != null)
                saveNotificationUI.ShowNotification("Game Saved");
        }
    }
}
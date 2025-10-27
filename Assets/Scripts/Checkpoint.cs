using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Lấy component PlayerSaveLoad từ đối tượng Player
            PlayerSaveLoad playerSaveLoad = collision.GetComponent<PlayerSaveLoad>();

            if (playerSaveLoad != null)
            {
                // Gọi AutoSave bằng biến instance playerSaveLoad
                SaveManager.Instance.AutoSave(
                    transform.position,
                    playerSaveLoad.currentHP,   
                    playerSaveLoad.mana          
                );

                Debug.Log("Checkpoint saved!");
            }
            else
            {
                Debug.LogWarning("PlayerSaveLoad not found on Player object!");
            }
        }
    }
}

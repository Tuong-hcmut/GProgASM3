using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerStats == null) return;

        if (collision.CompareTag("Trap"))
        {
            playerStats.TakeDamage(10);
            Debug.Log("Player hit a trap! -10 HP");
        }
        else if (collision.CompareTag("Water"))
        {
            playerStats.TakeDamage(15);
            Debug.Log("Player fell into water! -15 HP");
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class VisionCone : MonoBehaviour
{
    private Enemy enemyOwner;
    private BigBoss bossOwner;

    private void Awake()
    {
        
        enemyOwner = GetComponentInParent<Enemy>();
        bossOwner = GetComponentInParent<BigBoss>();
    }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        enemyOwner?.OnVisionEnter(other);
        bossOwner?.OnVisionEnter(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        enemyOwner?.OnVisionExit(other);
        bossOwner?.OnVisionExit(other);
    }
}
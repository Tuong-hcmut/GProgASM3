using System.Numerics;
using TMPro;
using UnityEngine;

public class DirectionIndicator : MonoBehaviour
{
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private Transform target;
    [SerializeField] private float distanceMultiplier;

    void Update()
    {
        LookAtTarget();
        ShowDistance();
    }

    private void LookAtTarget()
    {
        if (target == null) return;

        // Direction from this object to target (2D)
        UnityEngine.Vector2 direction = target.position - transform.position;

        // Compute angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply rotation (around Z axis only)
        transform.rotation = UnityEngine.Quaternion.Euler(0f, 0f, angle - 90f);
    }
    private void ShowDistance()
    {
        // distance in 2D (XY plane)
        float distance = UnityEngine.Vector2.Distance(transform.position, target.position) * distanceMultiplier;

        distanceText.SetText(Mathf.RoundToInt(distance) + "m");

        distanceText.transform.rotation = UnityEngine.Quaternion.Euler(0f, 0f, 0f);
    }
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

}

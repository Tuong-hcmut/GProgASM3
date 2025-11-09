using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class VisionCone : MonoBehaviour
{
    [HideInInspector] public Enemy owner;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        owner?.OnVisionEnter(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        owner?.OnVisionExit(other);
    }
}
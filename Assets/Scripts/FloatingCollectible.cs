using UnityEngine;

public class FloatingCollectible : MonoBehaviour
{
    public float floatAmplitude = 0.25f; // Độ cao dao động
    public float floatFrequency = 5f;    // Tốc độ dao động

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
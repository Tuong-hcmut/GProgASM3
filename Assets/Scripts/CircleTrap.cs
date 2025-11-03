using UnityEngine;
using UnityEngine.SceneManagement;
public class CircleTrap : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public float moveSpeed = 5f;
    public Transform pointA;
    public Transform pointB;
    private Vector3 targetPosition;
    void Start()
    {
        targetPosition = pointA.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (targetPosition == pointA.position)
            {
                targetPosition = pointB.position;
            }
            else
            {
                targetPosition = pointA.position;
            }
        }
    }
    void FixedUpdate()
    {
        transform.Rotate(0, 0, rotationSpeed);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene("environment");
        }
    }
}

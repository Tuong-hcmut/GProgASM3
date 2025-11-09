using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 1f;
    public int damage = 10;
    private Vector2 direction;

    public void Setup(Vector2 dir)
    {
        direction = dir;
        // Xoay cầu lửa theo hướng bay (tùy chọn)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        // Di chuyển
        transform.Translate(Vector2.right * speed * Time.deltaTime * transform.lossyScale.x);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu trúng Player
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerStats>().TakeDamage(damage);
            Destroy(gameObject); // Biến mất
        }
        // Nếu trúng tường (giả sử tường có tag "Ground")
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject); // Biến mất
        }
    }
}
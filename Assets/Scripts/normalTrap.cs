using UnityEngine;
using UnityEngine.SceneManagement;
public class normalTrap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    void ontriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Add your trap logic here, e.g., reduce player health or restart level
            SceneManager.LoadScene("environment");
        }
    }
}

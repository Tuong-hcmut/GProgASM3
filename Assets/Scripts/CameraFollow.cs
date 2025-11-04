using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Character 1").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            Vector3 pos = transform.position;
            pos.x = player.position.x;
            transform.position = pos;
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("References")]
    public BoxCollider2D spawnArea;         // khu vực cho spawn (world coords)
    public GameObject collectiblePrefab;    // prefab item
    public LayerMask obstacleMask;          // layers to avoid (walls, players, other collectibles)

    [Header("Spawn settings")]
    public int targetCount = 8;             // số item muốn có trên map
    public float itemRadius = 0.5f;         // bán kính "an toàn" xung quanh item (world units)
    public float paddingFromEdges = 0.1f;   // khoảng cách tối thiểu đến biên sân
    public int maxAttemptsPerItem = 40;     // số lần retry trước khi bỏ

    [Header("Optional")]
    public bool spawnOnStart = true;
    public bool usePooling = false;         // không bắt buộc; nếu true bạn cần implement pool

    private List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        if (spawnOnStart) SpawnInitial();
    }

    public void SpawnInitial()
    {
        // spawn khi bắt đầu: cố gắng đạt targetCount
        for (int i = 0; i < targetCount; i++)
        {
            TrySpawnOne();
        }
    }

    // Thử spawn 1 item hợp lệ
    public bool TrySpawnOne()
    {
        if (spawnArea == null || collectiblePrefab == null)
        {
            Debug.LogWarning("Spawn area or prefab not assigned.");
            return false;
        }

        Bounds bounds = spawnArea.bounds;

        // shrink bounds bằng padding + itemRadius để không spawn quá sát tường
        float margin = paddingFromEdges + itemRadius;
        float minX = bounds.min.x + margin;
        float maxX = bounds.max.x - margin;
        float minY = bounds.min.y + margin;
        float maxY = bounds.max.y - margin;

        if (minX > maxX || minY > maxY)
        {
            Debug.LogError("SpawnArea too small for given itemRadius/padding.");
            return false;
        }

        for (int attempt = 0; attempt < maxAttemptsPerItem; attempt++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            // ensure inside collider shape (useful if spawnArea is not perfect rect)
            if (!spawnArea.OverlapPoint(candidate)) 
                continue;

            // check overlap with obstacles (players, walls, other items, etc.)
            Collider2D hit = Physics2D.OverlapCircle(candidate, itemRadius, obstacleMask);
            
            if (hit != null && hit.gameObject != spawnArea.gameObject)
            {
                Debug.Log("Blocked by: " + hit.name);
                continue;
            }


            // ok: spawn here
            GameObject go = Instantiate(collectiblePrefab, candidate, Quaternion.identity);
            spawned.Add(go);
            return true;
        }

        Debug.LogWarning($"Failed to spawn item after {maxAttemptsPerItem} attempts.");
        return false;
    }

    // gọi khi 1 item bị ăn/quăng đi để giữ số lượng ổn định
    public void NotifyItemRemoved(GameObject item)
    {
        spawned.Remove(item);
        // respawn ngay hoặc theo delay:
        TrySpawnOne();
    }

    // Optional: remove all and respawn (useful for level reset)
    public void ClearAndRespawn()
    {
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
        SpawnInitial();
    }

    // Debug: vẽ bounds
    void OnDrawGizmosSelected()
    {
        if (spawnArea != null)
        {
            Gizmos.color = Color.cyan;
            var b = spawnArea.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
            // inner margin
            float margin = paddingFromEdges + itemRadius;
            Vector3 innerSize = new Vector3(b.size.x - 2*margin, b.size.y - 2*margin, b.size.z);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(b.center, innerSize);
        }
    }
}

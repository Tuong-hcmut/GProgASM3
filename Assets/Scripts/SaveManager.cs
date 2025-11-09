using System.IO;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public int slotCount = 3;
    private string[] filePaths;
    private SaveData[] cachedSaves;

    public SaveData pendingLoadedSave;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        filePaths = new string[slotCount];
        cachedSaves = new SaveData[slotCount];
        for (int i = 0; i < slotCount; i++)
            filePaths[i] = Path.Combine(Application.persistentDataPath, $"save_slot_{i + 1}.json");

        LoadAllFromDisk();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void AutoSaveToFile(Vector3 pos, int hp, int mana, int score)
    {
    SaveData data = new SaveData(pos, SceneManager.GetActiveScene().name, hp, mana, score);
    string path = Application.persistentDataPath + "/save1.json";
    File.WriteAllText(path, JsonUtility.ToJson(data));
    Debug.Log($"[SAVE] Game saved at {path}");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- Load all saves from disk ---
    public void LoadAllFromDisk()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (File.Exists(filePaths[i]))
            {
                try
                {
                    string json = File.ReadAllText(filePaths[i]);
                    cachedSaves[i] = JsonUtility.FromJson<SaveData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to read save file {filePaths[i]}: {e.Message}");
                    cachedSaves[i] = null;
                }
            }
            else cachedSaves[i] = null;
        }
    }

    public SaveData[] GetCachedSaves() => cachedSaves;

    // --- AutoSave when checkpoint triggered ---
    public int AutoSave(Vector3 pos, int hp = 0, int mana = 0, int score = 0)
    {
        int chosen = -1;

        for (int i = 0; i < slotCount; i++)
            if (cachedSaves[i] == null) { chosen = i; break; }

        if (chosen == -1)
        {
            long minTicks = long.MaxValue;
            for (int i = 0; i < slotCount; i++)
            {
                if (cachedSaves[i] != null && cachedSaves[i].timestampTicks < minTicks)
                {
                    minTicks = cachedSaves[i].timestampTicks;
                    chosen = i;
                }
            }
            if (chosen == -1) chosen = 0;
        }

        SaveData sd = new SaveData(pos, SceneManager.GetActiveScene().name, hp, mana, score);
        cachedSaves[chosen] = sd;
        SaveToDisk(chosen);
        Debug.Log($"[AutoSave] Slot {chosen + 1}: {sd.sceneName} @ {sd.GetTimestampString()}");
        return chosen;
    }

    public void SaveToSlot(int slotIndex, Vector3 pos, int hp = 0, int mana = 0, int score = 0)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return;
        SaveData sd = new SaveData(pos, SceneManager.GetActiveScene().name, hp, mana, score);
        cachedSaves[slotIndex] = sd;
        SaveToDisk(slotIndex);
    }

    private void SaveToDisk(int index)
    {
        if (index < 0 || index >= slotCount) return;
        try
        {
            string json = JsonUtility.ToJson(cachedSaves[index], true);
            File.WriteAllText(filePaths[index], json);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveToDisk failed: {e.Message}");
        }
    }

    // --- Load slot from main menu ---
    public void LoadSlotAndStart(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return;
        if (cachedSaves[slotIndex] == null)
        {
            Debug.Log("No save in that slot.");
            return;
        }
        SaveData data = cachedSaves[slotIndex];

        if (SceneManager.GetActiveScene().name == data.sceneName)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var psl = player.GetComponent<PlayerSaveLoad>();
                if (psl != null)
                    psl.ApplyLoadedSave(data);
                else
                    player.transform.position = data.GetPosition();

                Debug.Log($"[Load] Restored player to position {data.GetPosition()} in {data.sceneName}");
            }
            else
            {
                Debug.LogWarning("[Load] Player not found in current scene!");
            }
        }
        else
        {
            // Nếu khác scene, thì mới load scene
            pendingLoadedSave = data;
            SceneManager.LoadScene(data.sceneName);
        }
    }

    // --- Apply save to player after scene loads ---
    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     if (pendingLoadedSave != null)
    //     {
    //         var handler = FindFirstObjectByType<SceneLoadHandler>();
    //         if (handler != null)
    //         {
    //             handler.ApplyLoadedSave(pendingLoadedSave);
    //             pendingLoadedSave = null;
    //             return;
    //         }

    //         var player = GameObject.FindWithTag("Player");
    //         if (player != null)
    //         {
    //             var playerSaveLoad = player.GetComponent<PlayerSaveLoad>();
    //             if (playerSaveLoad != null)
    //                 playerSaveLoad.ApplyLoadedSave(pendingLoadedSave);
    //         }
    //         pendingLoadedSave = null;
    //     }
    // }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Chỉ chạy nếu có save đang chờ load
        if (pendingLoadedSave != null)
        {
            // Bắt đầu một coroutine để đợi 1 frame
            // Điều này đảm bảo tất cả hàm Start() trong scene đã chạy xong
            StartCoroutine(ApplySaveAfterSceneLoad());
        }
    }

    private System.Collections.IEnumerator ApplySaveAfterSceneLoad()
    {
        // Đợi đến cuối frame, sau khi tất cả Update() và Start() đã chạy
        yield return new WaitForEndOfFrame();
        
        // Kiểm tra lại phòng khi có lỗi
        if (pendingLoadedSave == null) yield break; 

        // 1. Tìm SceneLoadHandler (cách ưu tiên)
        var handler = FindFirstObjectByType<SceneLoadHandler>();
        if (handler != null)
        {
            Debug.Log("ApplySaveAfterSceneLoad: Tìm thấy SceneLoadHandler. Đang áp dụng save...");
            handler.ApplyLoadedSave(pendingLoadedSave);
        }
        else
        {
            // 2. Cách dự phòng (nếu quên thêm SceneLoadHandler vào scene)
            Debug.LogWarning("ApplySaveAfterSceneLoad: Không tìm thấy SceneLoadHandler! SaveManager sẽ tự áp dụng.");
            
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // Áp dụng vị trí
                player.transform.position = pendingLoadedSave.GetPosition();

                // Áp dụng chỉ số
                var stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.currentHP = pendingLoadedSave.playerHP;
                    stats.mana = pendingLoadedSave.playerMana;
                    stats.score = pendingLoadedSave.playerScore;
                }
                Debug.Log($"[Load] Đã áp dụng save dự phòng cho Player.");
            }
            else
            {
                Debug.LogError("ApplySaveAfterSceneLoad: Không tìm thấy Player (tag 'Player')!");
            }
        }

        // Xóa save đang chờ sau khi đã áp dụng xong
        pendingLoadedSave = null;
    }
    public void DeleteSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return;
        cachedSaves[slotIndex] = null;
        try { if (File.Exists(filePaths[slotIndex])) File.Delete(filePaths[slotIndex]); } catch { }
    }
}

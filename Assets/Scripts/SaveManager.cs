// SaveManager.cs
using System.IO;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public int slotCount = 3;
    private string[] filePaths;
    private SaveData[] cachedSaves; // loaded from disk

    // When player clicks Load, store the loaded save here so SceneLoadHandler can apply
    public SaveData pendingLoadedSave;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        filePaths = new string[slotCount];
        cachedSaves = new SaveData[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            filePaths[i] = Path.Combine(Application.persistentDataPath, $"save_slot_{i+1}.json");
        }
        LoadAllFromDisk();
        SceneManager.sceneLoaded += OnSceneLoaded; // for safety
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Load all saves from disk into cachedSaves
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

    // Return copy of cached saves for UI
    public SaveData[] GetCachedSaves()
    {
        return cachedSaves;
    }

    // AutoSave at checkpoint: choose slot = first empty, else oldest (min timestamp)
    public int AutoSave(UnityEngine.Vector3 pos, int hp = 0, int mana = 0)
    {
        // Choose slot index
        int chosen = -1;
        for (int i = 0; i < slotCount; i++)
            if (cachedSaves[i] == null) { chosen = i; break; }

        if (chosen == -1)
        {
            // no empty slot => pick oldest
            long minTicks = long.MaxValue;
            for (int i = 0; i < slotCount; i++)
            {
                if (cachedSaves[i] != null && cachedSaves[i].timestampTicks < minTicks)
                {
                    minTicks = cachedSaves[i].timestampTicks;
                    chosen = i;
                }
            }
            // fallback
            if (chosen == -1) chosen = 0;
        }

        SaveData sd = new SaveData(pos, SceneManager.GetActiveScene().name, hp, mana);
        cachedSaves[chosen] = sd;
        SaveToDisk(chosen);
        Debug.Log($"Autosaved to slot {chosen+1} at {sd.GetTimestampString()}");
        return chosen;
    }

    // Explicit save to a specific slot (e.g., user-invoked)
    public void SaveToSlot(int slotIndex, UnityEngine.Vector3 pos, int hp = 0, int mana = 0)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return;
        SaveData sd = new SaveData(pos, SceneManager.GetActiveScene().name, hp, mana);
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

    // Called by MainMenu when player clicks Load on a slot
    public void LoadSlotAndStart(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return;
        if (cachedSaves[slotIndex] == null) { Debug.Log("No save in that slot."); return; }
        pendingLoadedSave = cachedSaves[slotIndex];
        // Begin loading the scene stored in save
        SceneManager.LoadScene(pendingLoadedSave.sceneName);
    }

    // After scene loads we must apply pendingLoadedSave to the player object
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoadedSave != null)
        {
            // Find a SceneLoadHandler in the scene to apply the save (recommended)
            var handler = FindFirstObjectByType<SceneLoadHandler>();
            if (handler != null)
            {
                handler.ApplyLoadedSave(pendingLoadedSave);
                pendingLoadedSave = null;
            }
            else
            {
                // fallback: try to find player immediately
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    var playerSaveLoad = player.GetComponent<PlayerSaveLoad>();
                    if (playerSaveLoad != null)
                        playerSaveLoad.ApplySave(pendingLoadedSave);
                }
                pendingLoadedSave = null;
            }
        }
    }

    // Utility: delete a slot
    public void DeleteSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return;
        cachedSaves[slotIndex] = null;
        try { if (File.Exists(filePaths[slotIndex])) File.Delete(filePaths[slotIndex]); } catch { }
    }
}

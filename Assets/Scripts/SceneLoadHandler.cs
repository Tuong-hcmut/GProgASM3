// SceneLoadHandler.cs
using UnityEngine;

public class SceneLoadHandler : MonoBehaviour
{
    private void Start()
    {
        // If SaveManager has pendingLoadedSave, apply it here
        if (SaveManager.Instance != null && SaveManager.Instance.pendingLoadedSave != null)
        {
            ApplyLoadedSave(SaveManager.Instance.pendingLoadedSave);
            SaveManager.Instance.pendingLoadedSave = null;
        }
    }

    public void ApplyLoadedSave(SaveData sd)
    {
        if (sd == null) return;
        // find player by tag
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var psl = player.GetComponent<PlayerSaveLoad>();
            if (psl != null) psl.ApplySave(sd);
            else player.transform.position = sd.GetPosition();
        }
        else
        {
            // If player not found yet (spawned later), you could store sd in a temporary static var
            // For simplicity, do nothing here.
            Debug.LogWarning("Player not found to apply save. Ensure Player exists in scene or apply later.");
        }
    }
}

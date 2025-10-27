// PlayerSaveLoad.cs
using UnityEngine;

public class PlayerSaveLoad : MonoBehaviour
{
    public int currentHP = 100;
    public int mana = 100;

    public void ApplySave(SaveData sd)
    {
        if (sd == null) return;
        Vector3 pos = sd.GetPosition();
        // Optionally do a simple teleport:
        transform.position = pos;
        currentHP = sd.playerHP;
        mana = sd.playerMana;
        Debug.Log($"Player applied save pos {pos}, HP {currentHP}, Lives {mana}");
    }
}

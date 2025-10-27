using UnityEngine;
using System;

[Serializable]
public class SaveData
{
    public float[] position;
    public string sceneName;
    public long timestampTicks;
    public int playerHP;
    public int playerMana;
    public SaveData() { }

    public SaveData(UnityEngine.Vector3 pos, string scene, int hp = 0, int mana = 0)
    {
        position = new float[3] { pos.x, pos.y, pos.z };
        sceneName = scene;
        timestampTicks = DateTime.Now.Ticks;
        playerHP = hp;
        playerMana = mana;
    }

    public UnityEngine.Vector3 GetPosition()
    {
        if (position != null && position.Length == 3)
        {
            return new UnityEngine.Vector3(position[0], position[1], position[2]);
        }
        return UnityEngine.Vector3.zero;
    }
    
    public string GetTimestampString()
    {
        var date_time = new DateTime(timestampTicks);
        return date_time.ToString("yyyy-MM-dd HH:mm");
    }
}

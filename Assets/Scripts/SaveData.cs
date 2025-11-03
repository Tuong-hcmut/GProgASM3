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
    public int playerScore;

    public SaveData() { }

    public SaveData(Vector3 pos, string scene, int hp = 0, int mana = 0, int score = 0)
    {
        position = new float[3] { pos.x, pos.y, pos.z };
        sceneName = scene;
        timestampTicks = DateTime.Now.Ticks;
        playerHP = hp;
        playerMana = mana;
        playerScore = score;
    }

    public Vector3 GetPosition()
    {
        if (position != null && position.Length == 3)
            return new Vector3(position[0], position[1], position[2]);
        return Vector3.zero;
    }

    public string GetTimestampString()
    {
        var dateTime = new DateTime(timestampTicks);
        return dateTime.ToString("yyyy-MM-dd HH:mm");
    }
}

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public PlayerData playerData;
    public List<EnemyData> enemyData;
}

[System.Serializable]
public class PlayerData
{
    public Vector3 position;
    public int health;
    public int coins;
}

[System.Serializable]
public class EnemyData
{
    public Vector3 position;
    public int health;
    public string enemyType; // Or use an enum for enemy types
}
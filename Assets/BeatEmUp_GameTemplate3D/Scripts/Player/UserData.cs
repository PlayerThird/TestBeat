using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserData
{
    public int coin;
    public int hp;
    public float damageMulti;

    public void SetParam(int coin, int hp, float damage)
    {
        this.coin = coin;
        this.hp = hp;
        this.damageMulti = damage;
    }
}

/*public partial class PlayerData
{
    public Vector3 pos, rotation, scale;
}*/

public class LevelCoord
{
    public Transform coord;
}

public class JsonData
{
    public List<UserData> Items { get; set; }
}

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


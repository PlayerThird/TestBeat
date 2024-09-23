using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

public class DataManager : MonoBehaviour
{
    public UserData usrData;
    public int coin;
    public int hp;
    public float damageMulti;

    public void SaveData(UserData data)
    {
        data.SetParam(coin, hp, damageMulti);
        Debug.Log("Сохранено1" + data);
        string json = JsonUtility.ToJson(data);
        Debug.Log("Сохранено2" + json);
        File.WriteAllText(Application.streamingAssetsPath + "/PlayerData.json", json);
    }


    public UserData DownloadData(UserData userData)
    {
        userData = JsonUtility.FromJson<UserData>(File.ReadAllText(Application.streamingAssetsPath +
                                                                   "/PlayerData.json"));
        return userData;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveData(usrData);
            Debug.Log("Сохранено в файл");
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            usrData = DownloadData(usrData);
            Debug.Log("Загружено");
            coin = usrData.coin;
            hp = usrData.hp;
            damageMulti = usrData.damageMulti;
        }
    }

    private void Awake()
    {
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class SaveManager : MonoBehaviour
{
    public static SaveData currentSaveData;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
            Debug.Log("Игра сохранена!");
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame();
            Debug.Log("Игра загружена!");
        }
    }


    public static void SaveGame()
    {
        // Get player data
        PlayerData playerData = new PlayerData();
        playerData.position = GameObject.FindWithTag("Player").transform.position;
        playerData.health = GameObject.FindWithTag("Player").GetComponent<HealthSystem>().BackCurrentHP();
        playerData.coins = GlobalGameSettings.GetCoins();

        // Get enemy data
        List<EnemyData> enemyData = new List<EnemyData>();
        foreach (GameObject enemy in EnemyManager.enemyList)
        {
            EnemyData enemyDataEntry = new EnemyData();
            enemyDataEntry.position = enemy.transform.position;
            enemyDataEntry.health = enemy.GetComponent<HealthSystem>().BackCurrentHP();
            enemyDataEntry.enemyType = enemy.name; // Or use an enum
            enemyData.Add(enemyDataEntry);
        }

        // Create save data
        currentSaveData = new SaveData();
        currentSaveData.playerData = playerData;
        currentSaveData.enemyData = enemyData;

        // Save data to a file (e.g., using JSON)
        string json = JsonUtility.ToJson(currentSaveData);
        File.WriteAllText(Application.streamingAssetsPath + "/jsonList.json", json);
    }
    public static void LoadGame()
    {
        // Load data from a file
        string json = File.ReadAllText(Application.streamingAssetsPath + "/jsonList.json");
        currentSaveData = JsonUtility.FromJson<SaveData>(json);

        // Restore player data
        GameObject player = GameObject.FindWithTag("Player");
        player.transform.position = currentSaveData.playerData.position;
        player.GetComponent<HealthSystem>().SetHealth(currentSaveData.playerData.health);
        GlobalGameSettings.SetCoin(currentSaveData.playerData.coins);

        // Restore enemy data
        foreach (EnemyData enemyData in currentSaveData.enemyData)
        {
            // Instantiate enemy based on enemy type
            GameObject enemyPrefab = Resources.Load<GameObject>(enemyData.enemyType);
            GameObject newEnemy = Instantiate(enemyPrefab, enemyData.position, Quaternion.identity);
            newEnemy.GetComponent<HealthSystem>().CurrentHp = enemyData.health;
        }
    }
    
}


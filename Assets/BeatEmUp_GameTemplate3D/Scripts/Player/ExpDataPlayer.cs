using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

public class ExpDataPlayer : MonoBehaviour
{
    /*
        public PlayerData playerDat;
        [SerializeField] private GameObject target;
        public GameObject playerPrefs;

        public class ListOfObject : IEnumerable
        {
            public List<PlayerData> plaList = new List<PlayerData>();
            public IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public void SaveData()
        {
            playerDat = new PlayerData()
            {
                pos = target.transform.position,
                //rotation = target.transform.rotation.eulerAngles,
                scale = target.transform.localScale
            };
            string forSave = JsonConvert.SerializeObject(playerDat);
            File.WriteAllText(Application.streamingAssetsPath + "/jsonList.json", forSave);
        }

        public void DownloadData()
        {
            PlayerData newPlayer =
                JsonConvert.DeserializeObject<PlayerData>(
                    File.ReadAllText(Application.streamingAssetsPath + "/jsonList.json"));
            //Destroy(target);
            GameObject player = GameObject.Instantiate(playerPrefs) as GameObject;
            player.transform.position = newPlayer.pos;
            //player.transform.rotation = Quaternion.Euler(newPlayer.rotation);
            player.transform.localScale = newPlayer.scale;


        }

        private void Update()
        {
            UpdatePlayerTargets();
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveData();
                Debug.Log("Сохранено в файл");
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                DownloadData();
                Debug.Log("Загружено");
            }
        }

        public void UpdatePlayerTargets()
        {
            target = GameObject.FindGameObjectWithTag("Player");
        }
    */
}
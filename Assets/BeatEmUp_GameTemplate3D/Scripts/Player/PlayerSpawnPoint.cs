using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour {

	public GameObject defaultPlayerPrefab;
	[SerializeField]
	public float newZ = 4.5f;//Скорость глубины хода, для битэмапа и 2Д

	void Awake(){

		//получить выбранного игрока на экране выбора персонажа
		if(GlobalGameSettings.Player1Prefab) {
			loadPlayer(GlobalGameSettings.Player1Prefab);
			return;
		}	

		//в противном случае загрузить стандартного персонажа(PlayerBeat)
		if(defaultPlayerPrefab) {
			loadPlayer(defaultPlayerPrefab);
		} else {
			Debug.Log("Please assign a default player prefab in the  playerSpawnPoint");
		}
	}

	//загрузка префаба игрока
	void loadPlayer(GameObject playerPrefab){
		GameObject player = GameObject.Instantiate(playerPrefab) as GameObject;
		player.GetComponent<PlayerMovement>().SetZspread(newZ);
		player.transform.position = transform.position;
	}
}
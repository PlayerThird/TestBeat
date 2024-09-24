using UnityEngine;
using System.Collections.Generic;

public static class GlobalGameSettings  {
	public static GameObject Player1Prefab;
	public static List<LevelData> LevelData = new List<global::LevelData>();
	public static int currentLevelId = 0;
	public static int coins;

	public static void SetCoin(int newCoin)
	{
		coins = newCoin;
	}

	public static int GetCoins()
	{
		return coins;
	}
}
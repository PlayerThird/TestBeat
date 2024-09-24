using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Combo
{
    public List<string> attacks;
    public string animationTrigger;
    // Другие параметры
}

public class ComboManager : MonoBehaviour
{
    public List<Combo> combos;
}

[System.Serializable]
public class ComboData
{
    public List<string> attacks;
    public string animationTrigger;
    // Другие параметры
}
[CreateAssetMenu(fileName = "ComboList", menuName = "ScriptableObjects/ComboList")]
public class ComboList : ScriptableObject
{
    public List<ComboData> combos;

    public void SaveToJson(string path)
    {
        string jsonString = JsonUtility.ToJson(this, true);
        File.WriteAllText(path, jsonString);
    }

    public void LoadFromJson(string path)
    {
        if (File.Exists(path))
        {
            string jsonString = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(jsonString, this);
        }
    }
}
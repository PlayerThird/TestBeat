using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComboList))]
public class ComboListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ComboList comboList = (ComboList)target;

        // Кнопка добавления нового комбо
        if (GUILayout.Button("Add Combo"))
        {
            comboList.combos.Add(new ComboData());
        }

        // Цикл по всем комбо
        for (int i = 0; i < comboList.combos.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            // Поля для редактирования каждого комбо
            // ...
            EditorGUILayout.EndHorizontal();

            // Кнопка удаления комбо
            if (GUILayout.Button("Delete"))
            {
                comboList.combos.RemoveAt(i);
                i--;
            }
        }

        // Кнопка сохранения в JSON
        if (GUILayout.Button("Save to JSON"))
        {
            string path = EditorUtility.SaveFilePanel("Save Combo List", "", "comboList", "json");
            if (!string.IsNullOrEmpty(path))
            {
                comboList.SaveToJson(path);
            }
        }
    }
}
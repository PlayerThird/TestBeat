using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class DialogeCut
{
    //Считываем файл и обрезаем на участке, который нам нужен
    public static string CutFile(string path, int number)
    {
        Object obj = Resources.Load("Dialogues/" + path);
        String line = obj.ToString();
        String[] secSave = line.Split("<d>");
        //возвращаем указанную строку с удалёнными переносами строк
        return secSave[number] = secSave[number].Replace(Environment.NewLine,"");
    }

    //Проверяем, кто говорит
    public static bool PlayerSay(string text)
    {
        String[] splitText = text.Split();
        if (splitText[0] == "(p)")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static string CutStart(string text)
    {
        string[] cuttingText = text.Split();
        cuttingText[0] = "";
        text = null;
        foreach (string str in cuttingText)
        {
            text += str + " ";
        }
       //text = text.Replace("(p)",string.Empty);
        return text;
    }
}
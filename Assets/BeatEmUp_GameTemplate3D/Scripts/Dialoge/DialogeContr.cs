using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DialogeContr : MonoBehaviour
{
    public TextMeshProUGUI textUi;
    public TextMeshProUGUI nameDialogeDude;
    public TextMeshProUGUI promtTest;

    private static String[] text;
    private static String nameDude;//Имя говорящего человека

    private static int numericWord = 1; //начинаем с следующего предложения т.к. вначале идёт цифра диалога
    private static bool startDialog; //Дебаггер
    private bool itsSayPlayer;


    public void StartDialoge(String textLink, String nameDialogDude)
    {
        text = textLink.Split(".");
        nameDude = nameDialogDude;
        startDialog = true;
        SetFirstWord();
    }

    private void SetFirstWord()
    {
        itsSayPlayer = DialogeCut.PlayerSay(text[numericWord]);
        textUi.text = DialogeCut.CutStart(text[numericWord]);
        nameDialogeDude.text = nameDude;
        promtTest.text = textVariant["dialoge"] + textVariant["end"];
        numericWord++;
    }

    public void CleanDialogue()
    {
        FindObjectOfType<UIManager>().CloseMenu("Dialog");

        text = null;
        nameDude = null;

        numericWord = 1;
        startDialog = false;
        FindObjectOfType<DialogeTrigger>().SetDialogActive();
        Debug.Log("Диалог закрыт и очищен");
    }


    void Update()
    {
        //выводим диалог по одному приложению
        if (Input.GetKeyDown(KeyCode.Q) && numericWord <= text.Length)
        {
            Debug.Log("Длина текста " + text.Length);
            Debug.Log("Число слова " + numericWord);
            Debug.Log("Текст диалога " + text[numericWord]);
            
            itsSayPlayer = DialogeCut.PlayerSay(text[numericWord]);
          //  text[numericWord] = ;
            textUi.text = DialogeCut.CutStart(text[numericWord]);
            
            numericWord++;
            
            if (numericWord + 1< text.Length)
            {
                promtTest.text = textVariant["dialoge"] + textVariant["end"];
            }
            else
            {
                promtTest.text = textVariant["end"];
            }
        }
        else if (text.Length == numericWord)
        {
            CleanDialogue();
        }

        if (itsSayPlayer)
        {
            nameDialogeDude.text = "Вы";
            nameDialogeDude.color = Color.cyan;
            textUi.color = Color.cyan;
        }
        else
        {
            nameDialogeDude.text = nameDude;
            nameDialogeDude.color = Color.red;
            textUi.color = Color.red;
        }
    }

    Dictionary<string, string> textVariant = new Dictionary<string, string>()
    {
        { "dialoge", "Нажмите Q чтобы продолжить" + Environment.NewLine},
        { "end", "Нажмите E чтобы закрыть" }
    };
}
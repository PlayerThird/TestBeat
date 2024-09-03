using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class DialogeContr : MonoBehaviour
{
    public TextMeshProUGUI textUi;
    private String line;
    private String[] secSave;
    private String[] text;
    private Object obj;
    private int numericWord=0;
    
    void Start()
    {
       obj = Resources.Load("Dialogues/Dialogs");
       line = obj.ToString();
       secSave = line.Split("<d2>");
       line = secSave.ToString();
       text = line.Split(".");
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && numericWord <= text.Length)
        {
            textUi.text = text[numericWord];
            numericWord++;
        }
    }
}
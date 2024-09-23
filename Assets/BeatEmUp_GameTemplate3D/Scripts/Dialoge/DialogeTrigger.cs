using System;
using UnityEngine;

public class DialogeTrigger : MonoBehaviour
{
    [SerializeField] public String nameFile; // название файла в папке Resources/Dialogues
    [SerializeField] public String nameDudeDialog; //имя говорящего
    public int numberDialog; //номер диалога из файла

    
    [SerializeField] private GameObject player;
    [SerializeField] private bool isTrigger;
    [SerializeField] private bool dialogActive;
    
    
    private void OnTriggerEnter(Collider collider)
    {
        player = collider.gameObject;
        if (player.CompareTag("Player"))
        {
            isTrigger = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        player = other.gameObject;
    }

    public void SetDialogActive()
    {
        dialogActive = false;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Вышли за пределы коллайдера диалога");

        isTrigger = false;
        dialogActive = false;

        FindObjectOfType<DialogeContr>().CleanDialogue();
    }

    void Update()
    {
        if (isTrigger)
        {
            if (dialogActive)
            {
                //выключаем скрипты, контролирующие передвижение игрока
                player.GetComponent<PlayerMovement>().enabled = false;
                player.GetComponent<PlayerCombat>().enabled = false;
            }
            else
            {
                //включаем скрипты, контролирующие передвижение игрока
                player.GetComponent<PlayerMovement>().enabled = true;
                player.GetComponent<PlayerCombat>().enabled = true;
            }
            
            if (Input.GetKeyUp(KeyCode.E) && dialogActive)
            {
                dialogActive = false;

                Debug.Log("Закрыли текст с помощью Е");
                FindObjectOfType<DialogeContr>().CleanDialogue();
            }

            else if (Input.GetKeyUp(KeyCode.E) && !dialogActive)
            {
                dialogActive = true;
                
                FindObjectOfType<UIManager>().ShowMenu("Dialog", false);
                String text = DialogeCut.CutFile(nameFile, numberDialog);

                Debug.Log("Текст, который ушёл в диалог" + text);

                FindObjectOfType<DialogeContr>().StartDialoge(text, nameDudeDialog);
            }
        }
    }
}
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public UIFader UI_fader;
    public UI_Screen[] UIMenus;
    private UI_Screen[] UISave;

    void Awake()
    {
        DisableAllScreens();

        //don't destroy
        DontDestroyOnLoad(gameObject);
    }

    //Показать меню по имени
    public void ShowMenu(string name, bool disableAllScreens)
    {
        if (disableAllScreens) DisableAllScreens();

        foreach (UI_Screen UI in UIMenus)
        {
            if (UI.UI_Name == name)
            {
                if (UI.UI_Gameobject != null)
                {
                    UI.UI_Gameobject.SetActive(true);
                    UI.active = false;
                    SetTouchScreenControls(UI);
                }
                else
                {
                    Debug.Log("Не нашлось меню с именем: " + name);
                }
            }
        }

        //fadeIn(появление меню)
        if (UI_fader != null) UI_fader.gameObject.SetActive(true);
        UI_fader.Fade(UIFader.FADE.FadeIn, .5f, .3f);
    }

    public void ShowMenu(string name)
    {
        ShowMenu(name, true);
    }

    //Закрыть меню по имени
    public void CloseMenu(string name)
    {
        foreach (UI_Screen UI in UIMenus)
        {
            if (UI.UI_Name == name)
            {
                UI.UI_Gameobject.SetActive(false);
                UI.active = false;
            }
        }
    }

    //Выключить все меню
    public void DisableAllScreens()
    {
        foreach (UI_Screen UI in UIMenus)
        {
            if (UI.UI_Gameobject != null)
            {
                UI.UI_Gameobject.SetActive(false);
                UI.active = false;
            }
            else
                Debug.Log("Null ref found in UI with name: " + UI.UI_Name);
        }
    }

    //показать или спрятать touch screen controls
    void SetTouchScreenControls(UI_Screen UI)
    {
        if (UI.UI_Name == "TouchScreenControls") return;
        InputManager inputManager = GameObject.FindObjectOfType<InputManager>();
        if (inputManager != null && inputManager.inputType == INPUTTYPE.TOUCHSCREEN)
        {
            if (UI.showTouchControls)
            {
                ShowMenu("TouchScreenControls", false);
            }
            else
            {
                CloseMenu("TouchScreenControls");
            }
        }
    }
}

[System.Serializable]
public class UI_Screen
{
    public string UI_Name;
    public GameObject UI_Gameobject;
    public bool active;
    public bool showTouchControls;
}
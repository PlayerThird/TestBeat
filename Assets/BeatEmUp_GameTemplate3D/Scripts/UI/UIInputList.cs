using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInputList : MonoBehaviour {

	private int maxIconCount = 20; //the max number of icons in the list
	private float time = 0;
	private float timeCombo = 0;
	public TextMeshProUGUI textCombo;
	public TextMeshProUGUI timeText;
	public TextMeshProUGUI windowComb;

	void OnEnable(){
		InputManager.onInputEvent += OnInputEvent;
	}

	void OnDisable(){
		InputManager.onInputEvent -= OnInputEvent;
	}
		
	void OnInputEvent(string action, BUTTONSTATE buttonState){
		if(buttonState != BUTTONSTATE.PRESS) return; //only respond to button press states

		Sprite icon = Resources.Load<Sprite>("Icons/Icon" + action);
		if(icon != null) AddIcon(action.ToString(), icon);
	}
		
	//adds a new icon to the input list
	void AddIcon(string iconName, Sprite iconSprite){
		GameObject icon = new GameObject();
		Image img = icon.AddComponent<Image>();
		img.sprite = iconSprite;
		icon.GetComponent<RectTransform>().SetParent(transform);
		icon.GetComponent<RectTransform>().transform.localScale = Vector3.one;
		icon.transform.SetAsFirstSibling();
		icon.name = iconName;
		icon.gameObject.AddComponent<UISpriteFade>();
		if(transform.childCount > maxIconCount) Destroy(transform.GetChild(maxIconCount).gameObject);
	}

	private void Update()
	{
		timeCombo = FindObjectOfType<PlayerCombat>().GetTimeCombo();
		textCombo.text =("Time,combo:" + timeCombo.ToString());
		time = FindObjectOfType<PlayerCombat>().GetTime();
		timeText.text = ("Time,time: " + time.ToString());
		windowComb.text = FindObjectOfType<PlayerCombat>().GetComboWindow() ? "Yes" : "No";
	}
}
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UserStudyUIDropdownMenuButton : MonoBehaviour {

    public UserStudyUIDropdownMenu dropdownMenu;
    public Text buttonText;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void UpdateUserStudyDropdownMenu(int value)
    {
        dropdownMenu.UpdateDropdownMenu(buttonText.text, value);
    }
}

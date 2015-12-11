using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {

    public GameObject[] buttons;
    public GameObject userStudyUi;
    public bool showUI;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        /*if (Up(KeyCode.Tab))
        {
            if (!showUI)
            {
                foreach (GameObject gO in buttons)
                {
                    gO.SetActive(true);
                }

                showUI = true;
            }

            else
            {
                foreach (GameObject gO in buttons)
                {
                    gO.SetActive(false);
                }

                showUI = false;
            }
        }*/

        /*if (Input.GetButtonUp("UserStudyButton"))
        {
            if (userStudyUi != null)
            {
                if (userStudyUi.activeSelf == false)
                {
                    userStudyUi.SetActive(true);
                }
                else
                {
                    userStudyUi.SetActive(false);
                }
            }
            
        }*/
	
	}
}

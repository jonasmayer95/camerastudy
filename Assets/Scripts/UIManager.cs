using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {

    public GameObject[] buttons;
    public bool showUI;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.Tab))
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
        }
	
	}
}

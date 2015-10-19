using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KinectCursor : MonoBehaviour {

    public Transform hand;
    public Transform relativeBone; //In this case should be the shoulder
    private Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
    private RectTransform rectTransform;
    private Vector2 curserPos;
    public float selectionTime;
    private float selectionStartTime;
    private bool selecting;
    public EventSystem eventSystem;

	// Use this for initialization
	void Start () {
        rectTransform = GetComponent<RectTransform>();
	
	}
	
	// Update is called once per frame
	void Update () {


        // Updating cursor position
        curserPos = screenCenter + (Vector2)(hand.position - relativeBone.position) * Screen.width;
        curserPos.x = Mathf.Min(Screen.width, curserPos.x);
        curserPos.x = Mathf.Max(0, curserPos.x);
        curserPos.y = Mathf.Min(Screen.height, curserPos.y);
        curserPos.y = Mathf.Max(0, curserPos.y);
        rectTransform.position = curserPos;

        
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.name);
        if(other.tag == "UI")
        {
            selecting = true;
            selectionStartTime = Time.time;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.tag == "UI")
        {
            if(Time.time - selectionStartTime > selectionTime && selecting)
            {
                ExecuteEvents.Execute(other.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == "UI")
        {
            selecting = false;
        }
    }
}

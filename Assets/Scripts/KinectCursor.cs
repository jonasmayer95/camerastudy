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
    private Vector3 curserPos;
    public float selectionTime;
    private float selectionStartTime;
    private bool selecting;
    public EventSystem eventSystem;
    private Image pic;
    public RectTransform slider;

	// Use this for initialization
	void Start () {
        rectTransform = GetComponent<RectTransform>();
        pic = transform.GetChild(0).GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {


        // Updating cursor position
        curserPos = screenCenter + (Vector2)(hand.position - relativeBone.position) * Screen.width;
        curserPos.x = Mathf.Min(Screen.width, curserPos.x);
        curserPos.x = Mathf.Max(0, curserPos.x);
        curserPos.y = Mathf.Min(Screen.height, curserPos.y);
        curserPos.y = Mathf.Max(0, curserPos.y);
        curserPos.z = 0;
        rectTransform.position = curserPos;
        slider.position = rectTransform.position;

        
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
            pic.fillAmount = (Time.time - selectionStartTime) / selectionTime;
            if(Time.time - selectionStartTime > selectionTime && selecting)
            {
                
                ExecuteEvents.Execute(other.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
                selectionStartTime = Time.time;
                pic.fillAmount = 0;
            }
            
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == "UI")
        {
            selecting = false;
            pic.fillAmount = 0;
        }
    }
}

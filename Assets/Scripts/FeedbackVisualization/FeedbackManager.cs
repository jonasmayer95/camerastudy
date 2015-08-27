using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FeedbackManager : MonoBehaviour {

    public static FeedbackManager instance;

    public List<InseilFeedback> feedbackTypes = new List<InseilFeedback>();
    private int index;


    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {

        foreach (InseilFeedback iFB in feedbackTypes)
        {
            iFB.gameObject.SetActive(false);
        }
        ShowFeedback();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ShowFeedback()
    {

        feedbackTypes[index].gameObject.SetActive(true);
    }

    public void SwitchFeedbackType()
    {
        index = (index + 1) % feedbackTypes.Count;

        ShowFeedback();        
    }
}

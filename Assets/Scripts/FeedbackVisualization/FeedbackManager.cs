using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FeedbackManager : MonoBehaviour {

    public static FeedbackManager instance;

    public List<InseilExercise> exercises = new List<InseilExercise>();
    
    private int index = 0;


    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {

        foreach (InseilExercise iEX in exercises)
        {
            iEX.gameObject.SetActive(false);
        }
        //ShowFeedback(0);
	}
	
	// Update is called once per frame
	void Update () {

        if(Input.GetKeyDown(KeyCode.Space))
        {
            SwitchExercise((index + 1) % exercises.Count);
        }	
	}

    public void AddExercise(InseilExercise ex)
    {
        exercises.Add(ex);
    }

    public void ShowFeedback(int type)
    {
        exercises[index].enabledFeedBackType = (FeedbackType)type;
    }


    private void SwitchExercise(int id)
    {
        if (id >= 0 && id < exercises.Count && exercises.Count > 0)
        {
            exercises[index].gameObject.SetActive(false);
            index = id;
            exercises[index].gameObject.SetActive(true);
        }
    }

    public void NextExercise()
    {
        SwitchExercise((index + 1) % exercises.Count);
    }
}

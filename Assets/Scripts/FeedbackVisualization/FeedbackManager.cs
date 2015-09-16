using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FeedbackManager : MonoBehaviour {

    // Singleton
    public static FeedbackManager instance;

    // Stores all exercises found in the scene
    private List<InseilExercise> exercises = new List<InseilExercise>();
    
    // Index of active exercise
    private int index = 0;

    // Called before Start
    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {

        // Deactivate all exercises
        foreach (InseilExercise iEX in exercises)
        {
            iEX.gameObject.SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () {

        if(Input.GetKeyDown(KeyCode.Space))
        {
            SwitchExercise((index + 1) % exercises.Count);
        }	
	}

    // Used by exercises to register
    public void AddExercise(InseilExercise ex)
    {
        exercises.Add(ex);
    }

    // Shows feedback by type
    public void ShowFeedback(int type)
    {
        // Tells feedback which feedback state is active at the moment
        exercises[index].enabledFeedBackType = (FeedbackType)type;
    }

    // Called by GUI
    private void SwitchExercise(int id)
    {
        // Deactivate old exercise and activate new one
        if (id >= 0 && id < exercises.Count && exercises.Count > 0)
        {
            exercises[index].gameObject.SetActive(false);
            index = id;
            exercises[index].gameObject.SetActive(true);
        }
    }

    // Called by GUI
    public void NextExercise()
    {
        SwitchExercise((index + 1) % exercises.Count);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct Trial
{
    public Vector3 start;
    public Vector3 end;
    public uint number;

    public Trial(Vector3 start, Vector3 end, uint number)
    {
        this.start = start;
        this.end = end;
        this.number = number;
    }
}

enum TrialState 
{
    start,end
}

public class TargetSphere : MonoBehaviour {

    private Trial trial;
    private TrialState trialState;
    private Handedness handedness;
    private Material progressBar;
    public float progressBarTime;
    private float progressBarStartTime;
    //private Vector4 color = new Vector4();
    private Transform hip;
    private bool initialized = false;
	
    // Use this for initialization
	void Start () {
        progressBar = transform.GetChild(0).gameObject.GetComponent<Renderer>().material;
        progressBar.SetFloat("_Cutoff", 1);
	}
	
	// Update is called once per frame
	void Update () {
        if (initialized)
        {
            if (trialState == TrialState.start)
            {
                transform.position = hip.position + trial.start;
            }
            else
            {
                transform.position = hip.position + trial.end;
            }
        }
	}

    public void InitTargetSphere(Trial trial, Handedness handedness, Transform hip)
    {
        this.trial = trial;
        this.handedness = handedness;
        transform.position = trial.start;
        this.hip = hip;
        initialized = true;
        progressBarStartTime = 0.0f;
        trialState = TrialState.start;
    }

    void OnTriggerEnter(Collider other)
    {
        if (handedness == Handedness.LeftHanded && other.name == "RightHand" || handedness == Handedness.RightHanded && other.name == "LeftHand")
        {
            if (trialState == TrialState.end)
            {
                progressBar.SetFloat("_Cutoff", 1);
                UserStudyLogic.instance.EndTrial();
            }
            progressBarStartTime = Time.time;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (trialState == TrialState.start && (handedness == Handedness.LeftHanded && other.name == "RightHand" || handedness == Handedness.RightHanded && other.name == "LeftHand"))
        {
            progressBar.SetFloat("_Cutoff", 1 - (Time.time - progressBarStartTime) / progressBarTime);
            if (Time.time - progressBarStartTime > progressBarTime)
            {
                trialState = TrialState.end;
                progressBar.SetFloat("_Cutoff", 1);
                UserStudyLogic.instance.StartTrial();        
            }
        }
    }
}

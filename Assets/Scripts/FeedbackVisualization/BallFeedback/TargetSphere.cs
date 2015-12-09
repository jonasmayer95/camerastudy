using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum TrialState 
{
    start,end, waiting
}

public class TargetSphere : MonoBehaviour {

    private PositionSet pos;
    private TrialState trialState;
    private Handedness handedness;
    private Material progressBar;
    public float progressBarTime;
    public GameObject particles;
    private float progressBarStartTime;
    //private Vector4 color = new Vector4();
    private Transform hip;
    private bool initialized = false;
    private float pulseSpeed = 0.025f;
    private float pulseWidth = 0.025f;
    private float pulseStartTime;
	
    // Use this for initialization
	void Start () {
        progressBar = transform.GetChild(1).gameObject.GetComponent<Renderer>().material;
        progressBar.SetFloat("_Cutoff", 1);

	}
	
	// Update is called once per frame
	void Update () {
        if (initialized)
        {
            if (trialState == TrialState.start)
            {
                transform.position = hip.position + pos.StartPosition;
            }
            else
            {
                transform.position = hip.position + pos.EndPosition;
                float pulse = Mathf.PingPong((Time.time - pulseStartTime) * pulseSpeed, pulseWidth);
                transform.GetChild(0).localScale = new Vector3(pulse, pulse, pulse);
            }
        }
	}

    public void InitTargetSphere(PositionSet pos, Handedness handedness, Transform hip)
    {
        this.pos = pos;
        this.handedness = handedness;
        transform.position = pos.StartPosition;
        this.hip = hip;
        initialized = true;
        progressBarStartTime = 0.0f;
        trialState = TrialState.start;
        this.GetComponent<SphereCollider>().radius = 1 / UserStudyLogic.instance.exercisePrecision * 0.5f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (handedness == Handedness.LeftHanded && other.name == "RightHand" || handedness == Handedness.RightHanded && other.name == "LeftHand")
        {
            if (trialState == TrialState.end)
            {
                trialState = TrialState.waiting;
                UserStudyLogic.instance.snapping = true;
                progressBar.SetFloat("_Cutoff", 1);
                UserStudyLogic.instance.EndTrial(other.gameObject);
                Destroy(Instantiate(particles, transform.position, Quaternion.identity), 2.5f);
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
                pulseStartTime = Time.time;
                progressBar.SetFloat("_Cutoff", 1);
                UserStudyLogic.instance.StartTrial();        
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(handedness == Handedness.LeftHanded && other.name == "RightHand" || handedness == Handedness.RightHanded && other.name == "LeftHand")
        {
            progressBar.SetFloat("_Cutoff", 1);
        }
    }
}

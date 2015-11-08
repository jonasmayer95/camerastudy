using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetSphere : MonoBehaviour {

    private List<Vector3> positions;
    private int positionIndex = 0;
    private Handedness handedness;
    private Material progressBar;
    public float progressBarTime;
    private float progressBarStartTime;
    private Vector4 color = new Vector4();
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
            transform.position = hip.position + positions[positionIndex];
        }
	}

    public void InitTargetSphere(List<Vector3> positions, Handedness handedness, Transform hip)
    {
        this.positions = positions;
        this.handedness = handedness;
        transform.position = positions[0];
        positionIndex = 0;
        this.hip = hip;
        initialized = true;
    }

  

    void OnTriggerEnter(Collider other)
    {
        if (positionIndex > 0 && (handedness == Handedness.LeftHanded && other.name == "RightHand" || handedness == Handedness.RightHanded && other.name == "LeftHand"))
        {
            positionIndex++;
            if(positionIndex >= positions.Count)
            {
                UserStudyLogic.instance.EndUserStudy();
            }
        }
        else
        {
            progressBarStartTime = Time.time;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (positionIndex == 0 && (handedness == Handedness.LeftHanded && other.name == "RightHand" || handedness == Handedness.RightHanded && other.name == "LeftHand"))
        {
            progressBar.SetFloat("_Cutoff", 1 - (Time.time - progressBarStartTime) / progressBarTime);

            if (Time.time - progressBarStartTime > progressBarTime)
            {               
                positionIndex++;
                progressBar.SetFloat("_Cutoff", 1);
                UserStudyLogic.instance.StartUserStudy(handedness);
            }
        }
    }
}

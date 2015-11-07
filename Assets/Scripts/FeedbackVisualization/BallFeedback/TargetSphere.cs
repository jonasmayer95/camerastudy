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
	
    // Use this for initialization
	void Start () {
        progressBar = transform.GetChild(0).gameObject.GetComponent<Renderer>().material;
        progressBar.SetFloat("_Cutoff", 1);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void InitTargetSphere(List<Vector3> positions, Handedness handedness)
    {
        this.positions = positions;
        this.handedness = handedness;
        transform.position = positions[0];
        positionIndex = 0;
    }

  

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("enter");
        if (positionIndex > 0 && (handedness == Handedness.RightHanded && other.name == "RightHand" || handedness == Handedness.LeftHanded && other.name == "LeftHand"))
        {
            positionIndex++;
            if (positionIndex < positions.Count)
            {
                transform.position = positions[positionIndex];
            }
            else
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
        Debug.Log(handedness);
        if (positionIndex == 0 && (handedness == Handedness.RightHanded && other.name == "RightHand" || handedness == Handedness.LeftHanded && other.name == "LeftHand"))
        {
            progressBar.SetFloat("_Cutoff", 1 - (Time.time - progressBarStartTime) / progressBarTime);

            if (Time.time - progressBarStartTime > progressBarTime)
            {
                UserStudyLogic.instance.StartUserStudy();
                positionIndex++;
                transform.position = positions[positionIndex];
                progressBar.SetFloat("_Cutoff", 1);
            }
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetSphere : MonoBehaviour {

    private List<Vector3> positions;
    public Transform rightShoulder;
    public Transform leftShoulder;
    private int positionIndex = 0;
    private Handedness handedness;
    private Material progressBar;
    public float progressBarTime;
    private float progressBarStartTime;
    private Vector4 color = new Vector4();
	
    // Use this for initialization
	void Start () {
        progressBar = transform.GetChild(0).gameObject.GetComponent<Material>();
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
        if (positionIndex > 0 && (handedness == Handedness.RightHanded && other.name == "RightHand" || handedness == Handedness.LeftHanded && other.name == "LeftHand"))
        {
            positionIndex++;
            if (positionIndex < positions.Count)
            {
                transform.position = positions[positionIndex];
            }
            else
            {
                //Load feedback summary screen
            }
        }
        else
        {
            progressBarStartTime = Time.time;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (positionIndex == 0 && (handedness == Handedness.RightHanded && other.name == "RightHand" || handedness == Handedness.LeftHanded && other.name == "LeftHand"))
        {
            color = progressBar.color;
            color.w = (Time.time - progressBarStartTime) / progressBarTime;
            progressBar.color = color;   
        }
    }
}

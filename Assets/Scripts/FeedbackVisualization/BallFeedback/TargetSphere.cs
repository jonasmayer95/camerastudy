using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetSphere : MonoBehaviour {

    private List<Vector3> positions;
    public Transform rightShoulder;
    public Transform leftShoulder;
    private int positionIndex = 0;
    private bool rightHanded;
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

    public void InitTargetSphere(List<Vector3> positions, bool rightHanded)
    {
        this.positions = positions;
        this.rightHanded = rightHanded;
        transform.position = positions[0];
        positionIndex = 0;
    }

    public Vector3 CalculateRandomOrbPosition(bool righthanded)
    {
        Vector3 pos;
        Transform rootBone;
        Transform joint;
        float distance = 0;
        if (righthanded)
        {
            rootBone = rightShoulder;
             joint = rootBone;
            while (joint.GetChild(0).name != "HandRight")
            {
                distance += (joint.GetChild(0).position - joint.transform.position).magnitude;
            }
            pos = Random.onUnitSphere * Random.value * distance + rootBone.transform.position;
        }
        else
        {
            rootBone = leftShoulder;
            joint = rootBone;
            while (joint.GetChild(0).name != "LeftHand")
            {
                distance += (joint.GetChild(0).position - joint.transform.position).magnitude;
            }
            pos = Random.onUnitSphere * Random.value * distance + rootBone.transform.position;
        }
        return pos;
    }

    void OnTriggerEnter(Collider other)
    {
        if (positionIndex > 0 && (rightHanded && other.name == "RightHand" || !rightHanded && other.name == "LeftHand"))
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
        if (positionIndex == 0 && (rightHanded && other.name == "RightHand" || !rightHanded && other.name == "LeftHand"))
        {
            color = progressBar.color;
            color.w = (Time.time - progressBarStartTime) / progressBarTime;
            progressBar.color = color;   
        }
    }
}

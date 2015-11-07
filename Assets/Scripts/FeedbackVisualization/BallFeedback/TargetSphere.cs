using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetSphere : MonoBehaviour {

    private List<Vector3> positions;
    public Transform rightShoulder;
    public Transform leftShoulder;
    private int positionIndex = 0;
    private bool rightHanded;

	// Use this for initialization
	void Start () {
        
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void InitTargetSphere(int posCount, bool rightHanded)
    {
        positions = new List<Vector3>();
        for (int i = 0; i < posCount; i++)
        {
            positions.Add(CalculateRandomOrbPosition(true));
        }
        this.rightHanded = rightHanded;
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
        if (rightHanded && other.name == "RightHand" || !rightHanded && other.name == "LeftHand")
        {
            positionIndex++;
            transform.position = positions[positionIndex];
        }
    }
}

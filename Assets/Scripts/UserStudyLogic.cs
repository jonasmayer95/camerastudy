using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct UserStudyTargetPositions
{
    public List<Vector3> positions;
}

public class UserStudyLogic : MonoBehaviour {

    public static UserStudyLogic instance;
    public GameObject cameraFeedbackPrefab;
    public GameObject targetSpherePrefab;
    public Transform leftShoulder, rightShoulder;
    private CameraFeedback cameraFeedback;
    private TargetSphere targetSphere;
    private List<UserStudyTargetPositions> targetPositions = new List<UserStudyTargetPositions>();
    private CameraType cameraType;

    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () 
    {
        SpawnUserStudyComponents();
	}
	
	// Update is called once per frame
	void Update () {
	
	}   

    private void SpawnUserStudyComponents()
    {
        // Spawn and init camerafeedback
        cameraFeedback = (Instantiate(cameraFeedbackPrefab, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<CameraFeedback>();
        cameraFeedback.gameObject.SetActive(false);
        targetSphere = (Instantiate(targetSpherePrefab, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<TargetSphere>();
        targetSphere.gameObject.SetActive(false);
    }

    public void InitNewUserStudy(CameraFeedbackMode feedbackType, Handedness handedness, CameraType cam)
    {
        cameraType = cam;
        targetSphere.gameObject.SetActive(true);

        // Temp debug code for testing
        UserStudyTargetPositions userStudyTargetPositions = new UserStudyTargetPositions();
        userStudyTargetPositions.positions = new List<Vector3>();
        userStudyTargetPositions.positions.Add(CalculateRandomOrbPosition(handedness));
        userStudyTargetPositions.positions.Add(CalculateRandomOrbPosition(handedness));
        targetPositions.Add(userStudyTargetPositions);

        targetSphere.InitTargetSphere(userStudyTargetPositions.positions, handedness);
    }

    public void StartUserStudy()
    {
        
    }

    public void EndUserStudy()
    {

    }

    public Vector3 CalculateRandomOrbPosition(Handedness hand)
    {
        Vector3 pos;
        Transform rootBone;
        Transform joint;
        float distance = 0;
        if (hand == Handedness.RightHanded)
        {
            rootBone = rightShoulder;
            joint = rootBone;
            while (joint.GetChild(0).name != "RightHand")
            {
                distance += (joint.GetChild(0).position - joint.transform.position).magnitude;
                joint = joint.GetChild(0);
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
                joint = joint.GetChild(0);
            }
            pos = Random.onUnitSphere * Random.value * distance + rootBone.transform.position;
        }
        return pos;
    }
}

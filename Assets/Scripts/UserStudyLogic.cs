using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct UserStudyTargetPositions
{
    public Vector3 startPosition;
    public Vector3 endPosition;
}

public class UserStudyLogic : MonoBehaviour {

    public static UserStudyLogic instance;
    public GameObject cameraFeedbackPrefab;
    public GameObject targetSpherePrefab;
    public BoneMap avatarBones;
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
        targetSphere.InitTargetSphere(0, true);
    }

    public void StartUserStudy()
    {
        
    }

    public void EndUserStudy()
    {

    }

    public BoneMap GetAvatarBones()
    {
        return avatarBones;
    }
}

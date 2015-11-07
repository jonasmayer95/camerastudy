using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct UserStudyTargetPositions
{
    public Vector3 startPosition;
    public Vector3 endPosition;
}

public class UserStudyLogic : MonoBehaviour {

    public CameraFeedback cameraFeedbackPrefab;
    private CameraFeedback cameraFeedback;
    private List<UserStudyTargetPositions> targetPositions = new List<UserStudyTargetPositions>();

	// Use this for initialization
	void Start () 
    {
        InitUserStudyComponents();
	}
	
	// Update is called once per frame
	void Update () {
	
	}   

    private void InitUserStudyComponents()
    {
        // Spawn and init camerafeedback
        cameraFeedback = (Instantiate(cameraFeedbackPrefab, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<CameraFeedback>();
    }

    public void StartUserStudy()
    {
        
    }

    public void EndUserStudy()
    {

    }
}

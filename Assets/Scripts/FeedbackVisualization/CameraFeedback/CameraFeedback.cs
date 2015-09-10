using UnityEngine;
using System.Collections;

public enum CameraFeedbackMode
{
    LinearArrow, Cylinder
}

public class CameraFeedback : InseilFeedback {

    public float camDistance;
    public Transform feedbackCamera;
    public Transform feedbackAvatar_hip;
    public Transform feedbackAvatar_joint;
    public Transform connectingJoint;
    public Vector3 relTargetPos;
    public float tolerance;
    public GameObject arrow3DPrefab;
    public GameObject targetSpherePrefab;
    public Color targetSphereColor;
    public GameObject line3DPrefab;
    public CameraFeedbackMode cameraFeedbackMode;
    private GameObject targetSphere;
    private GameObject arrow3D;
    private GameObject line3D;
    private LineRenderer rend;


	// Use this for initialization
    void Start()
    {
        if (cameraFeedbackMode == CameraFeedbackMode.LinearArrow)
        {
            targetSphere = Instantiate(targetSpherePrefab, feedbackAvatar_hip.position + relTargetPos, Quaternion.identity) as GameObject;
            targetSphere.GetComponent<Renderer>().material.color = targetSphereColor;
            targetSphere.transform.parent = transform;
            targetSphere.SetActive(false);

            arrow3D = Instantiate(arrow3DPrefab, feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + relTargetPos) - feedbackAvatar_joint.position) / 2.0f, Quaternion.identity) as GameObject;
            arrow3D.transform.parent = transform;
            arrow3D.SetActive(false);

            line3D = Instantiate(line3DPrefab, transform.position, Quaternion.identity) as GameObject;
            line3D.transform.parent = transform;
            rend = line3D.GetComponent<LineRenderer>();
            rend.SetVertexCount(2);
            rend.SetPosition(0, transform.position);
            rend.SetPosition(1, connectingJoint.position);

            UpdateCameraPosition();
        }

        if (cameraFeedbackMode == CameraFeedbackMode.Cylinder)
        {

        }
    }

    void OnEnable()
    {
        //UpdateCameraPosition();
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (cameraFeedbackMode == CameraFeedbackMode.LinearArrow)
        {
            if (Vector3.Distance(feedbackAvatar_joint.position - feedbackAvatar_hip.position, relTargetPos) > tolerance)
            {
                targetSphere.SetActive(true);
                targetSphere.transform.position = feedbackAvatar_hip.position + relTargetPos;

                arrow3D.transform.position = feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + relTargetPos) - feedbackAvatar_joint.position) / 2.0f;
                arrow3D.transform.LookAt(targetSphere.transform.position);
                arrow3D.SetActive(true);
            }

            else
            {
                targetSphere.SetActive(false);
                arrow3D.SetActive(false);
            }

            UpdateCameraPosition();
            rend.SetPosition(1, connectingJoint.position);
        }

        if (cameraFeedbackMode == CameraFeedbackMode.Cylinder)
        {

        }
	}

    void UpdateCameraPosition()
    {
        // Calculate vector of arrow
        Vector3 arrowVector = targetSphere.transform.position - feedbackAvatar_joint.position;
        arrowVector = arrowVector.normalized;

        // Calculate middled vector
        Vector3 boneVectorA = feedbackAvatar_joint.GetChild(0).transform.position - feedbackAvatar_joint.position;
        boneVectorA = boneVectorA.normalized;

        Transform parent = feedbackAvatar_joint.parent.transform;
        Vector3 boneVectorB = parent.position - feedbackAvatar_joint.position;
        boneVectorB = boneVectorB.normalized;        

        // Calculate normal for camera
       // Vector3 camNormal = Vector3.Cross(arrowVector, ((boneVectorA + boneVectorB) - feedbackAvatar_joint.position).normalized);
        Vector3 camNormal = Vector3.Cross(boneVectorA, boneVectorB);

        //feedbackCamera.position = arrow3D.transform.position - camNormal.normalized * camDistance;
        //feedbackCamera.transform.LookAt(feedbackAvatar_joint.transform.position);
        feedbackCamera.position = feedbackAvatar_joint.transform.position + camDistance * Vector3.forward;
    }

}

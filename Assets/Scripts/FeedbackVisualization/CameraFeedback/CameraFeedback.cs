using UnityEngine;
using System.Collections;

public class CameraFeedback : InseilFeedback {

    public Vector3 cameraPosition;
    public Vector3 cameraOrientation;
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
    private GameObject targetSphere;
    private GameObject arrow3D;
    private GameObject line3D;

	// Use this for initialization
    void Start()
    {
        cameraPosition = feedbackCamera.position;
        cameraOrientation = Quaternion.ToEulerAngles(feedbackCamera.rotation);

        targetSphere = Instantiate(targetSpherePrefab, feedbackAvatar_hip.position + relTargetPos, Quaternion.identity) as GameObject;
        targetSphere.GetComponent<Renderer>().material.color = targetSphereColor;
        targetSphere.transform.parent = transform;
        targetSphere.SetActive(false);

        arrow3D = Instantiate(arrow3DPrefab, feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + relTargetPos) - feedbackAvatar_joint.position) / 2.0f, Quaternion.identity) as GameObject;
        arrow3D.transform.parent = transform;
        arrow3D.SetActive(false);

        line3D = Instantiate(line3DPrefab, transform.position, Quaternion.identity) as GameObject;
        line3D.transform.parent = transform;
        LineRenderer rend = line3D.GetComponent<LineRenderer>();
        rend.SetVertexCount(2);
        rend.SetPosition(0, transform.position);
        rend.SetPosition(1, connectingJoint.position);
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (Vector3.Distance(feedbackAvatar_joint.position - feedbackAvatar_hip.position, relTargetPos) > tolerance)
        {
            targetSphere.SetActive(true);

            arrow3D.transform.position = feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + relTargetPos) - feedbackAvatar_joint.position) / 2.0f;
            arrow3D.transform.LookAt(feedbackAvatar_hip.position + relTargetPos);
            arrow3D.SetActive(true);
        }

        else
        {
            targetSphere.SetActive(false);
            arrow3D.SetActive(false);
        }

        feedbackCamera.position = cameraPosition;
        feedbackCamera.rotation = Quaternion.Euler(cameraOrientation);
	}
}

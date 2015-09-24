using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CameraFeedbackMode
{
    LinearArrow, Cylinder
}

public class CameraFeedback : InseilFeedback {

    // Camera Setup
    public float camDistance;
    public Camera feedbackCamera;

    // Joint setup
    public Transform feedbackAvatar_hip;
    public Transform feedbackAvatar_joint;
    public Transform connectingJoint;

    // Target position and tolerance
    public List<Vector3> positions = new List<Vector3>();
    public float tolerance;
    private int index;

    // Feedback mode
    public CameraFeedbackMode cameraFeedbackMode;

    // Prefabs
    public GameObject targetSpherePrefab;
    public Color targetSphereColor;
    public GameObject arrow3DPrefab;
    public GameObject cylinderPrefab;    
    public GameObject line3DPrefab;
    
    // Variables used for referencing
    private GameObject feedbackCylinder;    
    private GameObject targetSphere;
    private GameObject arrow3D;
    private GameObject line3D;
    private LineRenderer rend;


	// Use this for initialization
    void Start()
    {
        // Spawn a sphere to show the target position for debugging
        targetSphere = Instantiate(targetSpherePrefab, feedbackAvatar_hip.position + positions[index], Quaternion.identity) as GameObject;
        targetSphere.GetComponent<Renderer>().material.color = targetSphereColor;
        targetSphere.transform.parent = transform;
        targetSphere.SetActive(false);

        // Spawn a linear arrow pointing from the joint to the correct position
        arrow3D = Instantiate(arrow3DPrefab, feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + positions[index]) - feedbackAvatar_joint.position) / 2.0f, Quaternion.identity) as GameObject;
        arrow3D.transform.parent = transform;
        arrow3D.SetActive(false);

        // Spawn the line pointing from the joint to the camera image
        line3D = Instantiate(line3DPrefab, transform.position, Quaternion.identity) as GameObject;
        line3D.transform.parent = transform;
        rend = line3D.GetComponent<LineRenderer>();
        rend.SetVertexCount(2);
        rend.SetPosition(0, transform.position);
        rend.SetPosition(1, connectingJoint.position);

        // Spawn a cylinder showing an bent arrow from the joint to the correct position
        feedbackCylinder = Instantiate(cylinderPrefab, feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + positions[index]) - feedbackAvatar_joint.position) / 2.0f, Quaternion.identity) as GameObject;
        feedbackCylinder.transform.parent = transform;
        feedbackCylinder.SetActive(false);

        // Init camera position
        UpdateCameraPosition();
    }

    public override void InitFeedback(StaticJoint joint, Transform relTo, BoneMap bones)
    {
        //throw new System.NotImplementedException();
        positions.Add(joint.targetPosition);

        Transform bone;
        bones.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(joint.joint), out bone);
        connectingJoint = bone;
        Debug.Log(FeedbackCamera_Avatar.instance.GetBoneMap()[BoneMap.GetBoneMapKey(joint.joint)]);
        FeedbackCamera_Avatar.instance.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(joint.joint), out bone);
        feedbackAvatar_joint = bone;

        FeedbackCamera_Avatar.instance.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey("spinebase"), out bone);
        feedbackAvatar_hip = bone;

        feedbackCamera = FeedbackCamera_Avatar.instance.feedbackCamera;
    }

    public override void InitFeedback(MotionJoint joint, Transform relTo, BoneMap bones)
    {
        //throw new System.NotImplementedException();
        positions.Add(joint.startPosition);
        positions.Add(joint.endPosition);

        Transform bone;
        bones.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(joint.joint), out bone);
        connectingJoint = bone;

        FeedbackCamera_Avatar.instance.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(joint.joint), out bone);
        feedbackAvatar_joint = bone;

        FeedbackCamera_Avatar.instance.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey("spinebase"), out bone);
        feedbackAvatar_hip = bone;

        feedbackCamera = FeedbackCamera_Avatar.instance.feedbackCamera;
    }

    void OnEnable()
    {
        //UpdateCameraPosition();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == feedbackAvatar_joint)
        {
            index = (index + 1) % positions.Count;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Show the arrow only when out of tolerance
        if (Vector3.Distance(feedbackAvatar_joint.position - feedbackAvatar_hip.position, positions[index]) > tolerance)
        {
            // Show sphere at target position (for debugging)
            targetSphere.SetActive(true);
            targetSphere.transform.position = feedbackAvatar_hip.position + positions[index];

            // Update the position and orientation of the 3D arrow
            if (cameraFeedbackMode == CameraFeedbackMode.LinearArrow)
            {
                // Deactivate the cylinder
                feedbackCylinder.SetActive(false);

                float radius = Vector3.Distance(targetSphere.transform.position, feedbackAvatar_joint.position) / 2.0f;
                arrow3D.transform.localScale = new Vector3(radius, radius, radius);

                // Update and activate the 3D arrow
                arrow3D.transform.position = feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + positions[index]) - feedbackAvatar_joint.position) / 2.0f;
                arrow3D.transform.LookAt(targetSphere.transform.position);

                // Activate the 3D arrow
                arrow3D.SetActive(true);
            }

            // Update the position size and orientation of the cylinder showing a bent arrow
            if (cameraFeedbackMode == CameraFeedbackMode.Cylinder)
            {
                // Deactivate the 3D arrow
                arrow3D.SetActive(false);

                // Update position and size
                feedbackCylinder.transform.position = feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + positions[index]) - feedbackAvatar_joint.position) / 2.0f;
                float radius = Vector3.Distance(targetSphere.transform.position, feedbackAvatar_joint.position) / 2.0f;
                feedbackCylinder.transform.localScale = new Vector3(radius, radius, radius);

                // Update orientation
                Vector2 projectedErrorVector;
                projectedErrorVector = (targetSphere.transform.position - feedbackAvatar_joint.position).normalized;
                Vector3 eulerOrientation = Vector3.Cross(projectedErrorVector, feedbackCamera.transform.forward.normalized);
                
                // Upper Left
                if (feedbackAvatar_joint.position.y - targetSphere.transform.position.y >= 0 && targetSphere.transform.position.x - feedbackAvatar_joint.position.x > 0)
                {
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
                    Debug.Log("Now upper left ");

                    Quaternion upRotation = Quaternion.FromToRotation(-feedbackCylinder.transform.forward, eulerOrientation);
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation, Time.deltaTime * 20);                    
                }

                // Bottom Left
                if (targetSphere.transform.position.y - feedbackAvatar_joint.position.y > 0 && targetSphere.transform.position.x - feedbackAvatar_joint.position.x > 0)
                {
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
                    Debug.Log("Now bottom left ");

                    Quaternion upRotation = Quaternion.FromToRotation(-feedbackCylinder.transform.forward, eulerOrientation) * Quaternion.Euler(180, 0, 0);
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation, Time.deltaTime * 20); 
                }                

                // Upper Right
                if (feedbackAvatar_joint.position.y - targetSphere.transform.position.y > 0 && feedbackAvatar_joint.position.x - targetSphere.transform.position.x >= 0)
                {                    
                    Debug.Log("Now upper right ");
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
                    Quaternion upRotation = Quaternion.FromToRotation(feedbackCylinder.transform.forward, eulerOrientation) * Quaternion.Euler(0, 0, 180);
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation, Time.deltaTime * 20);
                }

                // Bottom right
                if (targetSphere.transform.position.y - feedbackAvatar_joint.position.y >= 0 && feedbackAvatar_joint.position.x - targetSphere.transform.position.x >= 0)
                {                    
                    Debug.Log("Now bottom right ");
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
                    Quaternion upRotation = Quaternion.FromToRotation(feedbackCylinder.transform.forward, eulerOrientation) * Quaternion.Euler(0, 180, 0);
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation, Time.deltaTime * 20); 
                }

                // Activate cylinder
                feedbackCylinder.SetActive(true);
            }
        }
        
        // Within tolerance
        else
        {
            // Deactivate sphere and arrow
            targetSphere.SetActive(false);
            arrow3D.SetActive(false);
            feedbackCylinder.SetActive(false);
        }        

        // Update Camera position
        UpdateCameraPosition();

        // Update the end point of the 3D line according to the new position of the joint
        rend.SetPosition(1, connectingJoint.position);
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
        //Vector3 camNormal = Vector3.Cross(boneVectorA, boneVectorB);

        //feedbackCamera.position = arrow3D.transform.position - camNormal.normalized * camDistance;
        //feedbackCamera.transform.LookAt(feedbackAvatar_joint.transform.position);
        feedbackCamera.transform.position = feedbackAvatar_joint.transform.position + camDistance * Vector3.forward;
    }
}

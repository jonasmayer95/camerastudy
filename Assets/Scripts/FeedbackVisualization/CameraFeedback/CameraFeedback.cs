using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CameraFeedbackMode
{
    LinearArrow, Cylinder, RigedArrow, DockingArrow2D
}

public enum CameraPerspectives
{
    Front, Up, Side, Behind, Normal
}

public enum CameraSide
{
    Left, Right
}

public enum CameraMotionStates
{
    Jumping, Moving
}

public class CameraFeedback : MonoBehaviour {

    // Camera Setup
    public float camDistance;
    public Camera feedbackCamera;
    private bool showingWindow = false;

    // Joint setup
    public Transform feedbackAvatar_hip;
    public Transform feedbackAvatar_joint;
    public Transform connectingJoint;
    private float lineAlpha;

    // Target position and tolerance
    public List<Vector3> positions = new List<Vector3>();
    public float tolerance;
    private int index;

    // Feedback mode
    public CameraFeedbackMode cameraFeedbackMode = CameraFeedbackMode.RigedArrow;
    public CameraPerspectives cameraPerspective = CameraPerspectives.Front;
    public CameraMotionStates cameraMotion = CameraMotionStates.Jumping;
    public CameraSide cameraSide = CameraSide.Left;

    // Prefabs
    public GameObject targetSpherePrefab;
    public Color targetSphereColor;
    public GameObject arrow3DPrefab;
    public GameObject cylinderPrefab;    
    public GameObject line3DPrefab;
    public GameObject rigedArrowPrefab;
    public GameObject dockingArrow2DPrefab;
    public GameObject arrowDock2DPrefab;
   
    // Variables used for referencing
    private GameObject feedbackCylinder;    
    private GameObject targetSphere;
    private GameObject arrow3D;
    private GameObject line3D;
    private LineRenderer rend;
    private GameObject rigedArrow;
    private Transform rootBoneArrow;
    private GameObject dockingArrow2D;
    private GameObject arrowDock2D;
    private Renderer targetSphereRenderer;
    public float spriteDistance;
    private bool initialized = false;
    private bool left;


	// Use this for initialization
    void Start()
    {
        // Spawn a sphere to show the target position for debugging
        targetSphere = Instantiate(targetSpherePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        targetSphereRenderer = targetSphere.GetComponent<Renderer>();
        targetSphereRenderer.material.color = targetSphereColor;
        targetSphere.transform.parent = transform;
        targetSphere.SetActive(false);

        // Spawn a linear arrow pointing from the joint to the correct position
        arrow3D = Instantiate(arrow3DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        arrow3D.transform.parent = transform;
        arrow3D.SetActive(false);

        // Spawn the line pointing from the joint to the camera image
        line3D = Instantiate(line3DPrefab, transform.position, Quaternion.identity) as GameObject;
        line3D.transform.parent = transform;
        rend = line3D.GetComponent<LineRenderer>();
        rend.SetVertexCount(2);
        rend.SetPosition(0, transform.position);
        rend.SetPosition(1, transform.position);

        // Spawn a cylinder showing an bent arrow from the joint to the correct position
        feedbackCylinder = Instantiate(cylinderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        feedbackCylinder.transform.parent = transform;
        feedbackCylinder.SetActive(false);

        // Spawn a riged Arrow which can be bent during runtime
        rigedArrow = Instantiate(rigedArrowPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        rigedArrow.transform.parent = transform;
        //rootBoneArrow = rigedArrow.transform.GetChild(0).GetChild(0);
        //BendArrow(180);
        rigedArrow.SetActive(false);


        // Spawn a dockingStation to show the target position for debugging
        arrowDock2D = Instantiate(arrowDock2DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        arrowDock2D.transform.parent = feedbackCamera.transform;
        arrowDock2D.SetActive(false);

        // Spawn a dockingArrow pointing from the joint to the dock
        dockingArrow2D = Instantiate(dockingArrow2DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        dockingArrow2D.transform.parent = feedbackCamera.transform;
        dockingArrow2D.SetActive(false);

        // Init camera position
        //UpdateCameraPosition();
        positions.Add(Vector3.zero);
    }

    public void ResetWindow()
    {
        showingWindow = false;
    }

    public void InitCorrectionWindow(StaticJoint joint, FeedbackCamera_Avatar cameraAvatar, BoneMap basicAvatar, Vector3 windowPosition, bool enabled, float lineAlpha, bool left)
    {
        showingWindow = enabled;
        this.lineAlpha = lineAlpha;

        this.left = left;

        //throw new System.NotImplementedException();
        positions[0] = joint.targetPosition;

        Transform bone;
        basicAvatar.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(joint.joint, basicAvatar.gameObject.GetComponent<AvatarController>().mirroredMovement), out bone);
        connectingJoint = bone;

        cameraAvatar.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(joint.joint, cameraAvatar.gameObject.GetComponent<AvatarController>().mirroredMovement), out bone);
        feedbackAvatar_joint = bone;

        cameraAvatar.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey("spinebase", cameraAvatar.gameObject.GetComponent<AvatarController>().mirroredMovement), out bone);
        feedbackAvatar_hip = bone;

        feedbackCamera = cameraAvatar.feedbackCamera;

        rend.SetPosition(0, windowPosition);

        initialized = true;
        
        UpdateCameraPosition();
    }

    void OnEnable()
    {
        //UpdateCameraPosition();
    }

    void OnTriggerEnter(Collider other)
    {
        if (initialized && other.gameObject == feedbackAvatar_joint)
        {
            index = (index + 1) % positions.Count;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (initialized)
        {
            // Show sphere at target position (for debugging)
            targetSphere.SetActive(true);
            targetSphere.transform.position = feedbackAvatar_hip.position + positions[index];

            // Update the position and orientation of the 3D arrow
            if (cameraFeedbackMode == CameraFeedbackMode.LinearArrow)
            {
                // Deactivate the cylinder
                feedbackCylinder.SetActive(false);
                rigedArrow.SetActive(false);
                dockingArrow2D.SetActive(false);
                arrowDock2D.SetActive(false);

                float radius = Vector3.Distance(targetSphere.transform.position, feedbackAvatar_joint.position) / 2.0f;
                arrow3D.transform.localScale = new Vector3(radius, radius, radius);

                // Update and activate the 3D arrow
                arrow3D.transform.position = feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + positions[index]) - feedbackAvatar_joint.position) / 2.0f;
                arrow3D.transform.LookAt(targetSphere.transform.position);

                // Activate the 3D arrow
                arrow3D.SetActive(true);
                targetSphereRenderer.enabled = true;
            }

            if (cameraFeedbackMode == CameraFeedbackMode.RigedArrow)
            {
                // Deactivate the 3D arrow
                arrow3D.SetActive(false);
                feedbackCylinder.SetActive(false);
                dockingArrow2D.SetActive(false);
                arrowDock2D.SetActive(false);

                // Update and activate the riged 3D arrow
                rigedArrow.transform.position = feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + positions[index]) - feedbackAvatar_joint.position) / 2.0f;

                rigedArrow.transform.LookAt(targetSphere.transform.position);
                Quaternion baseRot = rigedArrow.transform.rotation;
                rigedArrow.transform.rotation = baseRot * Quaternion.Euler(-90, 0, 0);

                float radius = Vector3.Distance(targetSphere.transform.position, feedbackAvatar_joint.position) / 2.0f;
                rigedArrow.transform.localScale = new Vector3(radius , radius , radius );

                // Activate riged Arrow
                rigedArrow.SetActive(true);
                targetSphereRenderer.enabled = true;
            }

            // Update the position size and orientation of the cylinder showing a bent arrow
            if (cameraFeedbackMode == CameraFeedbackMode.Cylinder)
            {
                // Deactivate the 3D arrow
                arrow3D.SetActive(false);
                rigedArrow.SetActive(false);
                dockingArrow2D.SetActive(false);
                arrowDock2D.SetActive(false);


                // Update position and size
                feedbackCylinder.transform.position = feedbackAvatar_joint.position + ((feedbackAvatar_hip.position + positions[index]) - feedbackAvatar_joint.position) / 2.0f;
                float radius = Vector3.Distance(targetSphere.transform.position, feedbackAvatar_joint.position) / 1.5f;
                feedbackCylinder.transform.localScale = new Vector3(radius, radius, radius);

                // Update orientation
                Vector2 projectedErrorVector;
                projectedErrorVector = (targetSphere.transform.position - feedbackAvatar_joint.position).normalized;
                Vector3 eulerOrientation = Vector3.Cross(projectedErrorVector, feedbackCamera.transform.forward.normalized);

                // Upper Left
                if (feedbackAvatar_joint.position.y - targetSphere.transform.position.y >= 0 && targetSphere.transform.position.x - feedbackAvatar_joint.position.x > 0)
                {
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
                    //Debug.Log("Now upper left ");

                    Quaternion upRotation = Quaternion.FromToRotation(-feedbackCylinder.transform.forward, eulerOrientation);// ;
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation * Quaternion.AngleAxis(35, Vector3.forward), Time.deltaTime * 20);
                }

                // Bottom Left
                if (targetSphere.transform.position.y - feedbackAvatar_joint.position.y > 0 && targetSphere.transform.position.x - feedbackAvatar_joint.position.x > 0)
                {
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
                    //Debug.Log("Now bottom left ");

                    Quaternion upRotation = Quaternion.FromToRotation(-feedbackCylinder.transform.forward, eulerOrientation) * Quaternion.Euler(180, 0, 0);
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation, Time.deltaTime * 20);
                }

                // Upper Right
                if (feedbackAvatar_joint.position.y - targetSphere.transform.position.y > 0 && feedbackAvatar_joint.position.x - targetSphere.transform.position.x >= 0)
                {
                    //Debug.Log("Now upper right ");
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
                    Quaternion upRotation = Quaternion.FromToRotation(feedbackCylinder.transform.forward, eulerOrientation) * Quaternion.Euler(0, 0, 180);
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation * Quaternion.AngleAxis(-35, Vector3.forward), Time.deltaTime * 20);
                }

                // Bottom right
                if (targetSphere.transform.position.y - feedbackAvatar_joint.position.y >= 0 && feedbackAvatar_joint.position.x - targetSphere.transform.position.x >= 0)
                {
                    //Debug.Log("Now bottom right ");
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
                    Quaternion upRotation = Quaternion.FromToRotation(feedbackCylinder.transform.forward, eulerOrientation) * Quaternion.Euler(0, 180, 0);
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation, Time.deltaTime * 20);
                }

                // Activate cylinder
                feedbackCylinder.SetActive(true);
                targetSphereRenderer.enabled = true;
            }

            // Update the position and orientation of the 3D arrow
            if (cameraFeedbackMode == CameraFeedbackMode.DockingArrow2D)
            {
                // Deactivate the 3D arrow
                arrow3D.SetActive(false);
                rigedArrow.SetActive(false);
                feedbackCylinder.SetActive(false);
                targetSphereRenderer.enabled = false;

                dockingArrow2D.transform.position = feedbackCamera.transform.position + spriteDistance * (feedbackAvatar_joint.position - feedbackCamera.transform.position).normalized;
                arrowDock2D.transform.position = feedbackCamera.transform.position + spriteDistance * (targetSphere.transform.position - feedbackCamera.transform.position).normalized;

                dockingArrow2D.transform.localRotation = Quaternion.identity;
                arrowDock2D.transform.localRotation = Quaternion.identity;                

                Vector2 dir = arrowDock2D.transform.localPosition - dockingArrow2D.transform.localPosition;
                dir.Normalize();

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                dockingArrow2D.transform.localRotation = Quaternion.Euler(dockingArrow2D.transform.localRotation.x, 0, angle);
                arrowDock2D.transform.localRotation = Quaternion.Euler(dockingArrow2D.transform.localRotation.x, 0, angle);                

                //Activate the Dock and arrow 
                dockingArrow2D.SetActive(true);
                arrowDock2D.SetActive(true);
            }
        }


        // Not active
        else
        {
            // Deactivate sphere and arrow
            targetSphere.SetActive(false);
            arrow3D.SetActive(false);
            feedbackCylinder.SetActive(false);
            rigedArrow.SetActive(false);
            dockingArrow2D.SetActive(false);
            arrowDock2D.SetActive(false);
        }

        // Update Camera position
        UpdateCameraPosition();

        // Update the end point of the 3D line according to the new position of the joint
        if (showingWindow && lineAlpha == 1)
        {
            rend.enabled = true;
            rend.SetPosition(1, connectingJoint.position);
        }
        else
        {
            rend.enabled = false;
        }

    }

    void UpdateCameraPosition()
    {
        if (initialized)
        {
            // Calculate vector of arrow
            Vector3 arrowVector = targetSphere.transform.position - feedbackAvatar_joint.position;
            arrowVector = arrowVector.normalized;
            /*
            if (Mathf.Abs(arrowVector.x) > Mathf.Abs(arrowVector.z))
            {
                cameraPerspective = CameraPerspectives.Front;
            }
            */
            //else
            {
                if (left)
                {
                    cameraSide = CameraSide.Left;
                }
                else
                {
                    cameraSide = CameraSide.Right;
                }
            }

            // Calculate middled vector
            //Vector3 boneVectorA = feedbackAvatar_joint.GetChild(0).transform.position - feedbackAvatar_joint.position;
            //boneVectorA = boneVectorA.normalized;

            //Transform parent = feedbackAvatar_joint.parent.transform;
            //Vector3 boneVectorB = parent.position - feedbackAvatar_joint.position;
            //boneVectorB = boneVectorB.normalized;

            // Calculate normal for camera
            // Vector3 camNormal = Vector3.Cross(arrowVector, ((boneVectorA + boneVectorB) - feedbackAvatar_joint.position).normalized);
            //Vector3 camNormal = Vector3.Cross(boneVectorA, boneVectorB);

            //feedbackCamera.position = arrow3D.transform.position - camNormal.normalized * camDistance;
            //feedbackCamera.transform.LookAt(feedbackAvatar_joint.transform.position);

            if (cameraMotion == CameraMotionStates.Jumping)
            {
                if (cameraPerspective == CameraPerspectives.Front)
                {
                    feedbackCamera.transform.rotation = Quaternion.identity;
                    feedbackCamera.transform.position = (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * -Vector3.forward;                    
                }

                if (cameraPerspective == CameraPerspectives.Behind)
                {
                    feedbackCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
                    feedbackCamera.transform.position = (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.forward;
                }

                if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Left)
                {
                    feedbackCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
                    feedbackCamera.transform.position = (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.left;
                }

                if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Right)
                {
                    feedbackCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
                    feedbackCamera.transform.position = (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.right;
                }

                if (cameraPerspective == CameraPerspectives.Up)
                {
                    feedbackCamera.transform.rotation = Quaternion.Euler(90,0,0);
                    feedbackCamera.transform.position = (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.up;
                }

                if (cameraPerspective == CameraPerspectives.Normal)
                {
                    if (cameraSide == CameraSide.Left)
                    {
                        feedbackCamera.transform.position = (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.Cross(targetSphere.transform.position - feedbackAvatar_joint.position, Vector3.up).normalized;
                        feedbackCamera.transform.LookAt((targetSphere.transform.position + feedbackAvatar_joint.position) * 0.5f);
                    }
                    else
                    {
                        feedbackCamera.transform.position = (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * -Vector3.Cross(targetSphere.transform.position - feedbackAvatar_joint.position, Vector3.up).normalized;
                        feedbackCamera.transform.LookAt((targetSphere.transform.position + feedbackAvatar_joint.position) * 0.5f);
                    
                    }
                }
            }

            if (cameraMotion == CameraMotionStates.Moving)
            {
                if (cameraPerspective == CameraPerspectives.Front)
                {
                    feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.identity, Time.deltaTime * 1);
                    feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * -Vector3.forward, Time.deltaTime * 1);
                }

                if (cameraPerspective == CameraPerspectives.Behind)
                {
                    feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, 180, 0), Time.deltaTime * 1);
                    feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.forward, Time.deltaTime * 1);
                }

                if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Left)
                {
                    feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation,Quaternion.Euler(0, 90, 0), Time.deltaTime * 1);
                    feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.left, Time.deltaTime * 1);
                }

                if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Right)
                {
                    feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, -90, 0), Time.deltaTime * 1);
                    feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.right, Time.deltaTime * 1);
                }

                if (cameraPerspective == CameraPerspectives.Up)
                {
                    feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(90,0,0), Time.deltaTime * 1);
                    feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.up, Time.deltaTime * 1);
                }

                if (cameraPerspective == CameraPerspectives.Normal)
                {
                    if (cameraSide == CameraSide.Left)
                    {
                        feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.Cross(targetSphere.transform.position - feedbackAvatar_joint.position, Vector3.up).normalized, Time.deltaTime * 1);
                        //feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, , Time.deltaTime * 1);                      
                        feedbackCamera.transform.LookAt((targetSphere.transform.position + feedbackAvatar_joint.position) * 0.5f);
                    }
                    else
                    {
                        feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * -Vector3.Cross(targetSphere.transform.position - feedbackAvatar_joint.position, Vector3.up).normalized, Time.deltaTime * 1);
                        //feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, , Time.deltaTime * 1);
                        feedbackCamera.transform.LookAt((targetSphere.transform.position + feedbackAvatar_joint.position) * 0.5f);
                    }
                }
            }
        }
    }

    void BendArrow(float angle)
    {
        int boneCount = 0;
        Transform child = rootBoneArrow;
        while(child.childCount > 0)
        {
            boneCount++;
            child = child.GetChild(0);
        }
        boneCount -= 1;
        float boneAngle = angle / boneCount;
        child = rootBoneArrow;
        while(boneCount != 0)
        {
            child.localRotation = Quaternion.Euler(0, 0, boneAngle);
            child = child.GetChild(0);
            boneCount--;
        }
    }
}

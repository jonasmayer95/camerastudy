using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class UserStudyLogic : MonoBehaviour
{

    public static UserStudyLogic instance;
    public float camDistance = 1.5f;
    public GameObject cameraFeedbackPrefab;
    public GameObject targetSpherePrefab;
    public Transform leftShoulder, rightShoulder, hip, leftHand, rightHand;
    private CameraFeedback cameraFeedback;
    private Camera feedbackCamera;
    private TargetSphere targetSphere;
    public List<List<Vector3>> targetPositions;
    private CameraFeedbackMode camFeedbackMode;
    private Transform feedbackAvatar_joint;
    private bool initialized;
    private CameraPerspectives cameraPerspective = CameraPerspectives.Front;
    public CameraMotionStates cameraMotion = CameraMotionStates.Jumping;
    private CameraSide cameraSide = CameraSide.Left;
    private UserStudyUI userStudyUI;

    private Vector3 startPosition;
    private Vector3 endPosition;

    // Object that has an attached MovementRecorder
    public GameObject userStudyObject;

    private void InitTargetPositions(Handedness handedness)
    {
        List<Vector3> positions = new List<Vector3>();
        targetPositions = new List<List<Vector3>>();

        //Initialize righthanded positions
        positions.Add(new Vector3(0.75f, 0.5f, -0.2f));
        positions.Add(new Vector3(0.5f, 0.75f, -0.3f));
        targetPositions.Add(positions);


        positions = new List<Vector3>();
        positions.Add(new Vector3(0.5f, 0.3f, -0.4f));
        positions.Add(new Vector3(0.6f, 0.6f, -0.1f));
        targetPositions.Add(positions);

        positions = new List<Vector3>();
        positions.Add(new Vector3(0.4f, 0.8f, 0));
        positions.Add(new Vector3(0.5f, 0.3f, -0.3f));
        targetPositions.Add(positions);

        positions = new List<Vector3>();
        positions.Add(new Vector3(0.6f, 0.2f, -0.5f));
        positions.Add(new Vector3(0.2f, 0.8f, -0.1f));
        targetPositions.Add(positions);

        positions = new List<Vector3>();
        positions.Add(new Vector3(0.1f, 0.3f, -0.75f));
        positions.Add(new Vector3(0.2f, 0.8f, -0.1f));
        targetPositions.Add(positions);

        //Flip directions if lefthanded
        if (handedness == Handedness.LeftHanded)
        {
            foreach (List<Vector3> posVector in targetPositions)
            {
                for (int i = 0; i < posVector.Count; i++)
                {
                    posVector[i] = new Vector3(posVector[i].x * -1, posVector[i].y, posVector[i].z);
                }
            }
        }
    }

    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        SpawnUserStudyComponents();

    }

    void Update()
    {

        if (initialized)
        {
            UpdateCameraPosition();
        }
    }

    private void SpawnUserStudyComponents()
    {
        feedbackCamera = Camera.main;
        // Spawn and init camerafeedback
        cameraFeedback = (Instantiate(cameraFeedbackPrefab, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<CameraFeedback>();
        cameraFeedback.gameObject.SetActive(false);
        targetSphere = (Instantiate(targetSpherePrefab, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<TargetSphere>();
        targetSphere.gameObject.SetActive(false);
    }

    public void InitNewUserStudy(CameraFeedbackMode feedbackType, Handedness handedness, CameraPerspectives cam, UserStudyUI userStudyUI)
    {
        this.userStudyUI = userStudyUI;
        cameraPerspective = cam;
        camFeedbackMode = feedbackType;
        Debug.Log(camFeedbackMode);
        targetSphere.gameObject.SetActive(true);

        InitTargetPositions(handedness);

        var positions = targetPositions[Random.Range(0, targetPositions.Count)];
        this.startPosition = positions[0];
        this.endPosition = positions[1];

        targetSphere.InitTargetSphere(positions, handedness, hip);

        if (handedness == Handedness.LeftHanded)
        {
            feedbackAvatar_joint = rightHand;
        }
        else
        {
            feedbackAvatar_joint = leftHand;
        }

        initialized = true;
    }

    public void StartUserStudy(Handedness handedness)
    {
        cameraFeedback.gameObject.SetActive(true);
        if (handedness == Handedness.LeftHanded)
        {
            cameraFeedback.InitCorrectionCamera(hip, rightHand, targetSphere.transform.position - hip.position, targetSphere.gameObject, camFeedbackMode);
            feedbackAvatar_joint = rightHand;
        }
        else
        {
            cameraFeedback.InitCorrectionCamera(hip, leftHand, targetSphere.transform.position - hip.position, targetSphere.gameObject, camFeedbackMode);
            feedbackAvatar_joint = leftHand;
        }

        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetCamera(cameraPerspective));

        //TODO: Send me a proper trial code + start and end ball positions
        //ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(trialcode, startPos, endPos));

        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.StartTrial(Time.time));
    }

    public void EndUserStudy()
    {
        targetSphere.gameObject.SetActive(false);
        cameraFeedback.gameObject.SetActive(false);
        initialized = false;
        userStudyUI.gameObject.SetActive(true);

        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.EndTrial(Time.time));
    }

    void UpdateCameraPosition()
    {
        // Calculate vector of arrow
        Vector3 arrowVector = targetSphere.transform.position - feedbackAvatar_joint.position;
        arrowVector = arrowVector.normalized;

        if (cameraPerspective == CameraPerspectives.Up)
        {
            cameraMotion = CameraMotionStates.Jumping;
        }

        if (cameraPerspective != CameraPerspectives.Up && cameraPerspective != CameraPerspectives.Behind
            && cameraPerspective != CameraPerspectives.Normal)
        {
            if (Mathf.Abs(arrowVector.x) > Mathf.Abs(arrowVector.z))
            {
                cameraPerspective = CameraPerspectives.Front;
            }

            else
            {
                cameraPerspective = CameraPerspectives.Side;
                if (targetSphere.transform.position.x < hip.transform.position.x)
                {
                    cameraSide = CameraSide.Left;
                }
                else
                {
                    cameraSide = CameraSide.Right;
                }
            }
        }

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
                feedbackCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
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
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, 90, 0), Time.deltaTime * 1);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.left, Time.deltaTime * 1);
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Right)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, -90, 0), Time.deltaTime * 1);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.right, Time.deltaTime * 1);
            }

            if (cameraPerspective == CameraPerspectives.Up)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(90, 0, 0), Time.deltaTime * 1);
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

    public Vector3 CalculateRandomOrbPosition(Handedness hand)
    {
        Vector3 pos;
        Transform rootBone;
        Transform joint;
        float distance = 0;
        if (hand == Handedness.LeftHanded)
        {
            rootBone = rightShoulder;
            joint = rootBone;
            while (joint.GetChild(0).name != "RightHand")
            {
                distance += (joint.GetChild(0).position - joint.position).magnitude;
                joint = joint.GetChild(0);
            }
            pos = Random.onUnitSphere * Random.value * distance + rootBone.transform.position - hip.position;
        }
        else
        {
            rootBone = leftShoulder;
            joint = rootBone;
            while (joint.GetChild(0).name != "LeftHand")
            {
                distance += (joint.GetChild(0).position - joint.position).magnitude;
                joint = joint.GetChild(0);
            }
            pos = Random.onUnitSphere * Random.value * distance + rootBone.position - hip.position;
        }
        return pos;
    }
}

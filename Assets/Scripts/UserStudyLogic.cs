using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public enum CameraPerspectives
{
    Side, Up, Normal, Front, Behind
}

public enum CameraUpdateMode
{
    Static, Updated
}

public enum CameraSide
{
    Left, Right
}

public enum CameraMotionStates
{
    Jumping, Moving
}

public class UserStudyLogic : MonoBehaviour
{

    public static UserStudyLogic instance;
    public float camDistance = 1.5f;
    public GameObject cameraFeedbackPrefab;
    public GameObject targetSpherePrefab;
    public Transform leftShoulder, rightShoulder, hip, leftHand, rightHand;
    private CameraFeedback cameraFeedback;
    private Camera feedbackCamera;
    private Vector3 camStartPos;
    private Quaternion camStartOrientation;
    private TargetSphere targetSphere;
    public List<List<Vector3>> targetPositions;
    private CameraFeedbackMode camFeedbackMode;
    private Transform feedbackAvatar_joint;
    private bool initialized;
    private CameraPerspectives cameraPerspective = CameraPerspectives.Front;
    public CameraMotionStates cameraMotion = CameraMotionStates.Jumping;
    private CameraSide cameraSide = CameraSide.Left;
    private CameraUpdateMode camUpdateMode = CameraUpdateMode.Static;
    private UserStudyUI userStudyUI;
    private bool camMotion;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private uint trialCounter;
    private uint numTrials;
    private Handedness handedness;

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
        camStartPos = feedbackCamera.transform.position;
        camStartOrientation = feedbackCamera.transform.rotation;
    }

    void Update()
    {

        if (camMotion)
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

    public void InitNewUserStudy(CameraFeedbackMode feedbackType, Handedness handedness, CameraPerspectives camPerspective, CameraMotionStates camMotion, UserStudyUI userStudyUI, uint numTrials)
    {
        this.userStudyUI = userStudyUI;
        cameraPerspective = camPerspective;
        cameraMotion = camMotion;
        camFeedbackMode = feedbackType;
        this.numTrials = numTrials;
        this.handedness = handedness;

        if (handedness == Handedness.LeftHanded)
        {
            feedbackAvatar_joint = rightHand;
        }
        else
        {
            feedbackAvatar_joint = leftHand;
        } 

        InitTargetPositions(handedness);

        ShuffleList(targetPositions);

        InitNewTrial();

        initialized = true;
    }

    private void InitNewTrial()
    {
        //var positions = targetPositions[Random.Range(0, targetPositions.Count - 1)];
        var positions = targetPositions[(int)trialCounter];
        this.startPosition = positions[0];
        this.endPosition = positions[1];

        Trial trial = new Trial(positions[0], positions[1], trialCounter);

        targetSphere.gameObject.SetActive(true); 
        targetSphere.InitTargetSphere(trial, handedness, hip);                      
    }

    public void StartTrial()
    {
        cameraFeedback.gameObject.SetActive(true);
        camMotion = true;
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

        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(trialCounter/*, startPosition, endPosition*/));

        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.StartTrial(Time.time));

        trialCounter++;
        Debug.Log(trialCounter + " TrialCounter" + numTrials + " numTrials");
    }

    public void EndTrial()
    {
        cameraFeedback.gameObject.SetActive(false);
        camMotion = false;
        feedbackCamera.transform.position = camStartPos;
        feedbackCamera.transform.rotation = camStartOrientation;
        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.EndTrial(Time.time));

        if (trialCounter < numTrials)
        {
            InitNewTrial();
        }
        else
        {
            trialCounter = 0;
            userStudyUI.gameObject.SetActive(true);
            initialized = false;
        }
    }

    private void ShuffleArray<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int r = Random.Range(0, i);
            T tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i);
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    void UpdateCameraPosition()
    {
        // Calculate vector of arrow
        Vector3 arrowVector = targetSphere.transform.position - feedbackAvatar_joint.position;
        arrowVector = arrowVector.normalized;

        if (cameraPerspective == CameraPerspectives.Side || cameraPerspective == CameraPerspectives.Normal)
        {
            if (handedness == Handedness.LeftHanded)
            {
                cameraSide = CameraSide.Left;
                Debug.Log("Enabled Lefthanded");
            }
            else
            {
                cameraSide = CameraSide.Right;
                Debug.Log("Enabled Righthanded");
            }
        }

        if (cameraMotion == CameraMotionStates.Jumping)
        {
            if (cameraPerspective == CameraPerspectives.Front)
            {
                feedbackCamera.transform.rotation = Quaternion.identity;
                feedbackCamera.transform.position = hip.position + (startPosition + endPosition) * 0.5f + camDistance * -Vector3.forward;
            }

            if (cameraPerspective == CameraPerspectives.Behind)
            {
                feedbackCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
                feedbackCamera.transform.position = hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.forward;
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Left)
            {
                feedbackCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
                feedbackCamera.transform.position = hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.left;
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Right)
            {
                feedbackCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
                feedbackCamera.transform.position = hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.right;
            }

            if (cameraPerspective == CameraPerspectives.Up)
            {
                feedbackCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
                feedbackCamera.transform.position = hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.up;
            }

            if (cameraPerspective == CameraPerspectives.Normal)
            {
                if (camUpdateMode == CameraUpdateMode.Updated)
                {
                    if (cameraSide == CameraSide.Left)
                    {
                        feedbackCamera.transform.position = (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * -Vector3.Cross(targetSphere.transform.position - feedbackAvatar_joint.position, Vector3.up).normalized;
                        feedbackCamera.transform.LookAt((targetSphere.transform.position + feedbackAvatar_joint.position) * 0.5f);
                    }
                    else
                    {
                        feedbackCamera.transform.position = (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.Cross(targetSphere.transform.position - feedbackAvatar_joint.position, Vector3.up).normalized;
                        feedbackCamera.transform.LookAt((targetSphere.transform.position + feedbackAvatar_joint.position) * 0.5f);

                    }
                }
                else
                {
                    Vector3 crossProduct = Vector3.Cross((hip.position + endPosition) - (hip.position + startPosition), Vector3.up).normalized;
                    if (cameraSide == CameraSide.Left)
                    {
                        // ToDo: Calculate static normal position
                        if (crossProduct.x - hip.position.x > 0)
                        {
                            feedbackCamera.transform.position = ((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f + camDistance * -crossProduct;
                        }
                        else
                        {
                            feedbackCamera.transform.position = ((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f + camDistance * crossProduct;
                        }
                        feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);                        
                    }
                    else
                    {
                        // ToDo: Calculate static normal position
                        if (crossProduct.x - hip.position.x < 0)
                        {
                            feedbackCamera.transform.position = ((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f + camDistance * -crossProduct;
                        }
                        else
                        {
                            feedbackCamera.transform.position = ((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f + camDistance * crossProduct;
                        }
                        feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);
                    }
                }
            }
        }

        if (cameraMotion == CameraMotionStates.Moving)
        {
            if (cameraPerspective == CameraPerspectives.Front)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.identity, Time.deltaTime * 1);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * -Vector3.forward, Time.deltaTime * 1);
            }

            if (cameraPerspective == CameraPerspectives.Behind)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, 180, 0), Time.deltaTime * 1);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.forward, Time.deltaTime * 1);
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Left)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, 90, 0), Time.deltaTime * 1);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.left, Time.deltaTime * 1);
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Right)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, -90, 0), Time.deltaTime * 1);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.right, Time.deltaTime * 1);
            }

            if (cameraPerspective == CameraPerspectives.Up)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(90, 0, 0), Time.deltaTime * 1);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.up, Time.deltaTime * 1);
            }

            if (cameraPerspective == CameraPerspectives.Normal)
            {
                if (camUpdateMode == CameraUpdateMode.Updated)
                {
                    if (cameraSide == CameraSide.Left)
                    {
                        feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * -Vector3.Cross(targetSphere.transform.position - feedbackAvatar_joint.position, Vector3.up).normalized, Time.deltaTime * 1);
                        //feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, , Time.deltaTime * 1);                      
                        feedbackCamera.transform.LookAt((targetSphere.transform.position + feedbackAvatar_joint.position) * 0.5f);
                    }
                    else
                    {
                        feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.Cross(targetSphere.transform.position - feedbackAvatar_joint.position, Vector3.up).normalized, Time.deltaTime * 1);
                        //feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, , Time.deltaTime * 1);
                        feedbackCamera.transform.LookAt((targetSphere.transform.position + feedbackAvatar_joint.position) * 0.5f);
                    }
                }

                else
                {
                    Vector3 crossProduct = Vector3.Cross((hip.position + endPosition) - (hip.position + startPosition), Vector3.up).normalized;
                    Debug.Log(crossProduct);
                    //Debug.DrawRay(hip.position + startPosition, crossProduct);
                    Debug.DrawRay(hip.position + startPosition, -crossProduct);
                    if (cameraSide == CameraSide.Left)
                    {
                        if (crossProduct.x - hip.position.x > 0)
                        {
                            feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * -crossProduct, Time.deltaTime);
                       
                        }

                        else
                        {
                            feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * crossProduct, Time.deltaTime);
                        }
                        // ToDo Calculate smoothed normal position
                        feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);
                    }
                    else
                    {
                        // ToDo Calculate smoothed normal position
                        if (crossProduct.x - hip.position.x > 0)
                        {
                            feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * crossProduct, Time.deltaTime);
                        }
                        else
                        {
                            feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * -crossProduct, Time.deltaTime);
                        }
                        feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);
                    }
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

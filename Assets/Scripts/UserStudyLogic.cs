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

/// <summary>
/// Defines start and target positions and a sequence number for the user study.
/// </summary>
public class PositionSet
{
    public PositionSet(Vector3 start, Vector3 end, uint trialCode)
    {
        this.StartPosition = start;
        this.EndPosition = end;
        this.TrialCode = trialCode;
    }

    /// <summary>
    /// Flips the x component of the vector so that left-handed people don't complain when
    /// doing the user study.
    /// </summary>
    public void FlipHandedness()
    {
        this.StartPosition = new Vector3(StartPosition.x * -1, StartPosition.y, StartPosition.z);
        this.EndPosition = new Vector3(EndPosition.x * -1, StartPosition.y, StartPosition.z);
    }

    public Vector3 StartPosition { get; set; }
    public Vector3 EndPosition { get; set; }
    public uint TrialCode { get; set; }
}

public class UserStudyLogic : MonoBehaviour
{
    //these are all defined positions for now. we should build sets out of these.
    private PositionSet[] targetPos = { new PositionSet(new Vector3(0.75f, 0.5f, -0.2f), new Vector3(0.5f, 0.75f, -0.3f), 0), 
                                        new PositionSet(new Vector3(0.5f, 0.3f, -0.4f), new Vector3(0.6f, 0.6f, -0.1f), 1), 
                                        new PositionSet(new Vector3(0.4f, 0.8f, 0), new Vector3(0.5f, 0.3f, -0.3f), 2),
                                        new PositionSet(new Vector3(0.6f, 0.2f, -0.5f), new Vector3(0.2f, 0.8f, -0.1f), 3),
                                        new PositionSet(new Vector3(0.1f, 0.3f, -0.75f), new Vector3(0.2f, 0.8f, -0.1f), 4)
                                      };
    private List<PositionSet> targetPositions = new List<PositionSet>();
    private Vector3 icoSphereOffset = new Vector3(0.25f, 0.25f, -0.45f);
    private float icoSphereScale = 0.125f;
    private float targetSphereHugeScale;
    private float targetSphereSmallScale = 0.025f;
    private Renderer targetSphereRenderer;
    private Color targetSphereStartColor;

    public static UserStudyLogic instance;
    public float camDistance = 1.5f;
    public GameObject cameraFeedbackPrefab;
    public GameObject targetSpherePrefab;
    public Transform leftShoulder, rightShoulder, hip, leftHand, rightHand;
    public AudioClip startSound, endSound;
    private AudioSource audioSource;
    private CameraFeedback cameraFeedback;
    private Camera feedbackCamera;
    private Vector3 camStartPos;
    private Quaternion camStartOrientation;
    private TargetSphere targetSphere;
    private CameraFeedbackMode camFeedbackMode;
    private Transform feedbackAvatar_joint;
    //private bool initialized;
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
    private float startTime;
    private float journeyTime;
    private float camHeightOffset;
    public float cameraSpeed;
    public float exercisePrecision;
    public bool snapping;
    private bool camMotionComplete = false;
    // Object that has an attached MovementRecorder
    public GameObject userStudyObject;

    // Timer for random shuffle
    private System.DateTime dTime;
    private float dayTime;

    private void InitTargetPositions(Handedness handedness)
    {
        List<Vector3> pos = BuildIcoSphereVertices();
        targetPositions.Clear();

        Vector3 icoSOffset;

        // Flip to the left side
        if (handedness == Handedness.LeftHanded)
        {
            icoSOffset = new Vector3(-icoSphereOffset.x, icoSphereOffset.y, icoSphereOffset.z);
        }
        else
        {
            icoSOffset = icoSphereOffset;
        }

        //First Direction: In->Out
        for (int i = 0; i < pos.Count; i++)
        {
            targetPositions.Add(new PositionSet(icoSOffset - pos[i] * icoSphereScale, icoSOffset + pos[i] * icoSphereScale, (uint)i));
        }
        //Second Direction: Out->In
        for (int i = 0; i < pos.Count; i++)
        {
            targetPositions.Add(new PositionSet(icoSOffset + pos[i] * icoSphereScale, icoSOffset - pos[i] * icoSphereScale, (uint)(i + pos.Count)));
        }

        //Applying to targetPos array        
        targetPos = new PositionSet[pos.Count * 2];
        for (int i = 0; i < targetPositions.Count; i++)
        {
            targetPos[i] = targetPositions[i];
        }
    }

    void Awake()
    {
        instance = this;      

        // Calculating random seed based on exact daytime (up to seconds)
        dTime = System.DateTime.Now;
        dayTime = dTime.Hour * 360 + dTime.Minute * 60 + dTime.Second;
        Random.seed = (int) dayTime;
    }


    void Start()
    {
        journeyTime = 5 * 150/cameraSpeed;
        SpawnUserStudyComponents();
        feedbackCamera.transform.position = hip.position - Vector3.forward * camDistance;
        camStartPos = feedbackCamera.transform.position;
        camStartOrientation = feedbackCamera.transform.rotation;
        camHeightOffset = feedbackCamera.transform.position.y - hip.position.y;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (camMotion)
        {
            UpdateCameraPosition();
        }
        else
        {
            feedbackCamera.transform.position = hip.position - Vector3.forward * camDistance;
        }
    }

    private void SpawnUserStudyComponents()
    {
        feedbackCamera = Camera.main;
        // Spawn and init camerafeedback
        cameraFeedback = (Instantiate(cameraFeedbackPrefab, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<CameraFeedback>();
        cameraFeedback.gameObject.SetActive(false);
        targetSphere = (Instantiate(targetSpherePrefab, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<TargetSphere>();
        targetSphereRenderer = targetSphere.transform.GetChild(0).GetComponent<Renderer>();
        targetSphereStartColor = targetSphereRenderer.material.color;
        targetSphereHugeScale = targetSphere.transform.GetChild(0).localScale.x;
        targetSphere.gameObject.SetActive(false);
    }

    public void InitNewUserStudy(CameraFeedbackMode feedbackType, Handedness handedness, CameraPerspectives camPerspective, CameraMotionStates camMotion,float precision, UserStudyUI userStudyUI, uint numTrials, bool coloring, bool scaling)
    {
        this.userStudyUI = userStudyUI;
        cameraPerspective = camPerspective;
        cameraMotion = camMotion;
        camFeedbackMode = feedbackType;
        this.numTrials = numTrials;
        this.handedness = handedness;
        this.exercisePrecision = precision;
        cameraFeedback.spriteScaling = scaling;
        cameraFeedback.spriteColoring = coloring;
        leftHand.GetComponent<SphereCollider>().radius = 1 / exercisePrecision * 0.5f;
        rightHand.GetComponent<SphereCollider>().radius = 1 / exercisePrecision * 0.5f;
        if (handedness == Handedness.LeftHanded)
        {
            feedbackAvatar_joint = rightHand;
        }
        else
        {
            feedbackAvatar_joint = leftHand;
        }

        InitTargetPositions(handedness);        

        if (this.numTrials > targetPos.Length)
        {
            this.numTrials = (uint) targetPos.Length;
        }

        if (this.numTrials <= 0)
        {
            this.numTrials = 0;
            trialCounter = 0;
            userStudyUI.gameObject.SetActive(true);
        }
        else
        {
            ShuffleArray<PositionSet>(targetPos);

            InitNewTrial();
        }

        //initialized = true;
    }

    private void InitNewTrial()
    {
        this.startPosition = targetPos[trialCounter].StartPosition;
        this.endPosition = targetPos[trialCounter].EndPosition;
        targetSphere.gameObject.SetActive(true);
        targetSphere.transform.GetChild(0).localScale = new Vector3(targetSphereHugeScale, targetSphereHugeScale, targetSphereHugeScale);
        targetSphereRenderer.material.color = targetSphereStartColor;
        targetSphere.InitTargetSphere(targetPos[trialCounter], handedness, hip);
    }

    public void StartTrial()
    {
        cameraFeedback.initialized = true;
        cameraFeedback.gameObject.SetActive(true);
        startTime = Time.time;
        camMotion = true;
        snapping = false;
        targetSphere.transform.GetChild(0).localScale = new Vector3(targetSphereSmallScale, targetSphereSmallScale, targetSphereSmallScale);
        targetSphereRenderer.material.color = Color.red;
     
        
        if (handedness == Handedness.LeftHanded)
        {
            cameraFeedback.InitCorrectionCamera(hip, rightHand, targetSphere.transform.position - hip.position, targetSphere.gameObject, camFeedbackMode, startPosition);
            feedbackAvatar_joint = rightHand;
        }
        else
        {
            cameraFeedback.InitCorrectionCamera(hip, leftHand, targetSphere.transform.position - hip.position, targetSphere.gameObject, camFeedbackMode, startPosition);
            feedbackAvatar_joint = leftHand;
        }

        if (cameraMotion == CameraMotionStates.Jumping)
        {
            audioSource.clip = startSound;
            audioSource.Play();

            ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetCamera(cameraPerspective));

            ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(targetPos[trialCounter].TrialCode));

            ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.StartTrial(Time.time));
        }
        else
            camMotionComplete = false;
        trialCounter++;
        //Debug.Log(trialCounter + " TrialCounter" + numTrials + " numTrials");
    }

    public void EndTrial(GameObject endEffector)
    {
        audioSource.Stop();
        audioSource.clip = endSound;
        audioSource.Play();
        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.EndTrial(Time.time, endEffector.transform.position));
        StartCoroutine(ExerciseDelay());
        //camMotionComplete = false;
    }

    IEnumerator ExerciseDelay()
    {
        yield return new WaitForSeconds(1.0f);
        cameraFeedback.initialized = false; 
        cameraFeedback.DisableSubObjects();
        yield return new WaitForSeconds(0.5f);               
        cameraFeedback.gameObject.SetActive(false);
        camMotion = false;
        feedbackCamera.transform.position = camStartPos;
        feedbackCamera.transform.rotation = camStartOrientation;

        if (trialCounter < numTrials)
        {
            InitNewTrial();
        }
        else
        {
            trialCounter = 0;
            userStudyUI.gameObject.SetActive(true);
            //initialized = false;
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

    private List<Vector3> BuildIcoSphereVertices()
    {
        List<Vector3> vertexList = new List<Vector3>();

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        vertexList.Add(new Vector3(-1, t, 0));
        vertexList.Add(new Vector3(1, t, 0));
        vertexList.Add(new Vector3(-1, -t, 0));
        vertexList.Add(new Vector3(1, -t, 0));

        vertexList.Add(new Vector3(0, -1, t));
        vertexList.Add(new Vector3(0, 1, t));
        vertexList.Add(new Vector3(0, -1, -t));
        vertexList.Add(new Vector3(0, 1, -t));

        vertexList.Add(new Vector3(t, 0, -1));
        vertexList.Add(new Vector3(t, 0, 1));
        vertexList.Add(new Vector3(-t, 0, -1));
        vertexList.Add(new Vector3(-t, 0, 1));

        return vertexList;
    }

    void UpdateCameraPosition()
    {
        // Calculate vector of arrow
        Vector3 arrowVector = targetSphere.transform.position - feedbackAvatar_joint.position;
        arrowVector = arrowVector.normalized;

        // Assign handedness
        if (cameraPerspective == CameraPerspectives.Side || cameraPerspective == CameraPerspectives.Normal)
        {
            if (handedness == Handedness.LeftHanded)
            {
                cameraSide = CameraSide.Left;
            }
            else
            {
                cameraSide = CameraSide.Right;
            }
        }

        // Directly jumping to desired positions and perspectives
        if (cameraMotion == CameraMotionStates.Jumping)
        {
            if (cameraPerspective == CameraPerspectives.Front)
            {
                feedbackCamera.transform.rotation = Quaternion.identity;
                feedbackCamera.transform.position = hip.position + camDistance * -Vector3.forward;
            }

            if (cameraPerspective == CameraPerspectives.Behind)
            {
                feedbackCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
                feedbackCamera.transform.position = hip.position + camDistance * Vector3.forward;
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Left)
            {
                feedbackCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
                feedbackCamera.transform.position = hip.position +  camDistance * Vector3.left;
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Right)
            {
                feedbackCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
                feedbackCamera.transform.position = hip.position + camDistance * Vector3.right;
            }

            if (cameraPerspective == CameraPerspectives.Up)
            {
                feedbackCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
                feedbackCamera.transform.position = hip.position + camDistance * Vector3.up;
            }

            if (cameraPerspective == CameraPerspectives.Normal)
            {
                // Continiously updating normal position
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
                // Initial normal position
                else
                {
                    Vector3 crossProduct = Vector3.Cross((hip.position + endPosition) - (hip.position + startPosition), Vector3.up).normalized;

                    if (Mathf.Abs(crossProduct.z) > Mathf.Abs(crossProduct.x) && (hip.position + camDistance * crossProduct).z < hip.position.z)
                    {
                        crossProduct *= -1;
                    }

                    if (cameraSide == CameraSide.Left)
                    {
                        // Calculate static normal position
                        if (crossProduct.x - hip.position.x > 0)
                        {
                            feedbackCamera.transform.position = hip.position + camDistance * -crossProduct;
                        }
                        else
                        {
                            feedbackCamera.transform.position = hip.position + camDistance * -crossProduct;
                        }
                        //feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);
                        feedbackCamera.transform.LookAt(hip.position + new Vector3(0,camHeightOffset,0));
                    }
                    else
                    {
                        // Calculate static normal position
                        if (crossProduct.x - hip.position.x < 0)
                        {
                            feedbackCamera.transform.position = hip.position + camDistance * -crossProduct;
                        }
                        else
                        {
                            feedbackCamera.transform.position = hip.position + camDistance * -crossProduct;
                        }
                        //feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);
                        feedbackCamera.transform.LookAt(hip.position + new Vector3(0, camHeightOffset, 0));
                    }
                }
            }
        }

        // Smoothly moving to target position and orientation
        if (cameraMotion == CameraMotionStates.Moving)
        {
            float fracComplete = (Time.time - startTime) / journeyTime;

            if (cameraPerspective == CameraPerspectives.Front)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.identity, fracComplete);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + camDistance * -Vector3.forward, fracComplete);
            }

            if (cameraPerspective == CameraPerspectives.Behind)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, 180, 0), fracComplete);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + camDistance * Vector3.forward, fracComplete);
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Left)
            {
                if (Vector3.Angle(feedbackCamera.transform.forward, Vector3.right) > 30)
                {
                    feedbackCamera.transform.RotateAround(hip.position, Vector3.up, cameraSpeed * Time.deltaTime);
                    feedbackCamera.transform.position = feedbackCamera.transform.position + feedbackCamera.transform.forward * (Vector3.Distance(hip.position, feedbackCamera.transform.position) - camDistance);
                }
                else
                {
                    feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, 90, 0), fracComplete);
                    feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + camDistance * Vector3.left, fracComplete);
                    if (Vector3.Angle(feedbackCamera.transform.forward, Vector3.right) < 1.0f && !camMotionComplete)
                    {
                        camMotionComplete = true;
                        audioSource.clip = startSound;
                        audioSource.Play();

                        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetCamera(cameraPerspective));

                        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(targetPos[trialCounter].TrialCode));

                        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.StartTrial(Time.time));

                    }
                }
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Right)
            {
                if (Vector3.Angle(feedbackCamera.transform.forward, Vector3.left) > 30)
                {
                    feedbackCamera.transform.RotateAround(hip.position, Vector3.up, -cameraSpeed * Time.deltaTime);
                    feedbackCamera.transform.position = feedbackCamera.transform.position + feedbackCamera.transform.forward * (Vector3.Distance(hip.position, feedbackCamera.transform.position) - camDistance);
                }
                else
                {
                    feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, -90, 0), fracComplete);
                    feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + camDistance * Vector3.right, fracComplete);
                    if (Vector3.Angle(feedbackCamera.transform.forward, Vector3.left) < 1.0f && !camMotionComplete)
                    {
                        camMotionComplete = true;
                        audioSource.clip = startSound;
                        audioSource.Play();

                        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetCamera(cameraPerspective));

                        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(targetPos[trialCounter].TrialCode));

                        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.StartTrial(Time.time));

                    }
                }
            }

            if (cameraPerspective == CameraPerspectives.Up)
            {
                if (Vector3.Angle(feedbackCamera.transform.forward, Vector3.down) > 30)
                {
                    feedbackCamera.transform.RotateAround(hip.position, Vector3.right, cameraSpeed * Time.deltaTime);
                    feedbackCamera.transform.position = feedbackCamera.transform.position + feedbackCamera.transform.forward * (Vector3.Distance(hip.position, feedbackCamera.transform.position) - camDistance);
                }
                else
                {
                    feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(90, 0, 0), fracComplete);
                    feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + camDistance * Vector3.up, fracComplete);
                    if (Vector3.Angle(feedbackCamera.transform.forward, Vector3.down) < 1.0f && !camMotionComplete)
                    {
                        camMotionComplete = true;
                        audioSource.clip = startSound;
                        audioSource.Play();

                        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetCamera(cameraPerspective));

                        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(targetPos[trialCounter].TrialCode));

                        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.StartTrial(Time.time));

                    }
                }
            }

            if (cameraPerspective == CameraPerspectives.Normal)
            {
                // Continiously updating normal position: even during an exercise
                if (camUpdateMode == CameraUpdateMode.Updated)
                {
                    if (cameraSide == CameraSide.Left)
                    {
                        feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * -Vector3.Cross(targetSphere.transform.position - feedbackAvatar_joint.position, Vector3.up).normalized, fracComplete);
                        //feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, , Time.deltaTime * 1);                      
                        feedbackCamera.transform.LookAt((targetSphere.transform.position + feedbackAvatar_joint.position) * 0.5f);
                    }
                    else
                    {
                        feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, (feedbackAvatar_joint.position + targetSphere.transform.position) * 0.5f + camDistance * Vector3.Cross(targetSphere.transform.position - feedbackAvatar_joint.position, Vector3.up).normalized, fracComplete);
                        //feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, , Time.deltaTime * 1);
                        feedbackCamera.transform.LookAt((targetSphere.transform.position + feedbackAvatar_joint.position) * 0.5f);
                    }
                }

                // Pre computing target position: position will not be updated during the exercise
                else
                {
                    Vector3 crossProduct = Vector3.Cross((hip.position + endPosition) - (hip.position + startPosition), Vector3.up).normalized;

                    if (Mathf.Abs(crossProduct.z) > Mathf.Abs(crossProduct.x) && (hip.position + camDistance * crossProduct).z < hip.position.z)
                    {
                        crossProduct *= -1;
                    }

                    if (cameraSide == CameraSide.Left)
                    {
                        if (crossProduct.x - hip.position.x > 0)
                        {
                            if (Vector3.Angle(new Vector3(feedbackCamera.transform.position.x, 0 ,feedbackCamera.transform.position.z), new Vector3((hip.position + camDistance * -crossProduct).x,0,(hip.position + camDistance * -crossProduct).z)) > 30)
                            {
                                feedbackCamera.transform.RotateAround(hip.position, Vector3.up, cameraSpeed * Time.deltaTime);
                                feedbackCamera.transform.position = feedbackCamera.transform.position + feedbackCamera.transform.forward * (Vector3.Distance(hip.position, feedbackCamera.transform.position) - camDistance);
                            }
                            else
                            {
                                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + camDistance * -crossProduct, fracComplete);
                                //feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);
                                feedbackCamera.transform.LookAt(hip.position + new Vector3(0, camHeightOffset, 0));
                                if (Vector3.Angle(feedbackCamera.transform.forward, crossProduct) < 1.0f && !camMotionComplete)
                                {
                                    camMotionComplete = true;
                                    audioSource.clip = startSound;
                                    audioSource.Play();

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetCamera(cameraPerspective));

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(targetPos[trialCounter].TrialCode));

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.StartTrial(Time.time));

                                }
                            }
                        }
                        // Double Check for steep angles to rotate around the other directiction
                        else
                        {
                            if (Vector3.Angle(new Vector3(feedbackCamera.transform.position.x, 0, feedbackCamera.transform.position.z), new Vector3((hip.position + camDistance * -crossProduct).x, 0, (hip.position + camDistance * -crossProduct).z)) > 30)
                            {
                                feedbackCamera.transform.RotateAround(hip.position, Vector3.up, -cameraSpeed * Time.deltaTime);
                                feedbackCamera.transform.position = feedbackCamera.transform.position + feedbackCamera.transform.forward * (Vector3.Distance(hip.position, feedbackCamera.transform.position) - camDistance);
                            }
                            else
                            {
                                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + camDistance * -crossProduct, fracComplete);
                                //feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);
                                feedbackCamera.transform.LookAt(hip.position + new Vector3(0, camHeightOffset, 0));
                                if (Vector3.Angle(feedbackCamera.transform.forward, crossProduct) < 1.0f && !camMotionComplete)
                                {
                                    camMotionComplete = true;
                                    audioSource.clip = startSound;
                                    audioSource.Play();

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetCamera(cameraPerspective));

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(targetPos[trialCounter].TrialCode));

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.StartTrial(Time.time));

                                }
                            }
                        }
                    }
                    else
                    {
                        // Double Check for steep angles to rotate around the other directiction
                        if (crossProduct.x - hip.position.x > 0)
                        {
                            if (Vector3.Angle(new Vector3(feedbackCamera.transform.position.x, 0, feedbackCamera.transform.position.z), new Vector3((hip.position + camDistance * -crossProduct).x, 0, (hip.position + camDistance * -crossProduct).z)) > 30)
                            {
                                feedbackCamera.transform.RotateAround(hip.position + (startPosition + endPosition) * 0.5f, Vector3.up, cameraSpeed * Time.deltaTime);
                                feedbackCamera.transform.position = feedbackCamera.transform.position + feedbackCamera.transform.forward * (Vector3.Distance(hip.position, feedbackCamera.transform.position) - camDistance);
                            }
                            else
                            {
                                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + camDistance * -crossProduct, fracComplete);
                                //feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);
                                feedbackCamera.transform.LookAt(hip.position + new Vector3(0, camHeightOffset, 0));
                                if (Vector3.Angle(feedbackCamera.transform.forward, crossProduct) < 1.0f && !camMotionComplete)
                                {
                                    camMotionComplete = true;
                                    audioSource.clip = startSound;
                                    audioSource.Play();

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetCamera(cameraPerspective));

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(targetPos[trialCounter].TrialCode));

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.StartTrial(Time.time));

                                }
                            }
                        }
                        
                        else
                        {
                            if (Vector3.Angle(new Vector3(feedbackCamera.transform.position.x, 0, feedbackCamera.transform.position.z), new Vector3((hip.position + camDistance * -crossProduct).x, 0, (hip.position  + camDistance * -crossProduct).z)) > 30)
                            {
                                feedbackCamera.transform.RotateAround(hip.position + (startPosition + endPosition) * 0.5f, Vector3.up, -cameraSpeed * Time.deltaTime);
                                feedbackCamera.transform.position = feedbackCamera.transform.position + feedbackCamera.transform.forward * (Vector3.Distance(hip.position, feedbackCamera.transform.position) - camDistance);
                            }
                            else
                            {
                                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + camDistance * -crossProduct, fracComplete);
                                //feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);
                                feedbackCamera.transform.LookAt(hip.position + new Vector3(0, camHeightOffset, 0));
                                if (Vector3.Angle(feedbackCamera.transform.forward, crossProduct) < 1.0f && !camMotionComplete)
                                {
                                    camMotionComplete = true;
                                    audioSource.clip = startSound;
                                    audioSource.Play();

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetCamera(cameraPerspective));

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(targetPos[trialCounter].TrialCode));

                                    ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.StartTrial(Time.time));

                                }
                            }
                        }                     
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

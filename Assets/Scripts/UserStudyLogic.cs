﻿using UnityEngine;
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
    private Vector3 icoSphereOffset = new Vector3(0.25f, 0.25f, -0.5f);
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
    private float journeyTime = 5;

    // Object that has an attached MovementRecorder
    public GameObject userStudyObject;

    private void InitTargetPositions(Handedness handedness)
    {
        List<Vector3> pos = BuildIcoSphereVertices();
        //First Direction: In->Out
        for (int i = 0; i < pos.Count; i++)
        {
            targetPositions.Add(new PositionSet(icoSphereOffset - pos[i] * icoSphereScale, icoSphereOffset +  pos[i] * icoSphereScale, (uint)i));
        }
        //Second Direction: Out->In
        for (int i = 0; i < pos.Count; i++)
        {
            targetPositions.Add(new PositionSet(icoSphereOffset + pos[i] * icoSphereScale, icoSphereOffset  - pos[i] * icoSphereScale, (uint)(i + pos.Count)));
        }

        //Applying to targetPos array
        targetPos = new PositionSet[pos.Count * 2];
        for (int i = 0; i < targetPositions.Count; i++)
        {
            targetPos[i] = targetPositions[i];
        }

        if (handedness == Handedness.LeftHanded)
        {
            foreach (var positions in targetPos)
            {
                positions.FlipHandedness();
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
        targetSphereRenderer = targetSphere.GetComponent<Renderer>();
        targetSphereStartColor = targetSphereRenderer.material.color;
        targetSphereHugeScale = targetSphere.transform.localScale.x;
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
            //ShuffleArray<PositionSet>(targetPos);

            InitNewTrial();
        }

        //initialized = true;
    }

    private void InitNewTrial()
    {
        this.startPosition = targetPos[trialCounter].StartPosition;
        this.endPosition = targetPos[trialCounter].EndPosition;

        targetSphere.gameObject.SetActive(true);
        targetSphere.transform.localScale = new Vector3(targetSphereHugeScale, targetSphereHugeScale, targetSphereHugeScale);
        targetSphereRenderer.material.color = targetSphereStartColor;
        targetSphere.InitTargetSphere(targetPos[trialCounter], handedness, hip);
    }

    public void StartTrial()
    {
        cameraFeedback.gameObject.SetActive(true);
        startTime = Time.time;
        camMotion = true;
        targetSphere.transform.localScale = new Vector3(targetSphereSmallScale, targetSphereSmallScale, targetSphereSmallScale);
        targetSphereRenderer.material.color = Color.red;
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

        ExecuteEvents.Execute<IUserStudyMessageTarget>(userStudyObject, null, (x, y) => x.SetTrial(targetPos[trialCounter].TrialCode));

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
                    if (cameraSide == CameraSide.Left)
                    {
                        // Calculate static normal position
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
                        // Calculate static normal position
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

        // Smoothly moving to target position and orientation
        if (cameraMotion == CameraMotionStates.Moving)
        {
            float fracComplete = (Time.time - startTime) / journeyTime;

            if (cameraPerspective == CameraPerspectives.Front)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.identity, fracComplete);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * -Vector3.forward, fracComplete);
            }

            if (cameraPerspective == CameraPerspectives.Behind)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, 180, 0), fracComplete);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.forward, fracComplete);
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Left)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, 90, 0), fracComplete);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.left, fracComplete);
            }

            if (cameraPerspective == CameraPerspectives.Side && cameraSide == CameraSide.Right)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(0, -90, 0), fracComplete);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.right, fracComplete);
            }

            if (cameraPerspective == CameraPerspectives.Up)
            {
                feedbackCamera.transform.rotation = Quaternion.Slerp(feedbackCamera.transform.rotation, Quaternion.Euler(90, 0, 0), fracComplete);
                feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * Vector3.up, fracComplete);
            }

            if (cameraPerspective == CameraPerspectives.Normal)
            {
                // Continiously updating normal position
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

                else
                {
                    Vector3 crossProduct = Vector3.Cross((hip.position + endPosition) - (hip.position + startPosition), Vector3.up).normalized;

                    if (cameraSide == CameraSide.Left)
                    {
                        if (crossProduct.x - hip.position.x > 0)
                        {
                            feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * -crossProduct, fracComplete);
                        }

                        else
                        {
                            feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * crossProduct, fracComplete);
                        }
                        // ToDo Calculate smoothed normal position
                        feedbackCamera.transform.LookAt(((hip.position + startPosition) + (hip.position + endPosition)) * 0.5f);
                    }
                    else
                    {
                        // ToDo Calculate smoothed normal position
                        if (crossProduct.x - hip.position.x > 0)
                        {
                            feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * crossProduct, fracComplete);
                        }
                        else
                        {
                            feedbackCamera.transform.position = Vector3.Slerp(feedbackCamera.transform.position, hip.position + (startPosition + endPosition) * 0.5f + camDistance * -crossProduct, fracComplete);
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

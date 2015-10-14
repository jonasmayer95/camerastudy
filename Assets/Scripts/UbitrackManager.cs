using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;


class UbitrackManager : MonoBehaviour
{
    private Matrix4x4 kinectToWorld = Matrix4x4.identity;
    private float sensorAngle;
    private float sensorHeight = 1.0f;
    //private List<AvatarController> avatarControllers = new List<AvatarController>();
    
    private const int kinectJointCount = 26;
    private UTBodyData bodyData = new UTBodyData(kinectJointCount);
    public static UbitrackManager instance;
    public InseilMeasurement measurement = new InseilMeasurement();

    public GameObject avatar = null;
    private InseilAvatarController avatarController;

    private PairSocket socket;
    private byte[] msgBytes;
    private bool recv;

    void Awake()
    {
        instance = this;

        if (avatar != null)
        {
            avatarController = avatar.GetComponent<InseilAvatarController>();
        }
    }

    void Start()
    {
        // Get and init avatar controller
        //MonoBehaviour[] monoScripts = FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];
        //foreach (MonoBehaviour monoScript in monoScripts)
        //{
        //    if (typeof(AvatarController).IsAssignableFrom(monoScript.GetType()))
        //    {
        //        AvatarController avatar = (AvatarController)monoScript;
        //        avatarControllers.Add(avatar);
        //    }
        //}

        Quaternion quatTiltAngle = Quaternion.Euler(-sensorAngle, 0.0f, 0.0f);
        kinectToWorld.SetTRS(new Vector3(0.0f, sensorHeight, 0.0f), quatTiltAngle, Vector3.one);
       
    }

    void Update()
    {
        //TODO: get rid of this check by making sure the socket exists before calling update for the first time
        if (socket != null)
        {
            recv = socket.TryReceiveFrameBytes(out msgBytes);

            if (recv)
            {
                //get data, convert it and update the avatar
                measurement.FromByteArray(msgBytes);
                GenerateBodyData(measurement);
            }
        }
        
    }

    public void SocketShutdown()
    {
        if (socket != null)
        {
            socket.Dispose();
            Debug.Log("UbitrackManager: disposed of inproc socket");
        }
    }

    /// <summary>
    /// Sets up a pair socket to get joint updates for the avatar.
    /// Needs to be called from outside as the context might not exist yet
    /// when the avatar is created.
    /// </summary>
    public void SetupSocket(string address)
    {
        if (ServerCommunication.Context != null)
        {
            socket = ServerCommunication.Context.CreatePairSocket();
            socket.Connect(address);
        }
        else
        {
            Debug.Log("servercommunication has not set up its netmqcontext yet");
        }
    }

    /// <summary>
    /// Returns a UTBodyData object that can be used for visualization of an avatar.
    /// </summary>
    /// <param name="skeleton"></param>
    public void GenerateBodyData(InseilMeasurement skeleton)
    {
        //update kinect to world matrix, but we need sensor height and angle for that
        Quaternion quatTiltAngle = Quaternion.Euler(-sensorAngle, 0.0f, 0.0f);
        kinectToWorld.SetTRS(new Vector3(0.0f, sensorHeight, 0.0f), quatTiltAngle, Vector3.one);

        //UTBodyData retVal = new UTBodyData(skeleton.data.Count);
        InitializeBodyData(ref bodyData, ref skeleton, ref kinectToWorld);

        CalculateJointDirections(ref bodyData);

        //calculate special directions and joint orientations
        CalculateSpecialDirections(ref bodyData);
        CalculateJointOrientations(ref bodyData);

        //update avatar
        //avatarControllers[0].UpdateInseilAvatar(ref bodyData);
        avatarController.UpdateInseilAvatar(ref bodyData);
    }

    public void InitializeBodyData(ref UTBodyData data, ref InseilMeasurement skeleton, ref Matrix4x4 kinectToWorld)
    {
        foreach (var joint in skeleton.data)
        {
            //get the index of our joint from the current message
            JointType type = GetJointType(joint.Key);
            if (type == JointType.Invalid)
                continue;

            if (type == JointType.FloorPlane)
            {
                Vector3 rot = new Vector3((float)joint.Value.r.x, (float)joint.Value.r.y, (float)joint.Value.r.z);
                Quaternion sensorRotDetected = Quaternion.FromToRotation(rot, Vector3.up);
                float sensorHgtDetected = (float)joint.Value.r.w;

                float angle = sensorRotDetected.eulerAngles.x;
                angle = angle > 180f ? (angle - 360f) : angle;
                sensorAngle = -angle;

                float height = sensorHgtDetected > 0f ? sensorHgtDetected : sensorHeight;
                sensorHeight = height;
            }

            //set its position and orientation
            UTJointData jointData = new UTJointData();
            jointData.jointType = type;
            jointData.kinectPos = new Vector3((float)joint.Value.p.x, (float)joint.Value.p.y, (float)joint.Value.p.z);
            jointData.position = kinectToWorld.MultiplyPoint3x4(jointData.kinectPos);

            //for some reason orientations from the sensor interface are not used, hence the 'deprecated' comment
            //they are calculated in CalculateJointOrients
            jointData.orientation = Quaternion.identity;
            //jointData.orientation = new Quaternion((float)joint.Value.rotation.x, (float)joint.Value.rotation.y, (float)joint.Value.rotation.z, (float)joint.Value.rotation.w);

            if (type == JointType.SpineBase)
            {
                data.position = jointData.position;
                data.orientation = jointData.orientation;
                //Debug.Log(string.Format("UbitrackManager: world position: {0}, kinect position: {1}\n", jointData.position, jointData.kinectPos));
            }

            //write it into UTBodyData's joints array
            data.joints[(int)type] = jointData;
        }
    }

    private void CalculateJointDirections(ref UTBodyData data)
    {
        //spinebase does not point anywhere
        data.joints[0].direction = Vector3.zero;

        for (int i = 1; i < data.joints.Length; ++i)
        {
            int parent = (int)GetParentJoint(data.joints[i].jointType);

            data.joints[i].direction = data.joints[i].position - data.joints[parent].position;
            data.joints[i].direction = new Vector3(-data.joints[i].direction.x, data.joints[i].direction.y, data.joints[i].direction.z);
        }
    }

    private void CalculateJointOrientations(ref UTBodyData bodyData)
    {
        int jointCount = bodyData.joints.Length;

        for (int j = 0; j < jointCount; j++)
        {
            int joint = j;

            UTJointData jointData = bodyData.joints[joint];

            //bool bJointValid = ignoreInferredJoints ? jointData.trackingState == KinectInterop.TrackingState.Tracked : jointData.trackingState != KinectInterop.TrackingState.NotTracked;

            //if (bJointValid)
            //{
            int nextJoint = (int)GetNextJoint((JointType)joint);
            if (nextJoint != joint && nextJoint >= 0 && nextJoint < jointCount)
            {
                UTJointData nextJointData = bodyData.joints[nextJoint];
                //bool bNextJointValid = ignoreInferredJoints ? nextJointData.trackingState == KinectInterop.TrackingState.Tracked : nextJointData.trackingState != KinectInterop.TrackingState.NotTracked;

                Vector3 baseDir = JointBaseDir[nextJoint];
                Vector3 jointDir = nextJointData.direction;
                jointDir = new Vector3(jointDir.x, jointDir.y, jointDir.z).normalized;

                Quaternion jointOrientNormal = jointData.normalRotation;
                //if (bNextJointValid)
                //{
                jointOrientNormal = Quaternion.FromToRotation(baseDir, jointDir);
                //}

                if ((joint == (int)JointType.ShoulderLeft) ||
                   (joint == (int)JointType.ShoulderRight))
                {
                    float angle = -bodyData.bodyFullAngle; //bodyfullangle is not used any longer, the assignment is commented out in kinectmanager
                    Vector3 axis = jointDir;
                    Quaternion armTurnRotation = Quaternion.AngleAxis(angle, axis);

                    jointData.normalRotation = armTurnRotation * jointOrientNormal;
                }
                else if ((joint == (int)JointType.ElbowLeft) ||
                        (joint == (int)JointType.WristLeft) ||
                        (joint == (int)JointType.HandLeft))
                {
                    if (joint == (int)JointType.WristLeft)
                    {
                        UTJointData handData = bodyData.joints[(int)JointType.HandLeft];
                        UTJointData handTipData = bodyData.joints[(int)JointType.HandTipLeft];

                        //if (handData.trackingState != KinectInterop.TrackingState.NotTracked &&
                        //   handTipData.trackingState != KinectInterop.TrackingState.NotTracked)
                        //{
                        jointDir = handData.direction + handTipData.direction;
                        jointDir = new Vector3(jointDir.x, jointDir.y, -jointDir.z).normalized;
                        //}
                    }

                    UTJointData shCenterData = bodyData.joints[(int)JointType.SpineShoulder];
                    if (/*shCenterData.trackingState != KinectInterop.TrackingState.NotTracked &&*/
                       jointDir != Vector3.zero && shCenterData.direction != Vector3.zero &&
                       Mathf.Abs(Vector3.Dot(jointDir, shCenterData.direction.normalized)) < 0.5f)
                    {
                        Vector3 spineDir = shCenterData.direction;
                        spineDir = new Vector3(spineDir.x, spineDir.y, -spineDir.z).normalized;

                        Vector3 fwdDir = Vector3.Cross(-jointDir, spineDir).normalized;
                        Vector3 upDir = Vector3.Cross(fwdDir, -jointDir).normalized;
                        jointOrientNormal = Quaternion.LookRotation(fwdDir, upDir);
                    }
                    else
                    {
                        jointOrientNormal = Quaternion.FromToRotation(baseDir, jointDir);
                    }

                    bool bRotated = false;
                    if (/*(allowedHandRotations == AllowedRotations.All) &&*/
                       (joint != (int)JointType.ElbowLeft))
                    {
                        UTJointData handData = bodyData.joints[(int)JointType.HandLeft];
                        UTJointData handTipData = bodyData.joints[(int)JointType.HandTipLeft];
                        UTJointData thumbData = bodyData.joints[(int)JointType.ThumbLeft];

                        //if (handData.trackingState != KinectInterop.TrackingState.NotTracked &&
                        //   handTipData.trackingState != KinectInterop.TrackingState.NotTracked &&
                        //   thumbData.trackingState != KinectInterop.TrackingState.NotTracked)
                        //{
                        Vector3 rightDir = -(handData.direction + handTipData.direction);
                        rightDir = new Vector3(rightDir.x, rightDir.y, -rightDir.z).normalized;

                        Vector3 fwdDir = thumbData.direction;
                        fwdDir = new Vector3(fwdDir.x, fwdDir.y, -fwdDir.z).normalized;

                        if (rightDir != Vector3.zero && fwdDir != Vector3.zero)
                        {
                            Vector3 upDir = Vector3.Cross(fwdDir, rightDir).normalized;
                            fwdDir = Vector3.Cross(rightDir, upDir).normalized;

                            jointData.normalRotation = Quaternion.LookRotation(fwdDir, upDir);
                            //bRotated = true;

                            // fix invalid wrist rotation
                            UTJointData elbowData = bodyData.joints[(int)JointType.ElbowLeft];
                            //if (elbowData.trackingState != KinectInterop.TrackingState.NotTracked)
                            //{
                            Quaternion quatLocalRot = Quaternion.Inverse(elbowData.normalRotation) * jointData.normalRotation;
                            float angleY = quatLocalRot.eulerAngles.y;

                            if (angleY >= 90f && angleY < 270f && bodyData.leftHandOrientation != Quaternion.identity)
                            {
                                jointData.normalRotation = bodyData.leftHandOrientation;
                            }

                            bodyData.leftHandOrientation = jointData.normalRotation;
                            //}
                        }
                        //}

                        bRotated = true;
                    }

                    if (!bRotated)
                    {
                        float angle = -bodyData.bodyFullAngle;
                        Vector3 axis = jointDir;
                        Quaternion armTurnRotation = Quaternion.AngleAxis(angle, axis);

                        jointData.normalRotation = (/*allowedHandRotations != AllowedRotations.None ||*/ joint == (int)JointType.ElbowLeft) ?
                            armTurnRotation * jointOrientNormal : armTurnRotation;
                    }
                }
                else if ((joint == (int)JointType.ElbowRight) ||
                        (joint == (int)JointType.WristRight) ||
                        (joint == (int)JointType.HandRight))
                {
                    if (joint == (int)JointType.WristRight)
                    {
                        UTJointData handData = bodyData.joints[(int)JointType.HandRight];
                        UTJointData handTipData = bodyData.joints[(int)JointType.HandTipRight];

                        //if (handData.trackingState != KinectInterop.TrackingState.NotTracked &&
                        //   handTipData.trackingState != KinectInterop.TrackingState.NotTracked)
                        //{
                        jointDir = handData.direction + handTipData.direction;
                        jointDir = new Vector3(jointDir.x, jointDir.y, -jointDir.z).normalized;
                        //}
                    }

                    UTJointData shCenterData = bodyData.joints[(int)JointType.SpineShoulder];
                    if (/*shCenterData.trackingState != KinectInterop.TrackingState.NotTracked &&*/
                       jointDir != Vector3.zero && shCenterData.direction != Vector3.zero &&
                       Mathf.Abs(Vector3.Dot(jointDir, shCenterData.direction.normalized)) < 0.5f)
                    {
                        Vector3 spineDir = shCenterData.direction;
                        spineDir = new Vector3(spineDir.x, spineDir.y, -spineDir.z).normalized;

                        Vector3 fwdDir = Vector3.Cross(jointDir, spineDir).normalized;
                        Vector3 upDir = Vector3.Cross(fwdDir, jointDir).normalized;
                        jointOrientNormal = Quaternion.LookRotation(fwdDir, upDir);
                    }
                    else
                    {
                        jointOrientNormal = Quaternion.FromToRotation(baseDir, jointDir);
                    }

                    bool bRotated = false;
                    if (/*(allowedHandRotations == AllowedRotations.All) &&*/
                       (joint != (int)JointType.ElbowRight))
                    {
                        UTJointData handData = bodyData.joints[(int)JointType.HandRight];
                        UTJointData handTipData = bodyData.joints[(int)JointType.HandTipRight];
                        UTJointData thumbData = bodyData.joints[(int)JointType.ThumbRight];

                        //if (handData.trackingState != KinectInterop.TrackingState.NotTracked &&
                        //   handTipData.trackingState != KinectInterop.TrackingState.NotTracked &&
                        //   thumbData.trackingState != KinectInterop.TrackingState.NotTracked)
                        //{
                        Vector3 rightDir = handData.direction + handTipData.direction;
                        rightDir = new Vector3(rightDir.x, rightDir.y, -rightDir.z).normalized;

                        Vector3 fwdDir = thumbData.direction;
                        fwdDir = new Vector3(fwdDir.x, fwdDir.y, -fwdDir.z).normalized;

                        if (rightDir != Vector3.zero && fwdDir != Vector3.zero)
                        {
                            Vector3 upDir = Vector3.Cross(fwdDir, rightDir).normalized;
                            fwdDir = Vector3.Cross(rightDir, upDir).normalized;

                            jointData.normalRotation = Quaternion.LookRotation(fwdDir, upDir);
                            //bRotated = true;

                            // fix invalid wrist rotation
                            UTJointData elbowData = bodyData.joints[(int)JointType.ElbowRight];
                            //if (elbowData.trackingState != KinectInterop.TrackingState.NotTracked)
                            //{
                            Quaternion quatLocalRot = Quaternion.Inverse(elbowData.normalRotation) * jointData.normalRotation;
                            float angleY = quatLocalRot.eulerAngles.y;

                            if (angleY >= 90f && angleY < 270f && bodyData.rightHandOrientation != Quaternion.identity)
                            {
                                jointData.normalRotation = bodyData.rightHandOrientation;
                            }

                            bodyData.rightHandOrientation = jointData.normalRotation;
                            //}
                        }
                        //}

                        bRotated = true;
                    }

                    if (!bRotated)
                    {
                        float angle = -bodyData.bodyFullAngle;
                        Vector3 axis = jointDir;
                        Quaternion armTurnRotation = Quaternion.AngleAxis(angle, axis);

                        jointData.normalRotation = (/*allowedHandRotations != AllowedRotations.None ||*/ joint == (int)JointType.ElbowRight) ?
                            armTurnRotation * jointOrientNormal : armTurnRotation;
                    }
                }
                else
                {
                    jointData.normalRotation = jointOrientNormal;
                }

                if ((joint == (int)JointType.SpineMid) ||
                   (joint == (int)JointType.SpineShoulder) ||
                   (joint == (int)JointType.Neck))
                {
                    Vector3 baseDir2 = Vector3.right;
                    Vector3 jointDir2 = Vector3.Lerp(bodyData.shouldersDirection, -bodyData.shouldersDirection, bodyData.turnAroundFactor);
                    jointDir2.z = -jointDir2.z;

                    jointData.normalRotation *= Quaternion.FromToRotation(baseDir2, jointDir2);
                }
                else if ((joint == (int)JointType.SpineBase) ||
                   (joint == (int)JointType.HipLeft) || (joint == (int)JointType.HipRight) ||
                   (joint == (int)JointType.KneeLeft) || (joint == (int)JointType.KneeRight) ||
                   (joint == (int)JointType.AnkleLeft) || (joint == (int)JointType.AnkleRight))
                {
                    Vector3 baseDir2 = Vector3.right;
                    Vector3 jointDir2 = Vector3.Lerp(bodyData.hipsDirection, -bodyData.hipsDirection, bodyData.turnAroundFactor);
                    jointDir2.z = -jointDir2.z;

                    jointData.normalRotation *= Quaternion.FromToRotation(baseDir2, jointDir2);
                }

                Vector3 mirroredAngles = jointData.normalRotation.eulerAngles;
                mirroredAngles.y = -mirroredAngles.y;
                mirroredAngles.z = -mirroredAngles.z;

                jointData.mirroredRotation = Quaternion.Euler(mirroredAngles);            
            }
            else
            {
                // get the orientation of the parent joint
                int prevJoint = (int)GetParentJoint((JointType)joint);
                if (prevJoint != joint && prevJoint >= 0 && prevJoint < jointCount)
                {
                    jointData.normalRotation = bodyData.joints[prevJoint].normalRotation;
                    jointData.mirroredRotation = bodyData.joints[prevJoint].mirroredRotation;
                }
                else
                {
                    jointData.normalRotation = Quaternion.identity;
                    jointData.mirroredRotation = Quaternion.identity;
                }
            }
            //}

            bodyData.joints[joint] = jointData;

            if (joint == (int)JointType.SpineBase)
            {
                bodyData.normalRotation = jointData.normalRotation;
                bodyData.mirroredRotation = jointData.mirroredRotation;
            }
        }
    }

    public void CalculateSpecialDirections(ref UTBodyData bodyData)
    {
        // calculate special directions
        Vector3 posRHip = bodyData.joints[(int)JointType.HipRight].position;
        Vector3 posLHip = bodyData.joints[(int)JointType.HipLeft].position;

        bodyData.hipsDirection = posRHip - posLHip;
        bodyData.hipsDirection -= Vector3.Project(bodyData.hipsDirection, Vector3.up);
 
        Vector3 posRShoulder = bodyData.joints[(int)JointType.ShoulderRight].position;
        Vector3 posLShoulder = bodyData.joints[(int)JointType.ShoulderLeft].position;

        bodyData.shouldersDirection = posRShoulder - posLShoulder;
        bodyData.shouldersDirection -= Vector3.Project(bodyData.shouldersDirection, Vector3.up);

        Vector3 shouldersDir = bodyData.shouldersDirection;
        shouldersDir.z = -shouldersDir.z;

        Quaternion turnRot = Quaternion.FromToRotation(Vector3.right, shouldersDir);
        bodyData.bodyTurnAngle = turnRot.eulerAngles.y;

        //TODO: ankle directions are missing, look into ProcessBodyFrameData()
    }

    public JointType GetNextJoint(JointType joint)
    {
        switch (joint)
        {
            case JointType.SpineBase:
                return JointType.SpineMid;
            case JointType.SpineMid:
                return JointType.SpineShoulder;
            case JointType.SpineShoulder:
                return JointType.Neck;
            case JointType.Neck:
                return JointType.Head;

            case JointType.ShoulderLeft:
                return JointType.ElbowLeft;
            case JointType.ElbowLeft:
                return JointType.WristLeft;
            case JointType.WristLeft:
                return JointType.HandLeft;
            case JointType.HandLeft:
                return JointType.HandTipLeft;

            case JointType.ShoulderRight:
                return JointType.ElbowRight;
            case JointType.ElbowRight:
                return JointType.WristRight;
            case JointType.WristRight:
                return JointType.HandRight;
            case JointType.HandRight:
                return JointType.HandTipRight;

            case JointType.HipLeft:
                return JointType.KneeLeft;
            case JointType.KneeLeft:
                return JointType.AnkleLeft;
            case JointType.AnkleLeft:
                return JointType.FootLeft;

            case JointType.HipRight:
                return JointType.KneeRight;
            case JointType.KneeRight:
                return JointType.AnkleRight;
            case JointType.AnkleRight:
                return JointType.FootRight;
        }

        return joint;  // in case of end joint - Head, HandTipLeft, HandTipRight, FootLeft, FootRight
    }

    /// <summary>
    /// Gets the JointType for the corresponding name.
    /// </summary>
    /// <param name="name">Joint name in lowercase letters.</param>
    /// <returns></returns>
    public JointType GetJointType(string name)
    {
        switch (name)
        {
            case "spinebase":
                return JointType.SpineBase;
            case "spinemid":
                return JointType.SpineMid;
            case "neck":
                return JointType.Neck;
            case "head":
                return JointType.Head;
            case "shoulderleft":
                return JointType.ShoulderLeft;
            case "elbowleft":
                return JointType.ElbowLeft;
            case "wristleft":
                return JointType.WristLeft;
            case "handleft":
                return JointType.HandLeft;
            case "shoulderright":
                return JointType.ShoulderRight;
            case "elbowright":
                return JointType.ElbowRight;
            case "wristright":
                return JointType.WristRight;
            case "handright":
                return JointType.HandRight;
            case "hipleft":
                return JointType.HipLeft;
            case "kneeleft":
                return JointType.KneeLeft;
            case "ankleleft":
                return JointType.AnkleLeft;
            case "footleft":
                return JointType.FootLeft;
            case "hipright":
                return JointType.HipRight;
            case "kneeright":
                return JointType.KneeRight;
            case "ankleright":
                return JointType.AnkleRight;
            case "footright":
                return JointType.FootRight;
            case "spineshoulder":
                return JointType.SpineShoulder;
            case "handtipleft":
                return JointType.HandTipLeft;
            case "thumbleft":
                return JointType.ThumbLeft;
            case "handtipright":
                return JointType.HandTipRight;
            case "thumbright":
                return JointType.ThumbRight;
            case "floorplane":
                return JointType.FloorPlane;
            default:
//                Debug.Log(name);
                return JointType.Invalid;
        }
    }

    public string GetJointName(JointType joint)
    {
        switch (joint)
        {
            case JointType.SpineBase:
                return "spinebase";
            case JointType.SpineMid:
                return "spinemid";
            case JointType.Neck:
                return "neck";
            case JointType.Head:
                return "head";
            case JointType.ShoulderLeft:
                return "shoulderleft";
            case JointType.ElbowLeft:
                return "elbowleft";
            case JointType.WristLeft:
                return "wristleft";
            case JointType.HandLeft:
                return "handleft";
            case JointType.ShoulderRight:
                return "shoulderright";
            case JointType.ElbowRight:
                return "elbowright";
            case JointType.WristRight:
                return "wristright";
            case JointType.HandRight:
                return "handright";
            case JointType.HipLeft:
                return "hipleft";
            case JointType.KneeLeft:
                return "kneeleft";
            case JointType.AnkleLeft:
                return "ankleleft";
            case JointType.FootLeft:
                return "footleft";
            case JointType.HipRight:
                return "hipright";
            case JointType.KneeRight:
                return "kneeright";
            case JointType.AnkleRight:
                return "ankleright";
            case JointType.FootRight:
                return "footright";
            case JointType.SpineShoulder:
                return "spineshoulder";
            case JointType.HandTipLeft:
                return "handtipleft";
            case JointType.ThumbLeft:
                return "thumbleft";
            case JointType.HandTipRight:
                return "handtipright";
            case JointType.ThumbRight:
                return "thumbright";
            case JointType.FloorPlane:
                return "floorplane";
            default:
                return "invalid";
        }
    }

    /// <summary>
    /// Returns the parent of a given joint.
    /// </summary>
    /// <param name="joint">The joint whose parent we want to retrieve.</param>
    /// <returns></returns>
    public JointType GetParentJoint(JointType joint)
    {
        switch (joint)
        {
            case JointType.SpineBase:
                return JointType.SpineBase;

            case JointType.Neck:
                return JointType.SpineShoulder;

            case JointType.SpineShoulder:
                return JointType.SpineMid;

            case JointType.ShoulderLeft:
            case JointType.ShoulderRight:
                return JointType.SpineShoulder;

            case JointType.HipLeft:
            case JointType.HipRight:
                return JointType.SpineBase;

            case JointType.HandTipLeft:
                return JointType.HandLeft;

            case JointType.ThumbLeft:
                return JointType.WristLeft;

            case JointType.HandTipRight:
                return JointType.HandRight;

            case JointType.ThumbRight:
                return JointType.WristRight;
        }

        return (JointType)((int)joint - 1);
    }

    public static readonly Vector3[] JointBaseDir =
    {
        Vector3.zero,
        Vector3.up,
        Vector3.up,
        Vector3.up,
        Vector3.left,
        Vector3.left,
        Vector3.left,
        Vector3.left,
        Vector3.right,
        Vector3.right,
        Vector3.right,
        Vector3.right,
        Vector3.down,
        Vector3.down,
        Vector3.down,
        Vector3.forward,
        Vector3.down,
        Vector3.down,
        Vector3.down,
        Vector3.forward,
        Vector3.up,
        Vector3.left,
        Vector3.forward,
        Vector3.right,
        Vector3.forward
    };
}

public struct UTBodyData
{
    public UTBodyData(int jointCount)
    {
        this.position = Vector3.zero;
        this.orientation = Quaternion.identity;
        this.joints = new UTJointData[jointCount];

        this.normalRotation = Quaternion.identity;
        this.mirroredRotation = Quaternion.identity;
        this.bodyFullAngle = 0f;
        this.bodyTurnAngle = 0f;
        this.leftHandOrientation = Quaternion.identity;
        this.rightHandOrientation = Quaternion.identity;
        this.hipsDirection = Vector3.zero;
        this.shouldersDirection = Vector3.zero;
        this.turnAroundFactor = 0.5f;
    }

    public Vector3 position;
    public Quaternion orientation;
    public UTJointData[] joints;

    public Quaternion normalRotation;
    public Quaternion mirroredRotation;
    public float bodyFullAngle;
    public float bodyTurnAngle;
    public Quaternion leftHandOrientation;
    public Quaternion rightHandOrientation;
    public Vector3 hipsDirection;
    public Vector3 shouldersDirection;
    public float turnAroundFactor;
}

public struct UTJointData
{
    public JointType jointType;
    public Vector3 kinectPos;
    public Vector3 position; //we need a kinect to world matrix to compute this
    public Quaternion orientation;

    //calculated from the raw data above
    public Vector3 direction;
    public Quaternion normalRotation;
    public Quaternion mirroredRotation;
}

public enum JointType : int
{
    SpineBase = 0,
    SpineMid = 1,
    Neck = 2,
    Head = 3,
    ShoulderLeft = 4,
    ElbowLeft = 5,
    WristLeft = 6,
    HandLeft = 7,
    ShoulderRight = 8,
    ElbowRight = 9,
    WristRight = 10,
    HandRight = 11,
    HipLeft = 12,
    KneeLeft = 13,
    AnkleLeft = 14,
    FootLeft = 15,
    HipRight = 16,
    KneeRight = 17,
    AnkleRight = 18,
    FootRight = 19,
    SpineShoulder = 20,
    HandTipLeft = 21,
    ThumbLeft = 22,
    HandTipRight = 23,
    ThumbRight = 24,
    FloorPlane = 25,
    Invalid = -1
    //Count = 25
}
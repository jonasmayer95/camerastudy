using UnityEngine;
//using Windows.Kinect;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

[RequireComponent(typeof(Animator))]
public class AvatarController : MonoBehaviour
{
    // The index of the player, whose movements the model represents. Default 0 (first player).
    public int playerIndex = 0;

    // Whether the character is facing the player or not. Default false.
    public bool mirroredMovement = false;

    // Bool that determines whether the avatar is allowed to do vertical movement
    public bool verticalMovement = false;

    // Rate at which avatar will move through the scene.
    public float moveRate = 1f;

    // Slerp smooth factor
    public float smoothFactor = 5f;

    // Offset node this transform is relative to, if any (optional)
    public GameObject offsetNode;

    // Makes initial avatar position, relative to the specified camera
    // to be equal to the user's position, relative to the sensor (optional)
    public Camera posRelativeToCamera;


    // The body root node
    protected Transform bodyRoot;

    // Variable to hold all them bones. It will initialize the same size as initialRotations.
    protected Transform[] bones;

    // Rotations of the bones when the Kinect tracking starts.
    protected Quaternion[] initialRotations;

    // Initial position and rotation of the transform
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;
    protected Vector3 offsetNodePos;
    protected Quaternion offsetNodeRot;
    protected Vector3 bodyRootPosition;

    // Calibration Offset Variables for Character Position.
    protected bool offsetCalibrated = false;
    protected float xOffset, yOffset, zOffset;
    //private Quaternion originalRotation;

    // whether the parent transform obeys physics
    protected bool isRigidBody = false;

    // private instance of the KinectManager
    //protected KinectManager kinectManager;

    public enum AllowedRotations : int { None = 0, Default = 1, All = 2 }
    public AllowedRotations allowedHandRotations = AllowedRotations.Default;

    // returns the number of bone transforms (array length)
    public int GetBoneTransformCount()
    {
        return bones != null ? bones.Length : 0;
    }

    // returns the bone transform by index
    public Transform GetBoneTransform(int index)
    {
        if (index >= 0 && index < bones.Length)
        {
            return bones[index];
        }

        return null;
    }


    // transform caching gives performance boost since Unity calls GetComponent<Transform>() each time you call transform 
    private Transform _transformCache;
    public new Transform transform
    {
        get
        {
            if (!_transformCache)
                _transformCache = base.transform;

            return _transformCache;
        }
    }


    public void Awake()
    {
        // check for double start
        if (bones != null)
            return;
        if (!gameObject.activeInHierarchy)
            return;

        // inits the bones array
        bones = new Transform[27];

        // Initial rotations and directions of the bones.
        initialRotations = new Quaternion[bones.Length];

        // Map bones to the points the Kinect tracks
        MapBones();

        // Get initial bone rotations
        GetInitialRotations();

        // if parent transform uses physics
        isRigidBody = gameObject.GetComponent<Rigidbody>();
    }

    // Update the avatar each frame.
    public void UpdateAvatar(Int64 UserID)
    {
        if (!gameObject.activeInHierarchy)
            return;


        // Get the KinectManager instance
        //if (kinectManager == null)
        //{
        //    kinectManager = KinectManager.Instance;
        //}

        // move the avatar to its Kinect position
        MoveAvatar(UserID);

        for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!bones[boneIndex])
                continue;

            if (boneIndex2JointMap.ContainsKey(boneIndex))
            {
                KinectInterop.JointType joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];
                TransformBone(UserID, joint, boneIndex, !mirroredMovement);
            }
            else if (specIndex2JointMap.ContainsKey(boneIndex))
            {
                // special bones (clavicles)
                List<KinectInterop.JointType> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorJointMap[boneIndex];

                if (alJoints.Count >= 2)
                {
                    //Debug.Log(alJoints[0].ToString());
                    Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
                    TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement);
                }
            }
        }
    }

    public void UpdateInseilAvatar(InseilMeasurement kinectData)
    {
        if (!gameObject.activeInHierarchy)
            return;

        // move the avatar to its Kinect position
        MoveInseilAvatar(kinectData);

        for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!bones[boneIndex])
                continue;

            if (boneIndex2JointMap.ContainsKey(boneIndex))
            {
                KinectInterop.JointType joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];
                TransformInseilBone(joint, boneIndex, !mirroredMovement);
            }
            else if (specIndex2JointMap.ContainsKey(boneIndex))
            {
                // special bones (clavicles)
                List<KinectInterop.JointType> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorJointMap[boneIndex];

                if (alJoints.Count >= 2)
                {
                    //Debug.Log(alJoints[0].ToString());
                    Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
                    TransformInseilSpecialBone(alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement);
                }
            }
        }
    }

    // Set bones to their initial positions and rotations.
    public void ResetToInitialPosition()
    {
        if (bones == null)
            return;

        // For each bone that was defined, reset to initial position.
        transform.rotation = Quaternion.identity;

        for (int pass = 0; pass < 2; pass++)  // 2 passes because clavicles are at the end
        {
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null)
                {
                    bones[i].rotation = initialRotations[i];
                }
            }
        }

        //		if(bodyRoot != null)
        //		{
        //			bodyRoot.localPosition = Vector3.zero;
        //			bodyRoot.localRotation = Quaternion.identity;
        //		}

        // Restore the offset's position and rotation
        if (offsetNode != null)
        {
            offsetNode.transform.position = offsetNodePos;
            offsetNode.transform.rotation = offsetNodeRot;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    // Invoked on the successful calibration of a player.
    public void SuccessfulCalibration(Int64 userId)
    {
        // reset the models position
        if (offsetNode != null)
        {
            offsetNode.transform.position = offsetNodePos;
            offsetNode.transform.rotation = offsetNodeRot;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // re-calibrate the position offset
        offsetCalibrated = false;
    }

    // Apply the rotations tracked by kinect to the joints.
    protected void TransformBone(Int64 userId, KinectInterop.JointType joint, int boneIndex, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null /*|| kinectManager == null*/)
            return;

        int iJoint = (int)joint;
        if (iJoint < 0 /*|| !kinectManager.IsJointTracked(userId, iJoint)*/)
            return;

        // Get Kinect joint orientation
        Quaternion jointRotation = /*kinectManager.GetJointOrientation(userId, iJoint, flip)*/ Quaternion.identity;
        if (jointRotation == Quaternion.identity)
            return;

        // Smoothly transition to the new rotation
        Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

        if (smoothFactor != 0f)
            boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
        else
            boneTransform.rotation = newRotation;
    }

    protected void TransformInseilBone(KinectInterop.JointType joint, int boneIndex, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null)
            return;

        int iJoint = (int)joint;

        //ubitrack only gives us tracked joints, no need to check
        if (iJoint < 0 /*|| !kinectManager.IsJointTracked(userId, iJoint)*/)
            return;

        // Get Kinect joint orientation
        Quaternion jointRotation = /*kinectManager.GetJointOrientation(userId, iJoint, flip)*/ Quaternion.identity; //TODO: get ubitrack data here
        if (jointRotation == Quaternion.identity)
            return;

        // Smoothly transition to the new rotation
        Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

        if (smoothFactor != 0f)
            boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
        else
            boneTransform.rotation = newRotation;
    }

    // Apply the rotations tracked by kinect to a special joint
    protected void TransformSpecialBone(Int64 userId, KinectInterop.JointType joint, KinectInterop.JointType jointParent, int boneIndex, Vector3 baseDir, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null /*|| kinectManager == null*/)
            return;

        //if(!kinectManager.IsJointTracked(userId, (int)joint) || 
        //   !kinectManager.IsJointTracked(userId, (int)jointParent))
        //{
        //    return;
        //}

        Vector3 jointDir = /*kinectManager.GetJointDirection(userId, (int)joint, false, true)*/ Vector3.zero;
        Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;

        if (!flip)
        {
            Vector3 mirroredAngles = jointRotation.eulerAngles;
            mirroredAngles.y = -mirroredAngles.y;
            mirroredAngles.z = -mirroredAngles.z;

            jointRotation = Quaternion.Euler(mirroredAngles);
        }

        if (jointRotation != Quaternion.identity)
        {
            // Smoothly transition to the new rotation
            Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

            if (smoothFactor != 0f)
                boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
            else
                boneTransform.rotation = newRotation;
        }

    }

    protected void TransformInseilSpecialBone(KinectInterop.JointType joint, KinectInterop.JointType jointParent, int boneIndex, Vector3 baseDir, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null)
            return;

        //TODO: remove dependency on kinectmanager. this will be tricky, since the sensor only gives us position
        //and orientation data. the rest is calculated by kinectmanager from raw body frame data.
        //CalculateJointOrients seems to be doing most of the work.

        Vector3 jointDir = /*kinectManager.GetJointDirection(userId, (int)joint, false, true)*/ Vector3.zero;
        Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;

        if (!flip)
        {
            Vector3 mirroredAngles = jointRotation.eulerAngles;
            mirroredAngles.y = -mirroredAngles.y;
            mirroredAngles.z = -mirroredAngles.z;

            jointRotation = Quaternion.Euler(mirroredAngles);
        }

        if (jointRotation != Quaternion.identity)
        {
            // Smoothly transition to the new rotation
            Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

            if (smoothFactor != 0f)
                boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
            else
                boneTransform.rotation = newRotation;
        }
    }

    

    private void CalculateJointOrients(InseilMeasurement trackingData)
    {
        int jointCount = trackingData.data.Count;

        for (int j = 0; j < jointCount; j++)
        {
            int joint = j;

            //TODO: find the right joint order
            KinectInterop.JointData jointData = bodyData.joint[joint];


            //if (bJointValid)
            {
                int nextJoint = (int)GetNextJoint((JointType)joint);
                if (nextJoint != joint && nextJoint >= 0 && nextJoint < jointCount) //set jointcount to that from inseilmessage
                {
                    //if current joint is in range, get the next one's data

                    KinectInterop.JointData nextJointData = bodyData.joint[nextJoint];
                    //bool bNextJointValid = ignoreInferredJoints ? nextJointData.trackingState == KinectInterop.TrackingState.Tracked : nextJointData.trackingState != KinectInterop.TrackingState.NotTracked;

                    //calculate normalized direction of next joint
                    Vector3 baseDir = KinectInterop.JointBaseDir[nextJoint]; //TODO: copy base directions from KinectInterop
                    Vector3 jointDir = nextJointData.direction;
                    jointDir = new Vector3(jointDir.x, jointDir.y, -jointDir.z).normalized;

                    Quaternion jointOrientNormal = jointData.normalRotation;
                    //if (bNextJointValid)
                    //{
                        jointOrientNormal = Quaternion.FromToRotation(baseDir, jointDir);
                    //}

                    //special case for shoulders
                    if ((joint == (int)KinectInterop.JointType.ShoulderLeft) ||
                       (joint == (int)KinectInterop.JointType.ShoulderRight))
                    {
                        float angle = -bodyData.bodyFullAngle;
                        Vector3 axis = jointDir;
                        Quaternion armTurnRotation = Quaternion.AngleAxis(angle, axis);

                        jointData.normalRotation = armTurnRotation * jointOrientNormal;
                    }
                        //left arm
                    else if ((joint == (int)KinectInterop.JointType.ElbowLeft) ||
                            (joint == (int)KinectInterop.JointType.WristLeft) ||
                            (joint == (int)KinectInterop.JointType.HandLeft))
                    {
                        if (joint == (int)KinectInterop.JointType.WristLeft)
                        {
                            KinectInterop.JointData handData = bodyData.joint[(int)KinectInterop.JointType.HandLeft];
                            KinectInterop.JointData handTipData = bodyData.joint[(int)KinectInterop.JointType.HandTipLeft];

                            if (handData.trackingState != KinectInterop.TrackingState.NotTracked &&
                               handTipData.trackingState != KinectInterop.TrackingState.NotTracked)
                            {
                                jointDir = handData.direction + handTipData.direction;
                                jointDir = new Vector3(jointDir.x, jointDir.y, -jointDir.z).normalized;
                            }
                        }

                        KinectInterop.JointData shCenterData = bodyData.joint[(int)KinectInterop.JointType.SpineShoulder];
                        if (shCenterData.trackingState != KinectInterop.TrackingState.NotTracked &&
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
                        if ((allowedHandRotations == AllowedRotations.All) &&
                           (joint != (int)KinectInterop.JointType.ElbowLeft))
                        {
                            KinectInterop.JointData handData = bodyData.joint[(int)KinectInterop.JointType.HandLeft];
                            KinectInterop.JointData handTipData = bodyData.joint[(int)KinectInterop.JointType.HandTipLeft];
                            KinectInterop.JointData thumbData = bodyData.joint[(int)KinectInterop.JointType.ThumbLeft];

                            if (handData.trackingState != KinectInterop.TrackingState.NotTracked &&
                               handTipData.trackingState != KinectInterop.TrackingState.NotTracked &&
                               thumbData.trackingState != KinectInterop.TrackingState.NotTracked)
                            {
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
                                    KinectInterop.JointData elbowData = bodyData.joint[(int)KinectInterop.JointType.ElbowLeft];
                                    if (elbowData.trackingState != KinectInterop.TrackingState.NotTracked)
                                    {
                                        Quaternion quatLocalRot = Quaternion.Inverse(elbowData.normalRotation) * jointData.normalRotation;
                                        float angleY = quatLocalRot.eulerAngles.y;

                                        if (angleY >= 90f && angleY < 270f && bodyData.leftHandOrientation != Quaternion.identity)
                                        {
                                            jointData.normalRotation = bodyData.leftHandOrientation;
                                        }

                                        bodyData.leftHandOrientation = jointData.normalRotation;
                                    }
                                }
                            }

                            bRotated = true;
                        }

                        if (!bRotated)
                        {
                            float angle = -bodyData.bodyFullAngle;
                            Vector3 axis = jointDir;
                            Quaternion armTurnRotation = Quaternion.AngleAxis(angle, axis);

                            jointData.normalRotation = (allowedHandRotations != AllowedRotations.None || joint == (int)KinectInterop.JointType.ElbowLeft) ?
                                armTurnRotation * jointOrientNormal : armTurnRotation;
                        }
                    }
                        //right arm
                    else if ((joint == (int)KinectInterop.JointType.ElbowRight) ||
                            (joint == (int)KinectInterop.JointType.WristRight) ||
                            (joint == (int)KinectInterop.JointType.HandRight))
                    {
                        if (joint == (int)KinectInterop.JointType.WristRight)
                        {
                            KinectInterop.JointData handData = bodyData.joint[(int)KinectInterop.JointType.HandRight];
                            KinectInterop.JointData handTipData = bodyData.joint[(int)KinectInterop.JointType.HandTipRight];

                            if (handData.trackingState != KinectInterop.TrackingState.NotTracked &&
                               handTipData.trackingState != KinectInterop.TrackingState.NotTracked)
                            {
                                jointDir = handData.direction + handTipData.direction;
                                jointDir = new Vector3(jointDir.x, jointDir.y, -jointDir.z).normalized;
                            }
                        }

                        KinectInterop.JointData shCenterData = bodyData.joint[(int)KinectInterop.JointType.SpineShoulder];
                        if (shCenterData.trackingState != KinectInterop.TrackingState.NotTracked &&
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
                        if ((allowedHandRotations == AllowedRotations.All) &&
                           (joint != (int)KinectInterop.JointType.ElbowRight))
                        {
                            KinectInterop.JointData handData = bodyData.joint[(int)KinectInterop.JointType.HandRight];
                            KinectInterop.JointData handTipData = bodyData.joint[(int)KinectInterop.JointType.HandTipRight];
                            KinectInterop.JointData thumbData = bodyData.joint[(int)KinectInterop.JointType.ThumbRight];

                            if (handData.trackingState != KinectInterop.TrackingState.NotTracked &&
                               handTipData.trackingState != KinectInterop.TrackingState.NotTracked &&
                               thumbData.trackingState != KinectInterop.TrackingState.NotTracked)
                            {
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
                                    KinectInterop.JointData elbowData = bodyData.joint[(int)KinectInterop.JointType.ElbowRight];
                                    if (elbowData.trackingState != KinectInterop.TrackingState.NotTracked)
                                    {
                                        Quaternion quatLocalRot = Quaternion.Inverse(elbowData.normalRotation) * jointData.normalRotation;
                                        float angleY = quatLocalRot.eulerAngles.y;

                                        if (angleY >= 90f && angleY < 270f && bodyData.rightHandOrientation != Quaternion.identity)
                                        {
                                            jointData.normalRotation = bodyData.rightHandOrientation;
                                        }

                                        bodyData.rightHandOrientation = jointData.normalRotation;
                                    }
                                }
                            }

                            bRotated = true;
                        }

                        if (!bRotated)
                        {
                            float angle = -bodyData.bodyFullAngle;
                            Vector3 axis = jointDir;
                            Quaternion armTurnRotation = Quaternion.AngleAxis(angle, axis);

                            jointData.normalRotation = (allowedHandRotations != AllowedRotations.None || joint == (int)KinectInterop.JointType.ElbowRight) ?
                                armTurnRotation * jointOrientNormal : armTurnRotation;
                        }
                    }
                    else
                    {
                        jointData.normalRotation = jointOrientNormal;
                    }

                    //torso
                    if ((joint == (int)KinectInterop.JointType.SpineMid) ||
                       (joint == (int)KinectInterop.JointType.SpineShoulder) ||
                       (joint == (int)KinectInterop.JointType.Neck))
                    {
                        Vector3 baseDir2 = Vector3.right;
                        Vector3 jointDir2 = Vector3.Lerp(bodyData.shouldersDirection, -bodyData.shouldersDirection, bodyData.turnAroundFactor);
                        jointDir2.z = -jointDir2.z;

                        jointData.normalRotation *= Quaternion.FromToRotation(baseDir2, jointDir2);
                    }
                        //spinebase and legs
                    else if ((joint == (int)KinectInterop.JointType.SpineBase) ||
                       (joint == (int)KinectInterop.JointType.HipLeft) || (joint == (int)KinectInterop.JointType.HipRight) ||
                       (joint == (int)KinectInterop.JointType.KneeLeft) || (joint == (int)KinectInterop.JointType.KneeRight) ||
                       (joint == (int)KinectInterop.JointType.AnkleLeft) || (joint == (int)KinectInterop.JointType.AnkleRight))
                    {
                        Vector3 baseDir2 = Vector3.right;
                        Vector3 jointDir2 = Vector3.Lerp(bodyData.hipsDirection, -bodyData.hipsDirection, bodyData.turnAroundFactor);
                        jointDir2.z = -jointDir2.z;

                        jointData.normalRotation *= Quaternion.FromToRotation(baseDir2, jointDir2);
                    }

                    //face tracking (not needed for now)
                    //if (joint == (int)KinectInterop.JointType.Neck &&
                    //   sensorData != null && sensorData.sensorInterface != null)
                    //{
                    //    if (sensorData.sensorInterface.IsFaceTrackingActive() &&
                    //       sensorData.sensorInterface.IsFaceTracked(bodyData.liTrackingID))
                    //    {
                    //        KinectInterop.JointData neckData = bodyData.joint[(int)KinectInterop.JointType.Neck];
                    //        KinectInterop.JointData headData = bodyData.joint[(int)KinectInterop.JointType.Head];

                    //        if (neckData.trackingState == KinectInterop.TrackingState.Tracked &&
                    //           headData.trackingState == KinectInterop.TrackingState.Tracked)
                    //        {
                    //            Quaternion headRotation = Quaternion.identity;
                    //            if (sensorData.sensorInterface.GetHeadRotation(bodyData.liTrackingID, ref headRotation))
                    //            {
                    //                Vector3 rotAngles = headRotation.eulerAngles;
                    //                rotAngles.x = -rotAngles.x;
                    //                rotAngles.y = -rotAngles.y;

                    //                bodyData.headOrientation = bodyData.headOrientation != Quaternion.identity ?
                    //                    Quaternion.Slerp(bodyData.headOrientation, Quaternion.Euler(rotAngles), 5f * Time.deltaTime) :
                    //                        Quaternion.Euler(rotAngles);

                    //                jointData.normalRotation = bodyData.headOrientation;
                    //            }
                    //        }
                    //    }
                    //}

                    Vector3 mirroredAngles = jointData.normalRotation.eulerAngles;
                    mirroredAngles.y = -mirroredAngles.y;
                    mirroredAngles.z = -mirroredAngles.z;

                    jointData.mirroredRotation = Quaternion.Euler(mirroredAngles);
                }
                else
                {
                    // get the orientation of the parent joint
                    int prevJoint = (int)GetParentJoint((JointType)joint);
                    if (prevJoint != joint && prevJoint >= 0 && prevJoint < sensorData.jointCount) //get joint count from meaurement
                    {
                        jointData.normalRotation = bodyData.joint[prevJoint].normalRotation;
                        jointData.mirroredRotation = bodyData.joint[prevJoint].mirroredRotation;
                    }
                    else
                    {
                        jointData.normalRotation = Quaternion.identity;
                        jointData.mirroredRotation = Quaternion.identity;
                    }
                }
            }

            bodyData.joint[joint] = jointData;

            if (joint == (int)KinectInterop.JointType.SpineBase)
            {
                bodyData.normalRotation = jointData.normalRotation;
                bodyData.mirroredRotation = jointData.mirroredRotation;
            }
        }
    }

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

    // Moves the avatar in 3D space - pulls the tracked position of the user and applies it to root.
    protected void MoveAvatar(Int64 UserID)
    {
        //if(!kinectManager || !kinectManager.IsJointTracked(UserID, (int)KinectInterop.JointType.SpineBase))
        //    return;

        // Get the position of the body and store it.
        Vector3 trans = /*kinectManager.GetUserPosition(UserID)*/ Vector3.zero;

        // If this is the first time we're moving the avatar, set the offset. Otherwise ignore it.
        if (!offsetCalibrated)
        {
            offsetCalibrated = true;

            xOffset = trans.x;  // !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
            yOffset = trans.y;  // trans.y * moveRate;
            zOffset = !mirroredMovement ? -trans.z : trans.z;  // -trans.z * moveRate;

            if (posRelativeToCamera)
            {
                Vector3 cameraPos = posRelativeToCamera.transform.position;
                Vector3 bodyRootPos = bodyRoot != null ? bodyRoot.position : transform.position;
                Vector3 hipCenterPos = bodyRoot != null ? bodyRoot.position : bones[0].position;

                float yRelToAvatar = 0f;
                if (verticalMovement)
                {
                    yRelToAvatar = (trans.y - cameraPos.y) - (hipCenterPos - bodyRootPos).magnitude;
                }
                else
                {
                    yRelToAvatar = bodyRootPos.y - cameraPos.y;
                }

                Vector3 relativePos = new Vector3(trans.x, yRelToAvatar, trans.z);
                Vector3 newBodyRootPos = cameraPos + relativePos;

                //				if(offsetNode != null)
                //				{
                //					newBodyRootPos += offsetNode.transform.position;
                //				}

                if (bodyRoot != null)
                {
                    bodyRoot.position = newBodyRootPos;
                }
                else
                {
                    transform.position = newBodyRootPos;
                }

                bodyRootPosition = newBodyRootPos;
            }
        }

        // Smoothly transition to the new position
        Vector3 targetPos = bodyRootPosition + Kinect2AvatarPos(trans, verticalMovement);

        if (isRigidBody && !verticalMovement)
        {
            // workaround for obeying the physics (e.g. gravity falling)
            targetPos.y = bodyRoot != null ? bodyRoot.position.y : transform.position.y;
        }

        if (bodyRoot != null)
        {
            bodyRoot.position = smoothFactor != 0f ?
                Vector3.Lerp(bodyRoot.position, targetPos, smoothFactor * Time.deltaTime) : targetPos;
        }
        else
        {
            transform.position = smoothFactor != 0f ?
                Vector3.Lerp(transform.position, targetPos, smoothFactor * Time.deltaTime) : targetPos;
        }
    }

    public void MoveInseilAvatar(InseilMeasurement kinectData)
    {
        // Get the position of the body and store it.
        float xPos = (float)kinectData.data["spinebase"].position.x;
        float yPos = (float)kinectData.data["spinebase"].position.y;
        float zPos = (float)kinectData.data["spinebase"].position.z;
        Vector3 trans = new Vector3(xPos, yPos, zPos);

        // If this is the first time we're moving the avatar, set the offset. Otherwise ignore it.
        if (!offsetCalibrated)
        {
            offsetCalibrated = true;

            xOffset = trans.x;  // !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
            yOffset = trans.y;  // trans.y * moveRate;
            zOffset = !mirroredMovement ? -trans.z : trans.z;  // -trans.z * moveRate;

            if (posRelativeToCamera)
            {
                Vector3 cameraPos = posRelativeToCamera.transform.position;
                Vector3 bodyRootPos = bodyRoot != null ? bodyRoot.position : transform.position;
                Vector3 hipCenterPos = bodyRoot != null ? bodyRoot.position : bones[0].position;

                float yRelToAvatar = 0f;
                if (verticalMovement)
                {
                    yRelToAvatar = (trans.y - cameraPos.y) - (hipCenterPos - bodyRootPos).magnitude;
                }
                else
                {
                    yRelToAvatar = bodyRootPos.y - cameraPos.y;
                }

                Vector3 relativePos = new Vector3(trans.x, yRelToAvatar, trans.z);
                Vector3 newBodyRootPos = cameraPos + relativePos;

                //				if(offsetNode != null)
                //				{
                //					newBodyRootPos += offsetNode.transform.position;
                //				}

                if (bodyRoot != null)
                {
                    bodyRoot.position = newBodyRootPos;
                }
                else
                {
                    transform.position = newBodyRootPos;
                }

                bodyRootPosition = newBodyRootPos;
            }
        }

        // Smoothly transition to the new position
        Vector3 targetPos = bodyRootPosition + Kinect2AvatarPos(trans, verticalMovement);

        if (isRigidBody && !verticalMovement)
        {
            // workaround for obeying the physics (e.g. gravity falling)
            targetPos.y = bodyRoot != null ? bodyRoot.position.y : transform.position.y;
        }

        if (bodyRoot != null)
        {
            bodyRoot.position = smoothFactor != 0f ?
                Vector3.Lerp(bodyRoot.position, targetPos, smoothFactor * Time.deltaTime) : targetPos;
        }
        else
        {
            transform.position = smoothFactor != 0f ?
                Vector3.Lerp(transform.position, targetPos, smoothFactor * Time.deltaTime) : targetPos;
        }
    }

    // If the bones to be mapped have been declared, map that bone to the model.
    protected virtual void MapBones()
    {
        //		// make OffsetNode as a parent of model transform.
        //		offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
        //		offsetNode.transform.position = transform.position;
        //		offsetNode.transform.rotation = transform.rotation;
        //		offsetNode.transform.parent = transform.parent;

        //		// take model transform as body root
        //		transform.parent = offsetNode.transform;
        //		transform.localPosition = Vector3.zero;
        //		transform.localRotation = Quaternion.identity;

        //bodyRoot = transform;

        // get bone transforms from the animator component
        Animator animatorComponent = GetComponent<Animator>();

        for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!boneIndex2MecanimMap.ContainsKey(boneIndex))
                continue;

            bones[boneIndex] = animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]);
        }
    }

    // Capture the initial rotations of the bones
    protected void GetInitialRotations()
    {
        // save the initial rotation
        if (offsetNode != null)
        {
            offsetNodePos = offsetNode.transform.position;
            offsetNodeRot = offsetNode.transform.rotation;
        }

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        //		if(offsetNode != null)
        //		{
        //			initialRotation = Quaternion.Inverse(offsetNodeRot) * initialRotation;
        //		}

        transform.rotation = Quaternion.identity;

        // save the body root initial position
        if (bodyRoot != null)
        {
            bodyRootPosition = bodyRoot.position;
        }
        else
        {
            bodyRootPosition = transform.position;
        }

        if (offsetNode != null)
        {
            bodyRootPosition = bodyRootPosition - offsetNodePos;
        }

        // save the initial bone rotations
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                initialRotations[i] = bones[i].rotation;
            }
        }

        // Restore the initial rotation
        transform.rotation = initialRotation;
    }

    // Converts kinect joint rotation to avatar joint rotation, depending on joint initial rotation and offset rotation
    protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
    {
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];
        //newRotation = initialRotation * newRotation;

        if (offsetNode != null)
        {
            newRotation = offsetNode.transform.rotation * newRotation;
        }
        else
        {
            newRotation = initialRotation * newRotation;
        }

        return newRotation;
    }

    // Converts Kinect position to avatar skeleton position, depending on initial position, mirroring and move rate
    protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
    {
        float xPos;

        //		if(!mirroredMovement)
        xPos = (jointPosition.x - xOffset) * moveRate;
        //		else
        //			xPos = (-jointPosition.x - xOffset) * moveRate;

        float yPos = (jointPosition.y - yOffset) * moveRate;
        //float zPos = (-jointPosition.z - zOffset) * moveRate;
        float zPos = !mirroredMovement ? (-jointPosition.z - zOffset) * moveRate : (jointPosition.z - zOffset) * moveRate;

        Vector3 newPosition = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);

        if (offsetNode != null)
        {
            newPosition += offsetNode.transform.position;
        }

        return newPosition;
    }

    //	protected void OnCollisionEnter(Collision col)
    //	{
    //		Debug.Log("Collision entered");
    //	}
    //
    //	protected void OnCollisionExit(Collision col)
    //	{
    //		Debug.Log("Collision exited");
    //	}

    // dictionaries to speed up bones' processing
    // the author of the terrific idea for kinect-joints to mecanim-bones mapping
    // along with its initial implementation, including following dictionary is
    // Mikhail Korchun (korchoon@gmail.com). Big thanks to this guy!
    private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
	{
		{0, HumanBodyBones.Hips},
		{1, HumanBodyBones.Spine},
//        {2, HumanBodyBones.Chest},
		{3, HumanBodyBones.Neck},
//		{4, HumanBodyBones.Head},
		
		{5, HumanBodyBones.LeftUpperArm},
		{6, HumanBodyBones.LeftLowerArm},
		{7, HumanBodyBones.LeftHand},
		{8, HumanBodyBones.LeftIndexProximal},
//		{9, HumanBodyBones.LeftIndexIntermediate},
//		{10, HumanBodyBones.LeftThumbProximal},
		
		{11, HumanBodyBones.RightUpperArm},
		{12, HumanBodyBones.RightLowerArm},
		{13, HumanBodyBones.RightHand},
		{14, HumanBodyBones.RightIndexProximal},
//		{15, HumanBodyBones.RightIndexIntermediate},
//		{16, HumanBodyBones.RightThumbProximal},
		
		{17, HumanBodyBones.LeftUpperLeg},
		{18, HumanBodyBones.LeftLowerLeg},
		{19, HumanBodyBones.LeftFoot},
//		{20, HumanBodyBones.LeftToes},
		
		{21, HumanBodyBones.RightUpperLeg},
		{22, HumanBodyBones.RightLowerLeg},
		{23, HumanBodyBones.RightFoot},
//		{24, HumanBodyBones.RightToes},
		
		{25, HumanBodyBones.LeftShoulder},
		{26, HumanBodyBones.RightShoulder},
	};

    protected readonly Dictionary<int, KinectInterop.JointType> boneIndex2JointMap = new Dictionary<int, KinectInterop.JointType>
	{
		{0, KinectInterop.JointType.SpineBase},
		{1, KinectInterop.JointType.SpineMid},
		{2, KinectInterop.JointType.SpineShoulder},
		{3, KinectInterop.JointType.Neck},
		{4, KinectInterop.JointType.Head},
		
		{5, KinectInterop.JointType.ShoulderLeft},
		{6, KinectInterop.JointType.ElbowLeft},
		{7, KinectInterop.JointType.WristLeft},
		{8, KinectInterop.JointType.HandLeft},
		
		{9, KinectInterop.JointType.HandTipLeft},
		{10, KinectInterop.JointType.ThumbLeft},
		
		{11, KinectInterop.JointType.ShoulderRight},
		{12, KinectInterop.JointType.ElbowRight},
		{13, KinectInterop.JointType.WristRight},
		{14, KinectInterop.JointType.HandRight},
		
		{15, KinectInterop.JointType.HandTipRight},
		{16, KinectInterop.JointType.ThumbRight},
		
		{17, KinectInterop.JointType.HipLeft},
		{18, KinectInterop.JointType.KneeLeft},
		{19, KinectInterop.JointType.AnkleLeft},
		{20, KinectInterop.JointType.FootLeft},
		
		{21, KinectInterop.JointType.HipRight},
		{22, KinectInterop.JointType.KneeRight},
		{23, KinectInterop.JointType.AnkleRight},
		{24, KinectInterop.JointType.FootRight},
	};

    protected readonly Dictionary<int, List<KinectInterop.JointType>> specIndex2JointMap = new Dictionary<int, List<KinectInterop.JointType>>
	{
		{25, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderLeft, KinectInterop.JointType.SpineShoulder} },
		{26, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderRight, KinectInterop.JointType.SpineShoulder} },
	};

    protected readonly Dictionary<int, KinectInterop.JointType> boneIndex2MirrorJointMap = new Dictionary<int, KinectInterop.JointType>
	{
		{0, KinectInterop.JointType.SpineBase},
		{1, KinectInterop.JointType.SpineMid},
		{2, KinectInterop.JointType.SpineShoulder},
		{3, KinectInterop.JointType.Neck},
		{4, KinectInterop.JointType.Head},
		
		{5, KinectInterop.JointType.ShoulderRight},
		{6, KinectInterop.JointType.ElbowRight},
		{7, KinectInterop.JointType.WristRight},
		{8, KinectInterop.JointType.HandRight},
		
		{9, KinectInterop.JointType.HandTipRight},
		{10, KinectInterop.JointType.ThumbRight},
		
		{11, KinectInterop.JointType.ShoulderLeft},
		{12, KinectInterop.JointType.ElbowLeft},
		{13, KinectInterop.JointType.WristLeft},
		{14, KinectInterop.JointType.HandLeft},
		
		{15, KinectInterop.JointType.HandTipLeft},
		{16, KinectInterop.JointType.ThumbLeft},
		
		{17, KinectInterop.JointType.HipRight},
		{18, KinectInterop.JointType.KneeRight},
		{19, KinectInterop.JointType.AnkleRight},
		{20, KinectInterop.JointType.FootRight},
		
		{21, KinectInterop.JointType.HipLeft},
		{22, KinectInterop.JointType.KneeLeft},
		{23, KinectInterop.JointType.AnkleLeft},
		{24, KinectInterop.JointType.FootLeft},
	};

    protected readonly Dictionary<int, List<KinectInterop.JointType>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectInterop.JointType>>
	{
		{25, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderRight, KinectInterop.JointType.SpineShoulder} },
		{26, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderLeft, KinectInterop.JointType.SpineShoulder} },
	};

}


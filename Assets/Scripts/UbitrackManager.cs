using UnityEngine;
using System;
using System.Collections.Generic;


class UbitrackManager
{
    /// <summary>
    /// Generates a UTBodyData object that can be used for visualization.
    /// </summary>
    /// <param name="skeleton">Kinect-tracked skeleton data</param>
    /// <returns></returns>
    public UTBodyData GenerateBodyData(InseilMeasurement skeleton)
    {
        //map strings we get in InseilMeasurement to JointType, because we can't rely on dictionary ordering

        //create JointData for each joint we get, fill in raw data and calculate extended data
        //similar to what KinectManager does
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
            default:
                return JointType.Invalid;
        }
    }
}

public struct UTBodyData
{
    public Vector3 position;
    public Quaternion orientation;
    public UTJointData[] joints;
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
    Invalid = -1,
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
    ThumbRight = 24
    //Count = 25
}
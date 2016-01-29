using UnityEngine;
using System.Collections;

public class ArtCalibration : MonoBehaviour
{
    public Transform testSphere;
    public bool Calibrated { get; set; }
    private Matrix4x4 artToKinect = new Matrix4x4();


    public void Calibrate(ref KinectInterop.BodyData kinectData, ArtBodyData[] artData, ref Matrix4x4 kinectToWorld)
    {
        //record art marker position
        Vector3 marker = artData[0].pos;

        //record kinect hand (wrist) position
        Vector3 rightHand = kinectData.joint[(int)KinectInterop.JointType.WristRight].kinectPos;

        //90° rotation around x should do it, look here: https://msdn.microsoft.com/en-us/library/dn785530.aspx
        //and in the "room calibration" picture in dtrack
        artToKinect.SetTRS(rightHand, Quaternion.AngleAxis(Mathf.PI / 2, Vector3.right), Vector3.one);

        //substitute kinect data with art

        //move a sphere around with that data for testing...but we still need kinect to world space transformation
        var handPos = artToKinect.MultiplyPoint3x4(marker);
        kinectData.joint[(int)KinectInterop.JointType.WristRight].kinectPos  = handPos;
        kinectData.joint[(int)KinectInterop.JointType.WristRight].position = kinectToWorld.MultiplyPoint3x4(handPos);

        Debug.Log(string.Format("kinectPos: {0}, transformed art pos: {1}", rightHand, handPos));
    }
}

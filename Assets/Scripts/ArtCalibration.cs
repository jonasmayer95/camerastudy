using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Globalization;

//This script should be placed on the 'ART' object below Kinect. Do not modify
//the hierarchy or it will screw up calibration.
public class ArtCalibration : MonoBehaviour
{
    //public bool Calibrated { get; set; }

    //we expect those to be located inside cfg/
    public string art2KinectFileName;
    public string artTarget2MarkerFileName;

    private GameObject artTarget;
    private GameObject squareMarker;

    private Matrix4x4 artToKinect = new Matrix4x4();

    void Awake()
    {
        //do art to kinect calibration here, i.e. read the calibration files and set up
        //stuff inside child gameobjects
        artTarget = transform.GetChild(0).gameObject;
        squareMarker = artTarget.transform.GetChild(0).gameObject;

        Vector3 pos;
        Quaternion rot;

        ArtCalibration.ReadCalibrationFile(out pos, out rot, art2KinectFileName);
        this.transform.localPosition = pos;
        this.transform.localRotation = rot;

        ArtCalibration.ReadCalibrationFile(out pos, out rot, artTarget2MarkerFileName);
        squareMarker.transform.localPosition = pos;
        squareMarker.transform.localRotation = rot;
    }

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

    /// <summary>
    /// Reads data from a calibration file and returns position and rotation data in the out parameters.
    /// </summary>
    /// <param name="position">Position return value</param>
    /// <param name="rotation">Rotation return value</param>
    /// <param name="path">Path to calibration file</param>
    public static void ReadCalibrationFile(out Vector3 position, out Quaternion rotation, string fileName)
    {
        string path = Path.Combine("cfg", fileName);
        string content = File.ReadAllText(path, Encoding.UTF8);

        NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
        nfi.NumberDecimalSeparator = ".";

        char[] splitter = { ' ' };
        string[] strList = content.Split(splitter);
        

        position = new Vector3(float.Parse(strList[0 + 16], nfi), float.Parse(strList[1 + 16], nfi), float.Parse(strList[2 + 16], nfi));
        rotation = new Quaternion(float.Parse(strList[0 + 10], nfi), float.Parse(strList[1 + 10], nfi), float.Parse(strList[2 + 10], nfi), float.Parse(strList[3 + 10], nfi));

        //well...might not be the prettiest way to do that.
        ArtToKinectPosition(position, ref position);
        ArtToKinectRotation(rotation, ref rotation);
    }

    
    public static void ArtToKinectPosition(Vector3 input, ref Vector3 output)
    {
        output.x = input.x;
        output.y = input.y;
        output.z = -input.z;
    }

    public static void ArtToKinectRotation(Quaternion input, ref Quaternion output)
    {
        output.x = -input.x;
        output.y = -input.y;
        output.z = input.z;
        output.w = input.w;
    }
}

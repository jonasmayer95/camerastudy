using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Globalization;

//This script should be placed on the 'ART' object below Kinect. Do not modify
//the hierarchy or it will screw up calibration.
public class ArtCalibration : MonoBehaviour
{

    //we expect those to be located inside cfg/
    public string art2KinectFileName;
    public string artTarget2MarkerFileName;

    private GameObject squareMarker;
    private GameObject kinect;

    void Awake()
    {
        //do art to kinect calibration here, i.e. read the calibration files and set up
        //stuff inside child gameobjects
        kinect = transform.parent.gameObject;
        var artTarget = transform.GetChild(0).gameObject;
        squareMarker = artTarget.transform.GetChild(0).gameObject;

        //get kinectToWorld from KinectManager
        var km = kinect.GetComponent<KinectManager>();
        if (km != null)
        {
            //kinect.transform.position = km.KinectToWorld.MultiplyPoint3x4(kinect.transform.position);
        }
        else
        {
            Debug.Log("ArtCalibration: KinectManager not found on kinect GameObject, calibration will not work");
        }

        Vector3 pos;
        Quaternion rot;

        ArtCalibration.ReadCalibrationFile(out pos, out rot, art2KinectFileName);
        this.transform.localPosition = pos;
        this.transform.localRotation = rot;

        ArtCalibration.ReadCalibrationFile(out pos, out rot, artTarget2MarkerFileName);
        squareMarker.transform.localPosition = pos;
        squareMarker.transform.localRotation = rot;
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
        ArtCalibration.ArtToKinectPosition(position, ref position);
        ArtCalibration.ArtToKinectRotation(rotation, ref rotation);
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

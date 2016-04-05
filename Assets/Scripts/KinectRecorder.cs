using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.IO;
using System;

public class KinectRecorder : MonoBehaviour
{
    public bool LoopPlayback { get; set; }

    public string PlaybackFileName { get; set; }

    // A line from our playback file
    private string line = "";

    // A single item from our line, e.g. a joint
    private string cell = "";

    private StreamWriter kinectWriter;
    private StreamReader reader;
    private Stream playbackFile;

    private float recordStartTime;


    /// <summary>
    /// Sets up the file stream for playing back recorded data, destroying the
    /// previous stream if there was one.
    /// </summary>
    public void OpenPlaybackFile()
    {
        try
        {
            if (reader != null)
            {
                reader.Close();
            }

            if (PlaybackFileName != "")
            {
                playbackFile = new FileStream(PlaybackFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                reader = new StreamReader(playbackFile);
            }
        }
        catch (IOException ex)
        {
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// Gets the next frame/line from the recorded file. If the end of the stream
    /// is reached, it seeks back to the beginning.
    /// </summary>
    /// <param name="frame"></param>
    public void GetFrame(ref KinectInterop.BodyData frame)
    {
        int cellStart = 0;
        int cellEnd = 0;
        int cellIndex = 0;
        line = reader.ReadLine();

        if (line != null)
        {
            while (cellEnd != line.Length - 1)
            {
                //get cell, increment counter, do work on it, set new cell start
                cellEnd = line.IndexOf(";", cellStart);

                ReadCellData(ref frame, line, cellStart, cellEnd, cellIndex);

                cellIndex++;
                cellStart = cellEnd + 1; //one position after the separator
            }
        }
        else if (LoopPlayback == true)
        {
            playbackFile.Seek(0, SeekOrigin.Begin);
        }
    }

    /// <summary>
    /// Constructs a Quaternion from a string.
    /// </summary>
    /// <param name="str">A string with the format (x, y, z, w)</param>
    /// <returns></returns>
    private Quaternion QuaternionFromString(string str)
    {
        float x, y, z, w;

        //char after ( is the first number
        int startIndex = str.IndexOf('(') + 1;
        int endIndex = str.IndexOf(',');

        x = float.Parse(str.Substring(startIndex, endIndex - startIndex));

        startIndex = endIndex + 2; // , and whitespace between members
        endIndex = str.IndexOf(',', startIndex);

        y = float.Parse(str.Substring(startIndex, endIndex - startIndex));

        startIndex = endIndex + 2; // , and whitespace between members
        endIndex = str.IndexOf(',', startIndex);

        z = float.Parse(str.Substring(startIndex, endIndex - startIndex));

        startIndex = endIndex + 2;
        endIndex = str.IndexOf(')', startIndex);

        w = float.Parse(str.Substring(startIndex, endIndex - startIndex));

        return new Quaternion(x, y, z, w);
    }

    /// <summary>
    /// Constructs a Vector 3 from a string
    /// </summary>
    /// <param name="str">A string with the format (x, y, z)</param>
    /// <returns></returns>
    private Vector3 Vector3FromString(string str)
    {
        float x, y, z;

        int startIndex = str.IndexOf('(') + 1;
        int endIndex = str.IndexOf(',');

        x = float.Parse(str.Substring(startIndex, endIndex - startIndex));

        startIndex = endIndex + 2; // , and whitespace between members
        endIndex = str.IndexOf(',', startIndex);

        y = float.Parse(str.Substring(startIndex, endIndex - startIndex));

        startIndex = endIndex + 2; // , and whitespace between members
        endIndex = str.IndexOf(')', startIndex);

        z = float.Parse(str.Substring(startIndex, endIndex - startIndex));

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Constructs a JointData from a string.
    /// </summary>
    /// <param name="str">A string with comma-separated fields of JointData.</param>
    /// <param name="joint"></param>
    private void JointFromString(string str, ref KinectInterop.JointData joint)
    {
        int startIndex = 1; //skip double quote
        int endIndex = str.IndexOf(')', startIndex);

        joint.direction = Vector3FromString(str.Substring(startIndex, endIndex - startIndex + 1)); //include parentheses when dealing with vectors

        startIndex = endIndex + 2; //skip comma
        endIndex = str.IndexOf(',', startIndex);

        joint.jointType = (KinectInterop.JointType)Enum.Parse(typeof(KinectInterop.JointType), str.Substring(startIndex, endIndex - startIndex));

        startIndex = endIndex + 1;
        endIndex = str.IndexOf(')', startIndex);
        joint.kinectPos = Vector3FromString(str.Substring(startIndex, endIndex - startIndex + 1));

        startIndex = endIndex + 2; //comma in between members
        endIndex = str.IndexOf(')', startIndex);
        joint.mirroredRotation = QuaternionFromString(str.Substring(startIndex, endIndex - startIndex + 1));

        startIndex = endIndex + 2;
        endIndex = str.IndexOf(')', startIndex);
        joint.normalRotation = QuaternionFromString(str.Substring(startIndex, endIndex - startIndex + 1));

        startIndex = endIndex + 2;
        endIndex = str.IndexOf(')', startIndex);
        joint.orientation = QuaternionFromString(str.Substring(startIndex, endIndex - startIndex + 1));

        startIndex = endIndex + 2;
        endIndex = str.IndexOf(')', startIndex);
        joint.position = Vector3FromString(str.Substring(startIndex, endIndex - startIndex + 1));

        startIndex = endIndex + 2;
        endIndex = str.IndexOf('"', startIndex);
        joint.trackingState = (KinectInterop.TrackingState)Enum.Parse(typeof(KinectInterop.TrackingState), str.Substring(startIndex, endIndex - startIndex));
    }

    /// <summary>
    /// Reads data from a cell in a single line and assigns it to the corresponding BodyData member
    /// based on the index.
    /// </summary>
    /// <param name="frame"></param>
    /// <param name="line"></param>
    /// <param name="cellStart"></param>
    /// <param name="cellEnd"></param>
    /// <param name="cellIndex"></param>
    private void ReadCellData(ref KinectInterop.BodyData frame, string line, int cellStart, int cellEnd, int cellIndex)
    {
        cell = line.Substring(cellStart, cellEnd - cellStart);

        switch (cellIndex)
        {
            case 0:
                frame.bIsRestricted = short.Parse(cell);
                break;

            case 1:
                frame.bIsTracked = short.Parse(cell);
                break;

            case 2:
                frame.bodyFullAngle = float.Parse(cell);
                break;


            case 3:
                frame.bodyTurnAngle = float.Parse(cell);
                break;

            case 4:
                frame.dwClippedEdges = uint.Parse(cell);
                break;

            case 5:
                frame.headOrientation = QuaternionFromString(cell);
                break;

            case 6:
                frame.hipsDirection = Vector3FromString(cell);
                break;

            case 7:
                frame.isTurnedAround = bool.Parse(cell);
                break;

            case 8:
                frame.leftHandConfidence = (KinectInterop.TrackingConfidence)Enum.Parse(typeof(KinectInterop.TrackingConfidence), cell);
                break;

            case 9:
                frame.leftHandOrientation = QuaternionFromString(cell);
                break;

            case 10:
                frame.leftHandState = (KinectInterop.HandState)Enum.Parse(typeof(KinectInterop.HandState), cell);
                break;

            case 11:
                frame.liTrackingID = long.Parse(cell);
                break;

            case 12:
                frame.mirroredRotation = QuaternionFromString(cell);
                break;

            case 13:
                frame.normalRotation = QuaternionFromString(cell);
                break;

            case 14:
                frame.orientation = QuaternionFromString(cell);
                break;

            case 15:
                frame.position = Vector3FromString(cell);
                break;

            case 16:
                frame.rightHandConfidence = (KinectInterop.TrackingConfidence)Enum.Parse(typeof(KinectInterop.TrackingConfidence), cell);
                break;

            case 17:
                frame.rightHandOrientation = QuaternionFromString(cell);
                break;

            case 18:
                var e = (KinectInterop.HandState)Enum.Parse(typeof(KinectInterop.HandState), cell);
                frame.rightHandState = e;
                break;

            case 19:
                frame.shouldersDirection = Vector3FromString(cell);
                break;

            case 20:
                frame.turnAroundFactor = float.Parse(cell);
                break;

            case 21:
                //TODO: save frametime somewhere, maybe even move it to the beginning of the line so it can be parsed
                //separately before our saved BodyData
                break;

            default:
                JointFromString(cell, ref frame.joint[cellIndex - 22]);
                break;
        }
    }

    public void StartRecording()
    {
        var today = DateTime.Now;
        string fileName = string.Concat("kinectstream_", today.Day.ToString("00"), today.Month.ToString("00"), today.Year.ToString(),
            "_", today.Hour.ToString("00"), today.Minute.ToString("00"), today.Second.ToString("00"), ".csv");

        kinectWriter = new StreamWriter(fileName);
        kinectWriter.AutoFlush = false;
        recordStartTime = Time.time;
    }

    public void EndRecording()
    {
        kinectWriter.Close();
    }

    void OnApplicationQuit()
    {
        if (kinectWriter != null)
        {
            kinectWriter.Close();
        }
    }

    public void WriteBodyData(KinectInterop.BodyData userBodyData)
    {
        //the semicolon is used as a cell separator (at least by libreoffice), just because we talked about it last time

        //write all fields except joints
        kinectWriter.Write("{0};{1};{2};{3};{4};\"{5}\";\"{6}\";{7};{8};\"{9}\";{10};{11};\"{12}\";\"{13}\";\"{14}\";\"{15}\";{16};\"{17}\";{18};\"{19}\";{20};{21};",
            userBodyData.bIsRestricted, userBodyData.bIsTracked, userBodyData.bodyFullAngle, userBodyData.bodyTurnAngle, userBodyData.dwClippedEdges,
            userBodyData.headOrientation.ToString("G"), userBodyData.hipsDirection.ToString("G"), userBodyData.isTurnedAround, userBodyData.leftHandConfidence,
            userBodyData.leftHandOrientation.ToString("G"), userBodyData.leftHandState, userBodyData.liTrackingID, userBodyData.mirroredRotation.ToString("G"),
            userBodyData.normalRotation.ToString("G"), userBodyData.orientation.ToString("G"), userBodyData.position.ToString("G"), userBodyData.rightHandConfidence,
            userBodyData.rightHandOrientation.ToString("G"), userBodyData.rightHandState, userBodyData.shouldersDirection.ToString("G"), userBodyData.turnAroundFactor,
            (Time.time - recordStartTime).ToString("G"));

        //write all joints
        for (int i = 0; i < userBodyData.joint.Length; ++i)
        {
            var joint = userBodyData.joint[i];

            kinectWriter.Write("\"{0},{1},{2},{3},{4},{5},{6},{7}\";", joint.direction.ToString("G"), joint.jointType, joint.kinectPos.ToString("G"),
                joint.mirroredRotation.ToString("G"), joint.normalRotation.ToString("G"), joint.orientation.ToString("G"), joint.position.ToString("G"),
                joint.trackingState);
        }

        kinectWriter.Write("\n");
    }
}

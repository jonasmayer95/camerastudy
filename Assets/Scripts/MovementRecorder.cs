using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.EventSystems;
using System.Text;

public class MovementRecorder : MonoBehaviour, IUserStudyMessageTarget
{
    private static uint frameCount;
    private StreamWriter rawWriter;
    //private StreamWriter detailedWriter;
    public string filePath;
    public string fileName;

    public GameObject avatar;
    private AvatarController controller;
    private static UserStudyData userData;
    
    
    void Start()
    {
        
        if (avatar != null)
        {
            controller = avatar.GetComponent<AvatarController>();

            if (controller == null)
            {
                Debug.LogError("MovementRecorder: avatar gameObject doesn't contain an AvatarController component");
            }
            else
            {
                //everything's fine, set up our filestream
                //TODO: validate path
                var date = DateTime.Now;
                filePath = filePath + fileName + date.Second.ToString() + date.Minute.ToString() +
                    date.Hour.ToString() + date.Day.ToString() + date.Month.ToString() + date.Year.ToString() + ".csv";
                rawWriter = new StreamWriter(filePath);

                //write raw header to file
                string header = string.Concat("name;trial;age;camera;sex;trial_code;start_frame;end_frame;completion_time;current_frame;current_time;", GetBoneDescriptions(controller));
                rawWriter.WriteLine(header);
            }
        }
        else
        {
            Debug.LogError("MovementRecorder: avatar gameObject is null");
            this.gameObject.SetActive(false);
        }
    }

    
    void Update()
    {
        var bones = controller.Bones;

        //we need to be able to write based on certain events. end_frame for example could be triggered by
        //the "game logic"...but rawwriter needs to record all the stuff anyway, so...
        //start_frame and end_frame should be set in response to events.
        //we also need to save the randomized trials somewhere and set them.
        
        //write stuff that has been set through events, then get avatar movement data
        rawWriter.Write("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};", userData.name, userData.trial, userData.age, userData.camType, userData.sex,
            userData.trialCode, userData.startFrame, userData.endFrame, userData.completionTime, frameCount, Time.time);


        for (int i = 0; i < bones.Length; ++i)
        {
            if (!bones[i])
                continue;

            if (controller.BoneIndex2JointMap.ContainsKey(i))
            {
                //record bone position, rotation and timestamp
                WriteBoneData(bones[i], rawWriter);
                
            }
            else if (controller.SpecIndex2JointMap.ContainsKey(i))
            {
                var alJoints = (!controller.mirroredMovement) ? controller.SpecIndex2JointMap[i] : controller.SpecIndex2MirrorJointMap[i];

                if (alJoints.Count >= 2)
                {
                    //record special bone position, rotation and timestamp
                    WriteBoneData(bones[i], rawWriter);
                }
            }
        }
        rawWriter.Write("\n");
        ++frameCount;
    }


    void OnApplicationQuit()
    {
        if (rawWriter != null)
        {
            rawWriter.Flush();
            rawWriter.Dispose();
        }
    }


    public static void InitializeAndActivateUserStudy(string name, uint trial, uint age, CameraType camType, Sex sex)
    {
        userData = new UserStudyData(name, trial, age, camType, sex);

        //so the flow looks like this: UI calls this method and activates the object that contains our recorder.
        //recorder initializes 2 streamwriters and writes stuff every frame. certain properties are set from
        //the outside using events. some of them should be nullable so we don't write garbage before the experiment
        //has started.
    }


    private void WriteBoneData(Transform bone, StreamWriter writer)
    {
        if (writer != null)
        {
            writer.Write("\"{0}, {1}, {2}, {3}, {4}, {5}, {6}\";", bone.position.x, bone.position.y, bone.position.z,
                        bone.rotation.x, bone.rotation.y, bone.rotation.z, bone.rotation.w);
        }
    }

    private string GetBoneDescriptions(AvatarController controller)
    {
        //copy-pasted code, but I don't see any other way to do it...other than a one-time init + public property in avatarcontroller
        if (controller == null)
            return null;

        StringBuilder sb = new StringBuilder();
        var bones = controller.Bones;

        for (int i = 0; i < bones.Length; ++i)
        {
            if (!bones[i])
                continue;

            if (controller.BoneIndex2JointMap.ContainsKey(i))
            {
                KinectInterop.JointType joint = (!controller.mirroredMovement) ? controller.BoneIndex2JointMap[i] : controller.BoneIndex2MirrorJointMap[i];

                sb.Append(joint.ToString());
            }
            else if (controller.SpecIndex2JointMap.ContainsKey(i))
            {
                var alJoints = (!controller.mirroredMovement) ? controller.SpecIndex2JointMap[i] : controller.SpecIndex2MirrorJointMap[i];

                if (alJoints.Count >= 2)
                {
                    sb.Append(string.Format("{0}-{1}", alJoints[0], alJoints[1]));
                }
            }

            sb.Append(";");
        }

        return sb.ToString();
    }

    public void StartTrial(uint startFrame, float startTime)
    {
        userData.startFrame = startFrame;
        userData.startTime = startTime;

        userData.endFrame = null;
        userData.endTime = null;
    }

    public void EndTrial(uint endFrame, float endTime)
    {
        userData.endFrame = endFrame;
        userData.endTime = endTime;

        var timeDiff = userData.endTime - userData.startTime;
        userData.completionTime = timeDiff * 1000;
        
    }

    public void ChangeTrial(uint trialCode)
    {
        userData.trialCode = trialCode;

        ResetTimesAndFrames(ref userData);
    }

    public void ChangeCamera(CameraType cam)
    {
        userData.camType = cam;

        ResetTimesAndFrames(ref userData);
    }

    private void ResetTimesAndFrames(ref UserStudyData data)
    {
        data.startTime = null;
        data.startFrame = null;
        data.endTime = null;
        data.endFrame = null;
        data.completionTime = null;
    }
}

public interface IUserStudyMessageTarget : IEventSystemHandler
{
    /// <summary>
    /// Called when the user puts his hand on the start marker. Should reset
    /// time and frame variables.
    /// </summary>
    /// <param name="startFrame">Number of the frame this was called in.</param>
    /// <param name="startTime">Absolute time this was called (from program start).</param>
    void StartTrial(uint startFrame, float startTime);

    /// <summary>
    /// Called when the user reaches the end marker. Should calculate and write
    /// time difference in milliseconds.
    /// </summary>
    /// <param name="endFrame">Number of the frame this was called in.</param>
    /// <param name="endTime">Absolute time this was called (from program start).</param>
    void EndTrial(uint endFrame, float endTime);

    /// <summary>
    /// Called when the trial (ball positions) should be changed. Should reset
    /// times and frames.
    /// </summary>
    /// <param name="trialCode">Number of the required trial.</param>
    void ChangeTrial(uint trialCode);

    /// <summary>
    /// Called when the camera pattern changes. SHould (probably) reset
    /// local trial number.
    /// </summary>
    /// <param name="cam">The desired camera pattern.</param>
    void ChangeCamera(CameraType cam);
}

struct UserStudyData
{
    public UserStudyData(string name, uint trial, uint age, CameraType camType, Sex sex)
    {
        this.name = name;
        this.trial = trial;
        this.age = age;
        this.camType = camType;
        this.sex = sex;

        this.startFrame = null;
        this.endFrame = null;
        this.startTime = null;
        this.endTime = null;
        this.completionTime = null;
        this.trialCode = null;
    }

    public string name;
    public uint trial;
    public uint age;
    public CameraType camType;
    public Sex sex;

    public uint? startFrame;
    public uint? endFrame;

    //these are for calculating the timespan it took the user to complete a trial.
    public float? startTime;
    public float? endTime;
    public float? completionTime;

    public uint? trialCode;

    //we still need (nullable?) positions for start and target balls in here that can be set from the outside
}

public enum CameraType
{
    LeftRight,
    Top,
    Normal
}

public enum Sex
{
    Male,
    Female
}
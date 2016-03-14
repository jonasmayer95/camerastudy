using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.EventSystems;
using System.Text;
using System.Collections.Generic;

public class MovementRecorder : MonoBehaviour, IUserStudyMessageTarget
{
    private static uint frameCount;
    private List<StreamWriter> rawWriters;
    //private StreamWriter filteredWriter;
    public string filePath;
    public string fileName;

    public GameObject kinectAvatar;
    public GameObject art_IkAvatar;

    // We need two avatars to record simultaniously
    private List<GameObject> avatars;

    private List<AvatarController> controllers;
    //private static UserStudyData userData;
    private float recordStartTime;

    private static bool recordKinect;
    private static bool recordART_IK;


    void Start()
    {
        avatars = new List<GameObject>();
        if (recordART_IK)
        {
            avatars.Add(art_IkAvatar);
        }
        if (recordKinect)
        {
            avatars.Add(kinectAvatar);
            
        }
       
        controllers = new List<AvatarController>();
        rawWriters = new List<StreamWriter>();

        for (int i = 0; i < avatars.Count; i++)
        {
            if (avatars[i] != null)
            {
                controllers.Add(avatars[i].GetComponent<AvatarController>());

                if (controllers[i] == null)
                {
                    Debug.LogError("MovementRecorder: avatar gameObject doesn't contain an AvatarController component");
                }
                else
                {
                    //everything's fine, set up our filestream
                    //TODO: validate path
                    var date = DateTime.Now;
                    string path = filePath + avatars[i].name + date.Second.ToString() + date.Minute.ToString() +
                        date.Hour.ToString() + date.Day.ToString() + date.Month.ToString() + date.Year.ToString();
                    rawWriters.Add(new StreamWriter(path + ".csv"));
                    
                    //Store Last Recording as fallback value
                    PlayerPrefs.SetString("last" + avatars[i].name, path);
                    PlayerPrefs.Save();
                    //filteredWriter = new StreamWriter(path + "_filtered.csv");
                    recordStartTime = Time.time;
                    //write raw header to file
                    string rawHeader = string.Concat("current_time;", GetBoneDescriptions(controllers[i]));
                    //string detailedHeader = "";

                    rawWriters[i].WriteLine(rawHeader);
                    //filteredWriter.WriteLine(detailedHeader);
                }
            }
        }
        if(avatars == null)
        {
            Debug.LogError("MovementRecorder: avatar gameObject is null");
            this.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        for (int j = 0; j < controllers.Count; j++)
        {
            var bones = controllers[j].Bones;

            //write stuff that has been set through events, then get avatar movement data
            rawWriters[j].Write("{0};", /*userData.name, userData.set, userData.age, userData.camType, userData.sex,
            userData.trialCode, userData.startFrame, userData.endFrame, userData.completionTime.ToString(), frameCount,*/ Time.time - recordStartTime);


            for (int i = 0; i < bones.Length; ++i)
            {
                if (!bones[i])
                    continue;

                if (controllers[j].BoneIndex2JointMap.ContainsKey(i))
                {
                    //record bone position, rotation and timestamp
                    WriteBoneData(bones[i], rawWriters[j]);

                }
                else if (controllers[j].SpecIndex2JointMap.ContainsKey(i))
                {
                    var alJoints = (!controllers[j].mirroredMovement) ? controllers[j].SpecIndex2JointMap[i] : controllers[j].SpecIndex2MirrorJointMap[i];

                    if (alJoints.Count >= 2)
                    {
                        //record special bone position, rotation and timestamp
                        WriteBoneData(bones[i], rawWriters[j]);
                    }
                }
            }
            rawWriters[j].Write("\n");
            ++frameCount;
        }
    }


    void OnApplicationQuit()
    {
        EndRecording();
    }

    //call from UI to end recording
    public void EndRecording()
    {
        for (int i = 0; i < rawWriters.Count; i++)
        {
            if (rawWriters[i] != null)
            {
                rawWriters[i].Flush();
                rawWriters[i].Dispose();
            }
        }
        gameObject.SetActive(false);
        /*if (filteredWriter != null)
        {
            filteredWriter.Flush();
            filteredWriter.Dispose();
        }*/
    }

    public static void InitializeRecording(bool kinect, bool art_ik)
    {
        recordART_IK = art_ik;
        recordKinect = kinect;
    }

    /*public static void InitializeAndActivateUserStudy(string name, uint trial, uint set, uint age, CameraPerspectives camType, Sex sex)
    {
        userData = new UserStudyData(name, trial, set, age, camType, sex);

        //so the flow looks like this: UI calls this method and activates the object that contains our recorder.
        //recorder initializes 2 streamwriters and writes stuff every frame. certain properties are set from
        //the outside using events. some of them should be nullable so we don't write garbage before the experiment
        //has started.
    }*/


    private void WriteBoneData(Transform bone, StreamWriter writer)
    {
        if (writer != null)
        {
            writer.Write("\"{0},{1},{2},{3},{4},{5},{6}\";", bone.localPosition.x, bone.localPosition.y, bone.localPosition.z,
                        bone.localRotation.x, bone.localRotation.y, bone.localRotation.z, bone.localRotation.w);
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

    /*public void StartTrial(float startTime)
    {
        userData.startFrame = frameCount;
        userData.startTime = startTime;

        userData.endFrame = null;
        userData.endTime = null;
    }*/

    /*public void EndTrial(float endTime, Vector3 handPosition)
    {
        userData.endFrame = frameCount;
        userData.endTime = endTime;
        userData.completionTime = userData.endTime - userData.startTime;
        userData.handPosition = handPosition;


        string pos = userData.handPosition.Value.x.ToString() + ';' + userData.handPosition.Value.y.ToString() + ';'
            + userData.handPosition.Value.z.ToString();

        filteredWriter.WriteLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};", userData.name, userData.set, userData.age, userData.camType,
            userData.sex, userData.trialCode, userData.completionTime, pos));

        //flush after trial completion
        rawWriter.Flush();
        filteredWriter.Flush();
    }*/

    /*public void SetTrial(uint trialCode)
    {
        userData.trialCode = trialCode;

        Reset(ref userData);
    }

    public void SetCamera(CameraPerspectives cam)
    {
        userData.camType = cam;

        Reset(ref userData);
    }*/

    /*private void Reset(ref UserStudyData data)
    {
        data.startTime = null;
        data.startFrame = null;
        data.endTime = null;
        data.endFrame = null;
        data.completionTime = null;
        data.handPosition = null;
    }*/
}

public interface IUserStudyMessageTarget : IEventSystemHandler
{
    /// <summary>
    /// Called when the user puts his hand on the start marker. Should reset
    /// time and frame variables.
    /// </summary>
    /// <param name="startTime">Absolute time this was called (from program start).</param>
    ///void StartTrial(float startTime);

    /// <summary>
    /// Called when the user reaches the end marker. Should calculate and write
    /// time difference in milliseconds.
    /// </summary>
    /// <param name="endTime">Absolute time this was called (from program start).</param>
    ///void EndTrial(float endTime, Vector3 handPosition);

    /// <summary>
    /// Called when the trial (ball positions) should be changed. Should reset
    /// times and frames.
    /// </summary>
    /// <param name="trialCode"></param>
    ///void SetTrial(uint trialCode/*, Vector3 start, Vector3 end*/);

    /// <summary>
    /// Called when the camera pattern changes. Should (probably) reset
    /// local trial number.
    /// </summary>
    /// <param name="cam">The desired camera pattern.</param>
    ///void SetCamera(CameraPerspectives cam);
}

/*struct UserStudyData
{
    public UserStudyData(string name, uint trial, uint set, uint age, CameraPerspectives camType, Sex sex)
    {
        this.name = name;
        this.trial = trial;
        this.set = set;
        this.age = age;
        this.camType = camType;
        this.sex = sex;

        this.startFrame = null;
        this.endFrame = null;
        this.startTime = null;
        this.endTime = null;
        this.completionTime = null;
        this.trialCode = null;
        this.handPosition = null;
    }

    public string name;
    public uint trial;
    public uint age;
    public CameraPerspectives camType;
    public Sex sex;

    public uint? startFrame;
    public uint? endFrame;

    //these are for calculating the timespan it took the user to complete a trial.
    public float? startTime;
    public float? endTime;
    public float? completionTime;

    public uint? trialCode;
    public uint? set;

    public Vector3? handPosition;
}


public enum Sex
{
    Male,
    Female
}*/
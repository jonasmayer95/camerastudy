using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.EventSystems;

public class MovementRecorder : MonoBehaviour/*, UserStudyMessageTarget*/
{
    private static uint frameCount;
    private StreamWriter writer;
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
                    date.Hour.ToString() + date.Day.ToString() + date.Month.ToString() + date.Year.ToString() + ".txt";
                writer = new StreamWriter(filePath);
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
        ++frameCount;
        var bones = controller.Bones;

        //iterate over avatar bones, get the relevant stuff and write it to a file
        for (int i = 0; i < bones.Length; ++i)
        {
            if (!bones[i])
                continue;

            if (controller.BoneIndex2JointMap.ContainsKey(i))
            {
                KinectInterop.JointType joint = (!controller.mirroredMovement) ? controller.BoneIndex2JointMap[i] : controller.BoneIndex2MirrorJointMap[i];

                //TODO: record bone position, rotation and timestamp
            }
            else if (controller.SpecIndex2JointMap.ContainsKey(i))
            {
                var alJoints = (!controller.mirroredMovement) ? controller.SpecIndex2JointMap[i] : controller.SpecIndex2MirrorJointMap[i];

                if (alJoints.Count >= 2)
                {
                    //TODO: record special bone position, rotation and timestamp
                }
            }
        }
    }

    public static void InitializeAndActivateUserStudy(string name, uint trial, uint age, CameraType camType, Sex sex)
    {
        userData = new UserStudyData(name, trial, age, camType, sex);
        //this.gameObject.SetActive(true);
    }
}

public interface UserStudyMessageTarget : IEventSystemHandler
{
    void InitializeAndActivateUserStudy(string name, uint trial, uint age, CameraType camType, Sex sex);
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
    }

    public string name;
    public uint trial;
    public uint age;
    public CameraType camType;
    public Sex sex;
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
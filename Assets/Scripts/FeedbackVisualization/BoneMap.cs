using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoneMap : MonoBehaviour {

    public List<Transform> bones = new List<Transform>();
    private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
    //private AvatarController avatar;

    void Awake()
    {
        //avatar = gameObject.GetComponent<AvatarController>();
        foreach (Transform bone in bones)
        {
            if (!boneMap.ContainsKey(bone.name))
            {
                boneMap.Add(bone.name, bone);
            }
            else
            {
                Debug.Log("Bone already contained in BoneMap!");
            }
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public Dictionary<string, Transform> GetBoneMap()
    {
        return boneMap;
    }


    /// <summary>
    /// Search for bone with this name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Transform GetBone(string name)
    {
        return bones.Find(delegate(Transform bone)
        {
            return bone.gameObject.name == name;
        });
    }

    public static string GetBoneMapKey(string jointName, bool mirrored)
    {
        if (mirrored)
        {
            switch (jointName)
            {
                case "spinebase":
                    return "Spine";
                case "spinemid":
                    return "Spine.001";
                case "spineshoulder":
                    return "Neck";
                case "hipleft":
                    return "LeftHip";
                case "kneeleft":
                    return "LeftLeg";
                case "ankleleft":
                    return "LeftLowerLeg";
                case "footleft":
                    return "LeftFoot";
                case "toesleft":
                    return "LeftToes";
                case "hipright":
                    return "RightHip";
                case "kneeright":
                    return "RightLeg";
                case "ankleright":
                    return "RightLowerLeg";
                case "footright":
                    return "RightFoot";
                case "toesright":
                    return "RightToes";
                case "shoulderleft":
                    return "LeftShoulder";
                case "elbowleft":
                    return "LeftForeArm";
                //    return "LeftArm";
                case "wristleft":
                    return "LeftHand";
                case "handleft":
                    return "LeftHand";
                case "fingersleft":
                    return "LeftHand";
                case "thumbleft":
                    return "LeftThumb";
                case "shoulderright":
                    return "RightShoulder";
                case "elbowright":
                    return "RightForeArm";
                //    return "RightArm";
                case "wristright":
                    return "RightHand";
                case "handright":
                    return "RightHand";
                case "fingersright":
                    return "RightFingers";
                case "thumbright":
                    return "RightTumb";
                case "neck":
                    return "Head";
                case "head":
                    return "Head_end";
                default: return "not found";
            }
        }
        else
        {
            switch (jointName)
            {
                case "spinebase":
                    return "Spine";
                case "spinemid":
                    return "Spine.001";
                case "spineshoulder":
                    return "Neck";
                case "hipleft":
                    return "RightHip";
                case "kneeleft":
                    return "RightLeg";
                case "ankleleft":
                    return "RightLowerLeg";
                case "footleft":
                    return "RightFoot";
                case "toesleft":
                    return "RightToes";
                case "hipright":
                    return "LeftHip";
                case "kneeright":
                    return "LeftLeg";
                case "ankleright":
                    return "LeftLowerLeg";
                case "footright":
                    return "LeftFoot";
                case "toesright":
                    return "LeftToes";
                case "shoulderleft":
                    return "RightShoulder";
                case "elbowleft":
                    return "RightForeArm";
                //    return "RightArm";
                //case "wristleft":
                //    return "RightForeArm";
                case "handleft":
                    return "RightHand";
                case "fingersleft":
                    return "RightFingers";
                case "thumbleft":
                    return "RightThumb";
                case "shoulderright":
                    return "LeftShoulder";
                case "elbowright":
                    return "LeftArm";
                case "wristright":
                    return "LeftForeArm";
                case "handright":
                    return "LeftHand";
                case "fingersright":
                    return "LeftFingers";
                case "thumbright":
                    return "LeftTumb";
                case "neck":
                    return "Head";
                case "head":
                    return "Head_end";
                default: return "not found";
            }
        }
    }

    public static string KinectJointToInseiJointName(string name)
    {
        switch (name)
        {
            case "SpineBase":
                return "spinebase";
            case "SpineMid":
                return "spinemid";
            case "Neck":
                return "neck";
            case "Head":
                return "head";
            case "ShoulderLeft":
                return "shoulderleft";
            case "ElbowLeft":
                return "elbowleft";
            case "WristLeft":
                return "handleft";
            case "HandLeft":
                return "fingersleft";
            case "ElbowRight":
                return "elbowright";
            case "WristRight":
                return "handright";
            case "HandRight":
                return "fingersright";
            case "HipLeft":
                return "hipleft";
            case "KneeLeft":
                return "kneeleft";
            case "AnkleLeft":
                return "ankleleft";
            case "FootLeft":
                return "footleft";
            case "HipRight":
                return "hipright";
            case "KneeRight":
                return "kneeright";
            case "AnkleRight":
                return "ankleright";
            case "FootRight":
                return "footright";
            case "SpineShoulder":
                return "spineshoulder";
            default: return "not found";
        }
    }

    public static string TransformNameToKinectName(string name)
    {
        switch (name)
        {
            case "Spine":
                return "SpineBase";
            case "Spine.001":
                return "SpineMid";
            case "Neck":
                return "SpineShoulder";
            case "Head":
                return "Neck";
            case "Head_end":
                return "Head";
            case "LeftArm":
                return "ShoulderLeft";
            case "LeftForeArm":
                return "ElbowLeft";
            case "LeftHand":
                return "WristLeft";
            case "LeftFingers":
                return "HandLeft";
            case "RightForeArm":
                return "ElbowRight";
            case "RightHand":
                return "WristRight";
            case "RightFingers":
                return "HandRight";
            case "LeftHip":
                return "HipLeft";
            case "LeftLeg":
                return "KneeLeft";
            case "LeftLowerLeg":
                return "AnkleLeft";
            case "LeftFoot":
                return "FootLeft";
            case "RightHip":
                return "HipRight";
            case "RightLeg":
                return "KneeRight";
            case "RightLowerLeg":
                return "AnkleRight";
            case "RightFoot":
                return "FootRight";
            default: return "not found";
        }
    }
}
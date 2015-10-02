using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoneMap : MonoBehaviour {

    public List<Transform> bones = new List<Transform>();
    private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
    private AvatarController avatar;

    void Awake()
    {
        avatar = gameObject.GetComponent<AvatarController>();
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
                //case "wristleft":
                //    return "LeftForeArm";
                case "handleft":
                    return "LeftHand";
                case "fingersleft":
                    return "LeftFingers";
                case "thumbleft":
                    return "LeftThumb";
                case "shoulderright":
                    return "RightShoulder";
                case "elbowright":
                    return "RightForeArm";
                //    return "RightArm";
                //case "wristright":
                //    return "RightForeArm";
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
}

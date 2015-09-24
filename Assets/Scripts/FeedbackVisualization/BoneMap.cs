using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoneMap : MonoBehaviour {

    public List<Transform> bones = new List<Transform>();
    private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

    void Awake()
    {
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

    public string GetBoneMapKey(string jointName)
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
                return "LeftArm";
            case "wristleft":
                return "LeftForeArm";
            case "handleft":
                return "LeftHand";
            case "fingersleft":
                return "LeftFingers";
            case "thumbleft":
                return "LeftThumb";
            case "shoulderright":
                return "RightShoulder";
            case "elbowright":
                return "RightArm";
            case "wristright":
                return "RightForeArm";
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
}

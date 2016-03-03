using UnityEngine;
using System.Collections;

public class KinectBasicUpdater : MonoBehaviour {
    private AvatarController controller;
    public AvatarController kinectController;
	// Use this for initialization
	void Start () {
        controller = GetComponent<AvatarController>();
	}

    //if I change this to update, IK works but the avatar doesn't move.
    void Update()
    {
        //iterate over all bones, set positions as in the kinect-driven avatar
        for (int i = 0; i < kinectController.Bones.Length; ++i)
        {
            //TODO: write this in a less ugly way
            if (controller.Bones[i] != null)
            {
                if (controller.BoneIndex2MirrorJointMap.ContainsKey(i) && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ElbowRight
                && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.WristRight && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.HandRight
                    /*&& controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ShoulderRight*/)
                {
                    controller.Bones[i].position = kinectController.Bones[i].position;
                    controller.Bones[i].rotation = kinectController.Bones[i].rotation;
                }
            }
        }
    }
}

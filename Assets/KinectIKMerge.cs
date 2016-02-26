using UnityEngine;
using System.Collections;

public class KinectIKMerge : MonoBehaviour
{
    private AvatarController kinectController;
    private AvatarController ikController;
    private AvatarController controller;
    public GameObject kinectAvatar;
    public GameObject ikAvatar;


    // Use this for initialization
    void Start()
    {
        kinectController = kinectAvatar.GetComponent<AvatarController>();
        ikController = ikAvatar.GetComponent<AvatarController>();
        controller = this.GetComponent<AvatarController>();
    }

    //if I change this to update, IK works but the avatar doesn't move.
    void LateUpdate()
    {
        //iterate over all bones, set positions as in the kinect-driven avatar
        for (int i = 0; i < kinectController.Bones.Length; ++i)
        {
            //TODO: write this in a less ugly way
            if (controller.Bones[i] != null)
            {
                if (controller.BoneIndex2MirrorJointMap.ContainsKey(i) && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ElbowRight
                && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.WristRight && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.HandRight
                    && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ElbowLeft
                && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.WristLeft && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.HandLeft
                    /*&& controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ShoulderRight*/)
                {
                    controller.Bones[i].position = kinectController.Bones[i].position;
                    controller.Bones[i].rotation = kinectController.Bones[i].rotation;
                }
                else
                {
                    controller.Bones[i].localRotation = ikController.Bones[i].localRotation;
                }
            }
        }
    }
}

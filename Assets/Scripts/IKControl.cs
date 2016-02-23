using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class IKControl : MonoBehaviour
{
    public Transform targetPoint;
    public GameObject kinectAvatar;
    public bool ikActive = false;

    private Animator anim;
    private AvatarController kinectController;
    private AvatarController controller;


    void Start()
    {
        anim = GetComponent<Animator>();
        kinectController = kinectAvatar.GetComponent<AvatarController>();
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
                    /*&& controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ShoulderRight*/)
                {
                    controller.Bones[i].position = kinectController.Bones[i].position;
                    controller.Bones[i].rotation = kinectController.Bones[i].rotation;
                }
            }
        }
    }

    void OnAnimatorIK()
    {
        if (ikActive)
        {
            if (targetPoint != null)
            {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                //anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                anim.SetIKPosition(AvatarIKGoal.LeftHand, targetPoint.position);
                //anim.SetIKRotation(AvatarIKGoal.LeftHand, targetPoint.rotation);
            }
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            //anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }
}

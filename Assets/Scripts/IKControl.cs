using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]

public class IKControl : MonoBehaviour
{
    public Transform leftHand;
    public Transform leftHint;
    public Transform rightHand;
    public Transform rightHint;
    public Transform kinectBase;
    public GameObject kinectAvatar;
    public bool ikActive = false;

    private Animator anim;
    private AvatarController kinectController;
    private AvatarController controller;
    public AnimationClip clip;
    private List<AnimationCurve[]> curves;

    void Start()
    {
        anim = GetComponent<Animator>();
        kinectController = kinectAvatar.GetComponent<AvatarController>();
        controller = this.GetComponent<AvatarController>();
        curves = new List<AnimationCurve[]>();
        for (int i = 0; i < kinectController.Bones.Length; ++i)
        {
            curves.Add(new AnimationCurve[7]);
            if (controller.Bones[i] != null)
            {
                if (controller.BoneIndex2MirrorJointMap.ContainsKey(i) && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ElbowRight
                && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.WristRight && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.HandRight
                    /*&& controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ShoulderRight*/)
                {
                    
                    for (int k = 0; k < 7; k++)
                    {
                        curves[i][k] = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1,0));                        
                    }
                    curves[i][0].AddKey(0, kinectController.Bones[i].localPosition.x);
                    clip.SetCurve("", typeof(Transform), "localPosition.x", curves[i][0]);
                    curves[i][1].AddKey(0, kinectController.Bones[i].localPosition.y);
                    clip.SetCurve("", typeof(Transform), "localPosition.y", curves[i][1]);
                    curves[i][2].AddKey(0, kinectController.Bones[i].localPosition.z);
                    clip.SetCurve("", typeof(Transform), "localPosition.z", curves[i][2]);
                    curves[i][3].AddKey(0, kinectController.Bones[i].localRotation.eulerAngles.x);
                    clip.SetCurve("", typeof(Transform), "localRotation.x", curves[i][3]);
                    curves[i][4].AddKey(0, kinectController.Bones[i].localRotation.eulerAngles.y);
                    clip.SetCurve("", typeof(Transform), "localRotation.y", curves[i][4]); 
                    curves[i][5].AddKey(0, kinectController.Bones[i].localRotation.eulerAngles.z);
                    clip.SetCurve("", typeof(Transform), "localRotation.z", curves[i][5]);
                    curves[i][6].AddKey(0, kinectController.Bones[i].localRotation.w);
                    clip.SetCurve("", typeof(Transform), "localRotation.w", curves[i][6]);
                }
            }
        }
    }

    //if I change this to update, IK works but the avatar doesn't move.
    //void Update()
    //{
    //    //iterate over all bones, set positions as in the kinect-driven avatar
    //    for (int i = 0; i < kinectController.Bones.Length; ++i)
    //    {
    //        //TODO: write this in a less ugly way
    //        if (controller.Bones[i] != null)
    //        {
    //            if (controller.BoneIndex2MirrorJointMap.ContainsKey(i) && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ElbowRight
    //            && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.WristRight && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.HandRight
    //                /*&& controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ShoulderRight*/)
    //            {
    //                controller.Bones[i].position = kinectController.Bones[i].position;
    //                controller.Bones[i].rotation = kinectController.Bones[i].rotation;
    //            }
    //        }
    //    }
    //}

    void Update()
    {
        transform.position = new Vector3(kinectBase.position.x, kinectBase.position.y + 1.108f, kinectBase.position.z);
        clip.ClearCurves();
        //Write kinect Data into animation clip
        //iterate over all bones, set positions as in the kinect-driven avatar
        for (int i = 0; i < kinectController.Bones.Length; ++i)
        {
            if (controller.Bones[i] != null)
            {
                if (controller.BoneIndex2MirrorJointMap.ContainsKey(i) && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ElbowRight
                && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.WristRight && controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.HandRight
                    /*&& controller.BoneIndex2MirrorJointMap[i] != KinectInterop.JointType.ShoulderRight*/)
                {
                    curves[i][0].keys[0].value = kinectController.Bones[i].localPosition.x;
                    curves[i][1].keys[0].value = kinectController.Bones[i].localPosition.y;
                    curves[i][2].keys[0].value = kinectController.Bones[i].localPosition.z;
                    curves[i][3].keys[0].value = kinectController.Bones[i].localRotation.x;
                    curves[i][4].keys[0].value = kinectController.Bones[i].localRotation.y;
                    curves[i][5].keys[0].value = kinectController.Bones[i].localRotation.z;
                    curves[i][6].keys[0].value = kinectController.Bones[i].localRotation.w;
                    curves[i][0].keys[1].value = kinectController.Bones[i].localPosition.x;
                    curves[i][1].keys[1].value = kinectController.Bones[i].localPosition.y;
                    curves[i][2].keys[1].value = kinectController.Bones[i].localPosition.z;
                    curves[i][3].keys[1].value = kinectController.Bones[i].localRotation.x;
                    curves[i][4].keys[1].value = kinectController.Bones[i].localRotation.y;
                    curves[i][5].keys[1].value = kinectController.Bones[i].localRotation.z;
                    curves[i][6].keys[0].value = kinectController.Bones[i].localRotation.w;
                    clip.SetCurve("", typeof(Transform), "localPosition.x", curves[i][0]);
                    clip.SetCurve("", typeof(Transform), "localPosition.y", curves[i][1]);
                    clip.SetCurve("", typeof(Transform), "localPosition.z", curves[i][2]);
                    clip.SetCurve("", typeof(Transform), "localRotation.x", curves[i][3]);
                    clip.SetCurve("", typeof(Transform), "localRotation.y", curves[i][4]);
                    clip.SetCurve("", typeof(Transform), "localRotation.z", curves[i][5]);
                    clip.SetCurve("", typeof(Transform), "localRotation.w", curves[i][6]);
                }
            }
        }
    }

    void OnAnimatorIK()
    {
        if (ikActive)
        {
            
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                //anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.position);
                //anim.SetIKRotation(AvatarIKGoal.LeftHand, targetPoint.rotation);
                anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                anim.SetIKHintPosition(AvatarIKHint.LeftElbow, leftHint.position);


                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                //anim.SetIKRotationWeight(AvatarIKGoal.rightHand, 1);
                anim.SetIKPosition(AvatarIKGoal.RightHand, rightHand.position);
                //anim.SetIKRotation(AvatarIKGoal.rightHand, targetPoint.rotation);
                anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                anim.SetIKHintPosition(AvatarIKHint.RightElbow, rightHint.position);
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            //anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class IKControl : MonoBehaviour
{
    public Transform targetPoint;
    public bool ikActive = false;

    private Animator anim;


    void Start()
    {
        anim = GetComponent<Animator>();
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

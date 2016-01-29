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
                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                anim.SetIKPosition(AvatarIKGoal.RightHand, targetPoint.position);
                anim.SetIKRotation(AvatarIKGoal.RightHand, targetPoint.rotation);
            }
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }
    }
}

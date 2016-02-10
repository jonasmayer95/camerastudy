using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarFeedback : MonoBehaviour {

    public Transform avatarToMirror;
    public Vector3 avatarOffset;
    public float userHeight = 1.8f;
    public bool mirroringAvatar;
    public float threshold = 0.2f;
    public Transform leftEllbow;
    public Transform rightEllbow;
    public Transform leftHand;
    public Transform rightHand;
    public List<Transform> ownBones = new List<Transform>();
    private List<Transform> otherBones = new List<Transform>();
    public Vector3[] relHandPosLeft = new Vector3[2];
    public Vector3[] relHandPosRight = new Vector3[2];
    public Vector3[] relEllbowPosLeft = new Vector3[1];
    public Vector3[] relEllbowPosRight = new Vector3[1];
    private bool correctingLeftArm;
    private bool correctingRightArm;
    public BallFeedback leftHandBall, rightHandBall, leftEllbowBall, rightEllbowBall;

    protected Animator animator;
    private Vector3 RightElbowtargetPos;
    private Vector3 LeftElbowtargetPos;

	// Use this for initialization
	void Start () {

        otherBones = avatarToMirror.GetComponent<BoneMap>().bones;
        avatarOffset = transform.position - avatarToMirror.position;

        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {

        if (mirroringAvatar)
        {
            // Spine and Left Leg
            for (int i = 0; i < 6; i++)
            {
                ownBones[i].localPosition = otherBones[i].localPosition;
                ownBones[i].localRotation = otherBones[i].localRotation;
            }

            // Right Leg and spine 001
            for (int i = 6; i < 12; i++)
            {
                ownBones[i].localPosition = otherBones[i].localPosition;
                ownBones[i].localRotation = otherBones[i].localRotation;
            }

            if (!correctingLeftArm)
            {
                // Left Shoulder
                for (int i = 12; i < 20; i++)
                {
                    ownBones[i].localPosition = otherBones[i].localPosition;
                    ownBones[i].localRotation = otherBones[i].localRotation;
                }

            }
            else
            {
                LeftElbowtargetPos = transform.position + relEllbowPosRight[0];
            }
           

            // Neck and Head
            for (int i = 20; i < 23; i++)
            {
                ownBones[i].localPosition = otherBones[i].localPosition;
                ownBones[i].localRotation = otherBones[i].localRotation;
            }

            if (!correctingRightArm)
            {
                // Right Shoulder
                for (int i = 23; i < 31; i++)
                {
                    ownBones[i].localPosition = otherBones[i].localPosition;
                    ownBones[i].localRotation = otherBones[i].localRotation;
                }
            }
            else
            {
                RightElbowtargetPos = transform.position + relEllbowPosLeft[0];
                
            }
           
        }

        if (Vector3.Distance(leftEllbowBall.transform.position, leftEllbow.position) >= threshold)
        {
            correctingLeftArm = true;
           
        }
        else
        {
            correctingLeftArm = false;
        }

        if (Vector3.Distance(rightEllbowBall.transform.position, rightEllbow.position) >= threshold)
        {
            correctingRightArm = true;
        }
        else
        {
            correctingRightArm = false;
        }
        
        transform.position = avatarToMirror.position + avatarOffset;	
	}

    void OnAnimatorIK()
    {
        if (correctingLeftArm)
        {

            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
            animator.SetIKHintPosition(AvatarIKHint.LeftElbow, LeftElbowtargetPos);
        }

        else
        {
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
        }

        if (correctingRightArm)
        {
            //animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            //animator.SetIKPosition(AvatarIKGoal.RightHand, transform.position + relHandPosLeft[0]);                


            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, RightElbowtargetPos);
            Debug.Log(RightElbowtargetPos);

            Debug.DrawLine(RightElbowtargetPos, transform.position);
            //Debug.Log(RightElbowtargetPos);
        }
        else
        {
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
        }
    }
}

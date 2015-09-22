using UnityEngine;
using System.Collections;

public class CircularProgressFeedback : InseilFeedback {

    private float revealOffset;
    private SpriteRenderer rend;

	// Use this for initialization
	void Start () {

        rend = GetComponent<SpriteRenderer>();	
	}
	
	// Update is called once per frame
	void Update () {

        rend.material.SetFloat("_Cutoff", revealOffset );
	}

    public override void InitFeedback(StaticJoint joint, Transform relTo, BoneMap bones)
    {
        throw new System.NotImplementedException();
    }

    public override void InitFeedback(MotionJoint joint, Transform relTo, BoneMap bones)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateLoadingCircle(float percentage)
    {
        revealOffset = percentage;
    }
}

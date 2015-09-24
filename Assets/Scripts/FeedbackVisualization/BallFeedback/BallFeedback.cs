using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallFeedback : InseilFeedback {

    public Transform relToObject;
    public List<Vector3> positions = new List<Vector3>();
    public Vector3 scale;
    public GameObject joint;
    public GameObject loadingCircle;
    public bool showBall;
    public float holdingTime;
    public Color colorClose;
    public Color colorFar;
    public float fadeDistance;
    public GameObject particles;   
    public float particlesLifeTime;
    public int positionChanges;
    private Vector3 relPos;
    private int index = 0;
    private CircularProgressFeedback circle;
    private float currHoldingTime;
    private Renderer ballRenderer;
    private bool motionjoint;
    

	// Use this for initialization
	void Start () {

        InitBallFeedback();	
	}
	
	// Update is called once per frame
	void Update () {

        transform.position = relPos + positions[index];
        ballRenderer.enabled = showBall;

        float distance = (joint.transform.position - transform.position).sqrMagnitude;

        if (distance <= fadeDistance)
        {
            float transparencyValue = distance / fadeDistance;

            if (transparencyValue >= 0.25f)
            {
                ballRenderer.material.color = new Color(ballRenderer.material.color.r, ballRenderer.material.color.g, ballRenderer.material.color.b, transparencyValue);
            }
        }
        else
        {
            ballRenderer.material.color = new Color(ballRenderer.material.color.r, ballRenderer.material.color.g, ballRenderer.material.color.b, 1);
        }
	}

    public override void InitFeedback(StaticJoint joint, Transform relTo, BoneMap bones)
    {
        motionjoint = false;
        positions.Add(joint.targetPosition);
        relToObject = relTo;

        colorFar = colorClose = Color.blue;

        Transform bone;
        bones.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(joint.joint), out bone);
        this.joint = bone.gameObject;
    }

    public override void InitFeedback(MotionJoint joint, Transform relTo, BoneMap bones)
    {
        motionjoint = true;
        positions.Add(joint.startPosition);
        positions.Add(joint.endPosition);
        relToObject = relTo;

        Transform bone;
        bones.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(joint.joint), out bone);
        this.joint = bone.gameObject;
    }

    void OnEnable()
    {
        //InitBallFeedback();
    }

    private void InitBallFeedback()
    {
        relPos = relToObject.position;
        transform.position = relPos + positions[0];

        ballRenderer = GetComponent<Renderer>();
        //transform.localScale = scale;

        ballRenderer.material.color = colorFar;

        if (loadingCircle != null)
        {
            GameObject c = Instantiate(loadingCircle, transform.position, Quaternion.identity) as GameObject;
            c.transform.parent = transform;
            circle = c.GetComponent<CircularProgressFeedback>();
            currHoldingTime = holdingTime;
            circle.UpdateLoadingCircle(1);

            if (!showBall)
            {
                ballRenderer.enabled = false;
            }
        }
    }

    public void ResetCounter()
    {
        positionChanges = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject == joint && loadingCircle == null)
        if (other.gameObject == joint && motionjoint)
        {
            index = (index + 1) % positions.Count;
            positionChanges++;
            if (particles != null)
            {
                Destroy(Instantiate(particles, transform.position, Quaternion.identity), particlesLifeTime);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == joint)
        {
            //ballRenderer.material.color = colorFar;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == joint && loadingCircle != null && circle != null)
        {
            currHoldingTime -= Time.deltaTime;
            float percentage = currHoldingTime / holdingTime;
            circle.UpdateLoadingCircle(percentage);

            //ballRenderer.material.color = colorClose;

            if (currHoldingTime <= 0)
            {
                index = (index + 1) % positions.Count;
                currHoldingTime = holdingTime;
                percentage = currHoldingTime / holdingTime;
                circle.UpdateLoadingCircle(percentage);
                //ballRenderer.material.color = colorFar;
                positionChanges++;
            }
        }
    }
}

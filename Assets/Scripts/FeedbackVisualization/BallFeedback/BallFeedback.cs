using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallFeedback : InseilFeedback {

    public Transform relToObject;
    public Vector3[] positions;
    public Vector3 scale;
    public GameObject joint;
    public GameObject loadingCircle;
    public bool showBall;
    public float holdingTime;
    public GameObject particles;   
    public float particlesLifeTime;
    private Vector3 relPos;
    private int index = 0;
    private CircularProgressFeedback circle;
    private float currHoldingTime;
    private Renderer ballRenderer;
    

	// Use this for initialization
	void Start () {

        relPos = relToObject.position;
        transform.position = relPos + positions[0];
        //transform.localScale = scale;

        if (loadingCircle != null)
        {
            GameObject c = Instantiate(loadingCircle, transform.position, Quaternion.identity) as GameObject;
            c.transform.parent = transform;
            circle = c.GetComponent<CircularProgressFeedback>();
            currHoldingTime = holdingTime;
            circle.UpdateLoadingCircle(1);

            ballRenderer = GetComponent<Renderer>();
            ballRenderer.material.color = Color.red;

            if (!showBall)
            {               
                ballRenderer.enabled = false;
            }
        }
	
	}
	
	// Update is called once per frame
	void Update () {

        transform.position = relPos + positions[index];
        ballRenderer.enabled = showBall;
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == joint && loadingCircle == null)
        {
            index = (index + 1) % positions.Length;
            if (particles != null)
            {
                Destroy(Instantiate(particles, transform.position, Quaternion.identity), particlesLifeTime);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == joint && loadingCircle != null)
        {
            ballRenderer.material.color = Color.red;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == joint && loadingCircle != null)
        {
            currHoldingTime -= Time.deltaTime;
            float percentage = currHoldingTime / holdingTime;
            circle.UpdateLoadingCircle(percentage);

            ballRenderer.material.color = Color.green;

            if (currHoldingTime <= 0)
            {
                index = (index + 1) % positions.Length;
                currHoldingTime = holdingTime;
                percentage = currHoldingTime / holdingTime;
                circle.UpdateLoadingCircle(percentage);
                ballRenderer.material.color = Color.red;
            }
        }
    }
}

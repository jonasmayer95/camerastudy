using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallFeedback : MonoBehaviour {

    public Vector3[] positions;
    public Vector3 scale;
    public GameObject joint;
    public GameObject particles;
    public float particlesLifeTime;
    private int index = 0;

	// Use this for initialization
	void Start () {

        transform.position = positions[0];
        transform.localScale = scale;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == joint)
        {
            index = (index + 1) % positions.Length;
            if (particles != null)
            {
                Destroy(Instantiate(particles, transform.position, Quaternion.identity), particlesLifeTime);
            }
            transform.position = positions[index];
        }
    }
}

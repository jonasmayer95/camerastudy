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

    public void UpdateLoadingCircle(float percentage)
    {
        revealOffset = percentage;
    }
}

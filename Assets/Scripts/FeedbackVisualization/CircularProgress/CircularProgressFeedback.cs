using UnityEngine;
using System.Collections;

public class CircularProgressFeedback : MonoBehaviour {

    float revealOffset;
    private SpriteRenderer rend;

	// Use this for initialization
	void Start () {

        rend = GetComponent<SpriteRenderer>();	
	}
	
	// Update is called once per frame
	void Update () {

        revealOffset = (float)(Time.timeSinceLevelLoad % 10) / 10.1f;

        rend.material.SetFloat("_Cutoff", revealOffset );
	}
}

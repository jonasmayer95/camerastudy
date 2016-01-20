using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BicepsCurl : MonoBehaviour {

    public BallFeedback leftHand, rightHand, leftEllbow, rightEllbow;
    public GameObject smallOrbWayPointPrefab;
    public int numOrbs;

    Vector3 centerPosLeft;
    Vector3 centerPosRight;
    Vector3 startPosLeft;
    Vector3 startPosRight;
    Vector3 endPosLeft;
    Vector3 endPosRight;

	// Use this for initialization
	void Start () {

        SpawnOrbs();	
	}
	
	// Update is called once per frame
	void Update () {

        Debug.DrawLine(centerPosLeft, endPosLeft);
        Debug.DrawLine(centerPosLeft, startPosLeft);

        Debug.DrawLine(centerPosRight, endPosRight);
        Debug.DrawLine(centerPosRight, startPosRight);
	}

    public void SpawnOrbs()
    {
        centerPosLeft = leftEllbow.relToObject.position + leftEllbow.positions[0] ;
        centerPosRight = rightEllbow.relToObject.position + rightEllbow.positions[0] ;

        startPosLeft = leftHand.relToObject.position + leftHand.positions[0] ;
        startPosRight = rightHand.relToObject.position + rightHand.positions[0] ;

        endPosLeft = leftHand.relToObject.position + leftHand.positions[1] ;
        endPosRight = rightHand.relToObject.position + rightHand.positions[1] ;

        Vector3 dirToStartPosLeft = startPosLeft - centerPosLeft;
        Vector3 dirToStartPosRight = startPosRight - centerPosRight;

        Vector3 dirToEndPosLeft = endPosLeft - centerPosLeft;
        Vector3 dirToEndPosRight = endPosRight - centerPosRight;

       

        float totalAngleLeft = Vector3.Angle(dirToStartPosLeft, dirToEndPosLeft);
        float totalAngleRight = Vector3.Angle(dirToStartPosRight, dirToEndPosRight);

        float angleStepLeft = totalAngleLeft / numOrbs;
        float angleStepRight = totalAngleRight / numOrbs;

        for (int i = 0; i < numOrbs; i++)
        {
            GameObject sphere = (GameObject) Instantiate(smallOrbWayPointPrefab, centerPosLeft + Vector3.RotateTowards(dirToStartPosLeft, dirToEndPosLeft, (i * angleStepLeft) * Mathf.Deg2Rad, 0.0f), Quaternion.identity);
            sphere.transform.parent = transform;
        }

        for (int i = 0; i < numOrbs; i++)
        {
            GameObject sphere = (GameObject)Instantiate(smallOrbWayPointPrefab, centerPosRight + Vector3.RotateTowards(dirToStartPosRight, dirToEndPosRight, (i * angleStepLeft) * Mathf.Deg2Rad, 0.0f), Quaternion.identity);
            sphere.transform.parent = transform;
        }
    }    
}

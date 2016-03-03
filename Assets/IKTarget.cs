using UnityEngine;
using System.Collections;

public class IKTarget : MonoBehaviour {

    public Transform ikTarget;

	// Use this for initialization
	void Start () {
        transform.position = ikTarget.position;
        transform.rotation = ikTarget.rotation;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = ikTarget.position;
        transform.rotation = ikTarget.rotation;
	}
}

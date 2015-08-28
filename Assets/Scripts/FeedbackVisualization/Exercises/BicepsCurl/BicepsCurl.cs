using UnityEngine;
using System.Collections;
using System.IO;

public class BicepsCurl : InseilExercise {

    Vector3 startPosition;
    Vector3 endPosition;
    public int sizeOfSet;
    public Transform wrist;
    BallFeedback targetBall;
    public string namePerson;
    public int numSet;

    private bool oneShot;

    StreamWriter sw;

	// Use this for initialization
	void Start () {

        // Get Feedback Method: BallFeedback
        targetBall = (BallFeedback) FeedbackManager.instance.GetFeedbackType(1);

        // Init positions
        targetBall.positions.Clear();
        targetBall.positions.Add(startPosition);
        targetBall.positions.Add(endPosition);

        // Init moving bone
        targetBall.joint = wrist.gameObject;

        // Init ballScale
        targetBall.scale = new Vector3(0.25f, 0.25f, 0.25f);

        string path = Application.dataPath;
        Debug.Log(path);
        sw = new StreamWriter("BicepsCurl.txt");
        sw.WriteLine(namePerson + ", " + numSet + ", " + targetBall.positionChanges + ", " + startPosition.x + ", " + startPosition.y + ", " + startPosition.z + ", "
                                                        + endPosition.x + ", "
                                                        + endPosition.y + ", "
                                                        + endPosition.z + ", "
                                                        + wrist.position.x + ", "
                                                        + wrist.position.y + ", "
                                                        + wrist.position.z);

        sw.Flush();
	}
	
	// Update is called once per frame
	void Update () {

        while (targetBall.positionChanges < sizeOfSet && !oneShot)
        {
            PrintBicepsCurlInfo2();
            oneShot = true;
        }	


	}

    void PrintBicepsCurlInfo1()
    {
        // Writing name, set, rep, start, end, wrist.pos into file
        string path = Application.dataPath;
        
        System.IO.File.WriteAllText(path + "/Exercises/BicepsCurl.txt", namePerson + ", "
                                                    + numSet + ", "
                                                    + targetBall.positionChanges + ", "
                                                    + startPosition.x + ", "
                                                    + startPosition.y + ", "
                                                    + startPosition.z + ", "
                                                    + endPosition.x + ", "
                                                    + endPosition.y + ", "
                                                    + endPosition.z + ", "
                                                    + wrist.position.x + ", "
                                                    + wrist.position.y + ", "
                                                    + wrist.position.z + "/n");
    }

    void PrintBicepsCurlInfo2()
    {
        if (sw != null)
        {
            Debug.Log("Write to file");
            sw.WriteLine(namePerson + ", " + numSet + ", " + targetBall.positionChanges + ", " + startPosition.x + ", " + startPosition.y + ", " + startPosition.z + ", "
                                                        + endPosition.x + ", "
                                                        + endPosition.y + ", "
                                                        + endPosition.z + ", "
                                                        + wrist.position.x + ", "
                                                        + wrist.position.y + ", "
                                                        + wrist.position.z);
        }
    }
}

using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class BicepsCurl : InseilExercise {

    public int sizeOfSet;
    public string nameOfPerson;
    public int set;
    public Transform printPosRelToJoint;
    private List<InseilFeedback> feedbackList = new List<InseilFeedback>();

    private List<StreamWriter> sw = new List<StreamWriter>();

	// Use this for initialization
	void Start () {

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            InseilFeedback iFB = child.GetComponent<InseilFeedback>();

            feedbackList.Add(iFB);
            sw.Add(new StreamWriter("BicepsCurl_" + i + "_" + iFB.type + ".txt"));
        }

        FeedbackManager.instance.AddExercise(this);
	}
	
	// Update is called once per frame
	void Update () {

        for (int i = 0; i < feedbackList.Count; i++)
        {
            InseilFeedback iFB = feedbackList[i];
            if (iFB.type == enabledFeedBackType)
            {
                iFB.gameObject.SetActive(true);

                // Print ballFeedback info
                if (iFB.type == FeedbackType.BallFeedback)
                {
                    BallFeedback targetBall = (BallFeedback)iFB;

                    // Check for end of set
                    if (targetBall.positionChanges / 2 < sizeOfSet)
                    {
                        PrintBicepsCurlInfo(targetBall, i);
                    }
                }

                // Print areaFeedback info
                if (iFB.type == FeedbackType.AreaFeedback)
                {
                    AreaFeedback area = (AreaFeedback)iFB;

                    // ToDo: Print Info
                    PrintBicepsCurlInfo(area, i);
                }

                // Print imageFeedback info
                if (iFB.type == FeedbackType.ImageFeedback)
                {
                    ImageFeedback3D image = (ImageFeedback3D)iFB;

                    // ToDO: Print Info
                    PrintBicepsCurlInfo(image, i);
                }
            }

            else
            {
                iFB.gameObject.SetActive(false);
            }
        }
	}


    void PrintBicepsCurlInfo(BallFeedback targetBall, int fileIndex)
    {
       
        if (sw[fileIndex] != null)
        {
            int repetitions = targetBall.positionChanges / 2;
            sw[fileIndex].WriteLine(nameOfPerson + ", " + set + ", "
                        + repetitions + ", "
                        + (targetBall.positions[0].x - printPosRelToJoint.position.x) + ", " + (targetBall.positions[0].y - printPosRelToJoint.position.y) + ", " + (targetBall.positions[0].z - printPosRelToJoint.position.z) + ", "
                        + (targetBall.positions[1].x - printPosRelToJoint.position.x) + ", " + (targetBall.positions[1].y - printPosRelToJoint.position.y) + ", " + (targetBall.positions[1].z - printPosRelToJoint.position.z) + ", "
                        + (targetBall.joint.transform.position.x - printPosRelToJoint.position.x) + ", " + (targetBall.joint.transform.position.y - printPosRelToJoint.position.y) + ", " + (targetBall.joint.transform.position.z - printPosRelToJoint.position.z));

            sw[fileIndex].Flush();
        }
    }

    void PrintBicepsCurlInfo(AreaFeedback area, int fileIndex)
    {

    }

    void PrintBicepsCurlInfo(ImageFeedback3D image, int fileIndex)
    {

    }
}

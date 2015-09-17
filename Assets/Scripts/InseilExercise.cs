using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.IO;
using System.Collections.Generic;

public class StaticJoint
{
    public Vector3 targetPosition;
    public string joint;

    public StaticJoint(Vector3 targetPosition, string joint)
    {
        this.targetPosition = targetPosition;
        this.joint = joint;
    }
}
public class MotionJoint
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public string joint;

    public MotionJoint(Vector3 startPosition, Vector3 endPosition, string joint)
    {
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        this.joint = joint;
    }
}
public class InseilExercise : MonoBehaviour {    
    
    // Print information
    public int sizeOfSet;
    public string nameOfPerson;
    public int set;
    public Transform printPosRelToJoint;
    private List<StreamWriter> sw = new List<StreamWriter>();    

    // Exercise parameter
    private string exerciseName;
    private ExerciseConstraint[] exerciseConstraints;
    private List<StaticJoint> staticjoints;
    private List<MotionJoint> motionjoints;

    // Feedback
    public FeedbackType enabledFeedBackType;
    private List<FeedbackType> feedbackTypes = new List<FeedbackType>();
    private List<InseilFeedback> feedbackList = new List<InseilFeedback>();


    // Use this for initialization
    void Start()
    {       
       
    }   

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitExercise(string exerciseName, List<FeedbackType> feedbackTypes)
    {
        this.exerciseName = exerciseName;
        this.feedbackTypes = feedbackTypes;

        string json = File.ReadAllText("Assets/Resources/ExerciseFiles/" + exerciseName + ".json");
        DeserializeExercise(json);

        staticjoints = new List<StaticJoint>();
        motionjoints = new List<MotionJoint>();

        for (int i = 0; i < exerciseConstraints.Length; i++ )
        {
            if(exerciseConstraints[i].type == "static")
            {
                staticjoints.Add(new StaticJoint(exerciseConstraints[i].position, exerciseConstraints[i].joint));
            }
            else
            {
                if(!ContainsJointName(exerciseConstraints[i].joint))
                {
                    if (exerciseConstraints[i].type == "motion_start")
                    {
                        motionjoints.Add(new MotionJoint(exerciseConstraints[i].position, Vector3.zero, exerciseConstraints[i].joint));
                    }
                    else
                    {
                        motionjoints.Add(new MotionJoint(Vector3.zero, exerciseConstraints[i].position, exerciseConstraints[i].joint));
                    }
                }
                else
                {
                    if(exerciseConstraints[i].type == "motion_start")
                    {
                        
                    }
                }
            }
        }

        InitAndSpawnFeedback();
    }

    // Init and spawn all feedback interesting for this exercise here as child of this exercise 
    public void InitAndSpawnFeedback()
    {
        // Init every type
        foreach (FeedbackType type in feedbackTypes)
        {
            // Spawn category object in hierarchy
            GameObject feedbackTypeObject = new GameObject(type.ToString());
            feedbackTypeObject.transform.parent = transform;

            GameObject staticJointsObject = new GameObject("static joints");
            staticJointsObject.transform.parent = feedbackTypeObject.transform;
            GameObject motionJointsObject = new GameObject("motion joints");
            motionJointsObject.transform.parent = feedbackTypeObject.transform;

            // Init feedback for static joints
            for (int i = 0; i < staticjoints.Count; i++)
            {
                GameObject feedbackObject = new GameObject(staticjoints[i].joint);
                feedbackObject.transform.parent = staticJointsObject.transform;

                // Make new feedback by type
                // Call Init feedback here
                // Store in collection
            }

            // Init feedback for motion joints
            for (int i = 0; i < motionjoints.Count; i++)
            {
                GameObject feedbackObject = new GameObject(motionjoints[i].joint);
                feedbackObject.transform.parent = motionJointsObject.transform;

                // Make new feedback by type
                // Call Init feedback here
                // Store in collection
            }
        }
    }

    public void InitPrinting()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            InseilFeedback iFB = child.GetComponent<InseilFeedback>();

            feedbackList.Add(iFB);
            sw.Add(new StreamWriter(name + i + "_" + iFB.type + ".txt"));
        }
    }

    public void PrintExersiceInfo()
    {
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
                        PrintExerciseInfo(targetBall, i);
                    }
                }

                // Print areaFeedback info
                if (iFB.type == FeedbackType.AreaFeedback)
                {
                    AreaFeedback area = (AreaFeedback)iFB;

                    // ToDo: Print Info
                    PrintExerciseInfo(area, i);
                }

                // Print imageFeedback info
                if (iFB.type == FeedbackType.ImageFeedback)
                {
                    ImageFeedback3D image = (ImageFeedback3D)iFB;

                    // ToDO: Print Info
                    PrintExerciseInfo(image, i);
                }
            }

            else
            {
                iFB.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Deserializes JSON string of feedback exercise
    /// </summary>
    /// <param name="json"></param>
    public void DeserializeExercise(string json)
    {
        var N = JSON.Parse(json);
        exerciseConstraints = new ExerciseConstraint[N["constraints"].AsArray.Count];
        for (int i = 0; i < exerciseConstraints.Length; i++)
        {
            exerciseConstraints[i] = new ExerciseConstraint(N["constraints"][i]["position"]["x"].AsDouble, N["constraints"][i]["position"]["y"].AsDouble, N["constraints"][i]["position"]["z"].AsDouble,
                (string)N["constraints"][i]["joint"], (string)N["constraints"][i]["base"], (string)N["constraints"][i]["type"], N["constraints"][i]["radius"].AsDouble);
        }
    }

    void PrintExerciseInfo(BallFeedback targetBall, int fileIndex)
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

    void PrintExerciseInfo(AreaFeedback area, int fileIndex)
    {

    }

    void PrintExerciseInfo(ImageFeedback3D image, int fileIndex)
    {

    }

    // Helper functions

    private bool ContainsJointName(string name)
    {
        for(int i = 0; i < motionjoints.Count; i++)
        {
            if(motionjoints[i].joint == name)
            {
                return true;
            }
        }
        return false;
    }

    private MotionJoint FindMotionJoint(string name)
    {
        for(int i = 0; i < motionjoints.Count; i++)
        {
            if(motionjoints[i].joint == name)
            {
                return motionjoints[i];
            }
        }
        return null;
    }
}

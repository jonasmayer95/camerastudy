using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.IO;
using System.Collections.Generic;

public class ExerciseInfo
{
    public int sizeOfSet;
    public string nameOfPerson;
    public int set;
}

public abstract class ExerciseJoint
{
    public float tolerance;
}

public class StaticJoint : ExerciseJoint
{
    public Vector3 targetPosition;
    public string joint;

    public StaticJoint(Vector3 targetPosition, string joint, float tolerance)
    {
        this.tolerance = tolerance;
        this.targetPosition = targetPosition;
        this.joint = joint;
    }
}
public class MotionJoint : ExerciseJoint
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public string joint;

    public MotionJoint(Vector3 startPosition, Vector3 endPosition, string joint, float tolerance)
    {
        this.tolerance = tolerance;
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        this.joint = joint;
    }
}
public class InseilExercise : MonoBehaviour {    
    
    // Print information
    private int sizeOfSet;
    private string nameOfPerson;
    private int set;
    private Transform coordinatesRelToJoint;
    //private List<StreamWriter> sw = new List<StreamWriter>();    

    // Exercise parameter
    private string exerciseName;
    private ExerciseConstraint[] exerciseConstraints;
    private List<StaticJoint> staticjoints;
    private List<MotionJoint> motionjoints;

    // Feedback
    public ExerciseExplanation enabledFeedBackType;
    private List<ExerciseExplanation> feedbackTypes = new List<ExerciseExplanation>();
    private Dictionary<InseilFeedback, StreamWriter> feedbackList = new Dictionary<InseilFeedback, StreamWriter>();
    private BoneMap avatar;
    private Vector2 minDimension, maxDimension;
    private float minZ;


    // Use this for initialization
    void Start()
    {
        
    }   

    // Update is called once per frame
    void Update()
    {
        PrintExersiceInfo();
        InseilMainCamera.instance.UpdateExerciseDimensions(minDimension + new Vector2(coordinatesRelToJoint.position.x, coordinatesRelToJoint.position.y),
            maxDimension + new Vector2(coordinatesRelToJoint.position.x, coordinatesRelToJoint.position.y), minZ + coordinatesRelToJoint.position.z, coordinatesRelToJoint);
    }

    void OnEnable()
    {
/*        InseilMainCamera.instance.UpdateExerciseDimensions(minDimension + new Vector2(coordinatesRelToJoint.position.x, coordinatesRelToJoint.position.y),
            maxDimension + new Vector2(coordinatesRelToJoint.position.x, coordinatesRelToJoint.position.y), minZ + coordinatesRelToJoint.position.z, coordinatesRelToJoint);*/
    }

    public void InitExercise(string exerciseName, ExerciseInfo info, List<ExerciseExplanation> feedbackTypes, float bodyHeight, Transform relTo, BoneMap avatar)
    {
        this.exerciseName = exerciseName;
        this.feedbackTypes = feedbackTypes;

        string json = File.ReadAllText("Assets/Resources/ExerciseFiles/" + exerciseName + ".json");
        DeserializeExercise(json);

        staticjoints = new List<StaticJoint>();
        motionjoints = new List<MotionJoint>();

        coordinatesRelToJoint = relTo;
        this.avatar = avatar;

        this.sizeOfSet = info.sizeOfSet;
        this.nameOfPerson = info.nameOfPerson;
        this.set = info.set;

        if(avatar.gameObject.GetComponent<AvatarController>().mirroredMovement)
        {
            NegateAllZValues();
        }

        for (int i = 0; i < exerciseConstraints.Length; i++ )
        {
            if(exerciseConstraints[i].type == "static")
            {
                staticjoints.Add(new StaticJoint(exerciseConstraints[i].position * bodyHeight, exerciseConstraints[i].joint, exerciseConstraints[i].tolerance));
            }
            else
            {
                if (!ContainsJointName(exerciseConstraints[i].joint))
                {
                    if (exerciseConstraints[i].type == "motion_start")
                    {
                        motionjoints.Add(new MotionJoint(exerciseConstraints[i].position * bodyHeight, Vector3.zero, exerciseConstraints[i].joint, exerciseConstraints[i].tolerance));
                    }
                    else
                    {
                        motionjoints.Add(new MotionJoint(Vector3.zero, exerciseConstraints[i].position * bodyHeight, exerciseConstraints[i].joint, exerciseConstraints[i].tolerance));
                    }
                }
                else if (ContainsJointName(exerciseConstraints[i].joint))
                {
                    if (exerciseConstraints[i].type == "motion_end")
                    {
                        for (int k = 0; k < motionjoints.Count; k++)
                        {
                            if(motionjoints[k].joint == exerciseConstraints[i].joint)
                            {
                                motionjoints[k].endPosition = exerciseConstraints[i].position * bodyHeight;
                            }
                        }                                               
                    }
                    else if (exerciseConstraints[i].type == "motion_start")
                    {
                        for (int k = 0; k < motionjoints.Count; k++)
                        {
                            if (motionjoints[k].joint == exerciseConstraints[i].joint)
                            {
                                motionjoints[k].endPosition = exerciseConstraints[i].position * bodyHeight;
                            }
                        }
                    }
                }
            }
        }

        InitAndSpawnFeedback();
        InitPostureCorrection();
    }

    private void InitPostureCorrection()
    {
        for (int i = 0; i < staticjoints.Count; i++)
        {
            CorrectionManager.instance.AddJointToObserve(staticjoints[i]);
        }
    }

    // Init and spawn all feedback interesting for this exercise here as child of this exercise 
    private void InitAndSpawnFeedback()
    {

        // Init every type
        foreach (ExerciseExplanation feedbackType in feedbackTypes)
        {
            // Spawn category object in hierarchy
            GameObject feedbackTypeObject = new GameObject(feedbackType.ToString());
            feedbackTypeObject.transform.parent = transform;

            GameObject staticJointsObject = new GameObject("static joints");
            staticJointsObject.transform.parent = feedbackTypeObject.transform;
            GameObject motionJointsObject = new GameObject("motion joints");
            motionJointsObject.transform.parent = feedbackTypeObject.transform;

            // Init feedback for static joints
            for (int i = 0; i < staticjoints.Count; i++)
            {
                // Load feedback from Resources
                if (Resources.Load("FeedbackTypes/" + feedbackType.ToString()))
                {
                    GameObject feedbackObject = Instantiate(Resources.Load("FeedbackTypes/" + feedbackType.ToString()), Vector3.zero, Quaternion.identity) as GameObject;
                    feedbackObject.transform.parent = staticJointsObject.transform;
                    feedbackObject.name = staticjoints[i].joint;
                    InseilFeedback iFB = feedbackObject.GetComponent<InseilFeedback>();

                    // Init Printing
                    feedbackList.Add(iFB, new StreamWriter(name + "_static_" + i + "_" + iFB.type + ".txt"));
                    //sw.Add(new StreamWriter(name + "_static_" + i + "_" + iFB.type + ".txt"));

                    // Init Feedback
                    iFB.InitFeedback(staticjoints[i], coordinatesRelToJoint, avatar);
                    //feedbackList.Add(iFB);

                }
                else
                {
                    Debug.Log("Failed to load:" + "FeedbackTypes/" + feedbackType.ToString());
                }
            }

            // Init feedback for motion joints
            for (int i = 0; i < motionjoints.Count; i++)
            {
                // Load feedback from Resources
                if (Resources.Load("FeedbackTypes/" + feedbackType.ToString()))
                {
                    GameObject feedbackObject = Instantiate(Resources.Load("FeedbackTypes/" + feedbackType.ToString()), Vector3.zero, Quaternion.identity) as GameObject;
                    feedbackObject.transform.parent = motionJointsObject.transform;
                    feedbackObject.name = motionjoints[i].joint;
                    InseilFeedback iFB = feedbackObject.GetComponent<InseilFeedback>();

                    // Init Printing
                    feedbackList.Add(iFB, new StreamWriter(name + "_motion_" + i + "_" + iFB.type + ".txt"));
                    //sw.Add(new StreamWriter(name + "_motion_" + i + "_" + iFB.type + ".txt"));

                    // Init Feedback
                    iFB.InitFeedback(motionjoints[i], coordinatesRelToJoint, avatar);
                    //feedbackList.Add(iFB);
                }
                else
                {
                    Debug.Log("Failed to load:" + "FeedbackTypes/" + feedbackType.ToString());
                }
            }

            // Adjusting Camera position
            Vector4 exerciseDimensions = CalculateExerciseDimensions();
            minDimension = new Vector2(exerciseDimensions.x, exerciseDimensions.y);
            maxDimension = new Vector2(exerciseDimensions.z, exerciseDimensions.w);
            
        }
    }

    private Vector4 CalculateExerciseDimensions()
    {
        Vector4 dimensions = Vector4.zero;
        minZ = 0;

        // Initializing min and max feedback positions for camera positioning
        for (int i = 0; i < staticjoints.Count; i++)
        {
            if (staticjoints[i].targetPosition.x < dimensions.x)
            {
                dimensions = new Vector4(staticjoints[i].targetPosition.x, dimensions.y, dimensions.z, dimensions.w);
            }

            if (staticjoints[i].targetPosition.y < dimensions.y)
            {
                dimensions = new Vector4(dimensions.x, staticjoints[i].targetPosition.y, dimensions.z, dimensions.w);
            }

            if (staticjoints[i].targetPosition.x > dimensions.z)
            {
                dimensions = new Vector4(dimensions.x, dimensions.y, staticjoints[i].targetPosition.x, dimensions.w);
            }

            if (staticjoints[i].targetPosition.y > dimensions.w)
            {
                dimensions = new Vector4(dimensions.x, dimensions.y, dimensions.z, staticjoints[i].targetPosition.y);
            }

            if (staticjoints[i].targetPosition.z < minZ)
            {
                minZ = staticjoints[i].targetPosition.z;
            }
        }

        // Initializing min and max feedback positions for camera positioning
        for (int i = 0; i < motionjoints.Count; i++)
        {
            // Comparing startPositions
            if (motionjoints[i].startPosition.x < dimensions.x)
            {
                dimensions = new Vector4(motionjoints[i].startPosition.x, dimensions.y, dimensions.z, dimensions.w);
            }

            if (motionjoints[i].startPosition.y < dimensions.y)
            {
                dimensions = new Vector4(dimensions.x, motionjoints[i].startPosition.y, dimensions.z, dimensions.w);
            }

            if (motionjoints[i].startPosition.x > dimensions.z)
            {
                dimensions = new Vector4(dimensions.x, dimensions.y, motionjoints[i].startPosition.x, dimensions.w);
            }

            if (motionjoints[i].startPosition.y > dimensions.w)
            {
                dimensions = new Vector4(dimensions.x, dimensions.y, dimensions.z, motionjoints[i].startPosition.y);
            }

            if (motionjoints[i].startPosition.z < minZ)
            {
                minZ = motionjoints[i].startPosition.z;
            }

            // Comparing endPositions
            if (motionjoints[i].endPosition.x < dimensions.x)
            {
                dimensions = new Vector4(motionjoints[i].endPosition.x, dimensions.y, dimensions.z, dimensions.w);
            }

            if (motionjoints[i].endPosition.y < dimensions.y)
            {
                dimensions = new Vector4(dimensions.x, motionjoints[i].endPosition.y, dimensions.z, dimensions.w);
            }

            if (motionjoints[i].endPosition.x > dimensions.z)
            {
                dimensions = new Vector4(dimensions.x, dimensions.y, motionjoints[i].endPosition.x, dimensions.w);
            }

            if (motionjoints[i].endPosition.y > dimensions.w)
            {
                dimensions = new Vector4(dimensions.x, dimensions.y, dimensions.z, motionjoints[i].endPosition.y);
            }

            if (motionjoints[i].endPosition.z < minZ)
            {
                minZ = motionjoints[i].endPosition.z;
            }
        }

        return dimensions;
    }

    /*public void InitPrinting()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            InseilFeedback iFB = child.GetComponent<InseilFeedback>();

            feedbackList.Add(iFB);
            sw.Add(new StreamWriter(name + i + "_" + iFB.type + ".txt"));
        }
    }*/

    public void PrintExersiceInfo()
    {
        foreach (InseilFeedback iFB in feedbackList.Keys)
        {
            //InseilFeedback iFB = feedbackList[i];
            if (iFB.type == enabledFeedBackType)
            {
                iFB.gameObject.SetActive(true);

                // Print ballFeedback info
                if (iFB.type == ExerciseExplanation.BallFeedback)
                {
                    BallFeedback targetBall = (BallFeedback)iFB;

                    // Check for end of set
                    if (targetBall.positionChanges / 2 < sizeOfSet)
                    {
                        PrintExerciseInfo(targetBall, feedbackList[iFB]);
                    }
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

    void PrintExerciseInfo(BallFeedback targetBall, StreamWriter sw)
    {

        if (sw != null)
        {
            int repetitions = targetBall.positionChanges / 2;

            if (targetBall.positions.Count == 2)
            {
                sw.WriteLine(nameOfPerson + ", " + set + ", "
                            + repetitions + ", "
                            + (targetBall.positions[0].x - coordinatesRelToJoint.position.x) + ", " + (targetBall.positions[0].y - coordinatesRelToJoint.position.y) + ", " + (targetBall.positions[0].z - coordinatesRelToJoint.position.z) + ", "
                            + (targetBall.positions[1].x - coordinatesRelToJoint.position.x) + ", " + (targetBall.positions[1].y - coordinatesRelToJoint.position.y) + ", " + (targetBall.positions[1].z - coordinatesRelToJoint.position.z) + ", "
                            + (targetBall.joint.transform.position.x - coordinatesRelToJoint.position.x) + ", " + (targetBall.joint.transform.position.y - coordinatesRelToJoint.position.y) + ", " + (targetBall.joint.transform.position.z - coordinatesRelToJoint.position.z));

                sw.Flush();
            }
            else
            {
                sw.WriteLine(nameOfPerson + ", " + set + ", "
                            + repetitions + ", "
                            + (targetBall.positions[0].x - coordinatesRelToJoint.position.x) + ", " + (targetBall.positions[0].y - coordinatesRelToJoint.position.y) + ", " + (targetBall.positions[0].z - coordinatesRelToJoint.position.z) + ", "
                            + (targetBall.joint.transform.position.x - coordinatesRelToJoint.position.x) + ", " + (targetBall.joint.transform.position.y - coordinatesRelToJoint.position.y) + ", " + (targetBall.joint.transform.position.z - coordinatesRelToJoint.position.z));

                sw.Flush();
            }
        }
    }

    void PrintExerciseInfo(AreaFeedback area, StreamWriter sw)
    {

    }

    void PrintExerciseInfo(ImageFeedback3D image, StreamWriter sw)
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

    private void NegateAllZValues()
    {
        for(int i = 0; i < exerciseConstraints.Length; i++)
        {
            exerciseConstraints[i].position.z *= -1;
        }
    }
}

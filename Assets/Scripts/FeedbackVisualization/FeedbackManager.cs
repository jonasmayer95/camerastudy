using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ExerciseExplanation
{
    BallFeedback
}

public abstract class InseilFeedback : MonoBehaviour
{
    public ExerciseExplanation type;

    public abstract void InitFeedback(StaticJoint joint, Transform relTo, BoneMap bones);
    public abstract void InitFeedback(MotionJoint joint, Transform relTo, BoneMap bones);
}

public class FeedbackManager : MonoBehaviour {

    // Singleton
    public static FeedbackManager instance;

    public List<ExerciseExplanation> feedbackTypes = new List<ExerciseExplanation>();

    //Exercise Data
    public float bodyHeight;
    public string exercisePath;
    public int sizeOfSet;
    public string nameOfPerson;
    public int set;
    private Object[] exerciseData;
    private List<string> exerciseNames = new List<string>();

    // Avatars
    public BoneMap boneMap;
    public Transform coordinatesRelToJoint;

    // Stores all exercises found in the scene
    private List<InseilExercise> exercises = new List<InseilExercise>();
    
    // Index of active exercise
    private int index = 0;

    // UI variables
    public GameObject exerciseButtonPrefab;
    private List<GameObject> exerciseButtons;
    public GameObject exerciseUI;
    public GameObject feedbackUI;
    public GameObject backButton;
    public GameObject canvas;

    // Called before Start
    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {        

        exerciseData = Resources.LoadAll(exercisePath);

        // Init exercises
        for (int i = 0; i < exerciseData.Length; i++){
            exerciseNames.Add(exerciseData[i].name);

            GameObject exerciseObject = new GameObject(exerciseData[i].name);
            exerciseObject.transform.parent = transform;

            ExerciseInfo eInfo = new ExerciseInfo();
            eInfo.sizeOfSet = sizeOfSet;
            eInfo.set = set;
            eInfo.nameOfPerson = nameOfPerson;

            InseilExercise ex = exerciseObject.AddComponent<InseilExercise>();
            ex.InitExercise(exerciseData[i].name, eInfo, feedbackTypes, bodyHeight, coordinatesRelToJoint, boneMap);
            exercises.Add(ex);
            
        }

        // Deactivate all exercises
        foreach (InseilExercise iEX in exercises)
        {
            iEX.gameObject.SetActive(false);
        }

        // Activate first exercise
        exercises[index].gameObject.SetActive(true);
        exercises[index].CameraExerciseSwitch();

        InitExerciseUI();
	}
	
	// Update is called once per frame
	void Update () {

        if(Input.GetKeyDown(KeyCode.Space))
        {
            SwitchExercise((index + 1) % exercises.Count);
        }

        // Hide/unhide UI
        if(Input.GetKeyDown("h"))
        {
            EnableUI(!canvas.activeInHierarchy);
        }

        // Print exercise Info into external files
        //exercises[index].PrintExersiceInfo();
	}

    public List<string> GetExerciseFileNames()
    {
        return exerciseNames;
    }

    // Used by exercises to register
    /*public void AddExercise(InseilExercise ex)
    {
        exercises.Add(ex);
    }*/

    // Shows feedback by type
    public void ShowFeedback(int type)
    {
        // Tells feedback which feedback state is active at the moment
        exercises[index].enabledFeedBackType = (ExerciseExplanation)type;
    }

    // Called by GUI
    public void SwitchExercise(int id)
    {
        // Deactivate old exercise and activate new one
        if (id >= 0 && id < exercises.Count && exercises.Count > 0)
        {
            exercises[index].gameObject.SetActive(false);
            index = id;
            exercises[index].gameObject.SetActive(true);
            InseilMainCamera.instance.ResetCamera();
            exercises[index].CameraExerciseSwitch();
        }
    }

    // Called by GUI
    public void NextExercise()
    {
        SwitchExercise((index + 1) % exercises.Count);
    }

    private void InitExerciseUI()
    {
        // Init the buttons
        exerciseButtons = new List<GameObject>();
        int index = 0;
        foreach(InseilExercise exercise in exercises)
        {
            GameObject button = Instantiate(exerciseButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            exerciseButtons.Add(button);
            button.transform.SetParent(exerciseUI.transform);
            button.GetComponent<ExerciseButton>().InitButton(exercise.gameObject.name, index, Vector2.one);
            index++;
        }
        DisableExerciseUI();
    }

    public void LoadExerciseUI()
    {
        exerciseUI.SetActive(true);
        feedbackUI.SetActive(false);
        backButton.SetActive(true);
    }

    public void DisableExerciseUI()
    {
        exerciseUI.SetActive(false);
        feedbackUI.SetActive(true);
        backButton.SetActive(false);
    }

    public void EnableUI(bool enable)
    {
        canvas.SetActive(enable);
    }
}

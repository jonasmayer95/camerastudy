using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PostureError
{
    public Transform joint;
    public StaticJoint staticJoint;
    public float errorDistance;
    public float tolerance;    

    public PostureError(Transform joint, StaticJoint staticJoint, float errorDistance, float tolerance)
    {
        this.joint = joint;
        this.staticJoint = staticJoint;
        this.errorDistance = errorDistance;
        this.tolerance = tolerance;
    }
}

public class CorrectionManager : MonoBehaviour {

    public static CorrectionManager instance;
    public CameraFeedbackMode mode;
    public float tolerance;
    public Camera correctionCamera;
    public GameObject cameraFeedbackPrefab;
    public FeedbackCamera_Avatar correctionAvatar;
    public Transform relTo;
    public RenderTexture cameraImageRenderTexture;
    private CameraFeedback cameraFeedback;
    private Rect feedbackWindow;
    private Dictionary<Transform, PostureError> postureErrors = new Dictionary<Transform,PostureError>();
    private bool correcting = false;


    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start ()
    {
        SpawnCorrectionWindow();
	}
	
	// Update is called once per frame
	void Update () 
    {
        CalculatePostureErrors();
        HandleErrors();	
	}

    public void AddJointToObserve(StaticJoint staticJoint)
    {
        Transform joint;
        correctionAvatar.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(staticJoint.joint, correctionAvatar.gameObject.GetComponent<AvatarController>().mirroredMovement), out joint);

        PostureError error = new PostureError(joint, staticJoint, 0, tolerance);
        
        if (!postureErrors.ContainsKey(joint))
        {
            postureErrors.Add(joint, error);
            
        }
    }

    private void SpawnCorrectionWindow()
    {
        // Spawn Window
        cameraFeedbackPrefab = Instantiate(cameraFeedbackPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        cameraFeedback = cameraFeedbackPrefab.GetComponent<CameraFeedback>();

        // Init Window
        cameraFeedback.feedbackCamera = correctionCamera;
        
    }

    void OnGUI()
    {
        if (correcting)
        {
            Camera mainCam = InseilMainCamera.instance.GetComponent<Camera>();
            Vector2 rectPos = new Vector2(Screen.width * 0.1f, Screen.height * 0.25f);
            Vector2 rectSize = new Vector2(Screen.width * 0.2f, Screen.width * 0.2f);
            feedbackWindow = new Rect(rectPos.x, rectPos.y, rectSize.x, rectSize.y);
            GUI.DrawTexture(feedbackWindow, cameraImageRenderTexture);
        }
    }

    private void CalculatePostureErrors()
    {
        foreach (PostureError error in postureErrors.Values)
        {
            error.errorDistance = Vector3.Distance(relTo.position + error.staticJoint.targetPosition, error.joint.position);
            //Debug.Log("Error: " + error.errorDistance + " Name: " + error.staticJoint.joint + " Transform: " + error.joint.name);
        }
    }

    private void HandleErrors()
    {
        float maxErrorDistance = 0;
        Transform joint = null;
        foreach (PostureError error in postureErrors.Values)
        {
            if (error.errorDistance > maxErrorDistance)
            {
                joint = error.joint;
                maxErrorDistance = error.errorDistance;
            }
        }

        if (joint != null)
        {
            cameraFeedback.InitCorrectionWindow(postureErrors[joint].staticJoint, correctionAvatar);
            cameraFeedback.cameraFeedbackMode = mode;
            //Debug.Log("greatest error: " + postureErrors[joint].errorDistance + " Name: " + postureErrors[joint].staticJoint.joint);
            correcting = true;
        }

        else
        {
            correcting = false;
        }
    }
}

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

public enum WindowPosition
{
    Left, Right
}

public enum WindowMode
{
    Jumping, Moving
}

public class CorrectionManager : MonoBehaviour {

    public Vector2 relativeRectPos;
    public static CorrectionManager instance;
    public CameraFeedbackMode mode;
    public float tolerance;
    public Camera correctionCamera;
    public GameObject cameraFeedbackPrefab;
    public FeedbackCamera_Avatar correctionAvatar;
    public BoneMap avatar;
    public Transform relTo;
    public RenderTexture cameraImageRenderTexture;
    private CameraFeedback cameraFeedback;
    private Rect feedbackWindow;
    private Dictionary<Transform, PostureError> postureErrors = new Dictionary<Transform,PostureError>();
    private bool correcting = false;
    private Vector2 rectPos;
    private Camera mainCam;
    private Vector2 rectSize;
    private WindowPosition windowPos;
    public WindowMode windowMode;
    private bool showWindow;


    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start ()
    {
        SpawnCorrectionWindow();
        mainCam = InseilMainCamera.instance.GetComponent<Camera>();

        rectPos = new Vector2(Screen.width * relativeRectPos.x, Screen.height * relativeRectPos.y);
	}
	
	// Update is called once per frame
	void Update () 
    {
        CalculatePostureErrors();
        HandleErrors();

        if (windowPos == WindowPosition.Left)
        {
            if (windowMode == WindowMode.Moving)
            {
                rectPos = Vector2.Lerp(rectPos, new Vector2(Screen.width * relativeRectPos.x, Screen.height * relativeRectPos.y), Time.deltaTime * 2);
            }

            else
            {
                rectPos = new Vector2(Screen.width * relativeRectPos.x, Screen.height * relativeRectPos.y);
            }
        }
        else
        {
            if (windowMode == WindowMode.Moving)
            {
                rectPos = Vector2.Lerp(rectPos, new Vector2(Screen.width * (1 - relativeRectPos.x) - rectSize.x, Screen.height * relativeRectPos.y), Time.deltaTime * 2);
            }

            else
            {
                rectPos = new Vector2(Screen.width * (1 - relativeRectPos.x) - rectSize.x, Screen.height * relativeRectPos.y);
            }
        }
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

    public void Reset()
    {
        postureErrors.Clear();
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
            rectSize = new Vector2(Screen.width * 0.2f, Screen.width * 0.2f);            
           
            feedbackWindow = new Rect(rectPos.x, rectPos.y, rectSize.x, rectSize.y);

            if (showWindow)
            {
                GUI.DrawTexture(feedbackWindow, cameraImageRenderTexture, ScaleMode.ScaleToFit, false);
            }
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
        showWindow = false;

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
            Ray ray = mainCam.ScreenPointToRay(new Vector2((rectPos + new Vector2(rectSize.x / 2, rectSize.y / 2)).x, Screen.height -(rectPos + new Vector2(rectSize.x / 2, rectSize.y / 2)).y));
            //Debug.Log(new Vector2((rectPos + new Vector2(rectSize.x / 2, rectSize.y / 2)).x, Screen.height -(rectPos + new Vector2(rectSize.x / 2, rectSize.y / 2)).y));
            Physics.Raycast(ray, 10.0f);
            Vector3 RectWorldPos = ray.origin + ray.direction * -(mainCam.transform.position.z + mainCam.nearClipPlane);
            
            cameraFeedback.cameraFeedbackMode = mode;
            //Debug.Log("greatest error: " + postureErrors[joint].errorDistance + " Name: " + postureErrors[joint].staticJoint.joint);
            cameraFeedback.InitCorrectionWindow(postureErrors[joint].staticJoint, correctionAvatar, avatar, RectWorldPos, false);
            correcting = true;
            

            if (postureErrors[joint].staticJoint.targetPosition.x > 0 && postureErrors[joint].errorDistance > tolerance)
            {
                windowPos = WindowPosition.Right;
                cameraFeedback.InitCorrectionWindow(postureErrors[joint].staticJoint, correctionAvatar, avatar, RectWorldPos, true);
                showWindow = true;
            }
            else if (postureErrors[joint].errorDistance > tolerance)
            {
                windowPos = WindowPosition.Left;
                cameraFeedback.InitCorrectionWindow(postureErrors[joint].staticJoint, correctionAvatar, avatar, RectWorldPos, true);
                showWindow = true;
            }
           
        }

        else
        {
            correcting = false;
        }
    }
}

﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CameraFeedbackMode
{
    DockingArrow2D, DockingBall2D, DockingPuzzle2D, DockingSpike2D, LinearArrow, RigedArrow, PokeBall3D, PokeBall3DPlanar
}


public class CameraFeedback : MonoBehaviour {

    // Camera Setup
    public Camera feedbackCamera;
    //private bool showingWindow = false;

    // Joint setup
    public Transform feedbackAvatar_hip;
    public Transform feedbackAvatar_joint;
    //public Transform connectingJoint;
    //private float lineAlpha;

    // Target position and tolerance
    //private Vector3 targetPosition;
    public float tolerance;
    private int index;
    private Vector3 startPosition;
    private SphereCollider handCollider;
    // Feedback mode
    public CameraFeedbackMode cameraFeedbackMode = CameraFeedbackMode.DockingArrow2D;

    public bool spriteColoring;
    public bool spriteScaling;
    

    // Prefabs
    public GameObject targetSpherePrefab;
    public Color targetSphereColor;
    public GameObject arrow3DPrefab;
    public GameObject cylinderPrefab;    
    public GameObject line3DPrefab;
    public GameObject rigedArrowPrefab;

    public GameObject dockingArrow2DPrefab;
    public GameObject arrowDock2DPrefab;
    private GameObject dockingArrow2D;
    private GameObject arrowDock2D;

    public GameObject ballDock2DPrefab;
    public GameObject dockingBall2DPrefab;
    private GameObject ballDock2D;
    private GameObject dockingBall2D;

    public GameObject puzzleDock2DPrefab;
    public GameObject dockingPuzzle2DPrefab;
    private GameObject puzzleDock2D;
    private GameObject dockingPuzzle2D;

    public GameObject spikeDock2DPrefab;
    public GameObject dockingSpike2DPrefab;
    private GameObject spikeDock2D;
    private GameObject dockingSpike2D;

    public GameObject Pokeball3DPrefab;
    public GameObject PokeDock3DPrefab;
    private GameObject Pokeball3D;
    private GameObject PokeDock3D;
   
    // Variables used for referencing
    private GameObject feedbackCylinder;    
    private GameObject targetSphere;
    private GameObject arrow3D;
    //private GameObject line3D;
    //private LineRenderer rend;
    private GameObject rigedArrow;
    //private Transform rootBoneArrow;
    
    
    private Renderer targetSphereRenderer;
    public float spriteDistance;
    public bool initialized = false;
    private bool left;


    private Material dockingArrow2DMaterial;
    private Material arrowDock2DMaterial;
    private Material ballDock2DMaterial;
    private Material dockingBall2DMaterial;
    private Material puzzleDock2DMaterial;
    private Material dockingPuzzle2DMaterial;
    private Material spikeDock2DMaterial;
    private Material dockingSpike2DMaterial;

    private Material pokeball3DMaterial;
    private Material pokedock3DMaterial;

    public float colorDistance;


	// Use this for initialization
    void Start()
    {
        // Spawn a sphere to show the target position for debugging
        /*targetSphere = Instantiate(targetSpherePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        targetSphereRenderer = targetSphere.GetComponent<Renderer>();
        targetSphereRenderer.material.color = targetSphereColor;
        targetSphere.transform.parent = transform;
        targetSphere.SetActive(false);
        */
        // Spawn a linear arrow pointing from the joint to the correct position
        arrow3D = Instantiate(arrow3DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        arrow3D.transform.parent = transform;
        arrow3D.SetActive(false);

        // Spawn the line pointing from the joint to the camera image
        //line3D = Instantiate(line3DPrefab, transform.position, Quaternion.identity) as GameObject;
        //line3D.transform.parent = transform;
        /*rend = line3D.GetComponent<LineRenderer>();
        rend.SetVertexCount(2);
        rend.SetPosition(0, transform.position);
        rend.SetPosition(1, transform.position);*/

        // Spawn a cylinder showing an bent arrow from the joint to the correct position
        feedbackCylinder = Instantiate(cylinderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        feedbackCylinder.transform.parent = transform;
        feedbackCylinder.SetActive(false);

        // Spawn a riged Arrow which can be bent during runtime
        rigedArrow = Instantiate(rigedArrowPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        rigedArrow.transform.parent = transform;
        //rootBoneArrow = rigedArrow.transform.GetChild(0).GetChild(0);
        //BendArrow(180);
        rigedArrow.SetActive(false);

        // Spawn a dockingStation to show the target position
        arrowDock2D = Instantiate(arrowDock2DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        arrowDock2D.transform.parent = feedbackCamera.transform;
        arrowDock2D.SetActive(false);

        arrowDock2DMaterial = arrowDock2D.GetComponent<SpriteRenderer>().material;

        // Spawn a dockingArrow pointing from the joint to the dock
        dockingArrow2D = Instantiate(dockingArrow2DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        dockingArrow2D.transform.parent = feedbackCamera.transform;
        dockingArrow2D.SetActive(false);

        dockingArrow2DMaterial = dockingArrow2D.GetComponent<SpriteRenderer>().material;

        // Spawn a ballDockingStation to show the target position
        ballDock2D = Instantiate(ballDock2DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        ballDock2D.transform.parent = feedbackCamera.transform;
        ballDock2D.SetActive(false);

        ballDock2DMaterial = ballDock2D.GetComponent<SpriteRenderer>().material;

        // Spawn a dockingBall pointing from the joint to the dock
        dockingBall2D = Instantiate(dockingBall2DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        dockingBall2D.transform.parent = feedbackCamera.transform;
        dockingBall2D.SetActive(false);

        dockingBall2DMaterial = dockingBall2D.GetComponent<SpriteRenderer>().material;

        // Spawn a puzzleDockingStation to show the target position
        puzzleDock2D = Instantiate(puzzleDock2DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        puzzleDock2D.transform.parent = feedbackCamera.transform;
        puzzleDock2D.SetActive(false);

        puzzleDock2DMaterial = puzzleDock2D.GetComponent<SpriteRenderer>().material;

        // Spawn a dockingPuzzle pointing from the joint to the dock
        dockingPuzzle2D = Instantiate(dockingPuzzle2DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        dockingPuzzle2D.transform.parent = feedbackCamera.transform;
        dockingPuzzle2D.SetActive(false);

        dockingPuzzle2DMaterial = dockingPuzzle2D.GetComponent<SpriteRenderer>().material;

        // Spawn a spikeDockingStation to show the target position
        spikeDock2D = Instantiate(spikeDock2DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        spikeDock2D.transform.parent = feedbackCamera.transform;
        spikeDock2D.SetActive(false);

        spikeDock2DMaterial = spikeDock2D.GetComponent<SpriteRenderer>().material;

        // Spawn a dockingSpike pointing from the joint to the dock
        dockingSpike2D = Instantiate(dockingSpike2DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        dockingSpike2D.transform.parent = feedbackCamera.transform;
        dockingSpike2D.SetActive(false);

        dockingSpike2DMaterial = dockingSpike2D.GetComponent<SpriteRenderer>().material;

        Pokeball3D = Instantiate(Pokeball3DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        PokeDock3D = Instantiate(PokeDock3DPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        Pokeball3D.transform.parent = transform;
        PokeDock3D.transform.parent = transform;
        pokeball3DMaterial = Pokeball3D.GetComponent<Renderer>().materials[1];
        pokedock3DMaterial = PokeDock3D.GetComponent<Renderer>().materials[1];

        Pokeball3D.SetActive(false);
        PokeDock3D.SetActive(false);

        // Init camera position
        //UpdateCameraPosition();
        //targetPosition = Vector3.zero;
    }

    /*public void ResetWindow()
    {
        showingWindow = false;
    }*/

    public void InitCorrectionCamera(Transform hip, Transform hand, Vector3 targetOrbRelPosition, GameObject targetSphere, CameraFeedbackMode camMode, Vector3 startPos)
    {
        feedbackCamera = Camera.main;
        feedbackAvatar_hip = hip;
        feedbackAvatar_joint = hand;
        //targetPosition = targetOrbRelPosition;
        this.targetSphere = targetSphere;
        targetSphereRenderer = targetSphere.GetComponent<Renderer>();
        cameraFeedbackMode = camMode;
        startPosition = startPos;
        initialized = true;
        handCollider = feedbackAvatar_joint.GetComponent<SphereCollider>();
    }

    /*public void InitCorrectionWindow(StaticJoint joint, FeedbackCamera_Avatar cameraAvatar, BoneMap basicAvatar, Vector3 windowPosition, bool enabled, float lineAlpha, bool left)
    {
        showingWindow = enabled;
        this.lineAlpha = lineAlpha;

        this.left = left;

        //throw new System.NotImplementedException();
        positions[0] = joint.targetPosition;

        Transform bone;
        basicAvatar.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(joint.joint, basicAvatar.gameObject.GetComponent<AvatarController>().mirroredMovement), out bone);
        connectingJoint = bone;

        cameraAvatar.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey(joint.joint, cameraAvatar.gameObject.GetComponent<AvatarController>().mirroredMovement), out bone);
        feedbackAvatar_joint = bone;

        cameraAvatar.GetBoneMap().TryGetValue(BoneMap.GetBoneMapKey("spinebase", cameraAvatar.gameObject.GetComponent<AvatarController>().mirroredMovement), out bone);
        feedbackAvatar_hip = bone;

        feedbackCamera = cameraAvatar.feedbackCamera;

        rend.SetPosition(0, windowPosition);

        initialized = true;
        
        UpdateCameraPosition();
    }*/

    void OnEnable()
    {
        //UpdateCameraPosition();
    }

   /* void OnTriggerEnter(Collider other)
    {
        if (initialized && other.gameObject == feedbackAvatar_joint)
        {
            index = (index + 1) % positions.Count;
        }
    }*/

    // Update is called once per frame
    void Update()
    {        
        if (initialized)
        {
            // Show sphere at target position (for debugging)
            targetSphere.SetActive(true);
            
            // Update the position and orientation of the 3D arrow
            if (cameraFeedbackMode == CameraFeedbackMode.LinearArrow)
            {
                // Deactivate the cylinder
                feedbackCylinder.SetActive(false);
                rigedArrow.SetActive(false);
                dockingArrow2D.SetActive(false);
                arrowDock2D.SetActive(false);

                float radius = Vector3.Distance(targetSphere.transform.position, feedbackAvatar_joint.position) / 2.0f;
                arrow3D.transform.localScale = new Vector3(radius, radius, radius);

                // Update and activate the 3D arrow
                arrow3D.transform.position = feedbackAvatar_joint.position + (targetSphere.transform.position - feedbackAvatar_joint.position) / 2.0f;
                arrow3D.transform.LookAt(targetSphere.transform.position);

                // Activate the 3D arrow
                arrow3D.SetActive(true);
                targetSphereRenderer.enabled = true;
            }

            if (cameraFeedbackMode == CameraFeedbackMode.RigedArrow)
            {
                // Deactivate the 3D arrow
                arrow3D.SetActive(false);
                feedbackCylinder.SetActive(false);
                dockingArrow2D.SetActive(false);
                arrowDock2D.SetActive(false);

                // Update and activate the riged 3D arrow
                rigedArrow.transform.position = feedbackAvatar_joint.position + (targetSphere.transform.position - feedbackAvatar_joint.position) / 2.0f;

                rigedArrow.transform.LookAt(targetSphere.transform.position);
                Quaternion baseRot = rigedArrow.transform.rotation;
                rigedArrow.transform.rotation = baseRot * Quaternion.Euler(-90, 0, 0);

                float radius = Vector3.Distance(targetSphere.transform.position, feedbackAvatar_joint.position) / 2.0f;
                rigedArrow.transform.localScale = new Vector3(radius , radius , radius );

                // Activate riged Arrow
                rigedArrow.SetActive(true);
                targetSphereRenderer.enabled = true;
            }

            // Update the position size and orientation of the cylinder showing a bent arrow
            /*if (cameraFeedbackMode == CameraFeedbackMode.Cylinder)
            {
                // Deactivate the 3D arrow
                arrow3D.SetActive(false);
                rigedArrow.SetActive(false);
                dockingArrow2D.SetActive(false);
                arrowDock2D.SetActive(false);


                // Update position and size
                feedbackCylinder.transform.position = feedbackAvatar_joint.position + (targetSphere.transform.position - feedbackAvatar_joint.position) / 2.0f;
                float radius = Vector3.Distance(targetSphere.transform.position, feedbackAvatar_joint.position) / 1.5f;
                feedbackCylinder.transform.localScale = new Vector3(radius, radius, radius);

                // Update orientation
                Vector2 projectedErrorVector;
                projectedErrorVector = (targetSphere.transform.position - feedbackAvatar_joint.position).normalized;
                Vector3 eulerOrientation = Vector3.Cross(projectedErrorVector, feedbackCamera.transform.forward.normalized);

                // Upper Left
                if (feedbackAvatar_joint.position.y - targetSphere.transform.position.y >= 0 && targetSphere.transform.position.x - feedbackAvatar_joint.position.x > 0)
                {
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
                    //Debug.Log("Now upper left ");

                    Quaternion upRotation = Quaternion.FromToRotation(-feedbackCylinder.transform.forward, eulerOrientation);// ;
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation * Quaternion.AngleAxis(35, Vector3.forward), Time.deltaTime * 20);
                }

                // Bottom Left
                if (targetSphere.transform.position.y - feedbackAvatar_joint.position.y > 0 && targetSphere.transform.position.x - feedbackAvatar_joint.position.x > 0)
                {
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
                    //Debug.Log("Now bottom left ");

                    Quaternion upRotation = Quaternion.FromToRotation(-feedbackCylinder.transform.forward, eulerOrientation) * Quaternion.Euler(180, 0, 0);
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation, Time.deltaTime * 20);
                }

                // Upper Right
                if (feedbackAvatar_joint.position.y - targetSphere.transform.position.y > 0 && feedbackAvatar_joint.position.x - targetSphere.transform.position.x >= 0)
                {
                    //Debug.Log("Now upper right ");
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
                    Quaternion upRotation = Quaternion.FromToRotation(feedbackCylinder.transform.forward, eulerOrientation) * Quaternion.Euler(0, 0, 180);
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation * Quaternion.AngleAxis(-35, Vector3.forward), Time.deltaTime * 20);
                }

                // Bottom right
                if (targetSphere.transform.position.y - feedbackAvatar_joint.position.y >= 0 && feedbackAvatar_joint.position.x - targetSphere.transform.position.x >= 0)
                {
                    //Debug.Log("Now bottom right ");
                    feedbackCylinder.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
                    Quaternion upRotation = Quaternion.FromToRotation(feedbackCylinder.transform.forward, eulerOrientation) * Quaternion.Euler(0, 180, 0);
                    feedbackCylinder.transform.rotation = Quaternion.Slerp(feedbackCylinder.transform.rotation, upRotation, Time.deltaTime * 20);
                }

                // Activate cylinder
                feedbackCylinder.SetActive(true);
                targetSphereRenderer.enabled = true;
            }*/

            // Update the position and orientation of the 3D arrow
            if (cameraFeedbackMode == CameraFeedbackMode.DockingArrow2D)
            {
                // Deactivate the 3D arrow
                arrow3D.SetActive(false);
                rigedArrow.SetActive(false);
                feedbackCylinder.SetActive(false);
                //targetSphereRenderer.enabled = false;

                dockingArrow2D.transform.position = feedbackCamera.transform.position + spriteDistance * (feedbackAvatar_joint.TransformPoint(transform.TransformPoint(handCollider.center)) - feedbackCamera.transform.position).normalized;
                arrowDock2D.transform.position = feedbackCamera.transform.position + spriteDistance * (targetSphere.transform.position - feedbackCamera.transform.position).normalized;

                //dockingArrow2D.transform.localRotation = Quaternion.identity;
                //arrowDock2D.transform.localRotation = Quaternion.identity;                

                Vector2 dir = arrowDock2D.transform.localPosition - dockingArrow2D.transform.localPosition;
                dir.Normalize();

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (!UserStudyLogic.instance.snapping)
                {
                    dockingArrow2D.transform.localRotation = Quaternion.Euler(dockingArrow2D.transform.localRotation.x, 0, angle);
                    arrowDock2D.transform.localRotation = Quaternion.Euler(dockingArrow2D.transform.localRotation.x, 0, angle);
                }
                else
                {
                    dockingArrow2D.transform.position = arrowDock2D.transform.position;
                }

                //Activate the Dock and arrow 
                dockingArrow2D.SetActive(true);
                arrowDock2D.SetActive(true);

                UpdateSpriteColor(dockingArrow2DMaterial);

                UpdateSpriteScale(dockingArrow2D);
            }

            if (cameraFeedbackMode == CameraFeedbackMode.DockingBall2D)
            {
                // Deactivate the 3D arrow
                arrow3D.SetActive(false);
                rigedArrow.SetActive(false);
                feedbackCylinder.SetActive(false);
                //targetSphereRenderer.enabled = false;

                dockingBall2D.transform.position = feedbackCamera.transform.position + spriteDistance * (feedbackAvatar_joint.TransformPoint(transform.TransformPoint(handCollider.center)) - feedbackCamera.transform.position).normalized;
                ballDock2D.transform.position = feedbackCamera.transform.position + spriteDistance * (targetSphere.transform.position - feedbackCamera.transform.position).normalized;

                //dockingBall2D.transform.localRotation = Quaternion.identity;
                //ballDock2D.transform.localRotation = Quaternion.identity;

                Vector2 dir = ballDock2D.transform.localPosition - dockingBall2D.transform.localPosition;
                dir.Normalize();

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (!UserStudyLogic.instance.snapping)
                {
                    dockingBall2D.transform.localRotation = Quaternion.Euler(dockingBall2D.transform.localRotation.x, 0, angle);
                    ballDock2D.transform.localRotation = Quaternion.Euler(dockingBall2D.transform.localRotation.x, 0, angle);
                }
                else
                {
                    dockingBall2D.transform.position = ballDock2D.transform.position;
                }

                //Activate the Dock and arrow 
                dockingBall2D.SetActive(true);
                ballDock2D.SetActive(true);

                UpdateSpriteColor(dockingBall2DMaterial);
                UpdateSpriteScale(dockingBall2D);
            }

            if (cameraFeedbackMode == CameraFeedbackMode.DockingPuzzle2D)
            {
                // Deactivate the 3D arrow
                arrow3D.SetActive(false);
                rigedArrow.SetActive(false);
                feedbackCylinder.SetActive(false);
                //targetSphereRenderer.enabled = false;

                dockingPuzzle2D.transform.position = feedbackCamera.transform.position + spriteDistance * (feedbackAvatar_joint.TransformPoint(transform.TransformPoint(handCollider.center))- feedbackCamera.transform.position).normalized;
                puzzleDock2D.transform.position = feedbackCamera.transform.position + spriteDistance * (targetSphere.transform.position - feedbackCamera.transform.position).normalized;

                //dockingPuzzle2D.transform.localRotation = Quaternion.identity;
                //puzzleDock2D.transform.localRotation = Quaternion.identity;

                Vector2 dir = puzzleDock2D.transform.localPosition - dockingPuzzle2D.transform.localPosition;
                dir.Normalize();

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (!UserStudyLogic.instance.snapping)
                {
                    dockingPuzzle2D.transform.localRotation = Quaternion.Euler(dockingPuzzle2D.transform.localRotation.x, 0, angle);
                    puzzleDock2D.transform.localRotation = Quaternion.Euler(dockingPuzzle2D.transform.localRotation.x, 0, angle);
                }
                else
                {
                    dockingPuzzle2D.transform.position = puzzleDock2D.transform.position;
                }

                //Activate the Dock and arrow 
                dockingPuzzle2D.SetActive(true);
                puzzleDock2D.SetActive(true);

                UpdateSpriteColor(dockingPuzzle2DMaterial);

                UpdateSpriteScale(dockingPuzzle2D);
            }

            if (cameraFeedbackMode == CameraFeedbackMode.DockingSpike2D)
            {
                // Deactivate the 3D arrow
                arrow3D.SetActive(false);
                rigedArrow.SetActive(false);
                feedbackCylinder.SetActive(false);
                //targetSphereRenderer.enabled = false;

                dockingSpike2D.transform.position = feedbackCamera.transform.position + spriteDistance * (feedbackAvatar_joint.TransformPoint(transform.TransformPoint(handCollider.center)) - feedbackCamera.transform.position).normalized;
                spikeDock2D.transform.position = feedbackCamera.transform.position + spriteDistance * (targetSphere.transform.position - feedbackCamera.transform.position).normalized;

                //dockingSpike2D.transform.localRotation = Quaternion.identity;
                //spikeDock2D.transform.localRotation = Quaternion.identity;

                Vector2 dir = spikeDock2D.transform.localPosition - dockingSpike2D.transform.localPosition;
                dir.Normalize();

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (!UserStudyLogic.instance.snapping)
                {
                    dockingSpike2D.transform.localRotation = Quaternion.Euler(dockingSpike2D.transform.localRotation.x, 0, angle);
                    spikeDock2D.transform.localRotation = Quaternion.Euler(dockingSpike2D.transform.localRotation.x, 0, angle);
                }
                else
                {
                    dockingSpike2D.transform.position = spikeDock2D.transform.position;
                }
                //Activate the Dock and arrow 
                dockingSpike2D.SetActive(true);
                spikeDock2D.SetActive(true);

                //Set color
                UpdateSpriteColor(dockingSpike2DMaterial);

                UpdateSpriteScale(dockingSpike2D);
            }

            if (cameraFeedbackMode == CameraFeedbackMode.PokeBall3D)
            {
                Pokeball3D.transform.parent = transform;
                PokeDock3D.transform.parent = transform;
                arrow3D.SetActive(false);
                rigedArrow.SetActive(false);
                feedbackCylinder.SetActive(false);

                Pokeball3D.transform.position = feedbackAvatar_joint.TransformPoint(transform.TransformPoint(handCollider.center));
                PokeDock3D.transform.position = targetSphere.transform.position;

                        
                Pokeball3D.SetActive(true);
                PokeDock3D.SetActive(true);

                UpdateSpriteColor(pokeball3DMaterial);


                if (!UserStudyLogic.instance.snapping)
                {
                    Vector3 dir = (PokeDock3D.transform.position - Pokeball3D.transform.position).normalized;    
                    Pokeball3D.transform.right = -dir;
                    PokeDock3D.transform.right = -dir;  
                }
                else
                {
                    Pokeball3D.transform.position = PokeDock3D.transform.position;
                    Pokeball3D.transform.rotation = PokeDock3D.transform.rotation;
                }
            }

            if (cameraFeedbackMode == CameraFeedbackMode.PokeBall3DPlanar)
            {
                PokeDock3D.transform.parent = feedbackCamera.transform;
                Pokeball3D.transform.parent = feedbackCamera.transform;
                Pokeball3D.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                PokeDock3D.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                arrow3D.SetActive(false);
                rigedArrow.SetActive(false);
                feedbackCylinder.SetActive(false);

                Pokeball3D.SetActive(true);
                PokeDock3D.SetActive(true);

                Pokeball3D.transform.position = feedbackCamera.transform.position + spriteDistance * (feedbackAvatar_joint.TransformPoint(transform.TransformPoint(handCollider.center)) - feedbackCamera.transform.position).normalized;
                PokeDock3D.transform.position = feedbackCamera.transform.position + spriteDistance * (targetSphere.transform.position - feedbackCamera.transform.position).normalized;

                UpdateSpriteColor(pokeball3DMaterial);


                Vector3 dir = PokeDock3D.transform.position - Pokeball3D.transform.position;
                dir.Normalize();

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (!UserStudyLogic.instance.snapping)
                {
                    Pokeball3D.transform.forward = -feedbackCamera.transform.forward;
                    PokeDock3D.transform.forward = -feedbackCamera.transform.forward;
                    Pokeball3D.transform.right = -dir;
                    PokeDock3D.transform.right = -dir;
                }
                else
                {
                    Pokeball3D.transform.position = PokeDock3D.transform.position;
                    Pokeball3D.transform.rotation = PokeDock3D.transform.rotation;
                }              
            }
        }


        // Not active
        else
        {
            // Deactivate sphere and arrow
            targetSphere.SetActive(false);
            arrow3D.SetActive(false);
            feedbackCylinder.SetActive(false);
            rigedArrow.SetActive(false);
            dockingArrow2D.SetActive(false);
            arrowDock2D.SetActive(false);
            ballDock2D.SetActive(false);
            dockingBall2D.SetActive(false);
            puzzleDock2D.SetActive(false);
            dockingPuzzle2D.SetActive(false);
            spikeDock2D.SetActive(false);
            dockingSpike2D.SetActive(false);
            PokeDock3D.SetActive(false);
            Pokeball3D.SetActive(false);
        }

        // Update Camera position
        //UpdateCameraPosition();

        // Update the end point of the 3D line according to the new position of the joint
        /*if (showingWindow && lineAlpha == 1)
        {
            rend.enabled = true;
            rend.SetPosition(1, connectingJoint.position);
        }
        else
        {
            rend.enabled = false;
        }*/

    }

    public void DisableSubObjects()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);            
        }

        // Disabling also children of camera
        dockingArrow2D.SetActive(false);
        arrowDock2D.SetActive(false);
        ballDock2D.SetActive(false);
        dockingBall2D.SetActive(false);
        puzzleDock2D.SetActive(false);
        dockingPuzzle2D.SetActive(false);
        spikeDock2D.SetActive(false);
        dockingSpike2D.SetActive(false);
    }
    void OnDisable()
    {
        if (initialized)
        { 
            //targetSphere.SetActive(false);
            arrow3D.SetActive(false);
            feedbackCylinder.SetActive(false);
            rigedArrow.SetActive(false);
            dockingArrow2D.SetActive(false);
            arrowDock2D.SetActive(false);
            ballDock2D.SetActive(false);
            dockingBall2D.SetActive(false);
            puzzleDock2D.SetActive(false);
            dockingPuzzle2D.SetActive(false);
            spikeDock2D.SetActive(false);
            dockingSpike2D.SetActive(false);
            Pokeball3D.SetActive(false);
            PokeDock3D.SetActive(false);
        }
    }

    

    /*void BendArrow(float angle)
    {
        int boneCount = 0;
        Transform child = rootBoneArrow;
        while(child.childCount > 0)
        {
            boneCount++;
            child = child.GetChild(0);
        }
        boneCount -= 1;
        float boneAngle = angle / boneCount;
        child = rootBoneArrow;
        while(boneCount != 0)
        {
            child.localRotation = Quaternion.Euler(0, 0, boneAngle);
            child = child.GetChild(0);
            boneCount--;
        }
    }*/


    //Colors the sprite dependent on the distance between the targetsphere and the joint with a color value between red and green
    void UpdateSpriteColor(Material objectToColor)
    {
        if (spriteColoring)
        {
            //float distance = Vector3.Distance(targetSphere.transform.position, feedbackAvatar_joint.position);
            float distance = Mathf.Abs((feedbackCamera.transform.rotation * targetSphere.transform.position).z - (feedbackCamera.transform.rotation * feedbackAvatar_joint.TransformPoint(transform.TransformPoint(handCollider.center))).z);
            if (distance < colorDistance)
            {
                float redFactor = distance / colorDistance;
                float greenFactor = 1 - redFactor;
                objectToColor.color = new Vector4(redFactor, greenFactor, 0, 1);
            }
            else
            {
                objectToColor.color = Color.red;
            }
        }
    }

    void UpdateSpriteScale(GameObject objToScale)
    {
        if (spriteScaling)
        {
            float distance = (feedbackCamera.transform.rotation * targetSphere.transform.position).z - (feedbackCamera.transform.rotation * feedbackAvatar_joint.TransformPoint(transform.TransformPoint(handCollider.center))).z;
            if (distance < 0)
            {
                if (distance < 4 * colorDistance)
                {
                    float scaleFactor = Mathf.Abs(distance) / (4 * colorDistance);
                    objToScale.transform.localScale = Vector3.one * Mathf.Max(scaleFactor, 0.15f);
                }
            }
            else
            {
                
                if (Mathf.Abs(distance) < 4 * colorDistance)
                {
                    float scaleFactor = ((4 * colorDistance) - Mathf.Abs(distance)) / (4 * colorDistance);
                    objToScale.transform.localScale = Vector3.one * Mathf.Min(scaleFactor, 1) * 0.15f;
                }
            }
        }
    }
}

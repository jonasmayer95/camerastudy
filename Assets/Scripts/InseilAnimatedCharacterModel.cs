using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InseilAnimatedCharacterModel : MonoBehaviour
{

    // Character joints
    // Head and upper part
    public Transform head;
    public Transform neck;

    // Mid body part
    public Transform spineshoulder;
    public Transform spinemid;
    public Transform spinebase;

    // Left arm
    public Transform shoulderleft;
    public Transform elbowleft;
    public Transform wristleft;
    public Transform handleft;
    public Transform handtipleft;
    public Transform thumbleft;

    // Right arm
    public Transform shoulderright;
    public Transform elbowright;
    public Transform wristright;
    public Transform handright;
    public Transform handtipright;
    public Transform thumbright;

    // Left leg
    public Transform hipleft;
    public Transform kneeleft;
    public Transform ankleleft;
    public Transform footleft;

    // Right leg
    public Transform hipright;
    public Transform kneeright;
    public Transform ankleright;
    public Transform footright;

    public Transform model_root;

    // Dictionaries containing reference information
    private Dictionary<string, Transform> model_Joints = new Dictionary<string, Transform>();                       // Contains model rigg
    private Dictionary<string, Vector3> model_InitialJointPositions = new Dictionary<string, Vector3>();            // Used to calculate orientation of spinebase
    private Dictionary<string, Quaternion> model_InitialJointOrientations = new Dictionary<string, Quaternion>();   // Used to calculate new joint orientations (for reference)
    protected Dictionary<string, Vector3> model_InitialJointDirections = new Dictionary<string, Vector3>();         // Used to calculate new joint orientations    

    // Updated over network
    private Dictionary<string, Vector3> model_CurrentJointPositions = new Dictionary<string, Vector3>();            // Used to calculate new joint orientations
    private Dictionary<string, Quaternion> model_CurrentJointOrientations = new Dictionary<string, Quaternion>();   // not used at the moment
    
    // Skeletal rigg hierarchy
    private Dictionary<string, string> skeletalModelStructure_FromJointToJoint = new Dictionary<string, string>()
    {
            // Spine to Head
            {"spinebase" , "spinemid"},
            {"spinemid" , "spineshoulder" },
            {"spineshoulder", "neck"},
            {"neck" , "head" }, // use inverse direction to rotate the head like the neck
                 
            // Spineshoulder to Left Arm
            {"spineshoulder", "shoulderleft"},
            {"shoulderleft" , "elbowleft" },
            {"elbowleft" , "wristleft"},
            {"wristleft" , "handleft" },
            {"handleft" , "thumbleft"},
            {"handleft" , "handtipleft"},
            //{"handtipleft" , "handleft" },// rotation of bone endings can't be computed from bone positions 
            //{"thumbleft" , "wristleft" },// rotation of bone endings can't be computed from bone positions 
            
            // Spineshoulder to Right Arm
            {"spineshoulder", "shoulderright"},
            {"shoulderright" , "elbowright" },
            {"elbowright" , "wristright" },
            {"wristright" , "handright" },
            {"handright" , "thumbright"},
            {"handright" , "handtipright"},
            //{"handtipright" , "handright" },// rotation of bone endings can't be computed from bone positions 
            //{"thumbright" , "wristright" }// rotation of bone endings can't be computed from bone positions 
     
            // Spine to Left Leg
            {"spinebase" , "hipleft"},
            {"hipleft" , "kneeleft" },
            {"kneeleft" , "ankleleft" },
            {"ankleleft" , "footleft" },
            //{"footleft" , "ankleleft" },// rotation of bone endings can't be computed from bone positions

            // Spine to Right Leg
            {"spinebase" , "hipright"},
            {"hipright" , "kneeright" },
            {"kneeright" , "ankleright" },
            {"ankleright" , "footright" },
            //{"footright , "ankleright" },// rotation of bone endings can't be computed from bone positions 
    };
            

    // Use this for initialization
    void Start()
    {
        // Set Transforms
        AddModelJoints();

        // Set initial joint Data
        CalculateInitialJointData();        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateCharacterModel(InseilMessage msg)
    {
        // Accessing updated joint information
        Dictionary<string, InseilJoint> jointData = msg.measurement.data;

        // Updating position and orientation of all assigned joints
        foreach (KeyValuePair<string, InseilJoint> joint in jointData)
        {
            // Get joint in model
            if (!model_Joints.ContainsKey(joint.Key))
                continue;

            // Fetch new position data
            model_CurrentJointPositions[joint.Key] = new Vector3((float)(joint.Value.position.x), (float)(joint.Value.position.y), (float)(joint.Value.position.z));

            // Fetch new rotation data
            model_CurrentJointOrientations[joint.Key] = Quaternion.Euler(new Vector3((float)(joint.Value.rotation.x), (float)(joint.Value.rotation.y), (float)(joint.Value.rotation.z)));

            // the applyRelativeRotationChange function returns the new "local rotation" relative to the RootTransform Rotation...
            Quaternion localRotTowardsRootTransform = applyRelativeRotationChange(joint.Key, model_InitialJointOrientations[joint.Key]);
            // ...therefore we have to multiply it with the RootTransform Rotation to get the global rotation of the joint
            model_Joints[joint.Key].rotation = model_root.rotation * localRotTowardsRootTransform;

            //--------------DEBUG, TEST if code above not working (lines with arrow=)--------
            //transforms[jt].rotation = getRawWorldRotation(jt);
            //----->model_Joints[joint.Key].position = getRawWorldPosition(joint.Key);
            // debug: show computed rotatations
            //----->model_Joints[joint.Key].rotation = applyRelativeRotationChange(joint.Key, Quaternion.identity);

            Debug.Log(string.Format("{0} {1} {2}\n", joint.Key, joint.Value.position, model_CurrentJointPositions[joint.Key]));
        }
    }

    private void AddModelJoints()
    {
        // Manually adding :( all joints to a more generic data structure :)
        // Head
        model_Joints.Add("head", head);
        model_Joints.Add("neck", neck);
        // Mid body
        model_Joints.Add("spineshoulder", spineshoulder);
        model_Joints.Add("spinemid", spinemid);
        model_Joints.Add("spinebase", spinebase);
        // Left arm
        model_Joints.Add("shoulderleft", shoulderleft);
        model_Joints.Add("elbowleft", elbowleft);
        model_Joints.Add("wristleft", wristleft);
        //model_Joints.Add("handleft", handleft);
        model_Joints.Add("handtipleft", handtipleft);
        model_Joints.Add("thumbleft", thumbleft);
        // Right arm
        model_Joints.Add("shoulderright", shoulderright);
        model_Joints.Add("elbowright", elbowright);
        model_Joints.Add("wristright", wristright);
        //model_Joints.Add("handright", handright);
        model_Joints.Add("handtipright", handtipright);
        model_Joints.Add("thumbright", thumbright);
        // Left leg
        model_Joints.Add("hipleft", hipleft);
        model_Joints.Add("kneeleft", kneeleft);
        model_Joints.Add("ankleleft", ankleleft);
        model_Joints.Add("footleft", footleft);
        // Right leg
        model_Joints.Add("hipright", hipright);
        model_Joints.Add("kneeright", kneeright);
        model_Joints.Add("ankleright", ankleright);
        model_Joints.Add("footright", footright);
    }

    private void CalculateInitialJointData()
    {
        // Check each joint
        foreach (KeyValuePair<string, Transform> joint in model_Joints)
        {
            // Skip joints which are not applied
            if (!model_Joints.ContainsKey(joint.Key))
                continue;

            // Add initial joint Orientations
            model_InitialJointOrientations.Add(joint.Key, Quaternion.Inverse(model_root.rotation) * model_Joints[joint.Key].rotation);

            // Add initial joint Directions
            model_InitialJointDirections.Add(joint.Key,getJointDirectionFromGO(joint.Key));

            // Add initial joint Positions
            model_InitialJointPositions.Add(joint.Key, model_Joints[joint.Key].position);
        }
    }

    public Quaternion applyRelativeRotationChange(string jt, Quaternion initialModelJointRotation)
    {
        //missing information to calculate rotation for joint type
        if (!skeletalModelStructure_FromJointToJoint.ContainsKey(jt))
        {
            return initialModelJointRotation;
        }

        // check if tracking is available
        /*if (currentBody == null)
        {
            return initialModelJointRotation;
        }*/

        // original direction of bone
        Vector3 initialDirection = model_InitialJointDirections[jt];
        // new direction of bone
        Vector3 currentDirection = getJointDirection(jt);
        // rotation between the original an new bone direction in the kinect coordinate system
        Quaternion avatarInitialToCurrentRotation = Quaternion.FromToRotation(initialDirection, currentDirection);

        // STUDENT TASK
        // additional rotations of the hip and chest should be computed to be able to turn
        switch (jt)
        {
            case "spinebase":
                {
                    // Solution using cross product and ignoring initial values (ignoring means: assume zero rotation between hips of kinect avatar)
                    // compute right vector of the spine (hip) rotation using the left and right hip position
                    // Vector3 rightVector = getRawWorldPosition(JointType.HipRight) - getRawWorldPosition(JointType.HipLeft);
                    // forward vector of the spine rotation
                    // Vector3 forward = Vector3.Cross(rightVector, getJointDirection(JointType.SpineBase));
                    // if (forward.x == 0 && forward.y == 0 && forward.z == 0) break;
                    // Quaternion of the hip rotation
                    // Quaternion hipRotation = Quaternion.LookRotation(forward, getJointDirection(JointType.SpineBase));

                    // recommended Solution taking initial rotation of Hips of kinect avatar into account
                    // Note: The initial Rotation of the UnityChan-Hips is already included in the Spine.base rotation (= initialModelJointRotation)!
                    Vector3 rightVector = getRawWorldPosition("hipright") - getRawWorldPosition("hipleft");
                    Quaternion hipRotation = Quaternion.FromToRotation((model_InitialJointPositions["hipright"] - model_InitialJointPositions["hipleft"]), rightVector);
                    // first apply additional hip rotation, then the rotation from the bones
                    avatarInitialToCurrentRotation = hipRotation * avatarInitialToCurrentRotation;
                    break;
                }
            case "spineshoulder":
                {
                    //here you can also use a solution similar to above

                    // same as with the spine
                    Vector3 rightVector = getRawWorldPosition("shoulderright") - getRawWorldPosition("shoulderleft");
                    Vector3 forward = Vector3.Cross(rightVector, Vector3.up);
                    if (forward.x == 0 && forward.y == 0 && forward.z == 0)
                        break;
                    Quaternion chestRotation = Quaternion.LookRotation(forward, Vector3.up);

                    avatarInitialToCurrentRotation = chestRotation * avatarInitialToCurrentRotation;
                    break;
                }
            default:
                // do nothing
                break;
        }


        // because we assured, that the model rotation is always relative to Quaternion.identity, we can simply apply the rotation-change to the original rotation
        Quaternion currentModelJointRotation = avatarInitialToCurrentRotation * initialModelJointRotation;

        return currentModelJointRotation;
    }

    private Vector3 getJointDirection(string jt)
    {
        Vector3 jointPos = getRawWorldPosition(jt);
        Vector3 nextJointPos = getRawWorldPosition(skeletalModelStructure_FromJointToJoint[jt]);  // Get connected joint from skeletal model

        return nextJointPos - jointPos;
    }

    private  Vector3 getRawWorldPosition(string jt)
    {
        // Access updated joint positions
        Vector3 point = model_CurrentJointPositions[jt];
    
        // mirror on X/Y Plane to remove mirroring effect of the kinect data
        return new Vector3(point.x, point.y, -point.z);
    }

    private Vector3 getJointDirectionFromGO(string jt)
    {
        Vector3 jointPos = model_Joints[jt].position;
        Vector3 nextJointPos = model_Joints[skeletalModelStructure_FromJointToJoint[jt]].position; // Get connected joint from skeletal model

        return nextJointPos - jointPos;
    }   
}

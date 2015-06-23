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

    // Dictionary containing all joints
    private Dictionary<string, Transform> model_Joints = new Dictionary<string, Transform>();

    // Use this for initialization
    void Start()
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

            Transform tmpJoint = model_Joints[joint.Key];


            // For now total positions and orientations are used: Ubitrack
            // Later on positions and orientations normalized to the hip are used: Feedback Engine
            // In general local positions and orientations should be used for model update

            // Applying position
            tmpJoint.transform.localPosition = spinebase.position + new Vector3((float)(joint.Value.position.x), (float)(joint.Value.position.y), (float)(joint.Value.position.z));

            // Applying orientation
            //tmpJoint.transform.rotation = new Quaternion((float)joint.Value.rotation.x, (float)joint.Value.rotation.y, (float)joint.Value.rotation.z, (float)joint.Value.rotation.w);

            // Update joint in model
            model_Joints[joint.Key] = tmpJoint;

            Debug.Log(string.Format("{0} {1} {2}\n", joint.Key, joint.Value.position, tmpJoint.transform.position));
        }
    }
}

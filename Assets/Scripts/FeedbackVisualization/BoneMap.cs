using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoneMap : MonoBehaviour {

    public List<Transform> bones = new List<Transform>();
    private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

    void Awake()
    {
        foreach (Transform bone in bones)
        {
            if (!boneMap.ContainsKey(bone.name))
            {
                boneMap.Add(bone.name, bone);
            }
            else
            {
                Debug.Log("Bone already contained in BoneMap!");
            }
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public Dictionary<string, Transform> GetBoneMap()
    {
        return boneMap;
    }
}

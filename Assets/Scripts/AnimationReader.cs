using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;

struct BoneData 
{
    public Vector3 pos;
    public Quaternion rot;
}

public class AnimationReader : MonoBehaviour {

    // The Transforms of every tracked joint at a certain time
    private Dictionary<float, List<BoneData>> AnimationData;
    private List<float> timeSteps;

    // The Avatar the animation is applied to
    public AvatarController avatar;
    private float startTime;
    private bool animationPlaying;
    public bool animationLoop;

	// Use this for initialization
	void Start () {
        AnimationData = new Dictionary<float,List<BoneData>>();
	}
	
	// Update is called once per frame
	void Update () {
        UpdateAnimation();
	}

    void ParseAnimation(string filePath, string fileName)
    {
        var reader = new StreamReader(File.OpenRead(filePath + fileName));
        timeSteps = new List<float>();
        List<BoneData> bones = new List<BoneData>();


        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(';');
            
            float time = float.Parse(values[10]); 
            timeSteps.Add(time);


            for (int i = 0; i < avatar.Bones.Length; i++)
            {
                int j = 0;
                if (avatar.BoneIndex2JointMap.ContainsKey(i))
                {
                    BoneData data;
                    //TODO check if indizes are correct
                    data.pos = new Vector3(float.Parse(values[11 + 18 * j]), float.Parse(values[12 + 18 * j]), float.Parse(values[13 + 18 * j]));
                    data.rot = new Quaternion(float.Parse(values[14 + 18 * j]), float.Parse(values[15 + 18 * j]), float.Parse(values[16 + 18 * j]), float.Parse(values[17 + 18 * j]));
                    bones.Add(data);
                    j++;
                }
            }
            AnimationData.Add(time, bones);
        }
    }

    void UpdateAnimation()
    {
        if (animationPlaying && timeSteps != null && timeSteps.Count > 0)
        {
            float timeFloor = 0;
            float timeCeiling = 0;

            for (int i = 1; i < timeSteps.Count; i++)
            {
                if (timeSteps[i] >= Time.time - startTime)
                {
                    timeFloor = timeSteps[i - 1];
                    timeCeiling = timeSteps[i];
                    break;
                }
            }

            //If animation is complete 
            if (timeCeiling == 0.0f)
            {
                animationPlaying = false;
                if (animationLoop)
                {
                   startTime = Time.time;
                   animationPlaying = true;
                }
                return;
            }

            // Interpolate between floor and ceiling positions/rotations
            for (int i = 0; i < avatar.Bones.Length; i++)
            {
                int j = 0;
                if (avatar.BoneIndex2JointMap.ContainsKey(i))
                {
                    avatar.Bones[i].localPosition = Vector3.Lerp(AnimationData[timeFloor][j].pos, AnimationData[timeCeiling][j].pos, Time.time - startTime);
                    avatar.Bones[i].localRotation = Quaternion.Lerp(AnimationData[timeFloor][j].rot, AnimationData[timeCeiling][j].rot, Time.time - startTime);
                    j++;
                }
            }
        }
    }

    // Call this function from the UI
    public void PlayAnimation(string filePath, string fileName)
    {
        ParseAnimation(filePath, fileName);
        startTime = Time.time;
        animationPlaying = true;
    }

}

using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;

struct BoneData
{
    public BoneData(Vector3 pos, Quaternion rot)
    {
        this.pos = pos;
        this.rot = rot;
    }
    public Vector3 pos;
    public Quaternion rot;
}

public class AnimationReader : MonoBehaviour
{

    // The Transforms of every tracked joint at a certain time
    private List<BoneData> AnimationData;
    private List<float> timeSteps;

    // The Avatar the animation is applied to
    public AvatarController avatar;
    private float startTime;
    private bool animationPlaying;
    public bool animationLoop;
    private char[] seperators = { ';' };

    // Use this for initialization
    void Start()
    {
        AnimationData = new List<BoneData>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateAnimation();
    }

    public bool ParseAnimation(string filePath, string fileName, AvatarController avatarController, bool loop)
    {
        if (fileName == "")
        {
            animationPlaying = false;
            return false;
        }

        var reader = new StreamReader(File.OpenRead(filePath + fileName + ".csv"));

        if (reader == null)
        {
            animationPlaying = false;
            return false;
        }

        animationLoop = loop;
        avatar = avatarController;
        timeSteps = new List<float>();
        

        reader.ReadLine();
        // Parse every line until the end
        if (AnimationData == null)
            AnimationData = new List<BoneData>();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            float time = float.Parse(values[0]);
            timeSteps.Add(time);
            List<BoneData> bones = new List<BoneData>();

            for (int i = 1; i < values.Length; i++)
            {

                var jointData = values[i].Split(',');

                //TODO check if indizes are correct
                float px = float.Parse(jointData[0].Substring(1));
                float py = float.Parse(jointData[1]);
                float pz = float.Parse(jointData[2]);
                float rx = float.Parse(jointData[3]);
                float ry = float.Parse(jointData[4]);
                float rz = float.Parse(jointData[5]);
                float rw = float.Parse(jointData[6].Substring(0, jointData[6].Length - 1));
                BoneData data = new BoneData(new Vector3(px, py, pz), new Quaternion(rx, ry, rz, rw));
                bones.Add(data);

            }
            //AnimationData.Add(bones);
        }
        return true;
    }

    void UpdateAnimation()
    {
        //Only update if there is a parsed animation
        if (animationPlaying && timeSteps != null && timeSteps.Count > 0)
        {
            // We need these for interpolation
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
                    //avatar.Bones[i].localPosition = Vector3.Lerp(AnimationData[timeFloor][j].pos, AnimationData[timeCeiling][j].pos, Time.time - startTime);
                    //avatar.Bones[i].localRotation = Quaternion.Lerp(AnimationData[timeFloor][j].rot, AnimationData[timeCeiling][j].rot, Time.time - startTime);
                    j++;
                }
            }
        }
    }

    public void StopAnimation()
    {
        animationPlaying = false;
    }

}

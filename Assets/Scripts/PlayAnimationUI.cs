using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayAnimationUI : MonoBehaviour {

    public Button playRecordingButton;
    public Button stopButton;
    public GameObject recordingUI;
    public GameObject animationReaderPrefab;
    public InputField kinectFileName;
    public InputField art_ikFileName;
    public AvatarController kinectAvatar;
    public AvatarController artIKAvatar;
    public Toggle animationLoop;
    public bool playInputData;
    private List<AnimationReader> animations;

    public void PlayRecordingButton()
    {
        if (!playInputData)
        {
            animations = new List<AnimationReader>();
            animations.Add((Instantiate(animationReaderPrefab) as GameObject).GetComponent<AnimationReader>());
            if (!animations[0].ParseAnimation("", art_ikFileName.text, artIKAvatar, animationLoop.isOn))
            {
                ParseLastRecording("art_ik");
                Debug.Log("File not found parsing last recording instead");
            }
            if (!animations[0].ParseAnimation("", kinectFileName.text, kinectAvatar, animationLoop.isOn))
            {
                ParseLastRecording("kinect");
                Debug.Log("File not found parsing last recording instead");
            }
        }
        else
        {
            //KinectManager.Instance.StartPlayback(kinectFileName.text);
        }
        recordingUI.SetActive(false);
        playRecordingButton.gameObject.SetActive(false);
        stopButton.gameObject.SetActive(true);
        kinectFileName.gameObject.SetActive(false);
        art_ikFileName.gameObject.SetActive(false);
        animationLoop.gameObject.SetActive(false);
    }

    public void StopAnimation()
    {
        if (!playInputData)
        {
            for (int i = 0; i < animations.Count; i++)
            {
                if (animations[i] != null)
                    animations[i].StopAnimation();
            }
        }
         else
        {
            KinectManager.Instance.EndPlayback();
        }
            recordingUI.SetActive(true);
            playRecordingButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            kinectFileName.gameObject.SetActive(true);
            art_ikFileName.gameObject.SetActive(true);
            animationLoop.gameObject.SetActive(true);
       
    }

    private void ParseLastRecording(string type)
    {
        if (type == "kinect")
        {
            animations[0].ParseAnimation("", PlayerPrefs.GetString("last" + kinectAvatar.name), kinectAvatar, animationLoop.isOn);
        }
        if (type == "art_ik")
        {
            animations[0].ParseAnimation("", PlayerPrefs.GetString("last" + artIKAvatar.name), artIKAvatar, animationLoop.isOn);
        }
    }

}

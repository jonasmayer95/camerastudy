using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecordUI : MonoBehaviour
{

    public Toggle recordInput;
    public Toggle recordKinect;
    public Toggle recordART_IK;
    public Button recordButton;
    public Button endRecordButton;
    public GameObject movementRecorder;
    public GameObject playAnimationUI;
    public AVProMovieCaptureBase movie;

    public void StartRecording()
    {
        if (!recordInput.isOn)
        {
            MovementRecorder.InitializeRecording(recordKinect.isOn, recordART_IK.isOn);
            movementRecorder.SetActive(true);
        }
        else
        {
            KinectManager.Instance.StartRecording();
            movie.StartCapture();
        }

        //Enable/Disable UI elements
        recordKinect.gameObject.SetActive(false);
        recordART_IK.gameObject.SetActive(false);
        endRecordButton.gameObject.SetActive(true);
        recordButton.gameObject.SetActive(false);
        playAnimationUI.SetActive(false);
        recordInput.gameObject.SetActive(false);
    }

    public void EndRecording()
    {
        if (!recordInput.isOn)
        {
            movementRecorder.GetComponent<MovementRecorder>().EndRecording();
            
        }
        else
        {
            KinectManager.Instance.EndRecording();
            movie.StopCapture();
        }
        recordKinect.gameObject.SetActive(true);
        recordART_IK.gameObject.SetActive(true);
        endRecordButton.gameObject.SetActive(false);
        recordButton.gameObject.SetActive(true);
        playAnimationUI.SetActive(true);
        recordInput.gameObject.SetActive(true);
    }
}

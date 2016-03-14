using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecordUI : MonoBehaviour {

    public Toggle recordKinect;
    public Toggle recordART_IK;
    public Button recordButton;
    public Button endRecordButton;
    public GameObject movementRecorder;
    public GameObject playAnimationUI;

    public void StartRecording()
    {
        MovementRecorder.InitializeRecording(recordKinect.isOn, recordART_IK.isOn);
        recordKinect.gameObject.SetActive(false);
        recordART_IK.gameObject.SetActive(false);
        movementRecorder.SetActive(true);
        endRecordButton.gameObject.SetActive(true);
        playAnimationUI.SetActive(false);
    }

    public void EndRecording()
    {
        movementRecorder.GetComponent<MovementRecorder>().EndRecording();
        recordKinect.gameObject.SetActive(true);
        recordART_IK.gameObject.SetActive(true);
        endRecordButton.gameObject.SetActive(false);
        recordButton.gameObject.SetActive(true);
        playAnimationUI.SetActive(true);
    }
}

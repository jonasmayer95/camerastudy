using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public enum Handedness
{
    RightHanded, LeftHanded
}

public class UserStudyUI : MonoBehaviour
{
    public GameObject nameInputField;
    public GameObject trialInputField;
    public GameObject setInputField;
    public GameObject ageInputField;
    public GameObject precisionInputField;
    //public GameObject sexToggleGroup;
    //public GameObject camPerspectivesToggleGroup;
    //public GameObject camMotionToggleGroup;
    //public GameObject handednessToggleGroup;
    //public GameObject feedbackToggleGroup;
    public GameObject submitButton;
    public GameObject colorToggle;
    public GameObject scalingToggle;
    public GameObject loggingDataToggle;
    //the object that contains a MovementRecorder script
    public GameObject userStudyObject;

    // DropdownStats
    public UserStudyUIDropdownMenu genderDropdownMenu;
    public UserStudyUIDropdownMenu handednessDropdownMenu;
    public UserStudyUIDropdownMenu cameraPerspectiveDropdownMenu;
    public UserStudyUIDropdownMenu cameraMotionDropdownMenu;
    public UserStudyUIDropdownMenu feedbackTypeDropdownMenu;

    
    /// <summary>
    /// Gets the data from child controls, validates it and sends it to objects
    /// that rely on that data for initialization.
    /// </summary>
    public void SubmitData()
    {
        try
        {
            string name = nameInputField.GetComponent<InputField>().text;
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name", "is null or an empty string.");

                // TODO: Set some UI indicator, e.g. red input field or something
            }

            uint trial = uint.Parse(trialInputField.GetComponent<InputField>().text);
            uint age = uint.Parse(ageInputField.GetComponent<InputField>().text);
            uint set = uint.Parse(setInputField.GetComponent<InputField>().text);
            float precision = float.Parse(precisionInputField.GetComponent<InputField>().text);

            Sex sex = (Sex)genderDropdownMenu.value;
            CameraPerspectives camPerspective = (CameraPerspectives)cameraPerspectiveDropdownMenu.value;
            CameraMotionStates camMotion = (CameraMotionStates)cameraMotionDropdownMenu.value;
            Handedness hand = (Handedness)handednessDropdownMenu.value;
            CameraFeedbackMode feedbackType = (CameraFeedbackMode)feedbackTypeDropdownMenu.value;
            bool coloring = colorToggle.GetComponent<Toggle>().isOn;
            bool scaling = scalingToggle.GetComponent<Toggle>().isOn;
            // TODO: validate enums (Enum.IsDefined)
            bool loggingData = loggingDataToggle.GetComponent<Toggle>().isOn;
            // We've got valid data, send it to MovementRecoreder
            if (loggingData)
            {
                MovementRecorder.InitializeAndActivateUserStudy(name, trial, set, age, camPerspective, sex);
                userStudyObject.SetActive(true);
            }

            if (feedbackType !=CameraFeedbackMode.BicepsCurl)
            {
                // Init UserStudyLogic component with userspecific data
                UserStudyLogic.instance.InitNewUserStudy(feedbackType, hand, camPerspective, camMotion, precision, this, trial, coloring, scaling);
            }

            else if (feedbackType == CameraFeedbackMode.BicepsCurl)
            {
                UserStudyLogic.instance.userStudyUI = this;
                UserStudyLogic.instance.camFeedbackMode = feedbackType;
                UserStudyLogic.instance.ActivateBicepsCurl();
            }

            this.gameObject.SetActive(false);
        }
        catch (ArgumentException ex)
        {
            Debug.Log(ex.Message);
        }
       
    }


    private uint GetToggleIndex( GameObject toggleGroup)
    {
        uint k = 0;
        for (int i = 0; i < toggleGroup.transform.childCount; i++)
        {
            if(toggleGroup.transform.GetChild(i).GetComponent<Toggle>().isOn)
            {
                k = (uint)i;
                break;
            }
        }
        return k;
    }    
}

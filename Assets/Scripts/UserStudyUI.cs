using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class UserStudyUI : MonoBehaviour
{
    public GameObject nameInputField;
    public GameObject trialInputField;
    public GameObject ageInputField;
    public GameObject sexToggleGroup;
    public GameObject camToggleGroup;
    public GameObject submitButton;

    //the object that contains a MovementRecorder script
    public GameObject userStudyObject;

    /// <summary>
    /// Gets the data from child controls, validates it and sends it to objects
    /// that rely on that data for initialization.
    /// </summary>
    public void SubmitData()
    {
        string name = nameInputField.GetComponent<InputField>().text;
        uint trial = uint.Parse(trialInputField.GetComponent<InputField>().text);
        uint age = uint.Parse(ageInputField.GetComponent<InputField>().text);

        Sex sex = (Sex) GetToggleIndex(sexToggleGroup.GetComponent<ToggleGroup>().ActiveToggles());
        CameraType cam = (CameraType) GetToggleIndex(camToggleGroup.GetComponent<ToggleGroup>().ActiveToggles());

        //alright, and who needs this data now except the avatar for recording? (I should decouple that, btw)
        
        //TODO: validate the data before firing the event


        //ExecuteEvents.Execute<UserStudyMessageTarget>(userStudyObject, null, (x, y) => x.InitializeAndActivateUserStudy(name, trial, age, cam, sex));
        MovementRecorder.InitializeAndActivateUserStudy(name, trial, age, cam, sex);
        userStudyObject.SetActive(true);
    }

    /// <summary>
    /// Gets the index of a Toggle from a ToggleGroup. Given an arbitrary set of
    /// toggles, it returns the first active one.
    /// </summary>
    /// <param name="toggles">A set of toggles.</param>
    /// <returns></returns>
    private uint GetToggleIndex(IEnumerable<Toggle> toggles)
    {
        uint i = 0;

        foreach (Toggle toggle in toggles)
        {
            if (toggle.isOn)
            {
                break;
            }
            ++i;
        }

        return i;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(Dropdown))]
public class PlaybackFileChooser : MonoBehaviour
{
    public KinectRecorder recorder;
    private Dropdown fileChooser;
    private List<string> options;

    void Start()
    {
        fileChooser = gameObject.GetComponent<Dropdown>();
        ListPlaybackFiles();
        SetPlaybackFileName(fileChooser.value);
    }

    /// <summary>
    /// Finds all .csv files in the current working directory.
    /// </summary>
    public void ListPlaybackFiles()
    {
        string[] fileNames = Directory.GetFiles(".", "*.csv");
        options = new List<string>(fileNames);

        if (fileChooser != null)
        {
            fileChooser.ClearOptions();
            fileChooser.AddOptions(options);
            SetPlaybackFileName(fileChooser.value);
        }
    }

    /// <summary>
    /// Sets KinectRecorder's playback file name to the path corresponding to the
    /// position in the dropdown menu.
    /// </summary>
    /// <param name="dropdownIndex">The index in the dropdown menu.</param>
    public void SetPlaybackFileName(int dropdownIndex)
    {
        if (dropdownIndex < fileChooser.options.Count)
        {
            recorder.PlaybackFileName = fileChooser.options[dropdownIndex].text;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(Dropdown))]
public class PlaybackFileChooser : MonoBehaviour
{
    public KinectRecorder recorder;
    public Dropdown streamSelectionDropdown;
    public Dropdown fileChooser;

    private List<string> options;

    void Start()
    {
        fileChooser = gameObject.GetComponent<Dropdown>();
        ListPlaybackFiles();
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
            SetStreamSuffix(streamSelectionDropdown.value);
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

    public void SetStreamSuffix(int dropdownIndex)
    {
        if (dropdownIndex < streamSelectionDropdown.options.Count)
        {
            string streamSuffix;

            switch (dropdownIndex)
            {
                default:
                case 0:
                    streamSuffix = "_color";
                    break;

                case 1:
                    streamSuffix = "_depth";
                    break;

                case 2:
                    streamSuffix = "_infrared";
                    break;
            }

            recorder.StreamSuffix = streamSuffix;
        }
    }
}

using UnityEngine;
using System.Collections;
using SimpleJSON;

public abstract class InseilExercise : MonoBehaviour {

    public FeedbackType enabledFeedBackType;
    public ExerciseConstraint[] exerciseConstraints;


    /// <summary>
    /// Deserializes JSON string of feedback exercise
    /// </summary>
    /// <param name="json"></param>
    public void DeserializeExercise(string json)
    {
        var N = JSON.Parse(json);
        exerciseConstraints = new ExerciseConstraint[N["constraints"].AsArray.Count];
        for (int i = 0; i < exerciseConstraints.Length; i++)
        {
            exerciseConstraints[i] = new ExerciseConstraint(N["constraints"][i]["position"]["x"].AsDouble, N["constraints"][i]["position"]["y"].AsDouble, N["constraints"][i]["position"]["z"].AsDouble,
                (string)N["constraints"][i]["joint"], (string)N["constraints"][i]["base"], (string)N["constraints"][i]["type"], N["constraints"][i]["radius"].AsDouble);
        }
    }
}

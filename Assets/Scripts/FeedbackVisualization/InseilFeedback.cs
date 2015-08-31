using UnityEngine;
using System.Collections;

public enum FeedbackType
{
    BallFeedback, AreaFeedback, ImageFeedback
}

public abstract class InseilFeedback : MonoBehaviour {

    public FeedbackType type;
}

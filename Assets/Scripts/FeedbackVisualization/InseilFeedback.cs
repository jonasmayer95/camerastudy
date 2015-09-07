using UnityEngine;
using System.Collections;

public enum FeedbackType
{
    BallFeedback, AreaFeedback, ImageFeedback, CameraFeedback
}

public abstract class InseilFeedback : MonoBehaviour {

    public FeedbackType type;
}

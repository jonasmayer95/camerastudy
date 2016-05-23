using UnityEngine;


/// <summary>
/// Allows the implementation of generic filter scripts with their own Kinect
/// data sets that can be attached to GameObjects (e.g. avatars). Derives from MonoBehaviour
/// for Start() and OnDestroy() methods (for setup and tear down).
/// </summary>
public abstract class AbstractFilter : MonoBehaviour
{
    //Kinect data set the filter works on
    public KinectInterop.BodyFrameData bodyFrame;

    //Cached KinectManager instance, needed for Init() and Shutdown() among other things
    protected KinectManager kinectManager;

    //Should be set to true after successful init, otherwise the filter won't register with KinectManager
    protected bool initialized;

    /// <summary>
    /// Applies the filter data, i.e. overwrites properties within bodyFrame.
    /// </summary>
    /// <param name="bodyIndex">The primary user body index.</param>
    public abstract void ApplyFilter(int bodyIndex);

    /// <summary>
    /// Initializes filter data.
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// Cleanly shuts down the filter.
    /// </summary>
    protected abstract void Shutdown();

    /// <summary>
    /// Per frame update of filter data (if needed or desired)
    /// </summary>
    protected abstract void UpdateFilter();
}
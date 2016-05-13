using UnityEngine;


/// <summary>
/// Allows the implementation of generic filter scripts that modify KinectManager's
/// data sets and can be attached to GameObjects (e.g. avatars). Derives from MonoBehaviour
/// for Awake() and OnDestroy() methods (for setup and tear down).
/// </summary>
public abstract class AbstractFilter : MonoBehaviour
{
    protected abstract void Init();
    protected abstract void Shutdown();
    protected abstract void UpdateFilter();
    //we might need an UpdateFilter() call e.g. to encapsulate ART recv stuff
    //implementors would call this from their unity Update()

    public abstract void ApplyFilter(ref KinectInterop.BodyData bodyData);

    protected KinectManager kinectManager;
    public KinectInterop.BodyFrameData BodyFrame { get; set; }
}

//filters are applied to avatars (on the fly, just add a filter script)
//that means kinectmanager needs to copy bodyData for each avatar and then
//avatars can apply filters to their data set and visualize using that data set...
//but we still need to look at which bodyData instances are used, as currently reading
//just writes it back to the first position of the BodyData[] in km. so we need to map
//avatarcontrollers to BodyData copies.
//and I need to change this to be an abstract class deriving from MonoBehaviour, otherwise
//I won't be able to attach/remove it to/from avatars. alright, now we can attach and
//remove it at runtime, which is exactly what we want. avatarcontroller needs to call
//the filter on its own data set, and that's all we need
//avatar count is known at compile time, so km knows how many copies it has to maintain.
//filters are then attached/disabled via UI, so we use the avatar's AddComponent()/Destroy()
//for that. km needs to be able to invoke filters, though, as the overwriting needs to take
//place at a certain stage. it could work like this:
//km calls avatarcontroller, which has knowledge of filters as well, and tells it to apply
//all its filters to its own data set.

//alternative approach (without MonoBehaviour):
//provide AddFilter(FilterType)/RemoveFilter() calls in avatarcontroller, call that from UI.
//avatars would have an array of filters somewhere and km would still need to call them, so
//for each avatar apply filter to avatar's data set. But what if filters rely on other
//gameobjects? ART needs a certain gameobject hierarchy to get the calibration done for
//example.
using UnityEngine;
using System.Collections;

/// <summary>
/// A simple test filter that marks feet as not tracked.
/// </summary>
public class DeadFootFilter : AbstractFilter
{

    void Start()
    {
        this.kinectManager = KinectManager.Instance;
        if (kinectManager != null)
        {
            this.bodyFrame = new KinectInterop.BodyFrameData(kinectManager.GetBodyCount(), kinectManager.GetJointCount());
            Init();
        }
        else
        {
            Debug.LogError("kinectmanager is null, destroying filter");
            Destroy(this);
        }
    }

    //in this case Update() isn't needed as we always do the same thing.
    //void Update()
    //{

    //}

    public override void ApplyFilter(int bodyIndex)
    {
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.FootLeft].trackingState = KinectInterop.TrackingState.NotTracked;
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.FootRight].trackingState = KinectInterop.TrackingState.NotTracked;
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.KneeLeft].trackingState = KinectInterop.TrackingState.NotTracked;
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.KneeRight].trackingState = KinectInterop.TrackingState.NotTracked;
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.AnkleLeft].trackingState = KinectInterop.TrackingState.NotTracked;
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.AnkleRight].trackingState = KinectInterop.TrackingState.NotTracked;

        var sensorData = kinectManager.GetSensorData();

        for (int j = (int)KinectInterop.JointType.KneeLeft; j < sensorData.jointCount; ++j)
        {
            KinectInterop.CalculateJointDirection(bodyIndex, j, ref bodyFrame, sensorData);
        }

        print("DeadFootFilter: set feet to not tracked");
    }

    protected override void Init()
    {
        initialized = true;

        kinectManager.filters.Add(this);
    }

    protected override void Shutdown()
    {
        if (kinectManager != null)
        {
            kinectManager.filters.Remove(this);
        }
    }

    protected override void UpdateFilter()
    {
        return;
    }
}

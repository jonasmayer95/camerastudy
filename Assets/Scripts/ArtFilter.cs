using UnityEngine;
using System.Collections;


public class ArtFilter : AbstractFilter
{

    void Awake()
    {
        //if we save our modified data set inside the filter, we need kinectmanager for init
        kinectManager = KinectManager.Instance;
        if (kinectManager != null)
        {
            this.bodyFrame = new KinectInterop.BodyFrameData(kinectManager.GetBodyCount(), kinectManager.GetJointCount());
            Init();
        }
        else
        {
            Debug.LogError("kinectmanager is null, cannot attach filter");
        }
    }

    
    void Update()
    {
        UpdateFilter();
        ApplyFilter();
    }

    void OnDestroy()
    {
        Shutdown();
    }

    protected override void Init()
    {
        print("ArtFilter: initializing");

        //i could attach this to a public property of kinectmanager, so filters would register themselves.
        //that's...pretty good, actually.
        kinectManager.filters.Add(this);
    }

    protected override void Shutdown()
    {
        print("ArtFilter: shutting down");

        kinectManager.filters.Remove(this);
    }

    protected override void UpdateFilter()
    {
        //throw new System.NotImplementedException();
    }

    public override void ApplyFilter()
    {
        print("ArtFilter: applying");
    }
}

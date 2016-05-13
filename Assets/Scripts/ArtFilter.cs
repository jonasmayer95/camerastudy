using UnityEngine;
using System.Collections;


public class ArtFilter : AbstractFilter
{
    //either we save our data inside the filter or inside kinectmanager...hm.
    //public KinectInterop.BodyFrameData

    void Awake()
    {
        //if we save our modified data set inside here, we rely on kinectmanager
        kinectManager = KinectManager.Instance;
        if (kinectManager != null)
        {
            Init();
        }
    }

    
    void Update()
    {
        UpdateFilter();
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
        
    }

    protected override void Shutdown()
    {
        print("ArtFilter: shutting down");
    }

    protected override void UpdateFilter()
    {
        //throw new System.NotImplementedException();
    }

    public override void ApplyFilter(ref KinectInterop.BodyData bodyData)
    {
        print("ArtFilter: applying");
    }
}

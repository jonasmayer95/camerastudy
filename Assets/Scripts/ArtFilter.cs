using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System;


public class ArtFilter : AbstractFilter
{
    public const int BufferSize = 8192; //up this to theoretical UDP limit if required.
    private readonly string[] frameDelimiters = { "\r\n" };
    private readonly string[] bodyDelimiters = { " ", "]", "[" };

    [HideInInspector]
    public byte[] buffer = new byte[BufferSize];

    [HideInInspector]
    public volatile bool terminate = false;

    private Socket artSocket;
    private Thread recvThread;
    private ArtBodyData[] artBodies = new ArtBodyData[2]; //initialize for hands

    //this port doesn't seem to be important, but our local one certainly is, as the server sends to remote port 5000
    private EndPoint trackingEndpoint = new IPEndPoint(IPAddress.Parse("131.159.10.100"), 0);

    private GameObject artObject;
    private Transform squareMarker;

    //Start() is used instead of Awake() because loading order is arbitrary and this
    //way we can be sure that kinectmanager will be initialized before us (if it exists)
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
            Debug.LogError("kinectmanager is null, cannot attach filter");
        }
    }


    void Update()
    {
        UpdateFilter();
        //ApplyFilter(); //this should be called by kinectmanager, so avatarcontroller can access it afterwards
    }

    void OnDestroy()
    {
        Shutdown();
    }

    protected override void Init()
    {
        print("ArtFilter: initializing");

        try
        {
            InitGameObjects();
            InitSocket();
            InitRecvThread();
        }
        catch (SocketException ex)
        {
            if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                initialized = false;
                Debug.LogError("ArtFilter: Address is already used. Close the other application blocking address:port or the filter will not work.");
            }
        }
        catch (Exception ex)
        {
            initialized = false;
            Debug.LogException(ex);
        }


        if (initialized)
        {
            kinectManager.filters.Add(this);
            print("ArtFilter: Init successful");
        }
        else
        {
            Debug.LogError("ArtFilter: Init failed, filter will not run");
        }
    }

    protected override void Shutdown()
    {
        print("ArtFilter: shutting down");

        //kinectManager can be torn down before this, so a null check is needed
        if (kinectManager != null)
        {
            kinectManager.filters.Remove(this);
        }
    }

    protected override void UpdateFilter()
    {
        var data = GetBodyData();

        //we always want 6d poses, otherwise ignore the data
        if (data[0].type == "6d")
        {
            var pos = data[0].pos;
            var rot = data[0].rot;

            artObject.transform.localPosition = new Vector3(-pos.x, pos.y, -pos.z);
            //Debug.Log(string.Format("ArtFilter: ART wrist/marker kinect pos: {0}, world pos: {1}", artObject.transform.localPosition, artObject.transform.position));
            Quaternion test = new Quaternion(-rot.x, -rot.y, rot.z, rot.w);
            artObject.transform.localRotation = test;
        }
    }

    public override void ApplyFilter(int bodyIndex)
    {
        print("ArtFilter: applying");

        //overwrite wrist data with art gameobject data

        var markerKinectPos = squareMarker.position;


        //Debug.Log(string.Format("KinectManager: squareMarker kinectPos: {0}", markerKinectPos));
        //Debug.Log(string.Format("KinectManager: kinectPos, worldPos before setting: {0}, {1}", TestObject.transform.position, bodyFrame.bodyData[index].joint[(int)KinectInterop.JointType.WristRight].position));
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.WristRight].kinectPos = markerKinectPos;
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.WristRight].position = kinectManager.KinectToWorld.MultiplyPoint3x4(markerKinectPos);
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.WristRight].orientation = Quaternion.identity;

        //bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.WristRight].trackingState = KinectInterop.TrackingState.NotTracked;


        //mask right arm components to prevent kinectmanager from calculating wrong directions, use ik instead
        //bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.ShoulderRight].trackingState = KinectInterop.TrackingState.NotTracked;
        //bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.ElbowRight].trackingState = KinectInterop.TrackingState.NotTracked;
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.HandRight].trackingState = KinectInterop.TrackingState.NotTracked;
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.HandTipRight].trackingState = KinectInterop.TrackingState.NotTracked;
        bodyFrame.bodyData[bodyIndex].joint[(int)KinectInterop.JointType.ThumbRight].trackingState = KinectInterop.TrackingState.NotTracked;

        //Debug.Log(string.Format("KinectManager: after setting: {0}, {1}", markerKinectPos, kinectManager.KinectToWorld.MultiplyPoint3x4(markerKinectPos)));
        var sensorData = kinectManager.GetSensorData();

        //recalculate directions because wrist data has been overwritten
        for (int j = 1; j < sensorData.jointCount; ++j)
        {
            KinectInterop.CalculateJointDirection(bodyIndex, (int)KinectInterop.JointType.WristRight, ref bodyFrame, sensorData);
        }

    }

    private void InitRecvThread()
    {
        //set up thread, kick off recv loop
        if (recvThread == null)
        {
            recvThread = new Thread(() => Run());
            recvThread.Start();
            Debug.Log("ArtFilter: started recvThread");
        }
    }

    private void InitSocket()
    {

        //TODO: get local IP instead of using a hardcoded string, like here http://stackoverflow.com/questions/6803073/get-local-ip-address
        artSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        artSocket.Bind(new IPEndPoint(IPAddress.Parse("131.159.10.228"), 5000));
        artSocket.ReceiveTimeout = 3000; //3sec timeout, then close the socket
    }

    private void InitGameObjects()
    {
        artObject = GameObject.Find("ARTTarget");

        if (artObject != null)
        {
            if (artObject.transform.childCount > 0)
            {
                squareMarker = artObject.transform.GetChild(0);
                initialized = true;
            }
        }
    }

    private void Run()
    {
        try
        {
            while (!terminate)
            {
                int bytesRead = artSocket.ReceiveFrom(buffer, BufferSize, SocketFlags.None, ref trackingEndpoint);
                //Debug.Log(Encoding.ASCII.GetString(buffer));
            }

            artSocket.Close();
            Debug.Log("ArtFilter: closed artSocket");
        }
        catch (SocketException ex)
        {
            if (artSocket != null && ex.SocketErrorCode == SocketError.TimedOut)
            {
                artSocket.Close();
                Debug.Log("ArtFilter: closed artSocket due to timeout");
            }
        }
    }

    private ArtBodyData[] GetBodyData()
    {
        string data;

        lock (buffer)
        {
            //parse stuff, fill into our struct, return
            data = Encoding.ASCII.GetString(buffer);
        }

        var lines = data.Split(frameDelimiters, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            int lastPos = 0;

            //find first space
            int findPos = line.IndexOf(' ');

            //invalid line
            if (findPos == -1)
                break;


            string recordType = line.Substring(lastPos, findPos - lastPos);


            //the only useful record type for us
            if (recordType == "6d")
            {
                //get body count (first number after record type)
                lastPos = findPos + 1; //start one char after last space
                findPos = line.IndexOf(' ', lastPos);

                string count = line.Substring(lastPos, findPos - lastPos);
                int bodyCount = int.Parse(count);

                for (int i = 0; i < bodyCount; ++i)
                {
                    //do stuff for each 6d body
                    if (lastPos > line.Length)
                    {
                        Debug.Log("6d: Unexpected end of line");
                        break;
                    }

                    //find first '['
                    if ((lastPos = line.IndexOf('[', lastPos)) == -1)
                    {
                        //there is no body data
                        break;
                    }

                    //find record ending
                    findPos = line.IndexOf("] ", lastPos);

                    string record = line.Substring(lastPos, findPos - lastPos);

                    var positions = record.Split(bodyDelimiters, StringSplitOptions.RemoveEmptyEntries);

                    //parse positions and write them into our struct
                    //rotations seem to be in euler angles
                    //int id;
                    //double qual, px, py, pz, rx, ry, rz;
                    artBodies[i] = new ArtBodyData(recordType, int.Parse(positions[0]), double.Parse(positions[1]),
                        double.Parse(positions[2]), double.Parse(positions[3]), double.Parse(positions[4]), double.Parse(positions[5]), double.Parse(positions[6]),
                        double.Parse(positions[7]));

                    //we could still parse the matrix here, its seems to be used for rotation
                }
            }
        }

        return artBodies;
    }

    void OnApplicationQuit()
    {
        //let the receiveing thread exit gracefully
        terminate = true;
    }
}

public struct ArtBodyData
{
    public ArtBodyData(string type, int id, double qual, double px, double py, double pz, double rx, double ry, double rz)
    {
        this.type = type;
        this.id = id;
        this.qual = qual;

        //convert art measurements (mm) to kinect (m)
        double x = px / 1000.0;
        double y = py / 1000.0;
        double z = pz / 1000.0;

        pos = new Vector3((float)x, (float)y, (float)z);
        rot = Quaternion.Euler((float)rx, (float)ry, (float)rz);

    }


    public string type;
    public int id;
    public double qual;
    public Vector3 pos;
    public Quaternion rot;
}
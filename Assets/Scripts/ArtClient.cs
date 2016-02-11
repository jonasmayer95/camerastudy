using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


/// <summary>
/// Connects to an ART server and receives its tracking data.
/// </summary>
class ArtClient : MonoBehaviour
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
    

    void Awake()
    {
        //TODO: exception handling here
        artSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        artSocket.Bind(new IPEndPoint(IPAddress.Parse("131.159.10.200"), 5000)); //TODO: make this more resilient if the port is used
        artSocket.ReceiveTimeout = 3000; //3sec timeout, then close the socket

        //set up thread, kick off recv loop
        if (recvThread == null)
        {
            recvThread = new Thread(() => Run());
            recvThread.Start();
            Debug.Log("ArtClient: started recvThread");
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
            Debug.Log("ArtClient: closed artSocket");
        }
        catch (SocketException ex)
        {
            if (artSocket != null && ex.SocketErrorCode == SocketError.TimedOut)
            {
                artSocket.Close();
                Debug.Log("ArtClient: closed artSocket due to timeout");
            }
        }
    }

    void Update()
    {
        var data = GetBodyData();

        //we always want 6d poses, otherwise ignore the data
        if (data[0].type == "6d")
        {
            var pos = data[0].pos;
            var rot = data[0].rot;

            this.transform.localPosition = new Vector3(-pos.x, pos.y, -pos.z );
            Debug.Log(string.Format("ART wrist/marker kinect pos: {0}, world pos: {1}", this.transform.localPosition, this.transform.position));
            Quaternion test = new Quaternion(-rot.x, -rot.y, rot.z, rot.w);
            this.transform.localRotation = test;
        }
    }

    public ArtBodyData[] GetBodyData()
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


            //we've got our record type!
            if (recordType == "fr")
            {
                //frame logic
            }
            else if (recordType == "3d")
            {
                //Debug.Log(data);

                //we should calculate a mean value of all marker positions or something, otherwise this case is pretty fucking useless
            }
            else if (recordType == "6d")
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
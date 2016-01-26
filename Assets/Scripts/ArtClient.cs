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
    //public ArtClientState State { get { return state; } }
    //private ArtClientState state = new ArtClientState();

    public const int BufferSize = 8192; //up this to theoretical UDP limit if required.
    private readonly string[] frameDelimiters = { "\r\n" };
    private readonly string[] bodyDelimiters = { " ", "]", "[" };
    public byte[] buffer = new byte[BufferSize];

    public volatile bool dataReceived = false;
    public volatile bool terminate = false;

    private Socket artSocket;
    private Thread recvThread;
    private List<ArtBodyData> artBodies = new List<ArtBodyData>(2); //initialize for hands

    //this port doesn't seem to be important, but our local one certainly is, as the server sends to remote port 5000
    private EndPoint trackingEndpoint = new IPEndPoint(IPAddress.Parse("131.159.10.100"), 0);


    void Awake()
    {
        //TODO: exception handling here
        artSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        artSocket.Bind(new IPEndPoint(IPAddress.Parse("131.159.10.200"), 5000)); //TODO: make this more resilient if the port is used

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
                Debug.Log(Encoding.ASCII.GetString(buffer));
            }


            artSocket.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message.ToString());
        }
    }

    public List<ArtBodyData> GetBodyData()
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
            else if (recordType == "ts")
            {

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

struct ArtBodyData
{
    ArtBodyData(string type, int id, double qual, double px, double py, double pz, double rx, double ry, double rz)
    {
        this.type = type;
        this.id = id;
        this.qual = qual;
        pos = new InseilPosition(px, py, pz);
        rot = Quaternion.Euler((float)rx, (float)ry, (float)rz);
    }

    string type;
    int id;
    double qual;
    InseilPosition pos; //either use floats here as well or do that inside kinectmanager...hmm
    Quaternion rot; //this sucks but there is no method for doubles
}
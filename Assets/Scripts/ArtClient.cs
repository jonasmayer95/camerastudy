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
    public ArtClientState State { get { return state; } }
    private ArtClientState state = new ArtClientState();

    public volatile bool dataReceived = false;
    public bool shutDown = false;

    private Socket artSocket;

    //this port doesn't seem to be important, but our local one certainly is, as the server sends to remote port 5000
    private EndPoint trackingEndpoint = new IPEndPoint(IPAddress.Parse("131.159.10.100"), 0);

    //public ArtClient()
    void Awake()
    {
        artSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        artSocket.Blocking = false;
        artSocket.Bind(new IPEndPoint(IPAddress.Parse("131.159.10.200"), 5000)); //TODO: make this more resilient if the port is used
    }

    void Update()
    {
        //only do this when we're done with the previous recv. this is reset by kinectmanager when it got its data
        if (!dataReceived)
        {
            Receive(state);
        }
    }

    public void Receive(ArtClientState stateObject)
    {
        try
        {
            stateObject.socket = artSocket;

            artSocket.BeginReceiveFrom(stateObject.buffer, 0, ArtClientState.BufferSize, SocketFlags.None, ref trackingEndpoint,
                new AsyncCallback(ReceiveCallback), stateObject);
            
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    /// <summary>
    /// Called when we receive body data from the ART server.
    /// </summary>
    /// <param name="ar">The async state object</param>
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            ArtClientState stateObject = (ArtClientState)ar.AsyncState;
            Socket client = stateObject.socket;

            int bytesRead = client.EndReceiveFrom(ar, ref trackingEndpoint);

            //parse data, extract useful stuff, write into some variable useful to kinectmanager and carry on
            stateObject.sb.Append(Encoding.ASCII.GetString(stateObject.buffer));
            //Debug.Log(stateObject.sb.ToString());

            //signal when data has been processed
            dataReceived = true;

            //kick off a new receive if we're still running
            //if (!shutDown)
            //{
            //    client.BeginReceiveFrom(stateObject.buffer, 0, ArtClientState.BufferSize, SocketFlags.None, ref trackingEndpoint,
            //        new AsyncCallback(ReceiveCallback), stateObject);
            //    Debug.Log("Kicked off new recv from callback");
            //}
            //else Shutdown();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    //private void Shutdown()
    void OnApplicationQuit()
    {
        artSocket.Close();
        shutDown = true;
    }
}

class ArtClientState
{
    public const int BufferSize = 8192; //up this to theoretical UDP limit if required.

    public Socket socket;
    public byte[] buffer = new byte[BufferSize];
    public StringBuilder sb = new StringBuilder(BufferSize);

    /// <summary>
    /// Resets the contents of our StringBuilder instance.
    /// </summary>
    public void Reset()
    {
        sb.Remove(0, sb.Length);
    }
}
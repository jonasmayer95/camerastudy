using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


/// <summary>
/// Connects to an ART server and receives its tracking data.
/// </summary>
class ArtClient
{
    private Socket artSocket;
    private EndPoint trackingEndpoint = new IPEndPoint(IPAddress.Parse("131.159.10.100"), 5000);

    public volatile bool dataReceived;

    public ArtClient()
    {
        artSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        //get ip address, set up listening, listen each frame (from somewhere inside kinectmanager,
        //so we can combine the data more easily)
    }

    public void Receive(ArtClientState stateObject)
    {
        //clear stringbuilder before receiving new data instead of allocating new instance
        //stateObject.Reset();
        stateObject.socket = artSocket;

        artSocket.BeginReceiveFrom(stateObject.buffer, 0, ArtClientState.BufferSize, 0, ref trackingEndpoint, new AsyncCallback(ReceiveCallback), stateObject);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            ArtClientState stateObject = (ArtClientState)ar.AsyncState;
            Socket client = stateObject.socket;

            //reads the first enqueued datagram in the network buffer according to
            //https://msdn.microsoft.com/en-us/library/w7wtt64b(v=vs.110).aspx
            int bytesRead = client.EndReceiveFrom(ar, ref trackingEndpoint);

            //parse data, extract useful stuff, write into some variable useful to kinectmanager and carry on
            stateObject.sb.Append(Encoding.ASCII.GetString(stateObject.buffer));
            
            //assume our useful data is in a stringbuilder

            //signal when data has been processed
            dataReceived = true;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
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
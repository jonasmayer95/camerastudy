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
    private IPEndPoint trackingEndpoint = new IPEndPoint(new IPAddress(new byte[] {131,159,10,200}), 5000);

    public ArtClient()
    {
        //artSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IPv4);
        
        //get ip address, connect, set up listening, listen each frame (from somewhere inside kinectmanager,
        //so we can combine the data more easily)
    }

    public void Connect()
    {
        artSocket.BeginConnect(trackingEndpoint, new AsyncCallback(ConnectCallback), artSocket);
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndConnect(ar);

            Debug.Log(client.RemoteEndPoint.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public void Receive(ArtClientState stateObject)
    {
        //clear stringbuilder before receiving new data instead of allocating new instance
        stateObject.Reset();
        stateObject.socket = artSocket;

        artSocket.BeginReceive(stateObject.buffer, 0, ArtClientState.BufferSize, 0, new AsyncCallback(ReceiveCallback), stateObject);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            ArtClientState state = (ArtClientState)ar.AsyncState;
            Socket client = state.socket;

            //reads the first enqueued datagram in the network buffer according to
            //https://msdn.microsoft.com/en-us/library/w7wtt64b(v=vs.110).aspx
            int bytesRead = client.EndReceive(ar);

            //parse data, extract useful stuff, write into some variable useful to kinectmanager and carry on
            state.sb.Append(Encoding.ASCII.GetString(state.buffer));
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
    public StringBuilder sb;

    /// <summary>
    /// Resets the contents of our StringBuilder instance.
    /// </summary>
    public void Reset()
    {
        sb.Remove(0, sb.Length);
    }
}
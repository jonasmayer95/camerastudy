﻿using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;
using NetJSON;
using System.Threading;
using System.Diagnostics;

public class ServerCommunication : MonoBehaviour
{
    public static NetMQContext Context
    { 
        get { return ctx; } 
    }

    public GameObject utObject;
    private NetworkedKinectManager utManager;

    private static NetMQContext ctx;
    private SubscriberSocket client;
    private PairSocket threadCommSocket;

    private NetMQMessage serverMessage;
    private InseilMessage im;
    private string json;
    private bool recv;

    private static string serverTopic;
    private static string serverUrl;
    private string inprocAddress = "inproc://avatarupdate";

    private static Thread commThread;
    private static volatile bool terminate = false;
    private volatile bool setupComplete = false;

    //private Stopwatch sw = new Stopwatch();

    void Awake()
    {

        im = new InseilMessage();
        NetJSON.NetJSON.IncludeFields = true;
        NetJSON.NetJSON.SkipDefaultValue = false;


        if (commThread == null)
        {
            commThread = new Thread(() => Run());
        }
        commThread.Start();


        utManager = utObject.GetComponent<NetworkedKinectManager>();
        if (utManager != null)
        {
            //we need to wait for bind before we can connect. that means waiting
            //for the second thread to set up its socket.
            while (!setupComplete)
                Thread.Sleep(1);

            utManager.SetupSocket(inprocAddress);
        }
    }

    void ServerUpdate()
    {
        
        recv = client.TryReceiveMultipartMessage(ref serverMessage);

        if (recv)
        {
            //sw.Start();

            //assuming we get everything in a single frame
            json = serverMessage.First.ConvertToString();

            im = NetJSON.NetJSON.Deserialize<InseilMessage>(json);
            //sw.Stop();

            //convert inseilmessage to byte buffer and send it down the inproc socket
            var data = im.measurement.ToByteArray();
            //UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
            threadCommSocket.Send(data);
        }
        else Thread.Sleep(0);

        //sw.Reset();
    }

    void OnApplicationQuit()
    {
        utManager.SocketShutdown();
        Shutdown();
    }

    private void Run()
    {
        UnityEngine.Debug.Log("ServerCommunication: started servercomm thread");
        terminate = false;

        if (client == null)
        {
            client = ctx.CreateSubscriberSocket();
        }

        client.Options.ReceiveHighWatermark = 1000;
        string addr = string.Concat("tcp://", serverUrl);
        UnityEngine.Debug.Log(addr);
        client.Connect(addr);
        client.Subscribe(serverTopic);

        if (threadCommSocket == null)
        {
            threadCommSocket = ctx.CreatePairSocket();
        }

        threadCommSocket.Bind(inprocAddress);
        setupComplete = true;

        while (!terminate)
        {
            ServerUpdate();
        }

        //clean up sockets here
        if (threadCommSocket != null)
        {
            threadCommSocket.Dispose();
            UnityEngine.Debug.Log("ServerCommunication: threadCommSocket disposed");
        }

        if (client != null)
        {
            client.Dispose();
            UnityEngine.Debug.Log("ServerCommunication: client disposed");
        }
    }

    /// <summary>
    /// Sets up NetMQ context and SUB socket, then connects and subscribes to the
    /// given topic. Returns true on success and false if any NetMQ method throws
    /// an exception. Static because otherwise Unity UI wouldn't be able to call
    /// this when the parent object is disabled.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="url">The publisher's IP and port.</param>
    /// <returns></returns>
    public static bool ClientConnect(string topic, string url)
    {

        try
        {
            if (ctx == null)
            {
                ctx = NetMQContext.Create();
            }

            //do error checking here and return false if something's wrong so the UI
            //doesn't disappear
            serverTopic = topic;
            serverUrl = url;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Properly disposes of NetMQ sockets and context.
    /// </summary>
    public static void Shutdown()
    {
        
        terminate = true;

        //wait for other thread to dispose of its sockets
        if (commThread != null)
        {
            while (commThread.IsAlive)
            {
                Thread.Sleep(1);
            }
        }

        if (ctx != null)
        {
            ctx.Dispose();
            UnityEngine.Debug.Log("ServerCommunication: context disposed");
        }
    }
}

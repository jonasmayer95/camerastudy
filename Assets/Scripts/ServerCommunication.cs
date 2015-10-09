using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;
using NetJSON;
using System.Threading;

public class ServerCommunication : MonoBehaviour
{
    private static NetMQContext ctx;
    private static SubscriberSocket client;
    private static PairSocket threadCommSocket;
    private NetMQMessage serverMessage;
    private InseilMessage im;
    private string json;
    private bool recv;

    private static Thread commThread;
    private static volatile bool terminate;

    void Awake()
    {
        //instance = this;
        im = new InseilMessage();
        NetJSON.NetJSON.IncludeFields = true;
        NetJSON.NetJSON.SkipDefaultValue = false;

        //alright, we need another inproc socket pair and a thread. server comm, deserialization and
        //sending the deserialized byte buffer should be run in that thread. that means we need to set up
        //when waking up and getting rid of Update().
    }

    void ServerUpdate()
    {
        recv = client.TryReceiveMultipartMessage(ref serverMessage);

        if (recv)
        {
            //assuming we get everything in a single frame
            json = serverMessage.First.ConvertToString();

            //Deserialize<InseilMessage>(json, ref im);
            //Deserialize(json, ref im);
            im = NetJSON.NetJSON.Deserialize<InseilMessage>(json);

            //uncomment as soon as the code is ubitrack-ready
            //UbitrackManager.instance.UpdateInseilMeasurement(im.measurement);
            //UbitrackManager.instance.recievedData = true;
            //UbitrackManager.instance.GenerateBodyData(im.measurement);

            //send a giant byte buffer down the inproc socket
        }
    }

    void OnApplicationQuit()
    {
        Shutdown();
    }

    private static void Run(string topic, string url)
    {
        terminate = false;

        if (client == null)
        {
            client = ctx.CreateSubscriberSocket();
        }

        client.Options.ReceiveHighWatermark = 1000;
        string addr = string.Concat("tcp://", url);
        Debug.Log(addr);
        client.Connect(addr);
        client.Subscribe(topic);

        if (threadCommSocket == null)
        {
            threadCommSocket = ctx.CreatePairSocket();
        }

        threadCommSocket.Bind("inproc://avatarupdate");

        while (!terminate)
        {
            //do communication here. i still need a solution for static/instance members,
            //as this is quite messy and i could use some instance members in here. maybe i should
            //refactor some classes a bit.
        }

        //clean up sockets here
        if (client != null)
        {
            client.Dispose();
            Debug.Log("client disposed");
        }

        if (threadCommSocket != null)
        {
            threadCommSocket.Dispose();
            Debug.Log("threadcommsocket disposed");
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
        //write passed params to instance members, create context and let awake do the rest.
        //should work better with instance members and comm.

        try
        {
            if (ctx == null)
            {
                ctx = NetMQContext.Create();
            }

            if (commThread == null)
            {
                commThread = new Thread(() => Run(topic, url));
            }
            commThread.Start();

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
        while (commThread.IsAlive)
        {
            Thread.Sleep(1);
        }

        if (ctx != null)
        {
            ctx.Dispose();
            Debug.Log("context disposed");
        }
    }
}

using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;
using FullSerializer;
using NetJSON;

public class ServerCommunication : MonoBehaviour
{
    private static NetMQContext ctx;
    private static SubscriberSocket client;
    private NetMQMessage serverMessage;
    private InseilMessage im;
    private string json;
    private bool recv;

    private static readonly fsSerializer serializer = new fsSerializer();

    public static ServerCommunication instance;
    


    void Awake()
    {
        instance = this;
        im = new InseilMessage();
        NetJSON.NetJSON.IncludeFields = true;
        NetJSON.NetJSON.SkipDefaultValue = false;
    }

    void Update()
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
            UbitrackManager.instance.GenerateBodyData(im.measurement);
        }
    }

    void OnApplicationQuit()
    {
        Shutdown();
    }

    /// <summary>
    /// Sets up NetMQ context and SUB socket, then connects and subscribes to the
    /// given topic. Returns true on success and false if any NetMQ method throws
    /// an exception.
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

            if (client == null)
            {
                client = ctx.CreateSubscriberSocket();
            }

            client.Options.ReceiveHighWatermark = 1000;
            string addr = string.Concat("tcp://", url);
            Debug.Log(addr);
            client.Connect(addr);
            client.Subscribe(topic);

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
        if (client != null)
        {
            client.Dispose();
            Debug.Log("client disposed");
        }

        if (ctx != null)
        {
            ctx.Dispose();
            Debug.Log("context disposed");
        }
    }

    ///// <summary>
    ///// Deserializes a JSON string into an instance of the passed type.
    ///// </summary>
    ///// <typeparam name="T">The type into which to deserialize</typeparam>
    ///// <param name="instance">An instance of T</param>
    ///// <param name="json">The JSON string to deserialize</param>
    //private void Deserialize<T>(string json, ref T instance)
    //{
    //    fsData data = fsJsonParser.Parse(json);

    //    //change this to AssertSuccess to disable exception handling
    //    serializer.TryDeserialize<T>(data, ref instance).AssertSuccessWithoutWarnings();
    //}



    //private void Deserialize(string json, ref InseilMessage msg)
    //{
    //    msg.measurement.data.Clear();

    //    var jsonObj = JSON.Parse(json);
    //    msg.version = uint.Parse(jsonObj["protocol_version"].Value);
    //    msg.id = jsonObj["id"].Value;
    //    msg.sendtime = (ulong)jsonObj["sendtime"].AsDouble;

    //    msg.measurement.timestamp = (ulong)jsonObj["measurement"]["timestamp"].AsDouble;

    //    //InseilJoint tmp = new InseilJoint();
    //    JSONNode joints = jsonObj["measurement"]["data"];

    //    foreach (var key in joints.Keys)
    //    {
    //        //nice and simple thanks to http://answers.unity3d.com/questions/648066/how-to-get-the-keys-of-a-dictionary-.html

    //        var joint = new InseilJoint(joints[key]["p"]["x"].AsDouble, joints[key]["p"]["y"].AsDouble, joints[key]["p"]["z"].AsDouble,
    //            joints[key]["r"]["x"].AsDouble, joints[key]["r"]["y"].AsDouble, joints[key]["r"]["z"].AsDouble, joints[key]["r"]["w"].AsDouble);
            
    //        msg.measurement.data.Add(key, joint);
    //    }
    //}
}

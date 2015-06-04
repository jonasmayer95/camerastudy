using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;
using FullSerializer;

public class ServerCommunication : MonoBehaviour
{
    private static NetMQContext ctx;
    private static SubscriberSocket client;
    private NetMQMessage serverMessage;
    private bool recv;

    private static readonly fsSerializer serializer = new fsSerializer();
    private InseilMessage message;

    void Update()
    {
        recv = client.TryReceiveMultipartMessage(ref serverMessage);

        if (recv)
        {
            Debug.Log(string.Format("Frames: {0}", serverMessage.FrameCount));
            foreach (NetMQFrame fr in serverMessage)
            {
                Debug.Log(fr.ConvertToString());
            }
            
            //TODO: get message as string, feed it into a json parser and pass
            //the relevant data to the visualization component
            //you should find some useful info on how to do that here:
            //https://github.com/jacobdufault/fullserializer
            
            //the access pattern for single joints would be e.g.:
            //message.measurement.data["spinebase"].position.x
            //deserialize into the message class variable and you're good to go.

            //i'm not quite sure how the serializer will handle the dictionary,
            //but i think it should be rather straightforward. don't take my word
            //for it, though.

            //testing might be a bit of a problem since we don't have a server up and running,
            //but you could just copy/paste that json from the protocol spec into my
            //server sample (look it up at Doc/) and send that every second, just to verify
            //the serializer is not doing some weird shit.

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
}

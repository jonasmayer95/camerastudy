using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;

public class ServerCommunication : MonoBehaviour
{
    private static NetMQContext ctx;
    private static SubscriberSocket client;

    private NetMQMessage message;
    private bool recv;


    void Update()
    {
        recv = client.TryReceiveMultipartMessage(ref message);

        if (recv)
        {
            Debug.Log(string.Format("Frames: {0}", message.FrameCount));
            foreach (NetMQFrame fr in message)
            {
                Debug.Log(fr.ConvertToString());
            }
            
            //TODO: get message as string, feed it into a json parser and pass
            //the relevant data to the visualization component
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

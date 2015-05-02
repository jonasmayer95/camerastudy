using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;

public class ServerCommunication : MonoBehaviour
{
    private NetMQContext ctx;
    private SubscriberSocket client;
    private string topic;
    private string serverAddress;

    private NetMQMessage message;
    private bool recv;

    void Update()
    {
        recv = client.TryReceiveMultipartMessage(ref message);
        if (recv)
        {
            Debug.Log("Got a message; parse it and pass it on to whatever component does the visualization");
        }
    }

    void OnApplicationQuit()
    {
        if (client != null)
            client.Dispose();

        if (ctx != null)
            ctx.Dispose();
    }

    /// <summary>
    /// Sets up NetMQ context and SUB socket, then connects and subscribes to the
    /// given topic. Works only once, before a context is instantiated.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="url">The publisher's IP and port.</param>
    public void SetupConnection(string topic, string url)
    {
        if (ctx != null)
        {
            ctx = NetMQContext.Create();
            client = ctx.CreateSubscriberSocket();

            client.Options.ReceiveHighWatermark = 1000;
            client.Connect(string.Concat("tcp://", url));
            client.Subscribe(topic);
            gameObject.SetActive(true);
        }
    }
}

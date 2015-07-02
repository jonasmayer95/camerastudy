﻿using UnityEngine;
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
    
    public GameObject avatar;
    private AvatarController avatarController;

    void Awake()
    {
        if (avatar != null)
        {
            avatarController = avatar.GetComponent<AvatarController>();
        }
    }

    void Update()
    {
        recv = client.TryReceiveMultipartMessage(ref serverMessage);

        if (recv)
        {
            //assuming we get everything in a single frame
            string json = serverMessage.First.ConvertToString();

            InseilMessage message = new InseilMessage();
            Deserialize<InseilMessage>(json, ref message);


            Debug.Log(message.measurement.data["spinebase"].ToString());

            //uncomment as soon as the code is ubitrack-ready
            //avatarController.UpdateInseilAvatar(message.measurement);
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

    /// <summary>
    /// Deserializes a JSON string into an instance of the passed type.
    /// </summary>
    /// <typeparam name="T">The type into which to deserialize</typeparam>
    /// <param name="instance">An instance of T</param>
    /// <param name="json">The JSON string to deserialize</param>
    private void Deserialize<T>(string json, ref T instance)
    {
        fsData data = fsJsonParser.Parse(json);

        //change this to AssertSuccess to disable exception handling
        serializer.TryDeserialize<T>(data, ref instance).AssertSuccessWithoutWarnings();
    }
}

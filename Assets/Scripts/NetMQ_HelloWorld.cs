using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;

public class NetMQ_HelloWorld : MonoBehaviour
{
    //context and sockets are IDisposable, don't forget to release them on shutdown!
    NetMQContext ctx;
    ResponseSocket server;
    RequestSocket client;
    string message;
    bool sendRequest = true;

    void Start()
    {
        ctx = NetMQContext.Create();
        server = ctx.CreateResponseSocket();
        client = ctx.CreateRequestSocket();

        server.Bind("tcp://localhost:5555");
        client.Connect("tcp://localhost:5555");

    }


    void Update()
    {
        //send client request, recv on the server, send back if the message is right
        if (sendRequest)
        {
            Debug.Log("client: sending hello");
            client.Send("hello");
            sendRequest = false;
        }
        else
        {
            var retVal = server.TryReceiveFrameString(System.TimeSpan.FromMilliseconds(1000), out message);

            if (retVal && message == "hello")
            {
                Debug.Log("server: received hello, sending back world");
                server.Send("world");

                retVal = client.TryReceiveFrameString(System.TimeSpan.FromMilliseconds(1000), out message);
                if (retVal && message == "world")
                {
                    Debug.Log("it's working!");
                    sendRequest = true;
                }
            }
        }

        //...why is this code so convoluted, you ask? well, first of all the REQ/REP pattern is not well
        //suited to our use case anyway, since you can't send two messages in a row without getting exceptions
        //(which is intentional in this case). Secondly, I cannot use "using" statements for automatic socket
        //disposal, because we have Start() and Update() instead of having a "main" function; that means
        //we need to set up stuff in Start() before Update() can do its thing, and when we're done, everything
        //needs to go away in OnApplicationQuit(). Otherwise we'd be recreating and tearing down stuff each
        //frame and I highly doubt that's what we need.
    }


    void OnApplicationQuit()
    {
        client.Dispose();
        server.Dispose();
        ctx.Dispose();
    }

    void OneShotTest()
    {
        //minimal testing code I found here: http://pastebin.com/EHvt4Na7
        //comment out everything above and call this in Start() to test
        //using (NetMQContext ctx = NetMQContext.Create())
        //{
        //    using (var server = ctx.CreateResponseSocket())
        //    {
        //        server.Bind("tcp://127.0.0.1:5556");
        //        using (var client = ctx.CreateRequestSocket())
        //        {
        //            client.Connect("tcp://127.0.0.1:5556");
        //            client.Send("Hello" + Random.Range(0, 100).ToString());

        //            string m1 = server.ReceiveString();
        //            Debug.Log("From Client: " + m1.ToString());
        //            server.Send("Hi " + m1.ToString());

        //            string m2 = client.ReceiveString();
        //            Debug.Log("From Server: " + m2.ToString());
        //        }
        //    }
        //}
    }
}

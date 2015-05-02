using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConnectUI : MonoBehaviour
{
    private ServerCommunication comm;
    private Text addressInput;
    private Text topicInput;

    void Start()
    {
        comm = GetComponent<ServerCommunication>();
        addressInput = transform.FindChild("AddressInputField").GetComponent<Text>();
        topicInput = transform.FindChild("TopicInputField").GetComponent<Text>();
    }

    public void Connect()
    {
        //check both strings for correct formatting and pass them on
        //as args for ServerCommunication

        //comm.SetupConnection(topic, url);
    }
}

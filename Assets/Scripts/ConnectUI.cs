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
        addressInput = transform.FindChild("AddressInputField").GetChild(1).GetComponent<Text>();
        topicInput = transform.FindChild("TopicInputField").GetChild(1). GetComponent<Text>();
    }

    public void Connect()
    {
        //check both strings for correct formatting and pass them on
        //as args for ServerCommunication
        string topic = topicInput.text;
        string url = addressInput.text;

        //comm.SetupConnection(topic, url);
        Debug.Log(string.Format("Connecting to {0}, subscribing to {1}", url, topic));
    }
}

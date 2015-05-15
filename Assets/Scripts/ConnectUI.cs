using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConnectUI : MonoBehaviour
{
    public GameObject commObj;
    private Text addressInput;
    private Text topicInput;

    void Start()
    {
        addressInput = transform.FindChild("AddressInputField").GetChild(1).GetComponent<Text>();
        topicInput = transform.FindChild("TopicInputField").GetChild(1). GetComponent<Text>();
    }

    public void Connect()
    {
        //check both strings for correct formatting and pass them on
        //as args for ServerCommunication
        string topic = topicInput.text;
        string url = addressInput.text;


        if (ServerCommunication.ClientConnect(topic, url))
        {
            commObj.SetActive(true);
            this.gameObject.SetActive(false);
        }
        else
        {
            //dispose of NetMQ objects, otherwise the editor will freeze when compiling scripts
            ServerCommunication.Shutdown();
        }
        
    }
}

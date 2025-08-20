using Rocworks.Mqtt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class to manage MQTT client operations in Unity.
/// </summary>
public class MqttManager : MonoBehaviour
{
    public static MqttManager Instance { get; private set; }

    public void Awake()
    {
        Debug.Log("MqttManager.Awake called");
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnConnect()
    {
        Debug.Log("MqttManager.OnConnect called");
        INAClientMqtt.InitializeINAParameters();
    }

    public void OnDisconnect()
    {
        Debug.Log("MqttManager.OnDisconnect called");
        INAClientMqtt.ResetINAParameters();
    }


    public void Publish(string topic, string payload, int qos = 0, bool retain = false)
    {
        MqttClient mqttClient = MqttClient.Instance;
        if (mqttClient != null && mqttClient.Connection != null && mqttClient.Connection.GetConnectState())
        {
            mqttClient.Connection.Publish(topic, payload, qos, retain);
        }
    }

    public void Publish(string topic, byte[] payload, int qos = 0, bool retain = false)
    {
        MqttClient mqttClient = MqttClient.Instance;
        if (mqttClient != null && mqttClient.Connection != null && mqttClient.Connection.GetConnectState())
        {
            mqttClient.Connection.Publish(topic, payload, qos, retain);
        }
    }
}

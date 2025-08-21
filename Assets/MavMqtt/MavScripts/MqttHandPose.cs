using Rocworks.Mqtt;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

// ====== Data Models ======
[Serializable]
public class Root
{
    [JsonProperty("Keypoints")]
    public List<HandKeypoints> Keypoints { get; set; }
}

[Serializable]
public class HandKeypoints
{
    [JsonProperty("Keypoints")]
    public List<Vector3> Keypoints { get; set; }

    [JsonProperty("Scores")]
    public List<float> Scores { get; set; }

    [JsonProperty("Label")]
    public int Label { get; set; }
}

// ====== Custom Converter for Vector3 ======
public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // Expecting [x,y,z]
        var arr = serializer.Deserialize<List<float>>(reader);
        if (arr != null && arr.Count == 3)
            return new Vector3(arr[0], arr[1], -arr[2]);
        return Vector3.zero;
    }

    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        writer.WriteValue(value.x);
        writer.WriteValue(value.y);
        writer.WriteValue(value.z);
        writer.WriteEndArray();
    }
}


public class MqttHandPose : MonoBehaviour
{
    // Event that others can subscribe to
    public static event Action<List<Vector3>> OnKeypointsReceived;

    private JsonSerializerSettings jsonSettings;

    public void Start()
    {
        // Configure JSON to use our Vector3 converter
        jsonSettings = new JsonSerializerSettings();
        jsonSettings.Converters.Add(new Vector3Converter());

        // Subscribe to topics
        MqttClient mqttClient = MqttClient.Instance;
        mqttClient.SubscribeTopics.Add("Ina/HandPoseDetected");
        mqttClient.SubscribeTopics.Add("Ina/HandPoseKeyPoints");

        mqttClient.OnMessageArrived.AddListener(OnMessageArrived);
    }

    public void OnMessageArrived(MqttMessage m)
    {
        if (m.GetTopic() == "Ina/HandPoseDetected")
        {
        }
        else if (m.GetTopic() == "Ina/HandPoseKeyPoints")
        {
            ProcessKeyPoints(m.GetString());
        }
    }

    public void ProcessKeyPoints(string json)
    {
        try
        {
            Root data = JsonConvert.DeserializeObject<Root>(json, jsonSettings);

            if (data?.Keypoints == null || data.Keypoints.Count == 0)
                return;

            List<Vector3> points = data.Keypoints[0].Keypoints;

            if (points == null || points.Count == 0)
                return;

            // Broadcast the keypoints to all listeners
            OnKeypointsReceived?.Invoke(points);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse keypoints JSON: " + ex.Message);
        }
    }
}

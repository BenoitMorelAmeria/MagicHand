
using System;
using Rocworks.Mqtt;
using UnityEngine;
using System.Globalization;

/// <summary>
/// Helper class with static methods to handle MQTT communication with the INA client
/// </summary>
public class INAClientMqtt
{

    /// <summary>
    /// Configure INA for the current application on startup
    /// </summary>
    public static void InitializeINAParameters()
    {
        float hoverDistance = 0.25f;
        bool multiPointer = true; // we need multi-pointer for the zoom
        bool touchInjectorEnabled = true; 
        bool clickSoundEnabled = false; // disable click sound
        int inaMode = 0; // point and click mode
        int minActiveCount = 0; // this variable introduces a slight delay when user starts interacting, we dont want it
        SetTouchlessTouchConfig(hoverDistance, multiPointer);
        SetTouchInjectorConfig(touchInjectorEnabled, clickSoundEnabled);
        SetInaMode(inaMode);
        SetMinActiveCount(minActiveCount);
    }

    /// <summary>
    /// Reset INA parameters to default values
    /// </summary>
    public static void ResetINAParameters()
    {
        float hoverDistance = 0.25f;
        bool multiPointer = false;
        bool touchInjectorEnabled = false;
        bool clickSoundEnabled = true;
        int inaMode = 1;
        int minActiveCount = -1; // let INA decide the default value
        SetTouchlessTouchConfig(hoverDistance, multiPointer);
        SetTouchInjectorConfig(touchInjectorEnabled, clickSoundEnabled);
        SetInaMode(inaMode);
        SetMinActiveCount(minActiveCount);
    }

    public static void SetTouchlessTouchConfig(float hoverDistance, bool multiPointer)
    {
        string payload = "{\"TouchlessTouch\":{\"HoverDistance\":" + hoverDistance.ToString(CultureInfo.InvariantCulture) + "," + "\"UseMultiplePointer\":" + multiPointer.ToString().ToLower() + "}}";
        MqttManager.Instance.Publish("Ina/SetConfig", payload, 1, true);
    }

    public static void SetTouchInjectorConfig(bool enabled, bool enableClickSound)
    {
        bool disabled = !enabled;
        string payload = "{\"TouchInjector\":{\"IsDisabled\":" + disabled.ToString().ToLower() + ","
            + "\"IsClickSoundEnabled\":" + enableClickSound.ToString().ToLower() + "}}";
        MqttManager.Instance.Publish("Ina/SetConfig", payload, 1, true);
    }

    public static void SetInaMode(int mode)
    {
        string payload = mode.ToString();
        MqttManager.Instance.Publish("Ina/SetMode", payload, 1, true);
    }

    public static void SetMinActiveCount(int count = -1)
    {
        MqttManager.Instance.Publish("Ina/SetMinActiveCount", count.ToString(), 1, true);
    }
}
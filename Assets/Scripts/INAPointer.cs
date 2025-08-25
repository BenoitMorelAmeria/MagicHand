using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class INAPointer : MonoBehaviour
{

    public Vector2 relativeCursorPos = Vector2.zero;
    public float depthInMeters = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        MqttHandPose.OnInaPointerInfoReceived += OnInaPointerInfoReceived;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnInaPointerInfoReceived(InaPointerInfo info)
    {
        relativeCursorPos = new Vector2(info.x / Screen.width, info.y / Screen.height);
        depthInMeters = -info.distToScreen;
    }


}



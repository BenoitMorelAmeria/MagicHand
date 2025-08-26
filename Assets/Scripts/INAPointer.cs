using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class INAPointer : MonoBehaviour
{

    public Vector3 pointer3D;

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

        pointer3D = new Vector3(info.pointer3DX, info.pointer3DY, -info.pointer3DZ);
    }


}



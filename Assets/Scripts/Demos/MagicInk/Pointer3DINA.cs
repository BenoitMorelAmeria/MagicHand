using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer3DÎNA : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private INAPointer inaPointer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    { 
        transform.position = inaPointer.pointer3D;
    }

}

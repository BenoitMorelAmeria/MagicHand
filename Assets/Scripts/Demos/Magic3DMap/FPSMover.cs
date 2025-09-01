using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSMover : MonoBehaviour
{
    [SerializeField] CameraManager cameraManager;
    [SerializeField] Transform sceneToMove;
    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] bool moveCamera = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateKeyboard();
        UpdateCameraFromTarget();

       
    }

    private void UpdateKeyboard()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += Vector3.forward * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position += Vector3.back * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += Vector3.right * Time.deltaTime * moveSpeed;
        }
    }

    private void UpdateCameraFromTarget()
    {
        if (moveCamera)
        {
            cameraManager.mainCamera.transform.position = transform.position;
            cameraManager.mainCamera.transform.rotation = transform.rotation;   
        } else
        {
            sceneToMove.position = -transform.position;
            sceneToMove.rotation = Quaternion.Inverse(transform.rotation);
        }
    }
}

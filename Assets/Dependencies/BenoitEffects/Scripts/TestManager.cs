using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{

    [SerializeField] GameObject objectToMove;
    [SerializeField] GameObject objectToRotate;


    public float rotationSpeed = 50f;
    private float currentAngle = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (objectToMove != null)
        {
            // Move the object to a new position
            objectToMove.transform.position = new Vector3(Mathf.Sin(Time.time) * 3, 0, 0);
        }
        if (objectToRotate != null)
        {
            // Rotate the object around its Y-axis
            float rotationThisFrame = rotationSpeed * Time.deltaTime;
            if (currentAngle + rotationThisFrame >= 180f)
            {
                // Reset rotation and angle
                objectToRotate.transform.rotation = Quaternion.identity;
                currentAngle = 0f;
            }
            else
            {
                objectToRotate.transform.Rotate(Vector3.up, rotationThisFrame);
                currentAngle += rotationThisFrame;
            }
        }
    }
    
}

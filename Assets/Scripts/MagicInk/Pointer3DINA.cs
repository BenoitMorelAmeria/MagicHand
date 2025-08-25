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
        transform.position = Compute3DPosition();
    }

    private Vector3 Compute3DPosition()
    {
        Vector2 relativeMousePose = inaPointer.relativeCursorPos;
        return GetIntersectionWithZ(Camera.main, relativeMousePose, inaPointer.depthInMeters);
    }


    public static Vector3 GetIntersectionWithZ(Camera cam, Vector2 normalizedScreenPos, float Z)
    {
        // Convert normalized (0..1) position to pixel coordinates
        Vector3 screenPoint = new Vector3(
            normalizedScreenPos.x * cam.pixelWidth,
            normalizedScreenPos.y * cam.pixelHeight,
            0f
        );

        // Create a ray from the camera through that screen position
        Ray ray = cam.ScreenPointToRay(screenPoint);

        // Define the z=0 plane (normal pointing forward along +Z axis)
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, Z));

        // Check intersection
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return new Vector3(0, 0, 0); // Return a default value if no intersection is found
    }

    private static Vector3 Convert2DTo3DVector(Vector2 pos2D)
    {
        Vector3 res = new Vector3();
        float screenAspectRatio = (float)Screen.width / (float)Screen.height;
        res.x = pos2D.x * 2.0f - 1.0f;
        res.y = pos2D.y * 2.0f - 1.0f;
        res.z = 0.0f;
        float factor = 1.6f;
        res.x *= factor * screenAspectRatio;
        res.y *= factor;
        return res;
    }
}

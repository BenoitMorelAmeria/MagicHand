using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MagicHand : MonoBehaviour
{
    public float sphereSize = 0.02f;       // Size of each keypoint sphere
    public Material sphereMaterial;        // Optional material
    public bool showDebugSpheres = true;   // Toggle visibility

    private List<Rigidbody> keypointBodies = new List<Rigidbody>();

    private void OnEnable()
    {
        MqttHandPose.OnKeypointsReceived += UpdateHand;
        InitSpheres(21); // we know it’s always 21 points
    }

    private void OnDisable()
    {
        MqttHandPose.OnKeypointsReceived -= UpdateHand;
        ClearSpheres();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }

    private void InitSpheres(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform, false);
            sphere.transform.localScale = Vector3.one * sphereSize;

            if (sphereMaterial != null)
                sphere.GetComponent<Renderer>().material = sphereMaterial;

            sphere.layer = LayerMask.NameToLayer("MagicHand");


            // Physics setup
            Rigidbody rb = sphere.AddComponent<Rigidbody>();
            rb.isKinematic = true; // we move it manually
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            keypointBodies.Add(rb);

            if (!showDebugSpheres)
                sphere.GetComponent<Renderer>().enabled = false;
        }
    }

    private void ClearSpheres()
    {
        foreach (var rb in keypointBodies)
        {
            if (rb != null)
                Destroy(rb.gameObject);
        }
        keypointBodies.Clear();
    }

    private void UpdateHand(List<Vector3> keypoints)
    {
        if (keypoints.Count != keypointBodies.Count)
        {
            Debug.LogWarning("Mismatch between keypoints and physics bodies!");
            return;
        }

        for (int i = 0; i < keypoints.Count; i++)
        {
            Rigidbody rb = keypointBodies[i];
            Vector3 targetPos = transform.TransformPoint(keypoints[i]); // local → world

            // Use physics-safe movement
            rb.MovePosition(targetPos);
        }
    }
}

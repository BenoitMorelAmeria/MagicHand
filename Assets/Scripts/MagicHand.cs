using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MagicHand : MonoBehaviour
{
    [Header("Visuals")]
    public float sphereSize = 0.02f;
    public float cylinderRadius = 0.01f;
    public Material sphereMaterial;
    public Material cylinderMaterial;
    public bool showDebugSpheres = true;
    public bool showDebugCylinders = true;

    [Header("Hand definition")]
    [SerializeField] private List<Vector2Int> jointPairs = new List<Vector2Int>();

    [SerializeField] LayerMask handLayer;

    // Define pairs in the inspector, e.g. (0,1), (1,2), (2,3), ...

    private List<Rigidbody> keypointBodies = new List<Rigidbody>();
    private List<Rigidbody> jointBodies = new List<Rigidbody>();
    private List<Collider> keypointTriggers = new List<Collider>();

    // Hardcoded 21 keypoints
    private List<Vector3> initialKeypoints = new List<Vector3>
    {
        new Vector3( 0.050387f,  0.003815f, 0.219794f),
        new Vector3( 0.016194f,  0.013025f, 0.201708f),
        new Vector3(-0.008271f,  0.028810f, 0.181446f),
        new Vector3(-0.025817f,  0.047654f, 0.165032f),
        new Vector3(-0.044655f,  0.064870f, 0.157437f),
        new Vector3( 0.024341f,  0.067810f, 0.155142f),
        new Vector3( 0.019073f,  0.086579f, 0.128462f),
        new Vector3( 0.016844f,  0.096253f, 0.110984f),
        new Vector3( 0.014550f,  0.105442f, 0.095362f),
        new Vector3( 0.046899f,  0.066796f, 0.152860f),
        new Vector3( 0.048075f,  0.084298f, 0.123741f),
        new Vector3( 0.047478f,  0.092350f, 0.104096f),
        new Vector3( 0.047232f,  0.101823f, 0.087587f),
        new Vector3( 0.066940f,  0.058970f, 0.155252f),
        new Vector3( 0.071292f,  0.075266f, 0.127693f),
        new Vector3( 0.072811f,  0.085965f, 0.109357f),
        new Vector3( 0.073190f,  0.096290f, 0.093030f),
        new Vector3( 0.083589f,  0.043744f, 0.160998f),
        new Vector3( 0.092157f,  0.057866f, 0.141037f),
        new Vector3( 0.096973f,  0.065895f, 0.127748f),
        new Vector3( 0.099914f,  0.075193f, 0.114938f)
    };

    private void OnEnable()
    {
        InitSpheres(initialKeypoints);
        InitCylinders(initialKeypoints);

        MqttHandPose.OnKeypointsReceived += UpdateHand;
    }

    private void OnDisable()
    {
        MqttHandPose.OnKeypointsReceived -= UpdateHand;
        ClearSpheres();
        ClearCylinders();
    }
    private void InitSpheres(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            // --- Physics Sphere ---
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform, false);
            sphere.transform.localScale = Vector3.one * sphereSize;

            if (sphereMaterial != null)
                sphere.GetComponent<Renderer>().material = sphereMaterial;

            sphere.layer = Mathf.RoundToInt(Mathf.Log(handLayer.value, 2));
            Rigidbody rb = sphere.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.MovePosition(transform.TransformPoint(pos));
            keypointBodies.Add(rb);

            if (!showDebugSpheres)
                sphere.GetComponent<Renderer>().enabled = false;

            // --- Trigger Sphere ---
            GameObject triggerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            triggerSphere.transform.SetParent(transform, false);
            triggerSphere.transform.localScale = Vector3.one * sphereSize * 0.9f; // slightly smaller
            triggerSphere.transform.position = transform.TransformPoint(pos);

            Collider triggerCol = triggerSphere.GetComponent<Collider>();
            triggerCol.isTrigger = true;
            triggerCol.gameObject.layer = sphere.layer; // same layer as hand
            keypointTriggers.Add(triggerCol);

            // Hide renderer
            triggerSphere.GetComponent<Renderer>().enabled = false;

            // Optionally add a script/component to handle OnTriggerEnter/Stay/Exit
            // triggerSphere.AddComponent<KeypointTriggerHandler>();
        }
    }
    private void InitCylinders(List<Vector3> positions)
    {
        foreach (var pair in jointPairs)
        {
            if (pair.x < 0 || pair.x >= positions.Count || pair.y < 0 || pair.y >= positions.Count)
                continue;

            Vector3 p1 = transform.TransformPoint(positions[pair.x]);
            Vector3 p2 = transform.TransformPoint(positions[pair.y]);
            Vector3 mid = (p1 + p2) / 2f;
            Vector3 dir = p2 - p1;
            float length = dir.magnitude;

            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.transform.SetParent(transform, false);
            cyl.layer = Mathf.RoundToInt(Mathf.Log(handLayer.value, 2));
            Collider col = cyl.GetComponent<Collider>();
            col.isTrigger = true;

            // Cylinder in Unity points up the Y axis
            cyl.transform.position = mid;
            cyl.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            cyl.transform.localScale = new Vector3(cylinderRadius, length / 2f, cylinderRadius);

            if (cylinderMaterial != null)
                cyl.GetComponent<Renderer>().material = cylinderMaterial;

            Rigidbody rb = cyl.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            jointBodies.Add(rb);

            if (!showDebugCylinders)
                cyl.GetComponent<Renderer>().enabled = false;
        }
    }

    private void ClearSpheres()
    {
        foreach (var rb in keypointBodies)
            if (rb != null) Destroy(rb.gameObject);
        keypointBodies.Clear();
    }

    private void ClearCylinders()
    {
        foreach (var rb in jointBodies)
            if (rb != null) Destroy(rb.gameObject);
        jointBodies.Clear();
    }
    private void UpdateHand(List<Vector3> keypoints)
    {
        if (keypoints.Count != keypointBodies.Count)
        {
            Debug.LogWarning("Mismatch between keypoints and physics bodies!");
            return;
        }

        // --- Update spheres ---
        for (int i = 0; i < keypoints.Count; i++)
        {
            Rigidbody rb = keypointBodies[i];
            Vector3 targetPos = transform.TransformPoint(keypoints[i]);
            rb.MovePosition(targetPos);

            // Move the trigger sphere
            Collider trigger = keypointTriggers[i];
            trigger.transform.position = targetPos;
        }

        // --- Update cylinders using the SAME keypoints ---
        for (int i = 0; i < jointPairs.Count; i++)
        {
            var pair = jointPairs[i];
            if (pair.x < 0 || pair.x >= keypoints.Count || pair.y < 0 || pair.y >= keypoints.Count)
                continue;

            Vector3 p1 = transform.TransformPoint(keypoints[pair.x]);
            Vector3 p2 = transform.TransformPoint(keypoints[pair.y]);
            Vector3 mid = (p1 + p2) / 2f;
            Vector3 dir = p2 - p1;
            float length = dir.magnitude;

            Rigidbody rb = jointBodies[i];
            rb.MovePosition(mid);
            if (dir != Vector3.zero)
                rb.MoveRotation(Quaternion.FromToRotation(Vector3.up, dir));

            rb.transform.localScale = new Vector3(cylinderRadius, length / 2f, cylinderRadius);
        }
    }


}

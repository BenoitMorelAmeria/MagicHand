using System.Collections.Generic;
using UnityEngine;

public class MagicHandPhysics : MonoBehaviour
{
    [SerializeField] private LayerMask handLayer;
    [SerializeField] private PhysicMaterial physicMaterial;
    [SerializeField] private float sphereRadius = 0.01f;
    [SerializeField] private float cylinderRadius = 0.01f;
    [SerializeField] private List<Vector2Int> jointPairs = new List<Vector2Int>();

    private List<Rigidbody> keypointBodies = new List<Rigidbody>();
    private List<Rigidbody> jointBodies = new List<Rigidbody>();
    private List<Collider> keypointTriggers = new List<Collider>();

    public void Init(List<Vector3> initialKeypoints)
    {
        InitKeypoints(initialKeypoints);
        InitJoints(initialKeypoints);
    }

    private void InitKeypoints(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = new Vector3(sphereRadius, sphereRadius, sphereRadius);
            sphere.transform.SetParent(transform, false);

            SetPhysics(sphere);
            SetTrigger(sphere);

            Rigidbody rb = sphere.GetComponent<Rigidbody>();
            rb.MovePosition(transform.TransformPoint(pos));
            keypointBodies.Add(rb);
        }
    }

    private void InitJoints(List<Vector3> positions)
    {
        foreach (var pair in jointPairs)
        {
            if (pair.x < 0 || pair.x >= positions.Count || pair.y < 0 || pair.y >= positions.Count)
                continue;

            Vector3 p1 = positions[pair.x];
            Vector3 p2 = positions[pair.y];
            Vector3 mid = (p1 + p2) / 2f;
            Vector3 dir = p2 - p1;
            float length = dir.magnitude;

            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.transform.SetParent(transform, false);



            cyl.transform.position = mid;
            cyl.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            cyl.transform.localScale = new Vector3(cylinderRadius, length / 2f, cylinderRadius);

            SetPhysics(cyl);
            jointBodies.Add(cyl.GetComponent<Rigidbody>());
        }
    }

    private void SetPhysics(GameObject go)
    {
        go.layer = Mathf.RoundToInt(Mathf.Log(handLayer.value, 2));

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        Collider col = go.GetComponent<Collider>();
        col.material = physicMaterial;
    }

    private void SetTrigger(GameObject go)
    {
        Collider trigger = go.GetComponent<Collider>();
        trigger.isTrigger = true;
        keypointTriggers.Add(trigger);
    }

    public void UpdatePhysics(List<Vector3> keypoints)
    {
        if (keypoints.Count != keypointBodies.Count)
        {
            Debug.LogWarning("Mismatch between keypoints and rigidbodies! " + keypoints.Count + " " + keypointBodies.Count);
            return;
        }   

        // update spheres
        for (int i = 0; i < keypoints.Count; i++)
        {
            Vector3 target = keypoints[i];// transform.TransformPoint(keypoints[i]);
            keypointBodies[i].MovePosition(target);
            keypointTriggers[i].transform.position = target;
        }

        // update cylinders
        for (int i = 0; i < jointPairs.Count; i++)
        {
            var pair = jointPairs[i];
            Vector3 p1 = keypoints[pair.x]; // transform.TransformPoint(keypoints[pair.x]);
            Vector3 p2 = keypoints[pair.y]; // transform.TransformPoint(keypoints[pair.y]);
            Vector3 mid = (p1 + p2) / 2f;
            Vector3 dir = p2 - p1;
            float length = dir.magnitude;

            Rigidbody rb = jointBodies[i];
            rb.MovePosition(mid);
            if (dir != Vector3.zero)
                rb.MoveRotation(Quaternion.FromToRotation(Vector3.up, dir));

            Vector3 globalScale = new Vector3(cylinderRadius, length / 2f, cylinderRadius);
            SetGlobalScale(jointBodies[i].transform, globalScale);

        }
    }
    public static void SetGlobalScale(Transform transform, Vector3 globalScale)
    {
        if (transform.parent == null) transform.localScale = globalScale;
        else
        {
            var parentScale = transform.parent.lossyScale;
            transform.localScale = new Vector3(
                globalScale.x / parentScale.x,
                globalScale.y / parentScale.y,
                globalScale.z / parentScale.z
            );
        }
    }
}

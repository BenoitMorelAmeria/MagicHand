using System.Collections.Generic;
using UnityEngine;

public class MagicHandPhysics : MonoBehaviour
{
    [SerializeField] private LayerMask handLayer;
    [SerializeField] private PhysicMaterial physicMaterial;
    [SerializeField] private float sphereRadius = 0.01f;
    [SerializeField] private float cylinderRadius = 0.01f;
    [SerializeField] private List<Vector2Int> jointPairs = new List<Vector2Int>();
    [SerializeField] private bool showPhysics;

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
            sphere.GetComponent<Renderer>().enabled = showPhysics;
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

            cyl.GetComponent<Renderer>().enabled = showPhysics;


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
        rb.isKinematic = false;
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

        float positionSpring = 100f; // adjust for stiffness
        float positionDamping = 5f;

        float rotationSpring = 50f;   // adjust for rotation response
        float rotationDamping = 1f;

        // Update spheres with forces
        for (int i = 0; i < keypoints.Count; i++)
        {
            Rigidbody rb = keypointBodies[i];
            Vector3 target = keypoints[i];

            // Position spring force
            Vector3 displacement = target - rb.position;
            Vector3 springForce = displacement * positionSpring;
            Vector3 dampingForce = -rb.velocity * positionDamping;
            rb.AddForce(springForce + dampingForce, ForceMode.Force);

            // Update trigger visuals
            keypointTriggers[i].transform.position = rb.position;
        }

        // Update cylinders with torque forces
        for (int i = 0; i < jointPairs.Count; i++)
        {
            var pair = jointPairs[i];
            Vector3 p1 = keypoints[pair.x];
            Vector3 p2 = keypoints[pair.y];
            Vector3 mid = (p1 + p2) / 2f;
            Vector3 dir = p2 - p1;
            float length = dir.magnitude;

            Rigidbody rb = jointBodies[i];

            // Position spring for mid-point
            Vector3 displacement = mid - rb.position;
            Vector3 springForce = displacement * positionSpring;
            Vector3 dampingForce = -rb.velocity * positionDamping;
            rb.AddForce(springForce + dampingForce, ForceMode.Force);

            // Rotation spring
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, dir);
                Quaternion deltaRot = targetRot * Quaternion.Inverse(rb.rotation);
                deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180f) angle -= 360f;
                if (Mathf.Abs(angle) > 0.01f)
                {
                    axis.Normalize();
                    Vector3 desiredAngularVel = axis * (angle * Mathf.Deg2Rad * rotationSpring);
                    Vector3 torque = (desiredAngularVel - rb.angularVelocity) * rotationDamping;
                    rb.AddTorque(torque, ForceMode.Force);
                }
            }

            // Scale update
            Vector3 globalScale = new Vector3(cylinderRadius, length / 2f, cylinderRadius);
            SetGlobalScale(rb.transform, globalScale);
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

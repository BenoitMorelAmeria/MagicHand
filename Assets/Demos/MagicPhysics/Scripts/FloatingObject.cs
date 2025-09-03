using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatingObject : MonoBehaviour
{
    [Header("Position Spring")]
    public float springStrength = 0.2f;  // small for tiny cubes
    public float damping = 0.1f;

    [Header("Rotation Spring")]
    public float angularSpringStrength = 0.02f; // small for tiny cubes
    public float angularDamping = 0.01f;
    public float maxTorque = 0.05f;

    private Rigidbody rb;
    private Vector3 originPos;
    private Quaternion originRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = 0f;
        rb.angularDrag = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Store world position and rotation
        originPos = transform.position;
        originRot = transform.rotation;
    }

    void FixedUpdate()
    {
        // --- Position Spring ---
        Vector3 displacement = originPos - transform.position;
        Vector3 springForce = displacement * springStrength;
        Vector3 dampingForce = -rb.velocity * damping;
        rb.AddForce(springForce + dampingForce, ForceMode.Force);

        // --- Rotation Spring ---
        Quaternion deltaRot = originRot * Quaternion.Inverse(transform.rotation);

        deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f)
            angle -= 360f; // shortest path

        if (Mathf.Abs(angle) > 0.01f)
        {
            axis.Normalize();
            float angleRad = angle * Mathf.Deg2Rad;

            Vector3 desiredAngularVel = axis * (angleRad * angularSpringStrength);
            Vector3 torque = (desiredAngularVel - rb.angularVelocity) * angularDamping;

            if (torque.magnitude > maxTorque)
                torque = torque.normalized * maxTorque;

            rb.AddTorque(torque, ForceMode.Force);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
            rb.MoveRotation(originRot);
        }
    }
}

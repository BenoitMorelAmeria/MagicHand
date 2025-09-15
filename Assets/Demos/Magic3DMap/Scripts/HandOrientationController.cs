using UnityEngine;

public class HandOrientationController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] public MagicHandGestures magicHandGestures;

    [Header("Hand pose")]
    [SerializeField] private float handZCenter = 0.2f;
    [SerializeField] private Vector3 handPoseTranslationSpeed = Vector3.zero;
    [SerializeField] private Vector3 handDeadZone = Vector3.zero;

    [SerializeField] private Vector3 interactionAreaMin = new Vector3(-1, -1, -1);
    [SerializeField] private Vector3 interactionAreaMax = new Vector3(1, 1, 1);
    [SerializeField] private float rotationSpeed  = 1;


    private bool mouseVisible = false;

    // Rotation state
    private Quaternion handNeutral = Quaternion.LookRotation(-Vector3.forward, Vector3.down);




    void Update()
    {
        HandleHandPos();
    }


    private bool IsPinching()
    {
        float dist = Vector3.Distance(
            magicHandGestures.magicHand.Data.GetKeypointScreenSpace(8),
            magicHandGestures.magicHand.Data.GetKeypointScreenSpace(4)
        );
        return dist < 0.03f;
    }


    static float ApplyDeadZone(float pos, float deadZone)
    {
        if (Mathf.Abs(pos) < deadZone)
        {
            return 0;
        }
        else if (pos > 0)
        {
            return pos - deadZone;
        }
        else
        {
            return pos + deadZone;
        }
    }

    private bool IsHandInInteractionArea()
    {
        Vector3 pos = magicHandGestures.magicHand.Data.GetKeypointScreenSpace(9);
        return (pos.x >= interactionAreaMin.x && pos.x <= interactionAreaMax.x) &&
               (pos.y >= interactionAreaMin.y && pos.y <= interactionAreaMax.y) &&
               (pos.z >= interactionAreaMin.z && pos.z <= interactionAreaMax.z);
    }

    public void HandleHandPos()
    {
        if (!magicHandGestures.magicHand.IsAvailable() || !IsHandInInteractionArea())
            return;




        // translation using the relative position
        Vector3 handPosePos = magicHandGestures.magicHand.Data.GetKeypointScreenSpace(9);
        Vector3 relativePos = handPosePos - new Vector3(0, 0, handZCenter);
        relativePos.x = ApplyDeadZone(relativePos.x, handDeadZone.x);
        relativePos.y = ApplyDeadZone(relativePos.y, handDeadZone.y);
        relativePos.z = ApplyDeadZone(relativePos.z, handDeadZone.z);
        transform.position += transform.rotation * Vector3.Scale(relativePos, handPoseTranslationSpeed) * Time.deltaTime;


        /*
        // rotate left-right using palm orientation around Y axis and up-down around X axis
        Vector3 handForward = magicHandGestures.palmForward;
        Vector3 handRight = magicHandGestures.palmRight;
        Vector3 handUp = Vector3.Cross(handForward, handRight);
        Quaternion handRot = Quaternion.LookRotation(handForward, handUp);

        // Get the rotated forward vector of the hand
        Vector3 handDir = (handRot * Vector3.forward);
        handDir.y = 0f;
        handDir.Normalize();

        // Do the same for the neutral orientation
        Vector3 neutralDir = (handNeutral * Vector3.forward);
        neutralDir.y = 0f;
        neutralDir.Normalize();

        // Signed angle around Y axis
        float yAngle = Vector3.SignedAngle(neutralDir, handDir, Vector3.up);
        float deltaRotationX = yAngle * leftRightRotationSpeed * Time.deltaTime;
        transform.rotation = transform.rotation * Quaternion.AngleAxis(deltaRotationX, Vector3.up);

        // Now the same for the up-down rotation around the right axis of the hand
        handDir = (handRot * Vector3.forward);
        handDir = Vector3.ProjectOnPlane(handDir, handRight);
        handDir.Normalize();
        neutralDir = (handNeutral * Vector3.forward);
        neutralDir = Vector3.ProjectOnPlane(neutralDir, handRight);
        neutralDir.Normalize();
        float xAngle = Vector3.SignedAngle(neutralDir, handDir, handRight);
        float deltaRotationY = xAngle * upDownRotationSpeed * Time.deltaTime;
        transform.rotation = transform.rotation * Quaternion.AngleAxis(deltaRotationY, Vector3.right);

        */

        /*

        // Construct current hand rotation
        Vector3 handForward = magicHandGestures.palmForward;
        Vector3 handRight = magicHandGestures.palmRight;
        Vector3 handUp = Vector3.Cross(handForward, handRight);
        Quaternion handRot = Quaternion.LookRotation(handForward, handUp);

        // Compute relative rotation between neutral and current hand
        Quaternion relativeRot = handRot * Quaternion.Inverse(handNeutral);
        // Extract the "delta" from relativeRot
        float angle;
        Vector3 axis;
        relativeRot.ToAngleAxis(out angle, out axis);

        // Clamp to avoid big jumps (e.g. >180° wrap-around)
        if (angle > 180f) angle -= 360f;

        // Apply scaled delta
        transform.rotation = transform.rotation * Quaternion.AngleAxis(
            angle * rotationSpeed * Time.deltaTime,
            axis
        );
        */

        // Build current hand rotation
        Vector3 handForward = magicHandGestures.palmForward;
        Vector3 handRight = magicHandGestures.palmRight;
        Vector3 handUp = Vector3.Cross(handForward, handRight);
        Quaternion handRot = Quaternion.LookRotation(handForward, handUp);

        // Relative rotation: from neutral to current
        Quaternion relativeRot = handRot * Quaternion.Inverse(handNeutral);

        // Extract Euler angles
        Vector3 euler = relativeRot.eulerAngles;

        // Convert Unity’s [0..360] range into signed angles [-180..180]
        if (euler.x > 180f) euler.x -= 360f;
        if (euler.y > 180f) euler.y -= 360f;

        // Constrain: keep only X (pitch) and Y (yaw), drop Z (roll)
        Vector3 constrainedEuler = new Vector3(euler.x, euler.y, 0f);

        // Scale by your own speeds if you want sensitivity control
        float deltaX = constrainedEuler.x * rotationSpeed * Time.deltaTime;
        float deltaY = constrainedEuler.y * rotationSpeed * Time.deltaTime;

        // Apply the deltas gradually
        transform.rotation =
            transform.rotation *
            Quaternion.AngleAxis(deltaY, Vector3.up) *
            Quaternion.AngleAxis(deltaX, Vector3.right);
    }

}

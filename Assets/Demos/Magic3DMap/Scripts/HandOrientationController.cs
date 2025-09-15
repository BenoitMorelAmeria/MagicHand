using UnityEngine;

public class HandOrientationController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] public MagicHandGestures magicHandGestures;

    [Header("Hand pose")]
    [SerializeField] private Vector3 neutralPosition = new Vector3(-0.2f, 0.1f, 0.0f);
    [SerializeField] private Vector3 handPoseTranslationSpeed = Vector3.zero;
    [SerializeField] private Vector3 handDeadZone = Vector3.zero;

    [SerializeField] private Vector3 interactionAreaMin = new Vector3(-1, -1, -1);
    [SerializeField] private Vector3 interactionAreaMax = new Vector3(1, 1, 1);

    [SerializeField] private Vector3 handNeutralEuler = new Vector3(0f, 180f, 180f);
    [SerializeField] private float rotationSpeed  = 1;


    private bool mouseVisible = false;

    // Rotation state
    private Quaternion handNeutral = Quaternion.LookRotation(-Vector3.forward, Vector3.down);


    private void Awake()
    {
        handNeutral = Quaternion.Euler(handNeutralEuler);
    }

    private void OnValidate()
    {
        handNeutral = Quaternion.Euler(handNeutralEuler);
    }

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
        Vector3 relativePos = handPosePos - neutralPosition;
        relativePos.x = ApplyDeadZone(relativePos.x, handDeadZone.x);
        relativePos.y = ApplyDeadZone(relativePos.y, handDeadZone.y);
        relativePos.z = ApplyDeadZone(relativePos.z, handDeadZone.z);
        transform.position += transform.rotation * Vector3.Scale(relativePos, handPoseTranslationSpeed) * Time.deltaTime;



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

        // Now remove roll by zeroing the Z component
        Vector3 currentEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(currentEuler.x, currentEuler.y, 0f);
    }

}

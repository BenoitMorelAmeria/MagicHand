using System.Data;
using UnityEngine;

public class HandOrientationController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] public MagicHandGestures magicHandGestures;

    [Header("Hand pose")]
    [SerializeField] private Vector3 neutralPosition = new Vector3(-0.2f, 0.1f, 0.0f);
    [SerializeField] private Vector3 neutralPointingPosition = new Vector3(-0.2f, 0.0f, 0.0f);
    [SerializeField] private Vector3 handPoseTranslationSpeed = Vector3.zero;
    [SerializeField] private Vector3 handDeadZone = Vector3.zero;
    [SerializeField] private float angleDeadZoneDegrees = 0;

    [SerializeField] private Vector3 interactionAreaMin = new Vector3(-1, -1, -1);
    [SerializeField] private Vector3 interactionAreaMax = new Vector3(1, 1, 1);

    [SerializeField] private Vector3 handNeutralEuler = new Vector3(0f, 180f, 180f);
    [SerializeField] private float rotationSpeed  = 1;
    [SerializeField] private float pointingRotationSpeed  = 100;

    [SerializeField] private float timeBeforeStartInteraction = 0.5f;
    [SerializeField] private float timeBeforeStopInteraction = 0.5f;

    [SerializeField] private GameObject visualFeedbackPrefab;
    [SerializeField] private GameObject visualFeedbackInsidePrefab;
    [SerializeField] private GameObject visualFeedbackNeutralPrefab;

    private GameObject visualFeedback;
    private GameObject visualInsideFeedback;
    private GameObject visualFeedbackNeutral;

    private bool _isInteracting = false;
    private float _interactionTimer = 0f;
    private float yaw;
    private float pitch;

    // Rotation state
    private Quaternion handNeutral = Quaternion.LookRotation(-Vector3.forward, Vector3.down);


    private void Awake()
    {
    }

    private void Start()
    {
        handNeutral = Quaternion.Euler(handNeutralEuler);
        visualFeedback = Instantiate(visualFeedbackPrefab, transform.parent);
        visualInsideFeedback = Instantiate(visualFeedbackInsidePrefab, visualFeedback.transform);
        visualFeedbackNeutral = Instantiate(visualFeedbackNeutralPrefab, transform.parent);

    }

    private void OnValidate()
    {
        handNeutral = Quaternion.Euler(handNeutralEuler);
    }

    void Update()
    {
        bool interactionReady = magicHandGestures.magicHand.IsAvailable() && IsHandInInteractionArea();
        if (interactionReady != _isInteracting)
        {
            _interactionTimer += Time.deltaTime;
            if (_interactionTimer >= timeBeforeStartInteraction && magicHandGestures.magicHand.IsAvailable())
            {
                _isInteracting = true;
                _interactionTimer = 0f;
            }
            else if (_interactionTimer >= timeBeforeStopInteraction && !magicHandGestures.magicHand.IsAvailable())
            {
                _isInteracting = false;
                _interactionTimer = 0f;
            }
        }
        else
        {
            _interactionTimer = 0f;
        }
        UpdateVisualFeedback();
        if (_isInteracting) { 
            HandleHandPos();
           
        }
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

    private Quaternion ComputeRotation()
    {

        Vector3 handForward = magicHandGestures.palmForward;
        Vector3 handRight = magicHandGestures.palmRight;
        Vector3 handUp = Vector3.Cross(handForward, handRight);
        return Quaternion.LookRotation(handForward, handUp);
    }

    public void HandleHandPos()
    {
        if (!magicHandGestures.magicHand.IsAvailable() || !IsHandInInteractionArea())
            return;

        if (magicHandGestures.IndexPointing || magicHandGestures.magicHand.GetPinchState()) {
            HandleHandPosePointing();
        } else
        {
            HandleHandPoseFreeFlying();
        }
    }

    private Vector3 GetRelativePosition()
    {

        Vector3 handPosePos = magicHandGestures.magicHand.Data.GetKeypointScreenSpace(9);
        Vector3 relativePos = handPosePos - neutralPosition;
        relativePos.x = ApplyDeadZone(relativePos.x, handDeadZone.x);
        relativePos.y = ApplyDeadZone(relativePos.y, handDeadZone.y);
        relativePos.z = ApplyDeadZone(relativePos.z, handDeadZone.z);
        return relativePos;
    }

    private void HandleHandPoseFreeFlying()
    {
        // translation using the relative position
        Vector3 handPosePos = magicHandGestures.magicHand.Data.GetKeypointScreenSpace(9);
        Vector3 relativePos = GetRelativePosition();
        Vector3 withRotationComponent = new Vector3(relativePos.x, 0, relativePos.z);
        transform.position += transform.rotation * Vector3.Scale(withRotationComponent, handPoseTranslationSpeed) * Time.deltaTime;
        Vector3 withoutRotationComponent = new Vector3(0, relativePos.y, 0);
        transform.position += Vector3.Scale(withoutRotationComponent, handPoseTranslationSpeed) * Time.deltaTime;


        // Build current hand rotation
        Quaternion handRot = ComputeRotation();

        // Relative rotation: from neutral to current
        // Relative rotation: from neutral to current
        Quaternion relativeRot = handRot * Quaternion.Inverse(handNeutral);

        // Extract Euler angles
        Vector3 euler = relativeRot.eulerAngles;

        // Convert Unity’s [0..360] range into signed angles [-180..180]
        if (euler.x > 180f) euler.x -= 360f;
        if (euler.y > 180f) euler.y -= 360f;

        // Apply deadzone
        euler.x = ApplyDeadZone(euler.x, angleDeadZoneDegrees);
        euler.y = ApplyDeadZone(euler.y, angleDeadZoneDegrees);

        // Scale
        float deltaX = euler.x * rotationSpeed * Time.deltaTime;
        float deltaY = euler.y * rotationSpeed * Time.deltaTime;

        // Accumulate pitch and yaw
        pitch += deltaX;
        yaw += deltaY;

        // Optionally clamp pitch so you don’t flip upside down
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        // Rebuild the rotation every frame (no roll!)
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleHandPosePointing()
    {

        // rotation using the relative position
        Vector3 handPosePos = magicHandGestures.magicHand.Data.GetKeypointScreenSpace(8);
        Vector3 relativePos = handPosePos - neutralPointingPosition;
        Debug.Log("relative pos: " + relativePos);
        
        relativePos.x = ApplyDeadZone(relativePos.x, handDeadZone.x);
        relativePos.y = ApplyDeadZone(relativePos.y, handDeadZone.y);
        relativePos.z = 0;
        float deltaX = relativePos.x * pointingRotationSpeed * Time.deltaTime;
        float deltaY = -relativePos.y * pointingRotationSpeed * Time.deltaTime;
        // Accumulate pitch and yaw
        pitch += deltaY;
        yaw += deltaX;

        // Optionally clamp pitch so you don’t flip upside down
       // pitch = Mathf.Clamp(pitch, -80f, 80f);

        // Rebuild the rotation every frame (no roll!)
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void UpdateVisualFeedback()
    {
        bool active = _isInteracting && !(magicHandGestures.IndexPointing || magicHandGestures.magicHand.GetPinchState());

        visualFeedback.SetActive(active);
        visualFeedback.transform.position = magicHandGestures.magicHand.Data.GetKeypoint(9);
        visualFeedback.transform.rotation = ComputeRotation();

        visualInsideFeedback.SetActive(active);
        visualInsideFeedback.transform.localScale = new Vector3(0.99f, 0.99f, 0.99f);
        visualInsideFeedback.transform.localPosition = new Vector3(0,
            0,
            -GetRelativePosition().z);

        visualFeedbackNeutral.SetActive(active);
        visualFeedbackNeutral.transform.position = neutralPosition;
        visualFeedbackNeutral.transform.rotation = handNeutral;
    }

}

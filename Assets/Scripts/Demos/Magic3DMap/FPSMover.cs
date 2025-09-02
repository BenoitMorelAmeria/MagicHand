using UnityEngine;

public class FreeFlyController : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] private bool moveCamera = false;
    [SerializeField] private bool enableMouseAndKeyboard = true;
    [SerializeField] public float mouseSensitivity = 2f;

    [Header("References")]
    [SerializeField] public MagicHandGestures magicHandGestures;
    [SerializeField] public Transform sceneRoot; // we simulate the camera by moving part of the scene
    [SerializeField] public CameraManager cameraManager;

    [Header("Hand pose")]
    [SerializeField] private float handZCenter = 0.2f; // z position of the hand where the camera is at origin
    [SerializeField] private float handPoseForwardSpeed = 10f; // z position of the hand where the camera is at origin
    [SerializeField] private float handZDeadZone = 0.01f; // z position dead zone around center to avoid jitter
    [SerializeField] private float handRotationSpeed = 60f; // degrees per second


    private float yaw = 0f;
    private float pitch = 0f;
    private bool mouseVisible = false;

    void Start()
    {
        SetMouseVisible(false);
    }

    private void SetMouseVisible(bool visible)
    {

        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;
        mouseVisible = visible;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            moveCamera = !moveCamera;
        if (Input.GetKeyDown(KeyCode.M))
            SetMouseVisible(!mouseVisible);

        if (enableMouseAndKeyboard)
        {
            HandleMouseLook();
            HandleKeyboardMovement();
        }
        handleHandPos();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f); // prevent flipping
    }

    void HandleKeyboardMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D up/down
        float v = Input.GetAxis("Vertical");   // W/S left/right
        // Forward is camera's forward, Right is camera's right
        Vector3 move = (GetCurrentRotation() * new Vector3(h, 0, v)) * moveSpeed * Time.deltaTime;
        transform.position += move;
    }

    Quaternion GetCurrentRotation() => Quaternion.Euler(pitch, yaw, 0f);

    void LateUpdate()
    {
        Quaternion camRot = Quaternion.Euler(pitch, yaw, 0f);

        if (moveCamera)
        {
            // Move the camera normally
            cameraManager.camerasParentTransform.SetPositionAndRotation(
                transform.position,
                camRot
            );
        }
        else
        {
            // Move the world as if the camera moved
            Quaternion inverseRot = Quaternion.Inverse(camRot);
            Vector3 inversePos = -(inverseRot * transform.position);

            sceneRoot.SetPositionAndRotation(
                inversePos,
                inverseRot
            );
        }
    }


    public void handleHandPos()
    {
        if (!magicHandGestures.magicHand.IsAvailable())
            return;

        // --- Forward/Backward movement based on hand Z ---
        Vector3 handPosePos = magicHandGestures.magicHand.GetCenter();
        float zOffset = handPosePos.z - handZCenter;

        if (Mathf.Abs(zOffset) > handZDeadZone)
        {
            Vector3 move = (GetCurrentRotation() * new Vector3(0, 0, zOffset))
                           * handPoseForwardSpeed * Time.deltaTime;
            transform.position += move;
        }

        // --- Orientation-based camera rotation ---
        Vector3 palmNormal = magicHandGestures.palmNormal.normalized;

        // "Neutral" is Vector3.down  compute deviation from down
        // Project palmNormal onto XZ plane (ignoring Y) for yaw
        Vector3 flat = new Vector3(palmNormal.x, 0, palmNormal.z).normalized;
        float targetYawDelta = 0f;
        if (flat.sqrMagnitude > 0.0001f)
        {
            // Compare against forward (Z+) to get signed yaw angle
            targetYawDelta = Vector3.SignedAngle(Vector3.forward, flat, Vector3.up);
        }

        // Pitch: check how much palmNormal tilts away from straight down
        // palmNormal = (0,-1,0)  pitch=0
        // tilt forward/back changes X/Z components
        float verticalDeviation = Vector3.Angle(Vector3.down, palmNormal);
        // Map to signed value using palmNormal.x (left/right tilt) or z (forward/back tilt)
        float targetPitchDelta = 0f;
        if (Mathf.Abs(palmNormal.y) < 0.999f) // avoid gimbal issues
        {
            // Use dot with forward to decide sign
            float sign = Mathf.Sign(Vector3.Dot(palmNormal, Vector3.forward));
            targetPitchDelta = verticalDeviation * sign;
        }

        // Apply deltas smoothly relative to speed
        yaw += targetYawDelta * handRotationSpeed * Time.deltaTime;
        pitch += targetPitchDelta * handRotationSpeed * Time.deltaTime;

        // Clamp pitch to avoid flipping
        pitch = Mathf.Clamp(pitch, -89f, 89f);
    }


}

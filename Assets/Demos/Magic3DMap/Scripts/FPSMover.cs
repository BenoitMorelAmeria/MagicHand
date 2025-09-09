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
    [SerializeField] private float handZCenter = 0.2f;
    [SerializeField] private float handPoseForwardSpeed = 10f;
    [SerializeField] private float handZDeadZone = 0.01f;
    [SerializeField] private float handRotationSpeed = 5f; // blend speed (higher = snappier)

    private bool mouseVisible = false;

    // Rotation state
    private Quaternion currentRotation = Quaternion.identity;
    private Quaternion handNeutral = Quaternion.LookRotation(Vector3.forward, Vector3.down);

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

        HandleHandPos();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Incremental yaw/pitch as quaternions
        Quaternion yawRot = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion pitchRot = Quaternion.AngleAxis(-mouseY, Vector3.right);

        // Apply to current rotation
        currentRotation = yawRot * currentRotation * pitchRot;
    }

    void HandleKeyboardMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S
        Vector3 move = (currentRotation * new Vector3(h, 0, v)) * moveSpeed * Time.deltaTime;
        transform.position += move;
    }

    void LateUpdate()
    {
        if (moveCamera)
        {
            // Move the camera normally
            cameraManager.camerasParentTransform.SetPositionAndRotation(
                transform.position,
                currentRotation
            );
        }
        else
        {
            // Move the world instead
            Quaternion inverseRot = Quaternion.Inverse(currentRotation);
            Vector3 inversePos = -(inverseRot * transform.position);

            sceneRoot.SetPositionAndRotation(inversePos, inverseRot);
        }
    }

    public void HandleHandPos()
    {
        if (!magicHandGestures.magicHand.IsAvailable())
            return;

        // --- Forward/Backward movement ---
        Vector3 handPosePos = magicHandGestures.magicHand.GetCenter();
        float zOffset = handPosePos.z - handZCenter;

        if (Mathf.Abs(zOffset) > handZDeadZone)
        {
            Vector3 move = (currentRotation * new Vector3(0, 0, zOffset))
                           * handPoseForwardSpeed * Time.deltaTime;
            transform.position += move;
        }

        // --- Orientation-based rotation ---
        Vector3 palmNormal = magicHandGestures.palmNormal.normalized; // acts as local "up"
        Vector3 palmRight = -magicHandGestures.palmRight.normalized;   // acts as local "right"

        // Derive palm forward using right × up
        Vector3 palmForward = Vector3.Cross(palmRight, palmNormal).normalized;

        // Construct orientation from basis vectors
        Quaternion handRot = Quaternion.LookRotation(palmForward, palmNormal);

        // Relative to neutral (palm down)
        Quaternion targetRotation = handRot * Quaternion.Inverse(handNeutral);

        // Apply instantly (no smoothing)
        currentRotation = targetRotation;
    }

}

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

    [SerializeField] private float rotSpeedFromPosX = 200f;
    [SerializeField] private float rotSpeedFromPosY = 200f;
    [SerializeField] private float pinchTransSpeed = 10f;

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

    private bool IsPinching()
    {
        float dist = Vector3.Distance(
            magicHandGestures.magicHand.Data.GetKeypointScreenSpace(8),
            magicHandGestures.magicHand.Data.GetKeypointScreenSpace(4)
        );
        return dist < 0.03f;
    }

    public void HandleHandPos()
    {
        if (!magicHandGestures.magicHand.IsAvailable())
            return;

        Vector3 handPosePos = magicHandGestures.magicHand.Data.GetKeypointScreenSpace(9);

        if (IsPinching()) // pinch, we translate the scene
        {
            Debug.Log("pinch");
            Vector3 deltaPos = handPosePos * pinchTransSpeed * Time.deltaTime;
            deltaPos.z = 0;
            transform.position += currentRotation * deltaPos;
        } else {  // no pinch, classic FPS movement
            // Forward - backward movement with hand depth
            float zOffset = handPosePos.z - handZCenter;
            if (Mathf.Abs(zOffset) > handZDeadZone)
            {
                Vector3 move = (currentRotation * new Vector3(0, 0, zOffset))
                               * handPoseForwardSpeed * Time.deltaTime;
                transform.position += move;
            }

            // Rotate left-right with hand x position
            float deltaRotationX = handPosePos.x * rotSpeedFromPosX * Time.deltaTime;
            currentRotation = currentRotation * Quaternion.AngleAxis(deltaRotationX, Vector3.up);

            // Rotate up-down with hand y position
            float deltaRotationY = handPosePos.y * rotSpeedFromPosY * Time.deltaTime;
            currentRotation = currentRotation * Quaternion.AngleAxis(deltaRotationY, Vector3.right);
        }
    }

}

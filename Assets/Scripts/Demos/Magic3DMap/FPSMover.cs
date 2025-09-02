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
        {
            return;
        }
        // move forward or background depending on the hand z position
        Vector3 handPosePos = magicHandGestures.magicHand.GetCenter(); 
        float zOffset = handPosePos.z - handZCenter;
        Debug.Log("hand z=" + handPosePos.z + " zOffset=" + zOffset);
        Vector3 move = (GetCurrentRotation() * new Vector3(0, 0, zOffset)) * handPoseForwardSpeed * Time.deltaTime;
        Debug.Log("Move: " + move);
        transform.position += move;

    }

}

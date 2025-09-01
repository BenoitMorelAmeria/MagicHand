using UnityEngine;

public class FreeFlyController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform sceneRoot; // assign your world root (all objects under here)
    public CameraManager cameraManager;

    private float yaw = 0f;
    private float pitch = 0f;
    private bool moveCamera = true; // toggle with Tab
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

        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f); // prevent flipping
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        // Full rotation (yaw + pitch)
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Forward is camera's forward, Right is camera's right
        Vector3 move = (rotation * new Vector3(h, 0, v)) * moveSpeed * Time.deltaTime;

        if (moveCamera)
            transform.position += move;
        else
            sceneRoot.position -= move;
    }

    void LateUpdate()
    {
        // Apply pitch + yaw to the actual camera
        cameraManager.camerasParentTransform.SetPositionAndRotation(
            transform.position,
            Quaternion.Euler(pitch, yaw, 0f)
        );
    }
}

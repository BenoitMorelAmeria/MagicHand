using UnityEngine;

public class FreeFlyController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform sceneRoot; // assign your world root (all objects under here)

    private float yaw = 0f;
    private float pitch = 0f;
    private bool moveCamera = true; // toggle with Tab

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            moveCamera = !moveCamera;

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
        Camera.main.transform.SetPositionAndRotation(
            transform.position,
            Quaternion.Euler(pitch, yaw, 0f)
        );
    }
}

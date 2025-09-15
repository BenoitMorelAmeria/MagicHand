using UnityEngine;

public class MouseKeyboardController : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] private bool moveCamera = false;
    [SerializeField] private bool enableMouseAndKeyboard = true;
    [SerializeField] public float mouseSensitivity = 2f;


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
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Incremental yaw/pitch as quaternions
        Quaternion yawRot = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion pitchRot = Quaternion.AngleAxis(-mouseY, Vector3.right);

        // Apply to current rotation
        transform.rotation = yawRot * transform.rotation * pitchRot;
    }

    void HandleKeyboardMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S
        Vector3 move = (transform.rotation * new Vector3(h, 0, v)) * moveSpeed * Time.deltaTime;
        transform.position += move;
    }
}

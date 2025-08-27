using UnityEngine;

public class BrushSizeController : MonoBehaviour
{
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] DrawManager drawManager;
    [SerializeField] Vector3 thumbOrientationToStart = Vector3.up;
    [SerializeField] float thumbOrientationThreshold = 0.8f;

    [Header("Brush scaling")]
    [Tooltip("Base speed multiplier for brush size change per second")]
    public float brushSizeChangeSpeed = 0.5f;

    [Tooltip("Minimum allowed brush size")]
    public float minBrushSize = 0.01f;

    [Tooltip("Maximum allowed brush size")]
    public float maxBrushSize = 1.0f;

    private bool _isChangingBrushSize = false;
    private float _startThumbX;

    void Update()
    {
        if (!magicHandGestures.magicHand.IsAvailable() || !magicHandGestures.IsThumbUp)
        {
            _isChangingBrushSize = false;
            return;
        }

        // Thumb vector (tip - base)
        Vector3 thumbOrientation = magicHandGestures.magicHand.GetKeyPoint(3) - magicHandGestures.magicHand.GetKeyPoint(1);
        thumbOrientation.Normalize();
        float dot = Vector3.Dot(thumbOrientation, thumbOrientationToStart);
        if (dot < thumbOrientationThreshold)
        {
            _isChangingBrushSize = false;
            return;
        }

        Vector3 thumbTip = magicHandGestures.magicHand.GetKeyPoint(3);
        Vector3 thumbBase = magicHandGestures.magicHand.GetKeyPoint(1); // base of thumb
        float thumbX = thumbTip.x - thumbBase.x; // horizontal offset

        if (!_isChangingBrushSize)
        {
            _isChangingBrushSize = true;
            _startThumbX = thumbX; // store reference X
        }
        else
        {
            float deltaX = thumbX - _startThumbX;

            // Horizontal movement drives scale
            float scaleChange = Mathf.Exp(deltaX * brushSizeChangeSpeed * Time.deltaTime);
            drawManager.brushSize *= scaleChange;

            drawManager.brushSize = Mathf.Clamp(drawManager.brushSize, minBrushSize, maxBrushSize);
        }
    }

}

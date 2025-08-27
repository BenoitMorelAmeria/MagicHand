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
    private float _startingSize = 0.0f;
    private float _startThumbX;

    public float debugDelta = 0.0f;
    private float debugX = 0.0f;

    void Update()
    {
        if (!magicHandGestures.magicHand.IsAvailable() || !magicHandGestures.IsThumbUp)
        {
            _isChangingBrushSize = false;
            return;
        }

        // Thumb vector (tip - base)
        Vector3 thumbOrientation = magicHandGestures.magicHand.GetKeyPoint(4) - magicHandGestures.magicHand.GetKeyPoint(1);
        thumbOrientation.Normalize();
        float dot = Vector3.Dot(thumbOrientation, thumbOrientationToStart);
        if (dot < thumbOrientationThreshold)
        {
            _isChangingBrushSize = false;
            return;
        }

        Vector3 thumbTip = magicHandGestures.magicHand.GetKeyPoint(4);
        Vector3 thumbBase = magicHandGestures.magicHand.GetKeyPoint(1); // base of thumb
        float thumbX = thumbTip.x; // horizontal offset

        if (!_isChangingBrushSize)
        {
            _isChangingBrushSize = true;
            _startThumbX = thumbX; // store reference X
            _startingSize = drawManager.brushSize;
        }
        else
        {
            float deltaX = thumbX - _startThumbX;
            debugDelta = deltaX;
            debugX = thumbX;

            // Horizontal movement drives scale
            float scaleChange = Mathf.Exp(deltaX * brushSizeChangeSpeed);
            drawManager.brushSize = _startingSize * scaleChange;

            drawManager.brushSize = Mathf.Clamp(drawManager.brushSize, minBrushSize, maxBrushSize);

        }
    }

}

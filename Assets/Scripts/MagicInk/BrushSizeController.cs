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
    private float _startThumbY;

    void Update()
    {
        if (!magicHandGestures.magicHand.IsAvailable() || !magicHandGestures.IsThumbUp)
        {
            _isChangingBrushSize = false;
            return;
        }

        // Check thumb orientation
        Vector3 thumbOrientation = magicHandGestures.magicHand.GetKeyPoint(3) - magicHandGestures.magicHand.GetKeyPoint(1);
        thumbOrientation.Normalize();
        float dot = Vector3.Dot(thumbOrientation, thumbOrientationToStart);
        if (dot < thumbOrientationThreshold)
        {
            _isChangingBrushSize = false;
            return;
        }

        if (!_isChangingBrushSize)
        {
            _isChangingBrushSize = true;
            _startThumbY = magicHandGestures.magicHand.GetKeyPoint(3).y;
        }
        else
        {
            float currentThumbY = magicHandGestures.magicHand.GetKeyPoint(3).y;
            float deltaY = currentThumbY - _startThumbY;

            // DeltaY drives growth/shrink rate
            float scaleChange = Mathf.Exp(deltaY * brushSizeChangeSpeed * Time.deltaTime);
            drawManager.brushSize *= scaleChange;

            // Clamp brush size
            drawManager.brushSize = Mathf.Clamp(drawManager.brushSize, minBrushSize, maxBrushSize);
        }
    }
}

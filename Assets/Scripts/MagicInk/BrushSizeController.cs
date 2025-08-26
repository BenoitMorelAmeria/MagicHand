

using UnityEngine;


public class BrushSizeController : MonoBehaviour
{
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] DrawManager drawManager;
    [SerializeField] Vector3 thumbOrientationToStart = Vector3.up;
    [SerializeField] float thumbOrientationThreshold = 0.8f;

    public float brushSizeChangeSpeed = 0.005f;

    private bool _isChangingBrushSize = false;
    private float _lastThumbY;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!magicHandGestures.magicHand.IsAvailable() || !magicHandGestures.IsThumbUp)
        {
            _isChangingBrushSize = false;
            return;
        }
        
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
        }
        else
        {
            float thumbY = magicHandGestures.magicHand.GetKeyPoint(3).y;
            float scale = Mathf.Pow(brushSizeChangeSpeed, -(thumbY - _lastThumbY));
            drawManager.brushSize *= scale;
        }
        _lastThumbY = magicHandGestures.magicHand.GetKeyPoint(3).y;

    }

}
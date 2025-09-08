using UnityEngine;
using System.Collections.Generic;

public class MagicHand : MonoBehaviour
{
    public MagicHandData Data { get; private set; }
    [SerializeField] public List<Vector2Int> jointPairs = new List<Vector2Int>();

    [SerializeField] private MonoBehaviour rendererComp;
    [SerializeField] private MagicHandPhysics physicsComp;

    IMagicHandRenderer magicHandRenderer;

    private void Awake()
    {
        Data = new MagicHandData();
        magicHandRenderer = rendererComp as IMagicHandRenderer;
        magicHandRenderer.Init(Data.Keypoints, jointPairs);
        physicsComp.Init(Data.Keypoints, jointPairs);
    }

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            magicHandRenderer.ToggleTransparency();
        physicsComp.UpdatePhysics(Data.Keypoints);

    }

    public void UpdateHand(List<Vector3> keypoints)
    {
        List<Vector3> transformedPoints = new List<Vector3>();
        foreach (var p in keypoints)
            transformedPoints.Add(transform.TransformPoint(p));

        Data.UpdateKeypoints(transformedPoints);
        magicHandRenderer.UpdateKeypoints(Data.Keypoints);
    }

    public void SetHandPoseEnabled(bool enabled)
    {
        Data.enabled = enabled;
    }

    public void SetVisible(bool visible) => magicHandRenderer.SetVisible(visible);

    public void SetPinchState(bool state) => Data.SetPinchState(state);
    public bool GetPinchState() => Data.PinchState;

    public bool IsAvailable() => Data.IsAvailable();

    public List<Vector3> GetCurrentKeyPoints() => Data.Keypoints;

    public Vector3 GetKeyPoint(int index) => Data.GetKeypoint(index);

    public Vector3 GetCenter() => Data.GetCenter();

}

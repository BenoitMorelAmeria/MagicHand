using UnityEngine;
using System.Collections.Generic;

public class MagicHand : MonoBehaviour
{
    public MagicHandData Data { get; private set; }
    [SerializeField] public List<Vector2Int> jointPairs = new List<Vector2Int>();

    [SerializeField] private MagicHandRenderer rendererComp;
    [SerializeField] private MagicHandPhysics physicsComp;

    private void Awake()
    {
        Data = new MagicHandData();
        rendererComp.Init(Data.Keypoints);
        physicsComp.Init(Data.Keypoints);
    }

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            rendererComp.ToggleTransparency();

        physicsComp.UpdatePhysics(Data.Keypoints);
    }

    public void UpdateHand(List<Vector3> keypoints)
    {
        Data.UpdateKeypoints(keypoints);
        rendererComp.UpdateKeypoints(Data.Keypoints);
    }

    public void SetVisible(bool visible) => rendererComp.SetVisible(visible);

    public void SetPinchState(bool state) => Data.SetPinchState(state);
    public bool GetPinchState() => Data.PinchState;

    public bool IsAvailable() => Data.IsAvailable();

    public List<Vector3> GetCurrentKeyPoints() => Data.Keypoints;

    public Vector3 GetKeyPoint(int index) => Data.GetKeypoint(index);

    public Vector3 GetCenter() => Data.GetCenter();

}

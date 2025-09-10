using UnityEngine;
using System.Collections.Generic;

public class MagicHand : MonoBehaviour
{
    public MagicHandData Data { get; private set; }
    [SerializeField] public List<Vector2Int> jointPairs = new List<Vector2Int>();

    [SerializeField] private List<MonoBehaviour> rendererComp;
    [SerializeField] private MagicHandPhysics physicsComp;

    int rendererIndex = 0;
    List<IMagicHandRenderer> magicRenderers = new List<IMagicHandRenderer>();
    IMagicHandRenderer magicHandRenderer;

    private void Awake()
    {
        Data = new MagicHandData();
        foreach (var comp in rendererComp)
        {
            comp.enabled = true;
            IMagicHandRenderer renderer = comp as IMagicHandRenderer;
            magicRenderers.Add(renderer);
            renderer.Init(Data.Keypoints, jointPairs);
        }
        SetCurrentRendererIndex(rendererIndex);
        physicsComp.Init(Data.Keypoints, jointPairs);

    }

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            magicHandRenderer.ToggleTransparency();
        if (Input.GetKeyDown(KeyCode.G))
        {
            rendererIndex = (rendererIndex + 1) % rendererComp.Count;
            SetCurrentRendererIndex(rendererIndex);
        }
        physicsComp.UpdatePhysics(Data.Keypoints);

    }

    public void UpdateHand(List<Vector3> keypoints)
    {
        List<Vector3> transformedPoints = new List<Vector3>();
        foreach (var p in keypoints)
            transformedPoints.Add(transform.TransformPoint(p));

        Data.UpdateKeypoints(transformedPoints, keypoints);
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

    private void SetCurrentRendererIndex(int index)
    {
        rendererIndex = index;
        magicHandRenderer = magicRenderers[rendererIndex];
        for (int i = 0; i < rendererComp.Count; i++)
        {
            if (i == rendererIndex)
            {
                magicRenderers[i].SetVisible(true);
            }
            else
            {
                magicRenderers[i].SetVisible(false);
            }
        }
    }
}

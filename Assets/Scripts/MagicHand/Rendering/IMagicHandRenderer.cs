

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public interface IMagicHandRenderer
{
    public void Init(List<Vector3> initialKeypoints, List<Vector2Int> jointPairs);

    public void UpdateKeypoints(List<Vector3> positions);

    public void SetVisible(bool visible);

    public void ToggleTransparency();


}
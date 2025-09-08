using System.Collections.Generic;
using UnityEngine;

public class MagicVoumeRenderer : MonoBehaviour, IMagicHandRenderer
{
    [Header("Rendering")]
    [SerializeField] private Material ghostHandMaterial; // assign GhostHandRaymarch.mat in Inspector
    [SerializeField] private GameObject volumeCubePrefab; // assign a Cube prefab
    [SerializeField] private float capsuleRadius = 0.02f;
    [SerializeField] private float sphereRadius = 0.02f;
    [SerializeField] private float stepSize = 0.01f;

    private GameObject volumeCube;
    private List<Vector3> keypoints;
    private List<Vector2Int> jointPairs;
    private bool isVisible = true;
    private bool isTransparent = true;

    private const float padding = 0.1f; // padding around bounding box

    public void Init(List<Vector3> initialKeypoints, List<Vector2Int> jointPairs)
    {
        this.keypoints = initialKeypoints;
        this.jointPairs = jointPairs;

        if (volumeCube == null)
        {
            volumeCube = Instantiate(volumeCubePrefab, transform);
            volumeCube.name = "GhostHandCube";
        }

        var renderer = volumeCube.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material = ghostHandMaterial;

        UpdateKeypoints(initialKeypoints);
    }

    private void UpdateVolumeCube(List<Vector3> positions)
    {
        if (positions == null || positions.Count == 0 || volumeCube == null) return;

        Vector3 min = positions[0];
        Vector3 max = positions[0];
        foreach (var p in positions)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        Vector3 center = (min + max) * 0.5f;
        Vector3 size = (max - min) + Vector3.one * padding;

        volumeCube.transform.position = center;
        volumeCube.transform.localScale = size;
    }

    public void UpdateKeypoints(List<Vector3> positions)
    {
        if (positions == null || jointPairs == null || ghostHandMaterial == null) return;

        keypoints = positions;

        // compute bounding box
        Vector3 min = positions[0];
        Vector3 max = positions[0];
        foreach (var p in positions)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        Vector3 center = (min + max) * 0.5f;
        Vector3 size = (max - min) + Vector3.one * padding;
        volumeCube.transform.position = center;
        volumeCube.transform.localScale = size;

        Vector3 safeSize = new Vector3(
            Mathf.Max(size.x, 0.0001f),
            Mathf.Max(size.y, 0.0001f),
            Mathf.Max(size.z, 0.0001f)
        );

        // --- update capsules ---
        Vector4[] A = new Vector4[jointPairs.Count];
        Vector4[] B = new Vector4[jointPairs.Count];

        for (int i = 0; i < jointPairs.Count; i++)
        {
            Vector3 pa = positions[jointPairs[i].x];
            Vector3 pb = positions[jointPairs[i].y];

            Vector3 la = new Vector3(
                (pa.x - center.x) / safeSize.x,
                (pa.y - center.y) / safeSize.y,
                (pa.z - center.z) / safeSize.z
            );
            Vector3 lb = new Vector3(
                (pb.x - center.x) / safeSize.x,
                (pb.y - center.y) / safeSize.y,
                (pb.z - center.z) / safeSize.z
            );

            A[i] = la;
            B[i] = lb;
        }

        ghostHandMaterial.SetVectorArray("_CapsuleA", A);
        ghostHandMaterial.SetVectorArray("_CapsuleB", B);
        ghostHandMaterial.SetInt("_CapsuleCount", jointPairs.Count);
        ghostHandMaterial.SetFloat("_CapsuleRadius", capsuleRadius);
        ghostHandMaterial.SetFloat("_StepSize", stepSize);

        // --- update spheres at keypoints ---
        Vector4[] spherePositions = new Vector4[positions.Count];
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 localPos = new Vector3(
                (positions[i].x - center.x) / safeSize.x,
                (positions[i].y - center.y) / safeSize.y,
                (positions[i].z - center.z) / safeSize.z
            );
            spherePositions[i] = localPos;
        }

        ghostHandMaterial.SetVectorArray("_SpherePos", spherePositions);
        ghostHandMaterial.SetInt("_SphereCount", positions.Count);
        ghostHandMaterial.SetFloat("_SphereRadius", sphereRadius);
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;
        if (volumeCube != null) volumeCube.SetActive(visible);
    }

    public void ToggleTransparency()
    {
        isTransparent = !isTransparent;
        if (ghostHandMaterial != null)
        {
            var col = ghostHandMaterial.GetColor("_Color");
            col.a = isTransparent ? 0.4f : 1.0f;
            ghostHandMaterial.SetColor("_Color", col);
        }
    }
}

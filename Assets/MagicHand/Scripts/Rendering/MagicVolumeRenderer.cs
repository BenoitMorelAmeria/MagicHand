using System.Collections.Generic;
using UnityEngine;

public class MagicVoumeRenderer : MonoBehaviour, IMagicHandRenderer
{
    [Header("Rendering")]
    [SerializeField] private Material ghostHandMaterial; // assign GhostHandRaymarch.mat
    [SerializeField] private GameObject volumeCubePrefab; // assign a Cube prefab
    [SerializeField] private float capsuleRadius = 0.02f;
    [SerializeField] private float fillerCapsuleRadius = 0.015f;
    [SerializeField] private float triangleThickness = 0.015f;
    [SerializeField] private float sphereRadius = 0.02f;
    [SerializeField] private float stepSize = 0.01f;
    [SerializeField] int thumbIndexSubdivisions = 4;
    [SerializeField] int otherFingersSubdivisions = 2;

    [SerializeField] private Color ambientColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField, Range(0, 1)] private float ambientIntensity = 0.2f;
    [SerializeField, Range(0, 2)] private float diffuseIntensity = 1.0f;
    [SerializeField, Range(0, 2)] private float specularIntensity = 0.5f;
    [SerializeField, Range(1, 64)] private float specularPower = 16.0f;

    [System.Serializable]
    struct Triangle
    {
        public Vector3 p0, p1, p2;
        public float radius;
    }

    private GameObject volumeCube;
    private List<Vector2Int> jointPairs;
    private bool isTransparent = true;
    private const float padding = 0.1f;

    public void Init(List<Vector3> initialKeypoints, List<Vector2Int> jointPairs)
    {
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

    private Vector3 NormalizeToLocal(Vector3 worldPos, Vector3 center, Vector3 safeSize)
    {
        return new Vector3(
            (worldPos.x - center.x) / safeSize.x,
            (worldPos.y - center.y) / safeSize.y,
            (worldPos.z - center.z) / safeSize.z
        );
    }
    public void UpdateKeypoints(List<Vector3> positions)
    {
        if (positions == null || jointPairs == null || ghostHandMaterial == null) return;

        // --- compute bounding box ---
        Vector3 min = positions[0];
        Vector3 max = positions[0];
        foreach (var p in positions)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        Vector3 center = (min + max) * 0.5f;
        Vector3 size = (max - min) + Vector3.one * padding;

        // scale/position cube in world space
        volumeCube.transform.position = center;
        SetGlobalScale(volumeCube.transform, size);
        //volumeCube.transform.localScale = size;

        // safe size for normalization (avoid div by zero)
        Vector3 safeSize = new Vector3(
            Mathf.Max(size.x, 1e-6f),
            Mathf.Max(size.y, 1e-6f),
            Mathf.Max(size.z, 1e-6f)
        );

        // --- capsules ---
        var allCapsules = new List<(Vector3 a, Vector3 b, float r)>();
        for (int i = 0; i < jointPairs.Count; i++)
        {
            var jp = jointPairs[i];
            if (jp.x < 0 || jp.x >= positions.Count || jp.y < 0 || jp.y >= positions.Count) continue;
            allCapsules.Add((positions[jp.x], positions[jp.y], capsuleRadius));
        }

        // cap number for shader
        const int MAX_CAPSULES = 64;
        if (allCapsules.Count > MAX_CAPSULES)
            allCapsules.RemoveRange(MAX_CAPSULES, allCapsules.Count - MAX_CAPSULES);

        int cCount = allCapsules.Count;
        Vector4[] A = new Vector4[cCount];
        Vector4[] B = new Vector4[cCount];
        float[] radii = new float[cCount];

        float normScale = Mathf.Min(safeSize.x, Mathf.Min(safeSize.y, safeSize.z));

        for (int i = 0; i < cCount; i++)
        {
            var c = allCapsules[i];
            A[i] = NormalizeToLocal(c.a, center, safeSize);
            B[i] = NormalizeToLocal(c.b, center, safeSize);
            radii[i] = c.r / normScale; // normalized radius
        }

        ghostHandMaterial.SetVectorArray("_CapsuleA", A);
        ghostHandMaterial.SetVectorArray("_CapsuleB", B);
        ghostHandMaterial.SetFloatArray("_CapsuleRadii", radii);
        ghostHandMaterial.SetInt("_CapsuleCount", cCount);

        ghostHandMaterial.SetFloat("_StepSize", stepSize / normScale);

        // --- spheres ---
        Vector4[] spherePositions = new Vector4[positions.Count];
        for (int i = 0; i < positions.Count; i++)
            spherePositions[i] = NormalizeToLocal(positions[i], center, safeSize);

        ghostHandMaterial.SetVectorArray("_SpherePos", spherePositions);
        ghostHandMaterial.SetInt("_SphereCount", positions.Count);
        ghostHandMaterial.SetFloat("_SphereRadius", sphereRadius / normScale);

        // --- triangles ---
        List<Triangle> triangles = new List<Triangle>();
        triangles.Add(new Triangle { p0 = positions[1], p1 = positions[2], p2 = positions[5], radius = triangleThickness });
        triangles.Add(new Triangle { p0 = positions[0], p1 = positions[1], p2 = positions[5], radius = triangleThickness });
        triangles.Add(new Triangle { p0 = positions[0], p1 = positions[5], p2 = positions[9], radius = triangleThickness });
        triangles.Add(new Triangle { p0 = positions[0], p1 = positions[9], p2 = positions[13], radius = triangleThickness });
        triangles.Add(new Triangle { p0 = positions[0], p1 = positions[13], p2 = positions[17], radius = triangleThickness });

        int tCount = Mathf.Min(triangles.Count, 16);
        Vector4[] p0 = new Vector4[tCount];
        Vector4[] p1 = new Vector4[tCount];
        Vector4[] p2 = new Vector4[tCount];
        float[] triRadii = new float[tCount];

        for (int i = 0; i < tCount; i++)
        {
            p0[i] = NormalizeToLocal(triangles[i].p0, center, safeSize);
            p1[i] = NormalizeToLocal(triangles[i].p1, center, safeSize);
            p2[i] = NormalizeToLocal(triangles[i].p2, center, safeSize);
            triRadii[i] = triangles[i].radius / normScale;
        }

        ghostHandMaterial.SetVectorArray("_TriP0", p0);
        ghostHandMaterial.SetVectorArray("_TriP1", p1);
        ghostHandMaterial.SetVectorArray("_TriP2", p2);
        ghostHandMaterial.SetFloatArray("_TriRadius", triRadii);
        ghostHandMaterial.SetInt("_TriangleCount", tCount);

        // --- lighting params ---
        ghostHandMaterial.SetColor("_AmbientColor", ambientColor);
        ghostHandMaterial.SetFloat("_AmbientIntensity", ambientIntensity);
        ghostHandMaterial.SetFloat("_DiffuseIntensity", diffuseIntensity);
        ghostHandMaterial.SetFloat("_SpecularIntensity", specularIntensity);
        ghostHandMaterial.SetFloat("_SpecularPower", specularPower);
    }


    public void SetVisible(bool visible)
    {
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
    public static void SetGlobalScale(Transform transform, Vector3 globalScale)
    {
        if (transform.parent == null) transform.localScale = globalScale;
        else
        {
            var parentScale = transform.parent.lossyScale;
            transform.localScale = new Vector3(
                globalScale.x / parentScale.x,
                globalScale.y / parentScale.y,
                globalScale.z / parentScale.z
            );
        }
    }
}

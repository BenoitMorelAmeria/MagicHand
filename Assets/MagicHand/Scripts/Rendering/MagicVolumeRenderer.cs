using System.Collections.Generic;
using UnityEngine;

public class MagicVoumeRenderer : MonoBehaviour, IMagicHandRenderer
{
    [Header("Rendering")]
    [SerializeField] private Material ghostHandMaterial; // assign GhostHandRaymarch.mat in Inspector
    [SerializeField] private GameObject volumeCubePrefab; // assign a Cube prefab
    [SerializeField] private float capsuleRadius = 0.02f;
    [SerializeField] private float fillerCapsuleRadius = 0.015f;   // thinner filler radius
    [SerializeField] private float triangleThickness = 0.015f;   // thinner filler radius

    [SerializeField] private float sphereRadius = 0.02f;
    [SerializeField] private float stepSize = 0.01f;
    [SerializeField] int thumbIndexSubdivisions = 4;   // for wrist -> midpoint between thumb & index
    [SerializeField] int otherFingersSubdivisions = 2; // for wrist -> midpoint between other fingers

    [System.Serializable]
    struct Triangle
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;
        public float radius;
    }

    private GameObject volumeCube;
    private List<Vector2Int> jointPairs;
    private bool isTransparent = true;

    private const float padding = 0.1f; // padding around bounding box

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

    Vector3 NormalizeToLocal(Vector3 worldPos, Vector3 center, Vector3 safeSize)
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
        volumeCube.transform.position = center;
        volumeCube.transform.localScale = size;

        Vector3 safeSize = new Vector3(
            Mathf.Max(size.x, 0.0001f),
            Mathf.Max(size.y, 0.0001f),
            Mathf.Max(size.z, 0.0001f)
        );

        // Build explicit capsules list (Vector3 endpoints + radius)
        var allCapsules = new List<(Vector3 a, Vector3 b, float radius)>();

        // (1) Main bone capsules from jointPairs
        for (int i = 0; i < jointPairs.Count; i++)
        {
            //if (jointPairs[i].x == 0  || jointPairs[i].y == 0)
            //{
            //    continue;
            //}
            var jp = jointPairs[i];
            if (jp.x < 0 || jp.x >= positions.Count || jp.y < 0 || jp.y >= positions.Count) continue;
            allCapsules.Add((positions[jp.x], positions[jp.y], capsuleRadius));
        }

        
        // compute finger base indices: baseIndex = 1 + f*4 (same formula you used)
        int fingerCount = 0;
        if (positions.Count > 1)
            fingerCount = Mathf.Max(0, (positions.Count - 1) / 4);
        /*
        // (2) Wrist -> finger bases
        for (int f = 0; f < fingerCount; f++)
        {
            int baseIdx = f * 4 + 1;
            if (baseIdx >= positions.Count) continue;
            allCapsules.Add((positions[0], positions[baseIdx], capsuleRadius));
        }
        */
        /*
        // (3) Filler capsules between adjacent finger bases (base-to-base)
        for (int f = 0; f + 1 < fingerCount; f++)
        {
            int baseA = f * 4 + 1;
            int baseB = (f + 1) * 4 + 1;
            if (baseA < positions.Count && baseB < positions.Count)
            {
                allCapsules.Add((positions[baseA], positions[baseB], fillerCapsuleRadius));
            }
        }


        // (4) Filler capsules from wrist to subdivision points between adjacent bases
        for (int f = 0; f + 1 < fingerCount; f++)
        {
            int baseA = f * 4 + 1;
            int baseB = (f + 1) * 4 + 1;
            if (baseA >= positions.Count || baseB >= positions.Count) continue;

            // choose subdivision count
            int segments = (f == 0) ? thumbIndexSubdivisions : otherFingersSubdivisions;

            Vector3 pa = positions[baseA];
            Vector3 pb = positions[baseB];

            for (int s = 1; s <= segments; s++)
            {
                float t = s / (float)(segments + 1);
                Vector3 interp = Vector3.Lerp(pa, pb, t);
                allCapsules.Add((positions[0], interp, fillerCapsuleRadius));
            }
        }
        */
        /*
        // --- fill the “thumb triangle” ---
        if (positions.Count > 2) // ensure thumb base + thumb joint + index base exist
        {
            int thumbBase = 1;    // first thumb base
            int thumbJoint = 2;   // second thumb point
            int indexBase = 5;    // first index base (adjust if keypoint layout differs)

            Vector3 pa = positions[thumbBase];
            Vector3 pb = positions[thumbJoint];
            Vector3 pc = positions[indexBase];

            // Capsule: thumb joint -> thumb base
            allCapsules.Add((pb, pa, fillerCapsuleRadius));

            // Capsule: thumb joint -> index base
            allCapsules.Add((pb, pc, fillerCapsuleRadius));

            // Optional: subdivisions along line thumb base -> index base
            int segments = thumbIndexSubdivisions;
            for (int s = 1; s <= segments; s++)
            {
                float t = s / (float)(segments + 1);
                Vector3 interp = Vector3.Lerp(pa, pc, t);
                allCapsules.Add((pb, interp, fillerCapsuleRadius));
            }
        }
        */
        // safety cap: shader max capsules
        const int MAX_CAPSULES = 64;
        if (allCapsules.Count > MAX_CAPSULES)
        {
            Debug.LogWarning($"AllCapsules count ({allCapsules.Count}) exceeds shader MAX_CAPSULES ({MAX_CAPSULES}). Trimming extras.");
            allCapsules.RemoveRange(MAX_CAPSULES, allCapsules.Count - MAX_CAPSULES);
        }

        // --- upload capsule data (normalized to object-local unit cube) ---
        int count = allCapsules.Count;
        Vector4[] A = new Vector4[count];
        Vector4[] B = new Vector4[count];
        float[] radii = new float[count];

        for (int i = 0; i < count; i++)
        {
            var c = allCapsules[i];

            Vector3 pa = c.a;
            Vector3 pb = c.b;

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
            radii[i] = c.radius / Mathf.Max(Mathf.Min(safeSize.x, Mathf.Min(safeSize.y, safeSize.z)), 1e-6f);
            // note: radii need to be normalized to the same local space as positions (if shader expects radius in object-space unit cube).
            // If your shader expects radii in *object-space units* already normalized by bounding box, use the line above.
            // If your shader expects world-space radii (not normalized), change to: radii[i] = c.radius;
        
        
            
        }


        ghostHandMaterial.SetVectorArray("_CapsuleA", A);
        ghostHandMaterial.SetVectorArray("_CapsuleB", B);
        ghostHandMaterial.SetFloatArray("_CapsuleRadii", radii);
        ghostHandMaterial.SetInt("_CapsuleCount", count);
        ghostHandMaterial.SetFloat("_StepSize", stepSize);

        // --- spheres at keypoints (same as before) ---
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
        ghostHandMaterial.SetFloat("_SphereRadius", sphereRadius / Mathf.Max(Mathf.Min(safeSize.x, Mathf.Min(safeSize.y, safeSize.z)), 1e-6f));


        List<Triangle> triangles = new List<Triangle>();
        // Example: thumb triangle
        triangles.Add(new Triangle {p0 = positions[1], p1 = positions[2], p2 = positions[5], radius = triangleThickness});
        triangles.Add(new Triangle {p0 = positions[0], p1 = positions[1], p2 = positions[5], radius = triangleThickness});
        triangles.Add(new Triangle {p0 = positions[0], p1 = positions[5], p2 = positions[9], radius = triangleThickness});
        triangles.Add(new Triangle {p0 = positions[0], p1 = positions[9], p2 = positions[13], radius = triangleThickness});
        triangles.Add(new Triangle {p0 = positions[0], p1 = positions[13], p2 = positions[17], radius = triangleThickness});

        // Upload to shader
        int tCount = Mathf.Min(triangles.Count, 16); // match MAX_TRIANGLES
        Vector4[] p0 = new Vector4[tCount];
        Vector4[] p1 = new Vector4[tCount];
        Vector4[] p2 = new Vector4[tCount];
        float[] triRadii = new float[tCount];

        for (int i = 0; i < tCount; i++)
        {
            p0[i] = NormalizeToLocal(triangles[i].p0, center, safeSize);
            p1[i] = NormalizeToLocal(triangles[i].p1, center, safeSize);
            p2[i] = NormalizeToLocal(triangles[i].p2, center, safeSize);
            triRadii[i] = triangles[i].radius / Mathf.Max(Mathf.Min(safeSize.x, Mathf.Min(safeSize.y, safeSize.z)), 1e-6f);
        }

        ghostHandMaterial.SetVectorArray("_TriP0", p0);
        ghostHandMaterial.SetVectorArray("_TriP1", p1);
        ghostHandMaterial.SetVectorArray("_TriP2", p2);
        ghostHandMaterial.SetFloatArray("_TriRadius", triRadii);
        ghostHandMaterial.SetInt("_TriangleCount", tCount);



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
}

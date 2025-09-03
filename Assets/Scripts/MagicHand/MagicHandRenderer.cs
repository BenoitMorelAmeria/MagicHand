using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class MagicHandRenderer : MonoBehaviour
{
    [Header("Visuals")]
    public float sphereSize = 0.02f;
    public float cylinderRadius = 0.01f;
    public Material sphereMaterial;
    public Material cylinderMaterial;

    [Header("Definition")]
    [SerializeField] public List<Vector2Int> jointPairs = new List<Vector2Int>();

    private List<Renderer> sphereRenderers = new List<Renderer>();
    private List<Renderer> cylinderRenderers = new List<Renderer>();
    private bool transparent = true;

    public void Init(List<Vector3> initialKeypoints)
    {
        InitSpheres(initialKeypoints);
        InitCylinders(initialKeypoints);
    }

    private void InitSpheres(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform, false);
            SetGlobalScale(sphere.transform, Vector3.one * sphereSize);
            Collider col = sphere.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Renderer rend = sphere.GetComponent<Renderer>();
            if (sphereMaterial != null) rend.material = sphereMaterial;
            sphereRenderers.Add(rend);
        }
    }

    private void InitCylinders(List<Vector3> positions)
    {
        foreach (var pair in jointPairs)
        {
            if (pair.x < 0 || pair.x >= positions.Count || pair.y < 0 || pair.y >= positions.Count)
                continue;
            Vector3 p1 = transform.TransformPoint(positions[pair.x]);
            Vector3 p2 = transform.TransformPoint(positions[pair.y]);
            Vector3 mid = (p1 + p2) / 2f;
            Vector3 dir = p2 - p1;
            float length = dir.magnitude;
            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.transform.SetParent(transform, false);
            cyl.transform.position = mid;
            cyl.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            Vector3 globalScale = new Vector3(cylinderRadius, length / 2.0f, cylinderRadius);
            SetGlobalScale(cyl.transform, globalScale);
            Renderer rend = cyl.GetComponent<Renderer>();
            if (cylinderMaterial != null) rend.material = cylinderMaterial;
            cylinderRenderers.Add(rend);

            Collider colCyl = cyl.GetComponent<Collider>();
            if (colCyl != null) colCyl.enabled = false;
        }
    }

    public void UpdateKeypoints(List<Vector3> positions)
    {
        int count = Mathf.Min(positions.Count, sphereRenderers.Count);
        for (int i = 0; i < count; i++)
        {
            if (sphereRenderers[i] != null)
                sphereRenderers[i].transform.position = positions[i];
        }
        count = Mathf.Min(jointPairs.Count, cylinderRenderers.Count);
        for (int i = 0; i < count; i++)
        {
            var pair = jointPairs[i];
            if (pair.x < 0 || pair.x >= positions.Count || pair.y < 0 || pair.y >= positions.Count)
                continue;
            Vector3 p1 = positions[pair.x];
            Vector3 p2 = positions[pair.y];
            Vector3 mid = (p1 + p2) / 2f;
            Vector3 dir = p2 - p1;
            float length = dir.magnitude;
            if (cylinderRenderers[i] != null)
            {
                cylinderRenderers[i].transform.position = mid;
                cylinderRenderers[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                Vector3 globalScale = new Vector3(cylinderRadius, length / 2.0f, cylinderRadius);
                SetGlobalScale(cylinderRenderers[i].transform, globalScale);
            }
        }
    }


    public void SetVisible(bool visible)
    {
        foreach (var rend in sphereRenderers) if (rend != null) rend.enabled = visible;
        foreach (var rend in cylinderRenderers) if (rend != null) rend.enabled = visible;
    }

    public void ToggleTransparency()
    {
        if (transparent)
        {
            SetOpaque(sphereMaterial);
            SetOpaque(cylinderMaterial);
        }
        else
        {
            SetTransparent(sphereMaterial);
            SetTransparent(cylinderMaterial);
        }
        transparent = !transparent;
    }

    public void SetOpaque(Material mat)
    {
        if (!mat) return;
        mat.SetFloat("_Surface", 0);
        mat.renderQueue = (int)RenderQueue.Geometry;
        mat.SetInt("_SrcBlend", (int)BlendMode.One);
        mat.SetInt("_DstBlend", (int)BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
    }

    public void SetTransparent(Material mat)
    {
        if (!mat) return;
        mat.SetFloat("_Surface", 1);
        mat.renderQueue = (int)RenderQueue.Transparent;
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
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

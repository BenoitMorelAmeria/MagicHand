using UnityEngine;

[System.Serializable]
public class CircleConfig
{
    public float radius = 1f;
    public float speed = 30f;
    public Vector3 axis = Vector3.forward;
    public Color color = Color.white;
    public Texture2D texture; 
}

[ExecuteAlways] // works in editor too
public class RotatingTextures : MonoBehaviour
{
    public CircleConfig[] circles;

    private Transform[] circleObjects;

    void Start()
    {
        GenerateCircles();
    }

    void Update()
    {
        if (circleObjects == null) return;

        for (int i = 0; i < circles.Length; i++)
        {
            if (circleObjects[i] == null) continue;

            // Rotate
            circleObjects[i].Rotate(circles[i].axis, circles[i].speed * Time.deltaTime, Space.Self);
        }
    }

    void GenerateCircles()
    {
        // cleanup old children
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        circleObjects = new Transform[circles.Length];

        for (int i = 0; i < circles.Length; i++)
        {
            var cfg = circles[i];

            // Create a quad
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(transform);
            quad.transform.localPosition = Vector3.zero;
            quad.transform.localRotation = Quaternion.identity;
            quad.transform.localScale = Vector3.one * cfg.radius;

            // Assign material
            var renderer = quad.GetComponent<MeshRenderer>();

            Material mat = new Material(Shader.Find("Custom/UnlitTextureTint"));
            mat.color = cfg.color;  // tint (including alpha)
            if (cfg.texture != null)
                mat.mainTexture = cfg.texture;

            if (cfg.texture != null)
                mat.mainTexture = cfg.texture; // apply user’s texture

            renderer.sharedMaterial = mat;

            circleObjects[i] = quad.transform;
        }
    }
}

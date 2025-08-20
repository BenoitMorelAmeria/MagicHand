using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailScaleFix : MonoBehaviour
{
    private TrailRenderer trail;
    private float baseWidth = 1f;

    void Start()
    {
        trail = GetComponent<TrailRenderer>();
        baseWidth = trail.widthMultiplier;
    }

    void Update()
    {
        // Use lossyScale.x or average if non-uniform scaling
        float scaleFactor = transform.lossyScale.x;
        trail.widthMultiplier = baseWidth * scaleFactor;
    }
}
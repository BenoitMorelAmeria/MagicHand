using System.Collections;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [Header("Base Movement Settings")]
    [SerializeField] private float baseRiseSpeed = 1.0f;
    [SerializeField] private float baseWobbleAmplitude = 0.1f;
    [SerializeField] private float baseWobbleFrequency = 2.0f;
    [SerializeField] private float maxHeight = 3.0f;

    [Header("Random Ranges (added to base values)")]
    [SerializeField] private float riseSpeedVariation = 0.3f;
    [SerializeField] private float wobbleAmplitudeVariation = 0.05f;
    [SerializeField] private float wobbleFrequencyVariation = 1.0f;
    [SerializeField] private float driftStrength = 0.05f; // small random drift offset

    [Header("Dissolve Effect")]
    [SerializeField] private float dissolveDuration = 1.0f;

    [SerializeField] private LayerMask handLayer;

    private bool isDissolving = false; // to prevent multiple triggers

    private Material bubbleMaterial;
    private Vector3 startPos;
    private float wobbleOffset;

    // Per-bubble randomized values
    private float riseSpeed;
    private float wobbleAmplitude;
    private float wobbleFrequency;
    private Vector3 driftDir;

    void Start()
    {
        startPos = transform.position;
        wobbleOffset = Random.Range(0f, Mathf.PI * 2f); // random phase

        // Randomized properties per bubble
        riseSpeed = baseRiseSpeed + Random.Range(-riseSpeedVariation, riseSpeedVariation);
        wobbleAmplitude = baseWobbleAmplitude + Random.Range(-wobbleAmplitudeVariation, wobbleAmplitudeVariation);
        wobbleFrequency = baseWobbleFrequency + Random.Range(-wobbleFrequencyVariation, wobbleFrequencyVariation);

        // Random drift direction in XZ plane
        driftDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * driftStrength;

        // Use instanced material for unique dissolve animation
        bubbleMaterial = GetComponent<Renderer>().material;
        bubbleMaterial.SetFloat("_DissolveAmount", 0f);
    }

    void Update()
    {
        // Bubble rises
        float y = transform.position.y + riseSpeed * Time.deltaTime;

        // Wobble sideways
        float wobbleX = Mathf.Sin(Time.time * wobbleFrequency + wobbleOffset) * wobbleAmplitude;
        float wobbleZ = Mathf.Cos(Time.time * wobbleFrequency + wobbleOffset) * wobbleAmplitude;

        // Add constant drift
        Vector3 drift = driftDir * Time.time;

        transform.position = new Vector3(startPos.x + wobbleX + drift.x, y, startPos.z + wobbleZ + drift.z);

        // Destroy when too high
        if (y >= maxHeight)
        {
            StartCoroutine(DissolveAndDestroy());
        }
    }

    private IEnumerator DissolveAndDestroy()
    {
        // Prevent multiple calls
        enabled = false;

        float elapsed = 0f;
        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveDuration);
            bubbleMaterial.SetFloat("_DissolveAmount", t);
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDissolving) return;

        if (((1 << other.gameObject.layer) & handLayer) != 0)
        {
            StartCoroutine(DissolveAndDestroy());
        }
    }
}

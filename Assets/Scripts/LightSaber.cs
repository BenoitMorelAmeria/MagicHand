using System.Collections;
using UnityEngine;

public class LightSaber : MonoBehaviour
{
    [SerializeField] Transform lightScaleTransform;
    [SerializeField] float openDuration = 0.1f; // Duration of the opening animation
    [SerializeField] float maxLength = 10.0f; // Maximum length of the saber
    [SerializeField] LayerMask handLayer; // Layer for hand detection
    [SerializeField] AudioSource openSound; // Sound to play when opening the saber
    [SerializeField] float timeBeforeClosing = 2.0f;


    [SerializeField] Material laserMaterialReference;
    [SerializeField] MeshRenderer laserMeshRenderer;
    [SerializeField] Color baseColor = Color.red;
    [SerializeField] float minEmission = 1.0f;
    [SerializeField] float maxEmission = 10.0f;
    [SerializeField] float flickerSpeed = 0.1f;
    private float targetEmission = 1.0f;
    private float currentEmission = 1.0f;
    
    
    enum State
    {
        Opened,
        Closed,
        Opening,
        Closing
    }

    State state = State.Closed;
    float timeSinceLastTrigger = 0;

    // Start is called before the first frame update
    void Start()
    {
        Material copy = Instantiate(laserMaterialReference);
        laserMeshRenderer.material = copy;
        copy.color = baseColor;
        SetEmission(5.0f);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (state == State.Closed)
            {
                StartCoroutine(OpenSaber());
            }
            else if (state == State.Opened)
            {
                StartCoroutine(CloseSaber());
            }
        }
        if (state == State.Opened && Time.time -  timeSinceLastTrigger > timeBeforeClosing)
        {
            StartCoroutine(CloseSaber());
        }
        UpdateEmission();
    }

    private void UpdateEmission()
    {
        if (Random.value < flickerSpeed)
        {
            targetEmission = Random.Range(minEmission, maxEmission);
        }
        currentEmission = Mathf.Lerp(currentEmission, targetEmission, Time.deltaTime * 10.0f);
        SetEmission(currentEmission);
    }


    void OnTriggerEnter(Collider other)
    {
        timeSinceLastTrigger = Time.time;
        if (state == State.Closed)
        {
            if (((1 << other.gameObject.layer) & handLayer.value) != 0)
            {

                StartCoroutine(OpenSaber());
            }
        }
    }


    private IEnumerator OpenSaber()
    {
        timeSinceLastTrigger = Time.time;
        if (openSound != null)
        {
            openSound.Play(); // Play the opening sound
        }
        float duration = openDuration; // Duration of the animation
        float elapsedTime = 0f;
        float initialLength = lightScaleTransform.localScale.y;
        float targetLength = maxLength; // Target length of the saber
        state = State.Opening;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float newLength = Mathf.Lerp(initialLength, targetLength, t);
            SetSaberLength(newLength);
            yield return null;
        }
        SetSaberLength(targetLength); // Ensure it ends at the target length
        state = State.Opened;
    }

    private IEnumerator CloseSaber()
    {
        if (openSound != null)
        {
            openSound.Play(); // Play the closing sound
        }
        float duration = openDuration; // Duration of the animation
        float elapsedTime = 0f;
        float initialLength = lightScaleTransform.localScale.y;
        float targetLength = 0.0f; // Target length of the saber
        state = State.Closing;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float newLength = Mathf.Lerp(initialLength, targetLength, t);
            SetSaberLength(newLength);
            yield return null;
        }
        SetSaberLength(targetLength); // Ensure it ends at the target length
        state = State.Closed;
    }

    private void SetSaberLength(float length)
    {
        lightScaleTransform.localScale = new Vector3(1.0f, length, 1.0f);
    }

    private void SetEmission(float emission)
    {
        Material mat = laserMeshRenderer.material;
        mat.SetColor("_EmissionColor", mat.color * emission);
    }


}

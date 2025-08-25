using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LightSaber : MonoBehaviour
{
    enum State
    {
        Opened,
        Closed,
        Opening,
        Closing
    }

    [SerializeField] MagicHand magicHand; // Reference to the MagicHand script to get hand position

    [SerializeField] Transform lightScaleTransform;
    [SerializeField] float openDuration = 0.1f; // Duration of the opening animation
    [SerializeField] float maxLength = 10.0f; // Maximum length of the saber
    [SerializeField] LayerMask handLayer; // Layer for hand detection
    [SerializeField] AudioClip openSoundClip; // Sound to play when opening the saber
    [SerializeField] float timeBeforeClosing = 2.0f;


    [SerializeField] Material laserMaterialReference;
    [SerializeField] MeshRenderer laserMeshRenderer;
    [SerializeField] Color baseColor = Color.red;
    [SerializeField] float minEmission = 1.0f;
    [SerializeField] float maxEmission = 10.0f;
    [SerializeField] float flickerSpeed = 0.1f;

    [SerializeField] float attractionMaxRadius = 0.2f;
    [SerializeField] float attractionMinRadius = 0.05f;
    [SerializeField] float attractionForce = 5.0f;

    private float targetEmission = 1.0f;
    private float currentEmission = 1.0f;
    State state = State.Closed;
    float timeSinceLastTrigger = 0;
    private AudioSource openSoundSource; // Sound to play when opening the saber

    // Start is called before the first frame update
    void Start()
    {
        Material copy = Instantiate(laserMaterialReference);
        laserMeshRenderer.material = copy;
        openSoundSource = gameObject.AddComponent<AudioSource>();
        openSoundSource.clip = openSoundClip;
        openSoundSource.playOnAwake = false;
        openSoundSource.loop = false;

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
        if (state == State.Closed)
        {
            UpdateAttraction();
        }
    }

    public void UpdateAttraction()
    {
        if (!magicHand.IsAvailable())
        {
            return;
        }
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 attractionPos = (magicHand.GetKeyPoint(2) + magicHand.GetKeyPoint(5)) * 0.5f; // Average position of index and middle finger
        float distance = Vector3.Distance(rb.position, attractionPos);

        if (distance < attractionMaxRadius && distance > attractionMinRadius)
        {
            Vector3 direction = (attractionPos - rb.position).normalized;
            // apply force toward hand
            rb.AddForce(direction * attractionForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
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
        if (openSoundSource != null)
        {
            openSoundSource.Play(); // Play the opening sound
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
        if (openSoundSource != null)
        {
            openSoundSource.Play(); // Play the closing sound
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
        mat.SetColor("_EmissionColor", baseColor * emission);
    }


}

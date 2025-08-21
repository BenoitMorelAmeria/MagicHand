using System.Collections;
using UnityEngine;

public class LightSaber : MonoBehaviour
{
    [SerializeField] Transform lightScaleTransform;
    [SerializeField] float openDuration = 0.1f; // Duration of the opening animation
    [SerializeField] float maxLength = 10.0f; // Maximum length of the saber
    [SerializeField] LayerMask handLayer; // Layer for hand detection
    enum State
    {
        Opened,
        Closed,
        Opening,
        Closing
    }

    State state = State.Closed;


    // Start is called before the first frame update
    void Start()
    {
        
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
    }


    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collide");
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




}
